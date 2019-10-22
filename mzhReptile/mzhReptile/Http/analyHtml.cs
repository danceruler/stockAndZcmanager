using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mzhReptile.Http
{
    public static class analyHtml
    {
        public static HtmlDocument GetHtmlDocument(string url)
        {
            HtmlWeb webClient = new HtmlWeb();
            //初始化文档
            return webClient.Load(url);
        } 

        public static List<string> GetContents(HtmlDocument htmlDocument,string nodePattern)
        {
            List<string> result = new List<string>();
            //查找节点
            HtmlNodeCollection titleNodes = htmlDocument.DocumentNode.SelectNodes(nodePattern);
            if (titleNodes != null)
            {
                foreach (var item in titleNodes)
                {
                    result.Add(item.InnerText);
                    //result.Add(item.Attributes["href"].Value);
                }
            }
            return result;
        }

    }
}
