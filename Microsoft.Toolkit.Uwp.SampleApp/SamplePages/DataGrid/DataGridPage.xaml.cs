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

using Microsoft.Toolkit.Uwp.SampleApp.Data;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Toolkit.Uwp.SampleApp.SamplePages
{
    public sealed partial class DataGridPage : Page, IXamlRenderListener
    {
        private DataGrid dataGrid;
        private DataGridDataSource viewModel = new DataGridDataSource();

        public DataGridPage()
        {
            InitializeComponent();
        }

        public async void OnXamlRendered(FrameworkElement control)
        {
            dataGrid = control.FindDescendantByName("dataGrid") as DataGrid;
            dataGrid.ItemsSource = await viewModel.GetDataAsync();
            dataGrid.Sorting += DataGrid_Sorting;
            dataGrid.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // set the tag to use in sorting
            e.Column.Tag = e.PropertyName;

            // if the column is already present from markup declaration, do not autogenerate it
            foreach (DataGridColumn c in dataGrid.Columns)
            {
                if (c.Tag.ToString() == e.PropertyName)
                {
                    e.Cancel = true;
                }
            }
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            string previousSortedColumn = viewModel.CachedSortedColumn;
            if (previousSortedColumn != string.Empty)
            {
                foreach (DataGridColumn c in dataGrid.Columns)
                {
                    if (c.Tag.ToString() == previousSortedColumn)
                    {
                        if (previousSortedColumn != e.Column.Tag.ToString())
                        {
                            c.SortDirection = null;
                        }
                    }
                }
            }

            switch (e.Column.SortDirection)
            {
                case null:
                case DataGridSortDirection.Ascending:
                    dataGrid.ItemsSource = viewModel.SortData(e.Column.Tag.ToString(), true);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                    break;
                case DataGridSortDirection.Descending:
                    dataGrid.ItemsSource = viewModel.SortData(e.Column.Tag.ToString(), false);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                    break;
            }
        }
    }
}
