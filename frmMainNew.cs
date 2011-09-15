using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ArchiveRecord.Globe;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;

using XtremeDockingPane;
using System.Globalization;
using Microsoft.Office.Interop.Excel;
using Winapp = System.Windows.Forms.Application;
using GISPoint = ESRI.ArcGIS.Geometry.IPoint;
using System.Data.OleDb;
using Microsoft.Win32;
using Autodesk.AutoCAD.ApplicationServices;

namespace ArchiveRecord
{

    public partial class frmMainNew : Form
    {
        #region ��������
        public frmFlash m_frmFlash;
        public frmlayerToc m_frmlayerToc = new frmlayerToc();
        public frmArchivePane m_frmArchivePane = new frmArchivePane();
        public frmQueryPan m_frmQueryPan = new frmQueryPan();
        public int m_ToolStatus;
        public GISPoint m_mapPoint;//�������ѯ��������
        public GISPoint m_FormPoint;//�����·��˳��ڵ�
        public IFeatureLayer m_CurFeatureLayer ;//��ǰFeatureLayer
        public IFeature m_CurFeature ;  //��ǰfeature
        public List<IFeature> m_featureCollection = new List<IFeature>();   //�õ�����ѡ�е�feature
        public List<IPolyline> m_PolylineCollection = new List<IPolyline>();   //�õ�����ѡ�е�IPolyline
        public MovePointFeedbackClass m_FeedBack;
        public bool m_bShowLayer;
        public int m_nPLineNum = 1;//��·ÿ�ΰ�������
        //AoInitialize m_pAoInitialize = new AoInitialize();//�жϰ汾
        #endregion

        public frmMainNew()
        {
            InitializeComponent();
        }

        private void frmMainNew_FormClosed(object sender, FormClosedEventArgs e)
        {
            ESRI.ArcGIS.ADF.COMSupport.AOUninitialize.Shutdown();
            if (m_frmFlash != null)
            {
                m_frmFlash.Close();
            }
        }

        private void frmMainNew_Load(object sender, EventArgs e)
        {
           
            EngineFuntions.m_AxMapControl = axMapControl1;//����Map�ؼ�

            axCommandBars1.LoadDesignerBars(null, null);
            axCommandBars1.ActiveMenuBar.Delete();
            axCommandBars1.Options.UseDisabledIcons = true;
            UInt32 pColor;
            pColor = System.Convert.ToUInt32(ColorTranslator.ToOle(Color.FromArgb(255, 255, 255)).ToString());
            axCommandBars1.SetSpecialColor((XtremeCommandBars.XTPColorManagerColor)15, pColor);
            
            //m_pMapDocument = new MapDocumentClass();
            //m_pMapDocument.Open("..\\..\\data\\DataSDE.mxd", string.Empty);
            //axMapControl1.Map = m_pMapDocument.get_Map(0);
            axMapControl1.Map.Name = "��ѯ";
            axMapControl1.Extent = axMapControl1.FullExtent;

            List<IFeatureLayer> colLayers;
            colLayers = EngineFuntions.GetAllValidFeatureLayers(axMapControl1.Map);
            //��������ͼ�㲻��ѡ��
            EngineFuntions.SetCanSelLay("");//��ͼ���������в���ѡ

            EngineFuntions.m_Layer_BusStation = EngineFuntions.GetLayerByName("������Ŀ", colLayers);
            //EngineFuntions.m_Layer_BusRoad = EngineFuntions.GetLayerByName("����վ��", colLayers);
            //EngineFuntions.m_Layer_BackRoad = EngineFuntions.GetLayerByName("վ�߱���", colLayers);


            //Determine if Alpha Context is supported, if it is, then enable it
            axDockingPane1.Options.AlphaDockingContext = true;
            //Determine if Docking Stickers is supported, if they are, then enable them
            axDockingPane1.Options.ShowDockingContextStickers = true;

            axDockingPane1.Options.ThemedFloatingFrames = true;
            axDockingPane1.TabPaintManager.Position = XTPTabPosition.xtpTabPositionTop;
            axDockingPane1.TabPaintManager.Appearance = XtremeDockingPane.XTPTabAppearanceStyle.xtpTabAppearanceVisualStudio;

            XtremeDockingPane.Pane ThePane = axDockingPane1.CreatePane(ForAR.Pan_Layer, 200, 200, DockingDirection.DockLeftOf, null);
            ThePane.Title = "ͼ������";
            axDockingPane1.FindPane(ForAR.Pan_Layer).Handle = m_frmlayerToc.Handle.ToInt32();

            ThePane = axDockingPane1.CreatePane(ForAR.Pan_Archive, 200, 200, DockingDirection.DockLeftOf, null);
            ThePane.Title = "�������";
            axDockingPane1.FindPane(ForAR.Pan_Archive).Handle = m_frmArchivePane.Handle.ToInt32();

            ThePane = axDockingPane1.CreatePane(ForAR.Pan_Query, 200, 200, DockingDirection.DockLeftOf, null);
            ThePane.Title = "���̲�ѯ";
            axDockingPane1.FindPane(ForAR.Pan_Query).Handle = m_frmQueryPan.Handle.ToInt32();

            axDockingPane1.AttachPane(axDockingPane1.FindPane(ForAR.Pan_Query), axDockingPane1.FindPane(ForAR.Pan_Archive));
            axDockingPane1.AttachPane(axDockingPane1.FindPane(ForAR.Pan_Layer), axDockingPane1.FindPane(ForAR.Pan_Archive));
            axDockingPane1.FindPane(ForAR.Pan_Layer).Select();
            axDockingPane1.FindPane(ForAR.Pan_Query).Options = PaneOptions.PaneNoCloseable;
            axDockingPane1.FindPane(ForAR.Pan_Layer).Options = PaneOptions.PaneNoCloseable;
            axDockingPane1.FindPane(ForAR.Pan_Archive).Options = PaneOptions.PaneNoCloseable;
            //'ӥ��ͼ��
            //String sHawkEyeFileName;
            //sHawkEyeFileName = ForAR.Mxd_Name;
            //m_frmlayerToc.MapHawkEye.LoadMxFile(sHawkEyeFileName);
            //m_frmlayerToc.MapHawkEye.Extent = m_frmlayerToc.MapHawkEye.FullExtent;
            //m_frmlayerToc.m_MapControl = axMapControl1.Object;
            m_frmlayerToc.TOCControl.SetBuddyControl(this.axMapControl1.Object);

            axDockingPane1.SetCommandBars(axCommandBars1.GetDispatch());
            this.WindowState = FormWindowState.Maximized;
            m_bShowLayer = false;
            ForAR.m_FrmMain = this;

            if (ForAR.Login_Operation != "")//�����û�Ȩ�޶�Ӧ��ֹ�Ĳ���
            {
                string[] strColu = ForAR.Login_Operation.Split('��');
                int nCol;
                foreach (string eStrRow in strColu)
                {
                    nCol = Convert.ToInt32(eStrRow[0].ToString());
                    string strRow = eStrRow.Substring(2);
                    string[] strRows = strRow.Split('��');
                    foreach (string eStrRows in strRows)
                    {
                        axCommandBars1[nCol].Controls[Convert.ToInt32(eStrRows)].Enabled = false;
                    }
                }
            }

            // Set what the Help file will be for the HelpProvider.
            this.helpProvider2.HelpNamespace = Winapp.StartupPath + @"\�û��ֲ�.doc";
        }

        private void axCommandBars1_Execute(object sender, AxXtremeCommandBars._DCommandBarsEvents_ExecuteEvent e)
        {
            EngineFuntions.SetToolNull();
            switch (e.control.Id)
            {
                //case ForAR.BusInfo_Help:
                //    System.Diagnostics.Process.Start(Winapp.StartupPath + @"\�û��ֲ�.doc");
                //    break;
                //case ForAR.BusInfo_ParaSet:
                //    System.Diagnostics.Process.Start(Winapp.StartupPath + @"\Businfo.ini");
                //    break;
                case ForAR.Map3D_ZoomIn:
                    m_ToolStatus = ForAR.Map3D_ZoomIn;
                    if(axMapControl1.Visible == true)
                    {
                        ICommand pCommand; 
                        pCommand = new ControlsMapZoomInTool();
                        pCommand.OnCreate(axMapControl1.Object);
                        axMapControl1.CurrentTool = (ITool)pCommand;
                    }
                    break;
                case ForAR.Map3D_ZoomOut:
                    m_ToolStatus = ForAR.Map3D_ZoomOut;
                    if (axMapControl1.Visible == true)
                    {
                        ICommand pCommand; 
                        pCommand = new ControlsMapZoomOutTool();
                        pCommand.OnCreate(axMapControl1.Object);
                        axMapControl1.CurrentTool = (ITool)pCommand;
                    }
                    break;
                case ForAR.Map3D_Pan:
                    m_ToolStatus = ForAR.Map3D_Pan;
                    if (axMapControl1.Visible == true)
                    {
                        ICommand pCommand; 
                        pCommand = new ControlsMapPanTool();
                        pCommand.OnCreate(axMapControl1.Object);
                        axMapControl1.CurrentTool = (ITool)pCommand;
                    }
                    break;
                case ForAR.Map3D_Reflash:
                    m_ToolStatus = ForAR.Map3D_Reflash;
                    if (axMapControl1.Visible == true)
                    {
                       axMapControl1.Map.ClearSelection();
                       axMapControl1.ActiveView.GraphicsContainer.DeleteAllElements();
                       EngineFuntions.MapRefresh();
                    }
                    break;
                //ǰһ��
                case ForAR.Map3D_PreView:
                    m_ToolStatus = ForAR.Map3D_PreView;
                    if (axMapControl1.Visible == true)
                    {
                       EngineFuntions.GoBack();
                    }
                    break;
                //��һ��
                case ForAR.Map3D_NextView:
                    m_ToolStatus = ForAR.Map3D_NextView;
                    if (axMapControl1.Visible == true)
                    {
                       EngineFuntions.GoNext();
                    }
                    break;
                case ForAR.Map3D_Distance://���㳤��
                    break;

                case ForAR.Map3D_Area://�������
                    break;

                case ForAR.Map3D_Select://����ѡ��
                    m_ToolStatus = ForAR.Map3D_Select;
                    axMapControl1.MousePointer = esriControlsMousePointer.esriPointerPencil;
                    
                    break;

                case ForAR.Map3D_PointSelect://���ѡ��
                    m_ToolStatus = ForAR.Map3D_PointSelect;
                    axMapControl1.MousePointer = esriControlsMousePointer.esriPointerPencil;

                    break;

                case ForAR.Map3D_PolySelect://�����ѡ��
                    m_ToolStatus = ForAR.Map3D_PolySelect;
                    axMapControl1.MousePointer = esriControlsMousePointer.esriPointerPencil;

                    break;
                case ForAR.Map3D_CircleSelect://Բ��ѡ��
                    m_ToolStatus = ForAR.Map3D_CircleSelect;
                    axMapControl1.MousePointer = esriControlsMousePointer.esriPointerPencil;

                    break;
                case ForAR.Record_Input://�������
                    m_ToolStatus = ForAR.Record_Input;
                    axMapControl1.MousePointer = esriControlsMousePointer.esriPointerPencil;
                    try
                    {
                        RegistryKey testKey = Registry.CurrentUser.OpenSubKey("TestKey");
                        if (testKey == null)
                        {
                            testKey = Registry.CurrentUser.CreateSubKey("TestKey");
                            testKey.SetValue("OpenFolderDir", "");
                            testKey.Close();
                            Registry.CurrentUser.Close();
                        }
                        else
                        {
                            folderBrowserDialog1.SelectedPath = testKey.GetValue("OpenFolderDir").ToString();
                            testKey.Close();
                            Registry.CurrentUser.Close();
                        }
                    }
                    catch (Exception ee)
                    {

                    }

                    if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                    {
                        string folderName = folderBrowserDialog1.SelectedPath;
                        m_frmArchivePane.m_FilePath.Clear();
                        m_frmArchivePane.m_strFolder = folderName;
                        ForAR.FindFile(folderName + "\\", ref m_frmArchivePane.m_FilePath, folderName.LastIndexOf("\\")+1);
                        m_frmArchivePane.ReflashGrid();
                        axDockingPane1.FindPane(ForAR.Pan_Archive).Selected = true;
                        RegistryKey testKey = Registry.CurrentUser.OpenSubKey("TestKey", true);  //true��ʾ��д��false��ʾֻ�� 
                        testKey.SetValue("OpenFolderDir", folderName);
                        testKey.Close();
                        Registry.CurrentUser.Close(); 

                    }
                    break;

                case ForAR.Map3D_Full:
                    m_ToolStatus = ForAR.Map3D_Full;
                    if (axMapControl1.Visible == true)
                    {
                       axMapControl1.Extent = axMapControl1.FullExtent;
                    }
                    break;
                default :
                    break;
            }
        }

        private void axCommandBars1_ResizeEvent(object sender, EventArgs e)
        {
            int left, top, right, bottom;
            axCommandBars1.GetClientRect(out left, out top, out right, out bottom);
            axMapControl1.SetBounds(left, top, right - left, bottom - top);
        }

        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            m_mapPoint = axMapControl1.ToMapPoint(e.x, e.y);
            IActiveView pActiveView = axMapControl1.ActiveView;
            if (e.button == 1)
             {
                 switch (m_ToolStatus)
                 {
                     case ForAR.Map3D_Select:
                         if (axMapControl1.Visible == true)
                         {
                            IGeometry pGeo;
                            pGeo = axMapControl1.TrackRectangle();
                            if (pGeo.Envelope.Height * pGeo.Envelope.Width == 0)
                            {
                                return;
                            }
                            List<IFeature> pSelFea = EngineFuntions.GetSeartchFeatures(EngineFuntions.m_Layer_BusStation,pGeo);
                            if (pSelFea.Count > 0)
                            {
                                frmAttribute frmPopup = new frmAttribute();
                                frmPopup.m_featureCollection = pSelFea;
                                frmPopup.ShowDialog();
                            }    
                         }
                         break;
                     case ForAR.Map3D_PointSelect:
                         if (axMapControl1.Visible == true)
                         {
                             m_CurFeatureLayer = EngineFuntions.SetCanSelLay("������Ŀ");
                             EngineFuntions.ClickSel(m_mapPoint, false, false, 6);
                             if (EngineFuntions.GetSeledFeatures(m_CurFeatureLayer, ref  m_featureCollection))
                             {
                                 frmAttribute frmPopup = new frmAttribute();
                                 frmPopup.m_featureCollection = m_featureCollection;
                                 frmPopup.ShowDialog();
                             }
                         }
                         break;
                     case ForAR.Map3D_PolySelect:
                         if (axMapControl1.Visible == true)
                         {
                             IGeometry pGeo;
                             pGeo = axMapControl1.TrackPolygon();
                             if (pGeo.Envelope.Height * pGeo.Envelope.Width == 0)
                             {
                                 return;
                             }
                             List<IFeature> pSelFea = EngineFuntions.GetSeartchFeatures(EngineFuntions.m_Layer_BusStation, pGeo);
                             if (pSelFea.Count > 0)
                             {
                                 frmAttribute frmPopup = new frmAttribute();
                                 frmPopup.m_featureCollection = pSelFea;
                                 frmPopup.ShowDialog();
                             }
                         }
                         break;
                     case ForAR.Map3D_CircleSelect:
                         if (axMapControl1.Visible == true)
                         {
                             IGeometry pGeo;
                             pGeo = axMapControl1.TrackCircle();
                             if (pGeo.Envelope.Height*pGeo.Envelope.Width == 0)
                             {
                                 return;
                             }
                             List<IFeature> pSelFea = EngineFuntions.GetSeartchFeatures(EngineFuntions.m_Layer_BusStation, pGeo);
                             if (pSelFea.Count > 0)
                             {
                                 frmAttribute frmPopup = new frmAttribute();
                                 frmPopup.m_featureCollection = pSelFea;
                                 frmPopup.ShowDialog();
                             }
                         }
                         break;
                     default :
           	                break;
               }
             }
           if (2 == e.button)
           {
               EngineFuntions.ZoomPoint(m_mapPoint, EngineFuntions.m_AxMapControl.Map.MapScale); 
           }
         
        }

        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            if (e.button == 1)
            {
                switch (m_ToolStatus)
                {
                    //case ForAR.Bus_Move:
                    //    m_mapPoint = axMapControl1.ToMapPoint(e.x, e.y);
                    //    m_FeedBack.MoveTo(m_mapPoint);
                    //    break;
                    default:
                        break;
                }
            }
        }

        private void axMapControl1_OnMouseUp(object sender, IMapControlEvents2_OnMouseUpEvent e)
        {
            if (e.button == 1)
            {
                switch (m_ToolStatus)
                {
                    //case ForAR.Bus_Move:
                    //    if (m_FeedBack != null)
                    //    {
                    //        IMovePointFeedback pointMoveFeedback = m_FeedBack;
                    //        if (m_CurFeature != null)
                    //        {
                    //              m_CurFeature.Shape = pointMoveFeedback.Stop();
                    //              m_CurFeature.Store();
                    //            EngineFuntions.PartialRefresh(EngineFuntions.m_Layer_BusStation);
                    //        }
                    //        m_FeedBack = null;
                    //    }
                       
                    //m_ToolStatus = -1;
                    //axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
                    //    break;
                    default:
                        break;
                }
            }
        }

        private void frmMainNew_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Release COM objects and shut down the AoInitilaize object
            //m_pAoInitialize.Shutdown();
            
        }

        private void axMapControl1_OnAfterScreenDraw(object sender, IMapControlEvents2_OnAfterScreenDrawEvent e)
        {
           
        }

    }
}