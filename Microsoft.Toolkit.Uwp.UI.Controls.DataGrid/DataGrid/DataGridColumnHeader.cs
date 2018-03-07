// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Diagnostics;
using Microsoft.Toolkit.Uwp.Automation.Peers;
using Microsoft.Toolkit.Uwp.UI.Controls.DataGridInternals;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// TODO - Handle IsEnabledChanged to reset the drag variables.
namespace Microsoft.Toolkit.Uwp.UI.Controls.Primitives
{
    /// <summary>
    /// Represents an individual <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.DataGrid"/> column header.
    /// </summary>
    [TemplateVisualState(Name = VisualStates.StateNormal, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StatePointerOver, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StatePressed, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StateUnsorted, GroupName = VisualStates.GroupSort)]
    [TemplateVisualState(Name = VisualStates.StateSortAscending, GroupName = VisualStates.GroupSort)]
    [TemplateVisualState(Name = VisualStates.StateSortDescending, GroupName = VisualStates.GroupSort)]
    public partial class DataGridColumnHeader : ContentControl
    {
        private enum DragMode
        {
            None = 0,
            PointerPressed = 1,
            Drag = 2,
            Resize = 3,
            Reorder = 4
        }

        private const int DATAGRIDCOLUMNHEADER_dragThreshold = 2;
        private const int DATAGRIDCOLUMNHEADER_resizeRegionWidthStrict = 5;
        private const int DATAGRIDCOLUMNHEADER_resizeRegionWidthLoose = 9;
        private const double DATAGRIDCOLUMNHEADER_separatorThickness = 1;

        private static CoreCursor _originalCursor;
        private static DataGridColumn _dragColumn;
        private static DragMode _dragMode;
        private static Point? _dragStart;
        private static Point? _pressedPointerPositionHeaders;
        private static Point? _lastPointerPositionHeaders;
        private static double _originalHorizontalOffset;
        private static double _originalWidth;
        private static double _frozenColumnsWidth;
        private static uint _resizePointerId;
        private static uint _dragPointerId;

        private Pointer _capturedPointer;
        private Visibility _desiredSeparatorVisibility;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridColumnHeader"/> class.
        /// </summary>
        public DataGridColumnHeader()
        {
            this.PointerCanceled += new PointerEventHandler(DataGridColumnHeader_PointerCanceled);
            this.PointerCaptureLost += new PointerEventHandler(DataGridColumnHeader_PointerCaptureLost);
            this.PointerPressed += new PointerEventHandler(DataGridColumnHeader_PointerPressed);
            this.PointerReleased += new PointerEventHandler(DataGridColumnHeader_PointerReleased);
            this.PointerMoved += new PointerEventHandler(DataGridColumnHeader_PointerMoved);
            this.PointerEntered += new PointerEventHandler(DataGridColumnHeader_PointerEntered);
            this.PointerExited += new PointerEventHandler(DataGridColumnHeader_PointerExited);

            DefaultStyleKey = typeof(DataGridColumnHeader);
        }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.Media.Brush"/> used to paint the column header separator lines.
        /// </summary>
        public Brush SeparatorBrush
        {
            get { return GetValue(SeparatorBrushProperty) as Brush; }
            set { SetValue(SeparatorBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridColumnHeader.SeparatorBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SeparatorBrushProperty =
            DependencyProperty.Register(
                "SeparatorBrush",
                typeof(Brush),
                typeof(DataGridColumnHeader),
                null);

        /// <summary>
        /// Gets or sets a value indicating whether the column header separator lines are visible.
        /// </summary>
        public Visibility SeparatorVisibility
        {
            get { return (Visibility)GetValue(SeparatorVisibilityProperty); }
            set { SetValue(SeparatorVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridColumnHeader.SeparatorVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SeparatorVisibilityProperty =
            DependencyProperty.Register(
                "SeparatorVisibility",
                typeof(Visibility),
                typeof(DataGridColumnHeader),
                new PropertyMetadata(Visibility.Visible, OnSeparatorVisibilityPropertyChanged));

        private static void OnSeparatorVisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGridColumnHeader columnHeader = (DataGridColumnHeader)d;

            if (!columnHeader.IsHandlerSuspended(e.Property))
            {
                columnHeader._desiredSeparatorVisibility = (Visibility)e.NewValue;
                if (columnHeader.OwningGrid != null)
                {
                    columnHeader.UpdateSeparatorVisibility(columnHeader.OwningGrid.ColumnsInternal.LastVisibleColumn);
                }
                else
                {
                    columnHeader.UpdateSeparatorVisibility(null);
                }
            }
        }

        internal static bool HasUserInteraction
        {
            get
            {
                return _dragMode != DragMode.None;
            }
        }

        internal int ColumnIndex
        {
            get
            {
                if (this.OwningColumn == null)
                {
                    return -1;
                }

                return this.OwningColumn.Index;
            }
        }

#if FEATURE_ICOLLECTIONVIEW_SORT
        internal ListSortDirection? CurrentSortingState
        {
            get;
            private set;
        }
#endif

        internal DataGrid OwningGrid
        {
            get
            {
                if (this.OwningColumn != null && this.OwningColumn.OwningGrid != null)
                {
                    return this.OwningColumn.OwningGrid;
                }

                return null;
            }
        }

        internal DataGridColumn OwningColumn
        {
            get;
            set;
        }

        private bool IsPointerOver
        {
            get;
            set;
        }

        private bool IsPressed
        {
            get;
            set;
        }

        /// <summary>
        /// Builds the visual tree for the column header when a new template is applied.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ApplyState(false /*useTransitions*/);
        }

        /// <summary>
        /// Called when the value of the <see cref="P:System.Windows.Controls.ContentControl.Content"/> property changes.
        /// </summary>
        /// <param name="oldContent">The old value of the <see cref="P:System.Windows.Controls.ContentControl.Content"/> property.</param>
        /// <param name="newContent">The new value of the <see cref="P:System.Windows.Controls.ContentControl.Content"/> property.</param>
        /// <exception cref="T:System.NotSupportedException">
        /// <paramref name="newContent"/> is not a UIElement.
        /// </exception>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (newContent is UIElement)
            {
                throw DataGridError.DataGridColumnHeader.ContentDoesNotSupportUIElements();
            }

            base.OnContentChanged(oldContent, newContent);
        }

        /// <summary>
        /// Creates AutomationPeer (<see cref="UIElement.OnCreateAutomationPeer"/>)
        /// </summary>
        /// <returns>An automation peer for this <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridColumnHeader"/>.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if (this.OwningGrid != null && this.OwningColumn != this.OwningGrid.ColumnsInternal.FillerColumn)
            {
                return new DataGridColumnHeaderAutomationPeer(this);
            }

            return base.OnCreateAutomationPeer();
        }

        internal void ApplyState(bool useTransitions)
        {
            // Common States
            if (this.IsPressed && _dragMode != DragMode.Resize)
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StatePressed, VisualStates.StatePointerOver, VisualStates.StateNormal);
            }
            else if (this.IsPointerOver && _dragMode != DragMode.Resize)
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StatePointerOver, VisualStates.StateNormal);
            }
            else
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateNormal);
            }

            // Sort States
            if (this.OwningColumn != null)
            {
                switch (this.OwningColumn.SortDirection)
                {
                    case null:
                        VisualStates.GoToState(this, useTransitions, VisualStates.StateUnsorted);
                        break;
                    case DataGridSortDirection.Ascending:
                        VisualStates.GoToState(this, useTransitions, VisualStates.StateSortAscending, VisualStates.StateUnsorted);
                        break;
                    case DataGridSortDirection.Descending:
                        VisualStates.GoToState(this, useTransitions, VisualStates.StateSortDescending, VisualStates.StateUnsorted);
                        break;
                }
            }
        }

        /// <summary>
        /// Ensures that the correct Style is applied to this object.
        /// </summary>
        /// <param name="previousStyle">Caller's previous associated Style</param>
        internal void EnsureStyle(Style previousStyle)
        {
            if (this.Style != null &&
                this.Style != previousStyle &&
                (this.OwningColumn == null || this.Style != this.OwningColumn.HeaderStyle) &&
                (this.OwningGrid == null || this.Style != this.OwningGrid.ColumnHeaderStyle))
            {
                return;
            }

            Style style = null;
            if (this.OwningColumn != null)
            {
                style = this.OwningColumn.HeaderStyle;
            }

            if (style == null && this.OwningGrid != null)
            {
                style = this.OwningGrid.ColumnHeaderStyle;
            }

            this.SetStyleWithType(style);
        }

#if FEATURE_ICOLLECTIONVIEW_SORT
        internal void InvokeProcessSort()
        {
            Debug.Assert(this.OwningGrid != null, "Expected non-null owning DataGrid.");

            if (this.OwningGrid.WaitForLostFocus(delegate { this.InvokeProcessSort(); }))
            {
                return;
            }

            if (this.OwningGrid.CommitEdit(DataGridEditingUnit.Row, true /*exitEditingMode*/))
            {
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { ProcessSort(); }).AsTask();
            }
        }
#endif

#if FEATURE_ICOLLECTIONVIEW_SORT
        internal void ProcessSort()
        {
            // if we can sort:
            //  - DataConnection.AllowSort is true, and
            //  - AllowUserToSortColumns and CanSort are true, and
            //  - OwningColumn is bound, and
            //  - SortDescriptionsCollection exists, and
            //  - the column's data type is comparable
            // then try to sort
            if (this.OwningColumn != null &&
                this.OwningGrid != null &&
                this.OwningGrid.EditingRow == null &&
                this.OwningColumn != this.OwningGrid.ColumnsInternal.FillerColumn &&
                this.OwningGrid.DataConnection.AllowSort &&
                this.OwningGrid.CanUserSortColumns &&
                this.OwningColumn.CanUserSort)
            {
                if (this.OwningGrid.DataConnection.SortDescriptions != null)
                {
                    DataGrid owningGrid = this.OwningGrid;
                    ListSortDirection newSortDirection;
                    SortDescription newSort;

                    bool ctrl;
                    bool shift;

                    KeyboardHelper.GetMetaKeyState(out ctrl, out shift);

                    SortDescription? sort = this.OwningColumn.GetSortDescription();
                    ICollectionView collectionView = owningGrid.DataConnection.CollectionView;
                    Debug.Assert(collectionView != null);
                    try
                    {
                        owningGrid.OnUserSorting();
                        using (collectionView.DeferRefresh())
                        {
                            // if shift is held down, we multi-sort, therefore if it isn't, we'll clear the sorts beforehand
                            if (!shift || owningGrid.DataConnection.SortDescriptions.Count == 0)
                            {
                                if (collectionView.CanGroup && collectionView.GroupDescriptions != null)
                                {
                                    // Make sure we sort by the GroupDescriptions first
                                    for (int i = 0; i < collectionView.GroupDescriptions.Count; i++)
                                    {
                                        PropertyGroupDescription groupDescription = collectionView.GroupDescriptions[i] as PropertyGroupDescription;
                                        if (groupDescription != null && collectionView.SortDescriptions.Count <= i || collectionView.SortDescriptions[i].PropertyName != groupDescription.PropertyName)
                                        {
                                            collectionView.SortDescriptions.Insert(Math.Min(i, collectionView.SortDescriptions.Count), new SortDescription(groupDescription.PropertyName, ListSortDirection.Ascending));
                                        }
                                    }
                                    while (collectionView.SortDescriptions.Count > collectionView.GroupDescriptions.Count)
                                    {
                                        collectionView.SortDescriptions.RemoveAt(collectionView.GroupDescriptions.Count);
                                    }
                                }
                                else if (!shift)
                                {
                                    owningGrid.DataConnection.SortDescriptions.Clear();
                                }
                            }

                            if (sort.HasValue)
                            {
                                // swap direction
                                switch (sort.Value.Direction)
                                {
                                    case ListSortDirection.Ascending:
                                        newSortDirection = ListSortDirection.Descending;
                                        break;
                                    default:
                                        newSortDirection = ListSortDirection.Ascending;
                                        break;
                                }

                                newSort = new SortDescription(sort.Value.PropertyName, newSortDirection);

                                // changing direction should not affect sort order, so we replace this column's
                                // sort description instead of just adding it to the end of the collection
                                int oldIndex = owningGrid.DataConnection.SortDescriptions.IndexOf(sort.Value);
                                if (oldIndex >= 0)
                                {
                                    owningGrid.DataConnection.SortDescriptions.Remove(sort.Value);
                                    owningGrid.DataConnection.SortDescriptions.Insert(oldIndex, newSort);
                                }
                                else
                                {
                                    owningGrid.DataConnection.SortDescriptions.Add(newSort);
                                }
                            }
                            else
                            {
                                // start new sort
                                newSortDirection = ListSortDirection.Ascending;

                                string propertyName = this.OwningColumn.GetSortPropertyName();

                                // no-opt if we couldn't find a property to sort on
                                if (string.IsNullOrEmpty(propertyName))
                                {
                                    return;
                                }

                                newSort = new SortDescription(propertyName, newSortDirection);

                                owningGrid.DataConnection.SortDescriptions.Add(newSort);
                            }
                        }
                    }
                    finally
                    {
                        owningGrid.OnUserSorted();
                    }

                    // We've completed the sort, so send the Invoked event for the column header's automation peer
                    if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
                    {
                        AutomationPeer peer = FrameworkElementAutomationPeer.FromElement(this);
                        if (peer != null)
                        {
                            peer.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
                        }
                    }
                }
            }
        }
#endif

        internal void UpdateSeparatorVisibility(DataGridColumn lastVisibleColumn)
        {
            Visibility newVisibility = _desiredSeparatorVisibility;

            // Collapse separator for the last column if there is no filler column
            if (this.OwningColumn != null &&
                this.OwningGrid != null &&
                _desiredSeparatorVisibility == Visibility.Visible &&
                this.OwningColumn == lastVisibleColumn &&
                !this.OwningGrid.ColumnsInternal.FillerColumn.IsActive)
            {
                newVisibility = Visibility.Collapsed;
            }

            // Update the public property if it has changed
            if (this.SeparatorVisibility != newVisibility)
            {
                this.SetValueNoCallback(DataGridColumnHeader.SeparatorVisibilityProperty, newVisibility);
            }
        }

        /// <summary>
        /// Determines whether a column can be resized by dragging the border of its header.  If star sizing
        /// is being used, there are special conditions that can prevent a column from being resized:
        /// 1. The column is the last visible column.
        /// 2. All columns are constrained by either their maximum or minimum values.
        /// </summary>
        /// <param name="column">Column to check.</param>
        /// <returns>Whether or not the column can be resized by dragging its header.</returns>
        private static bool CanResizeColumn(DataGridColumn column)
        {
            if (column.OwningGrid != null && column.OwningGrid.ColumnsInternal != null && column.OwningGrid.UsesStarSizing &&
                (column.OwningGrid.ColumnsInternal.LastVisibleColumn == column || !DoubleUtil.AreClose(column.OwningGrid.ColumnsInternal.VisibleEdgedColumnsWidth, column.OwningGrid.CellsWidth)))
            {
                return false;
            }

            return column.ActualCanUserResize;
        }

        private static bool TrySetResizeColumn(uint pointerId, DataGridColumn column)
        {
            // If Datagrid.CanUserResizeColumns == false, then the column can still override it
            if (CanResizeColumn(column))
            {
                Debug.Assert(_dragMode != DragMode.None, "Expected _dragMode other than None.");

                _dragColumn = column;
                _dragMode = DragMode.Resize;
                _dragPointerId = pointerId;

                return true;
            }

            return false;
        }

        private bool CanReorderColumn(DataGridColumn column)
        {
            return this.OwningGrid.CanUserReorderColumns &&
                !(column is DataGridFillerColumn) &&
                ((column.CanUserReorderInternal.HasValue && column.CanUserReorderInternal.Value) || !column.CanUserReorderInternal.HasValue);
        }

        private void DataGridColumnHeader_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            // Debug.WriteLine("DataGridColumnHeader.DataGridColumnHeader_PointerCanceled");
            CancelPointer(e);
        }

        private void DataGridColumnHeader_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            // Debug.WriteLine("DataGridColumnHeader.DataGridColumnHeader_PointerCaptureLost");
            CancelPointer(e);
        }

        private void CancelPointer(PointerRoutedEventArgs e)
        {
            // When we stop interacting with the column headers, we need to reset the drag mode and close any popups if they are open.
            bool setResizeCursor = false;

            if (this.OwningGrid != null && this.OwningGrid.ColumnHeaders != null)
            {
                Point pointerPositionHeaders = e.GetCurrentPoint(this.OwningGrid.ColumnHeaders).Position;
                setResizeCursor = _dragMode == DragMode.Resize && pointerPositionHeaders.X > 0 && pointerPositionHeaders.X < this.OwningGrid.ActualWidth;
            }

            if (!setResizeCursor)
            {
                SetOriginalCursor();
            }

            if (_dragPointerId == e.Pointer.PointerId)
            {
                _capturedPointer = null;
                _dragMode = DragMode.None;
                _dragPointerId = 0;
                _dragColumn = null;
                _dragStart = null;
                _pressedPointerPositionHeaders = null;
                _lastPointerPositionHeaders = null;

                if (this.OwningGrid != null && this.OwningGrid.ColumnHeaders != null)
                {
                    this.OwningGrid.ColumnHeaders.DragColumn = null;
                    this.OwningGrid.ColumnHeaders.DragIndicator = null;
                    this.OwningGrid.ColumnHeaders.DropLocationIndicator = null;
                }
            }

            if (setResizeCursor)
            {
                SetResizeCursor(e.Pointer, e.GetCurrentPoint(this).Position);
            }
        }

        private void DataGridColumnHeader_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            // Debug.WriteLine("DataGridColumnHeader.DataGridColumnHeader_PointerEntered");
            if (!this.IsEnabled)
            {
                return;
            }

            SetResizeCursor(e.Pointer, e.GetCurrentPoint(this).Position);

            ApplyState(true /*useTransitions*/);
        }

        private void DataGridColumnHeader_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            // Debug.WriteLine("DataGridColumnHeader.DataGridColumnHeader_PointerExited");
            if (!this.IsEnabled)
            {
                return;
            }

            if (_dragMode == DragMode.None && _resizePointerId == e.Pointer.PointerId)
            {
                SetOriginalCursor();
            }

            ApplyState(true /*useTransitions*/);
        }

        private void DataGridColumnHeader_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Debug.WriteLine("DataGridColumnHeader.DataGridColumnHeader_PointerPressed");
            if (this.OwningGrid == null || this.OwningColumn == null || e.Handled || !this.IsEnabled || _dragMode != DragMode.None)
            {
                return;
            }

            PointerPoint pointerPoint = e.GetCurrentPoint(this);

            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse && !pointerPoint.Properties.IsLeftButtonPressed)
            {
                return;
            }

            Debug.Assert(_dragPointerId == 0, "Expected _dragPointerId is 0.");

            bool handled = e.Handled;

            this.IsPressed = true;

            if (this.OwningGrid.ColumnHeaders != null)
            {
                Point pointerPosition = pointerPoint.Position;

                if (this.CapturePointer(e.Pointer))
                {
                    _capturedPointer = e.Pointer;
                }
                else
                {
                    _capturedPointer = null;
                }

                Debug.Assert(_dragMode == DragMode.None, "Expected _dragMode equals None.");
                Debug.Assert(_dragColumn == null, "Expected _dragColumn is null.");
                _dragMode = DragMode.PointerPressed;
                _dragPointerId = e.Pointer.PointerId;
                _frozenColumnsWidth = this.OwningGrid.ColumnsInternal.GetVisibleFrozenEdgedColumnsWidth();
                _pressedPointerPositionHeaders = _lastPointerPositionHeaders = this.Translate(this.OwningGrid.ColumnHeaders, pointerPosition);

                double distanceFromLeft = pointerPosition.X;
                double distanceFromRight = this.ActualWidth - distanceFromLeft;
                DataGridColumn currentColumn = this.OwningColumn;
                DataGridColumn previousColumn = null;
                if (!(this.OwningColumn is DataGridFillerColumn))
                {
                    previousColumn = this.OwningGrid.ColumnsInternal.GetPreviousVisibleNonFillerColumn(currentColumn);
                }

                int resizeRegionWidth = e.Pointer.PointerDeviceType == PointerDeviceType.Touch ? DATAGRIDCOLUMNHEADER_resizeRegionWidthLoose : DATAGRIDCOLUMNHEADER_resizeRegionWidthStrict;

                if (distanceFromRight <= resizeRegionWidth)
                {
                    handled = TrySetResizeColumn(e.Pointer.PointerId, currentColumn);
                }
                else if (distanceFromLeft <= resizeRegionWidth && previousColumn != null)
                {
                    handled = TrySetResizeColumn(e.Pointer.PointerId, previousColumn);
                }

                if (_dragMode == DragMode.Resize && _dragColumn != null)
                {
                    _dragStart = _lastPointerPositionHeaders;
                    _originalWidth = _dragColumn.ActualWidth;
                    _originalHorizontalOffset = this.OwningGrid.HorizontalOffset;

                    handled = true;
                }
            }

            e.Handled = handled;

            ApplyState(true /*useTransitions*/);
        }

        private void DataGridColumnHeader_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            // Debug.WriteLine("DataGridColumnHeader.DataGridColumnHeader_PointerReleased");
            if (this.OwningGrid == null || this.OwningColumn == null || e.Handled || !this.IsEnabled)
            {
                return;
            }

            PointerPoint pointerPoint = e.GetCurrentPoint(this);

            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse && pointerPoint.Properties.IsLeftButtonPressed)
            {
                return;
            }

            if (_dragPointerId != 0 && _dragPointerId != e.Pointer.PointerId)
            {
                return;
            }

            Point pointerPosition = pointerPoint.Position;
            Point pointerPositionHeaders = e.GetCurrentPoint(this.OwningGrid.ColumnHeaders).Position;
            bool handled = e.Handled;

            this.IsPressed = false;

            if (this.OwningGrid.ColumnHeaders != null)
            {
                switch (_dragMode)
                {
                    case DragMode.PointerPressed:
                    {
                        if (this.OwningGrid.EditingRow == null &&
                            this.OwningColumn != this.OwningGrid.ColumnsInternal.FillerColumn &&
                            this.OwningGrid.CanUserSortColumns &&
                            this.OwningColumn.CanUserSort)
                        {
                            // Completed a click or tap without dragging, so raise the DataGrid.Sorting event.
                            DataGridColumnEventArgs ea = new DataGridColumnEventArgs(this.OwningColumn);
                            this.OwningGrid.OnColumnSorting(ea);

#if FEATURE_ICOLLECTIONVIEW_SORT
                        InvokeProcessSort();
#endif
                            handled = true;
                        }

                        break;
                    }

                    case DragMode.Reorder:
                    {
                        // Find header hovered over
                        int targetIndex = this.GetReorderingTargetDisplayIndex(pointerPositionHeaders);

                        if ((!this.OwningColumn.IsFrozen && targetIndex >= this.OwningGrid.FrozenColumnCount) ||
                            (this.OwningColumn.IsFrozen && targetIndex < this.OwningGrid.FrozenColumnCount))
                        {
                            this.OwningColumn.DisplayIndex = targetIndex;

                            DataGridColumnEventArgs ea = new DataGridColumnEventArgs(this.OwningColumn);
                            this.OwningGrid.OnColumnReordered(ea);
                        }

                        DragCompletedEventArgs dragCompletedEventArgs = new DragCompletedEventArgs(pointerPosition.X - _dragStart.Value.X, pointerPosition.Y - _dragStart.Value.Y, false);
                        this.OwningGrid.OnColumnHeaderDragCompleted(dragCompletedEventArgs);
                        break;
                    }

                    case DragMode.Drag:
                    {
                        DragCompletedEventArgs dragCompletedEventArgs = new DragCompletedEventArgs(0, 0, false);
                        this.OwningGrid.OnColumnHeaderDragCompleted(dragCompletedEventArgs);
                        break;
                    }
                }

                SetResizeCursor(e.Pointer, pointerPosition);

                // Variables that track drag mode states get reset in DataGridColumnHeader_LostPointerCapture
                if (_capturedPointer != null)
                {
                    ReleasePointerCapture(_capturedPointer);
                    _capturedPointer = null;
                }

                _dragMode = DragMode.None;
                _dragPointerId = 0;
                _dragColumn = null;
                _dragStart = null;
                _pressedPointerPositionHeaders = null;
                _lastPointerPositionHeaders = null;
                handled = true;
            }

            e.Handled = handled;

            ApplyState(true /*useTransitions*/);
        }

        private void DataGridColumnHeader_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            // Debug.WriteLine("DataGridColumnHeader.DataGridColumnHeader_PointerMoved");
            if (this.OwningColumn == null || this.OwningGrid == null || this.OwningGrid.ColumnHeaders == null || !this.IsEnabled)
            {
                return;
            }

            PointerPoint pointerPoint = e.GetCurrentPoint(this);
            Point pointerPosition = pointerPoint.Position;

            if (pointerPoint.IsInContact && (_dragPointerId == 0 || _dragPointerId == e.Pointer.PointerId))
            {
                Point pointerPositionHeaders = e.GetCurrentPoint(this.OwningGrid.ColumnHeaders).Position;
                bool handled = false;

                Debug.Assert(this.OwningGrid.Parent is UIElement, "Expected owning DataGrid's parent to be a UIElement.");

                double distanceFromLeft = pointerPosition.X;
                double distanceFromRight = this.ActualWidth - distanceFromLeft;

                OnPointerMove_Resize(ref handled, pointerPositionHeaders);
                OnPointerMove_Reorder(ref handled, e.Pointer, pointerPosition, pointerPositionHeaders, distanceFromLeft, distanceFromRight);

                // If we still haven't done anything about moving the pointer while
                // the pointer is down, we remember that we're dragging, but we don't
                // claim to have actually handled the event.
                if (_dragMode == DragMode.PointerPressed &&
                    _pressedPointerPositionHeaders.HasValue &&
                    Math.Abs(_pressedPointerPositionHeaders.Value.X - pointerPositionHeaders.X) + Math.Abs(_pressedPointerPositionHeaders.Value.Y - pointerPositionHeaders.Y) > DATAGRIDCOLUMNHEADER_dragThreshold)
                {
                    _dragMode = DragMode.Drag;
                    _dragPointerId = e.Pointer.PointerId;
                }

                if (_dragMode == DragMode.Drag)
                {
                    DragDeltaEventArgs dragDeltaEventArgs = new DragDeltaEventArgs(pointerPositionHeaders.X - _lastPointerPositionHeaders.Value.X, pointerPositionHeaders.Y - _lastPointerPositionHeaders.Value.Y);
                    this.OwningGrid.OnColumnHeaderDragDelta(dragDeltaEventArgs);
                }

                _lastPointerPositionHeaders = pointerPositionHeaders;
            }

            SetResizeCursor(e.Pointer, pointerPosition);
        }

        /// <summary>
        /// Returns the column against whose top-left the reordering caret should be positioned
        /// </summary>
        /// <param name="pointerPositionHeaders">Pointer position within the ColumnHeadersPresenter</param>
        /// <param name="scroll">Whether or not to scroll horizontally when a column is dragged out of bounds</param>
        /// <param name="scrollAmount">If scroll is true, returns the horizontal amount that was scrolled</param>
        /// <returns>The column against whose top-left the reordering caret should be positioned.</returns>
        private DataGridColumn GetReorderingTargetColumn(Point pointerPositionHeaders, bool scroll, out double scrollAmount)
        {
            scrollAmount = 0;
            double leftEdge = this.OwningGrid.ColumnsInternal.RowGroupSpacerColumn.IsRepresented ? this.OwningGrid.ColumnsInternal.RowGroupSpacerColumn.ActualWidth : 0;
            double rightEdge = this.OwningGrid.CellsWidth;
            if (this.OwningColumn.IsFrozen)
            {
                rightEdge = Math.Min(rightEdge, _frozenColumnsWidth);
            }
            else if (this.OwningGrid.FrozenColumnCount > 0)
            {
                leftEdge = _frozenColumnsWidth;
            }

            if (pointerPositionHeaders.X < leftEdge)
            {
                if (scroll &&
                    this.OwningGrid.HorizontalScrollBar != null &&
                    this.OwningGrid.HorizontalScrollBar.Visibility == Visibility.Visible &&
                    this.OwningGrid.HorizontalScrollBar.Value > 0)
                {
                    double newVal = pointerPositionHeaders.X - leftEdge;
                    scrollAmount = Math.Min(newVal, this.OwningGrid.HorizontalScrollBar.Value);
                    this.OwningGrid.UpdateHorizontalOffset(scrollAmount + this.OwningGrid.HorizontalScrollBar.Value);
                }

                pointerPositionHeaders.X = leftEdge;
            }
            else if (pointerPositionHeaders.X >= rightEdge)
            {
                if (scroll &&
                    this.OwningGrid.HorizontalScrollBar != null &&
                    this.OwningGrid.HorizontalScrollBar.Visibility == Visibility.Visible &&
                    this.OwningGrid.HorizontalScrollBar.Value < this.OwningGrid.HorizontalScrollBar.Maximum)
                {
                    double newVal = pointerPositionHeaders.X - rightEdge;
                    scrollAmount = Math.Min(newVal, this.OwningGrid.HorizontalScrollBar.Maximum - this.OwningGrid.HorizontalScrollBar.Value);
                    this.OwningGrid.UpdateHorizontalOffset(scrollAmount + this.OwningGrid.HorizontalScrollBar.Value);
                }

                pointerPositionHeaders.X = rightEdge - 1;
            }

            foreach (DataGridColumn column in this.OwningGrid.ColumnsInternal.GetDisplayedColumns())
            {
                Point pointerPosition = this.OwningGrid.ColumnHeaders.Translate(column.HeaderCell, pointerPositionHeaders);
                double columnMiddle = column.HeaderCell.ActualWidth / 2;
                if (pointerPosition.X >= 0 && pointerPosition.X <= columnMiddle)
                {
                    return column;
                }
                else if (pointerPosition.X > columnMiddle && pointerPosition.X < column.HeaderCell.ActualWidth)
                {
                    return this.OwningGrid.ColumnsInternal.GetNextVisibleColumn(column);
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the display index to set the column to
        /// </summary>
        /// <param name="pointerPositionHeaders">Pointer position relative to the column headers presenter</param>
        /// <returns>The display index to set the column to.</returns>
        private int GetReorderingTargetDisplayIndex(Point pointerPositionHeaders)
        {
            double scrollAmount = 0;
            DataGridColumn targetColumn = GetReorderingTargetColumn(pointerPositionHeaders, false /*scroll*/, out scrollAmount);
            if (targetColumn != null)
            {
                return targetColumn.DisplayIndex > this.OwningColumn.DisplayIndex ? targetColumn.DisplayIndex - 1 : targetColumn.DisplayIndex;
            }
            else
            {
                return this.OwningGrid.Columns.Count - 1;
            }
        }

        private void OnPointerMove_BeginReorder(uint pointerId, Point pointerPosition)
        {
            DataGridColumnHeader dragIndicator = new DataGridColumnHeader();
            dragIndicator.OwningColumn = this.OwningColumn;
            dragIndicator.IsEnabled = false;
            dragIndicator.Content = this.Content;
            dragIndicator.ContentTemplate = this.ContentTemplate;

            Control dropLocationIndicator = new ContentControl();
            dropLocationIndicator.SetStyleWithType(this.OwningGrid.DropLocationIndicatorStyle);

            if (this.OwningColumn.DragIndicatorStyle != null)
            {
                dragIndicator.SetStyleWithType(this.OwningColumn.DragIndicatorStyle);
            }
            else if (this.OwningGrid.DragIndicatorStyle != null)
            {
                dragIndicator.SetStyleWithType(this.OwningGrid.DragIndicatorStyle);
            }

            // If the user didn't style the dragIndicator's Width, default it to the column header's width.
            if (double.IsNaN(dragIndicator.Width))
            {
                dragIndicator.Width = this.ActualWidth;
            }

            // If the user didn't style the dropLocationIndicator's Height, default to the column header's height.
            if (double.IsNaN(dropLocationIndicator.Height))
            {
                dropLocationIndicator.Height = this.ActualHeight;
            }

            // pass the caret's data template to the user for modification.
            DataGridColumnReorderingEventArgs columnReorderingEventArgs = new DataGridColumnReorderingEventArgs(this.OwningColumn)
            {
                DropLocationIndicator = dropLocationIndicator,
                DragIndicator = dragIndicator
            };
            this.OwningGrid.OnColumnReordering(columnReorderingEventArgs);
            if (columnReorderingEventArgs.Cancel)
            {
                return;
            }

            // The app didn't cancel, so prepare for the reorder.
            _dragColumn = this.OwningColumn;
            Debug.Assert(_dragMode != DragMode.None, "Expected _dragMode other than None.");
            _dragMode = DragMode.Reorder;
            _dragPointerId = pointerId;
            _dragStart = pointerPosition;

            // Display the reordering thumb.
            this.OwningGrid.ColumnHeaders.DragColumn = this.OwningColumn;
            this.OwningGrid.ColumnHeaders.DragIndicator = columnReorderingEventArgs.DragIndicator;
            this.OwningGrid.ColumnHeaders.DropLocationIndicator = columnReorderingEventArgs.DropLocationIndicator;
        }

        private void OnPointerMove_Reorder(ref bool handled, Pointer pointer, Point pointerPosition, Point pointerPositionHeaders, double distanceFromLeft, double distanceFromRight)
        {
            if (handled)
            {
                return;
            }

            int resizeRegionWidth = pointer.PointerDeviceType == PointerDeviceType.Touch ? DATAGRIDCOLUMNHEADER_resizeRegionWidthLoose : DATAGRIDCOLUMNHEADER_resizeRegionWidthStrict;

            // Handle entry into reorder mode
            if (_dragMode == DragMode.PointerPressed &&
                _dragColumn == null &&
                distanceFromRight > resizeRegionWidth &&
                distanceFromLeft > resizeRegionWidth &&
                _pressedPointerPositionHeaders.HasValue &&
                Math.Abs(_pressedPointerPositionHeaders.Value.X - pointerPositionHeaders.X) + Math.Abs(_pressedPointerPositionHeaders.Value.Y - pointerPositionHeaders.Y) > DATAGRIDCOLUMNHEADER_dragThreshold)
            {
                DragStartedEventArgs dragStartedEventArgs =
                    new DragStartedEventArgs(pointerPositionHeaders.X - _lastPointerPositionHeaders.Value.X, pointerPositionHeaders.Y - _lastPointerPositionHeaders.Value.Y);
                this.OwningGrid.OnColumnHeaderDragStarted(dragStartedEventArgs);

                handled = CanReorderColumn(this.OwningColumn);

                if (handled)
                {
                    OnPointerMove_BeginReorder(pointer.PointerId, pointerPosition);
                }
            }

            // Handle reorder mode (eg, positioning of the popup)
            if (_dragMode == DragMode.Reorder && this.OwningGrid.ColumnHeaders.DragIndicator != null)
            {
                DragDeltaEventArgs dragDeltaEventArgs = new DragDeltaEventArgs(pointerPositionHeaders.X - _lastPointerPositionHeaders.Value.X, pointerPositionHeaders.Y - _lastPointerPositionHeaders.Value.Y);
                this.OwningGrid.OnColumnHeaderDragDelta(dragDeltaEventArgs);

                // Find header we're hovering over
                double scrollAmount = 0;
                DataGridColumn targetColumn = this.GetReorderingTargetColumn(pointerPositionHeaders, !this.OwningColumn.IsFrozen /*scroll*/, out scrollAmount);

                this.OwningGrid.ColumnHeaders.DragIndicatorOffset = pointerPosition.X - _dragStart.Value.X + scrollAmount;
                this.OwningGrid.ColumnHeaders.InvalidateArrange();

                if (this.OwningGrid.ColumnHeaders.DropLocationIndicator != null)
                {
                    Point targetPosition = new Point(0, 0);
                    if (targetColumn == null || targetColumn == this.OwningGrid.ColumnsInternal.FillerColumn || targetColumn.IsFrozen != this.OwningColumn.IsFrozen)
                    {
                        targetColumn = this.OwningGrid.ColumnsInternal.GetLastColumn(true /*isVisible*/, this.OwningColumn.IsFrozen /*isFrozen*/, null /*isReadOnly*/);
                        targetPosition = targetColumn.HeaderCell.Translate(this.OwningGrid.ColumnHeaders, targetPosition);
                        targetPosition.X += targetColumn.ActualWidth;
                    }
                    else
                    {
                        targetPosition = targetColumn.HeaderCell.Translate(this.OwningGrid.ColumnHeaders, targetPosition);
                    }

                    this.OwningGrid.ColumnHeaders.DropLocationIndicatorOffset = targetPosition.X - scrollAmount;
                }

                handled = true;
            }
        }

        private void OnPointerMove_Resize(ref bool handled, Point pointerPositionHeaders)
        {
            if (!handled && _dragMode == DragMode.Resize && _dragColumn != null && _dragStart.HasValue)
            {
                Debug.Assert(_resizePointerId != 0, "Expected _resizePointerId other than 0.");

                // Resize column
                double pointerDelta = pointerPositionHeaders.X - _dragStart.Value.X;
                double desiredWidth = _originalWidth + pointerDelta;

                desiredWidth = Math.Max(_dragColumn.ActualMinWidth, Math.Min(_dragColumn.ActualMaxWidth, desiredWidth));
                _dragColumn.Resize(_dragColumn.Width.Value, _dragColumn.Width.UnitType, _dragColumn.Width.DesiredValue, desiredWidth, true);

                this.OwningGrid.UpdateHorizontalOffset(_originalHorizontalOffset);

                handled = true;
            }
        }

        private void SetOriginalCursor()
        {
            if (_resizePointerId != 0)
            {
                Debug.Assert(_originalCursor != null, "Expected non-null _originalCursor.");

                Window.Current.CoreWindow.PointerCursor = _originalCursor;
                _resizePointerId = 0;
            }
        }

        private void SetResizeCursor(Pointer pointer, Point pointerPosition)
        {
            if (_dragMode != DragMode.None || this.OwningGrid == null || this.OwningColumn == null)
            {
                return;
            }

            // Set mouse cursor if we can resize column.
            double distanceFromLeft = pointerPosition.X;
            double distanceFromTop = pointerPosition.Y;
            double distanceFromRight = this.ActualWidth - distanceFromLeft;
            DataGridColumn currentColumn = this.OwningColumn;
            DataGridColumn previousColumn = null;

            if (!(this.OwningColumn is DataGridFillerColumn))
            {
                previousColumn = this.OwningGrid.ColumnsInternal.GetPreviousVisibleNonFillerColumn(currentColumn);
            }

            int resizeRegionWidth = pointer.PointerDeviceType == PointerDeviceType.Touch ? DATAGRIDCOLUMNHEADER_resizeRegionWidthLoose : DATAGRIDCOLUMNHEADER_resizeRegionWidthStrict;
            bool nearCurrentResizableColumnRightEdge = distanceFromRight <= resizeRegionWidth && currentColumn != null && CanResizeColumn(currentColumn) && distanceFromTop < this.ActualHeight;
            bool nearPreviousResizableColumnLeftEdge = distanceFromLeft <= resizeRegionWidth && previousColumn != null && CanResizeColumn(previousColumn) && distanceFromTop < this.ActualHeight;

            if (nearCurrentResizableColumnRightEdge || nearPreviousResizableColumnLeftEdge)
            {
                if (Window.Current.CoreWindow.PointerCursor != null && Window.Current.CoreWindow.PointerCursor.Type != CoreCursorType.SizeWestEast)
                {
                    _originalCursor = Window.Current.CoreWindow.PointerCursor;
                    _resizePointerId = pointer.PointerId;
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeWestEast, 0);
                }
            }
            else
            {
                if (_resizePointerId == pointer.PointerId)
                {
                    SetOriginalCursor();
                }
            }
        }
    }
}