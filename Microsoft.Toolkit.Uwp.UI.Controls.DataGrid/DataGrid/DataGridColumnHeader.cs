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
using Microsoft.Toolkit.Uwp.UI.Controls.DataGridInternals;
#if WINDOWS_UWP
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#else
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
#endif

namespace Microsoft.Toolkit.Uwp.UI.Controls.Primitives
{
    /// <summary>
    /// Represents an individual <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.DataGrid"/> column header.
    /// </summary>
    [TemplateVisualState(Name = VisualStates.StateNormal, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StateMouseOver, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StatePressed, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StateUnsorted, GroupName = VisualStates.GroupSort)]
    [TemplateVisualState(Name = VisualStates.StateSortAscending, GroupName = VisualStates.GroupSort)]
    [TemplateVisualState(Name = VisualStates.StateSortDescending, GroupName = VisualStates.GroupSort)]
    public partial class DataGridColumnHeader : ContentControl
    {
        private enum DragMode
        {
            None = 0,
            MouseDown = 1,
            Drag = 2,
            Resize = 3,
            Reorder = 4
        }

        private const int DATAGRIDCOLUMNHEADER_resizeRegionWidth = 5;
        private const double DATAGRIDCOLUMNHEADER_separatorThickness = 1;

#if WINDOWS_UWP
        private static CoreCursorType _originalCursor;
#else
        private static Cursor _originalCursor;
#endif
        private static DragMode _dragMode;
        private static Point? _lastMousePositionHeaders;
        private static double _originalHorizontalOffset;
        private static double _originalWidth;
        private static Point? _dragStart;
        private static Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumn _dragColumn;
        private static double _frozenColumnsWidth;

#if WINDOWS_UWP
        private CoreCursorType _cursor;
        private Pointer _capturedPointer;
#endif
        private Visibility _desiredSeparatorVisibility;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridColumnHeader"/> class.
        /// </summary>
        public DataGridColumnHeader()
        {
#if WINDOWS_UWP
            this.PointerCaptureLost += new PointerEventHandler(DataGridColumnHeader_PointerCaptureLost);
            this.PointerPressed += new PointerEventHandler(DataGridColumnHeader_PointerPressed);
            this.PointerReleased += new PointerEventHandler(DataGridColumnHeader_PointerReleased);
            this.PointerMoved += new PointerEventHandler(DataGridColumnHeader_PointerMoved);
            this.PointerEntered += new PointerEventHandler(DataGridColumnHeader_PointerEntered);
            this.PointerExited += new PointerEventHandler(DataGridColumnHeader_PointerExited);
#else
            this.LostMouseCapture += new MouseEventHandler(DataGridColumnHeader_LostMouseCapture);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(DataGridColumnHeader_MouseLeftButtonDown);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(DataGridColumnHeader_MouseLeftButtonUp);
            this.MouseMove += new MouseEventHandler(DataGridColumnHeader_MouseMove);
            this.MouseEnter += new MouseEventHandler(DataGridColumnHeader_MouseEnter);
            this.MouseLeave += new MouseEventHandler(DataGridColumnHeader_MouseLeave);
#endif

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

        internal Microsoft.Toolkit.Uwp.UI.Controls.DataGrid OwningGrid
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

        internal Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumn OwningColumn
        {
            get;
            set;
        }

#if WINDOWS_UWP
        private bool IsMouseOver
        {
            get;
            set;
        }
#endif

        private bool IsPressed
        {
            get;
            set;
        }

        /// <summary>
        /// Builds the visual tree for the column header when a new template is applied.
        /// </summary>
#if WINDOWS_UWP
        protected
#else
        public
#endif
        override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ApplyState(false);
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
                // TODO - return new DataGridColumnHeaderAutomationPeer(this);
                return null;
            }

            return base.OnCreateAutomationPeer();
        }

        internal void ApplyState(bool useTransitions)
        {
            // Common States
            if (this.IsPressed && _dragMode != DragMode.Resize)
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StatePressed, VisualStates.StateMouseOver, VisualStates.StateNormal);
            }
            else if (this.IsMouseOver && _dragMode != DragMode.Resize)
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateMouseOver, VisualStates.StateNormal);
            }
            else
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateNormal);
            }

#if FEATURE_ICOLLECTIONVIEW_SORT
            // Sort States
            this.CurrentSortingState = null;

            if (this.OwningGrid != null
                && this.OwningGrid.DataConnection != null
                && this.OwningGrid.DataConnection.AllowSort)
            {
                SortDescription? sort = this.OwningColumn.GetSortDescription();

                if (sort.HasValue)
                {
                    this.CurrentSortingState = sort.Value.Direction;
                    if (this.CurrentSortingState == ListSortDirection.Ascending)
                    {
                        VisualStates.GoToState(this, useTransitions, VisualStates.StateSortAscending, VisualStates.StateUnsorted);
                    }
                    if (this.CurrentSortingState == ListSortDirection.Descending)
                    {
                        VisualStates.GoToState(this, useTransitions, VisualStates.StateSortDescending, VisualStates.StateUnsorted);
                    }
                }
                else
                {
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateUnsorted);
                }
            }
#endif
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
#if WINDOWS_UWP
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { ProcessSort(); }).AsTask();
#else
                this.Dispatcher.BeginInvoke(new Action(ProcessSort));
#endif
            }
        }
#endif

#if WINDOWS_UWP
        internal void OnMouseLeftButtonDown(ref bool handled, PointerRoutedEventArgs e)
#else
        internal void OnMouseLeftButtonDown(ref bool handled, Point mousePosition)
#endif
        {
            this.IsPressed = true;

            if (this.OwningGrid != null && this.OwningGrid.ColumnHeaders != null)
            {
#if WINDOWS_UWP
                Point mousePosition = e.GetCurrentPoint(this).Position;

                if (this.CapturePointer(e.Pointer))
                {
                    _capturedPointer = e.Pointer;
                }
                else
                {
                    _capturedPointer = null;
                }
#else
                this.CaptureMouse();
#endif

                _dragMode = DragMode.MouseDown;
                _frozenColumnsWidth = this.OwningGrid.ColumnsInternal.GetVisibleFrozenEdgedColumnsWidth();
                _lastMousePositionHeaders = this.Translate(this.OwningGrid.ColumnHeaders, mousePosition);

                double distanceFromLeft = mousePosition.X;
                double distanceFromRight = this.ActualWidth - distanceFromLeft;
                Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumn currentColumn = this.OwningColumn;
                Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumn previousColumn = null;
                if (!(this.OwningColumn is Microsoft.Toolkit.Uwp.UI.Controls.DataGridFillerColumn))
                {
                    previousColumn = this.OwningGrid.ColumnsInternal.GetPreviousVisibleNonFillerColumn(currentColumn);
                }

                if (_dragMode == DragMode.MouseDown && _dragColumn == null && (distanceFromRight <= DATAGRIDCOLUMNHEADER_resizeRegionWidth))
                {
                    handled = TrySetResizeColumn(currentColumn);
                }
                else if (_dragMode == DragMode.MouseDown && _dragColumn == null && distanceFromLeft <= DATAGRIDCOLUMNHEADER_resizeRegionWidth && previousColumn != null)
                {
                    handled = TrySetResizeColumn(previousColumn);
                }

                if (_dragMode == DragMode.Resize && _dragColumn != null)
                {
                    _dragStart = _lastMousePositionHeaders;
                    _originalWidth = _dragColumn.ActualWidth;
                    _originalHorizontalOffset = this.OwningGrid.HorizontalOffset;

                    handled = true;
                }
            }
        }

        internal void OnMouseLeftButtonUp(ref bool handled, Point mousePosition, Point mousePositionHeaders)
        {
            this.IsPressed = false;

            if (this.OwningGrid != null && this.OwningGrid.ColumnHeaders != null)
            {
                switch (_dragMode)
                {
#if FEATURE_ICOLLECTIONVIEW_SORT
                    case DragMode.MouseDown:
                    {
                        OnMouseLeftButtonUp_Click(ref handled);
                        break;
                    }
#endif
                    case DragMode.Reorder:
                    {
                        // Find header hovered over
                        int targetIndex = this.GetReorderingTargetDisplayIndex(mousePositionHeaders);

                        if ((!this.OwningColumn.IsFrozen && targetIndex >= this.OwningGrid.FrozenColumnCount) ||
                            (this.OwningColumn.IsFrozen && targetIndex < this.OwningGrid.FrozenColumnCount))
                        {
                            this.OwningColumn.DisplayIndex = targetIndex;

                            Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumnEventArgs ea = new Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumnEventArgs(this.OwningColumn);
                            this.OwningGrid.OnColumnReordered(ea);
                        }

                        DragCompletedEventArgs dragCompletedEventArgs = new DragCompletedEventArgs(mousePosition.X - _dragStart.Value.X, mousePosition.Y - _dragStart.Value.Y, false);
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

                SetDragCursor(mousePosition);

                // Variables that track drag mode states get reset in DataGridColumnHeader_LostMouseCapture
#if WINDOWS_UWP
                if (_capturedPointer != null)
                {
                    ReleasePointerCapture(_capturedPointer);
                    _capturedPointer = null;
                }
#else
                ReleaseMouseCapture();
#endif
                _dragMode = DragMode.None;
                handled = true;
            }
        }

#if FEATURE_ICOLLECTIONVIEW_SORT

        internal void OnMouseLeftButtonUp_Click(ref bool handled)
        {
            // completed a click without dragging, so we're sorting
            InvokeProcessSort();
            handled = true;
        }
#endif

        internal void OnMouseMove(ref bool handled, Point mousePosition, Point mousePositionHeaders)
        {
            if (handled || this.OwningGrid == null || this.OwningGrid.ColumnHeaders == null)
            {
                return;
            }

            Debug.Assert(this.OwningGrid.Parent is UIElement, "Expected owning DataGrid's parent to be a UIElement.");

            double distanceFromLeft = mousePosition.X;
            double distanceFromRight = this.ActualWidth - distanceFromLeft;

            OnMouseMove_Resize(ref handled, mousePositionHeaders);

            OnMouseMove_Reorder(ref handled, mousePosition, mousePositionHeaders, distanceFromLeft, distanceFromRight);

            // if we still haven't done anything about moving the mouse while
            // the button is down, we remember that we're dragging, but we don't
            // claim to have actually handled the event
            if (_dragMode == DragMode.MouseDown)
            {
                _dragMode = DragMode.Drag;
            }

            if (_dragMode == DragMode.Drag)
            {
                DragDeltaEventArgs dragDeltaEventArgs = new DragDeltaEventArgs(mousePositionHeaders.X - _lastMousePositionHeaders.Value.X, mousePositionHeaders.Y - _lastMousePositionHeaders.Value.Y);
                this.OwningGrid.OnColumnHeaderDragDelta(dragDeltaEventArgs);
            }

            _lastMousePositionHeaders = mousePositionHeaders;

            SetDragCursor(mousePosition);
        }

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
                    Microsoft.Toolkit.Uwp.UI.Controls.DataGrid owningGrid = this.OwningGrid;
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

        internal void UpdateSeparatorVisibility(Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumn lastVisibleColumn)
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
        private static bool CanResizeColumn(Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumn column)
        {
            if (column.OwningGrid != null && column.OwningGrid.ColumnsInternal != null && column.OwningGrid.UsesStarSizing &&
                (column.OwningGrid.ColumnsInternal.LastVisibleColumn == column || !DoubleUtil.AreClose(column.OwningGrid.ColumnsInternal.VisibleEdgedColumnsWidth, column.OwningGrid.CellsWidth)))
            {
                return false;
            }

            return column.ActualCanUserResize;
        }

        private static bool TrySetResizeColumn(Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumn column)
        {
            // If Datagrid.CanUserResizeColumns == false, then the column can still override it
            if (CanResizeColumn(column))
            {
                _dragColumn = column;

                _dragMode = DragMode.Resize;

                return true;
            }

            return false;
        }

        private bool CanReorderColumn(Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumn column)
        {
            return this.OwningGrid.CanUserReorderColumns &&
                !(column is Microsoft.Toolkit.Uwp.UI.Controls.DataGridFillerColumn) &&
                ((column.CanUserReorderInternal.HasValue && column.CanUserReorderInternal.Value) || !column.CanUserReorderInternal.HasValue);
        }

#if WINDOWS_UWP
        private void DataGridColumnHeader_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                OnLostMouseCapture();
            }
        }
#else
        private void DataGridColumnHeader_LostMouseCapture(object sender, MouseEventArgs e)
        {
            this.OnLostMouseCapture();
        }
#endif

#if WINDOWS_UWP
        private void DataGridColumnHeader_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!this.IsEnabled || e.Pointer.PointerDeviceType != PointerDeviceType.Mouse)
            {
                return;
            }

            OnMouseEnter(e.GetCurrentPoint(this).Position);
            ApplyState(true);
        }
#else
        private void DataGridColumnHeader_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            Point mousePosition = e.GetPosition(this);
            this.OnMouseEnter(mousePosition);
            ApplyState(true);
        }
#endif

#if WINDOWS_UWP
        private void DataGridColumnHeader_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!this.IsEnabled || e.Pointer.PointerDeviceType != PointerDeviceType.Mouse)
            {
                return;
            }

            OnMouseLeave();
            ApplyState(true);
        }
#else
        private void DataGridColumnHeader_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            this.OnMouseLeave();
            ApplyState(true);
        }
#endif

#if WINDOWS_UWP
        private void DataGridColumnHeader_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // TODO - Should Touch/Pen be supported too?
            if (this.OwningColumn == null || e.Handled || !this.IsEnabled || e.Pointer.PointerDeviceType != PointerDeviceType.Mouse)
            {
                return;
            }

            bool handled = e.Handled;
            OnMouseLeftButtonDown(ref handled, e);
            e.Handled = handled;

            ApplyState(true);
        }
#else
        private void DataGridColumnHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.OwningColumn == null || e.Handled || !this.IsEnabled)
            {
                return;
            }

            Point mousePosition = e.GetPosition(this);
            bool handled = e.Handled;
            OnMouseLeftButtonDown(ref handled, mousePosition);
            e.Handled = handled;

            ApplyState(true);
        }
#endif

#if WINDOWS_UWP
        private void DataGridColumnHeader_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (this.OwningColumn == null || e.Handled || !this.IsEnabled || e.Pointer.PointerDeviceType != PointerDeviceType.Mouse)
            {
                return;
            }

            Point mousePosition = e.GetCurrentPoint(this).Position;
            Point mousePositionHeaders = e.GetCurrentPoint(this.OwningGrid.ColumnHeaders).Position;
            bool handled = e.Handled;
            OnMouseLeftButtonUp(ref handled, mousePosition, mousePositionHeaders);
            e.Handled = handled;

            ApplyState(true);
        }
#else
        private void DataGridColumnHeader_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.OwningColumn == null || e.Handled || !this.IsEnabled)
            {
                return;
            }

            Point mousePosition = e.GetPosition(this);
            Point mousePositionHeaders = e.GetPosition(this.OwningGrid.ColumnHeaders);
            bool handled = e.Handled;
            OnMouseLeftButtonUp(ref handled, mousePosition, mousePositionHeaders);
            e.Handled = handled;

            ApplyState(true);
        }
#endif

#if WINDOWS_UWP
        private void DataGridColumnHeader_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (this.OwningColumn == null || !this.IsEnabled || e.Pointer.PointerDeviceType != PointerDeviceType.Mouse)
            {
                return;
            }

            Point mousePosition = e.GetCurrentPoint(this).Position;
            Point mousePositionHeaders = e.GetCurrentPoint(this.OwningGrid.ColumnHeaders).Position;
            bool handled = false;
            OnMouseMove(ref handled, mousePosition, mousePositionHeaders);
        }
#else
        private void DataGridColumnHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.OwningGrid == null || !this.IsEnabled)
            {
                return;
            }

            Point mousePosition = e.GetPosition(this);
            Point mousePositionHeaders = e.GetPosition(this.OwningGrid.ColumnHeaders);

            bool handled = false;
            OnMouseMove(ref handled, mousePosition, mousePositionHeaders);
        }
#endif

        /// <summary>
        /// Returns the column against whose top-left the reordering caret should be positioned
        /// </summary>
        /// <param name="mousePositionHeaders">Mouse position within the ColumnHeadersPresenter</param>
        /// <param name="scroll">Whether or not to scroll horizontally when a column is dragged out of bounds</param>
        /// <param name="scrollAmount">If scroll is true, returns the horizontal amount that was scrolled</param>
        /// <returns>The column against whose top-left the reordering caret should be positioned.</returns>
        private DataGridColumn GetReorderingTargetColumn(Point mousePositionHeaders, bool scroll, out double scrollAmount)
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

            if (mousePositionHeaders.X < leftEdge)
            {
                if (scroll &&
                    this.OwningGrid.HorizontalScrollBar != null &&
                    this.OwningGrid.HorizontalScrollBar.Visibility == Visibility.Visible &&
                    this.OwningGrid.HorizontalScrollBar.Value > 0)
                {
                    double newVal = mousePositionHeaders.X - leftEdge;
                    scrollAmount = Math.Min(newVal, this.OwningGrid.HorizontalScrollBar.Value);
                    this.OwningGrid.UpdateHorizontalOffset(scrollAmount + this.OwningGrid.HorizontalScrollBar.Value);
                }

                mousePositionHeaders.X = leftEdge;
            }
            else if (mousePositionHeaders.X >= rightEdge)
            {
                if (scroll &&
                    this.OwningGrid.HorizontalScrollBar != null &&
                    this.OwningGrid.HorizontalScrollBar.Visibility == Visibility.Visible &&
                    this.OwningGrid.HorizontalScrollBar.Value < this.OwningGrid.HorizontalScrollBar.Maximum)
                {
                    double newVal = mousePositionHeaders.X - rightEdge;
                    scrollAmount = Math.Min(newVal, this.OwningGrid.HorizontalScrollBar.Maximum - this.OwningGrid.HorizontalScrollBar.Value);
                    this.OwningGrid.UpdateHorizontalOffset(scrollAmount + this.OwningGrid.HorizontalScrollBar.Value);
                }

                mousePositionHeaders.X = rightEdge - 1;
            }

            foreach (Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumn column in this.OwningGrid.ColumnsInternal.GetDisplayedColumns())
            {
                Point mousePosition = this.OwningGrid.ColumnHeaders.Translate(column.HeaderCell, mousePositionHeaders);
                double columnMiddle = column.HeaderCell.ActualWidth / 2;
                if (mousePosition.X >= 0 && mousePosition.X <= columnMiddle)
                {
                    return column;
                }
                else if (mousePosition.X > columnMiddle && mousePosition.X < column.HeaderCell.ActualWidth)
                {
                    return this.OwningGrid.ColumnsInternal.GetNextVisibleColumn(column);
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the display index to set the column to
        /// </summary>
        /// <param name="mousePositionHeaders">Mouse position relative to the column headers presenter</param>
        /// <returns>The display index to set the column to.</returns>
        private int GetReorderingTargetDisplayIndex(Point mousePositionHeaders)
        {
            double scrollAmount = 0;
            Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumn targetColumn = GetReorderingTargetColumn(mousePositionHeaders, false /*scroll*/, out scrollAmount);
            if (targetColumn != null)
            {
                return targetColumn.DisplayIndex > this.OwningColumn.DisplayIndex ? targetColumn.DisplayIndex - 1 : targetColumn.DisplayIndex;
            }
            else
            {
                return this.OwningGrid.Columns.Count - 1;
            }
        }

        /// <summary>
        /// Resets the static DataGridColumnHeader properties when a header loses mouse capture.
        /// </summary>
        private void OnLostMouseCapture()
        {
            // When we stop interacting with the column headers, we need to reset the drag mode
            // and close any popups if they are open.
            if (_dragColumn != null && _dragColumn.HeaderCell != null)
            {
#if WINDOWS_UWP
                _dragColumn.HeaderCell._cursor = _originalCursor;
#else
                _dragColumn.HeaderCell.Cursor = _originalCursor;
#endif
            }

#if WINDOWS_UWP
            _capturedPointer = null;
#endif
            _dragMode = DragMode.None;
            _dragColumn = null;
            _dragStart = null;
            _lastMousePositionHeaders = null;

            if (this.OwningGrid != null && this.OwningGrid.ColumnHeaders != null)
            {
                this.OwningGrid.ColumnHeaders.DragColumn = null;
                this.OwningGrid.ColumnHeaders.DragIndicator = null;
                this.OwningGrid.ColumnHeaders.DropLocationIndicator = null;
            }
        }

        /// <summary>
        /// Sets up the DataGridColumnHeader for the MouseEnter event
        /// </summary>
        /// <param name="mousePosition">mouse position relative to the DataGridColumnHeader</param>
        private void OnMouseEnter(Point mousePosition)
        {
            // TODO - code removal correct?
            // this.IsMouseOver = true;
            SetDragCursor(mousePosition);
#if WINDOWS_UWP
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(_cursor, 0);
#endif
        }

        /// <summary>
        /// Sets up the DataGridColumnHeader for the MouseLeave event
        /// </summary>
        private void OnMouseLeave()
        {
            // TODO - code removal correct?
            // this.IsMouseOver = false;
#if WINDOWS_UWP
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(_cursor, 0);
#endif
        }

        private void OnMouseMove_BeginReorder(Point mousePosition)
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

            // If the user didn't style the dragIndicator's Width, default it to the column header's width
            if (double.IsNaN(dragIndicator.Width))
            {
                dragIndicator.Width = this.ActualWidth;
            }

            // If the user didn't style the dropLocationIndicator's Height, default to the column header's height
            if (double.IsNaN(dropLocationIndicator.Height))
            {
                dropLocationIndicator.Height = this.ActualHeight;
            }

            // pass the caret's data template to the user for modification
            Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumnReorderingEventArgs columnReorderingEventArgs = new Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumnReorderingEventArgs(this.OwningColumn)
            {
                DropLocationIndicator = dropLocationIndicator,
                DragIndicator = dragIndicator
            };
            this.OwningGrid.OnColumnReordering(columnReorderingEventArgs);
            if (columnReorderingEventArgs.Cancel)
            {
                return;
            }

            // The user didn't cancel, so prepare for the reorder
            _dragColumn = this.OwningColumn;
            _dragMode = DragMode.Reorder;
            _dragStart = mousePosition;

            // Display the reordering thumb
            this.OwningGrid.ColumnHeaders.DragColumn = this.OwningColumn;
            this.OwningGrid.ColumnHeaders.DragIndicator = columnReorderingEventArgs.DragIndicator;
            this.OwningGrid.ColumnHeaders.DropLocationIndicator = columnReorderingEventArgs.DropLocationIndicator;
        }

        private void OnMouseMove_Reorder(ref bool handled, Point mousePosition, Point mousePositionHeaders, double distanceFromLeft, double distanceFromRight)
        {
            if (handled)
            {
                return;
            }

            // Handle entry into reorder mode
            if (_dragMode == DragMode.MouseDown && _dragColumn == null && (distanceFromRight > DATAGRIDCOLUMNHEADER_resizeRegionWidth && distanceFromLeft > DATAGRIDCOLUMNHEADER_resizeRegionWidth))
            {
                DragStartedEventArgs dragStartedEventArgs = new DragStartedEventArgs(mousePositionHeaders.X - _lastMousePositionHeaders.Value.X, mousePositionHeaders.Y - _lastMousePositionHeaders.Value.Y);
                this.OwningGrid.OnColumnHeaderDragStarted(dragStartedEventArgs);

                handled = CanReorderColumn(this.OwningColumn);

                if (handled)
                {
                    OnMouseMove_BeginReorder(mousePosition);
                }
            }

            // Handle reorder mode (eg, positioning of the popup)
            if (_dragMode == DragMode.Reorder && this.OwningGrid.ColumnHeaders.DragIndicator != null)
            {
                DragDeltaEventArgs dragDeltaEventArgs = new DragDeltaEventArgs(mousePositionHeaders.X - _lastMousePositionHeaders.Value.X, mousePositionHeaders.Y - _lastMousePositionHeaders.Value.Y);
                this.OwningGrid.OnColumnHeaderDragDelta(dragDeltaEventArgs);

                // Find header we're hovering over
                double scrollAmount = 0;
                Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumn targetColumn = this.GetReorderingTargetColumn(mousePositionHeaders, !this.OwningColumn.IsFrozen /*scroll*/, out scrollAmount);

                this.OwningGrid.ColumnHeaders.DragIndicatorOffset = mousePosition.X - _dragStart.Value.X + scrollAmount;
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

        private void OnMouseMove_Resize(ref bool handled, Point mousePositionHeaders)
        {
            if (handled)
            {
                return;
            }

            if (_dragMode == DragMode.Resize && _dragColumn != null && _dragStart.HasValue)
            {
                // Resize column
                double mouseDelta = mousePositionHeaders.X - _dragStart.Value.X;
                double desiredWidth = _originalWidth + mouseDelta;

                desiredWidth = Math.Max(_dragColumn.ActualMinWidth, Math.Min(_dragColumn.ActualMaxWidth, desiredWidth));
                _dragColumn.Resize(_dragColumn.Width.Value, _dragColumn.Width.UnitType, _dragColumn.Width.DesiredValue, desiredWidth, true);

                this.OwningGrid.UpdateHorizontalOffset(_originalHorizontalOffset);

                handled = true;
            }
        }

        private void SetDragCursor(Point mousePosition)
        {
            if (_dragMode != DragMode.None || this.OwningGrid == null || this.OwningColumn == null)
            {
                return;
            }

            // Set mouse cusror if we can resize column
            double distanceFromLeft = mousePosition.X;
            double distanceFromRight = this.ActualWidth - distanceFromLeft;
            Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumn currentColumn = this.OwningColumn;
            Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumn previousColumn = null;

            if (!(this.OwningColumn is Microsoft.Toolkit.Uwp.UI.Controls.DataGridFillerColumn))
            {
                previousColumn = this.OwningGrid.ColumnsInternal.GetPreviousVisibleNonFillerColumn(currentColumn);
            }

            if ((distanceFromRight <= DATAGRIDCOLUMNHEADER_resizeRegionWidth && currentColumn != null && CanResizeColumn(currentColumn)) ||
                (distanceFromLeft <= DATAGRIDCOLUMNHEADER_resizeRegionWidth && previousColumn != null && CanResizeColumn(previousColumn)))
            {
#if WINDOWS_UWP
                if (_cursor != CoreCursorType.SizeWestEast)
                {
                    _originalCursor = _cursor;
                    _cursor = CoreCursorType.SizeWestEast;
                }
#else
                if (this.Cursor != Cursors.SizeWE)
                {
                    _originalCursor = this.Cursor;
                    this.Cursor = Cursors.SizeWE;
                }
#endif
            }
            else
            {
#if WINDOWS_UWP
                _cursor = _originalCursor;
#else
                this.Cursor = _originalCursor;
#endif
            }
        }
    }
}
