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
using System.Diagnostics;
using Microsoft.Toolkit.Uwp.UI.Controls.DataGridInternals;

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    internal class DataGridCellCollection
    {
        private List<DataGridCell> _cells;
        private DataGridRow _owningRow;

        internal event EventHandler<DataGridCellEventArgs> CellAdded;

        internal event EventHandler<DataGridCellEventArgs> CellRemoved;

        public DataGridCellCollection(DataGridRow owningRow)
        {
            this._owningRow = owningRow;
            this._cells = new List<DataGridCell>();
        }

        public int Count
        {
            get
            {
                return this._cells.Count;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return this._cells.GetEnumerator();
        }

        public void Insert(int cellIndex, DataGridCell cell)
        {
            Debug.Assert(cellIndex >= 0 && cellIndex <= this._cells.Count, "Expected cellIndex between 0 and _cells.Count inclusive.");
            Debug.Assert(cell != null, "Expected non-null cell.");

            cell.OwningRow = this._owningRow;
            this._cells.Insert(cellIndex, cell);

            if (CellAdded != null)
            {
                CellAdded(this, new DataGridCellEventArgs(cell));
            }
        }

        public void RemoveAt(int cellIndex)
        {
            DataGridCell dataGridCell = this._cells[cellIndex];
            this._cells.RemoveAt(cellIndex);
            dataGridCell.OwningRow = null;
            if (CellRemoved != null)
            {
                CellRemoved(this, new DataGridCellEventArgs(dataGridCell));
            }
        }

        public DataGridCell this[int index]
        {
            get
            {
                if (index < 0 || index >= this._cells.Count)
                {
                    throw DataGridError.DataGrid.ValueMustBeBetween("index", "Index", 0, true, this._cells.Count, false);
                }

                return this._cells[index];
            }
        }
    }
}