using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ArchiveRecord.Globe;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;

namespace ArchiveRecord
{
    public partial class frmAttribute : Form
    {
        public Int32 m_nCurRowIndex;
        bool m_bEdit = false;
        public IFeature m_pCurFeature;
        public List<IFeature> m_featureCollection = new List<IFeature>();
        DateTimePicker m_dtp = new DateTimePicker();  //这里实例化一个DateTimePicker控件
        Rectangle m_Rectangle;

        public frmAttribute()
        {
            InitializeComponent();
            dataGridView1.Controls.Add(m_dtp);  //把时间控件加入DataGridView
            m_dtp.Visible = false;  //先不让它显示
            m_dtp.Format = DateTimePickerFormat.Custom;  //设置日期格式为2010-08-05
            m_dtp.TextChanged += new EventHandler(dtp_TextChange); //为时间控件加入事件dtp_TextChange
        }

        /// <summary>
        /// 时间控件选择时间时消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtp_TextChange(object sender, EventArgs e)
        {
            dataGridView1.CurrentCell.Value = m_dtp.Text.ToString();  //时间控件选择时间时，就把时间赋给所在的单元格
            m_dtp.Visible = false;
        }

        private void frmAttribute_Load(object sender, EventArgs e)
        {
            if (m_featureCollection.Count < 1)
                RefreshGrid();
            else
                RefreshSelectGrid();
        }

        public void RefreshGrid()
        {
            ForAR.ArchiveFill(dataGridView1, ForAR.GridSetType.Archive_Fill, "",new string[]{""});
        }

        public void RefreshSelectGrid()
        {
            String strInPara = "";
            if (m_featureCollection.Count > 0)
            {
                foreach (IFeature pFeature in m_featureCollection)
                {
                    strInPara = String.Format("{0}{1},", strInPara, pFeature.get_Value(pFeature.Fields.FindField("OBJECTID")).ToString());
                }
                ForAR.ArchiveFill(dataGridView1, ForAR.GridSetType.Archive_Fill, string.Format(" WHERE (OBJECTID IN ({0}))", strInPara.Substring(0, strInPara.Length - 1)), new string[] { "" });
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (m_bEdit)
            {
                dataGridView1.EndEdit();
                foreach (DataGridViewColumn eColumn in dataGridView1.Columns)
                {
                    eColumn.ReadOnly = true;
                }
                m_bEdit = false;
                button1.Text = "开始编辑";
                m_dtp.Visible = false;//结束编辑时隐藏日期按钮
            }
            else
            {
                if (MessageBox.Show("是否要修改属性！", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    button1.Text = "保存修改";
                    foreach (DataGridViewColumn eColumn in dataGridView1.Columns)
                    {
                        eColumn.ReadOnly = false;
                    }
                    m_bEdit = true;
                }
                else
                {
                    m_bEdit = false;
                }
            }
        }

        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            m_pCurFeature = null;
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.RowIndex <= dataGridView1.Rows.Count)
            {
                m_nCurRowIndex = e.RowIndex;
                dataGridView1.Rows[m_nCurRowIndex].Selected = true;
                //contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
                m_pCurFeature = EngineFuntions.GetOneSeartchFeature(EngineFuntions.m_Layer_BusStation, "OBJECTID = " + dataGridView1.Rows[m_nCurRowIndex].Cells["OBJECTID"].Value.ToString());

               
            }

            if (m_bEdit && e.Button == MouseButtons.Left && e.RowIndex >= 0 && e.RowIndex <= dataGridView1.Rows.Count)
            {
                if (e.ColumnIndex == dataGridView1.Columns["开始日期"].Index
                       || e.ColumnIndex == dataGridView1.Columns["结束日期"].Index
                       || e.ColumnIndex == dataGridView1.Columns["申请时间"].Index
                       || e.ColumnIndex == dataGridView1.Columns["归档日期"].Index)
                {
                    m_Rectangle = dataGridView1.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true); //得到所在单元格位置和大小
                    m_dtp.Size = new Size(m_Rectangle.Width, m_Rectangle.Height); //把单元格大小赋给时间控件
                    m_dtp.Location = new System.Drawing.Point(m_Rectangle.X, m_Rectangle.Y); //把单元格位置赋给时间控件
                    m_dtp.Visible = true;  //可以显示控件了
                }
                else
                    m_dtp.Visible = false;
            }
          
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (m_bEdit)
            {
                m_nCurRowIndex = e.RowIndex;
                m_pCurFeature = EngineFuntions.GetOneSeartchFeature(EngineFuntions.m_Layer_BusStation, "OBJECTID = " + dataGridView1.Rows[m_nCurRowIndex].Cells["OBJECTID"].Value.ToString());
                if (m_pCurFeature != null)
                {
                    string strName = dataGridView1.Columns[e.ColumnIndex].Name;
                    int nField = m_pCurFeature.Fields.FindField(strName);
                    if (strName == "开始日期" || strName == "结束日期" || strName == "申请时间" || strName == "归档日期")
                    {
                        m_pCurFeature.set_Value(nField, Convert.ToDateTime(dataGridView1.Rows[m_nCurRowIndex].Cells[e.ColumnIndex].Value));
                    }
                    else
                    m_pCurFeature.set_Value(nField, dataGridView1.Rows[m_nCurRowIndex].Cells[e.ColumnIndex].Value);
                    m_pCurFeature.Store();
                }
            }
        }

        private void dataGridView1_Scroll(object sender, ScrollEventArgs e)
        {
            m_dtp.Visible = false;
        }
    }
}
