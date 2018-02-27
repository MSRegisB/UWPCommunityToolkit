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
using System.Globalization;
using Microsoft.Toolkit.Uwp.UI.Controls.DataGridInternals;
using Microsoft.Toolkit.Uwp.UI.Controls.Primitives;
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
#endif

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    /// <summary>
    /// Represents the header of a <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.DataGrid"/> row group.
    /// </summary>
    [TemplatePart(Name = DataGridRow.DATAGRIDROW_elementRoot, Type = typeof(Panel))]
    [TemplatePart(Name = DataGridRow.DATAGRIDROW_elementRowHeader, Type = typeof(Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridRowHeader))]
    [TemplatePart(Name = DATAGRIDROWGROUPHEADER_expanderButton, Type = typeof(ToggleButton))]
    [TemplatePart(Name = DATAGRIDROWGROUPHEADER_indentSpacer, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = DATAGRIDROWGROUPHEADER_itemCountElement, Type = typeof(TextBlock))]
    [TemplatePart(Name = DATAGRIDROWGROUPHEADER_propertyNameElement, Type = typeof(TextBlock))]
    [StyleTypedProperty(Property = "HeaderStyle", StyleTargetType = typeof(Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridRowHeader))]
    public class DataGridRowGroupHeader : Control
    {
        private const string DATAGRIDROWGROUPHEADER_expanderButton = "ExpanderButton";
        private const string DATAGRIDROWGROUPHEADER_indentSpacer = "IndentSpacer";
        private const string DATAGRIDROWGROUPHEADER_itemCountElement = "ItemCountElement";
        private const string DATAGRIDROWGROUPHEADER_propertyNameElement = "PropertyNameElement";

        private bool _areIsCheckedHandlersSuspended;
        private ToggleButton _expanderButton;
        private Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridRowHeader _headerElement;
        private FrameworkElement _indentSpacer;
        private TextBlock _itemCountElement;
        private TextBlock _propertyNameElement;
        private Panel _rootElement;
        private double _totalIndent;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.DataGridRowGroupHeader"/> class.
        /// </summary>
        public DataGridRowGroupHeader()
        {
            DefaultStyleKey = typeof(DataGridRowGroupHeader);

#if WINDOWS_UWP
            // TODO - listen to mouse button down.
#else
            this.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(DataGridRowGroupHeader_MouseLeftButtonDown), true /*handledEventsToo*/);
#endif
        }

        /// <summary>
        /// Gets or sets the style applied to the header cell of a <see cref="T:System.Windows.Controls.DataGridRowGroupHeader"/>.
        /// </summary>
        public Style HeaderStyle
        {
            get { return GetValue(HeaderStyleProperty) as Style; }
            set { SetValue(HeaderStyleProperty, value); }
        }

        /// <summary>
        /// Dependency Property for HeaderStyle
        /// </summary>
        public static readonly DependencyProperty HeaderStyleProperty =
            DependencyProperty.Register(
                "HeaderStyle",
                typeof(Style),
                typeof(DataGridRowGroupHeader),
                new PropertyMetadata(null, OnHeaderStylePropertyChanged));

        private static void OnHeaderStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGridRowGroupHeader groupHeader = d as DataGridRowGroupHeader;
            if (groupHeader._headerElement != null)
            {
                groupHeader._headerElement.EnsureStyle(e.OldValue as Style);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the item count is visible.
        /// </summary>
        public Visibility ItemCountVisibility
        {
            get { return (Visibility)GetValue(ItemCountVisibilityProperty); }
            set { SetValue(ItemCountVisibilityProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for ItemCountVisibility
        /// </summary>
        public static readonly DependencyProperty ItemCountVisibilityProperty =
            DependencyProperty.Register(
                "ItemCountVisibility",
                typeof(Visibility),
                typeof(DataGridRowGroupHeader),
                new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Gets or sets the name of the property that this <see cref="T:System.Windows.Controls.DataGrid"/> row is bound to.
        /// </summary>
        public string PropertyName
        {
            get { return GetValue(PropertyNameProperty) as string; }
            set { SetValue(PropertyNameProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for PropertyName
        /// </summary>
        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register(
                "PropertyName",
                typeof(string),
                typeof(DataGridRowGroupHeader),
                null);

        /// <summary>
        /// Gets or sets a value that indicates whether the property name is visible.
        /// </summary>
        public Visibility PropertyNameVisibility
        {
            get { return (Visibility)GetValue(PropertyNameVisibilityProperty); }
            set { SetValue(PropertyNameVisibilityProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for PropertyNameVisibility
        /// </summary>
        public static readonly DependencyProperty PropertyNameVisibilityProperty =
            DependencyProperty.Register(
                "PropertyNameVisibility",
                typeof(Visibility),
                typeof(DataGridRowGroupHeader),
                new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Gets or sets a value that indicates the amount that the
        /// children of the <see cref="T:System.Windows.Controls.RowGroupHeader"/> are indented.
        /// </summary>
        public double SublevelIndent
        {
            get { return (double)GetValue(SublevelIndentProperty); }
            set { SetValue(SublevelIndentProperty, value); }
        }

        /// <summary>
        /// SublevelIndent Dependency property
        /// </summary>
        public static readonly DependencyProperty SublevelIndentProperty =
            DependencyProperty.Register(
                "SublevelIndent",
                typeof(double),
                typeof(DataGridRowGroupHeader),
                new PropertyMetadata(DataGrid.DATAGRID_defaultRowGroupSublevelIndent, OnSublevelIndentPropertyChanged));

        private static void OnSublevelIndentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGridRowGroupHeader groupHeader = d as DataGridRowGroupHeader;
            double newValue = (double)e.NewValue;

            // We don't need to revert to the old value if our input is bad because we never read this property value
            if (double.IsNaN(newValue))
            {
                throw DataGridError.DataGrid.ValueCannotBeSetToNAN("SublevelIndent");
            }
            else if (double.IsInfinity(newValue))
            {
                throw DataGridError.DataGrid.ValueCannotBeSetToInfinity("SublevelIndent");
            }
            else if (newValue < 0)
            {
                throw DataGridError.DataGrid.ValueMustBeGreaterThanOrEqualTo("value", "SublevelIndent", 0);
            }

            if (groupHeader.OwningGrid != null)
            {
                groupHeader.OwningGrid.OnSublevelIndentUpdated(groupHeader, newValue);
            }
        }

        internal Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridRowHeader HeaderCell
        {
            get
            {
                return _headerElement;
            }
        }

        private bool IsCurrent
        {
            get
            {
                Debug.Assert(this.OwningGrid != null, "Expected non-null OwningGrid.");
                return this.RowGroupInfo.Slot == this.OwningGrid.CurrentSlot;
            }
        }

#if WINDOWS_UWP
        private bool IsMouseOver
        {
            get;
            set;
        }
#endif

        internal bool IsRecycled
        {
            get;
            set;
        }

        internal int Level
        {
            get;
            set;
        }

        internal DataGrid OwningGrid
        {
            get;
            set;
        }

        internal DataGridRowGroupInfo RowGroupInfo
        {
            get;
            set;
        }

        internal double TotalIndent
        {
            set
            {
                _totalIndent = value;
                if (_indentSpacer != null)
                {
                    _indentSpacer.Width = _totalIndent;
                }
            }
        }

        internal void ApplyHeaderStatus(bool animate)
        {
            if (_headerElement != null && this.OwningGrid.AreRowHeadersVisible)
            {
                _headerElement.ApplyOwnerStatus(animate);
            }
        }

        internal void ApplyState(bool useTransitions)
        {
            // Common States
            if (this.IsMouseOver)
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateMouseOver, VisualStates.StateNormal);
            }
            else
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateNormal);
            }

            // Current States
            if (this.IsCurrent)
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateCurrent, VisualStates.StateRegular);
            }
            else
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateRegular);
            }

#if FEATURE_COLLECTIONVIEWGROUP
            // Expanded States
            if (this.RowGroupInfo.CollectionViewGroup.ItemCount == 0)
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateEmpty);
            }
            else
            {
                if (this.RowGroupInfo.Visibility == Visibility.Visible)
                {
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateExpanded, VisualStates.StateEmpty);
                }
                else
                {
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateCollapsed, VisualStates.StateEmpty);
                }
            }
#endif
        }

        /// <summary>
        /// ArrangeOverride
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this object should use to arrange itself and its children.</param>
        /// <returns>The actual size that is used after the element is arranged in layout.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this.OwningGrid == null)
            {
                return base.ArrangeOverride(finalSize);
            }

            Size size = base.ArrangeOverride(finalSize);
            if (_rootElement != null)
            {
                if (this.OwningGrid.AreRowGroupHeadersFrozen)
                {
                    foreach (UIElement child in _rootElement.Children)
                    {
                        child.Clip = null;
                    }
                }
                else
                {
                    double frozenLeftEdge = 0;
                    foreach (UIElement child in _rootElement.Children)
                    {
                        if (DataGridFrozenGrid.GetIsFrozen(child) && child.Visibility == Visibility.Visible)
                        {
                            TranslateTransform transform = new TranslateTransform();

                            // Automatic layout rounding doesn't apply to transforms so we need to Round this
                            transform.X = Math.Round(this.OwningGrid.HorizontalOffset);
                            child.RenderTransform = transform;

                            double childLeftEdge = child.Translate(this, new Point(child.RenderSize.Width, 0)).X - transform.X;
                            frozenLeftEdge = Math.Max(frozenLeftEdge, childLeftEdge + this.OwningGrid.HorizontalOffset);
                        }
                    }

                    // Clip the non-frozen elements so they don't overlap the frozen ones
                    foreach (UIElement child in _rootElement.Children)
                    {
                        if (!DataGridFrozenGrid.GetIsFrozen(child))
                        {
                            EnsureChildClip(child, frozenLeftEdge);
                        }
                    }
                }
            }

            return size;
        }

        internal void ClearFrozenStates()
        {
            if (_rootElement != null)
            {
                foreach (UIElement child in _rootElement.Children)
                {
                    child.RenderTransform = null;
                }
            }
        }

#if !WINDOWS_UWP
        private void DataGridRowGroupHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.OwningGrid != null)
            {
                if (this.OwningGrid.IsDoubleClickRecordsClickOnCall(this) && !e.Handled)
                {
                    ToggleExpandCollapse(this.RowGroupInfo.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible, true);
                    e.Handled = true;
                }
                else
                {
                    if (!e.Handled && this.OwningGrid.IsTabStop)
                    {
                        bool success = this.OwningGrid.Focus();
                        Debug.Assert(success);
                    }
                    e.Handled = this.OwningGrid.UpdateStateOnMouseLeftButtonDown(e, this.OwningGrid.CurrentColumnIndex, this.RowGroupInfo.Slot, false /*allowEdit*/);
                }
            }
        }
#endif

        private void EnsureChildClip(UIElement child, double frozenLeftEdge)
        {
            double childLeftEdge = child.Translate(this, new Point(0, 0)).X;
            if (frozenLeftEdge > childLeftEdge)
            {
                double xClip = Math.Round(frozenLeftEdge - childLeftEdge);
                RectangleGeometry rg = new RectangleGeometry();
                rg.Rect = new Rect(xClip, 0, Math.Max(0, child.RenderSize.Width - xClip), child.RenderSize.Height);
                child.Clip = rg;
            }
            else
            {
                child.Clip = null;
            }
        }

        internal void EnsureExpanderButtonIsChecked()
        {
#if FEATURE_COLLECTIONVIEWGROUP
            if (_expanderButton != null && this.RowGroupInfo != null && this.RowGroupInfo.CollectionViewGroup != null &&
                this.RowGroupInfo.CollectionViewGroup.ItemCount != 0)
            {
                SetIsCheckedNoCallBack(this.RowGroupInfo.Visibility == Visibility.Visible);
            }
#endif
        }

        internal void EnsureHeaderStyleAndVisibility(Style previousStyle)
        {
            if (this._headerElement != null && this.OwningGrid != null)
            {
                if (this.OwningGrid.AreRowHeadersVisible)
                {
                    this._headerElement.EnsureStyle(previousStyle);
                    this._headerElement.Visibility = Visibility.Visible;
                }
                else
                {
                    this._headerElement.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ExpanderButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!_areIsCheckedHandlersSuspended)
            {
                ToggleExpandCollapse(Visibility.Visible, true);
            }
        }

        private void ExpanderButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_areIsCheckedHandlersSuspended)
            {
                ToggleExpandCollapse(Visibility.Collapsed, true);
            }
        }

        internal void LoadVisualsForDisplay()
        {
            EnsureExpanderButtonIsChecked();

            EnsureHeaderStyleAndVisibility(null);
            ApplyState(false /*useTransitions*/);
            ApplyHeaderStatus(false);
        }

        /// <summary>
        /// Builds the visual tree for the row group header when a new template is applied.
        /// </summary>
#if WINDOWS_UWP
        protected
#else
        public
#endif
        override void OnApplyTemplate()
        {
            _rootElement = GetTemplateChild(DataGridRow.DATAGRIDROW_elementRoot) as Panel;

            if (_expanderButton != null)
            {
                _expanderButton.Checked -= ExpanderButton_Checked;
                _expanderButton.Unchecked -= ExpanderButton_Unchecked;
            }

            _expanderButton = GetTemplateChild(DATAGRIDROWGROUPHEADER_expanderButton) as ToggleButton;
            if (_expanderButton != null)
            {
                EnsureExpanderButtonIsChecked();
                _expanderButton.Checked += new RoutedEventHandler(ExpanderButton_Checked);
                _expanderButton.Unchecked += new RoutedEventHandler(ExpanderButton_Unchecked);
            }

            _headerElement = GetTemplateChild(DataGridRow.DATAGRIDROW_elementRowHeader) as Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridRowHeader;
            if (_headerElement != null)
            {
                _headerElement.Owner = this;
                EnsureHeaderStyleAndVisibility(null);
            }

            _indentSpacer = GetTemplateChild(DATAGRIDROWGROUPHEADER_indentSpacer) as FrameworkElement;
            if (_indentSpacer != null)
            {
                _indentSpacer.Width = _totalIndent;
            }

            _itemCountElement = GetTemplateChild(DATAGRIDROWGROUPHEADER_itemCountElement) as TextBlock;
            _propertyNameElement = GetTemplateChild(DATAGRIDROWGROUPHEADER_propertyNameElement) as TextBlock;
            UpdateTitleElements();
        }

        /// <summary>
        /// Creates AutomationPeer (<see cref="UIElement.OnCreateAutomationPeer"/>)
        /// </summary>
        /// <returns>An automation peer for this <see cref="T:System.Windows.Controls.Primitives.DataGridRowGroupHeader"/>.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            // TODO - return new DataGridRowGroupHeaderAutomationPeer(this);
            return null;
        }

#if !WINDOWS_UWP
        /// <summary>
        /// OnMouseEnter
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            // TODO - removing line correct?
            // this.IsMouseOver = true;
            ApplyState(true /*useTransitions*/);
        }

        /// <summary>
        /// OnMouseLeave
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            // TODO - removing line correct?
            // this.IsMouseOver = false;
            ApplyState(true /*useTransitions*/);
        }
#endif

        private void SetIsCheckedNoCallBack(bool value)
        {
            if (_expanderButton != null && _expanderButton.IsChecked != value)
            {
                _areIsCheckedHandlersSuspended = true;
                try
                {
                    _expanderButton.IsChecked = value;
                }
                finally
                {
                    _areIsCheckedHandlersSuspended = false;
                }
            }
        }

        internal void ToggleExpandCollapse(Visibility newVisibility, bool setCurrent)
        {
#if FEATURE_COLLECTIONVIEWGROUP
            if (this.RowGroupInfo.CollectionViewGroup.ItemCount != 0)
            {
                if (this.OwningGrid == null)
                {
                    // Do these even if the OwningGrid is null in case it could improve the Designer experience for a standalone DataGridRowGroupHeader
                    this.RowGroupInfo.Visibility = newVisibility;
                }
                else
                {
                    this.OwningGrid.OnRowGroupHeaderToggled(this, newVisibility, setCurrent);
                }

                EnsureExpanderButtonIsChecked();
                ApplyState(true /*useTransitions*/);
            }
#endif
        }

        internal void UpdateTitleElements()
        {
            if (_propertyNameElement != null)
            {
                _propertyNameElement.Text = string.Format(CultureInfo.CurrentCulture, "{0}:", this.PropertyName);
            }

#if FEATURE_COLLECTIONVIEWGROUP
            if (_itemCountElement != null && this.RowGroupInfo != null && this.RowGroupInfo.CollectionViewGroup != null)
            {
                _itemCountElement.Text = string.Format(
                    CultureInfo.CurrentCulture,
                    this.RowGroupInfo.CollectionViewGroup.ItemCount == 1 ? "({0} item)" : "({0} items)",
                    this.RowGroupInfo.CollectionViewGroup.ItemCount);
            }
#endif
        }
    }
}
