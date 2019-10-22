using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebResolve.Model
{
    public class EastMoneyModel
    {
        public String code { get; set; }
        public String stockName { get; set; }
        public String title { get; set; }
        public String type { get; set; }
        public String url { get; set; }
        public DateTime Date { get; set; }
    }

    public enum InfoType
    {
        finace = 2,
        danger = 3,
        recombo = 6,
        infochange = 4,
        havestockchange = 7
    }
}
