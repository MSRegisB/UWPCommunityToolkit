using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Data;

namespace Microsoft.Toolkit.Uwp.SampleApp.Data
{
    [Bindable]
    public class DataGridDataSource
    {
        public string CachedSortedColumn
        {
            get
            {
                return _cachedSortedColumn;
            }

            set
            {
                _cachedSortedColumn = value;
            }
        }

        public async Task<IEnumerable<DataGridDataItem>> GetDataAsync()
        {
            var uri = new Uri($"ms-appx:///Assets/mtns.csv");
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            IRandomAccessStreamWithContentType randomStream = await file.OpenReadAsync();
            _items = new ObservableCollection<DataGridDataItem>();

            using (StreamReader sr = new StreamReader(randomStream.AsStreamForRead()))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] values = line.Split(',');

                    _items.Add(
                        new DataGridDataItem()
                        {
                            Rank = uint.Parse(values[0]),
                            Mountain = values[1],
                            Height_m = uint.Parse(values[2]),
                            Range = values[3],
                            Coordinates = values[4],
                            Prominence = uint.Parse(values[5]),
                            Parent_mountain = values[6],
                            First_ascent = uint.Parse(values[7]),
                            Ascents = values[8]
                        });
                }
            }

            return _items;
        }

        public ObservableCollection<DataGridDataItem> SortData(string sortBy, bool ascending)
        {
            CachedSortedColumn = sortBy;
            switch (sortBy)
            {
                case "Rank":
                    if (ascending)
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.Rank ascending
                                                                          select item);
                    }
                    else
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.Rank descending
                                                                          select item);
                    }

                case "Parent_mountain":
                    if (ascending)
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.Parent_mountain ascending
                                                                          select item);
                    }
                    else
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.Parent_mountain descending
                                                                          select item);
                    }

                case "Mountain":
                    if (ascending)
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.Mountain ascending
                                                                          select item);
                    }
                    else
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.Mountain descending
                                                                          select item);
                    }

                case "Height_m":
                    if (ascending)
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.Height_m ascending
                                                                          select item);
                    }
                    else
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.Height_m descending
                                                                          select item);
                    }

                case "Range":
                    if (ascending)
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.Range ascending
                                                                          select item);
                    }
                    else
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.Range descending
                                                                          select item);
                    }

                case "Coordinates":
                    if (ascending)
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.Coordinates ascending
                                                                          select item);
                    }
                    else
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.Coordinates descending
                                                                          select item);
                    }

                case "First_ascent":
                    if (ascending)
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.First_ascent ascending
                                                                          select item);
                    }
                    else
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.First_ascent descending
                                                                          select item);
                    }

                case "Ascents":
                    if (ascending)
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.Ascents ascending
                                                                          select item);
                    }
                    else
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.Ascents descending
                                                                          select item);
                    }

                case "Prominence":
                    if (ascending)
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.Prominence ascending
                                                                          select item);
                    }
                    else
                    {
                        return new ObservableCollection<DataGridDataItem>(from item in _items
                                                                          orderby item.Prominence descending
                                                                          select item);
                    }
            }

            return _items;
        }

        private static ObservableCollection<DataGridDataItem> _items;
        private string _cachedSortedColumn = string.Empty;
    }
}
