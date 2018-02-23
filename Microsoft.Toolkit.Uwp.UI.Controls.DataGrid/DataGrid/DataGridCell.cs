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

using System.Diagnostics;
using Microsoft.Toolkit.Uwp.UI.Controls.DataGridInternals;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
#else
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
#endif

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    /// <summary>
    /// Represents an individual <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.DataGrid"/> cell.
    /// </summary>
    [TemplatePart(Name = DATAGRIDCELL_elementRightGridLine, Type = typeof(Rectangle))]

    [TemplateVisualState(Name = VisualStates.StateNormal, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StateMouseOver, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StateUnselected, GroupName = VisualStates.GroupSelection)]
    [TemplateVisualState(Name = VisualStates.StateSelected, GroupName = VisualStates.GroupSelection)]
    [TemplateVisualState(Name = VisualStates.StateUnfocused, GroupName = VisualStates.GroupFocus)]
    [TemplateVisualState(Name = VisualStates.StateFocused, GroupName = VisualStates.GroupFocus)]
    [TemplateVisualState(Name = VisualStates.StateRegular, GroupName = VisualStates.GroupCurrent)]
    [TemplateVisualState(Name = VisualStates.StateCurrent, GroupName = VisualStates.GroupCurrent)]
    [TemplateVisualState(Name = VisualStates.StateDisplay, GroupName = VisualStates.GroupInteraction)]
    [TemplateVisualState(Name = VisualStates.StateEditing, GroupName = VisualStates.GroupInteraction)]
    [TemplateVisualState(Name = VisualStates.StateInvalid, GroupName = VisualStates.GroupValidation)]
    [TemplateVisualState(Name = VisualStates.StateValid, GroupName = VisualStates.GroupValidation)]
    public sealed partial class DataGridCell : ContentControl
    {
        private const string DATAGRIDCELL_elementRightGridLine = "RightGridLine";

        private Rectangle _rightGridLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.DataGridCell"/> class.
        /// </summary>
        public DataGridCell()
        {
#if WINDOWS_UWP
            // TODO - listen to mouse button down, PointerEnter and PointerLeave.
#else
            this.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(DataGridCell_MouseLeftButtonDown), true);
            this.MouseEnter += new MouseEventHandler(DataGridCell_MouseEnter);
            this.MouseLeave += new MouseEventHandler(DataGridCell_MouseLeave);
#endif

            DefaultStyleKey = typeof(DataGridCell);
        }

        /// <summary>
        /// Gets a value indicating whether the data in a cell is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return (bool)GetValue(IsValidProperty);
            }

            internal set
            {
                this.SetValueNoCallback(IsValidProperty, value);
            }
        }

        /// <summary>
        /// Identifies the IsValid dependency property.
        /// </summary>
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register(
                "IsValid",
                typeof(bool),
                typeof(DataGridCell),
                new PropertyMetadata(true, OnIsValidPropertyChanged));

        /// <summary>
        /// IsValidProperty property changed handler.
        /// </summary>
        /// <param name="d">DataGridCell that changed its IsValid.</param>
        /// <param name="e">DependencyPropertyChangedEventArgs.</param>
        private static void OnIsValidPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGridCell dataGridCell = (DataGridCell)d;
            if (!dataGridCell.IsHandlerSuspended(e.Property))
            {
                dataGridCell.SetValueNoCallback(DataGridCell.IsValidProperty, e.OldValue);
                throw DataGridError.DataGrid.UnderlyingPropertyIsReadOnly("IsValid");
            }
        }

        internal double ActualRightGridLineWidth
        {
            get
            {
                if (_rightGridLine != null)
                {
                    return _rightGridLine.ActualWidth;
                }

                return 0;
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

        internal bool IsCurrent
        {
            get
            {
                Debug.Assert(this.OwningGrid != null && this.OwningColumn != null && this.OwningRow != null, "Expected non-null owning DataGrid, DataGridColumn and DataGridRow.");

                return this.OwningGrid.CurrentColumnIndex == this.OwningColumn.Index &&
                       this.OwningGrid.CurrentSlot == this.OwningRow.Slot;
            }
        }

        internal DataGridColumn OwningColumn
        {
            get;
            set;
        }

        internal DataGrid OwningGrid
        {
            get
            {
                if (this.OwningRow != null && this.OwningRow.OwningGrid != null)
                {
                    return this.OwningRow.OwningGrid;
                }
                if (this.OwningColumn != null)
                {
                    return this.OwningColumn.OwningGrid;
                }
                return null;
            }
        }

        internal DataGridRow OwningRow
        {
            get;
            set;
        }

        internal int RowIndex
        {
            get
            {
                if (this.OwningRow == null)
                {
                    return -1;
                }

                return this.OwningRow.Index;
            }
        }

        private bool IsEdited
        {
            get
            {
                Debug.Assert(this.OwningGrid != null, "Expected non-null owning DataGrid.");

                return this.OwningGrid.EditingRow == this.OwningRow &&
                       this.OwningGrid.EditingColumnIndex == this.ColumnIndex;
            }
        }

        private bool IsMouseOver
        {
            get
            {
                return this.OwningRow != null && this.OwningRow.MouseOverColumnIndex == this.ColumnIndex;
            }

            set
            {
                Debug.Assert(this.OwningRow != null, "Expected non-null owning DataGridRow.");

                if (value != this.IsMouseOver)
                {
                    if (value)
                    {
                        this.OwningRow.MouseOverColumnIndex = this.ColumnIndex;
                    }
                    else
                    {
                        this.OwningRow.MouseOverColumnIndex = null;
                    }
                }
            }
        }

        /// <summary>
        /// Builds the visual tree for the row header when a new template is applied.
        /// </summary>
#if WINDOWS_UWP
        protected
#else
        public
#endif
        override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ApplyCellState(false /*animate*/);

            this._rightGridLine = GetTemplateChild(DATAGRIDCELL_elementRightGridLine) as Rectangle;
            if (_rightGridLine != null && this.OwningColumn == null)
            {
                // Turn off the right GridLine for filler cells
                _rightGridLine.Visibility = Visibility.Collapsed;
            }
            else
            {
                EnsureGridLine(null);
            }
        }

        /// <summary>
        /// Creates AutomationPeer (<see cref="UIElement.OnCreateAutomationPeer"/>)
        /// </summary>
        /// <returns>An automation peer for this <see cref="T:System.Windows.Controls.DataGridCell"/>.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if (this.OwningGrid != null &&
                this.OwningColumn != null &&
                this.OwningColumn != this.OwningGrid.ColumnsInternal.FillerColumn)
            {
                // TODO - return new DataGridCellAutomationPeer(this);
                return null;
            }

            return base.OnCreateAutomationPeer();
        }

        internal void ApplyCellState(bool animate)
        {
            if (this.OwningGrid == null || this.OwningColumn == null || this.OwningRow == null || this.OwningRow.Visibility == Visibility.Collapsed || this.OwningRow.Slot == -1)
            {
                return;
            }

            // CommonStates
            if (this.IsMouseOver)
            {
                VisualStates.GoToState(this, animate, VisualStates.StateMouseOver, VisualStates.StateNormal);
            }
            else
            {
                VisualStates.GoToState(this, animate, VisualStates.StateNormal);
            }

            // SelectionStates
            if (this.OwningRow.IsSelected)
            {
                VisualStates.GoToState(this, animate, VisualStates.StateSelected, VisualStates.StateUnselected);
            }
            else
            {
                VisualStates.GoToState(this, animate, VisualStates.StateUnselected);
            }

            // FocusStates
            if (this.OwningGrid.ContainsFocus)
            {
                VisualStates.GoToState(this, animate, VisualStates.StateFocused, VisualStates.StateUnfocused);
            }
            else
            {
                VisualStates.GoToState(this, animate, VisualStates.StateUnfocused);
            }

            // CurrentStates
            if (this.IsCurrent)
            {
                VisualStates.GoToState(this, animate, VisualStates.StateCurrent, VisualStates.StateRegular);
            }
            else
            {
                VisualStates.GoToState(this, animate, VisualStates.StateRegular);
            }

            // Interaction states
            if (this.IsEdited)
            {
                VisualStates.GoToState(this, animate, VisualStates.StateEditing, VisualStates.StateDisplay);
            }
            else
            {
                VisualStates.GoToState(this, animate, VisualStates.StateDisplay);
            }

            // Validation states
            if (this.IsValid)
            {
                VisualStates.GoToState(this, animate, VisualStates.StateValid);
            }
            else
            {
                VisualStates.GoToState(this, animate, VisualStates.StateInvalid, VisualStates.StateValid);
            }
        }

        /// <summary>
        /// Ensures that the correct Style is applied to this object.
        /// </summary>
        /// <param name="previousStyle">Caller's previous associated Style</param>
        internal void EnsureStyle(Style previousStyle)
        {
            if (this.Style != null
                && (this.OwningColumn == null || this.Style != this.OwningColumn.CellStyle)
                && (this.OwningGrid == null || this.Style != this.OwningGrid.CellStyle)
                && (this.Style != previousStyle))
            {
                return;
            }

            Style style = null;
            if (this.OwningColumn != null)
            {
                style = this.OwningColumn.CellStyle;
            }

            if (style == null && this.OwningGrid != null)
            {
                style = this.OwningGrid.CellStyle;
            }

            this.SetStyleWithType(style);
        }

        // Makes sure the right gridline has the proper stroke and visibility. If lastVisibleColumn is specified, the
        // right gridline will be collapsed if this cell belongs to the lastVisibileColumn and there is no filler column
        internal void EnsureGridLine(DataGridColumn lastVisibleColumn)
        {
            if (this.OwningGrid != null && _rightGridLine != null)
            {
                if (this.OwningGrid.VerticalGridLinesBrush != null && this.OwningGrid.VerticalGridLinesBrush != _rightGridLine.Fill)
                {
                    _rightGridLine.Fill = this.OwningGrid.VerticalGridLinesBrush;
                }

                Visibility newVisibility =
                    (this.OwningGrid.GridLinesVisibility == DataGridGridLinesVisibility.Vertical || this.OwningGrid.GridLinesVisibility == DataGridGridLinesVisibility.All) &&
                    (this.OwningGrid.ColumnsInternal.FillerColumn.IsActive || this.OwningColumn != lastVisibleColumn)
                    ? Visibility.Visible : Visibility.Collapsed;

                if (newVisibility != _rightGridLine.Visibility)
                {
                    _rightGridLine.Visibility = newVisibility;
                }
            }
        }

#if !WINDOWS_UWP
        private void DataGridCell_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.OwningRow != null)
            {
                this.IsMouseOver = true;
            }
        }

        private void DataGridCell_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.OwningRow != null)
            {
                this.IsMouseOver = false;
            }
        }

        private void DataGridCell_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // OwningGrid is null for TopLeftHeaderCell and TopRightHeaderCell because they have no OwningRow
            if (this.OwningGrid != null)
            {
                if (!e.Handled && this.OwningGrid.IsTabStop)
                {
                    bool success = this.OwningGrid.Focus();
                    Debug.Assert(success);
                }

                if (this.OwningRow != null)
                {
                    Debug.Assert(sender is DataGridCell);
                    Debug.Assert(sender == this);
                    e.Handled = this.OwningGrid.UpdateStateOnMouseLeftButtonDown(e, this.ColumnIndex, this.OwningRow.Slot, !e.Handled);
                    this.OwningGrid.UpdatedStateOnMouseLeftButtonDown = true;
                }
            }
        }
#endif
    }
}
