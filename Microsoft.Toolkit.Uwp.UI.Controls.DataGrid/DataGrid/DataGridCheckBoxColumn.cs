﻿// ******************************************************************
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
using System.Collections.Specialized;
using Microsoft.Toolkit.Uwp.UI.Controls.DataGridInternals;
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
#else
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
#endif

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    /// <summary>
    /// Represents a <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.DataGrid"/> column that hosts
    /// <see cref="T:System.Windows.Controls.CheckBox"/> controls in its cells.
    /// </summary>
    /// <QualityBand>Mature</QualityBand>
    [StyleTypedProperty(Property = "ElementStyle", StyleTargetType = typeof(CheckBox))]
    [StyleTypedProperty(Property = "EditingElementStyle", StyleTargetType = typeof(CheckBox))]
    public class DataGridCheckBoxColumn : DataGridBoundColumn
    {
        private const string DATAGRIDCHECKBOXCOLUMN_isThreeStateName = "IsThreeState";

        private bool _beganEditWithKeyboard;
        private bool _isThreeState; // TODO: remove once dependency properties are implemented.
        private CheckBox _currentCheckBox;
        private DataGrid _owningGrid;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.DataGridCheckBoxColumn"/> class.
        /// </summary>
        public DataGridCheckBoxColumn()
        {
            this.BindingTarget = CheckBox.IsCheckedProperty;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the hosted <see cref="T:System.Windows.Controls.CheckBox"/> controls allow three states or two.
        /// </summary>
        /// <returns>
        /// true if the hosted controls support three states; false if they support two states. The default is false.
        /// </returns>
        public bool IsThreeState
        {
            get
            {
                return this._isThreeState;
            }

            set
            {
                if (this._isThreeState != value)
                {
                    this._isThreeState = value;
                    NotifyPropertyChanged(DATAGRIDCHECKBOXCOLUMN_isThreeStateName);
                }
            }
        }

        /// <summary>
        /// Causes the column cell being edited to revert to the specified value.
        /// </summary>
        /// <param name="editingElement">
        /// The element that the column displays for a cell in editing mode.
        /// </param>
        /// <param name="uneditedValue">
        /// The previous, unedited value in the cell being edited.
        /// </param>
        protected override void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
        {
            CheckBox editingCheckBox = editingElement as CheckBox;
            if (editingCheckBox != null)
            {
                editingCheckBox.IsChecked = (bool?)uneditedValue;
            }
        }

        /// <summary>
        /// Gets a <see cref="T:System.Windows.Controls.CheckBox"/> control that is bound to the column's <see cref="P:Microsoft.Toolkit.Uwp.UI.Controls.DataGridBoundColumn.Binding"/> property value.
        /// </summary>
        /// <param name="cell">
        /// The cell that will contain the generated element.
        /// </param>
        /// <param name="dataItem">
        /// The data item represented by the row that contains the intended cell.
        /// </param>
        /// <returns>
        /// A new <see cref="T:System.Windows.Controls.CheckBox"/> control that is bound to the column's <see cref="P:Microsoft.Toolkit.Uwp.UI.Controls.DataGridBoundColumn.Binding"/> property value.
        /// </returns>
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            CheckBox checkBox = new CheckBox();

            // SSS_DROP_BEGIN
            // TODO: We might set Margin in a style instead, but Jolt Bugs 14282 prevents that from working
            // SSS_DROP_END
            checkBox.Margin = new Thickness(0);
            ConfigureCheckBox(checkBox);
            return checkBox;
        }

        /// <summary>
        /// Gets a read-only <see cref="T:System.Windows.Controls.CheckBox"/> control that is bound to the column's <see cref="P:Microsoft.Toolkit.Uwp.UI.Controls.DataGridBoundColumn.Binding"/> property value.
        /// </summary>
        /// <param name="cell">
        /// The cell that will contain the generated element.
        /// </param>
        /// <param name="dataItem">
        /// The data item represented by the row that contains the intended cell.
        /// </param>
        /// <returns>
        /// A new, read-only <see cref="T:System.Windows.Controls.CheckBox"/> control that is bound to the column's <see cref="P:Microsoft.Toolkit.Uwp.UI.Controls.DataGridBoundColumn.Binding"/> property value.
        /// </returns>
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            bool isEnabled = false;
            CheckBox checkBoxElement = new CheckBox();
            if (EnsureOwningGrid())
            {
                if (cell.RowIndex != -1 && cell.ColumnIndex != -1 &&
                    cell.OwningRow != null &&
                    cell.OwningRow.Slot == this.OwningGrid.CurrentSlot &&
                    cell.ColumnIndex == this.OwningGrid.CurrentColumnIndex)
                {
                    isEnabled = true;
                    if (_currentCheckBox != null)
                    {
                        _currentCheckBox.IsEnabled = false;
                    }

                    _currentCheckBox = checkBoxElement;
                }
            }

            checkBoxElement.IsEnabled = isEnabled;
            checkBoxElement.IsHitTestVisible = false;
            ConfigureCheckBox(checkBoxElement);
            return checkBoxElement;
        }

        /// <summary>
        /// Called when a cell in the column enters editing mode.
        /// </summary>
        /// <param name="editingElement">
        /// The element that the column displays for a cell in editing mode.
        /// </param>
        /// <param name="editingEventArgs">
        /// Information about the user gesture that is causing a cell to enter editing mode.
        /// </param>
        /// <returns>
        /// The unedited value.
        /// </returns>
        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            CheckBox editingCheckBox = editingElement as CheckBox;
            if (editingCheckBox != null)
            {
                bool? uneditedValue = editingCheckBox.IsChecked;
#if WINDOWS_UWP
                PointerRoutedEventArgs mouseButtonEventArgs = editingEventArgs as PointerRoutedEventArgs;
#else
                MouseButtonEventArgs mouseButtonEventArgs = editingEventArgs as MouseButtonEventArgs;
#endif
                bool editValue = false;
                if (mouseButtonEventArgs != null)
                {
                    // Editing was triggered by a mouse click
#if WINDOWS_UWP
                    Point position = mouseButtonEventArgs.GetCurrentPoint(editingCheckBox).Position;
#else
                    Point position = mouseButtonEventArgs.GetPosition(editingCheckBox);
#endif
                    Rect rect = new Rect(0, 0, editingCheckBox.ActualWidth, editingCheckBox.ActualHeight);
                    editValue = rect.Contains(position);
                }
                else if (_beganEditWithKeyboard)
                {
                    // Editing began by a user pressing spacebar
                    editValue = true;
                    _beganEditWithKeyboard = false;
                }

                if (editValue)
                {
                    // User clicked the checkbox itself or pressed space, let's toggle the IsChecked value
                    if (editingCheckBox.IsThreeState)
                    {
                        switch (editingCheckBox.IsChecked)
                        {
                            case false:
                                editingCheckBox.IsChecked = true;
                                break;
                            case true:
                                editingCheckBox.IsChecked = null;
                                break;
                            case null:
                                editingCheckBox.IsChecked = false;
                                break;
                        }
                    }
                    else
                    {
                        editingCheckBox.IsChecked = !editingCheckBox.IsChecked;
                    }
                }

                return uneditedValue;
            }

            return false;
        }

        /// <summary>
        /// Called by the DataGrid control when this column asks for its elements to be
        /// updated, because its CheckBoxContent or IsThreeState property changed.
        /// </summary>
        protected internal override void RefreshCellContent(FrameworkElement element, string propertyName)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            CheckBox checkBox = element as CheckBox;
            if (checkBox == null)
            {
                throw DataGridError.DataGrid.ValueIsNotAnInstanceOf("element", typeof(CheckBox));
            }

            if (propertyName == DATAGRIDCHECKBOXCOLUMN_isThreeStateName)
            {
                checkBox.IsThreeState = this.IsThreeState;
            }
            else
            {
                checkBox.IsThreeState = this.IsThreeState;
            }
        }

        private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Contains(this) && _owningGrid != null)
            {
                _owningGrid.Columns.CollectionChanged -= new NotifyCollectionChangedEventHandler(Columns_CollectionChanged);
                _owningGrid.CurrentCellChanged -= new EventHandler<EventArgs>(OwningGrid_CurrentCellChanged);
                _owningGrid.KeyDown -= new KeyEventHandler(OwningGrid_KeyDown);
                _owningGrid.LoadingRow -= new EventHandler<DataGridRowEventArgs>(OwningGrid_LoadingRow);
                _owningGrid = null;
            }
        }

        private void ConfigureCheckBox(CheckBox checkBox)
        {
            checkBox.HorizontalAlignment = HorizontalAlignment.Center;
            checkBox.VerticalAlignment = VerticalAlignment.Center;
            checkBox.IsThreeState = this.IsThreeState;
#if WINDOWS_UWP
            bool isDesignMode = Windows.ApplicationModel.DesignMode.DesignModeEnabled;
#else
            bool isDesignMode = DesignerProperties.GetIsInDesignMode(this);
#endif
            if (this.Binding != null || !isDesignMode)
            {
                checkBox.SetBinding(this.BindingTarget, this.Binding);
            }
        }

        private bool EnsureOwningGrid()
        {
            if (this.OwningGrid != null)
            {
                if (this.OwningGrid != _owningGrid)
                {
                    _owningGrid = this.OwningGrid;
                    _owningGrid.Columns.CollectionChanged += new NotifyCollectionChangedEventHandler(Columns_CollectionChanged);
                    _owningGrid.CurrentCellChanged += new EventHandler<EventArgs>(OwningGrid_CurrentCellChanged);
                    _owningGrid.KeyDown += new KeyEventHandler(OwningGrid_KeyDown);
                    _owningGrid.LoadingRow += new EventHandler<DataGridRowEventArgs>(OwningGrid_LoadingRow);
                }

                return true;
            }

            return false;
        }

        private void OwningGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (_currentCheckBox != null)
            {
                _currentCheckBox.IsEnabled = false;
            }

            if (this.OwningGrid != null && this.OwningGrid.CurrentColumn == this &&
                this.OwningGrid.IsSlotVisible(this.OwningGrid.CurrentSlot))
            {
                DataGridRow row = this.OwningGrid.DisplayData.GetDisplayedElement(this.OwningGrid.CurrentSlot) as DataGridRow;
                if (row != null)
                {
                    CheckBox checkBox = this.GetCellContent(row) as CheckBox;
                    if (checkBox != null)
                    {
                        checkBox.IsEnabled = true;
                    }

                    _currentCheckBox = checkBox;
                }
            }
        }

#if WINDOWS_UWP
        private void OwningGrid_KeyDown(object sender, KeyRoutedEventArgs e)
#else
        private void OwningGrid_KeyDown(object sender, KeyEventArgs e)
#endif
        {
#if WINDOWS_UWP
            bool isForSpaceKey = e.Key == Windows.System.VirtualKey.Space;
#else
            bool isForSpaceKey = e.Key == Key.Space;
#endif
            if (isForSpaceKey && this.OwningGrid != null &&
                this.OwningGrid.CurrentColumn == this)
            {
                DataGridRow row = this.OwningGrid.DisplayData.GetDisplayedElement(this.OwningGrid.CurrentSlot) as DataGridRow;
                if (row != null)
                {
                    CheckBox checkBox = this.GetCellContent(row) as CheckBox;
                    if (checkBox == _currentCheckBox)
                    {
                        _beganEditWithKeyboard = true;
                        this.OwningGrid.BeginEdit();
                        return;
                    }
                }
            }

            _beganEditWithKeyboard = false;
        }

        private void OwningGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (this.OwningGrid != null)
            {
                CheckBox checkBox = this.GetCellContent(e.Row) as CheckBox;
                if (checkBox != null)
                {
                    if (this.OwningGrid.CurrentColumnIndex == this.Index && this.OwningGrid.CurrentSlot == e.Row.Slot)
                    {
                        if (_currentCheckBox != null)
                        {
                            _currentCheckBox.IsEnabled = false;
                        }

                        checkBox.IsEnabled = true;
                        _currentCheckBox = checkBox;
                    }
                    else
                    {
                        checkBox.IsEnabled = false;
                    }
                }
            }
        }
    }
}
