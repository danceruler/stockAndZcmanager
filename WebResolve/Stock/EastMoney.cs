using mzhReptile.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebResolve.Model;

namespace WebResolve.Stock
{
    public static class EastMoney
    {
        private readonly static string urlmodel = "http://data.eastmoney.com/notices/hsa/{0}.html";
        public static List<EastMoneyModel> GetInfo(InfoType infoType)
        {
            List<EastMoneyModel> news = new List<EastMoneyModel>();
            var result = "";
            try
            {
                result = httpRequestHelper.GetHtml(getUrl((int)infoType + ""));
            }
            catch (Exception)
            {
                return GetInfo(infoType);
            }
            
            string pattern = @"defjson: {\S*},";
            var content = "";
            foreach (Match match in Regex.Matches(result, pattern))
            {
                content = match.Value;
            }
            content = content.Replace("defjson: ", "").Replace(",", "");
            content = content.Replace("\"\"", "\",\"");
            content = content.Replace("null\"", "null,\"");
            content = content.Replace("]\"", "],\"");
            content = content.Replace("}{", "},{");
            string pattern2 = @":\d+""";

            foreach (Match match in Regex.Matches(content, pattern2))
            {
                if (match.Value != ":00\"")
                {
                    content = content.Replace(match.Value, match.Value.Replace("\"", ",\""));
                }

            }
            JObject resultObj = (JObject)JsonConvert.DeserializeObject(content);
            foreach(JObject jObject in resultObj["data"])
            {
                //var t = jObject.ToString();
                EastMoneyModel eastMoneyModel = new EastMoneyModel();
                eastMoneyModel.code = jObject["CDSY_SECUCODES"][0]["SECURITYCODE"].ToString();
                eastMoneyModel.stockName = jObject["CDSY_SECUCODES"][0]["SECURITYFULLNAME"].ToString();
                eastMoneyModel.title = jObject["NOTICETITLE"].ToString();
                eastMoneyModel.Date = DateTime.Parse(jObject["NOTICEDATE"].ToString());
                eastMoneyModel.url = jObject["Url"].ToString();
                //eastMoneyModel.url = eastMoneyModel.url;
                string pattern3 = @"\d{3}[A-Z]";
                foreach (Match match in Regex.Matches(eastMoneyModel.url, pattern3))
                {
                    string t1 = match.Value;
                    string t2 = match.Value.Insert(3, ",");
                    eastMoneyModel.url = eastMoneyModel.url.Replace(t1,t2);
                    break;
                }
                eastMoneyModel.type = infoType.ToString();
                news.Add(eastMoneyModel);
            }
            return news;
        }
        private static string getUrl(string infoType)
        {
            return string.Format(urlmodel, infoType);
        }
    }
}
