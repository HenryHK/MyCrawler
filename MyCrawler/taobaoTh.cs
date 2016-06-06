using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCrawler 
{
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
                int num = 0; //count
                base.IniDataTable();
                base.http = new DoNet4.Utilities.HttpClientHelper(0x4e20);

                for (int i=0; i<base.keywordInfList.Count; i++)
                { //iterate the keyword list
                    base.keywordInf = base.keywordInfList[i];
                    if (string.IsNullOrEmpty(base.keywordInf.keyword))
                    {
                        continue;
                    }
                    base.updateTextBox(base.keywordInf.keyword + " 开始查询", true);
                    url = "http://www.taobao.com";

                }
            }
            catch (Exception e)
            {
                //exception handling
            }
        }


    }
}
