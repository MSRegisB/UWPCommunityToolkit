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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
#if WINDOWS_UWP
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.UI.Xaml.Data;
#else
using System.Windows.Data;
#endif

namespace Microsoft.Toolkit.Uwp.UI.Controls.DataGridInternals
{
    internal class DataGridDataConnection
    {
        private int _backupSlotForCurrentChanged;
        private int _columnForCurrentChanged;
        private PropertyInfo[] _dataProperties;
        private IEnumerable _dataSource;
        private Type _dataType;
        private bool _expectingCurrentChanged;
        private object _itemToSelectOnCurrentChanged;
        private DataGrid _owner;
        private bool _scrollForCurrentChanged;
        private DataGridSelectionAction _selectionActionForCurrentChanged;
        private WeakEventListener<DataGridDataConnection, object, NotifyCollectionChangedEventArgs> _weakCollectionChangedListener;
        private WeakEventListener<DataGridDataConnection, object, CurrentChangingEventArgs> _weakCurrentChangingListener;
#if WINDOWS_UWP
        private WeakEventListener<DataGridDataConnection, object, object> _weakCurrentChangedListener;
#else
        private WeakEventListener<DataGridDataConnection, object, EventArgs> _weakCurrentChangedListener;
#endif
#if FEATURE_ICOLLECTIONVIEW_SORT
        private WeakEventListener<DataGridDataConnection, object, NotifyCollectionChangedEventArgs> _weakSortDescriptionsCollectionChangedListener;
#endif

        public DataGridDataConnection(DataGrid owner)
        {
            this._owner = owner;
        }

        public bool AllowEdit
        {
            get
            {
                if (this.List == null)
                {
                    return true;
                }
                else
                {
                    return !this.List.IsReadOnly;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the collection view says it can sort.
        /// </summary>
        public bool AllowSort
        {
            get
            {
                if (this.CollectionView == null)
                {
                    return false;
                }

#if FEATURE_IEDITABLECOLLECTIONVIEW
                if (this.EditableCollectionView != null && (this.EditableCollectionView.IsAddingNew || this.EditableCollectionView.IsEditingItem))
                {
                    return false;
                }
#endif

#if FEATURE_ICOLLECTIONVIEW_SORT
                return this.CollectionView.CanSort;
#else
                return false;
#endif
            }
        }

        public bool CanCancelEdit
        {
            get
            {
#if FEATURE_IEDITABLECOLLECTIONVIEW
                return this.EditableCollectionView != null && this.EditableCollectionView.CanCancelEdit;
#else
                return false;
#endif
            }
        }

        public ICollectionView CollectionView
        {
            get
            {
                return this.DataSource as ICollectionView;
            }
        }

        public int Count
        {
            get
            {
                IList list = this.List;
                if (list != null)
                {
                    return list.Count;
                }

#if FEATURE_PAGEDCOLLECTIONVIEW
                PagedCollectionView collectionView = this.DataSource as PagedCollectionView;
                if (collectionView != null)
                {
                    return collectionView.Count;
                }
#endif

                int count = 0;
                IEnumerable enumerable = this.DataSource;
                if (enumerable != null)
                {
                    IEnumerator enumerator = enumerable.GetEnumerator();
                    if (enumerator != null)
                    {
                        while (enumerator.MoveNext())
                        {
                            count++;
                        }
                    }
                }

                return count;
            }
        }

        public bool DataIsPrimitive
        {
            get
            {
                return DataTypeIsPrimitive(this.DataType);
            }
        }

        public PropertyInfo[] DataProperties
        {
            get
            {
                if (_dataProperties == null)
                {
                    UpdateDataProperties();
                }

                return _dataProperties;
            }
        }

        public IEnumerable DataSource
        {
            get
            {
                return _dataSource;
            }

            set
            {
                _dataSource = value;

                // Because the DataSource is changing, we need to reset our cached values for DataType and DataProperties,
                // which are dependent on the current DataSource
                _dataType = null;
                UpdateDataProperties();
            }
        }

        public Type DataType
        {
            get
            {
                // We need to use the raw ItemsSource as opposed to DataSource because DataSource
                // may be the ItemsSource wrapped in a collection view, in which case we wouldn't
                // be able to take T to be the type if we're given IEnumerable<T>
                if (_dataType == null && _owner.ItemsSource != null)
                {
                    _dataType = _owner.ItemsSource.GetItemType();
                }

                return _dataType;
            }
        }

#if FEATURE_IEDITABLECOLLECTIONVIEW
        public IEditableCollectionView EditableCollectionView
        {
            get
            {
                return this.DataSource as IEditableCollectionView;
            }
        }
#endif

        public bool EndingEdit
        {
            get;
            private set;
        }

        public bool EventsWired
        {
            get;
            private set;
        }

        public bool IsAddingNew
        {
            get
            {
#if FEATURE_IEDITABLECOLLECTIONVIEW
                return this.EditableCollectionView != null && this.EditableCollectionView.IsAddingNew;
#else
                return false;
#endif
            }
        }

        private bool IsGrouping
        {
            get
            {
#if FEATURE_ICOLLECTIONVIEW_GROUP
                return this.CollectionView != null &&
                    this.CollectionView.CanGroup &&
                    this.CollectionView.GroupDescriptions != null &&
                    this.CollectionView.GroupDescriptions.Count > 0;
#else
                return false;
#endif
            }
        }

        public IList List
        {
            get
            {
                return this.DataSource as IList;
            }
        }

        public int NewItemPlaceholderIndex
        {
            get
            {
#if FEATURE_IEDITABLECOLLECTIONVIEW
                if (this.EditableCollectionView != null && this.EditableCollectionView.NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtEnd)
                {
                    return this.Count - 1;
                }
#endif

                return -1;
            }
        }

#if FEATURE_IEDITABLECOLLECTIONVIEW
        public NewItemPlaceholderPosition NewItemPlaceholderPosition
        {
            get
            {
                if (this.EditableCollectionView != null)
                {
                    return this.EditableCollectionView.NewItemPlaceholderPosition;
                }

                return NewItemPlaceholderPosition.None;
            }
        }
#endif

        public bool ShouldAutoGenerateColumns
        {
            get
            {
                return this._owner.AutoGenerateColumns
                    && (this._owner.ColumnsInternal.AutogeneratedColumnCount == 0)
                    && ((this.DataProperties != null && this.DataProperties.Length > 0) || this.DataIsPrimitive);
            }
        }

#if FEATURE_ICOLLECTIONVIEW_SORT
        public SortDescriptionCollection SortDescriptions
        {
            get
            {
                if (this.CollectionView != null && this.CollectionView.CanSort)
                {
                    return this.CollectionView.SortDescriptions;
                }
                else
                {
                    return (SortDescriptionCollection)null;
                }
            }
        }
#endif

        public static bool CanEdit(Type type)
        {
            Debug.Assert(type != null, "Expected non-null type.");

            type = type.GetNonNullableType();

            return
#if WINDOWS_UWP
                type.GetTypeInfo().IsEnum
#else
                type.IsEnum
#endif
                || type == typeof(string)
                || type == typeof(char)
                || type == typeof(bool)
                || type == typeof(byte)
                || type == typeof(sbyte)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal)
                || type == typeof(short)
                || type == typeof(int)
                || type == typeof(long)
                || type == typeof(ushort)
                || type == typeof(uint)
                || type == typeof(ulong)
                || type == typeof(DateTime);
        }

        /// <summary>
        /// Puts the entity into editing mode if possible
        /// </summary>
        /// <param name="dataItem">The entity to edit</param>
        /// <returns>True if editing was started</returns>
        public bool BeginEdit(object dataItem)
        {
            if (dataItem == null)
            {
                return false;
            }

#if FEATURE_IEDITABLECOLLECTIONVIEW
            IEditableCollectionView editableCollectionView = this.EditableCollectionView;
            if (editableCollectionView != null)
            {
                if ((editableCollectionView.IsEditingItem && (dataItem == editableCollectionView.CurrentEditItem)) ||
                    (editableCollectionView.IsAddingNew && (dataItem == editableCollectionView.CurrentAddItem)))
                {
                    return true;
                }
                else
                {
                    editableCollectionView.EditItem(dataItem);
                    return editableCollectionView.IsEditingItem;
                }
            }
#endif

            IEditableObject editableDataItem = dataItem as IEditableObject;
            if (editableDataItem != null)
            {
                editableDataItem.BeginEdit();
                return true;
            }

            return true;
        }

        /// <summary>
        /// Cancels the current entity editing and exits the editing mode.
        /// </summary>
        /// <param name="dataItem">The entity being edited</param>
        /// <returns>True if a cancellation operation was invoked.</returns>
        public bool CancelEdit(object dataItem)
        {
#if FEATURE_IEDITABLECOLLECTIONVIEW
            IEditableCollectionView editableCollectionView = this.EditableCollectionView;
            if (editableCollectionView != null)
            {
                this._owner.NoCurrentCellChangeCount++;
                this.EndingEdit = true;
                try
                {
                    if (editableCollectionView.IsAddingNew && dataItem == editableCollectionView.CurrentAddItem)
                    {
                        editableCollectionView.CancelNew();
                        return true;
                    }
                    else if (editableCollectionView.CanCancelEdit)
                    {
                        editableCollectionView.CancelEdit();
                        return true;
                    }
                }
                finally
                {
                    this._owner.NoCurrentCellChangeCount--;
                    this.EndingEdit = false;
                }

                return false;
            }
#endif

            IEditableObject editableDataItem = dataItem as IEditableObject;
            if (editableDataItem != null)
            {
                editableDataItem.CancelEdit();
                return true;
            }

            return true;
        }

        /// <summary>
        /// Commits the current entity editing and exits the editing mode.
        /// </summary>
        /// <param name="dataItem">The entity being edited</param>
        /// <returns>True if a commit operation was invoked.</returns>
        public bool EndEdit(object dataItem)
        {
#if FEATURE_IEDITABLECOLLECTIONVIEW
            IEditableCollectionView editableCollectionView = this.EditableCollectionView;
            if (editableCollectionView != null)
            {
                // IEditableCollectionView.CommitEdit can potentially change currency. If it does,
                // we don't want to attempt a second commit inside our CurrentChanging event handler.
                this._owner.NoCurrentCellChangeCount++;
                this.EndingEdit = true;
                try
                {
                    if (editableCollectionView.IsAddingNew && dataItem == editableCollectionView.CurrentAddItem)
                    {
                        editableCollectionView.CommitNew();
                    }
                    else
                    {
                        editableCollectionView.CommitEdit();
                    }
                }
                finally
                {
                    this._owner.NoCurrentCellChangeCount--;
                    this.EndingEdit = false;
                }

                return true;
            }
#endif

            IEditableObject editableDataItem = dataItem as IEditableObject;
            if (editableDataItem != null)
            {
                editableDataItem.EndEdit();
            }

            return true;
        }

        // Assumes index >= 0, returns null if index >= Count
        public object GetDataItem(int index)
        {
            Debug.Assert(index >= 0, "Expected positive index.");

            IList list = this.List;
            if (list != null)
            {
                return (index < list.Count) ? list[index] : null;
            }

#if FEATURE_PAGEDCOLLECTIONVIEW
            PagedCollectionView collectionView = this.DataSource as PagedCollectionView;
            if (collectionView != null)
            {
                return (index < collectionView.Count) ? collectionView.GetItemAt(index) : null;
            }
#endif

            IEnumerable enumerable = this.DataSource;
            if (enumerable != null)
            {
                IEnumerator enumerator = enumerable.GetEnumerator();
                int i = -1;
                while (enumerator.MoveNext() && i < index)
                {
                    i++;
                    if (i == index)
                    {
                        return enumerator.Current;
                    }
                }
            }

            return null;
        }

        public bool GetPropertyIsReadOnly(string propertyName)
        {
            if (this.DataType != null)
            {
                if (!string.IsNullOrEmpty(propertyName))
                {
                    Type propertyType = this.DataType;
                    PropertyInfo propertyInfo = null;
                    List<string> propertyNames = TypeHelper.SplitPropertyPath(propertyName);
                    for (int i = 0; i < propertyNames.Count; i++)
                    {
#if WINDOWS_UWP
                        if (propertyType.GetTypeInfo().GetIsReadOnly())
#else
                        if (propertyType.GetIsReadOnly())
#endif
                        {
                            return true;
                        }

                        object[] index = null;
                        propertyInfo = propertyType.GetPropertyOrIndexer(propertyNames[i], out index);
                        if (propertyInfo == null || propertyInfo.GetIsReadOnly())
                        {
                            // Either the property doesn't exist or it does exist but is read-only.
                            return true;
                        }

                        // Check if EditableAttribute is defined on the property and if it indicates uneditable
                        EditableAttribute editableAttribute = null;
#if WINDOWS_UWP
                        editableAttribute = propertyInfo.GetCustomAttributes().OfType<EditableAttribute>().FirstOrDefault();
#else
                        object[] attributes = propertyInfo.GetCustomAttributes(typeof(EditableAttribute), true);
                        if (attributes != null && attributes.Length > 0)
                        {
                            editableAttribute = attributes[0] as EditableAttribute;
                            Debug.Assert(editableAttribute != null, "Expected non-null editableAttribute.");
                        }
#endif
                        if (editableAttribute != null && !editableAttribute.AllowEdit)
                        {
                            return true;
                        }

                        propertyType = propertyInfo.PropertyType.GetNonNullableType();
                    }

                    return propertyInfo == null || !propertyInfo.CanWrite || !this.AllowEdit || !CanEdit(propertyType);
                }
                else
                {
#if WINDOWS_UWP
                    if (this.DataType.GetTypeInfo().GetIsReadOnly())
#else
                    if (this.DataType.GetIsReadOnly())
#endif
                    {
                        return true;
                    }
                }
            }

            return !this.AllowEdit;
        }

        public int IndexOf(object dataItem)
        {
            IList list = this.List;
            if (list != null)
            {
                return list.IndexOf(dataItem);
            }

#if FEATURE_PAGEDCOLLECTIONVIEW
            PagedCollectionView cv = this.DataSource as PagedCollectionView;
            if (cv != null)
            {
                return cv.IndexOf(dataItem);
            }
#endif

            IEnumerable enumerable = this.DataSource;
            if (enumerable != null && dataItem != null)
            {
                int index = 0;
                foreach (object dataItemTmp in enumerable)
                {
                    if ((dataItem == null && dataItemTmp == null) ||
                        dataItem.Equals(dataItemTmp))
                    {
                        return index;
                    }

                    index++;
                }
            }

            return -1;
        }

#if FEATURE_PAGEDCOLLECTIONVIEW
        /// <summary>
        /// Creates a collection view around the DataGrid's source. ICollectionViewFactory is
        /// used if the source implements it. Otherwise a PagedCollectionView is returned.
        /// </summary>
        /// <param name="source">Enumerable source for which to create a view</param>
        /// <returns>ICollectionView view over the provided source</returns>
#else
        /// <summary>
        /// Creates a collection view around the DataGrid's source. ICollectionViewFactory is
        /// used if the source implements it.
        /// </summary>
        /// <param name="source">Enumerable source for which to create a view</param>
        /// <returns>ICollectionView view over the provided source</returns>
#endif
        internal static ICollectionView CreateView(IEnumerable source)
        {
            Debug.Assert(source != null, "source unexpectedly null");
            Debug.Assert(!(source is ICollectionView), "source is an ICollectionView");

            ICollectionView collectionView = null;

            ICollectionViewFactory collectionViewFactory = source as ICollectionViewFactory;
            if (collectionViewFactory != null)
            {
                // If the source is a collection view factory, give it a chance to produce a custom collection view.
                collectionView = collectionViewFactory.CreateView();

                // Intentionally not catching potential exception thrown by ICollectionViewFactory.CreateView().
            }

#if FEATURE_PAGEDCOLLECTIONVIEW
            if (collectionView == null)
            {
                // If we still do not have a collection view, default to a PagedCollectionView.
                collectionView = new PagedCollectionView(source);
            }
#endif

            return collectionView;
        }

        internal static bool DataTypeIsPrimitive(Type dataType)
        {
            if (dataType != null)
            {
                Type type = TypeHelper.GetNonNullableType(dataType);  // no-opt if dataType isn't nullable
                return
#if WINDOWS_UWP
                    type.GetTypeInfo().IsPrimitive ||
#else
                    type.IsPrimitive ||
#endif
                    type == typeof(string) ||
                    type == typeof(decimal) ||
                    type == typeof(DateTime);
            }
            else
            {
                return false;
            }
        }

        internal void ClearDataProperties()
        {
            this._dataProperties = null;
        }

        internal void MoveCurrentTo(object item, int backupSlot, int columnIndex, DataGridSelectionAction action, bool scrollIntoView)
        {
            if (this.CollectionView != null)
            {
                this._expectingCurrentChanged = true;
                this._columnForCurrentChanged = columnIndex;
                this._itemToSelectOnCurrentChanged = item;
                this._selectionActionForCurrentChanged = action;
                this._scrollForCurrentChanged = scrollIntoView;
                this._backupSlotForCurrentChanged = backupSlot;

                bool itemIsCollectionViewGroup = false;

#if FEATURE_COLLECTIONVIEWGROUP
                itemIsCollectionViewGroup = item is CollectionViewGroup;
#endif
                this.CollectionView.MoveCurrentTo((itemIsCollectionViewGroup || this.IndexOf(item) == this.NewItemPlaceholderIndex) ? null : item);

                this._expectingCurrentChanged = false;
            }
        }

        internal void UnWireEvents(IEnumerable value)
        {
            INotifyCollectionChanged notifyingDataSource = value as INotifyCollectionChanged;
            if (notifyingDataSource != null && _weakCollectionChangedListener != null)
            {
                _weakCollectionChangedListener.Detach();
                _weakCollectionChangedListener = null;
            }

#if FEATURE_ICOLLECTIONVIEW_SORT
            if (this.SortDescriptions != null && _weakSortDescriptionsCollectionChangedListener != null)
            {
                _weakSortDescriptionsCollectionChangedListener.Detach();
                _weakSortDescriptionsCollectionChangedListener = null;
            }
#endif

            if (this.CollectionView != null)
            {
                if (_weakCurrentChangedListener != null)
                {
                    _weakCurrentChangedListener.Detach();
                    _weakCurrentChangedListener = null;
                }

                if (_weakCurrentChangingListener != null)
                {
                    _weakCurrentChangingListener.Detach();
                    _weakCurrentChangingListener = null;
                }
            }

            this.EventsWired = false;
        }

        internal void WireEvents(IEnumerable value)
        {
            INotifyCollectionChanged notifyingDataSource = value as INotifyCollectionChanged;
            if (notifyingDataSource != null)
            {
                _weakCollectionChangedListener = new WeakEventListener<DataGridDataConnection, object, NotifyCollectionChangedEventArgs>(this);
                _weakCollectionChangedListener.OnEventAction = (instance, source, eventArgs) => instance.NotifyingDataSource_CollectionChanged(source, eventArgs);
                _weakCollectionChangedListener.OnDetachAction = (weakEventListener) => notifyingDataSource.CollectionChanged -= weakEventListener.OnEvent;
                notifyingDataSource.CollectionChanged += _weakCollectionChangedListener.OnEvent;
            }

#if FEATURE_ICOLLECTIONVIEW_SORT
            if (this.SortDescriptions != null)
            {
                INotifyCollectionChanged sortDescriptionsINCC = (INotifyCollectionChanged)this.SortDescriptions;
                _weakSortDescriptionsCollectionChangedListener = new WeakEventListener<DataGridDataConnection, object, NotifyCollectionChangedEventArgs>(this);
                _weakSortDescriptionsCollectionChangedListener.OnEventAction = (instance, source, eventArgs) => instance.CollectionView_SortDescriptions_CollectionChanged(source, eventArgs);
                _weakSortDescriptionsCollectionChangedListener.OnDetachAction = (weakEventListener) => sortDescriptionsINCC.CollectionChanged -= weakEventListener.OnEvent;
                sortDescriptionsINCC.CollectionChanged += _weakSortDescriptionsCollectionChangedListener.OnEvent;
            }
#endif

            if (this.CollectionView != null)
            {
                // A local variable must be used in the lambda expression or the CollectionView will leak
                ICollectionView collectionView = this.CollectionView;

#if WINDOWS_UWP
                _weakCurrentChangedListener = new WeakEventListener<DataGridDataConnection, object, object>(this);
                _weakCurrentChangedListener.OnEventAction = (instance, source, eventArgs) => instance.CollectionView_CurrentChanged(source, null);
                _weakCurrentChangedListener.OnDetachAction = (weakEventListener) => collectionView.CurrentChanged -= weakEventListener.OnEvent;
                this.CollectionView.CurrentChanged += _weakCurrentChangedListener.OnEvent;
#else
                _weakCurrentChangedListener = new WeakEventListener<DataGridDataConnection, object, EventArgs>(this);
                _weakCurrentChangedListener.OnEventAction = (instance, source, eventArgs) => instance.CollectionView_CurrentChanged(source, eventArgs);
                _weakCurrentChangedListener.OnDetachAction = (weakEventListener) => collectionView.CurrentChanged -= weakEventListener.OnEvent;
                this.CollectionView.CurrentChanged += _weakCurrentChangedListener.OnEvent;
#endif

                _weakCurrentChangingListener = new WeakEventListener<DataGridDataConnection, object, CurrentChangingEventArgs>(this);
                _weakCurrentChangingListener.OnEventAction = (instance, source, eventArgs) => instance.CollectionView_CurrentChanging(source, eventArgs);
                _weakCurrentChangingListener.OnDetachAction = (weakEventListener) => collectionView.CurrentChanging -= weakEventListener.OnEvent;
                this.CollectionView.CurrentChanging += _weakCurrentChangingListener.OnEvent;
            }

            this.EventsWired = true;
        }

#if WINDOWS_UWP
        private void CollectionView_CurrentChanged(object sender, object e)
#else
        private void CollectionView_CurrentChanged(object sender, EventArgs e)
#endif
        {
            if (this._expectingCurrentChanged)
            {
#if FEATURE_COLLECTIONVIEWGROUP
                // Committing Edit could cause our item to move to a group that no longer exists.  In
                // this case, we need to update the item.
                CollectionViewGroup collectionViewGroup = _itemToSelectOnCurrentChanged as CollectionViewGroup;
                if (collectionViewGroup != null)
                {
                    DataGridRowGroupInfo groupInfo = this._owner.RowGroupInfoFromCollectionViewGroup(collectionViewGroup);
                    if (groupInfo == null)
                    {
                        // Move to the next slot if the target slot isn't visible
                        if (!this._owner.IsSlotVisible(_backupSlotForCurrentChanged))
                        {
                            _backupSlotForCurrentChanged = this._owner.GetNextVisibleSlot(_backupSlotForCurrentChanged);
                        }

                        // Move to the next best slot if we've moved past all the slots.  This could happen if multiple
                        // groups were removed.
                        if (_backupSlotForCurrentChanged >= this._owner.SlotCount)
                        {
                            _backupSlotForCurrentChanged = this._owner.GetPreviousVisibleSlot(this._owner.SlotCount);
                        }

                        // Update the itemToSelect
                        int newCurrentPosition = -1;
                        _itemToSelectOnCurrentChanged = this._owner.ItemFromSlot(_backupSlotForCurrentChanged, ref newCurrentPosition);
                    }
                }
#endif
                this._owner.ProcessSelectionAndCurrency(
                    this._columnForCurrentChanged,
                    this._itemToSelectOnCurrentChanged,
                    this._backupSlotForCurrentChanged,
                    this._selectionActionForCurrentChanged,
                    this._scrollForCurrentChanged);
            }
            else if (this.CollectionView != null)
            {
                this._owner.UpdateStateOnCurrentChanged(this.CollectionView.CurrentItem, this.CollectionView.CurrentPosition);
            }
        }

        private void CollectionView_CurrentChanging(object sender, CurrentChangingEventArgs e)
        {
            if (this._owner.NoCurrentCellChangeCount == 0 &&
                !this._expectingCurrentChanged &&
                !this.EndingEdit &&
                !this._owner.CommitEdit())
            {
                // If CommitEdit failed, then the user has most likely input invalid data.
                // We should cancel the current change if we can, otherwise we have to abort the edit.
                if (e.IsCancelable)
                {
                    e.Cancel = true;
                }
                else
                {
                    this._owner.CancelEdit(DataGridEditingUnit.Row, false);
                }
            }
        }

#if FEATURE_ICOLLECTIONVIEW_SORT
        private void CollectionView_SortDescriptions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this._owner.ColumnsItemsInternal.Count == 0)
            {
                return;
            }

            // Refresh sort description
            foreach (DataGridColumn column in this._owner.ColumnsItemsInternal)
            {
                column.HeaderCell.ApplyState(true);
            }
        }
#endif

        private void NotifyingDataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this._owner.LoadingOrUnloadingRow)
            {
                throw DataGridError.DataGrid.CannotChangeItemsWhenLoadingRows();
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems != null, "Unexpected NotifyCollectionChangedAction.Add notification");
                    if (ShouldAutoGenerateColumns)
                    {
                        // The columns are also affected (not just rows) in this case so we need to reset everything
                        this._owner.InitializeElements(false /*recycleRows*/);
                    }
                    else if (!this.IsGrouping)
                    {
                        // If we're grouping then we handle this through the CollectionViewGroup notifications
                        // According to WPF, Add is a single item operation
                        Debug.Assert(e.NewItems.Count == 1, "Expected NewItems.Count equals 1.");
                        this._owner.InsertRowAt(e.NewStartingIndex);
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    IList removedItems = e.OldItems;
                    if (removedItems == null || e.OldStartingIndex < 0)
                    {
                        Debug.Assert(false, "Unexpected NotifyCollectionChangedAction.Remove notification");
                        return;
                    }

                    if (!this.IsGrouping)
                    {
                        // If we're grouping then we handle this through the CollectionViewGroup notifications
                        // According to WPF, Remove is a single item operation
                        foreach (object item in e.OldItems)
                        {
                            Debug.Assert(item != null, "Expected non-null item.");
                            this._owner.RemoveRowAt(e.OldStartingIndex, item);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotSupportedException(); // TODO REGISB

                case NotifyCollectionChangedAction.Reset:
                    // Did the data type change during the reset?  If not, we can recycle
                    // the existing rows instead of having to clear them all.  We still need to clear our cached
                    // values for DataType and DataProperties, though, because the collection has been reset.
                    Type previousDataType = this._dataType;
                    this._dataType = null;
                    if (previousDataType != this.DataType)
                    {
                        ClearDataProperties();
                        this._owner.InitializeElements(false /*recycleRows*/);
                    }
                    else
                    {
                        this._owner.InitializeElements(!ShouldAutoGenerateColumns /*recycleRows*/);
                    }

                    break;
            }
        }

        private void UpdateDataProperties()
        {
            Type dataType = this.DataType;

            if (this.DataSource != null && dataType != null && !DataTypeIsPrimitive(dataType))
            {
                _dataProperties = dataType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Debug.Assert(_dataProperties != null, "Expected non-null _dataProperties.");
            }
            else
            {
                _dataProperties = null;
            }
        }
    }
}