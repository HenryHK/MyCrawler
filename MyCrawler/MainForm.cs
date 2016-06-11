namespace MyCrawler
{
    using DoNet4.MyLog;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    public class MainForm : Form
    {
        private Button btn_start;
        private Button btn_stop;
        private Button btn_toexcel;
        private ComboBox cb_platform;
        private ComboBox cb_searthType;
        private ComboBox cb_sortType;
        private IContainer components = null;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private Label label1;
        private Label label2;
        private BaseWorkth th;
        private TextBox txt_beginpage;
        private TextBox txt_beginPrice;
        private TextBox txt_endpage;
        private TextBox txt_endPrice;
        private TextBox txt_keywords;
        private TextBox txt_showInf;

        public MainForm()
        {
            this.InitializeComponent();
        }

        public void AppendTextBox(string str, bool writeLog = true)
        {
            MethodInvoker method = null;
            if (writeLog)
            {
                DoNet4.MyLog.Log.WriteLog(str);
            }
            if (this.txt_showInf.InvokeRequired)
            {
                if (method == null)
                {
                    method = () => this.txt_showInf.AppendText(str + "\r\n");
                }
                this.txt_showInf.Invoke(method);
            }
            else
            { 
                this.txt_showInf.AppendText(str + "\r\n");
            }
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            BaseWorkth.DoStop = true;
        }

        private void btn_toexcel_Click(object sender, EventArgs e)
        {
            if (((this.th == null) || (this.th.OutDataTable == null)) || (this.th.OutDataTable.Rows.Count == 0))
            {
                MessageBox.Show(this, "无可导出数据", "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                string path = Application.StartupPath + @"\db\" + DateTime.Now.ToString("yyyy-MM-dd");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = path + @"\" + DateTime.Now.ToString("yyyyMMhhhhmmss") + ".xls";
                string text = ExcelHelperNew.DataTableToExcel(this.th.OutDataTable, "", path, true);
                if (text == "success")
                {
                    Process.Start(path);
                }
                else
                {
                    MessageBox.Show(text);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txt_keywords.Text.Trim()))
            {
                MessageBox.Show(this, "请输入检索关键字再查询！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                byte result = 50;
                if (!string.IsNullOrEmpty(this.txt_endpage.Text.Trim()) && byte.TryParse(this.txt_endpage.Text.Trim(), out result))
                {
                    result = (result > 50) ? ((byte) 50) : result;
                }
                bool flag = false;
                double num2 = 0.0;
                if (!string.IsNullOrEmpty(this.txt_beginPrice.Text.Trim()) && !double.TryParse(this.txt_beginPrice.Text.Trim(), out num2))
                {
                    flag = true;
                }
                double num3 = 0.0;
                if (!string.IsNullOrEmpty(this.txt_endPrice.Text.Trim()))
                {
                    if (!double.TryParse(this.txt_endPrice.Text.Trim(), out num3))
                    {
                        flag = true;
                    }
                    else if (num2 > num3)
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    MessageBox.Show(this, "请输入起始价格，无需价格则留空！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                { //begin search
                    this.btn_start.Enabled = false;
                    this.btn_stop.Enabled = true;
                    this.btn_toexcel.Enabled = false;
                    List<KeywordInf> lst = new List<KeywordInf>();
                    string[] strArray = this.txt_keywords.Text.Trim().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        KeywordInf item = new KeywordInf {
                            keyword = strArray[i],
                            beginPage = 1,
                            endPage = result,
                            beginPrice = num2,
                            endPrice = num3,
                            platform = (byte) this.cb_platform.SelectedIndex,
                            searthType = (byte) this.cb_searthType.SelectedIndex,
                            sortType = (byte) this.cb_sortType.SelectedIndex
                        };
                        lst.Add(item);
                    }
                    this.th = this.GetWorkTh((byte) this.cb_platform.SelectedIndex, lst);
                    this.th.Start(new UpdateTextBox(this.AppendTextBox));
                    while (!this.th.Stoped)
                    {
                        
                        Thread.Sleep(500);
                        Application.DoEvents();

                    }
                    Thread.Sleep(0x7d0);
                    MessageBox.Show(this, "查询完毕！", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                    //th.Stop();
                    this.btn_start.Enabled = true;
                    this.btn_stop.Enabled = false;
                    this.btn_toexcel.Enabled = true;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        public BaseWorkth GetWorkTh(byte platform, List<KeywordInf> lst)
        {
            if (platform == 0)
            {
                return new MakepoloTh(lst);
            }
            else if (platform == 1)
            {
                return new YHDTh(lst);
            }
            return new taobaoTh(lst);
            
        }

        private void InitializeComponent()
        {
            this.groupBox1 = new GroupBox();
            this.groupBox3 = new GroupBox();
            this.txt_showInf = new TextBox();
            this.groupBox2 = new GroupBox();
            this.btn_toexcel = new Button();
            this.btn_stop = new Button();
            this.btn_start = new Button();
            this.txt_endPrice = new TextBox();
            this.txt_beginPrice = new TextBox();
            this.txt_endpage = new TextBox();
            this.txt_beginpage = new TextBox();
            this.label2 = new Label();
            this.label1 = new Label();
            this.cb_sortType = new ComboBox();
            this.cb_searthType = new ComboBox();
            this.txt_keywords = new TextBox();
            this.cb_platform = new ComboBox();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            base.SuspendLayout();
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Location = new Point(8, 3);
            this.groupBox1.Margin = new Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new Padding(3, 4, 3, 4);
            this.groupBox1.Size = new Size(0x219, 0x1b2);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox3.Controls.Add(this.txt_showInf);
            this.groupBox3.Location = new Point(14, 230);
            this.groupBox3.Margin = new Padding(3, 4, 3, 4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new Padding(3, 4, 3, 4);
            this.groupBox3.Size = new Size(0x1fc, 0xc3);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.txt_showInf.Location = new Point(0x10, 0x18);
            this.txt_showInf.Margin = new Padding(3, 4, 3, 4);
            this.txt_showInf.Multiline = true;
            this.txt_showInf.Name = "txt_showInf";
            this.txt_showInf.ScrollBars = ScrollBars.Vertical;
            this.txt_showInf.Size = new Size(0x1c9, 0x9f);
            this.txt_showInf.TabIndex = 13;
            this.groupBox2.Controls.Add(this.btn_toexcel);
            this.groupBox2.Controls.Add(this.btn_stop);
            this.groupBox2.Controls.Add(this.btn_start);
            this.groupBox2.Controls.Add(this.txt_endPrice);
            this.groupBox2.Controls.Add(this.txt_beginPrice);
            this.groupBox2.Controls.Add(this.txt_endpage);
            this.groupBox2.Controls.Add(this.txt_beginpage);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.cb_sortType);
            this.groupBox2.Controls.Add(this.cb_searthType);
            this.groupBox2.Controls.Add(this.txt_keywords);
            this.groupBox2.Controls.Add(this.cb_platform);
            this.groupBox2.Location = new Point(14, 13);
            this.groupBox2.Margin = new Padding(3, 4, 3, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new Padding(3, 4, 3, 4);
            this.groupBox2.Size = new Size(0x1fc, 0xd6);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.btn_toexcel.Location = new Point(0xed, 0x9f);
            this.btn_toexcel.Margin = new Padding(3, 4, 3, 4);
            this.btn_toexcel.Name = "btn_toexcel";
            this.btn_toexcel.Size = new Size(0x52, 40);
            this.btn_toexcel.TabIndex = 12;
            this.btn_toexcel.Text = "导出excel";
            this.btn_toexcel.UseVisualStyleBackColor = true;
            this.btn_toexcel.Click += new EventHandler(this.btn_toexcel_Click);
            this.btn_stop.Location = new Point(0x7e, 0x9f);
            this.btn_stop.Margin = new Padding(3, 4, 3, 4);
            this.btn_stop.Name = "btn_stop";
            this.btn_stop.Size = new Size(0x52, 40);
            this.btn_stop.TabIndex = 11;
            this.btn_stop.Text = "停止";
            this.btn_stop.UseVisualStyleBackColor = true;
            this.btn_stop.Click += new EventHandler(this.btn_stop_Click);
            this.btn_start.Location = new Point(0x10, 0x9f);
            this.btn_start.Margin = new Padding(3, 4, 3, 4);
            this.btn_start.Name = "btn_start";
            this.btn_start.Size = new Size(0x52, 40);
            this.btn_start.TabIndex = 10;
            this.btn_start.Text = "开启检索";
            this.btn_start.UseVisualStyleBackColor = true;
            this.btn_start.Click += new EventHandler(this.button1_Click);
            this.txt_endPrice.Location = new Point(0x18e, 0x6a);
            this.txt_endPrice.Margin = new Padding(3, 4, 3, 4);
            this.txt_endPrice.Name = "txt_endPrice";
            this.txt_endPrice.Size = new Size(0x3f, 0x17);
            this.txt_endPrice.TabIndex = 9;
            this.txt_beginPrice.Location = new Point(0x139, 0x6a);
            this.txt_beginPrice.Margin = new Padding(3, 4, 3, 4);
            this.txt_beginPrice.Name = "txt_beginPrice";
            this.txt_beginPrice.Size = new Size(0x3f, 0x17);
            this.txt_beginPrice.TabIndex = 8;
            this.txt_endpage.Location = new Point(0x18e, 0x45);
            this.txt_endpage.Margin = new Padding(3, 4, 3, 4);
            this.txt_endpage.Name = "txt_endpage";
            this.txt_endpage.Size = new Size(0x3f, 0x17);
            this.txt_endpage.TabIndex = 7;
            this.txt_endpage.Text = "50";
            this.txt_beginpage.Location = new Point(0x139, 0x45);
            this.txt_beginpage.Margin = new Padding(3, 4, 3, 4);
            this.txt_beginpage.Name = "txt_beginpage";
            this.txt_beginpage.Size = new Size(0x3f, 0x17);
            this.txt_beginpage.TabIndex = 6;
            this.txt_beginpage.Text = "1";
            this.label2.AutoSize = true;
            this.label2.Location = new Point(0xea, 0x6d);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0x38, 0x11);
            this.label2.TabIndex = 5;
            this.label2.Text = "价格区间";
            this.label1.AutoSize = true;
            this.label1.Location = new Point(0xea, 0x48);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x38, 0x11);
            this.label1.TabIndex = 4;
            this.label1.Text = "起始页数";
            this.cb_sortType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cb_sortType.FormattingEnabled = true;
            this.cb_sortType.Items.AddRange(new object[] { "综合排序" });
            this.cb_sortType.Location = new Point(0x155, 0x18);
            this.cb_sortType.Margin = new Padding(3, 4, 3, 4);
            this.cb_sortType.Name = "cb_sortType";
            this.cb_sortType.Size = new Size(0x4c, 0x19);
            this.cb_sortType.TabIndex = 3;
            this.cb_searthType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cb_searthType.FormattingEnabled = true;
            this.cb_searthType.Items.AddRange(new object[] { "商品" });
            this.cb_searthType.Location = new Point(0x1b6, 0x18);
            this.cb_searthType.Margin = new Padding(3, 4, 3, 4);
            this.cb_searthType.Name = "cb_searthType";
            this.cb_searthType.Size = new Size(0x36, 0x19);
            this.cb_searthType.TabIndex = 2;
            this.txt_keywords.Location = new Point(0x10, 0x18);
            this.txt_keywords.Margin = new Padding(3, 4, 3, 4);
            this.txt_keywords.Multiline = true;
            this.txt_keywords.Name = "txt_keywords";
            this.txt_keywords.ScrollBars = ScrollBars.Vertical;
            this.txt_keywords.Size = new Size(0xc9, 0x76);
            this.txt_keywords.TabIndex = 0;
            this.cb_platform.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cb_platform.FormattingEnabled = true;
            this.cb_platform.Items.AddRange(new object[] { "马可波罗", "一号店", "1688", "ali express", "amazon.jp" });
            this.cb_platform.Location = new Point(0xed, 0x18);
            this.cb_platform.Margin = new Padding(3, 4, 3, 4);
            this.cb_platform.Name = "cb_platform";
            this.cb_platform.Size = new Size(0x55, 0x19);
            this.cb_platform.TabIndex = 0;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x233, 0x1bb);
            base.Controls.Add(this.groupBox1);
            this.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 0x86);
            base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            base.Margin = new Padding(3, 4, 3, 4);
            base.Name = "MainForm";
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "数据采集工具V1.0";
            base.FormClosed += new FormClosedEventHandler(this.MainForm_FormClosed);
            base.Load += new EventHandler(this.MainForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            base.ResumeLayout(false);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.cb_platform.SelectedIndex = 0;
            this.cb_searthType.SelectedIndex = 0;
            this.cb_sortType.SelectedIndex = 0;
            this.txt_beginpage.ReadOnly = true;
            this.btn_stop.Enabled = false;
        }
    }
}

