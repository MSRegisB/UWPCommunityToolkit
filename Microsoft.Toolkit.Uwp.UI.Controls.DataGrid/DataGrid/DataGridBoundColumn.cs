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

using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.UI.Controls.DataGridInternals;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Data;
#endif

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    /// <summary>
    /// Represents a <see cref="T:System.Windows.Controls.DataGrid" /> column that can
    /// bind to a property in the grid's data source.
    /// </summary>
    /// <QualityBand>Mature</QualityBand>
    [StyleTypedProperty(Property = "ElementStyle", StyleTargetType = typeof(FrameworkElement))]
    [StyleTypedProperty(Property = "EditingElementStyle", StyleTargetType = typeof(FrameworkElement))]
    public abstract class DataGridBoundColumn : DataGridColumn
    {
        private Binding _binding;
        private Style _elementStyle;
        private Style _editingElementStyle;

        /// <summary>
        /// Gets or sets the binding that associates the column with a property in the data source.
        /// </summary>
        public virtual Binding Binding
        {
            get
            {
                return this._binding;
            }

            set
            {
                if (this._binding != value)
                {
                    if (this.OwningGrid != null && !this.OwningGrid.CommitEdit(DataGridEditingUnit.Row, true /*exitEditing*/))
                    {
                        // Edited value couldn't be committed, so we force a CancelEdit
                        this.OwningGrid.CancelEdit(DataGridEditingUnit.Row, false /*raiseEvents*/);
                    }

                    this._binding = value;

                    if (this._binding != null)
                    {
                        // Force the TwoWay binding mode if there is a Path present.  TwoWay binding requires a Path.
                        if (!string.IsNullOrEmpty(this._binding.Path.Path))
                        {
                            this._binding.Mode = BindingMode.TwoWay;
                        }

                        if (this._binding.Converter == null)
                        {
                            this._binding.Converter = new DataGridValueConverter();
                        }

#if !WINDOWS_UWP
                        // Setup the binding for validation
                        this._binding.ValidatesOnDataErrors = true;
                        this._binding.ValidatesOnExceptions = true;
                        this._binding.NotifyOnValidationError = true;
#endif
                        this._binding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;

                        // Apply the new Binding to existing rows in the DataGrid
                        if (this.OwningGrid != null)
                        {
                            // TODO: We want clear the Bindings if Binding is set to null
                            // but there's no way to do that right now.  Revisit this if Silverlight
                            // implements the equivalent of BindingOperations.ClearBinding
                            this.OwningGrid.OnColumnBindingChanged(this);
                        }
                    }

                    this.RemoveEditingElement();
                }
            }
        }

        /// <summary>
        /// Gets or sets the binding that will be used to get or set cell content for the clipboard.
        /// If the base ClipboardContentBinding is not explicitly set, this will return the value of Binding.
        /// </summary>
        public override Binding ClipboardContentBinding
        {
            get
            {
                return base.ClipboardContentBinding ?? this.Binding;
            }

            set
            {
                base.ClipboardContentBinding = value;
            }
        }

        /// <summary>
        /// Gets or sets the style that is used when rendering the element that the column displays for a cell in editing mode.
        /// </summary>
        public Style EditingElementStyle
        {
            get
            {
                return _editingElementStyle;
            }

            set
            {
                if (_editingElementStyle != value)
                {
                    _editingElementStyle = value;

                    // We choose not to update the elements already editing in the Grid here.
                    // They will get the EditingElementStyle next time they go into edit mode.
                }
            }
        }

        /// <summary>
        /// Gets or sets the style that is used when rendering the element that the column displays for a cell that is not in editing mode.
        /// </summary>
        public Style ElementStyle
        {
            get
            {
                return _elementStyle;
            }

            set
            {
                if (_elementStyle != value)
                {
                    _elementStyle = value;
                    if (this.OwningGrid != null)
                    {
                        this.OwningGrid.OnColumnElementStyleChanged(this);
                    }
                }
            }
        }

        internal DependencyProperty BindingTarget { get; set; }

        internal override List<string> CreateBindingPaths()
        {
            if (this.Binding != null && this.Binding.Path != null)
            {
                return new List<string>() { this.Binding.Path.Path };
            }

            return base.CreateBindingPaths();
        }

        internal override List<BindingInfo> CreateBindings(FrameworkElement element, object dataItem, bool twoWay)
        {
            BindingInfo bindingData = new BindingInfo();
            if (twoWay && this.BindingTarget != null)
            {
                bindingData.BindingExpression = element.GetBindingExpression(this.BindingTarget);
                if (bindingData.BindingExpression != null)
                {
                    bindingData.BindingTarget = this.BindingTarget;
                    bindingData.Element = element;
                    return new List<BindingInfo> { bindingData };
                }
            }

            foreach (DependencyProperty bindingTarget in element.GetDependencyProperties(false))
            {
                bindingData.BindingExpression = element.GetBindingExpression(bindingTarget);
                if (bindingData.BindingExpression != null
                    && bindingData.BindingExpression.ParentBinding == this.Binding)
                {
                    this.BindingTarget = bindingTarget;
                    bindingData.BindingTarget = this.BindingTarget;
                    bindingData.Element = element;
                    return new List<BindingInfo> { bindingData };
                }
            }

            return base.CreateBindings(element, dataItem, twoWay);
        }

        internal void SetHeaderFromBinding()
        {
            if (this.OwningGrid != null && this.OwningGrid.DataConnection.DataType != null &&
                this.Header == null && this.Binding != null && this.Binding.Path != null)
            {
                string header = this.OwningGrid.DataConnection.DataType.GetDisplayName(this.Binding.Path.Path);
                if (header != null)
                {
                    this.Header = header;
                }
            }
        }
    }
}