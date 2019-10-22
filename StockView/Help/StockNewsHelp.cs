using Stock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebResolve.Model;
using WebResolve.Stock;
using mzhReptile.Http;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Threading;

namespace StockView.Help
{
    class StockNewsHelp
    {
        private readonly static StockSupportSystemEntities db = new StockSupportSystemEntities();

        public static void initStockNews()
        {
            db.Database.ExecuteSqlCommand("delete from StockNews");
            refreshStockNews();
        }

        public static void refreshStockNews()
        {
            foreach(InfoType it in Enum.GetValues(typeof(InfoType)))
            {
                List<EastMoneyModel> news = EastMoney.GetInfo(it);
                for(int i = 0; i < news.Count(); i++)
                {
                    StockNews stockNews = new StockNews();
                    stockNews.code = news[i].code;
                    stockNews.stockname = news[i].stockName;
                    stockNews.title = news[i].title;
                    stockNews.sort = 1;
                    stockNews.Date = news[i].Date;
                    stockNews.url = news[i].url;
                    if (news[i].type == InfoType.finace.ToString())
                    {
                        stockNews.type = "融资公告";
                    }
                    else if (news[i].type == InfoType.danger.ToString())
                    {
                        stockNews.type = "风险提示";
                    }
                    else if (news[i].type == InfoType.infochange.ToString())
                    {
                        stockNews.type = "信息变更";
                    }
                    else if (news[i].type == InfoType.havestockchange.ToString())
                    {
                        stockNews.type = "持股变动";
                    }
                    else if (news[i].type == InfoType.recombo.ToString())
                    {
                        stockNews.type = "资产重组";
                       
                    }
                    UpodateOrderByKey(ref stockNews);
                    db.StockNews.Add(stockNews);
                }

                Thread.Sleep(100);
            }
            db.SaveChanges();
        }

        public static void UpodateOrderByKey(ref StockNews stockNews)
        {
            if(stockNews.type == "持股变动")
            {
                if (!stockNews.title.Contains("增持") && !stockNews.title.Contains("减持") && !stockNews.title.Contains("进展") && !stockNews.title.Contains("完毕") && !stockNews.title.Contains("结果") && !stockNews.title.Contains("完成") && !stockNews.title.Contains("终止"))
                {
                    stockNews.sort = 3;
                }
                if (stockNews.title.Contains("增持") || stockNews.title.Contains("减持"))
                {
                    var result = "";
                    try
                    {
                        result = httpRequestHelper.GetHtml(stockNews.url);
                    }
                    catch (Exception)
                    {
                        UpodateOrderByKey(ref stockNews);
                        return;
                    }

                    string pattern = @"证券代码[\s|\S]+特此公告";
                    var content = "";
                    foreach (Match match in Regex.Matches(result, pattern))
                    {
                        content = match.Value;
                    }
                    stockNews.sort = 100;
                    string[] patterns = new string[]{@"减持不超过[\n|\d|\s|,]+股", @"减持公司股份不超过[\n|\d|\s|,]+股", @"减持股份[\n|\d|\s|,]+股", @"减持公司股票数量不超过[\n|\d|\s|,]+股", @"减持股份不超过[\n|\d|\s|,]+股", @"减持本公司股份不超过[\n|\d|\s|,]+股", @"减持股份数量不超过[\n|\d|\s|,]+股",@"减持本公司股份[\n|\d|\s|,]+股",
                               @"增持[\n|\d|\s|,]+股", @"增持公司股份[\n|\d|\s|,]+股", @"增持股份[\n|\d|\s|,]+股", @"增持公司股票数量[\n|\d|\s|,]+股", @"增持股份[\n|\d|\s|,]+股", @"增持本公司股份[\n|\d|\s|,]+股", @"增持股份数量[\n|\d|\s|,]+股" };
                    string[] patterns2 = new string[]{"减持不超过", "减持公司股份不超过", "减持股份", "减持公司股票数量不超过", "减持股份不超过", "减持本公司股份不超过", "减持股份数量不超过","减持本公司股份",
                               "增持", "增持公司股份", "增持股份", "增持公司股票数量", "增持股份", "增持本公司股份", "增持股份数量" };

                    int cnd = 0;
                    foreach (string pattern2 in patterns)
                    {
                        string content2 = "";
                        foreach (Match match in Regex.Matches(content, pattern2))
                        {
                            content2 = match.Value;
                        }
                        content2 = content2.Replace(patterns2[cnd++], "").Replace("股", "").Replace("\n", "").Replace(" ", "").Replace(",", "");
                        int int_content = 100;
                        try
                        {
                            int_content = int.Parse(content2);
                        }
                        catch (Exception)
                        {

                        }
                        stockNews.sort = int_content > stockNews.sort ? int_content : stockNews.sort;
                    }

                }
                if (stockNews.title.Contains("进展"))
                {
                    stockNews.sort = 2;
                }
                if (stockNews.title.Contains("完毕") || stockNews.title.Contains("结果") || stockNews.title.Contains("完成") || stockNews.title.Contains("终止"))
                {
                    stockNews.sort = 1;
                }

            }else if(stockNews.type == "资产重组")
            {
                if (stockNews.title.Contains("进展") || stockNews.title.Contains("结果") || stockNews.title.Contains("完成") || stockNews.title.Contains("完毕") || stockNews.title.Contains("延长"))
                {
                    stockNews.sort = 1;
                }else if(stockNews.title.Contains("转让") || stockNews.title.Contains("出售") || stockNews.title.Contains("回购") || stockNews.title.Contains("收购") || stockNews.title.Contains("购买"))
                {
                    stockNews.sort = 3;
                }
                else
                {
                    stockNews.sort = 2;
                }
               
            }else if (stockNews.type == "风险提示")
            {
                if (stockNews.title.Contains("进展") || stockNews.title.Contains("终止") || stockNews.title.Contains("完成") || stockNews.title.Contains("歉"))
                {
                    stockNews.sort = 1;
                }
                else if (stockNews.title.Contains("交易异常波动"))
                {
                    var result = "";
                    try
                    {
                        result = httpRequestHelper.GetHtml(stockNews.url);
                    }
                    catch (Exception)
                    {
                        UpodateOrderByKey(ref stockNews);
                        return;
                    }

                    string pattern = @"证券代码[\s|\S]+特此公告";
                    var content = "";
                    foreach (Match match in Regex.Matches(result, pattern))
                    {
                        content = match.Value;
                    }
                    stockNews.sort = 100;
                    if (content.Contains("涨幅"))
                    {
                        stockNews.sort = 101;
                    }
                    else if (content.Contains("跌幅"))
                    {
                        stockNews.sort = 102;
                    }
                }
                else
                {
                    stockNews.sort = 2;
                }
            }



        }
    }
}
