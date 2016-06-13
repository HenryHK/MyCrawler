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

    class aeTh : BaseWorkth
    {

        int page = 1;

        public aeTh(List<KeywordInf> list)
        {
            base.keywordInfList = list;
        }

        public string innerHtml { get; private set;}

        public override void Excute()
        {
            //TODO
            try
            {

                DateTime begin = DateTime.Now;
                string url = string.Empty;
                string refererUrl = string.Empty;
                string text = string.Empty;
                string s = string.Empty;
                int num = 0;//count
                base.IniDataTable();
                base.http = new DoNet4.Utilities.HttpClientHelper(0x4e20);

                for (int i = 0; i < base.keywordInfList.Count; i++)
                {
                    base.keywordInf = base.keywordInfList[i];
                    if (string.IsNullOrEmpty(base.keywordInf.keyword))
                    {
                        continue;
                    }
                    base.updateTextBox(base.keywordInf.keyword + " 开始查询", true);
                    url = "http://aliexpress.com";
                    byte retry = 0;
                    while (true)
                    {
                        if (base.http.Get(url).IndexOf("www.aliexpress.com") != -1)
                        {
                            goto Label_begin_search;
                        }
                        retry = (byte)(retry + 1);
                        if (retry >= 3) { break; }
                        base.updateTextBox("AE主页打开失败，重试：" + retry.ToString(), true);
                    }
                    num++;
                    continue;

                    Label_begin_search:
                    Thread.Sleep(0x7d0);
                    url = "http://www.aliexpress.com/wholesale?SearchText=" + base.keywordInf.keyword;
                    text = base.http.Get(url);
                    refererUrl = url;
                    if (text.Contains("did not match any products"))
                    {
                        num++;
                        base.updateTextBox(base.keywordInf.keyword + " 没有找到相关商品", true);
                        Thread.Sleep(200);
                    }
                    else if (text.Contains("expressbuyerlogin"))
                    {
                        base.updateTextBox(base.keywordInf.keyword + " AE 要求登录", true);
                        Thread.Sleep(200);
                    }
                    else
                    {
                        HtmlDocument document = new HtmlDocument();
                        document.LoadHtml(text);
                        HtmlNode node = null;
                        int result = 0;
                        node = document.DocumentNode.SelectSingleNode("//strong[@class='search-count']");
                        if (node == null)
                        {
                            base.updateTextBox(base.keywordInf.keyword + " node为空（null）!", true);
                        }
                        else
                        {
                            s = node.InnerHtml.Replace(",","");
                            if (!int.TryParse(s, out result))
                            {
                                base.updateTextBox(base.keywordInf.keyword + " 获取商品数量错误！ - 2", true);
                                num++;
                            }
                            else
                            {
                                base.updateTextBox("共检索到" + s + " 件相关商品， 开始爬取", true);
                                for (string str6 = this.GetData(text); !string.IsNullOrEmpty(str6); str6 = this.GetData(text)) //?
                                {
                                    Thread.Sleep(0x7d0);
                                    text = base.http.Get(str6, refererUrl);
                                }
                                TimeSpan span = (TimeSpan)(DateTime.Now - begin);
                                base.updateTextBox(string.Concat(new object[] { base.keywordInf.keyword, " 获取完毕,耗时：", span.TotalSeconds, "秒" }), true);
                            }
                        }
                    }
                }
                base.updateTextBox(base.keywordInf.keyword + " 已到达规定页数，结束", true);
                base.updateTextBox("共 " + base.keywordInfList.Count.ToString() + " 件商品查询完毕，其中 " + num.ToString() + "件未检索到数据", true);
                base.http.Free();
                base.Stoped = true;
            }
            catch (Exception e)
            {
                Log.WriteLog("taobaoTh Excute Err:" + e.Message + " ,Err Stack:" + e.StackTrace);
            }

        }//end of excution

        private string GetData(string text)
        {
            string str = string.Empty;
            innerHtml = string.Empty;
            string source = string.Empty;

            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(text);
                HtmlNode node = null;
                node = document.DocumentNode.SelectSingleNode("//ul[@id='hs-below-list-items']");
                if (node == null)
                {
                    base.updateTextBox("获取商品列表失败", true);
                    
                }

                foreach (string itemInfo in node.InnerHtml.Split(new string[] {"<li qrdata"}, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (itemInfo.IndexOf("class=\"list-item") != -1)
                    {
                        //source = StrUnit；
                        DataRow row = base.OutDataTable.NewRow();
                        row["Platform"] = "Ali Express";
                        row["Keyword"] = base.keywordInf.keyword;
                        row["ItemID"] = string.Empty;
                        //
                        
                        string goodInfo = StrUnit.MidStrEx(itemInfo, "<a class=\"history-item product", ">");
                        row["Url"] = StrUnit.MidStrEx(goodInfo, "href=\"//", "\"");
                        row["Title"] = StrUnit.MidStrEx(goodInfo, "title=\"", "\"");
                        row["Price"] = StrUnit.MidStrEx(itemInfo,"itemprop=\"price\">", "</span>");

                        string storeInfo = StrUnit.MidStrEx(itemInfo, "class=\"store-name", "</div>");
                        row["StoreName"] = StrUnit.MidStrEx(storeInfo,"title=\"", "\"");
                        row["StoreUrl"] = StrUnit.MidStrEx(storeInfo,"href=\"//","\"");
                        row["SellerID"] = StrUnit.MidStrEx(storeInfo, "store/", "\"");
                        base.OutDataTable.Rows.Add(row);
                        Thread.Sleep(100);

                        if (string.IsNullOrEmpty(row["Url"].ToString()))
                        {
                            throw new Exception("获取商品列表数据失败！ -1");
                        }
                    }
                }

                base.updateTextBox(base.keywordInf.keyword + " 第 " + (page) + " 页获取完成", true);
                if (page >= base.keywordInf.endPage)
                {
                    str = string.Empty;
                    return str;
                }
                ++page;
                str = "http://www.aliexpress.com/wholesale?SearchText=" + base.keywordInf.keyword+"&page="+page;
                return str;
            }
            catch(Exception e)
            {
                base.updateTextBox("GetData Err:" + e.Message, true);
            }

            return str;
        }

        private string GetData1(string text)
        {
            string str = string.Empty;
            innerHtml = string.Empty;
            string source = string.Empty;

            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(text);
                HtmlNode node = null;
                node = document.DocumentNode.SelectSingleNode("//ul[@id='hs-below-list-items']");
                if (node == null)
                {
                    base.updateTextBox("获取商品列表失败", true);
                    throw new Exception("获取商品列表失败");
                }

                foreach (string itemInfo in node.InnerHtml.Split(new string[] { "<li qrdata" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (itemInfo.IndexOf("class=\"list-item") != -1)
                    {
                        //source = StrUnit；
                        DataRow row = base.OutDataTable.NewRow();
                        row["Platform"] = "Ali Express";
                        row["Keyword"] = base.keywordInf.keyword;
                        row["ItemID"] = string.Empty;
                        //

                        string goodInfo = StrUnit.MidStrEx(itemInfo, "<a class=\"history-item product", ">");
                        row["Url"] = StrUnit.MidStrEx(goodInfo, "href=\"//", "\"");
                        row["Title"] = StrUnit.MidStrEx(goodInfo, "title=\"", "\"");
                        row["Price"] = StrUnit.MidStrEx(itemInfo, "itemprop=\"price\">", "</span>");

                        string storeInfo = StrUnit.MidStrEx(itemInfo, "class=\"store-name", "</div>");
                        row["StoreName"] = StrUnit.MidStrEx(storeInfo, "title=\"", "\"");
                        row["StoreUrl"] = StrUnit.MidStrEx(storeInfo, "href=\"//", "\"");
                        row["SellerID"] = StrUnit.MidStrEx(storeInfo, "store/", "\"");
                        base.OutDataTable.Rows.Add(row);
                        Thread.Sleep(100);

                        if (string.IsNullOrEmpty(row["Url"].ToString()))
                        {
                            throw new Exception("获取商品列表数据失败！ -1");
                        }
                    }
                }

                base.updateTextBox(base.keywordInf.keyword + " 第 " + (page) + " 页获取完成", true);
                if (page >= base.keywordInf.endPage)
                {
                    str = string.Empty;
                    return str;
                }
                ++page;
                str = "http://www.aliexpress.com/wholesale?SearchText=" + base.keywordInf.keyword + "&page=" + page;
                return str;
            }
            catch (Exception e)
            {
                base.updateTextBox("GetData Err:" + e.Message, true);
            }

            return str;
        }

    }
}
