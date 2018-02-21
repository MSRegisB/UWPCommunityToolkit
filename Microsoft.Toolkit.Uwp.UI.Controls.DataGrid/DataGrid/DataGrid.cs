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

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    /// <summary>
    /// The best DataGrid control ever created.
    /// </summary>
    [TemplatePart(Name = RectanglePartName, Type = typeof(Rectangle))]
    public class DataGrid : Control
    {
        /// <summary>
        /// Identifies the <see cref="BooleanProperty"/> property.
        /// </summary>
        public static readonly DependencyProperty BooleanPropertyProperty =
            DependencyProperty.Register(nameof(BooleanProperty), typeof(bool), typeof(DataGrid), new PropertyMetadata(false, OnBooleanPropertyChanged));

        // Template Parts.
        private const string RectanglePartName = "PART_Rectangle";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGrid"/> class.
        /// Create a default DataGrid control.
        /// </summary>
        public DataGrid()
        {
            DefaultStyleKey = typeof(DataGrid);
        }

        /// <summary>
        /// Gets or sets a value indicating whether ...
        /// </summary>
        public bool BooleanProperty
        {
            get { return (bool)GetValue(BooleanPropertyProperty); }
            set { SetValue(BooleanPropertyProperty, value); }
        }

        private static void OnBooleanPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)d;

            if (dataGrid.BooleanProperty)
            {
                // Do this
            }
            else
            {
                // Do that
            }
        }
    }
}
