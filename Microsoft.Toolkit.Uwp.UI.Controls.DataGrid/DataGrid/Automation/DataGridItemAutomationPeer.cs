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
using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls.DataGridInternals;
using Windows.Foundation;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;

namespace Microsoft.Toolkit.Uwp.Automation.Peers
{
    /// <summary>
    /// AutomationPeer for an item in a DataGrid
    /// </summary>
    public class DataGridItemAutomationPeer : FrameworkElementAutomationPeer,
        IInvokeProvider, IScrollItemProvider, ISelectionItemProvider, ISelectionProvider
    {
        private object _item;
        private AutomationPeer _dataGridAutomationPeer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Toolkit.Uwp.Automation.Peers.DataGridItemAutomationPeer"/> class.
        /// </summary>
        public DataGridItemAutomationPeer(object item, DataGrid dataGrid)
            : base(dataGrid)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (dataGrid == null)
            {
                throw new ArgumentNullException("dataGrid");
            }

            _item = item;
            _dataGridAutomationPeer = FrameworkElementAutomationPeer.CreatePeerForElement(dataGrid);
        }

        private DataGrid OwningDataGrid
        {
            get
            {
                DataGridAutomationPeer gridPeer = _dataGridAutomationPeer as DataGridAutomationPeer;
                return gridPeer.Owner as DataGrid;
            }
        }

        private DataGridRow OwningRow
        {
            get
            {
                int index = this.OwningDataGrid.DataConnection.IndexOf(_item);
                int slot = this.OwningDataGrid.SlotFromRowIndex(index);

                if (this.OwningDataGrid.IsSlotVisible(slot))
                {
                    return this.OwningDataGrid.DisplayData.GetDisplayedElement(slot) as DataGridRow;
                }

                return null;

            }
        }

        internal DataGridRowAutomationPeer OwningRowPeer
        {
            get
            {
                DataGridRowAutomationPeer rowPeer = null;
                DataGridRow row = this.OwningRow;
                if (row != null)
                {
                    rowPeer = FrameworkElementAutomationPeer.CreatePeerForElement(row) as DataGridRowAutomationPeer;
                }

                return rowPeer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetAcceleratorKeyCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.GetAcceleratorKey() : string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetAccessKeyCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.GetAccessKey() : string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.DataItem;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetAutomationIdCore()
        {
            // The AutomationId should be unset for dynamic content.
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Rect GetBoundingRectangleCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.GetBoundingRectangle() : default(Rect);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IList<AutomationPeer> GetChildrenCore()
        {
            if (this.OwningRowPeer != null)
            {
                this.OwningRowPeer.InvalidatePeer();
                return this.OwningRowPeer.GetChildren();
            }

            return new List<AutomationPeer>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetClassNameCore()
        {
            return (this.OwningRowPeer != null) ? this.OwningRowPeer.GetClassName() : string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Point GetClickablePointCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.GetClickablePoint() : new Point(double.NaN, double.NaN);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetHelpTextCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.GetHelpText() : string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetItemStatusCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.GetItemStatus() : string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetItemTypeCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.GetItemType() : string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override AutomationPeer GetLabeledByCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.GetLabeledBy() : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetLocalizedControlTypeCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.GetLocalizedControlType() : string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetNameCore()
        {
            return _item.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override AutomationOrientation GetOrientationCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.GetOrientation() : AutomationOrientation.None;
        }

        /// <summary>
        /// Gets the control pattern that is associated with the specified Windows.UI.Xaml.Automation.Peers.PatternInterface.
        /// </summary>
        /// <param name="patternInterface">A value from the Windows.UI.Xaml.Automation.Peers.PatternInterface enumeration.</param>
        /// <returns>The object that supports the specified pattern, or null if unsupported.</returns>
        protected override object GetPatternCore(PatternInterface patternInterface)
        {
            switch (patternInterface)
            {
                case PatternInterface.Invoke:
                {
                    if (!this.OwningDataGrid.IsReadOnly)
                    {
                        return this;
                    }

                    break;
                }

                case PatternInterface.ScrollItem:
                {
                    if (this.OwningDataGrid.VerticalScrollBar != null &&
                        this.OwningDataGrid.VerticalScrollBar.Maximum > 0)
                    {
                        return this;
                    }

                    break;
                }

                case PatternInterface.Selection:
                case PatternInterface.SelectionItem:
                    return this;
            }

            return base.GetPatternCore(patternInterface);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool HasKeyboardFocusCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.HasKeyboardFocus() : false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool IsContentElementCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.IsContentElement() : true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool IsControlElementCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.IsControlElement() : true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool IsEnabledCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.IsEnabled() : false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool IsKeyboardFocusableCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.IsKeyboardFocusable() : false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool IsOffscreenCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.IsOffscreen() : true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool IsPasswordCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.IsPassword() : false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool IsRequiredForFormCore()
        {
            return this.OwningRowPeer != null ? this.OwningRowPeer.IsRequiredForForm() : false;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetFocusCore()
        {
            if (this.OwningRowPeer != null)
            {
                this.OwningRowPeer.SetFocus();
            }
        }

        void IInvokeProvider.Invoke()
        {
            EnsureEnabled();

            if (this.OwningRowPeer == null)
            {
                this.OwningDataGrid.ScrollIntoView(_item, null);
            }

            bool success = false;
            if (this.OwningRow != null)
            {
                if (this.OwningDataGrid.WaitForLostFocus(delegate { ((IInvokeProvider)this).Invoke(); }))
                {
                    return;
                }

                if (this.OwningDataGrid.EditingRow == this.OwningRow)
                {
                    success = this.OwningDataGrid.CommitEdit(DataGridEditingUnit.Row, true /*exitEditing*/);
                }
                else if (this.OwningDataGrid.UpdateSelectionAndCurrency(this.OwningDataGrid.CurrentColumnIndex, this.OwningRow.Slot, DataGridSelectionAction.SelectCurrent, false))
                {
                    success = this.OwningDataGrid.BeginEdit();
                }
            }
        }

        void IScrollItemProvider.ScrollIntoView()
        {
            this.OwningDataGrid.ScrollIntoView(_item, null);
        }

        bool ISelectionItemProvider.IsSelected
        {
            get
            {
                return this.OwningDataGrid.SelectedItems.Contains(_item);
            }
        }

        IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
        {
            get
            {
                return ProviderFromPeer(_dataGridAutomationPeer);
            }
        }

        void ISelectionItemProvider.AddToSelection()
        {
            EnsureEnabled();

            if (this.OwningDataGrid.SelectionMode == DataGridSelectionMode.Single &&
                this.OwningDataGrid.SelectedItems.Count > 0 &&
                !this.OwningDataGrid.SelectedItems.Contains(_item))
            {
                throw DataGridError.DataGridAutomationPeer.OperationCannotBePerformed();
            }

            int index = this.OwningDataGrid.DataConnection.IndexOf(_item);
            if (index != -1)
            {
                this.OwningDataGrid.SetRowSelection(this.OwningDataGrid.SlotFromRowIndex(index), true, false);
                return;
            }

            throw DataGridError.DataGridAutomationPeer.OperationCannotBePerformed();
        }

        void ISelectionItemProvider.RemoveFromSelection()
        {
            EnsureEnabled();

            int index = this.OwningDataGrid.DataConnection.IndexOf(_item);
            if (index != -1)
            {
                bool success = true;
                if (this.OwningDataGrid.EditingRow != null && this.OwningDataGrid.EditingRow.Index == index)
                {
                    if (this.OwningDataGrid.WaitForLostFocus(delegate { ((ISelectionItemProvider)this).RemoveFromSelection(); }))
                    {
                        return;
                    }

                    success = this.OwningDataGrid.CommitEdit(DataGridEditingUnit.Row, true /*exitEditing*/);
                }

                if (success)
                {
                    this.OwningDataGrid.SetRowSelection(this.OwningDataGrid.SlotFromRowIndex(index), false, false);
                    return;
                }

                throw DataGridError.DataGridAutomationPeer.OperationCannotBePerformed();
            }
        }

        void ISelectionItemProvider.Select()
        {
            EnsureEnabled();

            int index = this.OwningDataGrid.DataConnection.IndexOf(_item);
            if (index != -1)
            {
                bool success = true;
                if (this.OwningDataGrid.EditingRow != null && this.OwningDataGrid.EditingRow.Index != index)
                {
                    if (this.OwningDataGrid.WaitForLostFocus(delegate { ((ISelectionItemProvider)this).Select(); }))
                    {
                        return;
                    }

                    success = this.OwningDataGrid.CommitEdit(DataGridEditingUnit.Row, true /*exitEditing*/);
                }

                if (success)
                {
                    // Clear all the other selected items and select this one
                    int slot = this.OwningDataGrid.SlotFromRowIndex(index);
                    this.OwningDataGrid.UpdateSelectionAndCurrency(this.OwningDataGrid.CurrentColumnIndex, slot, DataGridSelectionAction.SelectCurrent, false);
                    return;
                }

                throw DataGridError.DataGridAutomationPeer.OperationCannotBePerformed();
            }
        }

        bool ISelectionProvider.CanSelectMultiple
        {
            get
            {
                return false;
            }
        }

        bool ISelectionProvider.IsSelectionRequired
        {
            get
            {
                return false;
            }
        }

        IRawElementProviderSimple[] ISelectionProvider.GetSelection()
        {
            if (this.OwningRow != null &&
                this.OwningDataGrid.IsSlotVisible(this.OwningRow.Slot) &&
                this.OwningDataGrid.CurrentSlot == this.OwningRow.Slot)
            {
                DataGridCell cell = this.OwningRow.Cells[this.OwningRow.OwningGrid.CurrentColumnIndex];
                AutomationPeer peer = FrameworkElementAutomationPeer.CreatePeerForElement(cell);
                if (peer != null)
                {
                    return new IRawElementProviderSimple[] { ProviderFromPeer(peer) };
                }
            }

            return null;
        }

        private void EnsureEnabled()
        {
            if (!_dataGridAutomationPeer.IsEnabled())
            {
                throw new ElementNotEnabledException();
            }
        }
    }
}
