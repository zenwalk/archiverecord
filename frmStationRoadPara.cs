using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Utility;

namespace MapAndPageLayoutSynchApp
{
    public partial class frmStationRoadPara : Form
    {
        public IFeature m_pFeature;
        public frmStationRoadPara()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IFields fields = m_pFeature.Fields;
            int nIndex = fields.FindFieldByAliasName("ÏßÂ·Ãû³Æ");
            m_pFeature.set_Value(nIndex,textBox1.Text);
            m_pFeature.Store();
        }
    }
}