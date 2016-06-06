namespace MyCrawler
{
    using DoNet4.MyLog;
    using DoNet4.Utilities;
    using HtmlAgilityPack;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using System.Threading;
    using System.Web;

    public class MakepoloTh : BaseWorkth
    {
        public MakepoloTh(List<KeywordInf> lst)
        {
            base.keywordInfList = lst;
        }

        public override void Excute()
        {
            try
            {
                DateTime now = DateTime.Now;
                string url = string.Empty;
                string html = string.Empty;
                int num = 0;
                base.IniDataTable();
                base.http = new HttpClientHelper(0x4e20);
                for (int i = 0; i < base.keywordInfList.Count; i++)
                {
                    TimeSpan span; //?
                    bool flag;
                    base.keywordInf = base.keywordInfList[i];
                    if (string.IsNullOrEmpty(base.keywordInf.keyword))
                    {
                        continue;
                    }
                    base.updateTextBox(base.keywordInf.keyword + " 开始查询", true);
                    Random random = new Random();
                    url = "http://caigou.makepolo.com/spc_new.php?search_flag=" + ((byte) random.Next(11)).ToString() + "&q=" + HttpUtility.UrlEncode(base.keywordInf.keyword, Encoding.UTF8);
                    html = base.http.Get(url, "http://www.makepolo.com");
                    if (html.Contains(">抱歉，没有找到<span>"))
                    {
                        num++;
                        base.updateTextBox(base.keywordInf.keyword + " 没有找到相关商品", true);
                        Thread.Sleep(200);
                        continue;
                    }
                    if (html.Contains("error|") && html.Contains("403"))
                    {
                        num++;
                        base.updateTextBox(base.keywordInf.keyword + " 找不到页面，请检查是否有该商品", true);
                        Thread.Sleep(200);
                        continue;
                    }
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(html);
                    HtmlNode node = document.DocumentNode.SelectSingleNode("//span[@class='f_red']");
                    if (node == null)
                    {
                        base.updateTextBox(base.keywordInf.keyword + " 获取商品数量错误", true);
                        continue;
                    }
                    base.updateTextBox(base.keywordInf.keyword + " 共找到 " + node.InnerText + " 件商品", true);
                    string str5 = string.Empty;
                    goto Label_0258;
                Label_0224:
                    str5 = this.GetData(html);
                    if (string.IsNullOrEmpty(str5))
                    {
                        goto Label_025D;
                    }
                    Thread.Sleep(100);
                    html = base.http.Get(str5);
                Label_0258:
                    flag = true;
                    goto Label_0224;
                Label_025D:
                    span = (TimeSpan) (DateTime.Now - now);
                    base.updateTextBox(string.Concat(new object[] { base.keywordInf.keyword, " 获取完毕,耗时：", span.TotalSeconds, "秒" }), true);
                }
                base.updateTextBox("共 " + base.keywordInfList.Count.ToString() + " 件商品查询完毕，其中 " + num.ToString() + "件未检索到数据", true);
                base.http.Free();
                base.Stoped = true;
            }
            catch (Exception exception)
            {
                Log.WriteLog("MakepoloTh Excute Err:" + exception.Message + " ,Err Stack:" + exception.StackTrace);
            }
        }

        public string GetData(string ss)
        {
            string nextUrl = string.Empty;
            string sourse = "";
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(ss);
                HtmlNode node = document.DocumentNode.SelectSingleNode("//div[@class='nextpage']");
                if (node.SelectSingleNode("//span[@class='current']") != null)
                {
                    byte result = 0;
                    if (!byte.TryParse(node.SelectSingleNode("//span[@class='current']").InnerText.Trim(), out result))
                    {
                        base.updateTextBox(base.keywordInf.keyword + " 获取当前页面出错", true);
                    }
                    if (base.keywordInf.endPage < result)
                    {
                        throw new Exception("已到规定获取的页数，退出");
                    }
                    base.updateTextBox(base.keywordInf.keyword + " 正在获取第 " + result.ToString() + " 页", true);
                }
                if (node.InnerHtml.IndexOf("下一页") > -1)
                {
                    nextUrl = this.GetNextUrl(node.InnerHtml);
                }
                node = document.DocumentNode.SelectSingleNode("//div[@class='s_product']");
                if (node == null)
                {
                    return nextUrl;
                }
                string[] strArray = node.InnerHtml.Split(new string[] { "<div class=\"s_product_item\">" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (BaseWorkth.DoStop)
                    {
                        return nextUrl;
                    }
                    if (strArray[i].Contains("<div class=\"s_product_info\">"))
                    {
                        string str3 = strArray[i].ToString();
                        DataRow row = base.OutDataTable.NewRow();
                        row["Platform"] = "makepolo.com";
                        row["Url"] = StrUnit.MidStrEx(str3, "href=\"", "\"");
                        row["ItemId"] = StrUnit.MidStrEx(row["Url"].ToString(), "product-detail/", ".");
                        row["Keyword"] = base.keywordInf.keyword;
                        row["Title"] = StrUnit.MidStrEx(str3, ".html\">", "</a>").Trim();
                        sourse = StrUnit.MidStrEx(str3, "报价:<strong>", "</strong>");
                        if (sourse.Trim() == "")
                        {
                            sourse = StrUnit.MidStrEx(str3, "报价:", "</");
                        }
                        row["Price"] = sourse;
                        sourse = StrUnit.MidStrEx(str3, "class=\"s_product_contact plistk\">", "<span");
                        row["StoreName"] = StrUnit.MidStrEx(sourse, "target=\"_blank\">", "</a>").Trim();
                        row["StoreUrl"] = StrUnit.MidStrEx(sourse, "<a href=\"", "\"");
                        row["SellerId"] = StrUnit.MidStrEx(row["StoreUrl"].ToString(), "://", ".");
                        Thread.Sleep(50);
                        base.OutDataTable.Rows.Add(row);
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception.Message != "已到规定获取的页数，退出")
                {
                    base.updateTextBox("GetData取信息异常：" + exception.Message, true);
                }
            }
            return nextUrl;
        }

        private string GetNextUrl(string text)
        {
            string str = string.Empty;
            string[] strArray = text.Split(new string[] { "</a>" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strArray.Length; i++)
            {
                if (strArray[i].IndexOf("下一页") > -1)
                {
                    return StrUnit.MidStrEx(strArray[i], "href=\"", "\"").Trim();
                }
            }
            return str;
        }
    }
}

