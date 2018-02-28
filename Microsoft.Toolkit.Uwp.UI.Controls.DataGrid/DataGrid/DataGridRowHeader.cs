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
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
#endif

namespace Microsoft.Toolkit.Uwp.UI.Controls.Primitives
{
    /// <summary>
    /// Represents an individual <see cref="T:System.Windows.Controls.DataGrid"/> row header.
    /// </summary>
    [TemplatePart(Name = DATAGRIDROWHEADER_elementRootName, Type = typeof(FrameworkElement))]

    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateNormal, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateNormalCurrentRow, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateNormalEditingRow, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateNormalEditingRowFocused, GroupName = VisualStates.GroupCommon)]

    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateMouseOver, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateMouseOverCurrentRow, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateMouseOverEditingRow, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateMouseOverEditingRowFocused, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateMouseOverSelected, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateMouseOverSelectedFocused, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRow, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRowFocused, GroupName = VisualStates.GroupCommon)]

    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateSelected, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateSelectedCurrentRow, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateSelectedCurrentRowFocused, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateSelectedFocused, GroupName = VisualStates.GroupCommon)]

    [TemplateVisualState(Name = VisualStates.StateRowInvalid, GroupName = VisualStates.GroupValidation)]
    [TemplateVisualState(Name = VisualStates.StateRowValid, GroupName = VisualStates.GroupValidation)]
    public partial class DataGridRowHeader : ContentControl
    {
        private const string DATAGRIDROWHEADER_elementRootName = "Root";
        private const double DATAGRIDROWHEADER_separatorThickness = 1;

        private const string DATAGRIDROWHEADER_stateMouseOver = "MouseOver";
        private const string DATAGRIDROWHEADER_stateMouseOverCurrentRow = "MouseOverCurrentRow";
        private const string DATAGRIDROWHEADER_stateMouseOverCurrentRowLegacy = "MouseOver CurrentRow";
        private const string DATAGRIDROWHEADER_stateMouseOverEditingRow = "MouseOverUnfocusedEditingRow";
        private const string DATAGRIDROWHEADER_stateMouseOverEditingRowLegacy = "MouseOver Unfocused EditingRow";
        private const string DATAGRIDROWHEADER_stateMouseOverEditingRowFocused = "MouseOverEditingRow";
        private const string DATAGRIDROWHEADER_stateMouseOverEditingRowFocusedLegacy = "MouseOver EditingRow";
        private const string DATAGRIDROWHEADER_stateMouseOverSelected = "MouseOverUnfocusedSelected";
        private const string DATAGRIDROWHEADER_stateMouseOverSelectedLegacy = "MouseOver Unfocused Selected";
        private const string DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRow = "MouseOverUnfocusedCurrentRowSelected";
        private const string DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRowLegacy = "MouseOver Unfocused CurrentRow Selected";
        private const string DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRowFocused = "MouseOverCurrentRowSelected";
        private const string DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRowFocusedLegacy = "MouseOver CurrentRow Selected";
        private const string DATAGRIDROWHEADER_stateMouseOverSelectedFocused = "MouseOverSelected";
        private const string DATAGRIDROWHEADER_stateMouseOverSelectedFocusedLegacy = "MouseOver Selected";
        private const string DATAGRIDROWHEADER_stateNormal = "Normal";
        private const string DATAGRIDROWHEADER_stateNormalCurrentRow = "NormalCurrentRow";
        private const string DATAGRIDROWHEADER_stateNormalCurrentRowLegacy = "Normal CurrentRow";
        private const string DATAGRIDROWHEADER_stateNormalEditingRow = "UnfocusedEditingRow";
        private const string DATAGRIDROWHEADER_stateNormalEditingRowLegacy = "Unfocused EditingRow";
        private const string DATAGRIDROWHEADER_stateNormalEditingRowFocusedLegacy = "NormalEditingRow";
        private const string DATAGRIDROWHEADER_stateNormalEditingRowFocused = "Normal EditingRow";
        private const string DATAGRIDROWHEADER_stateSelected = "UnfocusedSelected";
        private const string DATAGRIDROWHEADER_stateSelectedLegacy = "Unfocused Selected";
        private const string DATAGRIDROWHEADER_stateSelectedCurrentRow = "UnfocusedCurrentRowSelected";
        private const string DATAGRIDROWHEADER_stateSelectedCurrentRowLegacy = "Unfocused CurrentRow Selected";
        private const string DATAGRIDROWHEADER_stateSelectedCurrentRowFocused = "NormalCurrentRowSelected";
        private const string DATAGRIDROWHEADER_stateSelectedCurrentRowFocusedLegacy = "Normal CurrentRow Selected";
        private const string DATAGRIDROWHEADER_stateSelectedFocused = "NormalSelected";
        private const string DATAGRIDROWHEADER_stateSelectedFocusedLegacy = "Normal Selected";

        private const byte DATAGRIDROWHEADER_stateMouseOverCode = 0;
        private const byte DATAGRIDROWHEADER_stateMouseOverCurrentRowCode = 1;
        private const byte DATAGRIDROWHEADER_stateMouseOverEditingRowCode = 2;
        private const byte DATAGRIDROWHEADER_stateMouseOverEditingRowFocusedCode = 3;
        private const byte DATAGRIDROWHEADER_stateMouseOverSelectedCode = 4;
        private const byte DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRowCode = 5;
        private const byte DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRowFocusedCode = 6;
        private const byte DATAGRIDROWHEADER_stateMouseOverSelectedFocusedCode = 7;
        private const byte DATAGRIDROWHEADER_stateNormalCode = 8;
        private const byte DATAGRIDROWHEADER_stateNormalCurrentRowCode = 9;
        private const byte DATAGRIDROWHEADER_stateNormalEditingRowCode = 10;
        private const byte DATAGRIDROWHEADER_stateNormalEditingRowFocusedCode = 11;
        private const byte DATAGRIDROWHEADER_stateSelectedCode = 12;
        private const byte DATAGRIDROWHEADER_stateSelectedCurrentRowCode = 13;
        private const byte DATAGRIDROWHEADER_stateSelectedCurrentRowFocusedCode = 14;
        private const byte DATAGRIDROWHEADER_stateSelectedFocusedCode = 15;
        private const byte DATAGRIDROWHEADER_stateNullCode = 255;

        private static byte[] _fallbackStateMapping = new byte[]
        {
            DATAGRIDROWHEADER_stateNormalCode,
            DATAGRIDROWHEADER_stateNormalCurrentRowCode,
            DATAGRIDROWHEADER_stateMouseOverEditingRowFocusedCode,
            DATAGRIDROWHEADER_stateNormalEditingRowFocusedCode,
            DATAGRIDROWHEADER_stateMouseOverSelectedFocusedCode,
            DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRowFocusedCode,
            DATAGRIDROWHEADER_stateSelectedFocusedCode,
            DATAGRIDROWHEADER_stateSelectedFocusedCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateNormalCode,
            DATAGRIDROWHEADER_stateNormalEditingRowFocusedCode,
            DATAGRIDROWHEADER_stateSelectedCurrentRowFocusedCode,
            DATAGRIDROWHEADER_stateSelectedFocusedCode,
            DATAGRIDROWHEADER_stateSelectedCurrentRowFocusedCode,
            DATAGRIDROWHEADER_stateNormalCurrentRowCode,
            DATAGRIDROWHEADER_stateNormalCode,
        };

        private static byte[] _idealStateMapping = new byte[]
        {
            DATAGRIDROWHEADER_stateNormalCode,
            DATAGRIDROWHEADER_stateNormalCode,
            DATAGRIDROWHEADER_stateMouseOverCode,
            DATAGRIDROWHEADER_stateMouseOverCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateSelectedCode,
            DATAGRIDROWHEADER_stateSelectedFocusedCode,
            DATAGRIDROWHEADER_stateMouseOverSelectedCode,
            DATAGRIDROWHEADER_stateMouseOverSelectedFocusedCode,
            DATAGRIDROWHEADER_stateNormalEditingRowCode,
            DATAGRIDROWHEADER_stateNormalEditingRowFocusedCode,
            DATAGRIDROWHEADER_stateMouseOverEditingRowCode,
            DATAGRIDROWHEADER_stateMouseOverEditingRowFocusedCode,
            DATAGRIDROWHEADER_stateNormalCurrentRowCode,
            DATAGRIDROWHEADER_stateNormalCurrentRowCode,
            DATAGRIDROWHEADER_stateMouseOverCurrentRowCode,
            DATAGRIDROWHEADER_stateMouseOverCurrentRowCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateSelectedCurrentRowCode,
            DATAGRIDROWHEADER_stateSelectedCurrentRowFocusedCode,
            DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRowCode,
            DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRowFocusedCode,
            DATAGRIDROWHEADER_stateNormalEditingRowCode,
            DATAGRIDROWHEADER_stateNormalEditingRowFocusedCode,
            DATAGRIDROWHEADER_stateMouseOverEditingRowCode,
            DATAGRIDROWHEADER_stateMouseOverEditingRowFocusedCode
        };

        // TODO - Delete use of legacy state names
        // In SL 2, our state names had spaces.  Going forward, we are removing the spaces but still supporting the legacy state names
        private static string[] _legacyStateNames = new string[]
        {
            DATAGRIDROWHEADER_stateMouseOver,
            DATAGRIDROWHEADER_stateMouseOverCurrentRowLegacy,
            DATAGRIDROWHEADER_stateMouseOverEditingRowLegacy,
            DATAGRIDROWHEADER_stateMouseOverEditingRowFocusedLegacy,
            DATAGRIDROWHEADER_stateMouseOverSelectedLegacy,
            DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRowLegacy,
            DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRowFocusedLegacy,
            DATAGRIDROWHEADER_stateMouseOverSelectedFocusedLegacy,
            DATAGRIDROWHEADER_stateNormal,
            DATAGRIDROWHEADER_stateNormalCurrentRowLegacy,
            DATAGRIDROWHEADER_stateNormalEditingRowLegacy,
            DATAGRIDROWHEADER_stateNormalEditingRowFocusedLegacy,
            DATAGRIDROWHEADER_stateSelectedLegacy,
            DATAGRIDROWHEADER_stateSelectedCurrentRowLegacy,
            DATAGRIDROWHEADER_stateSelectedCurrentRowFocusedLegacy,
            DATAGRIDROWHEADER_stateSelectedFocusedLegacy
        };

        private static string[] _stateNames = new string[]
        {
            DATAGRIDROWHEADER_stateMouseOver,
            DATAGRIDROWHEADER_stateMouseOverCurrentRow,
            DATAGRIDROWHEADER_stateMouseOverEditingRow,
            DATAGRIDROWHEADER_stateMouseOverEditingRowFocused,
            DATAGRIDROWHEADER_stateMouseOverSelected,
            DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRow,
            DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRowFocused,
            DATAGRIDROWHEADER_stateMouseOverSelectedFocused,
            DATAGRIDROWHEADER_stateNormal,
            DATAGRIDROWHEADER_stateNormalCurrentRow,
            DATAGRIDROWHEADER_stateNormalEditingRow,
            DATAGRIDROWHEADER_stateNormalEditingRowFocused,
            DATAGRIDROWHEADER_stateSelected,
            DATAGRIDROWHEADER_stateSelectedCurrentRow,
            DATAGRIDROWHEADER_stateSelectedCurrentRowFocused,
            DATAGRIDROWHEADER_stateSelectedFocused
        };

        private FrameworkElement _rootElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridRowHeader"/> class.
        /// </summary>
        public DataGridRowHeader()
        {
#if WINDOWS_UWP
            this.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(DataGridRowHeader_PointerPressed), true /*handledEventsToo*/);
#else
            this.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(DataGridRowHeader_MouseLeftButtonDown), true /*handledEventsToo*/);
            this.MouseEnter += new MouseEventHandler(DataGridRowHeader_MouseEnter);
            this.MouseLeave += new MouseEventHandler(DataGridRowHeader_MouseLeave);
#endif

            DefaultStyleKey = typeof(DataGridRowHeader);
        }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.Media.Brush"/> used to paint the row header separator lines.
        /// </summary>
        public Brush SeparatorBrush
        {
            get { return GetValue(SeparatorBrushProperty) as Brush; }
            set { SetValue(SeparatorBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridRowHeader.SeparatorBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SeparatorBrushProperty =
            DependencyProperty.Register(
                "SeparatorBrush",
                typeof(Brush),
                typeof(DataGridRowHeader),
                null);

        /// <summary>
        /// Gets or sets a value indicating whether the row header separator lines are visible.
        /// </summary>
        public Visibility SeparatorVisibility
        {
            get { return (Visibility)GetValue(SeparatorVisibilityProperty); }
            set { SetValue(SeparatorVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridRowHeader.SeparatorVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SeparatorVisibilityProperty =
            DependencyProperty.Register(
                "SeparatorVisibility",
                typeof(Visibility),
                typeof(DataGridRowHeader),
                new PropertyMetadata(Visibility.Visible));

        private DataGrid OwningGrid
        {
            get
            {
                if (this.OwningRow != null)
                {
                    return this.OwningRow.OwningGrid;
                }
                else if (this.OwningRowGroupHeader != null)
                {
                    return this.OwningRowGroupHeader.OwningGrid;
                }

                return null;
            }
        }

        private DataGridRow OwningRow
        {
            get
            {
                return this.Owner as DataGridRow;
            }
        }

        private DataGridRowGroupHeader OwningRowGroupHeader
        {
            get
            {
                return this.Owner as DataGridRowGroupHeader;
            }
        }

        internal Control Owner
        {
            get;
            set;
        }

        private int Slot
        {
            get
            {
                if (this.OwningRow != null)
                {
                    return this.OwningRow.Slot;
                }
                else if (this.OwningRowGroupHeader != null)
                {
                    return this.OwningRowGroupHeader.RowGroupInfo.Slot;
                }

                return -1;
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

            this._rootElement = GetTemplateChild(DATAGRIDROWHEADER_elementRootName) as FrameworkElement;
            if (this._rootElement != null)
            {
                ApplyOwnerStatus(false /*animate*/);
            }
        }

        /// <summary>
        /// Measures the children of a <see cref="T:System.Windows.Controls.Primitives.DataGridRowHeader"/> to prepare for arranging them during the <see cref="M:System.Windows.FrameworkElement.ArrangeOverride(System.Windows.Size)"/> pass.
        /// </summary>
        /// <param name="availableSize">
        /// The available size that this element can give to child elements. Indicates an upper limit that child elements should not exceed.
        /// </param>
        /// <returns>
        /// The size that the <see cref="T:System.Windows.Controls.Primitives.DataGridRowHeader"/> determines it needs during layout, based on its calculations of child object allocated sizes.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.OwningRow == null || this.OwningGrid == null)
            {
                return base.MeasureOverride(availableSize);
            }

            double measureHeight = double.IsNaN(this.OwningGrid.RowHeight) ? availableSize.Height : this.OwningGrid.RowHeight;
            double measureWidth = double.IsNaN(this.OwningGrid.RowHeaderWidth) ? availableSize.Width : this.OwningGrid.RowHeaderWidth;
            Size measuredSize = base.MeasureOverride(new Size(measureWidth, measureHeight));

            // Auto grow the row header or force it to a fixed width based on the DataGrid's setting
            if (!double.IsNaN(this.OwningGrid.RowHeaderWidth) || measuredSize.Width < this.OwningGrid.ActualRowHeaderWidth)
            {
                return new Size(this.OwningGrid.ActualRowHeaderWidth, measuredSize.Height);
            }

            return measuredSize;
        }

        /// <summary>
        /// Creates AutomationPeer (<see cref="UIElement.OnCreateAutomationPeer"/>)
        /// </summary>
        /// <returns>An automation peer for this <see cref="T:System.Windows.Controls.Primitives.DataGridRowHeader"/>.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            // TODO - return new DataGridRowHeaderAutomationPeer(this);
            return null;
        }

        internal void ApplyOwnerStatus(bool animate)
        {
            if (this._rootElement != null && this.Owner != null && this.Owner.Visibility == Visibility.Visible)
            {
                byte idealStateMappingIndex = 0;

                if (this.OwningRow != null)
                {
                    if (this.OwningRow.IsValid)
                    {
                        VisualStates.GoToState(this, true, VisualStates.StateRowValid);
                    }
                    else
                    {
                        VisualStates.GoToState(this, true, VisualStates.StateRowInvalid, VisualStates.StateRowValid);
                    }

                    if (this.OwningGrid != null)
                    {
                        if (this.OwningGrid.CurrentSlot == this.OwningRow.Slot)
                        {
                            idealStateMappingIndex += 16;
                        }

                        if (this.OwningGrid.ContainsFocus)
                        {
                            idealStateMappingIndex += 1;
                        }
                    }

                    if (this.OwningRow.IsSelected || this.OwningRow.IsEditing)
                    {
                        idealStateMappingIndex += 8;
                    }

                    if (this.OwningRow.IsEditing)
                    {
                        idealStateMappingIndex += 4;
                    }

                    if (this.OwningRow.IsMouseOver)
                    {
                        idealStateMappingIndex += 2;
                    }
                }
                else if (this.OwningRowGroupHeader != null && this.OwningGrid != null && this.OwningGrid.CurrentSlot == this.OwningRowGroupHeader.RowGroupInfo.Slot)
                {
                    idealStateMappingIndex += 16;
                }

                byte stateCode = _idealStateMapping[idealStateMappingIndex];
                Debug.Assert(stateCode != DATAGRIDROWHEADER_stateNullCode, "Expected stateCode other than DATAGRIDROWHEADER_stateNullCode.");

                string storyboardName;
                while (stateCode != DATAGRIDROWHEADER_stateNullCode)
                {
                    storyboardName = _stateNames[stateCode];
                    if (VisualStateManager.GoToState(this, storyboardName, animate) || VisualStateManager.GoToState(this, _legacyStateNames[stateCode], animate))
                    {
                        break;
                    }
                    else
                    {
                        // The state wasn't implemented so fall back to the next one
                        stateCode = _fallbackStateMapping[stateCode];
                    }
                }
            }
        }

        /// <summary>
        /// Ensures that the correct Style is applied to this object.
        /// </summary>
        /// <param name="previousStyle">Caller's previous associated Style</param>
        internal void EnsureStyle(Style previousStyle)
        {
            if (this.Style != null
                && (this.OwningRow != null && this.Style != this.OwningRow.HeaderStyle)
                && (this.OwningRowGroupHeader != null && this.Style != this.OwningRowGroupHeader.HeaderStyle)
                && (this.OwningGrid != null && this.Style != this.OwningGrid.RowHeaderStyle)
                && (this.Style != previousStyle))
            {
                return;
            }

            Style style = null;
            if (this.OwningRow != null)
            {
                style = this.OwningRow.HeaderStyle;
            }

            if (style == null && this.OwningGrid != null)
            {
                style = this.OwningGrid.RowHeaderStyle;
            }

            this.SetStyleWithType(style);
        }

#if !WINDOWS_UWP
        private void DataGridRowHeader_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.OwningRow != null)
            {
                this.OwningRow.IsMouseOver = true;
            }
        }

        private void DataGridRowHeader_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.OwningRow != null)
            {
                this.OwningRow.IsMouseOver = false;
            }
        }
#endif

#if WINDOWS_UWP
        private void DataGridRowHeader_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // TODO - Should Touch/Pen be supported too?
            if (this.OwningGrid != null && e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
#else
        private void DataGridRowHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.OwningGrid != null)
#endif
            {
                if (!e.Handled && this.OwningGrid.IsTabStop)
                {
#if WINDOWS_UWP
                    bool success = this.OwningGrid.Focus(FocusState.Programmatic);
#else
                    bool success = this.OwningGrid.Focus();
#endif
                    Debug.Assert(success, "Expected successful focus change.");
                }

                if (this.OwningRow != null)
                {
                    Debug.Assert(sender is DataGridRowHeader, "Expected sender is DataGridRowHeader.");
                    Debug.Assert(sender == this, "Expected sender is this.");

                    e.Handled = this.OwningGrid.UpdateStateOnMouseLeftButtonDown(e, -1, this.Slot, false);
                    this.OwningGrid.UpdatedStateOnMouseLeftButtonDown = true;
                }
            }
        }
    }
}