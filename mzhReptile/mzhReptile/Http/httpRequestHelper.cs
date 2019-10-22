using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mzhReptile.Http
{
    public static class httpRequestHelper
    {
        //发送post请求
        public static string PostRequest(string url, string data, string contentType)
        {
            //定义request并设置request的路径
            WebRequest request = WebRequest.Create(url);
            request.Method = "post";

            //初始化request参数
            string postData = data;

            //设置参数的编码格式，解决中文乱码
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            //设置request的MIME类型及内容长度
            request.ContentType = contentType;
            request.ContentLength = byteArray.Length;

            //打开request字符流
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            //定义response为前面的request响应
            WebResponse response = request.GetResponse();

            //获取相应的状态代码
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            //定义response字符流
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();//读取所有
            return responseFromServer;
        }
        //发送get请求
        public static string GetRequest(string url, string data, string contentType)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + (data == "" ? "" : "?") + data);
            request.Method = "GET";
            request.ContentType = contentType;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("GB2312"));

            string retString = myStreamReader.ReadToEnd();

            myStreamReader.Close();
            myResponseStream.Close();
            return retString;

        }


        /// <summary>
        /// HttpWebRequest 通过get
        /// </summary>
        /// <param name="url">URI</param>
        /// <returns></returns>
        public static string GetDataGetHtml(string url)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
      
                httpWebRequest.ContentType = "text/html:charset=gb2312";
                httpWebRequest.Method = "GET";
                //对发送的数据不使用缓存
                httpWebRequest.AllowWriteStreamBuffering = false;
                httpWebRequest.Timeout = 300000;
                httpWebRequest.ServicePoint.Expect100Continue = false;

                HttpWebResponse webRespon = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream webStream = webRespon.GetResponseStream();
                if (webStream == null)
                {
                    return "网络错误(Network error)：" + new ArgumentNullException("webStream");
                }
                StreamReader streamReader = new StreamReader(webStream, Encoding.UTF8);
                string responseContent = streamReader.ReadToEnd();

                webRespon.Close();
                streamReader.Close();

                return responseContent;
            }
            catch (Exception ex)
            {
                return "网络错误(Network error)：" + ex.Message;
            }
        }


        public static string GetHtml(string url)
        {
            string htmlCode;
            HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            webRequest.Timeout = 30000;
            webRequest.Method = "GET";
            webRequest.UserAgent = "Mozilla/4.0";
            webRequest.Headers.Add("Accept-Encoding", "gzip, deflate");


            HttpWebResponse webResponse = (System.Net.HttpWebResponse)webRequest.GetResponse();

            //获取目标网站的编码格式
            string contentype = webResponse.Headers["Content-Type"];
            Regex regex = new Regex("charset\\s*=\\s*[\\W]?\\s*([\\w-]+)", RegexOptions.IgnoreCase);
            if (webResponse.ContentEncoding.ToLower() == "gzip")//如果使用了GZip则先解压
            {
                using (System.IO.Stream streamReceive = webResponse.GetResponseStream())
                {
                    using (var zipStream = new System.IO.Compression.GZipStream(streamReceive, System.IO.Compression.CompressionMode.Decompress))
                    {

                        //匹配编码格式
                        if (regex.IsMatch(contentype))
                        {
                            Encoding ending = Encoding.GetEncoding(regex.Match(contentype).Groups[1].Value.Trim());
                            using (StreamReader sr = new System.IO.StreamReader(zipStream, ending))
                            {
                                htmlCode = sr.ReadToEnd();
                            }
                        }
                        else
                        {
                            using (StreamReader sr = new System.IO.StreamReader(zipStream, Encoding.UTF8))
                            {
                                htmlCode = sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            else
            {
                using (System.IO.Stream streamReceive = webResponse.GetResponseStream())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(streamReceive, Encoding.Default))
                    {
                        htmlCode = sr.ReadToEnd();
                    }
                }
            }
            return htmlCode;
        }

    }
}
