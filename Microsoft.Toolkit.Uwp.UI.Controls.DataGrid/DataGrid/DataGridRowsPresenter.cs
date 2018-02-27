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
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
#endif

namespace Microsoft.Toolkit.Uwp.UI.Controls.Primitives
{
    /// <summary>
    /// Used within the template of a <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.DataGrid"/> to specify the
    /// location in the control's visual tree where the rows are to be added.
    /// </summary>
    /// <QualityBand>Mature</QualityBand>
    public sealed class DataGridRowsPresenter : Panel
    {
        internal DataGrid OwningGrid
        {
            get;
            set;
        }

        /// <summary>
        /// Arranges the content of the <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridRowsPresenter"/>.
        /// </summary>
        /// <returns>
        /// The actual size used by the <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridRowsPresenter"/>.
        /// </returns>
        /// <param name="finalSize">
        /// The final area within the parent that this element should use to arrange itself and its children.
        /// </param>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (finalSize.Height == 0 || this.OwningGrid == null)
            {
                return base.ArrangeOverride(finalSize);
            }

            this.OwningGrid.OnFillerColumnWidthNeeded(finalSize.Width);

            double rowDesiredWidth = this.OwningGrid.ColumnsInternal.VisibleEdgedColumnsWidth + this.OwningGrid.ColumnsInternal.FillerColumn.FillerWidth;
            double topEdge = -this.OwningGrid.NegVerticalOffset;
            foreach (UIElement element in this.OwningGrid.DisplayData.GetScrollingElements())
            {
                DataGridRow row = element as DataGridRow;
                if (row != null)
                {
                    Debug.Assert(row.Index != -1, "Expected Index other than -1."); // A displayed row should always have its index

                    // Visibility for all filler cells needs to be set in one place.  Setting it individually in
                    // each CellsPresenter causes an NxN layout cycle (see DevDiv Bugs 211557)
                    row.EnsureFillerVisibility();
                    row.Arrange(new Rect(-this.OwningGrid.HorizontalOffset, topEdge, rowDesiredWidth, element.DesiredSize.Height));
                }
                else
                {
                    DataGridRowGroupHeader groupHeader = element as DataGridRowGroupHeader;
                    if (groupHeader != null)
                    {
                        double leftEdge = this.OwningGrid.AreRowGroupHeadersFrozen ? 0 : -this.OwningGrid.HorizontalOffset;
                        groupHeader.Arrange(new Rect(leftEdge, topEdge, rowDesiredWidth - leftEdge, element.DesiredSize.Height));
                    }
                }

                topEdge += element.DesiredSize.Height;
            }

            double finalHeight = Math.Max(topEdge + this.OwningGrid.NegVerticalOffset, finalSize.Height);

            // Clip the RowsPresenter so rows cannot overlap other elements in certain styling scenarios
            RectangleGeometry rg = new RectangleGeometry();
            rg.Rect = new Rect(0, 0, finalSize.Width, finalHeight);
            this.Clip = rg;

            return new Size(finalSize.Width, finalHeight);
        }

        /// <summary>
        /// Measures the children of a <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridRowsPresenter"/> to
        /// prepare for arranging them during the <see cref="M:System.Windows.FrameworkElement.ArrangeOverride(System.Windows.Size)"/> pass.
        /// </summary>
        /// <param name="availableSize">
        /// The available size that this element can give to child elements. Indicates an upper limit that child elements should not exceed.
        /// </param>
        /// <returns>
        /// The size that the <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridRowsPresenter"/> determines it needs during layout, based on its calculations of child object allocated sizes.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (availableSize.Height == 0 || this.OwningGrid == null)
            {
                return base.MeasureOverride(availableSize);
            }

            // If the Width of our RowsPresenter changed then we need to invalidate our rows
            bool invalidateRows =
                (!this.OwningGrid.RowsPresenterAvailableSize.HasValue || availableSize.Width != this.OwningGrid.RowsPresenterAvailableSize.Value.Width) &&
                !double.IsInfinity(availableSize.Width);

            // The DataGrid uses the RowsPresenter available size in order to autogrow
            // and calculate the scrollbars
            this.OwningGrid.RowsPresenterAvailableSize = availableSize;

            this.OwningGrid.OnRowsMeasure();

            double totalHeight = -this.OwningGrid.NegVerticalOffset;
            double totalCellsWidth = this.OwningGrid.ColumnsInternal.VisibleEdgedColumnsWidth;

            double headerWidth = 0;
            foreach (UIElement element in this.OwningGrid.DisplayData.GetScrollingElements())
            {
                DataGridRow row = element as DataGridRow;
                if (row != null)
                {
                    if (invalidateRows)
                    {
                        row.InvalidateMeasure();
                    }
                }

                element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                if (row != null && row.HeaderCell != null)
                {
                    headerWidth = Math.Max(headerWidth, row.HeaderCell.DesiredSize.Width);
                }
                else
                {
                    DataGridRowGroupHeader groupHeader = element as DataGridRowGroupHeader;
                    if (groupHeader != null && groupHeader.HeaderCell != null)
                    {
                        headerWidth = Math.Max(headerWidth, groupHeader.HeaderCell.DesiredSize.Width);
                    }
                }

                totalHeight += element.DesiredSize.Height;
            }

            this.OwningGrid.RowHeadersDesiredWidth = headerWidth;

            // Could be positive infinity depending on the DataGrid's bounds
            this.OwningGrid.AvailableSlotElementRoom = availableSize.Height - totalHeight;

            // TODO: totalHeight can be negative if we've just collapsed details.  This is a workaround,
            // the real fix is to correct NegVerticalOffset
            totalHeight = Math.Max(0, totalHeight);

            return new Size(totalCellsWidth + headerWidth, totalHeight);
        }

        /// <summary>
        /// Creates AutomationPeer (<see cref="UIElement.OnCreateAutomationPeer"/>)
        /// </summary>
        /// <returns>An automation peer for this <see cref="T:Microsoft.Toolkit.Uwp.UI.Controls.Primitives.DataGridRowsPresenter"/>.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            // TODO - return new DataGridRowsPresenterAutomationPeer(this);
            return null;
        }

#if DEBUG
        internal void PrintChildren()
        {
            foreach (UIElement element in this.Children)
            {
                DataGridRow row = element as DataGridRow;
                if (row != null)
                {
                    Debug.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "Slot: {0} Row: {1} Visibility: {2} ", row.Slot, row.Index, row.Visibility));
                }
                else
                {
                    DataGridRowGroupHeader groupHeader = element as DataGridRowGroupHeader;
                    if (groupHeader != null)
                    {
#if FEATURE_COLLECTIONVIEWGROUP
                        Debug.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "Slot: {0} GroupHeader: {1} Visibility: {2}", groupHeader.RowGroupInfo.Slot, groupHeader.RowGroupInfo.CollectionViewGroup.Name, groupHeader.Visibility));
#else
                        Debug.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "Slot: {0} Visibility: {1}", groupHeader.RowGroupInfo.Slot, groupHeader.Visibility));
#endif
                    }
                }
            }
        }
#endif
                    }
}
