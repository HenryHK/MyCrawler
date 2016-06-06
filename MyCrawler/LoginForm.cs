namespace MyCrawler
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class LoginForm : Form
    {
        private Button btn_clear;
        private Button btn_login;
        private CheckBox checkBox1;
        private IContainer components = null;
        private GroupBox groupBox1;
        private Label label1;
        private Label label2;
        private TextBox txt_pwd;
        private TextBox txt_user;

        public LoginForm()
        {
            this.InitializeComponent();
        }

        private void btn_login_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txt_user.Text.Trim()) || string.IsNullOrEmpty(this.txt_pwd.Text.Trim()))
            {
                MessageBox.Show(this, "用户名或密码为空，请填写！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                string errMsg = string.Empty;
                if (!this.Login(this.txt_user.Text.Trim(), this.txt_pwd.Text.Trim(), ref errMsg))
                {
                    MessageBox.Show(this, errMsg, "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    MainForm form = new MainForm();
                    base.Hide();
                    form.ShowDialog();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.txt_user.Clear();
            this.txt_pwd.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.label1 = new Label();
            this.label2 = new Label();
            this.txt_user = new TextBox();
            this.txt_pwd = new TextBox();
            this.groupBox1 = new GroupBox();
            this.checkBox1 = new CheckBox();
            this.btn_clear = new Button();
            this.btn_login = new Button();
            this.groupBox1.SuspendLayout();
            base.SuspendLayout();
            this.label1.AutoSize = true;
            this.label1.Location = new Point(12, 0x22);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x2c, 0x11);
            this.label1.TabIndex = 0;
            this.label1.Text = "用户名";
            this.label2.AutoSize = true;
            this.label2.Location = new Point(0x18, 0x47);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0x20, 0x11);
            this.label2.TabIndex = 1;
            this.label2.Text = "密码";
            this.txt_user.Location = new Point(0x45, 0x1f);
            this.txt_user.Name = "txt_user";
            this.txt_user.Size = new Size(0x80, 0x17);
            this.txt_user.TabIndex = 2;
            this.txt_pwd.Location = new Point(0x45, 0x44);
            this.txt_pwd.Name = "txt_pwd";
            this.txt_pwd.PasswordChar = '*';
            this.txt_pwd.Size = new Size(0x80, 0x17);
            this.txt_pwd.TabIndex = 3;
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Controls.Add(this.btn_clear);
            this.groupBox1.Controls.Add(this.btn_login);
            this.groupBox1.Controls.Add(this.txt_user);
            this.groupBox1.Controls.Add(this.txt_pwd);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new Point(12, 1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(0x105, 0xb7);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = CheckState.Checked;
            this.checkBox1.Location = new Point(0x45, 0x69);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new Size(0x4b, 0x15);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.Text = "记住密码";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.btn_clear.Location = new Point(0x83, 0x88);
            this.btn_clear.Name = "btn_clear";
            this.btn_clear.Size = new Size(0x4b, 0x23);
            this.btn_clear.TabIndex = 5;
            this.btn_clear.Text = "清空";
            this.btn_clear.UseVisualStyleBackColor = true;
            this.btn_clear.Click += new EventHandler(this.button2_Click);
            this.btn_login.Location = new Point(0x1b, 0x88);
            this.btn_login.Name = "btn_login";
            this.btn_login.Size = new Size(0x4b, 0x23);
            this.btn_login.TabIndex = 0;
            this.btn_login.Text = "登录";
            this.btn_login.UseVisualStyleBackColor = true;
            this.btn_login.Click += new EventHandler(this.btn_login_Click);
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x11d, 0xc4);
            base.Controls.Add(this.groupBox1);
            this.Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point, 0x86);
            base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            base.Margin = new Padding(3, 4, 3, 4);
            base.Name = "LoginForm";
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "数据采集工具V1.0";
            base.Load += new EventHandler(this.LoginForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            base.ResumeLayout(false);
        }

        private bool Login(string userName, string pwd, ref string errMsg)
        {
            bool flag = false;
            try
            {
                if ((userName == "wuwang") || (pwd == "a123456"))
                {
                    flag = true;
                }
            }
            catch (Exception exception)
            {
                errMsg = "Login Err:" + exception.Message;
            }
            return flag;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            if (this.checkBox1.Checked)
            {
                this.txt_user.Text = "wuwang";
                this.txt_pwd.Text = "a123456";
            }
        }
    }
}

