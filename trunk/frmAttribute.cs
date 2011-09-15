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

        public frmAttribute()
        {
            InitializeComponent();
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
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (m_bEdit)
            {
                m_nCurRowIndex = e.RowIndex;
                m_pCurFeature = EngineFuntions.GetOneSeartchFeature(EngineFuntions.m_Layer_BusStation, "OBJECTID = " + dataGridView1.Rows[m_nCurRowIndex].Cells["OBJECTID"].Value.ToString());
                if (m_pCurFeature != null)
                {
                    int nField = m_pCurFeature.Fields.FindField(dataGridView1.Columns[e.ColumnIndex].Name);
                    m_pCurFeature.set_Value(nField, dataGridView1.Rows[m_nCurRowIndex].Cells[e.ColumnIndex].Value);
                    m_pCurFeature.Store();
                }
            }
        }
    }
}
