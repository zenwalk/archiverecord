using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ArchiveRecord
{
    public partial class frmPano : Form
    {
        public string m_strURL;

        public frmPano()
        {
            InitializeComponent();
        }

        private void frmPano_Load(object sender, EventArgs e)
        {
            webBrowser1.Navigate(m_strURL);
        }
    }
}