namespace MyCrawler
{
    using DoNet4.MyLog;
    using DoNet4.Utilities;
    using HtmlAgilityPack;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Web;

    public class YHDTh : BaseWorkth
    {
        public YHDTh(List<KeywordInf> lst)
        {
            base.keywordInfList = lst;
        }

        public string innerHtml { get; private set; }

        public override void Excute()
        {
            try
            {
                DateTime now = DateTime.Now; //mark of time
                string url = string.Empty;
                string refererUrl = string.Empty; //use refererURLinstead if url in makepolo
                string text = string.Empty; //crawled content
                string s = string.Empty;
                int num = 0;
                base.IniDataTable(); //InitDataTable from the base
                base.http = new HttpClientHelper(0x4e20); //new a HttpClientHelper
                for (int i = 0; i < base.keywordInfList.Count; i++)
                { //go iterations
                    base.keywordInf = base.keywordInfList[i]; //setup the keyword during this iteration
                    if (string.IsNullOrEmpty(base.keywordInf.keyword)) //skip null or empty keyword
                    {
                        continue;
                    }
                    base.updateTextBox(base.keywordInf.keyword + " 开始查询", true);
                    url = "http://www.yhd.com?forceId=6";
                    byte num3 = 0;
                    while (true) //open yhd
                    {
                        if (base.http.Get(url).IndexOf("<title>网上超市1号店，省力省钱省时间</title>") != -1) //open success
                        {
                            goto Label_0107;
                        }
                        num3 = (byte) (num3 + 1); //retry, max: 3 times
                        if (num3 >= 3)
                        {
                            break;
                        }
                        base.updateTextBox("一号店打开主页失败，重试：" + num3.ToString(), true);
                    }
                    num++;
                    continue;
                Label_0107:
                    Thread.Sleep(0x7d0);
                    url = "http://search.yhd.com/c0-0/k" + HttpUtility.UrlEncode(StrUnit.UrlEncode(base.keywordInf.keyword, true).ToUpper()) + "/null/"; //form of url
                    text = base.http.Get(url, "http://www.yhd.com");
                    //Console.WriteLine(text);
                    refererUrl = url; //copy search url to referer url
                    if (text.Contains("id=\"plist")) // id = "plist
                    {
                        //do nothing?
                    }
                    if (text.Contains("抱歉~没有找到"))
                    {
                        num++; //count the search num
                        base.updateTextBox(base.keywordInf.keyword + " 没有找到相关商品", true);
                        Thread.Sleep(200);
                    }
                    else if (!text.Contains("1号店为您找到"))
                    {
                        num++;
                        base.updateTextBox(base.keywordInf.keyword + " 返回页面错误", true);
                        Log.WritrTemp(text, "未知返回页面");
                    }
                    else
                    {
                        HtmlDocument document = new HtmlDocument(); //html document object
                        document.LoadHtml(text); //load the content got
                        HtmlNode node = null; //html node init
                        node = document.DocumentNode.SelectSingleNode("//small[@class='result_count']"); //select  'result_count'
                        if (node == null)
                        {
                            num++;
                            base.updateTextBox(base.keywordInf.keyword + " 获取商品数量错误1", false);
                            Log.WriteLog(base.keywordInf.keyword + " 节点不存在：//small[@class='result_count']");
                        }
                        else
                        {
                            s = StrUnit.MidStrEx(node.InnerText, "共", "条");
                            int result = 0;
                            if (!int.TryParse(s, out result))
                            {
                                base.updateTextBox(base.keywordInf.keyword + " 获取商品数量错误2", true);
                                num++;
                            }
                            else
                            {
                                for (string str6 = this.GetData(text); !string.IsNullOrEmpty(str6); str6 = this.GetJsonData(text)) //?
                                {
                                    Thread.Sleep(0x7d0);
                                    text = base.http.Get(str6, refererUrl);
                                }
                                TimeSpan span = (TimeSpan) (DateTime.Now - now);
                                base.updateTextBox(string.Concat(new object[] { base.keywordInf.keyword, " 获取完毕,耗时：", span.TotalSeconds, "秒" }), true);
                            }
                        }
                    }
                }
                base.updateTextBox(base.keywordInf.keyword + " 已到规定页数，结束", true);
                base.updateTextBox("共 " + base.keywordInfList.Count.ToString() + " 件商品查询完毕，其中 " + num.ToString() + "件未检索到数据", true);
                base.http.Free();
                base.Stoped = true;
            }
            catch (Exception exception)
            {
                Log.WriteLog("YHDTh Excute Err:" + exception.Message + " ,Err Stack:" + exception.StackTrace);
            }
        }

        private string GetData(string text)
        {
            string str = string.Empty;
            innerHtml = string.Empty;
            string sourse = string.Empty;
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(text);
                HtmlNode node = null;
                node = document.DocumentNode.SelectSingleNode("//div[@id='itemSearchList']"); //got div id == 'itemSearchList'
                if (node == null)
                {
                    throw new Exception("获取商品列表数据失败");
                }
                foreach (string str2 in node.InnerHtml.Split(new string[] { "<div class=\"mod_search_pro\"" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (str2.IndexOf("<div class=\"proImg\"") != -1) //contains <div class = "proImg"
                    {
                        sourse = StrUnit.MidStrEx(str2, "<div class=\"proImg\"", "</div>"); //get the mid text
                        DataRow row = base.OutDataTable.NewRow(); //out data row
                        row["Platform"] = "yhd.com";
                        row["Keyword"] = base.keywordInf.keyword;
                        row["ItemId"] = StrUnit.MidStrEx(sourse, "pmid=\"", "\"");
                        if (string.IsNullOrEmpty(row["ItemId"].ToString()))
                        {
                            throw new Exception("获取商品列表数据失败1");
                        }

                        sourse = StrUnit.MidStrEx(sourse, "href=\"", "\"");
                        sourse = sourse.StartsWith("//") ? ("http:" + sourse) : sourse; //get the url
                        row["Url"] = sourse;
   
                        sourse = StrUnit.MidStrEx(str2, "<p class=\"storeName", "</p>");
                        if (sourse.IndexOf("1号店自营") == -1) //get the store info
                        {
                            row["StoreName"] = StrUnit.MidStrEx(sourse, "title=\"", "\"");
                            sourse = StrUnit.MidStrEx(sourse, "href=\"", "\"");
                            sourse = sourse.StartsWith("//") ? ("http:" + sourse) : sourse;
                            row["StoreUrl"] = sourse;
                            row["SellerId"] = "m-" + StrUnit.MidStrEx(sourse, "merchantid=\"", "\"");
                        }
                        else
                        {
                            row["StoreName"] = "1号店自营";
                            row["StoreUrl"] = string.Empty;
                            row["SellerId"] = string.Empty;
                        }

                        sourse = StrUnit.MidStrEx(str2, "<p class=\"proName clearfix\">", "</p>");
                        row["Title"] = StrUnit.MidStrEx(sourse, "title=\"", "\""); //get title
                        if (string.IsNullOrEmpty(row["Url"].ToString()))
                        {
                            row["Url"] = StrUnit.MidStrEx(sourse, "href=\"", "\"");
                        }
                        sourse = StrUnit.MidStrEx(str2, "<p class=\"proPrice\">", "</p>");
                        row["Price"] = StrUnit.MidStrEx(sourse, "yhdprice=\"", "\""); //get price
                        base.OutDataTable.Rows.Add(row);
                        Thread.Sleep(50);
                    }
                }
                node = document.DocumentNode.SelectSingleNode("//div[@class='search_select_page']"); //get the div node of class = 'search_select_page', update current page info
                if (node != null)
                {
                    innerHtml = node.InnerHtml;
                    sourse = StrUnit.MidStrEx(innerHtml, "class=\"next iconSearch\"", "</a>");
                    if (!string.IsNullOrEmpty(sourse))
                    {
                        str = StrUnit.MidStrEx(sourse, "url=\"", "\"") + StrUnit.MidStrEx(sourse, "href=\"", "\"");
                    }
                    sourse = StrUnit.MidStrEx(StrUnit.MidStrEx(innerHtml, "id=\"currentPageNum\"", ">"), "value=\"", "\"");
                    if (string.IsNullOrEmpty(sourse))
                    {
                        sourse = StrUnit.MidStrEx(innerHtml, "pageSearch(", ",");
                    }
                    int result = 0;
                    if (!int.TryParse(sourse, out result))
                    {
                        return str;
                    }
                    base.updateTextBox(base.keywordInf.keyword + " 第 " + sourse + " 页获取完成", true);
                    if (result >= base.keywordInf.endPage)
                    {
                        str = string.Empty;
                        //base.updateTextBox(base.keywordInf.keyword + " 已到规定页数，结束", true);
                    }
                }
                return str;
            }
            catch (Exception exception)
            {
                base.updateTextBox("GetData Err:" + exception.Message, true);
            }
            return str;
        }

        private string GetJsonData(string text)
        {
            string str = string.Empty;
            innerHtml = string.Empty;
            string html = string.Empty;
            string sourse = string.Empty;
            try
            {
                if (text.IndexOf("itemSearchList") == -1)
                {
                    throw new Exception("未知页面内容");
                }
                html = text.Replace(@"\r\n", "").Replace("\\\"", "\"");
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(html);
                HtmlNode node = null;
                node = document.DocumentNode.SelectSingleNode("//div[@id='itemSearchList']");
                if (node == null)
                {
                    throw new Exception("获取商品列表数据失败");
                }
                foreach (string str2 in node.InnerHtml.Split(new string[] { "<div class=\"mod_search_pro\"" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (str2.IndexOf("<div class=\"proImg\"") != -1)
                    {
                        sourse = StrUnit.MidStrEx(str2, "<div class=\"proImg\"", "<p class=\"proPrice\">");
                        DataRow row = base.OutDataTable.NewRow();
                        row["Platform"] = "yhd.com";
                        row["Keyword"] = base.keywordInf.keyword;
                        row["ItemId"] = StrUnit.MidStrEx(sourse, "pmid=\"", "\"");
                        if (string.IsNullOrEmpty(row["ItemId"].ToString()))
                        {
                            throw new Exception("获取商品列表数据失败1");
                        }
                        sourse = StrUnit.MidStrEx(sourse, "href=\"", "\"");
                        sourse = sourse.StartsWith("//") ? ("http:" + sourse) : sourse;
                        row["Url"] = sourse;
                        if (str2.IndexOf("1号店自营") == -1)
                        {
                            sourse = StrUnit.MidStrEx(str2, "<p class=\"storeName", "onclick=");
                            row["StoreName"] = StrUnit.MidStrEx(sourse, "title=\"", "\"");
                            row["StoreUrl"] = StrUnit.MidStrEx(sourse, "href=\"", "\"");
                            row["SellerId"] = "m-" + StrUnit.MidStrEx(sourse, "merchantid=\"", "\"");
                        }
                        else
                        {
                            row["StoreName"] = "1号店自营";
                            row["StoreUrl"] = string.Empty;
                            row["SellerId"] = string.Empty;
                        }
                        sourse = StrUnit.MidStrEx(str2, "<p class=\"proName clearfix\">", "onclick=");
                        row["Title"] = StrUnit.MidStrEx(sourse, "title=\"", "\"");
                        if (string.IsNullOrEmpty(row["Url"].ToString()))
                        {
                            row["Url"] = StrUnit.MidStrEx(sourse, "href=\"", "\"");
                        }
                        row["Price"] = StrUnit.MidStrEx(str2, "yhdprice=\"", "\"");
                        base.OutDataTable.Rows.Add(row);
                    }
                }
                node = document.DocumentNode.SelectSingleNode("//div[@class='search_select_page']");
                if (node != null)
                {
                    innerHtml = node.InnerHtml;
                    sourse = StrUnit.MidStrEx(innerHtml, "class=\"next iconSearch\"", "</a>");
                    if (!string.IsNullOrEmpty(sourse))
                    {
                        str = StrUnit.MidStrEx(sourse, "url=\"", "\"") + StrUnit.MidStrEx(sourse, "href=\"", "\"");
                    }
                    sourse = StrUnit.MidStrEx(StrUnit.MidStrEx(innerHtml, "id=\"currentPageNum\"", ">"), "value=\"", "\"");
                    if (string.IsNullOrEmpty(sourse))
                    {
                        sourse = StrUnit.MidStrEx(innerHtml, "pageSearch(", ",");
                    }
                    int result = 0;
                    if (!int.TryParse(sourse, out result))
                    {
                        return str;
                    }
                    base.updateTextBox(base.keywordInf.keyword + " 第 " + sourse + " 页获取完成", true);
                    if (result >= base.keywordInf.endPage)
                    {
                        str = string.Empty;
                    }
                    //base.updateTextBox(base.keywordInf.keyword + " 已到规定页数，结束", true);
                }
                return str;
            }
            catch (Exception exception)
            {
                base.updateTextBox("GetJsonData Err:" + exception.Message, true);
            }
            return str;
        }
    }
}

