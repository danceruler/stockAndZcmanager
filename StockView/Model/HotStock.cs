using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockView.Model
{
    public class HotStock
    {
        public string id { get; set; }
        public DateTime date { get; set; }
        public string name { get; set; }
        public int InOupPoint { get; set; }
        public long rowNum { get; set; }
    }
}
