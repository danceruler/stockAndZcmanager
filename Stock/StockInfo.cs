//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Stock
{
    using System;
    using System.Collections.Generic;
    
    public partial class StockInfo
    {
        public string id { get; set; }
        public int type { get; set; }
        public System.DateTime date { get; set; }
        public string name { get; set; }
        public Nullable<decimal> closeprice { get; set; }
        public Nullable<decimal> minprice { get; set; }
        public Nullable<decimal> maxprice { get; set; }
        public Nullable<decimal> openprice { get; set; }
        public Nullable<decimal> fromtclsoeprice { get; set; }
        public Nullable<decimal> upsandowns { get; set; }
        public Nullable<decimal> chg { get; set; }
        public Nullable<long> volume { get; set; }
        public Nullable<decimal> EMA12 { get; set; }
        public Nullable<decimal> EMA26 { get; set; }
        public Nullable<decimal> DIF { get; set; }
        public Nullable<decimal> DEA { get; set; }
        public Nullable<decimal> MACD { get; set; }
        public Nullable<int> IsMACD { get; set; }
        public Nullable<decimal> MA5 { get; set; }
        public Nullable<decimal> MA10 { get; set; }
        public Nullable<decimal> MA20 { get; set; }
        public Nullable<decimal> MA30 { get; set; }
        public Nullable<decimal> MA60 { get; set; }
        public Nullable<int> InOutPoint { get; set; }
    }
}