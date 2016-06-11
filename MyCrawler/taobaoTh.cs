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

    class taobaoTh : BaseWorkth
    {

        //construct
        public taobaoTh(List<KeywordInf> list)
        {
            base.keywordInfList = list;
        }

        public string innerHtml { get; private set; }

        //excution part
        public override void Excute()
        {
            //TODO: implement the excute function
            try
            {
                DateTime begin = DateTime.Now;
                string url = string.Empty;
                string refererUrl = string.Empty;
                string text = string.Empty;
                string s = String.Empty;
                int num = 0; //count
                base.IniDataTable();
                base.http = new DoNet4.Utilities.HttpClientHelper(0x4e20);

                for (int i = 0; i < base.keywordInfList.Count; i++)
                { //iterate the keyword list
                    base.keywordInf = base.keywordInfList[i];
                    if (string.IsNullOrEmpty(base.keywordInf.keyword))
                    {
                        continue;
                    }
                    base.updateTextBox(base.keywordInf.keyword + " 开始查询", true);
                    url = "http://1688.com";
                    byte retry = 0;
                    while (true)
                    {
                        if (base.http.Get(url).IndexOf("阿里巴巴1688.com - 全球领先的采购批发平台") != -1)
                        {
                            goto Label_search_taobao;
                        }
                        retry = (byte)(retry + 1);
                        if (retry >= 3) { break; }
                        base.updateTextBox("阿里巴巴主页打开失败，重试： " + retry.ToString(), true);
                    }
                    num++;
                    continue;
                    Label_search_taobao:
                    Thread.Sleep(0x7d0);
                    System.Text.Encoding gb2312 = System.Text.Encoding.GetEncoding("gb2312");
                    url = "https://s.1688.com/selloffer/offer_search.htm?keywords=" + HttpUtility.UrlEncode(base.keywordInf.keyword, gb2312).ToUpper();//(base.keywordInf.keyword).ToString();
                    text = base.http.Get(url);
                    refererUrl = url;
                    if (text.Contains("没找到与"))
                    {
                        num++;
                        base.updateTextBox(base.keywordInf.keyword + " 没有找到相关商品", true);
                        Thread.Sleep(200);
                    }
                    else if (text.Contains("淘宝会员（仅限会员名）请在此登录")) {
                        base.updateTextBox(base.keywordInf.keyword + " 阿里巴巴要求登录", true);
                        Thread.Sleep(200);
                    }
                    else
                    {
                        HtmlDocument document = new HtmlDocument();
                        document.LoadHtml(text);
                        HtmlNode node = null;
                        int result = 0;
                        node = document.DocumentNode.SelectSingleNode("//span[@class='sm-widget-offer']");
                        if (node == null)
                        {
                            base.updateTextBox(base.keywordInf.keyword + " node为空（null）!", true);
                        }
                        else {
                            s = StrUnit.MidStrEx(node.InnerHtml, "<em>", "</em>");
                            if (!int.TryParse(s, out result))
                            {
                                base.updateTextBox(base.keywordInf.keyword + " 获取商品数量错误！ - 2", true);
                                num++;
                            }
                        }

                        s = StrUnit.MidStrEx(text, "共<em>", "</em>件");
                        
                        if (!int.TryParse(s, out result))
                        {
                            base.updateTextBox(base.keywordInf.keyword + " 获取商品数量错误！ - 2", true);
                            num++;
                        }
                        else
                        { //get the useful data, go into iterations
                          //for ()
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
                base.updateTextBox(base.keywordInf.keyword + " 已到达规定页数，结束", true);
                base.updateTextBox("共 " + base.keywordInfList.Count.ToString() + " 件商品查询完毕，其中 " + num.ToString() + "件未检索到数据", true);
                base.http.Free();
                base.Stoped = true;
            }
            catch (Exception e)
            {
                //exception handling
                Log.WriteLog("taobaoTh Excute Err:" + e.Message + " ,Err Stack:" + e.StackTrace);
            }
        }

        private string GetData(string text)
        {
            string str = string.Empty;
            innerHtml = string.Empty; //innerHtml content
            string source = string.Empty;            
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(text);
                HtmlNode node = null;
                node = document.DocumentNode.SelectSingleNode("//ul[@id='sm-offer-list']");
                if (node == null)
                {
                    base.updateTextBox("获取商品列表失败", true);
                    throw new Exception("获取商品列表失败");
                }

                foreach (string str2 in node.InnerHtml.Split(new string[] { "<li t" }, StringSplitOptions.RemoveEmptyEntries)) 
                {
                    if (str2.IndexOf("-rank") != -1)
                    {
                        source = StrUnit.MidStrEx(str2, "rank", "</li>");
                        DataRow row = base.OutDataTable.NewRow();
                        row["Platform"] = "1688.com";
                        row["Keyword"] = base.keywordInf.keyword;
                        row["ItemID"] = StrUnit.MidStrEx(source, "t-offer-id=\"", "\"");
                        row["SellerId"] = StrUnit.MidStrEx(source, "t-member-id=\"", "\"");
                        string goodInfo = string.Empty;
                        goodInfo = StrUnit.MidStrEx(source, "sm-offer-photo sw-dpl-offer-photo", "</div>");
                        row["Url"] = StrUnit.MidStrEx(goodInfo, "href=\"","\"");
                        row["Title"] = StrUnit.MidStrEx(goodInfo, "title=\"", "\"");
                        string priceInfo = string.Empty;
                        priceInfo = StrUnit.MidStrEx(source, "<div class=\"s-widget-offershopwindowprice sm-offer-price sw-dpl-offer-price\">", "</div>");
                        row["Price"] = StrUnit.MidStrEx(priceInfo, "title=\"&yen;", "\"");
                        string companyInfo = string.Empty;
                        companyInfo = StrUnit.MidStrEx(source, "<div class=\"s-widget-offershopwindowcompanyinfo sm-offer-company sw-dpl-offer-company\">", "</div>");
                        row["StoreName"] = StrUnit.MidStrEx(companyInfo, "title=\"", "\"");
                        row["StoreUrl"] = StrUnit.MidStrEx(companyInfo, "href=\"", "\"");
                        base.OutDataTable.Rows.Add(row);
                        Thread.Sleep(50);


                        if (string.IsNullOrEmpty(row["ItemID"].ToString())) {
                            throw new Exception("获取商品列表数据失败！ - 1");
                        }
                    }
                    
                }//end of info retrieving

                node = document.DocumentNode.SelectSingleNode("//a[@class='fui-current']");
                string nextPage = string.Empty;
                if (node != null)
                {
                    //innerHtml = node.InnerHtml;
                    foreach (string pageInfo in node.InnerHtml.Split(new string[] { "<a href=#" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (pageInfo.IndexOf("fui-next") != -1)
                        {
                            nextPage = StrUnit.MidStrEx(pageInfo, "data-page=\"", "\"");
                            break;
                        }

                        int resultPage = 0;
                        if (!int.TryParse(nextPage, out resultPage))
                        {
                            System.Text.Encoding gb2312 = System.Text.Encoding.GetEncoding("gb2312");
                            str = "https://s.1688.com/selloffer/offer_search.htm?keywords=" + HttpUtility.UrlEncode(base.keywordInf.keyword, gb2312).ToUpper() + "&beginPage=" + nextPage;
                            return str;
                        }
                        base.updateTextBox(base.keywordInf.keyword + " 第 " + (resultPage-1) + " 页获取完成", true);
                        if (resultPage > base.keywordInf.endPage)
                        {
                            str = string.Empty;
                        }

                    }//end of for each
                    //source = StrUnit.
                    return str;
                }

            }
            catch (Exception e)
            {
                base.updateTextBox("GetData Err:" + e.Message, true);
            }
            return str;
        }

    }
}
