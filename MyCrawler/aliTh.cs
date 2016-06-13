using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    class aliTh : BaseWorkth
    {

        int page = 1;

        public aliTh(List<KeywordInf> list)
        {
            base.keywordInfList = list;
        }

        public string innerHtml { get; private set; }

        public override void Excute()
        {
            try
            {
                DateTime begin = DateTime.Now;
                string url = string.Empty;
                string refererUrl = string.Empty;
                string text = string.Empty;
                string s = string.Empty;
                int num = 0;
                base.IniDataTable();
                base.http = new DoNet4.Utilities.HttpClientHelper(0x4e20);

                for (int i = 0; i < base.keywordInfList.Count; i++)
                {
                    base.keywordInf = base.keywordInfList[i];
                    if (string.IsNullOrEmpty(base.keywordInf.keyword))
                    {
                        continue;
                    }
                    base.updateTextBox(base.keywordInf.keyword+" 开始查询", true);
                    url = "http://alibaba.com";
                    byte retry = 0;
                    while (true)
                    {
                        if (base.http.Get(url).IndexOf("Find quality Manufacturers, Suppliers, Exporters, Importers, Buyers, Wholesalers, Products and Trade Leads from our award-winning International Trade Site") != -1)
                        {
                            goto Label_search_alibaba;
                        }
                        retry = (byte)(retry + 1);
                        if (retry >= 3) { break; }
                        base.updateTextBox("Alibaba主页打开失败，重试："+retry.ToString(),true);

                    }
                    num++;
                    continue;

                    Label_search_alibaba:
                    Thread.Sleep(0x7d0);
                    url = "http://www.alibaba.com/trade/search?SearchText=" + base.keywordInf.keyword;
                    text = base.http.Get(url);
                    refererUrl = url;
                    if (text.Contains("did not match any products"))
                    {
                        num++;
                        base.updateTextBox(base.keywordInf.keyword + " 没有找到相关产品", true);
                        Thread.Sleep(200);
                    }
                    else if (text.Contains("login-form"))
                    {
                        base.updateTextBox(base.keywordInf.keyword + " Alibaba要求登陆", true);
                        Thread.Sleep(200);
                    }
                    else
                    {
                        HtmlDocument document = new HtmlDocument();
                        document.LoadHtml(text);
                        HtmlDocument node = null;
                        int result = 0;
                        //node = document.DocumentNode.SelectSingleNode();
                        //string numTip = StrUnit.MidStrEx(text, "<strong>", "</strong>");
                        //if (!int.TryParse(numTip, out result))
                        //{
                        //    base.updateTextBox(base.keywordInf.keyword + " 获取商品数量错误！ - 2", true);
                        //    num++;
                        //}
                        //else
                        //{
                        //    base.updateTextBox("共检索到" + result + "件相关商品，开始爬取",true);
                            for (string newUrl = this.GetData(text); !string.IsNullOrEmpty(newUrl); newUrl = this.GetData(text))
                            {
                                Thread.Sleep(0x7d0);
                                text = base.http.Get(newUrl, refererUrl);
                            }
                            TimeSpan span = (TimeSpan)(DateTime.Now - begin);
                            base.updateTextBox(string.Concat(new object[] { base.keywordInf.keyword, " 获取完毕,耗时：", span.TotalSeconds, "秒" }), true);
                        //}
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

            
            //throw new NotImplementedException();
        }


        //Get data
        private string GetData(string text)
        {
            string str = string.Empty;
            innerHtml = string.Empty;
            string source = string.Empty;

            try
            {

                text = StrUnit.MidStrEx(text, "\"normalList\":[", "</html>");
                foreach (string itemInfo in text.Split(new string[] { "{\"productId\":" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (itemInfo.IndexOf("productId_f") != -1)
                    {
                        string info = "\"productID\":" + itemInfo;
                        DataRow row = base.OutDataTable.NewRow();
                        row["Platform"] = "alibaba.com";
                        row["Keyword"] = base.keywordInf.keyword;
                        row["ItemID"] = StrUnit.MidStrEx(info,"\"productID\":\"","\",");
                        row["SellerID"] = StrUnit.MidStrEx(info, "supplierId\":\"", "\"");
                        row["Url"] = StrUnit.MidStrEx(info, "productHref\":\"//", "\"");
                        row["Title"] = StrUnit.MidStrEx(info, "productPuretitle\":\"", "\"");
                        row["Price"] = StrUnit.MidStrEx(info, "\"price\":\"US $", "\"");
                        row["StoreName"] = StrUnit.MidStrEx(info, "\"supplierName\":\"", "\"");
                        row["StoreUrl"] = StrUnit.MidStrEx(info, "supplierHref\":\"", "\"");
                        base.OutDataTable.Rows.Add(row);
                        Thread.Sleep(100);
                        if (string.IsNullOrEmpty(row["ItemID"].ToString()))
                        {
                            throw new Exception("获取商品列表数据失败！ - 1");
                        }
                    }
                    else
                    {
                        throw new Exception("Json data 获取错误");
                    }
                }//end of foreach

                base.updateTextBox(base.keywordInf.keyword + " 第 " + (page) + " 页获取完成", true);
                if (page >= base.keywordInf.endPage)
                {
                    str = string.Empty;
                    return str;
                }
                ++page;
                str = "http://www.alibaba.com/products/F0/" + base.keywordInf.keyword + "/" + page + ".html";
                return str;
                /*
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(text);
                HtmlNode node = null;
                node = document.DocumentNode.SelectSingleNode("//div[@data-content='abox-ProductNormalList']");
                
                if(node == null)
                {
                    base.updateTextBox("获取商品列表失败！",true);
                    throw new Exception("获取商品列表失败");
                }

                foreach (string itemInfo in node.InnerHtml.Split(new string[] { "<div class=\"m-product-item"},StringSplitOptions.None))
                {
                    if (itemInfo.IndexOf("data-role=\"")!=-1)
                    {

                    }
                }//end of foreach

                base.updateTextBox(base.keywordInf.keyword + " 第 " + (page) + " 页获取完成", true);
                if (page >= base.keywordInf.endPage)
                {
                    str = string.Empty;
                    return str;
                }
                ++page;
                str = "http://www.alibaba.com/products/F0/"+base.keywordInf.keyword+"/"+page+".html";
                return str;
                */
            }
            catch (Exception e)
            {
                base.updateTextBox("GetData Err:" + e.Message, true);
            }

            return str;

        }

    }
}
