using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Toolkit.Uwp.SampleApp.Data
{
    public class DataGridDataItem
    {
        public uint Rank { get; set; }

        public string Mountain { get; set; }

        public uint Height_m { get; set; }

        public string Range { get; set; }

        public string Coordinates { get; set; }

        public uint Prominence { get; set; }

        public string Parent_mountain { get; set; }

        public uint First_ascent { get; set; }

        public string Ascents { get; set; }
    }
}
