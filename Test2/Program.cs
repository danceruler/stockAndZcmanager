using mzhReptile.Http;
using mzhReptile.Str;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebResolve.Model;
using WebResolve.Stock;

namespace Test2
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = EastMoney.GetInfo(InfoType.infochange);
            Console.ReadKey();
        }
    }
}
