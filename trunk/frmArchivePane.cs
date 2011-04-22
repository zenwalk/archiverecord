using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ArchiveRecord.Globe;
using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Interop;
using Autodesk.AutoCAD.Interop.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using System.Threading;

namespace ArchiveRecord
{
    public partial class frmArchivePane : UserControl
    {
        public List<FilePathInfo> m_FilePath = new List<FilePathInfo>();
        public IFeature m_pFeature;
        public string m_strFolder;

        frmLoading m_Popfrm = null;
        private Thread m_threadDWG;
        private event EventHandler OnDWGloaded;//加载完成引发的事件

        public frmArchivePane()
        {
            InitializeComponent();
        }

        public void ReflashGrid()
        {
            dataGridView1.DataSource = m_FilePath;
            dataGridView1.Columns["nKillPathLen"].Visible = false;
            dataGridView1.Columns["nKillPathLen"].ValueType = typeof(int);//指定类型有设么用还不知道
            dataGridView1.Columns["FileNameWithSuf"].Visible = false;
            dataGridView1.Columns["FileFolder"].Visible = false;
            dataGridView1.Columns["FilePath"].Visible = false;
            dataGridView1.Columns["FileName"].HeaderText = "文件名";
            dataGridView1.Columns["FileName"].ReadOnly = true;
            dataGridView1.Columns["FileSuffix"].HeaderText = "类型";
            dataGridView1.Columns["FileSuffix"].ReadOnly = true;
            dataGridView1.Columns[0].ReadOnly = false;

            textBox2.Text = m_strFolder.Substring(m_strFolder.LastIndexOf("\\") + 1);
        }

        private void frmArchivePane_Load(object sender, EventArgs e)
        {
            OnDWGloaded += new EventHandler(DWGLoaded);

        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex <= dataGridView1.Rows.Count)
            {
                System.Diagnostics.Process.Start(dataGridView1.Rows[e.RowIndex].Cells["FilePath"].Value.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (m_Popfrm == null)
            {
                m_Popfrm = new frmLoading();
                m_Popfrm.Show();
                backgroundWorker1.RunWorkerAsync();
            }
            //m_threadDWG = new Thread(new ThreadStart(DWGLoad));
            //// Start the thread
            //m_threadDWG.Start();
            
        }

        private void DWGLoad()
        {
            AcadApplication acadApp = new AcadApplicationClass();
            IGeometryCollection pGeometryCollection = new PolygonClass();
            foreach (DataGridViewRow eRow in dataGridView1.Rows)
            {
                if (eRow.Cells[0].Value != null && (bool)eRow.Cells[0].Value == true)
                {
                    object o = Type.Missing;
                    string strFileName = eRow.Cells["FilePath"].Value.ToString();
                    AcadDocument acadDoc = acadApp.Documents.Open(strFileName, true, null);
                    System.Windows.Forms.Application.DoEvents();
                    AcadLayer acadLyer = acadDoc.Layers.Item(1);
                    AcadSelectionSet ssetObj = acadDoc.SelectionSets.Add("FWX");
                    short[] vFilterType = null;
                    object[] vFilterData = null;
                    vFilterType = new short[1];
                    vFilterType[0] = 8;
                    vFilterData = new object[1];
                    vFilterData[0] = "FWX";
                    //ISegmentCollection pSegmentCollection = new RingClass();
                    //pSegmentCollection.AddSegment()
                    ssetObj.Select(AcSelect.acSelectionSetAll, null, null, vFilterType, vFilterData);
                    foreach (AcadObject eEntity in ssetObj)
                    {
                        if (eEntity.ObjectName == "AcDbPolyline")
                        {
                            AcadLWPolyline pPline = (AcadLWPolyline)eEntity;
                            double[] polyLinePoint;
                            polyLinePoint = (Double[])pPline.Coordinates;
                            int i, pointCount = polyLinePoint.Length / 2;
                            IPointCollection pPointColl = new RingClass();
                            for (i = 0; i < polyLinePoint.Length - 1; i = i + 2)
                            {
                                IPoint pPoint = new PointClass();
                                pPoint.X = polyLinePoint[i];
                                pPoint.Y = polyLinePoint[i + 1];
                                pPointColl.AddPoint(pPoint, ref o, ref o);
                            }
                            pGeometryCollection.AddGeometry(pPointColl as IRing, ref o, ref o);
                        }
                    }
                }
            }
            acadApp.Quit();
            //DWGLoaded(this.m_Popfrm, new EventArgs());//引发完成事件，有异常待研究
            System.Windows.Forms.Application.DoEvents();
            if (pGeometryCollection.GeometryCount > 0)
            {
                m_pFeature = EngineFuntions.m_Layer_BusStation.FeatureClass.CreateFeature();
                m_pFeature.Shape = pGeometryCollection as IPolygon;
                IFields fields = m_pFeature.Fields;
                int nIndex = fields.FindField("任务号");
                m_pFeature.set_Value(nIndex, m_strFolder.Substring(m_strFolder.LastIndexOf("\\") + 1));
                m_pFeature.Store();
                EngineFuntions.ZoomTo(m_pFeature.ShapeCopy);
            }
            else
            {
                m_pFeature = null;
            }
        }

        private void DWGLoaded(object sender, EventArgs e)//用thread有异常
        {
            m_Popfrm.Close();//线程不能调用m_Popfrm
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (m_Popfrm == null)
            {
                m_Popfrm = new frmLoading();
                m_Popfrm.m_nType = 2;
                m_Popfrm.Show();
                backgroundWorker2.RunWorkerAsync();
            }
            
            //IWorkspaceFactory pWorkspaceFactory = new CadWorkspaceFactoryClass();
            //IFeatureWorkspace pFeatureWorkspace = pWorkspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName("C:\\Drawing1.dwg"), 0) as IFeatureWorkspace;
            //IFeatureDataset pFeatureDataset = pFeatureWorkspace.OpenFeatureDataset(System.IO.Path.GetFileName("C:\\Drawing1.dwg"));
            //IFeatureClassContainer pFeatClassContainer = pFeatureDataset as IFeatureClassContainer;

            //for (int i = 0; i < pFeatClassContainer.ClassCount - 1; i++)
            //{
            //    IFeatureLayer pFeatureLayer;
            //    IFeatureClass pFeatClass = pFeatClassContainer.get_Class(i);
            //   if (pFeatClass.FeatureType == esriFeatureType.esriFTAnnotation)
            //    {
            //        //如果是注记，则添加注记层
            //        pFeatureLayer = new CadAnnotationLayerClass();
            //    }
            //    else//如果是点、线、面，则添加要素层
            //    {
            //        pFeatureLayer = new FeatureLayerClass();

            //    }
            //   pFeatureLayer.Name = pFeatClass.AliasName;
            //   pFeatureLayer.FeatureClass = pFeatClass;

            //    //this.axmc_Main.Map.AddLayer(pFeatureLayer);
            //    //this.axmc_Main.ActiveView.**();
            //}
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            DWGLoad();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            m_Popfrm.Close();
            m_Popfrm = null;
            System.Windows.Forms.Application.DoEvents();
            if (m_pFeature == null)
            {
                MessageBox.Show("没有找到范围线\n", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            FtpDo theftp = new FtpDo("172.16.34.233",9999,"lq","lq");
            theftp.UpLoader(m_FilePath);
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            m_Popfrm.Close();
            m_Popfrm = null;
            System.Windows.Forms.Application.DoEvents();
            MessageBox.Show("数据上传完毕！\n", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
