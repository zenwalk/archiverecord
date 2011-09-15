using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ArchiveRecord.Globe;
using System.Data.OleDb;

namespace ArchiveRecord
{
    
    public partial class frmFlash : Form
    {
        public frmFlash()
        {
            InitializeComponent();
        }

        [STAThread]
        static void Main()
        {
            ForAR.Connect_Type = 2;//初始化连接类型
            ForAR.AppIni();
            Application.Run(new frmFlash());
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.Opacity > 0)
            {
                this.Opacity = this.Opacity - 0.02;
            }
            else
            {
                this.Hide() ;
                timer1.Stop();
                frmMainNew frmmain = new frmMainNew();
                frmmain.m_frmFlash = this;
                frmmain.Show();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //OleDbConnection mycon = new OleDbConnection(ForAR.Connect_Sql);
            //mycon.Open();
            //OleDbDataAdapter da;
            //if (ForAR.Connect_Type == 1)
            //    da = ForAR.CreateCustomerAdapter(mycon, string.Format("select * from sde.Login where Name = '{0}'", textBox1.Text), "", "");
            //else
            //    da = ForAR.CreateCustomerAdapter(mycon, string.Format("select * from Login where Name = '{0}'", textBox1.Text), "", "");
            //da.SelectCommand.ExecuteNonQuery();
            //DataSet ds = new DataSet();
            //int nQueryCount = da.Fill(ds);
            //if (nQueryCount < 1)
            //{
            //    MessageBox.Show("用户名不存在，请重新输入\n", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
            //else if (maskedTextBox1.Text.ToLower() == ds.Tables[0].Rows[0][2].ToString().ToLower())
            //{
            //    ForAR.Login_name = textBox1.Text;
            //    ForAR.Login_Operation = ds.Tables[0].Rows[0][3].ToString();
                timer1.Start();
            //    //ForAR.WritePrivateProfileString("Businfo", "LoginName", textBox1.Text, Application.StartupPath + "\\Businfo.ini");
            //}
            //else
            //{
            //    MessageBox.Show("密码错误，请重新输入\n", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
            //mycon.Close();
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmFlash_Load(object sender, EventArgs e)
        {
            //textBox1.Text = ForAR.GetProfileString("Businfo", "LoginName", Application.StartupPath + "\\Businfo.ini");
        }
    }
}