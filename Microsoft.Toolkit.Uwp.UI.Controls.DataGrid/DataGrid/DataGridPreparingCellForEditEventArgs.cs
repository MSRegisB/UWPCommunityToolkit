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

#if WINDOWS_UWP
using System;
using Windows.UI.Xaml;
#endif

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    /// <summary>
    /// Provides data for the <see cref="E:Microsoft.Toolkit.Uwp.UI.Controls.DataGrid.PreparingCellForEdit"/> event.
    /// </summary>
    /// <QualityBand>Mature</QualityBand>
    public class DataGridPreparingCellForEditEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.DataGridPreparingCellForEditEventArgs"/> class.
        /// </summary>
        /// <param name="column">The column that contains the cell to be edited.</param>
        /// <param name="row">The row that contains the cell to be edited.</param>
        /// <param name="editingEventArgs">Information about the user gesture that caused the cell to enter edit mode.</param>
        /// <param name="editingElement">The element that the column displays for a cell in editing mode.</param>
        public DataGridPreparingCellForEditEventArgs(
            DataGridColumn column,
            DataGridRow row,
            RoutedEventArgs editingEventArgs,
            FrameworkElement editingElement)
        {
            this.Column = column;
            this.Row = row;
            this.EditingEventArgs = editingEventArgs;
            this.EditingElement = editingElement;
        }

        /// <summary>
        /// Gets the column that contains the cell to be edited.
        /// </summary>
        public DataGridColumn Column
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the element that the column displays for a cell in editing mode.
        /// </summary>
        public FrameworkElement EditingElement
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets information about the user gesture that caused the cell to enter edit mode.
        /// </summary>
        public RoutedEventArgs EditingEventArgs
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the row that contains the cell to be edited.
        /// </summary>
        public DataGridRow Row
        {
            get;
            private set;
        }
    }
}