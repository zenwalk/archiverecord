using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ArchiveRecord.Globe;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;

namespace ArchiveRecord
{
    public partial class frmQueryPan : UserControl
    {
        public Int32 m_nObjectId, m_nCurRowIndex;
        bool m_bEdit;
        public IFeature m_pCurFeature;
        public List<IFeature> m_featureCollection = new List<IFeature>();

        public frmQueryPan()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(TextBox1.Text))
            {
                RefreshGrid();
            }
            else
            {
                ForAR.ArchiveFill(dataGridView1, ForAR.GridSetType.Archive_FillPan, string.Format(" WHERE ({0} LIKE '%{1}%')", comboBox1.Text, TextBox1.Text), new string[] { "" });
            }
        }

        private void frmQueryPan_Load(object sender, EventArgs e)
        {
            RefreshGrid();
            foreach (DataGridViewColumn eCol in dataGridView1.Columns)
            {
                if (eCol.Name != "ID" && ForAR.IsChineseLetter(eCol.Name, 0))
                {
                    comboBox1.Items.Add(eCol.Name);
                }
            }
            comboBox1.SelectedIndex = 0;
        }

        private void dataGridView1_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            m_pCurFeature = null;
            if (e.RowIndex >= 0 && e.RowIndex <= dataGridView1.Rows.Count)
            {
                m_nCurRowIndex = e.RowIndex;
                dataGridView1.Rows[m_nCurRowIndex].Selected = true;
                m_pCurFeature = EngineFuntions.GetFeatureByFieldAndValue(EngineFuntions.m_Layer_BusStation, "OBJECTID", dataGridView1.Rows[m_nCurRowIndex].Cells["OBJECTID"].Value.ToString());
                if (m_pCurFeature != null)
                {
                    IEnvelope pEnvelope;
                    pEnvelope = m_pCurFeature.Extent;
                    pEnvelope.Expand(2, 2, true);
                    EngineFuntions.m_AxMapControl.ActiveView.Extent = pEnvelope;
                    EngineFuntions.m_AxMapControl.ActiveView.ScreenDisplay.Invalidate(null, true, (short)esriScreenCache.esriAllScreenCaches);
                    System.Windows.Forms.Application.DoEvents();
                    EngineFuntions.FlashShape(m_pCurFeature.ShapeCopy);
                    System.Windows.Forms.Application.DoEvents();
                    frmAttribute frmPopup = new frmAttribute();
                    frmPopup.m_featureCollection.Add(m_pCurFeature);
                    frmPopup.ShowDialog();
                }
            }
        }

        public void RefreshGrid()
        {
            ForAR.ArchiveFill(dataGridView1, ForAR.GridSetType.Archive_FillPan, "",new string[]{""});
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

        private void 属性编辑ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAttribute frmPopup = new frmAttribute();
            m_featureCollection.Clear();
            if (m_pCurFeature != null)
            {
                m_featureCollection.Add(m_pCurFeature);
                frmPopup.m_featureCollection = m_featureCollection;
                frmPopup.ShowDialog();
            }
        }

        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            m_pCurFeature = null;
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.RowIndex <= dataGridView1.Rows.Count)
            {
                m_nCurRowIndex = e.RowIndex;
                dataGridView1.Rows[m_nCurRowIndex].Selected = true;
                contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
                m_pCurFeature = EngineFuntions.GetOneSeartchFeature(EngineFuntions.m_Layer_BusStation, "OBJECTID = " + dataGridView1.Rows[m_nCurRowIndex].Cells["OBJECTID"].Value.ToString());
            }
        }

        private void 定位到ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_pCurFeature != null)
            {
                IEnvelope pEnvelope;
                pEnvelope = m_pCurFeature.Extent;
                pEnvelope.Expand(2, 2, true);
                EngineFuntions.m_AxMapControl.ActiveView.Extent = pEnvelope;
                EngineFuntions.m_AxMapControl.ActiveView.ScreenDisplay.Invalidate(null, true, (short)esriScreenCache.esriAllScreenCaches);
                System.Windows.Forms.Application.DoEvents();
                EngineFuntions.FlashShape(m_pCurFeature.ShapeCopy);
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_pCurFeature != null)
            {
                if (MessageBox.Show(string.Format("确认删除工程：{0}!", m_pCurFeature.get_Value(m_pCurFeature.Fields.FindField("任务号"))), "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                {
                    m_pCurFeature.Delete();
                    dataGridView1.Rows.RemoveAt(m_nCurRowIndex);
                }
            }
            EngineFuntions.m_AxMapControl.ActiveView.Refresh();
        }
    }
}
