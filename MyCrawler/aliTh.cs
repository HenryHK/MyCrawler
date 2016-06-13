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
            }
            catch (Exception e)
            {

            }

            
            //throw new NotImplementedException();
        }
    }
}
