using Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Stock.Help
{
    public static class StockHelp
    {
        public static string StockInfoRootPath = "D:\\StockInfo";

        public static void ImportStock(int start)
        {
            int startStockid = start;
            if (start == 0) startStockid = 900000;
            int endStockid = 999999;
            List<int> ints = new List<int>();
            for (int i = startStockid; i <= endStockid; i++)
            {
                var db = new StockSupportSystemEntities();
                try
                {
                    string result = httpRequestHelper.GetRequest("http://qt.gtimg.cn/q=" + gettockId("sh", i), "", "html/text");
                    string[] results = result.Split('~');
                    if (results.Length > 1)
                    {
                        db.stock.Add(new stock()
                        {
                            id = results[2],
                            name = results[1],
                            type = 0,
                        });
                        db.SaveChanges();
                        Console.Write(i);
                    }
                    else
                    {
                        ints.Add(i);
                    }
                }
                catch (Exception e)
                {
                    ImportStock(endStockid + 1);
                }

            }

        }

        public static string gettockId(string type, int id)
        {
            return type + id.ToString("000000");
        }

        public static void ImportAllCSV()
        {
            var db = new StockSupportSystemEntities();
            var stocks = db.Database.SqlQuery<stock>("select * from stock where id<700000 and id >=600000").ToList();
            foreach (stock stk in stocks)
            {
                string code = stk.type + stk.id;
                string now_day = DateTime.Now.ToString("yyyy-MM-dd");
                string now_datetime = now_day.Split('-')[0] + now_day.Split('-')[1] + now_day.Split('-')[2];
                string url = string.Format("http://quotes.money.163.com/service/chddata.html?code={0}&start=19900101&end={1}&fields=TCLOSE;LOW;HIGH;TOPEN;LCLOSE;CHG;PCHG;VOTURNOVER", code.Trim(), now_datetime);
                HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                request.ProtocolVersion = new Version(1, 1);
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return;
                }
                // 转换为byte类型
                System.IO.Stream stream = response.GetResponseStream();
                if (Directory.Exists(StockInfoRootPath))
                {
                    if (!File.Exists(StockInfoRootPath + "\\" + code.Trim() + ".CSV"))
                    {
                        MyFileHelper.CreateFileContent(StockInfoRootPath + "\\" + code.Trim() + ".CSV", "");
                    }
                    //创建本地文件写入流
                    Stream fs = new FileStream(StockInfoRootPath + "\\" + code.Trim() + ".CSV", FileMode.Create);
                    byte[] bArr = new byte[2048];
                    int size = stream.Read(bArr, 0, (int)bArr.Length);
                    while (size > 0)
                    {
                        fs.Write(bArr, 0, size);
                        size = stream.Read(bArr, 0, (int)bArr.Length);
                    }
                    fs.Close();
                    stream.Close();
                }
            }
        }

        public static void ImpordDayDataFromSCV(MainWindow mainWindow)
        {
            var db = new StockSupportSystemEntities();
            var Stockinfos = db.StockInfo.OrderByDescending(t => t.id).FirstOrDefault();
            string startIndex = "600000";
            if (Stockinfos != null) startIndex = int.Parse(Stockinfos.id) + "";
            db.Database.ExecuteSqlCommand("delete from [StockSupportSystem].[dbo].[StockInfo] where id =" + startIndex);
            Thread.Sleep(1000);
            var stocks = db.Database.SqlQuery<stock>("select * from stock where id<604000 and id >=" + startIndex).ToList();
            int big_sum = 0;
            int FindInorPut = 0;
            foreach (stock stk in stocks)
            {
                Action UpdateNow_Code = () =>
                {
                    mainWindow.Now_Code.Text = stk.id;
                };
                FindInorPut = 0;
                mainWindow.Now_Code.Dispatcher.BeginInvoke(UpdateNow_Code);
                Thread.Sleep(100);
                string code = stk.type + stk.id;
                string filename = StockInfoRootPath + "\\" + code.Trim() + ".CSV";
                List<string> content = new List<string>();
                string line;
                StreamReader sr = new StreamReader(filename, System.Text.Encoding.Default);
                while ((line = sr.ReadLine()) != null)
                {
                    content.Add(line);
                }
                List<StockInfo> stockInfos = new List<StockInfo>();
                int small_sum = 0;
                try
                {
                    for (int i = (content.Count() - 1); i >= 1; i--)
                    {
                        string[] infos = content[i].Split(',');
                        StockInfo stockInfo = new StockInfo();
                        stockInfo.id = stk.id;
                        stockInfo.type = stk.type;
                        stockInfo.date = DateTime.Parse(infos[0]);
                        stockInfo.name = stk.name;
                        stockInfo.closeprice = decimal.Parse(infos[3]);
                        stockInfo.minprice = decimal.Parse(infos[4]);
                        stockInfo.maxprice = decimal.Parse(infos[5]);
                        stockInfo.openprice = decimal.Parse(infos[6]);
                        stockInfo.fromtclsoeprice = decimal.Parse(infos[7]);
                        if (infos[8] == "None") stockInfo.upsandowns = null;
                        else stockInfo.upsandowns = decimal.Parse(infos[8]);
                        if (infos[9] == "None") stockInfo.chg = null;
                        else stockInfo.chg = decimal.Parse(infos[9]);
                        stockInfo.volume = long.Parse(infos[10]);
                        if (stockInfos.Count() == 0)
                        {
                            stockInfo.EMA12 = stockInfo.closeprice;
                            stockInfo.EMA26 = stockInfo.closeprice;
                            stockInfo.DIF = 0;
                            stockInfo.DEA = 0;
                            stockInfo.MACD = (stockInfo.DIF - stockInfo.DEA) * 2;
                        }
                        else if (stockInfos.Count() == 1)
                        {
                            stockInfo.EMA12 = stockInfo.closeprice * (decimal)2 / (decimal)13 + (decimal)11 / (decimal)13 * stockInfos[stockInfos.Count() - 1].closeprice;
                            stockInfo.EMA12 = decimal.Round(stockInfo.EMA12.Value, 6, MidpointRounding.AwayFromZero);
                            stockInfo.EMA26 = stockInfo.closeprice * (decimal)2 / (decimal)27 + (decimal)25 / (decimal)27 * stockInfos[stockInfos.Count() - 1].closeprice;
                            stockInfo.EMA26 = decimal.Round(stockInfo.EMA26.Value, 6, MidpointRounding.AwayFromZero);
                            stockInfo.DIF = stockInfo.EMA12 - stockInfo.EMA26;
                            stockInfo.DEA = (decimal)stockInfos[stockInfos.Count() - 1].DEA * (decimal)8 / (decimal)10 + stockInfo.DIF * (decimal)2 / (decimal)10;
                            stockInfo.DEA = decimal.Round(stockInfo.DEA.Value, 6, MidpointRounding.AwayFromZero);
                            stockInfo.MACD = (stockInfo.DIF - stockInfo.DEA) * 2;
                            stockInfo.MACD = decimal.Round(stockInfo.MACD.Value, 6, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            stockInfo.EMA12 = stockInfo.closeprice * (decimal)2 / (decimal)13 + (decimal)11 / (decimal)13 * stockInfos[stockInfos.Count() - 1].EMA12;
                            stockInfo.EMA12 = decimal.Round(stockInfo.EMA12.Value, 6, MidpointRounding.AwayFromZero);
                            stockInfo.EMA26 = stockInfo.closeprice * (decimal)2 / (decimal)27 + (decimal)25 / (decimal)27 * stockInfos[stockInfos.Count() - 1].EMA26;
                            stockInfo.EMA26 = decimal.Round(stockInfo.EMA26.Value, 6, MidpointRounding.AwayFromZero);
                            stockInfo.DIF = stockInfo.EMA12 - stockInfo.EMA26;
                            stockInfo.DEA = (decimal)stockInfos[stockInfos.Count() - 1].DEA * (decimal)8 / (decimal)10 + stockInfo.DIF * (decimal)2 / (decimal)10;
                            stockInfo.DEA = decimal.Round(stockInfo.DEA.Value, 6, MidpointRounding.AwayFromZero);
                            stockInfo.MACD = (stockInfo.DIF - stockInfo.DEA) * 2;
                            stockInfo.MACD = decimal.Round(stockInfo.MACD.Value, 6, MidpointRounding.AwayFromZero);
                        }
                        stockInfo.IsMACD = 0;
                        //判断IsMACD
                        if (stockInfos.Count() > 1)
                        {
                            if (stockInfo.DIF >= stockInfo.DEA && stockInfos[stockInfos.Count() - 1].DIF > stockInfos[stockInfos.Count() - 1].DEA)
                            {
                                stockInfo.IsMACD = 2;
                            }
                        }
                        if (stockInfo.closeprice != 0)
                        {
                            stockInfos.Add(stockInfo);
                        }

                        if (stockInfos.Count() >= 5)
                        {
                            int sum_stockInfos = stockInfos.Count();
                            decimal sum_closeprices = 0;
                            for (int j = 1; j <= 5; j++)
                            {
                                sum_closeprices += stockInfos[sum_stockInfos - j].closeprice.Value;
                            }
                            stockInfo.MA5 = (decimal)sum_closeprices / (decimal)5;
                        }
                        if (stockInfos.Count() >= 6)
                        {
                            int sum_stockInfos = stockInfos.Count();
                            //前面第五天为金叉，且这几天内出现|DIF-DEA| <= 当日价格*0.0002,去掉这个金叉
                            if (stockInfos[sum_stockInfos - 6].IsMACD == 1)
                            {
                                for (int j = 1; j <= 5; j++)
                                {
                                    if (Math.Abs(stockInfos[sum_stockInfos - j].DIF.Value - stockInfos[sum_stockInfos - j].DEA.Value) <= (decimal)0.002)
                                    {
                                        db.Database.ExecuteSqlCommand("update StockInfo set IsMACD = 1 where id = '" + stockInfos[sum_stockInfos - 6].id + "' and type = " + stockInfos[sum_stockInfos - 6].type);
                                        break;
                                    }
                                }
                            }
                        }
                        if (stockInfos.Count() >= 10)
                        {
                            int sum_stockInfos = stockInfos.Count();
                            decimal sum_closeprices = 0;
                            for (int j = 1; j <= 10; j++)
                            {
                                sum_closeprices += stockInfos[sum_stockInfos - j].closeprice.Value;
                            }

                            stockInfo.MA10 = (decimal)sum_closeprices / (decimal)10;
                        }
                        if (stockInfos.Count() >= 20)
                        {
                            int sum_stockInfos = stockInfos.Count();
                            decimal sum_closeprices = 0;
                            for (int j = 1; j <= 20; j++)
                            {
                                sum_closeprices += stockInfos[sum_stockInfos - j].closeprice.Value;
                            }

                            stockInfo.MA20 = (decimal)sum_closeprices / (decimal)20;
                        }
                        if (stockInfos.Count() >= 30)
                        {
                            int sum_stockInfos = stockInfos.Count();
                            decimal sum_closeprices = 0;
                            for (int j = 1; j <= 30; j++)
                            {
                                sum_closeprices += stockInfos[sum_stockInfos - j].closeprice.Value;
                            }

                            stockInfo.MA30 = (decimal)sum_closeprices / (decimal)30;
                        }

                        if (stockInfos.Count() >= 60)
                        {
                            int sum_stockInfos = stockInfos.Count();
                            decimal sum_closeprices = 0;
                            for (int j = 1; j <= 60; j++)
                            {
                                sum_closeprices += stockInfos[sum_stockInfos - j].closeprice.Value;
                            }

                            stockInfo.MA60 = (decimal)sum_closeprices / (decimal)60;
                        }
                        //寻找买入点时
                        if (FindInorPut == 0)
                        {
                            if (stockInfos.Count() >= 4)
                            {
                                int sum_stockInfos = stockInfos.Count();
                                if (stockInfos[sum_stockInfos - 4].IsMACD == 2)
                                {
                                    int cnd = 0;
                                    for (int j = 3; j >= 1; j--)
                                    {
                                        if (stockInfos[sum_stockInfos - j].MA5 > stockInfos[sum_stockInfos - j].MA10 && stockInfos[sum_stockInfos - j].MA10 > stockInfos[sum_stockInfos - j].MA20 && stockInfos[sum_stockInfos - j].MA20 > stockInfos[sum_stockInfos - j].MA30 && stockInfos[sum_stockInfos - j].MA30 > stockInfos[sum_stockInfos - j].MA60)
                                        {
                                            if (stockInfos[sum_stockInfos - j].MA5 > stockInfos[sum_stockInfos - j - 1].MA10 && stockInfos[sum_stockInfos - j].MA20 > stockInfos[sum_stockInfos - j - 1].MA20 && stockInfos[sum_stockInfos - j].MA30 > stockInfos[sum_stockInfos - j - 1].MA30 && stockInfos[sum_stockInfos - j].MA60 > stockInfos[sum_stockInfos - j - 1].MA60)
                                            {
                                                if (stockInfos[sum_stockInfos - j].closeprice > stockInfos[sum_stockInfos - j].MA5 && stockInfos[sum_stockInfos - j].closeprice > 0)
                                                {
                                                    cnd++;
                                                }
                                            }
                                        }
                                    }
                                    if (cnd >= 3)
                                    {
                                        stockInfo.InOutPoint = 1;
                                        FindInorPut = 1;
                                    }
                                }
                            }
                        }
                        //寻找卖出点
                        if (FindInorPut == 1)
                        {
                            int sum_stockInfos = stockInfos.Count();
                            if (stockInfos[sum_stockInfos - 1].MA5 < stockInfos[sum_stockInfos - 1].MA20 && stockInfos[sum_stockInfos - 1].MA10 < stockInfos[sum_stockInfos - 1].MA20)
                            {
                                stockInfo.InOutPoint = 2;
                                FindInorPut = 0;
                            }
                        }
                        try
                        {
                            db.StockInfo.Add(stockInfo);
                            db.SaveChanges();
                        }catch(Exception e)
                        {
                            db.StockInfo.Remove(stockInfo);
                        }
                       

                        small_sum++;
                        Action UpdateProgess = () =>
                        {
                            mainWindow.progress1.Percent = (double)small_sum / (double)content.Count();
                        };
                        mainWindow.progress1.Dispatcher.BeginInvoke(UpdateProgess);
                        Thread.Sleep(100);
                    }
                }catch(Exception e)
                {
                    continue;
                }
                
                big_sum++;
                Action UpdateProgess2 = () =>
                {
                    mainWindow.progress2.Percent = (double)big_sum / (double)stocks.Count();
                };
                mainWindow.progress2.Dispatcher.BeginInvoke(UpdateProgess2);
                Thread.Sleep(100);
            }
        }

    }

}
