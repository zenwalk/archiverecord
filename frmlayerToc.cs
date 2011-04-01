using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using ArchiveRecord.Globe;
using ESRI.ArcGIS.Carto;

namespace ArchiveRecord
{
    public partial class frmlayerToc : UserControl
    {
        ITOCControl m_pTOCControl;
        ILayer m_SelLayer = null;
        public frmlayerToc()
        {
            InitializeComponent();
        }

        private void frmlayerToc_Resize(object sender, EventArgs e)
        {
            TOCControl.SetBounds(1, 1, this.ClientRectangle.Width - 2, this.ClientRectangle.Height - this.MapHawkEye.Height);
            MapHawkEye.SetBounds(1, this.TOCControl.Height, this.ClientRectangle.Width - 2, this.ClientRectangle.Height - this.TOCControl.Height);
        }

        private void frmlayerToc_Load(object sender, EventArgs e)
        {
            m_pTOCControl = (ITOCControl)TOCControl.Object;
            TOCControl.EnableLayerDragDrop = false;
  
        }

        private void MapHawkEye_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            IEnvelope pEnvelope = MapHawkEye.TrackRectangle();
            MapHawkEye.ActiveView.Refresh();
            EngineFuntions.m_AxMapControl.ActiveView.Extent = pEnvelope;
            EngineFuntions.m_AxMapControl.ActiveView.Refresh();
        }

        private void TOCControl_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
               //��������Ҽ�����ֱ�ӷ���
               if (e.button != 2) return;
               esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
               IBasicMap map = null;
               ILayer layer = null;
               object other = null; 
               object index = null;
               //�ж���ѡ�˵�������
               TOCControl.HitTest(e.x, e.y, ref item, ref map, ref layer, ref other, ref index);
               //ȷ��ѡ���Ĳ˵����ͣ�Map����ͼ��˵�
               if (item == esriTOCControlItem.esriTOCControlItemMap)
                   TOCControl.SelectItem(map, null);
               else
                   TOCControl.SelectItem(layer, null);
               //����CustomPropertyΪlayer (�����Զ����Layer����)                   
              // m_mapControl.CustomProperty = layer;
               //�����Ҽ��˵�
               if (item == esriTOCControlItem.esriTOCControlItemMap)
                   contextMenuStrip1.Show(TOCControl,e.x, e.y);
               if (item == esriTOCControlItem.esriTOCControlItemLayer)
                   contextMenuStrip1.Show(TOCControl, e.x, e.y);

               m_SelLayer = layer;
        }

        private void ���Թ���ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(m_SelLayer != null)
            {
                switch (m_SelLayer.Name)
                {
                    case "������Ŀ":
                        frmAttribute frmPopup = new frmAttribute();
                        frmPopup.ShowDialog();
                	break;
                }
            }
        }

        private void TOCControl_OnEndLabelEdit(object sender, ITOCControlEvents_OnEndLabelEditEvent e)
        {
            e.canEdit = false;//ȡ���޸�
        }

    }
}
