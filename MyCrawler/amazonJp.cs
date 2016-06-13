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

    class amazonJp : BaseWorkth
    {

        int page = 1;

        public amazonJp(List<KeywordInf> list)
        {
            base.keywordInfList = list;
        }

        public string innerHtml { get; private set; }

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
                int num = 0;
                base.IniDataTable();
                base.http = new DoNet4.Utilities.HttpClientHelper(0x4e20);

                for (int i=0; i<base.keywordInfList.Count; i++)
                {
                    base.keywordInf = base.keywordInfList[i];
                    if (string.IsNullOrEmpty(base.keywordInf.keyword))
                    {
                        continue;
                    }
                    base.updateTextBox(base.keywordInf.keyword + " 开始查询", true);
                    url = "www.amazon.co.jp/s/";
                    byte retry = 0;
                    while (true)
                    {
                        if (base.http.Get(url).IndexOf("amazon.co.jp") != -1) {
                            goto Label_search_amazonJp;
                        }
                        retry = (byte)(retry + 1);
                        if (retry >= 3) { break; }
                        base.updateTextBox("Amazon.co.jp主页打开失败，重试： " + retry.ToString(), true);
                    }
                    num++;
                    continue;
                    Label_search_amazonJp:
                    Thread.Sleep(0x7d0);
                    url = "http://www.amazon.co.jp/s/field-keywords=" + base.keywordInf.keyword;
                    refererUrl = url;
                    if (text.Contains("の検索に一致する商品はありませんでした"))
                    {
                        num++;
                        base.updateTextBox(base.keywordInf.keyword + " 没有找到相关商品");
                        Thread.Sleep(200);
                    }
                    else
                    {
                        HtmlDocument document = new HtmlDocument();
                        document.LoadHtml(text);
                    }
                }

            }
            catch (Exception e)
            {
                Log.WriteLog("amazonJpTh Excute Err:" + e.Message + " ,Err Stack:" + e.StackTrace);
            }
        }
    }
}
