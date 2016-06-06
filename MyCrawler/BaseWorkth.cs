namespace MyCrawler
{
    using DoNet4.MyLog;
    using DoNet4.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public abstract class BaseWorkth
    {
        public HttpClientHelper http;
        public KeywordInf keywordInf;
        public List<KeywordInf> keywordInfList;
        public DataTable OutDataTable;
        public Thread th;
        public UpdateTextBox updateTextBox;
        private static bool k__BackingField = false;

        protected BaseWorkth()
        {
        }

        public abstract void Excute();
        public void Free()
        {
            if (this.http != null)
            {
                this.http.Free();
            }
        }

        public void IniDataTable()
        {
            if (this.OutDataTable == null)
            {
                this.OutDataTable = new DataTable();
                this.OutDataTable.Columns.AddRange(new DataColumn[] { new DataColumn("Platform", typeof(string)), new DataColumn("Keyword", typeof(string)), new DataColumn("ItemId", typeof(string)), new DataColumn("Title", typeof(string)), new DataColumn("Url", typeof(string)), new DataColumn("Price", typeof(string)), new DataColumn("StoreName", typeof(string)), new DataColumn("StoreUrl", typeof(string)), new DataColumn("SellerId", typeof(string)) });
            }
        }

        public void Start(UpdateTextBox updateText)
        {
            this.updateTextBox = updateText;
            this.th = new Thread(new ThreadStart(this.Excute));
            this.th.IsBackground = true;
            this.th.SetApartmentState(ApartmentState.STA);
            while (this.th.IsAlive)
            {
                Thread.Sleep(50);
            }
            Log.WriteLog("Workth Start");
            this.th.Start();
        }

        public void Stop(bool ChkTh = true)
        {
            byte num = 0;
            if (ChkTh)
            {
                DoStop = true;
                while (DoStop && (num < 30))
                {
                    num = (byte) (num + 1);
                    Thread.Sleep(500);
                }
            }
            try
            {
                if (this.th.IsAlive)
                {
                    Log.WriteLog("Abort WorkTh");
                    this.th.Abort();
                    this.th.Join();
                    while (this.th.IsAlive)
                    {
                        Thread.Sleep(10);
                    }
                    this.Stoped = true;
                }
            }
            catch (Exception exception)
            {
                Log.WriteLog("Stop WorkTh Error: " + exception.Message);
            }
            Log.WriteLog("WorkTh Stoped");
        }
           
        
        public static bool DoStop
        {
            
            get
            {
                return k__BackingField;
            }
            [CompilerGenerated]
            set
            {
                k__BackingField = value;
            }
        }
        
        public bool Stoped { get; set; }
    }
}

