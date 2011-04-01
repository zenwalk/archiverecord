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
        public Int32 m_nObjectId, m_nCurRowIndex;
        bool m_bEdit;
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
            ForAR.ArchiveFill(dataGridView1, ForAR.GridSetType.Archive_FillAll, "",new string[]{""});
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
                ForAR.ArchiveFill(dataGridView1, ForAR.GridSetType.Archive_FillByOBJECTID, string.Format(" WHERE (OBJECTID IN ({0}))", strInPara.Substring(0, strInPara.Length - 1)), new string[] { "" });
            }
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
                }
            }
        }
    }
}
