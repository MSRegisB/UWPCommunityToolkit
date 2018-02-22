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

#if WINDOWS_UWP
using System;
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Controls;
#endif

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    /// <summary>
    /// Represents a <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.DataGrid"/> row.
    /// </summary>
    public partial class DataGridRow : Control
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.DataGridRow"/> class.
        /// </summary>
        public DataGridRow()
        {
        }

        /// <summary>
        /// Gets a value indicating whether ...
        /// TODO - Temporary placeholder.
        /// </summary>
        internal bool IsRecyclable { get; }

        /// <summary>
        /// Gets this row's index...
        /// TODO - Temporary placeholder.
        /// </summary>
        internal int Index { get; }

        /// <summary>
        /// Gets this row's slot...
        /// TODO - Temporary placeholder.
        /// </summary>
        internal int Slot { get; }

        /// <summary>
        /// TODO: Temporary placeholder.
        /// </summary>
        /// <param name="recycle">True to recycle this row</param>
        internal void DetachFromDataGrid(bool recycle)
        {
        }
    }
}
