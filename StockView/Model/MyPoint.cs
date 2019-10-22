using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockView.Model
{
    public class MyPoint
    {
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public string date { get; set; }
        public string color { get; set; }
        public decimal closeP { get; set; }
        public decimal openP { get; set; }
        public decimal maxP { get; set; }
        public decimal minP { get; set; }
        public decimal ma5 { get; set; }
        public decimal ma10 { get; set; }
        public decimal ma20 { get; set; }
        public decimal ma30 { get; set; }
        public decimal ma60 { get; set; }
        public decimal dif { get; set; }
        public decimal dea { get; set; }
        public long volume { get; set; }
        public int? InOutP { get; set; }
    }
}
