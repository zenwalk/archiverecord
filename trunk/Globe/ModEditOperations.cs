using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Utility;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Controls;

namespace ArchiveRecord.Globe
{
    class EngineFuntions
    {

        public static AxMapControl m_AxMapControl;
        public static IFeatureLayer m_Layer_BusStation, m_Layer_BusRoad, m_Layer_BackRoad;
        
        //获取所有可查询子图层（featurelayer）
        public static List<IFeatureLayer> GetAllValidFeatureLayers(IMap pMap)
        {
            List < IFeatureLayer > colLayers = new List < IFeatureLayer >(); 
            Int32 i, j;
            ILayer pLayer,pSubLayer;
            IFeatureLayer pFeatureLayer;
            IGroupLayer pGroupLayer;
            try
            {
                for (i = 0;i<pMap.LayerCount;i++)
                {
                    pLayer = pMap.get_Layer(i);
                    if (pLayer is IGroupLayer)
                    {
                        pGroupLayer = (IGroupLayer)pMap.get_Layer(i);
                         //access all layers in each grouplayer
                        ICompositeLayer pCompositeLayer;
                        pCompositeLayer = (ICompositeLayer)pGroupLayer;
                         //loop for each layer in a grouplayer
                        for (j = 0;j<pCompositeLayer.Count;j++)
                        {
                            pSubLayer = pCompositeLayer.get_Layer(j);
                            if (pSubLayer is IFeatureLayer && pSubLayer.Valid == true)
                            {
                                pFeatureLayer = (IFeatureLayer)pSubLayer;
                                colLayers.Add(pFeatureLayer);
                            }
                        }
                    }
                    else if(pLayer is IFeatureLayer && pLayer.Valid == true)
                    {
                        pFeatureLayer = (IFeatureLayer)pLayer;
                        colLayers.Add(pFeatureLayer);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("GetAllFeatureLayers函数存在以下问题\n" + ex.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } 
            return colLayers;
        }

        public static IFeatureLayer GetLayerByName(String sLyName, List<IFeatureLayer> colLayers)
        {
            IFeatureLayer pLayer = null;
            try
            {
                foreach (IFeatureLayer pFeatureLayer in colLayers)
                {
                    if (pFeatureLayer.Name == sLyName)
                    {
                        pLayer = pFeatureLayer;
                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("GetLayerByName函数存在以下问题\n" + ex.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return pLayer;
        }

        public static IFeature GetFeatureByFieldAndValue(IFeatureLayer pFeatureLayer, String strField, String strValue)
        {
            IFeatureCursor pCursor;
            IFeature pFeature;

            pCursor = pFeatureLayer.FeatureClass.Search(null, false);
            if (pCursor == null)
            {
                return null;
            }
            else
            {
                pFeature = pCursor.NextFeature();
                while (pFeature != null)
                {
                    if (pFeature.get_Value(pFeature.Fields.FindField(strField)).ToString() == strValue)
                    {
                       return pFeature;
                    }
                    pFeature = pCursor.NextFeature();
                }
            }
            return null;
        }

        public static void PartialRefresh(IFeatureLayer pFeatureLayer)
        {
            m_AxMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, pFeatureLayer, null);
        }

        public static void SetToolNull()
        {
            IToolbarBuddy pToolbarBuddy;
            if (m_AxMapControl.Visible == true)
            {
                pToolbarBuddy = (IToolbarBuddy)m_AxMapControl.Object;
                pToolbarBuddy.CurrentTool = null;
            } 
        }

        public static void MapRefresh()
        {
            if (m_AxMapControl.Map.SelectionCount > 0)
            {
                m_AxMapControl.Map.ClearSelection();
            } 
            m_AxMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
        }

        public static void GoBack()
        {
            try
            {
                IExtentStack pExtentStack;
                pExtentStack = m_AxMapControl.ActiveView.ExtentStack;
                if (pExtentStack.CanUndo())
                {
                    pExtentStack.Undo();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("GoBack函数存在以下问题\n" + ex.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static void GoNext()
        {
            try
            {
                IExtentStack pExtentStack;
                pExtentStack = m_AxMapControl.ActiveView.ExtentStack;
                if (pExtentStack.CanRedo())
                {
                    pExtentStack.Redo();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("GoNext函数存在以下问题\n" + ex.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
      
        public static IFeatureLayer SetCanSelLay(String sLyName)
        {
            ILayer pLayer;
            IFeatureLayer pFLayer = null;
            for (int i = 0; i < m_AxMapControl.LayerCount ; i++)
            {
                pLayer = m_AxMapControl.Map.get_Layer(i);
                if (pLayer is IFeatureLayer)
                {
                    if (pLayer.Name == sLyName)
                    {
                        pFLayer = (IFeatureLayer)pLayer;
                        pFLayer.Selectable = true;
                    }
                    else
                    {
                       ((IFeatureLayer)pLayer).Selectable = false;
                    }
                }
               
            }
            return pFLayer;
        }

        /// <summary>点击选择实体
        /// </summary>
        /// <param name="pGeometry">点对象</param>
        /// <param name="iShiftDown">是否连续选择</param>
        /// <param name="bSeclectOne">是否只选择一个</param>
        /// <param name="nTorrent">缓冲区距离</param>
        /// <returns>返回缓冲后的面状图形</returns>
        public static IGeometry ClickSel(IGeometry pGeometry, bool iShiftDown, bool bSeclectOne, int nTorrent)
        {
                Double length;
                IGeometry pBuffer;
                ITopologicalOperator pTopo;
                pTopo =  (ITopologicalOperator)pGeometry;

                if (pGeometry.GeometryType == esriGeometryType.esriGeometryPoint)//点根据比例尺特殊处理
                {
                    length = ConvertPixelsToMapUnits(m_AxMapControl.ActiveView, nTorrent);
                    pBuffer = pTopo.Buffer(length); //圆形
                    pBuffer = pBuffer.Envelope; //圆的外接矩形
                }
                else
                {
                    pBuffer = pTopo.Buffer(nTorrent);
                }

                try
                {
                    ISelectionEnvironment pSelEnvironment = new SelectionEnvironmentClass();
                    if (iShiftDown)
                    {
                        pSelEnvironment.CombinationMethod = esriSelectionResultEnum.esriSelectionResultXOR;
                    } 
                    else
                    {
                        pSelEnvironment.CombinationMethod = esriSelectionResultEnum.esriSelectionResultNew;
                    }
                    m_AxMapControl.Map.SelectByShape(pBuffer, pSelEnvironment, bSeclectOne);
                    m_AxMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("ClickSel函数存在以下问题\n" + ex.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            return pBuffer;
        }

        /// <summary>获得当前图层所选择的要素Feature
        /// </summary>
        /// <param name="FLayer">要获取要素的层</param>
        /// <param name="colFeatures">选中要素的集合</param>
        /// <returns></returns>
        public static bool GetSeledFeatures(IFeatureLayer FLayer, ref List<IFeature> colFeatures)
        {
            try
            {
                IFeatureSelection pFeatureSelection;
                ISelectionSet pSelected;
                ICursor pCursor;
                IFeature pFeature;
                if (colFeatures == null)
                {
                    colFeatures = new List<IFeature>();
                }
                else
                {
                    colFeatures.Clear();
                }
                pFeatureSelection = (IFeatureSelection)FLayer;
                if (pFeatureSelection == null)
                {
                    return false;
                }
                pSelected = pFeatureSelection.SelectionSet;
                pSelected.Search(null, false, out pCursor);
                if (pCursor != null)
                {
                    pFeature = (IFeature)pCursor.NextRow();
                    if (pFeature == null)
                    {
                        return false;
                    }
                    while (pFeature != null)
                    {
                        colFeatures.Add(pFeature);
                        pFeature = (IFeature)pCursor.NextRow();
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("GetSeledFeatures函数存在以下问题\n" + ex.ToString(), "应用程序错误", MessageBoxButtons.OK);
            }
            return false;
        }

        /// <summary>像素单位转换到图形单位
        /// </summary>
        /// <param name="pActiveView">视图</param>
        /// <param name="pixelUnits">像素单位</param>
        /// <returns>对应的Map单位</returns>
        public static Double ConvertPixelsToMapUnits(IActiveView pActiveView  , Double pixelUnits)
        {
            try
            {
                int x1, x2, y1, y2;
                IPoint p1, p2;
                p1 = pActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds.UpperLeft;
                p2 = pActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds.UpperRight;
                pActiveView.ScreenDisplay.DisplayTransformation.FromMapPoint(p1,out x1,out y1);
                pActiveView.ScreenDisplay.DisplayTransformation.FromMapPoint(p2, out x2, out y2);
                Double  pixelExtent, realWorldDisplayExtent, sizeOfOnePixel;
                pixelExtent = x2 - x1;
                realWorldDisplayExtent = pActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds.Width;
                sizeOfOnePixel = realWorldDisplayExtent / pixelExtent;
                return pixelUnits * sizeOfOnePixel;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("ConvertPixelsToMapUnits函数存在以下问题\n" + ex.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return 0;
        }

        /// <summary>根据图形范围获取查询要素集
        /// </summary>
        /// <param name="pFeatureLayer">要素图层</param>
        /// <param name="pGeometry">图形范围参数</param>
        /// <returns>符合条件要素集合</returns>
        /// <other>GetSeartchFeatures可以替代SetCanSelLay，ClickSel，GetSeledFeatures三个步骤。</other>
        public static List<IFeature> GetSeartchFeatures(IFeatureLayer pFeatureLayer, IGeometry pGeometry)
        {
            try
	            {
                    if (0 == pGeometry.Envelope.Width * pGeometry.Envelope.Height)
                    {
                        ITopologicalOperator pTopo;
                        pTopo = (ITopologicalOperator)pGeometry;
                        pGeometry = pTopo.Buffer(ConvertPixelsToMapUnits(m_AxMapControl.ActiveView, 7));
                    } 
	                List<IFeature> pList = new List<IFeature>();
	                //创建SpatialFilter空间过滤器对象
	                ISpatialFilter pSpatialFilter = new SpatialFilterClass();
	                IQueryFilter pQueryFilter = pSpatialFilter as ISpatialFilter;
	                //设置过滤器的Geometry
	                pSpatialFilter.Geometry = pGeometry;
	                //设置空间关系类型
                    pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
	                //获取FeatureCursor游标
	                IFeatureCursor pFeatureCursor = pFeatureLayer.Search(pQueryFilter, false);
	                //遍历FeatureCursor
	                IFeature pFeature = pFeatureCursor.NextFeature();
	                while (pFeature != null)
	                {
	                    //获取要素对象
	                    pList.Add(pFeature);
	                    pFeature = pFeatureCursor.NextFeature();
	                }
	                System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);
	                return pList;
	            }
	            catch (Exception Err)
	            {
	                MessageBox.Show(Err.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
	                return null;
	            }
	        }

        /// <summary>根据条件查询语句获取查询要素集
        /// </summary>
        /// <param name="pFeatureLayer">要素图层</param>
        /// <param name="strQuery">条件查询语句</param>
        /// <returns>符合条件要素集合</returns>
        /// <other>GetSeartchFeatures可以替代根据属性遍历查找图层元素的操作</other>
        public static List<IFeature> GetSeartchFeatures(IFeatureLayer pFeatureLayer, string strQuery)
        {
            try
            {
                IFeatureSelection pFeatureSelection = (IFeatureSelection)pFeatureLayer;
                List<IFeature> pList = new List<IFeature>();
                //创建pQueryFilter属性过滤器对象
                IQueryFilter pQueryFilter = new QueryFilterClass();
                //设置过滤器的条件
                pQueryFilter.WhereClause = strQuery;//"人口> 10000000";

                //选择要素
                pFeatureSelection.SelectFeatures(pQueryFilter, esriSelectionResultEnum.esriSelectionResultNew, false);

                GetSeledFeatures(pFeatureLayer,ref pList);
                return pList;
            }
            catch (Exception Err)
            {
                MessageBox.Show(Err.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
        }

        /// <summary>获取查询要素，只返回第一个
        /// </summary>
        /// <param name="pFeatureLayer">要素图层</param>
        /// <param name="strQuery">条件查询语句，一般为=号查询</param>
        /// <returns>第一个符合条件要素</returns>
        /// <other>GetOneSeartchFeature可以替代根据属性遍历查找图层元素的操作</other>
        public static IFeature GetOneSeartchFeature(IFeatureLayer pFeatureLayer, string strQuery)
        {
            try
            {
                IFeatureSelection pFeatureSelection = (IFeatureSelection)pFeatureLayer;
                List<IFeature> pList = new List<IFeature>();
                //创建pQueryFilter属性过滤器对象
                IQueryFilter pQueryFilter = new QueryFilterClass();
                //设置过滤器的条件
                pQueryFilter.WhereClause = strQuery;//"人口 = 10000000";

                //选择要素
                pFeatureSelection.SelectFeatures(pQueryFilter, esriSelectionResultEnum.esriSelectionResultNew, false);

                GetSeledFeatures(pFeatureLayer, ref pList);

                if (pList.Count>0)
                {
                    return pList[0];
                } 
                else
                {
                    return null;
                }
            }
            catch (Exception Err)
            {
                MessageBox.Show(Err.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
        }

        /// <summary>合并PL线
       /// 
       /// </summary>
        /// <param name="colFeatures">传入要合并的线类Feature组</param>
        /// <param name="pPolyline">返回合并后的PL线 </param>
        /// <returns>合并是否成功</returns>
        public static bool MergeLines(List<IFeature> colFeatures, ref IPolyline pPolyline)
        {
            try
            {
                if (colFeatures.Count < 2)
                {
                    MessageBox.Show("合并要素少于2个！\n" , "提示", MessageBoxButtons.OK,MessageBoxIcon.Information);
                    pPolyline = null;
                    return false;
                }

                ITopologicalOperator pTopOp;
                IGeometryCollection pGeometryCollection;
                pTopOp = colFeatures[0].ShapeCopy as ITopologicalOperator;
                for (int i = 1; i < colFeatures.Count; i++)
                {
                    pTopOp = pTopOp.Union(colFeatures[i].ShapeCopy) as ITopologicalOperator;
                }
                pPolyline = pTopOp as IPolyline;

                pGeometryCollection = (IGeometryCollection)pPolyline;
                if (pGeometryCollection.GeometryCount > 1)
                {
                    pPolyline = null;
                    return false;
                }
                return true;
            }
            catch (Exception Err)
            {
                MessageBox.Show(Err.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
        //    //简化唯一的Polyline,将其打散为一个具有多个Segment的Polyline
        //    pTopo = pPolyline as ITopologicalOperator3;
        //    pTopo.IsKnownSimple_2 = false;
        //    pTopo.Simplify();
        //    //打散后的Polyline
        //    IPolyline pPly = pTopo as IPolyline;
        //    _pSegmentcollection = (ISegmentCollection)pPly;//打散后Polyline所有的Segment对象
        //    Console.WriteLine(_pSegmentcollection.SegmentCount.ToString());

        //    IPointCollection pPointCol = (IPointCollection)pPly;//打散后所有的节点,其数量为Segment的2倍,需要处理
        //    _pointCollection = this.processDuplicatePoint(pPointCol);//删除了重复点后的所有节点Point对象，即节点集合
        //    Console.WriteLine(_pointCollection.PointCount.ToString());
        //}


        }

        /// <summary>合并PL线
        /// 
        /// </summary>
        /// <param name="colPolylines">传入要合并PL线组</param>
        /// <param name="pPolyline">返回合并后的PL线</param>
        /// <returns>合并是否成功</returns>
        public static bool MergeLines(List<IPolyline> colPolylines, ref IPolyline pPolyline)
        {
            try
            {
                if (colPolylines.Count < 2)
                {
                    MessageBox.Show("合并要素少于2个！\n", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    pPolyline = null;
                    return false;
                }

                ITopologicalOperator pTopOp;
                IGeometryCollection pGeometryCollection;
                pTopOp = colPolylines[0] as ITopologicalOperator;
                for (int i = 1; i < colPolylines.Count; i++)
                {
                    pTopOp = pTopOp.Union(colPolylines[i]) as ITopologicalOperator;
                }
                pPolyline = pTopOp as IPolyline;

                pGeometryCollection = (IGeometryCollection)pPolyline;
                if (pGeometryCollection.GeometryCount > 1)
                {
                    pPolyline = null;
                    return false;
                }
                return true;
            }
            catch (Exception Err)
            {
                MessageBox.Show(Err.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
        }

        /// <summary>设置图层可见性
        /// </summary>
        /// <param name="colLayerNames">图成名集合</param>
        public static void SetLayerVisble(List<string> colLayerNames)
        {
            try
            {
                ILayer pLayer;
                bool bHave = false;
                for (int i = 0; i < m_AxMapControl.Map.LayerCount; i++)
                {
                    bHave = false;
                    pLayer = m_AxMapControl.Map.get_Layer(i);
                    for (int j = 0; j < colLayerNames.Count; j++)
                    {
                        if (pLayer.Name == colLayerNames[j])
                        {
                            pLayer.Visible = true;
                            bHave = true;
                            break;
                        }
                    }
                    if (!bHave)
                    {
                        pLayer.Visible = false;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("GetAllFeatureLayers函数存在以下问题\n" + ex.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>延长线段
        /// </summary>
        /// <param name="passLine">传入去的线</param>
        /// <param name="mode">模式，1为从FromPoint处延长，2为从ToPint处延长，3为两端延长</param>
        /// <param name="dis">延长的距离</param>
        /// <returns></returns>
        /// 创建人：
        public IPolyline getExtendLine(IPolyline passLine, int mode, double dis)
        {
            IPointCollection pPointCol = passLine as IPointCollection;
            switch (mode)
            {
                case 1:
                    IPoint fromPoint = new PointClass();
                    passLine.QueryPoint(esriSegmentExtension.esriExtendAtFrom, -1 * dis, false, fromPoint);
                    pPointCol.InsertPoints(0, 1, ref fromPoint);
                    break;
                case 2:
                    IPoint endPoint = new PointClass();
                    object missing = Type.Missing;
                    passLine.QueryPoint(esriSegmentExtension.esriExtendAtTo, dis + passLine.Length, false, endPoint);
                    pPointCol.AddPoint(endPoint, ref missing, ref missing);
                    break;
                case 3:
                    IPoint fPoint = new PointClass();
                    IPoint ePoint = new PointClass();
                    object missing2 = Type.Missing;
                    passLine.QueryPoint(esriSegmentExtension.esriExtendAtFrom, -1 * dis, false, fPoint);
                    pPointCol.InsertPoints(0, 1, ref fPoint);
                    passLine.QueryPoint(esriSegmentExtension.esriExtendAtTo, dis + passLine.Length, false, ePoint);
                    pPointCol.AddPoint(ePoint, ref missing2, ref missing2);
                    break;
                default:
                    return pPointCol as IPolyline;
            }
            return pPointCol as IPolyline;
        }

        /// <summary>平头buffer
        /// </summary>
        /// <param name="myLine">用做buffer的线图形</param>
        /// <param name="bufferDis">buffer的距离</param>
        /// <returns></returns>
        /// 创建人 ： 懒羊羊
        public static IPolygon FlatBuffer(IPolyline myLine, double bufferDis)
        {
            object o = System.Type.Missing;
            //分别对输入的线平移两次（正方向和负方向）
            IConstructCurve mycurve = new PolylineClass();
            mycurve.ConstructOffset(myLine, bufferDis, ref o, ref o);
            IPointCollection pCol = mycurve as IPointCollection;
            IConstructCurve mycurve2 = new PolylineClass();
            mycurve2.ConstructOffset(myLine, -1 * bufferDis, ref o, ref o);
            //把第二次平移的线的所有节点翻转
            IPolyline addline = mycurve2 as IPolyline;
            addline.ReverseOrientation();
            //把第二条的所有节点放到第一条线的IPointCollection里面
            IPointCollection pCol2 = addline as IPointCollection;
            pCol.AddPointCollection(pCol2);
            //用面去初始化一个IPointCollection
            IPointCollection myPCol = new PolygonClass();
            myPCol.AddPointCollection(pCol);
            //把IPointCollection转换为面
            IPolygon myPolygon = myPCol as IPolygon;
            //简化节点次序
            myPolygon.SimplifyPreserveFromTo();
            return myPolygon;
        }

        /// <summary>添加面Element
        /// </summary>
        /// <param name="pGeometry">面图形类</param>
        /// <returns></returns>
        public static IElement AddPolygonElement(IGeometry pGeometry)
        {
            IGraphicsContainer pGraphicsContainer;
            IFillShapeElement pPolygonElement;
            ISimpleFillSymbol Symbol;
            IRgbColor pColor;
            ISimpleLineSymbol pOutline;
            IElement pElement;
         if (null != pGeometry)
         {
             pGraphicsContainer = (IGraphicsContainer)m_AxMapControl.ActiveView;
              //* ''*/Set the color properties
            pColor = new RgbColor();
            pColor.Red = 255;
            pColor.Green = 0;
            pColor.Blue = 0;
            //Get the SimpleLineSymbol symbol interface
            pOutline = new SimpleLineSymbol();
            pOutline.Width = 1;
            pOutline.Color = pColor;
              //Get the ISimpleFillSymbol interface,  'Set the fill symbol properties
            Symbol = new SimpleFillSymbol();
            Symbol.Outline = pOutline;
            Symbol.Color = pColor;
            Symbol.Style = esriSimpleFillStyle.esriSFSForwardDiagonal;

            pPolygonElement = (IFillShapeElement)new CircleElement();
            pPolygonElement.Symbol = Symbol;
            pElement = (IElement)pPolygonElement;
            //将元素的图形转换成地图要素
            pElement.Geometry = pGeometry;
            //向Map 中添加元素
            pGraphicsContainer.AddElement(pElement, 0);
            //刷新
            m_AxMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            return pElement;
         }
             return null;
        }

        /// <summary>添加图片Element
        /// </summary>
        /// <param name="pGeometry">图片范围Geometry</param>
        /// <param name="strfilename">图片名文件地址</param>
        /// <returns></returns>
        public static IMarkerElement AddPictureElement(IGeometry pGeometry,string strfilename)
        {
            IMarkerElement pMarkerElement = new MarkerElementClass();
            IPictureMarkerSymbol pFillSymbol = new PictureMarkerSymbolClass();
            IRgbColor pColor = new RgbColorClass();
            IGraphicsContainer pGraphicsContainer;
            if (pGeometry != null)
            {
                pGraphicsContainer = (IGraphicsContainer)m_AxMapControl.ActiveView;
                //设置位图透明颜色
                pColor.Red = 255;
                pColor.Green = 255;
                pColor.Blue = 255;
                pFillSymbol.BitmapTransparencyColor = pColor;
                pFillSymbol.Size = 70;
                pFillSymbol.YOffset = 20;
                //string filename = "E:\\23.bmp";
                pFillSymbol.CreateMarkerSymbolFromFile(esriIPictureType.esriIPictureBitmap, strfilename);
                //设置标记位置
                pMarkerElement.Symbol = pFillSymbol;
                ((IElement)pMarkerElement).Geometry = pGeometry;//IPoint
                pGraphicsContainer.AddElement((IElement)pMarkerElement, 0);
            }
            else
            {
                pMarkerElement = null;
            }
            return pMarkerElement;
        }

        /// <summary>添加文字Element
        /// </summary>
        /// <param name="pGeometry">文字位置Geometry</param>
        /// <param name="strText">文字内容</param>
        /// <returns></returns>
        public static ITextElement AddTextElement(IGeometry pGeometry, string strText)
        {
            ITextElement pTextElement = new TextElementClass();
            ITextSymbol pTextSymbol = new TextSymbolClass();
            IRgbColor pColor = new RgbColorClass();
            IGraphicsContainer pGraphicsContainer;
            if (pGeometry != null)
            {
                pGraphicsContainer = (IGraphicsContainer)m_AxMapControl.ActiveView;
                //设置位图透明颜色
                pColor.Red = 11;
                pColor.Green = 11;
                pColor.Blue = 11;
                pTextSymbol.Color = pColor;
                pTextSymbol.Size = 15;
                //设置标记位置
                pTextElement.Symbol = pTextSymbol;
                pTextElement.Text = strText;
                pTextElement.ScaleText = true;
                ((IElement)pTextElement).Geometry = pGeometry;//IPoint
                pGraphicsContainer.AddElement((IElement)pTextElement, 0);
            }
            else
            {
                pTextElement = null;
            }
            return pTextElement;
        }

        /// <summary>提取行驶路径的点信息，等距划分，距离为1米
        /// </summary>
        /// <param name="pGeometry">多线对象</param>
        /// <param name="pGeoCol">返回点数组</param>
        public static void MakeMultiPoint(IGeometry pGeometry,ref IPointCollection pGeoCol)
        {
            if (pGeometry.GeometryType == esriGeometryType.esriGeometryPolyline)
            {
                IGeometryCollection pGeometryCollection;
                IConstructGeometryCollection pCGeometryCollection = new GeometryBagClass();

                //等距均分
                //pCGeometryCollection.ConstructDivideEqual((IPolyline)pGeometry, nPoints - 1, esriConstructDivideEnum.esriDivideIntoPolylines);

                //以定值划分,0.001的单位为Km
                pCGeometryCollection.ConstructDivideLength((IPolyline)pGeometry, 0.001, true, esriConstructDivideEnum.esriDivideIntoPolylines);

                IEnumGeometry pEnumGeometry = (IEnumGeometry)pCGeometryCollection;
                pGeometryCollection = new MultipointClass();
                IPolyline pPolyline = (IPolyline)pEnumGeometry.Next();
                object Missing = Type.Missing;

                //提取指定值划分后的点信息
                pGeometryCollection.AddGeometry(pPolyline.FromPoint, ref Missing, ref Missing);
                while (pPolyline != null)
                {
                    pGeometryCollection.AddGeometry(pPolyline.ToPoint, ref Missing, ref Missing);
                    pPolyline = (IPolyline)pEnumGeometry.Next();
                }
                pGeoCol = (IPointCollection)pGeometryCollection;
            }
        }

       


        /// <summary>拷贝元素
        /// 
        /// </summary>
        /// <param name="pFeatureLayer">拷贝到的图层</param>
        /// <param name="FormFeature">被拷贝的元素</param>
        /// <returns>返回拷贝后的元素</returns>
        public static IFeature CopyFeature(IFeatureLayer pFeatureLayer, IFeature FormFeature)
        {
            IFeature pFeature = pFeatureLayer.FeatureClass.CreateFeature();
            pFeature.Shape = FormFeature.Shape;
            for (int i = 0; i < FormFeature.Fields.FieldCount; i++)
            {
                IField pField = FormFeature.Fields.get_Field(i);
                if (pField.Type != esriFieldType.esriFieldTypeGeometry && pField.Type != esriFieldType.esriFieldTypeOID && pField.Editable)
                {
                    pFeature.set_Value(i, FormFeature.get_Value(i));
                }
            }
            pFeature.Store();
            return pFeature;
        }


        public static void ZoomPoint(IPoint pPoint, double nMapScale)
        {
            IEnvelope pEnvelope;
            IDisplayTransformation pDisplayTransformation;
            pDisplayTransformation = EngineFuntions.m_AxMapControl.ActiveView.ScreenDisplay.DisplayTransformation;
            pEnvelope = pDisplayTransformation.VisibleBounds;
            pEnvelope.CenterAt(pPoint);
            pDisplayTransformation.VisibleBounds = pEnvelope;
            EngineFuntions.m_AxMapControl.Map.MapScale = nMapScale;
            pDisplayTransformation.VisibleBounds = EngineFuntions.m_AxMapControl.ActiveView.Extent;
            EngineFuntions.m_AxMapControl.ActiveView.ScreenDisplay.Invalidate(null, true, (short)esriScreenCache.esriAllScreenCaches);
        }

        public static bool GetLinkPoint(IPolyline pBeforeLine, IPolyline pAfterLine, ref IPoint PointLink)
        {
            IPoint PointBefore1, PointBefore2, PointAfter1, PointAfter2;
            PointBefore1 = pBeforeLine.FromPoint;
            PointBefore2 = pBeforeLine.ToPoint;
            PointAfter1 = pAfterLine.FromPoint;
            PointAfter2 = pAfterLine.ToPoint;
            if (PointBefore1.Compare(PointAfter1) == 0)
            {
                PointLink = PointAfter2;
            }
            else if (PointBefore1.Compare(PointAfter2) == 0)
            {
                PointLink = PointAfter1;
            }
            else if (PointBefore2.Compare(PointAfter1) == 0)
            {
                PointLink = PointAfter2;
            }
            else if (PointBefore2.Compare(PointAfter2) == 0)
            {
                PointLink = PointAfter1;
            }
            else
            {
                MessageBox.Show("线路不连续！\n", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        public static void FlashShape(IGeometry pGeo)
          {
              IRgbColor pColor = new RgbColorClass();
              pColor.Red = 0;
              pColor.Green = 0;
              pColor.Blue = 255;
            switch (pGeo.GeometryType)
            {
                case esriGeometryType.esriGeometryPoint:
                    ISimpleMarkerSymbol pSimpleMarkerSymbol;
                    pSimpleMarkerSymbol = new SimpleMarkerSymbolClass();
                    pSimpleMarkerSymbol.Color = pColor;
                    pSimpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
                    pSimpleMarkerSymbol.Size = 10;
                    m_AxMapControl.FlashShape(pGeo, 3, 500, pSimpleMarkerSymbol as ISymbol);
            	break;
                case esriGeometryType.esriGeometryPolyline:
                    ISimpleLineSymbol iLineSymbol = new SimpleLineSymbolClass();
                    ISymbol iSymbol;
                    iLineSymbol.Width = 4;
                    iLineSymbol.Color = pColor;
                    iSymbol = (ISymbol)iLineSymbol;
                    iSymbol.ROP2 = esriRasterOpCode.esriROPNotXOrPen;
                    m_AxMapControl.FlashShape(pGeo, 3, 500, iSymbol);
                break;
                case esriGeometryType.esriGeometryPolygon:
                    ISimpleFillSymbol iFillSymbol;
                    iFillSymbol = new SimpleFillSymbol();
                    iFillSymbol.Style = esriSimpleFillStyle.esriSFSSolid;
                    iFillSymbol.Outline.Width = 12;
                    iFillSymbol.Color = pColor;
                    iSymbol = (ISymbol)iFillSymbol;
                    iSymbol.ROP2 = esriRasterOpCode.esriROPNotXOrPen;
                    //iScreenDisplay.SetSymbol(iSymbol);
                    m_AxMapControl.FlashShape(pGeo, 3, 500, iSymbol);
                break;
            }
          }
        
        //public static List<IFeature> SortByDist(IFeature pFeature, List<IFeature> pFeatureCol)
        //{
        //    IPolyline pPolyline = (IPolyline)pFeature.Shape;
        //    IPoint pFormPoint = pPolyline.FromPoint;
        //    List<Double> rDistance = new List<double>();
        //    IPoint pToPoint;
        //    foreach (IFeature pfea in pFeatureCol)
        //    {
        //        pToPoint = (IPoint)pfea.Shape;
        //        rDistance.Add(System.Math.Sqrt((pFormPoint.X - pToPoint.X) * (pFormPoint.X - pToPoint.X) + (pFormPoint.Y - pToPoint.Y) * (pFormPoint.Y - pToPoint.Y)));
        //    }
        //    rDistance[0].Equals(rDistance[0]);
        //}
     }
    /*
    class ModEditOperations
    {
        public static ILayer m_pCurrentLayer;
        public static IMap m_pEditMap;
        public static IFeature m_pEditFeature;
        public static IPoint m_pPoint;
        public static IPoint m_pAnchorPoint;
        public static IDisplayFeedback m_pFeedback;
        public static Boolean m_bInUse;
        public static IPointCollection m_pPointCollection;
        //启动编辑
        public static void StartEditing()
        {
            IWorkspaceEdit pWorkspaceEdit;
            IFeatureLayer pFeatureLayer;
            IDataset pDataset;

            try
            {
                // Check edit conditions before allowing edit to start
                if (m_pCurrentLayer == null)
                    return;

                if (!(m_pCurrentLayer is IGeoFeatureLayer))
                    return;
                //If Not TypeOf m_pCurrentLayer Is IGeoFeatureLayer Then Exit Sub
                pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                pDataset = (IDataset)pFeatureLayer.FeatureClass;
                if (pDataset == null)
                    return;

                //Start editing, making sure that undo/redo are enabled
                pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                if (!pWorkspaceEdit.IsBeingEdited())
                {
                    pWorkspaceEdit.StartEditing(true);
                    pWorkspaceEdit.EnableUndoRedo();
                }
            }
            catch (System.Exception e)
            {
                MessageBox.Show("StartEditing--" + e.Message, "错误", MessageBoxButtons.OK);
            }
        }

        // Completes an editing session on the currently selected layer
        public static void StopEditing()
        {
            IWorkspaceEdit pWorkspaceEdit;
            IFeatureLayer pFeatureLayer;
            IDataset pDataset;
            IActiveView pActiveView;
            Boolean bHasEdits = false;
            Boolean bSave = false;
            try
            {
                // Check edit conditions before allowing edit to stop
                if (m_pCurrentLayer == null)
                    return;
                //If Not TypeOf m_pCurrentLayer Is IGeoFeatureLayer Then Exit Sub
                if (!(m_pCurrentLayer is IGeoFeatureLayer))
                    return;

                pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                pDataset = (IDataset)pFeatureLayer.FeatureClass;
                if (pDataset == null)
                    return;

                // If the current document has been edited then prompt the user to save changes
                pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                if (pWorkspaceEdit.IsBeingEdited())
                {
                    pWorkspaceEdit.HasEdits(ref bHasEdits);
                    bSave = false;
                    if (bHasEdits)
                    {
                        bSave = true;
                    }
                    pWorkspaceEdit.StopEditing(bSave);
                }

                pActiveView = (IActiveView)m_pEditMap;
                pActiveView.Refresh();

            }
            catch (System.Exception e)
            {
                MessageBox.Show("StartEditing--" + e.Message, "错误", MessageBoxButtons.OK);
            }
        }

        //取消修改
        public static void DiscardEditing()
        {
            IWorkspaceEdit pWorkspaceEdit;
            IFeatureLayer pFeatureLayer;
            IDataset pDataset;
            IActiveView pActiveView;
            Boolean bHasEdits = false;
            Boolean bSave = false;
            try
            {
                // Check edit conditions before allowing edit to stop
                if (m_pCurrentLayer == null)
                    return;
                //If Not TypeOf m_pCurrentLayer Is IGeoFeatureLayer Then Exit Sub
                if (!(m_pCurrentLayer is IGeoFeatureLayer))
                    return;

                pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                pDataset = (IDataset)pFeatureLayer.FeatureClass;
                if (pDataset == null)
                    return;

                // If the current document has been edited then prompt the user to save changes
                pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                if (pWorkspaceEdit.IsBeingEdited())
                {
                    pWorkspaceEdit.HasEdits(ref bHasEdits);
                    bSave = false;
                    if (bHasEdits)
                    {
                        bSave = true;
                    }
                    pWorkspaceEdit.StopEditing(bSave);
                }
                m_pEditMap.ClearSelection();
                pActiveView = (IActiveView)m_pEditMap;
                pActiveView.Refresh();
            }
            catch (System.Exception e)
            {
                MessageBox.Show("StartEditing--" + e.Message, "错误", MessageBoxButtons.OK);
            }
        }
        //Returns true if the startEdit button has been pressed
        public static Boolean InEdit()
        {
            IWorkspaceEdit pWorkspaceEdit;
            IFeatureLayer pFeatureLayer;
            IDataset pDataset;


            try
            {
                // Check edit conditions before allowing edit to stop
                if (m_pCurrentLayer == null)
                    return false;
                if (!(m_pCurrentLayer is IGeoFeatureLayer))
                    return false;
                //If Not TypeOf m_pCurrentLayer Is IGeoFeatureLayer Then Exit Sub
                pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                if (pFeatureLayer.FeatureClass == null)
                    return false;
                pDataset = (IDataset)pFeatureLayer.FeatureClass;
                if (pDataset == null)
                    return false;
                pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                if (pWorkspaceEdit.IsBeingEdited())
                    return true;

            }
            catch (System.Exception e)
            {
                MessageBox.Show("StartEditing--" + e.Message, "错误", MessageBoxButtons.OK);
            }
            return false;
        }

        //Starts a new sketch or adds a point to an existing one, of a type
        //determined by the current layer selected in the layers combo.
        public static void SketchMouseDown(int X, int Y)
        {
            IFeatureLayer pFeatureLayer;
            IPoint pPoint;
            IActiveView pActiveView;
            INewPolygonFeedback pPolyFeed;
            INewLineFeedback pLineFeed;
            try
            {
                // Check edit conditions before allowing edit to stop
                if (m_pCurrentLayer == null)
                    return;
                if (!(m_pCurrentLayer is IGeoFeatureLayer))
                    return;
                //Get the mouse down point in map coordinates
                pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                if (pFeatureLayer.FeatureClass == null)
                    return;
                pActiveView = (IActiveView)m_pEditMap;
                pPoint = pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                //If this is a fresh sketch then create an appropriate feedback object,
                //otherwise extent the existing feedback

                if (!m_bInUse)
                {
                    switch (pFeatureLayer.FeatureClass.ShapeType)
                    {
                        case esriGeometryType.esriGeometryPoint:
                            CreateFeature(pPoint);
                            break;
                        case esriGeometryType.esriGeometryMultipoint:
                            //m_bInUse = true;
                            //m_pFeedback = new NewMultiPointFeedback();
                            //INewMultiPointFeedback pMPFeed;
                            //pMPFeed = m_pFeedback;
                            //m_pPointCollection = new Multipoint();
                            //pMPFeed.Start(m_pPointCollection, pPoint);
                            break;
                        case esriGeometryType.esriGeometryPolyline:
                            //m_bInUse = true;
                            //m_pFeedback = new NewLineFeedback();
                            //pLineFeed = m_pFeedback;
                            //pLineFeed.Start(pPoint);
                            break;
                        case esriGeometryType.esriGeometryPolygon:
                            //m_bInUse = true;
                            //m_pFeedback = new NewPolygonFeedback();
                            //pPolyFeed = m_pFeedback;
                            //pPolyFeed.Start(pPoint);
                            break;
                    }
                    if (m_pFeedback != null)
                    {
                        m_pFeedback.Display = pActiveView.ScreenDisplay;
                    }


                }
                else
                {
                    if (m_pFeedback.GetType() == typeof(INewMultiPointFeedback))
                    {
                        //       m_pPointCollection.AddPoint(pPoint,ref );
                    }
                    else if (m_pFeedback.GetType() == typeof(INewLineFeedback))
                    {
                        pLineFeed = (INewLineFeedback)m_pFeedback;
                        //         pLineFeed.AddPoint(pPoint);
                    }
                    else if (m_pFeedback.GetType() == typeof(INewPolygonFeedback))
                    {
                        pPolyFeed = (INewPolygonFeedback)m_pFeedback;
                        //         pPolyFeed.AddPoint(pPoint);
                    }
                }
            }
            catch (System.Exception e)
            {
                MessageBox.Show("StartEditing--" + e.Message, "错误", MessageBoxButtons.OK);
            }
        }



        public static void SketchMouseMove(int X, int Y)
        {
            try
            {
                // Check edit conditions before allowing edit to stop
                if (!m_bInUse)
                    return;
                if (m_pFeedback == null)
                    return;
                //Move the feedback envelope and store the current mouse position
                IActiveView pActiveView;
                pActiveView = (IActiveView)m_pEditMap;
                m_pFeedback.MoveTo(pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y));
                m_pPoint = pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            }
            catch (System.Exception e)
            {
                MessageBox.Show("StartEditing--" + e.Message, "错误", MessageBoxButtons.OK);
            }
        }


        public static void CreateFeature(IGeometry pGeom)
        {
            IWorkspaceEdit pWorkspaceEdit;
            IFeatureLayer pFeatureLayer;
            IFeatureClass pFeatureClass;
            IFeature pFeature;
            try
            {
                if (pGeom == null)
                    return;
                if (m_pCurrentLayer == null)
                    return;
                //Create the feature
                pWorkspaceEdit = GetWorkspaceEdit();
                pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                pFeatureClass = pFeatureLayer.FeatureClass;
                pWorkspaceEdit.StartEditOperation();
                pFeature = pFeatureClass.CreateFeature();
                pFeature.Shape = pGeom;
                pFeature.Store();
                m_pEditFeature = pFeature;
                pWorkspaceEdit.StopEditOperation();
                //Select the feature that's been created
                //m_pEditMap.SelectFeature(m_pCurrentLayer, pFeature)
                //Refresh the relevant area of the active view
                IActiveView pActiveView;
                pActiveView = (IActiveView)m_pEditMap;
                if (pGeom.GeometryType == esriGeometryType.esriGeometryPoint)
                {
                    Double length;
                    length = ConvertPixelsToMapUnits((IActiveView)m_pEditMap, 30);
                    ITopologicalOperator pTopo;
                    pTopo = (ITopologicalOperator)pGeom;
                    IGeometry pBuffer;
                    pBuffer = pTopo.Buffer(length);
                    pActiveView.PartialRefresh((esriViewDrawPhase)(esriDrawPhase.esriDPGeography | esriDrawPhase.esriDPSelection), m_pCurrentLayer, pBuffer.Envelope);
                }
                else
                    pActiveView.PartialRefresh((esriViewDrawPhase)(esriDrawPhase.esriDPGeography | esriDrawPhase.esriDPSelection), m_pCurrentLayer, pGeom.Envelope);

            }
            catch (System.Exception e)
            {
                MessageBox.Show("StartEditing--" + e.Message, "错误", MessageBoxButtons.OK);
            }
        }

        //Gets the Workspace edit object from the current layer, if possible
        public static IWorkspaceEdit GetWorkspaceEdit()
        {
            IFeatureLayer pFeatureLayer;
            IFeatureClass pFeatureClass;
            IDataset pDataset;
            if (m_pCurrentLayer == null)
                return null;
            pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
            pFeatureClass = pFeatureLayer.FeatureClass;
            pDataset = (IDataset)pFeatureClass;
            if (pDataset == null)
                return null;
            return (IWorkspaceEdit)pDataset.Workspace;

        }

        //Uses the ratio of the size of the map in pixels to map units to do the conversion
        public static double ConvertPixelsToMapUnits(IActiveView pActiveView, Double pixelUnits)
        {
            double realWorldDisplayExtent;
            int pixelExtent;
            double sizeOfOnePixel;

            pixelExtent = pActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().right - pActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().left;
            realWorldDisplayExtent = pActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds.Width;
            sizeOfOnePixel = realWorldDisplayExtent / pixelExtent;
            return pixelUnits * sizeOfOnePixel;
        }

        //public static void SetValue(IFeature pFeature, string AliasName)
        //{
        //    double realWorldDisplayExtent;
        //    int pixelExtent;
        //    double sizeOfOnePixel;

        //    pixelExtent = pActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().right - pActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().left;
        //    realWorldDisplayExtent = pActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds.Width;
        //    sizeOfOnePixel = realWorldDisplayExtent / pixelExtent;
        //    return pixelUnits * sizeOfOnePixel;
        //}



    }
    ///  
    /// 使用本类可以新建点、线、面 
    /// 移动点、线、面 
    /// 编辑线、面的节点 
    /// 使用时需设置Map和CurrentLayer 
    /// 
    public class AoEditor
    {
        private ILayer m_pCurrentLayer;
        private IMap m_pMap;
        private IFeature m_pEditFeature;
        private IPoint m_pPoint;
        private IDisplayFeedback m_pFeedback;
        //		private ISelectionTracker m_pSelectionTracker; 
        private bool m_bInUse;
        private IPointCollection m_pPointCollection;

        ///  
        /// 当前图层,只写 
        ///  
        public ILayer CurrentLayer
        {
            set
            {
                m_pCurrentLayer = (ILayer)value;
            }
        }

        ///  
        /// 地图对象,只写 
        ///  
        public IMap Map
        {
            set
            {
                m_pMap = (IMap)value;
            }
        }

        ///  
        /// 构造函数 
        ///  
        public AoEditor()
        {

        }

        ///  
        /// 开始编辑,使工作空间处于可编辑状态 
        /// 在进行图层编辑前必须调用本方法 
        ///  
        public void StartEditing()
        {
            try
            {
                if (m_pCurrentLayer == null) return;

                if (!(m_pCurrentLayer is IGeoFeatureLayer)) return;

                IFeatureLayer pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                IDataset pDataset = (IDataset)pFeatureLayer.FeatureClass;
                if (pDataset == null) return;

                // 开始编辑,并设置Undo/Redo 为可用 
                IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                if (!pWorkspaceEdit.IsBeingEdited())
                {
                    pWorkspaceEdit.StartEditing(true);
                    pWorkspaceEdit.EnableUndoRedo();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        ///  
        /// 停止编辑，并将以前的编辑结果保存到数据文件中。 
        ///  
        public void StopEditing()
        {
            bool bHasEdits = false;
            bool bSave = false;

            try
            {
                if (m_pCurrentLayer == null) return;

                IFeatureLayer pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                if (pFeatureLayer.FeatureClass == null) return;

                IDataset pDataset = (IDataset)pFeatureLayer.FeatureClass;
                if (pDataset == null) return;

                //如果数据已被修改，则提示用户是否保存 
                IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                if (pWorkspaceEdit.IsBeingEdited())
                {
                    pWorkspaceEdit.HasEdits(ref bHasEdits);
                    if (bHasEdits)
                    {
                        DialogResult result;
                        result = MessageBox.Show("是否保存已做的修改?", "提示", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            bSave = true;
                        }
                    }
                    pWorkspaceEdit.StopEditing(bSave);
                }

                m_pMap.ClearSelection();
                IActiveView pActiveView = (IActiveView)m_pMap;
                pActiveView.Refresh();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }

        }

        ///  
        /// 检查工作空间中是否有数据处于编辑状态 
        ///  
        /// 是否正在编辑 
        public bool InEdit()
        {
            try
            {
                if (m_pCurrentLayer == null) return false;

                IFeatureLayer pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                if (pFeatureLayer.FeatureClass == null) return false;

                IDataset pDataset = (IDataset)pFeatureLayer.FeatureClass;
                if (pDataset == null) return false;

                IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                if (pWorkspaceEdit.IsBeingEdited()) return true;

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                return false;
            }
        }

        ///  
        /// 新建对象方法 
        /// 当前图层为点图层时，每调用一次就新点一个点对象 
        /// 当前图层为线图层或面图层时，第一次调用开始新建对象，并添加当前点， 
        /// 以后每调用一次，即向新对象中添加一个点,调用NewFeatureEnd方法完成对象创建 
        /// 在Map.MouseDown事件中调用本方法 
        ///  
        /// 鼠标X坐标，屏幕坐标 
        /// 鼠标Y坐标，屏幕坐标 
        public void NewFeatureMouseDown(int x, int y)
        {
            INewPolygonFeedback pPolyFeed;
            INewLineFeedback pLineFeed;

            try
            {
                if (m_pCurrentLayer == null) return;

                if (!(m_pCurrentLayer is IGeoFeatureLayer)) return;

                IFeatureLayer pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                if (pFeatureLayer.FeatureClass == null) return;

                IActiveView pActiveView = (IActiveView)m_pMap;
                IPoint pPoint = pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

                // 如果是新开始创建的对象，则相应的创建一个新的Feedback对象； 
                // 否则，向已存在的Feedback对象中加点 
                if (!m_bInUse)
                {
                    m_pMap.ClearSelection();  //清除地图选中对象 

                    switch (pFeatureLayer.FeatureClass.ShapeType)
                    {
                        case esriGeometryType.esriGeometryPoint:
                            CreateFeature(pPoint);
                            break;
                        case esriGeometryType.esriGeometryMultipoint:
                            m_bInUse = true;
                            m_pFeedback = new NewMultiPointFeedbackClass();
                            INewMultiPointFeedback pMPFeed = (INewMultiPointFeedback)m_pFeedback;
                            m_pPointCollection = new MultipointClass();
                            pMPFeed.Start(m_pPointCollection, pPoint);
                            break;
                        case esriGeometryType.esriGeometryPolyline:
                            m_bInUse = true;
                            m_pFeedback = new NewLineFeedbackClass();
                            pLineFeed = (INewLineFeedback)m_pFeedback;
                            pLineFeed.Start(pPoint);
                            break;
                        case esriGeometryType.esriGeometryPolygon:
                            m_bInUse = true;
                            m_pFeedback = new NewPolygonFeedbackClass();
                            pPolyFeed = (INewPolygonFeedback)m_pFeedback;
                            pPolyFeed.Start(pPoint);
                            break;
                    }

                    if (m_pFeedback != null)
                        m_pFeedback.Display = pActiveView.ScreenDisplay;
                }
                else
                {
                    if (m_pFeedback is INewMultiPointFeedback)
                    {
                        object obj = System.Reflection.Missing.Value;
                        m_pPointCollection.AddPoint(pPoint, ref obj, ref obj);
                    }
                    else if (m_pFeedback is INewLineFeedback)
                    {
                        pLineFeed = (INewLineFeedback)m_pFeedback;
                        pLineFeed.AddPoint(pPoint);
                    }
                    else if (m_pFeedback is INewPolygonFeedback)
                    {
                        pPolyFeed = (INewPolygonFeedback)m_pFeedback;
                        pPolyFeed.AddPoint(pPoint);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        ///  
        /// 新建对象过程中鼠标移动方法,产生Track效果 
        /// 在Map.MouseMove事件中调用本方法 
        ///  
        /// 鼠标X坐标，屏幕坐标 
        /// 鼠标Y坐标，屏幕坐标 
        public void NewFeatureMouseMove(int x, int y)
        {
            if ((!m_bInUse) || (m_pFeedback == null)) return;

            IActiveView pActiveView = (IActiveView)m_pMap;
            m_pPoint = pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            m_pFeedback.MoveTo(m_pPoint);
        }

        ///  
        /// 完成新建对象，取得绘制的对象，并添加到图层中 
        /// 建议在Map.DblClick或Map.MouseDown(Button = 2)事件中调用本方法 
        ///  
        public void NewFeatureEnd()
        {
            IGeometry pGeom = null;
            IPointCollection pPointCollection;

            try
            {
                if (m_pFeedback is INewMultiPointFeedback)
                {
                    INewMultiPointFeedback pMPFeed = (INewMultiPointFeedback)m_pFeedback;
                    pMPFeed.Stop();
                    pGeom = (IGeometry)m_pPointCollection;
                }
                else if (m_pFeedback is INewLineFeedback)
                {
                    INewLineFeedback pLineFeed = (INewLineFeedback)m_pFeedback;

                    pLineFeed.AddPoint(m_pPoint);
                    IPolyline pPolyLine = pLineFeed.Stop();

                    pPointCollection = (IPointCollection)pPolyLine;
                    if (pPointCollection.PointCount < 2)
                        MessageBox.Show("至少输入两个节点");
                    else
                        pGeom = (IGeometry)pPointCollection;
                }
                else if (m_pFeedback is INewPolygonFeedback)
                {
                    INewPolygonFeedback pPolyFeed = (INewPolygonFeedback)m_pFeedback;
                    pPolyFeed.AddPoint(m_pPoint);

                    IPolygon pPolygon;
                    pPolygon = pPolyFeed.Stop();
                    if (pPolygon != null)
                    {
                        pPointCollection = (IPointCollection)pPolygon;
                        if (pPointCollection.PointCount < 3)
                            MessageBox.Show("至少输入三个节点");
                        else
                            pGeom = (IGeometry)pPointCollection;
                    }
                }

                CreateFeature(pGeom);
                m_pFeedback = null;
                m_bInUse = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        ///  
        /// 查询当前图层中鼠标位置处的地图对象 
        /// 建议在Map.MouseDown事件中调用本方法 
        ///  
        /// 鼠标X坐标，屏幕坐标 
        /// 鼠标Y坐标，屏幕坐标 
        public void SelectMouseDown(int x, int y)
        {

            ISpatialFilter pSpatialFilter;
            IQueryFilter pFilter;

            try
            {
                if (m_pCurrentLayer == null) return;
                if (!(m_pCurrentLayer is IGeoFeatureLayer)) return;

                IFeatureLayer pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                if (pFeatureClass == null) return;

                IActiveView pActiveView = (IActiveView)m_pMap;
                IPoint pPoint = pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                IGeometry pGeometry = pPoint;

                // 设置查询缓冲区 
                double length = ConvertPixelsToMapUnits(pActiveView, 4.0);
                ITopologicalOperator pTopo = (ITopologicalOperator)pGeometry;
                IGeometry pBuffer = pTopo.Buffer(length);
                pGeometry = pBuffer.Envelope;

                //设置过滤器对象 
                pSpatialFilter = new SpatialFilterClass();
                pSpatialFilter.Geometry = pGeometry;

                switch (pFeatureClass.ShapeType)
                {
                    case esriGeometryType.esriGeometryPoint:
                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                        break;
                    case esriGeometryType.esriGeometryPolyline:
                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                        break;
                    case esriGeometryType.esriGeometryPolygon:
                        pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        break;
                }
                pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;
                pFilter = pSpatialFilter;

                // 查询 
                IFeatureCursor pCursor = pFeatureLayer.Search(pFilter, false);

                // 在地图上高亮显示查询结果 
                IFeature pFeature = pCursor.NextFeature();
                while (pFeature != null)
                {
                    m_pMap.SelectFeature(m_pCurrentLayer, pFeature);
                    pFeature = pCursor.NextFeature();
                }
                pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        ///  
        /// 编辑当前图层中鼠标击中的地图对象(开始编辑), 
        /// 如果为点对象，可进行位置移动，如果为线对象或面对象，可进行节点编辑 
        /// 建议在Map.MouseDown事件中调用本方法 
        ///  
        /// 鼠标X坐标，屏幕坐标 
        /// 鼠标Y坐标，屏幕坐标 
        ///  
        public bool EditFeatureMouseDown(int x, int y)
        {
            IGeometryCollection pGeomColn;
            IPointCollection pPointColn;
            IObjectClass pObjectClass;
            IFeature pFeature;
            IGeometry pGeom;

            IPath pPath;
            IPoint pHitPoint = null;
            IPoint pPoint = null;
            Double hitDist = 0.0;
            double tol;
            int vertexIndex = 0;
            int numVertices;
            int partIndex = 0;
            bool vertex = false;

            try
            {

                m_pMap.ClearSelection();

                // 取得鼠标击中的第一个对象 
                SelectMouseDown(x, y);
                IEnumFeature pSelected = (IEnumFeature)m_pMap.FeatureSelection;
                pFeature = pSelected.Next();
                if (pFeature == null) return false;

                IActiveView pActiveView = (IActiveView)m_pMap;
                pPoint = pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

                // 节点空间查询容差 
                tol = ConvertPixelsToMapUnits(pActiveView, 4.0);

                pGeom = pFeature.Shape;
                pObjectClass = pFeature.Class;
                m_pEditFeature = pFeature;
                object objNull = System.Reflection.Missing.Value;
                object objBefore, objAfter;

                switch (pGeom.GeometryType)
                {
                    case esriGeometryType.esriGeometryPoint:
                        m_pFeedback = new MovePointFeedbackClass();
                        m_pFeedback.Display = pActiveView.ScreenDisplay;
                        IMovePointFeedback pPointMove = (IMovePointFeedback)m_pFeedback;
                        pPointMove.Start((IPoint)pGeom, pPoint);
                        break;
                    case esriGeometryType.esriGeometryPolyline:
                        if (TestGeometryHit(tol, pPoint, pFeature, pHitPoint, hitDist, out partIndex, out vertexIndex, out vertex))
                        {
                            if (!vertex)
                            {
                                pGeomColn = (IGeometryCollection)pGeom;
                                pPath = (IPath)pGeomColn.get_Geometry(partIndex);
                                pPointColn = (IPointCollection)pPath;
                                numVertices = pPointColn.PointCount;

                                if (vertexIndex == 0)
                                {
                                    objBefore = (object)(vertexIndex + 1);
                                    pPointColn.AddPoint(pPoint, ref objBefore, ref objNull);
                                }
                                else
                                {
                                    objAfter = (object)vertexIndex;
                                    pPointColn.AddPoint(pPoint, ref objNull, ref objAfter);
                                }

                                TestGeometryHit(tol, pPoint, pFeature, pHitPoint, hitDist, out partIndex, out vertexIndex, out vertex);
                            }
                            m_pFeedback = new LineMovePointFeedbackClass();
                            m_pFeedback.Display = pActiveView.ScreenDisplay;
                            ILineMovePointFeedback pLineMove = (ILineMovePointFeedback)m_pFeedback;
                            pLineMove.Start((IPolyline)pGeom, vertexIndex, pPoint);

                            //m_pSelectionTracker = new LineTrackerClass();
                            //m_pSelectionTracker.Display = pActiveView.ScreenDisplay;
                            //m_pSelectionTracker.Geometry = pGeom;
                            //m_pSelectionTracker.ShowHandles = true;
                            //m_pSelectionTracker.QueryResizeFeedback(ref m_pFeedback);
                            //m_pSelectionTracker.OnMouseDown(1, 0, x, y); 							 
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case esriGeometryType.esriGeometryPolygon:
                        if (TestGeometryHit(tol, pPoint, pFeature, pHitPoint, hitDist, out partIndex, out vertexIndex, out vertex))
                        {
                            if (!vertex)
                            {
                                pGeomColn = (IGeometryCollection)pGeom;
                                pPath = (IPath)pGeomColn.get_Geometry(partIndex);
                                pPointColn = (IPointCollection)pPath;
                                numVertices = pPointColn.PointCount;

                                if (vertexIndex == 0)
                                {
                                    objBefore = (object)(vertexIndex + 1);
                                    pPointColn.AddPoint(pPoint, ref objBefore, ref objNull);
                                }
                                else
                                {
                                    objAfter = (object)vertexIndex;
                                    pPointColn.AddPoint(pPoint, ref objNull, ref objAfter);
                                }

                                TestGeometryHit(tol, pPoint, pFeature, pHitPoint, hitDist, out partIndex, out vertexIndex, out vertex);
                            }

                            m_pFeedback = new PolygonMovePointFeedbackClass();
                            m_pFeedback.Display = pActiveView.ScreenDisplay;
                            IPolygonMovePointFeedback pPolyMove = (IPolygonMovePointFeedback)m_pFeedback;
                            pPolyMove.Start((IPolygon)pGeom, vertexIndex, pPoint);
                        }
                        else
                        {
                            return false;
                        }
                        break;
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                return false;
            }
        }

        ///  
        /// 编辑地图对象过程中的鼠标移动事件, 
        /// 如果为点对象，进行位置移动 
        /// 如果为线对象或面对象，进行节点移动 
        /// 建议在Map.MouseMove事件中调用本方法 
        ///  
        /// 鼠标X坐标，屏幕坐标 
        /// 鼠标Y坐标，屏幕坐标 
        public void EditFeatureMouseMove(int x, int y)
        {
            try
            {
                if (m_pFeedback == null) return;

                IActiveView pActiveView = (IActiveView)m_pMap;
                IPoint pPoint = pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                m_pFeedback.MoveTo(pPoint);

                //				if (m_pSelectionTracker !=null) m_pSelectionTracker.OnMouseMove(1,0,x,y);  
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        ///  
        /// 完成地图对象编辑，取得编辑后的对象，并将其更新到图层中 
        /// 建议在Map.MouseUp事件中调用本方法 
        ///  
        public void EditFeatureEnd()
        {
            IGeometry pGeometry;
            try
            {
                if (m_pFeedback == null) return;

                if (m_pFeedback is IMovePointFeedback)
                {
                    IMovePointFeedback pPointMove = (IMovePointFeedback)m_pFeedback;
                    pGeometry = pPointMove.Stop();
                    UpdateFeature(m_pEditFeature, pGeometry);
                }
                else if (m_pFeedback is ILineMovePointFeedback)
                {
                    ILineMovePointFeedback pLineMove = (ILineMovePointFeedback)m_pFeedback;
                    pGeometry = pLineMove.Stop();
                    UpdateFeature(m_pEditFeature, pGeometry);
                }
                else if (m_pFeedback is IPolygonMovePointFeedback)
                {
                    IPolygonMovePointFeedback pPolyMove = (IPolygonMovePointFeedback)m_pFeedback;
                    pGeometry = pPolyMove.Stop();
                    UpdateFeature(m_pEditFeature, pGeometry);
                }

                m_pFeedback = null;
                //				m_pSelectionTracker = null; 
                IActiveView pActiveView = (IActiveView)m_pMap;
                pActiveView.Refresh();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        ///  
        /// 移动当前图层中鼠标击中地图对象的位置（开始移动） 
        /// 建议在Map.MouseDown事件中调用本方法 
        ///  
        /// 鼠标X坐标，屏幕坐标 
        /// 鼠标Y坐标，屏幕坐标 
        ///  
        public bool MoveFeatureMouseDown(int x, int y)
        {
            try
            {
                m_pMap.ClearSelection();

                SelectMouseDown(x, y);
                IEnumFeature pSelected = (IEnumFeature)m_pMap.FeatureSelection;
                IFeature pFeature = pSelected.Next();
                if (pFeature == null) return false;

                IActiveView pActiveView = (IActiveView)m_pMap;
                IPoint pPoint = pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

                IGeometry pGeom = pFeature.Shape;
                m_pEditFeature = pFeature;

                switch (pGeom.GeometryType)
                {
                    case esriGeometryType.esriGeometryPoint:
                        m_pFeedback = new MovePointFeedbackClass();
                        m_pFeedback.Display = pActiveView.ScreenDisplay;
                        IMovePointFeedback pPointMove = (IMovePointFeedback)m_pFeedback;
                        pPointMove.Start((IPoint)pGeom, pPoint);
                        break;
                    case esriGeometryType.esriGeometryPolyline:

                        m_pFeedback = new MoveLineFeedbackClass();
                        m_pFeedback.Display = pActiveView.ScreenDisplay;
                        IMoveLineFeedback pLineMove = (IMoveLineFeedback)m_pFeedback;
                        pLineMove.Start((IPolyline)pGeom, pPoint);
                        break;
                    case esriGeometryType.esriGeometryPolygon:
                        m_pFeedback = new MovePolygonFeedbackClass();
                        m_pFeedback.Display = pActiveView.ScreenDisplay;
                        IMovePolygonFeedback pPolyMove = (IMovePolygonFeedback)m_pFeedback;
                        pPolyMove.Start((IPolygon)pGeom, pPoint);
                        break;
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                return false;
            }
        }

        ///  
        /// 移动地图对象过程中的鼠标移动事件 
        /// 建议在Map.MouseMove事件中调用本方法 
        ///  
        /// 鼠标X坐标，屏幕坐标 
        /// 鼠标Y坐标，屏幕坐标 
        public void MoveFeatureMouseMove(int x, int y)
        {
            try
            {
                if (m_pFeedback == null) return;

                IActiveView pActiveView = (IActiveView)m_pMap;
                IPoint pPoint = pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                m_pFeedback.MoveTo(pPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        ///  
        /// 完成地图对象移动，取得移动后的对象，并将其更新到图层中 
        /// 建议在Map.MouseUp事件中调用本方法 
        ///  
        public void MoveFeatureEnd()
        {
            IGeometry pGeometry;

            try
            {
                if (m_pFeedback == null) return;

                if (m_pFeedback is IMovePointFeedback)
                {
                    IMovePointFeedback pPointMove = (IMovePointFeedback)m_pFeedback;
                    pGeometry = pPointMove.Stop();
                    UpdateFeature(m_pEditFeature, pGeometry);
                }
                else if (m_pFeedback is IMoveLineFeedback)
                {
                    IMoveLineFeedback pLineMove = (IMoveLineFeedback)m_pFeedback;
                    pGeometry = pLineMove.Stop();
                    UpdateFeature(m_pEditFeature, pGeometry);
                }
                else if (m_pFeedback is IMovePolygonFeedback)
                {
                    IMovePolygonFeedback pPolyMove = (IMovePolygonFeedback)m_pFeedback;
                    pGeometry = pPolyMove.Stop();
                    UpdateFeature(m_pEditFeature, pGeometry);
                }

                m_pFeedback = null;
                IActiveView pActiveView = (IActiveView)m_pMap;
                pActiveView.Refresh();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        ///  
        /// 删除当前图层中选中的地图对象 
        ///  
        public void DeleteSelectedFeature()
        {
            try
            {
                if (m_pCurrentLayer == null) return;

                IFeatureCursor pFeatureCursor = GetSelectedFeatures();
                if (pFeatureCursor == null) return;

                m_pMap.ClearSelection();

                IWorkspaceEdit pWorkspaceEdit = GetWorkspaceEdit();
                pWorkspaceEdit.StartEditOperation();
                IFeature pFeature = pFeatureCursor.NextFeature();

                while (pFeature != null)
                {
                    pFeature.Delete();
                    pFeature = pFeatureCursor.NextFeature();
                }

                pWorkspaceEdit.StopEditOperation();

                IActiveView pActiveView = (IActiveView)m_pMap;
                pActiveView.Refresh();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        ///  
        /// 撤消以前所做的编辑 
        ///  
        public void UndoEdit()
        {
            bool bHasUndos = false;

            try
            {
                if (m_pCurrentLayer == null) return;

                IFeatureLayer pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                IDataset pDataset = (IDataset)pFeatureLayer.FeatureClass;
                if (pDataset == null) return;

                IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                pWorkspaceEdit.HasUndos(ref bHasUndos);
                if (bHasUndos) pWorkspaceEdit.UndoEditOperation();

                IActiveView pActiveView = (IActiveView)m_pMap;
                pActiveView.Refresh();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        ///  
        /// 重做已撤消的编辑 
        ///  
        public void RedoEdit()
        {
            bool bHasRedos = false;

            try
            {
                if (m_pCurrentLayer == null) return;

                IFeatureLayer pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                IDataset pDataset = (IDataset)pFeatureLayer.FeatureClass;
                if (pDataset == null) return;

                IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                pWorkspaceEdit.HasRedos(ref bHasRedos);
                if (bHasRedos) pWorkspaceEdit.RedoEditOperation();

                IActiveView pActiveView = (IActiveView)m_pMap;
                pActiveView.Refresh();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        ///  
        /// 向图层中添加新的地图对象，并使之处于选中状态 
        ///  
        /// 图形对象 
        private void CreateFeature(IGeometry pGeom)
        {
            try
            {
                if (pGeom == null) return;
                if (m_pCurrentLayer == null) return;

                IWorkspaceEdit pWorkspaceEdit = GetWorkspaceEdit();
                IFeatureLayer pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;

                pWorkspaceEdit.StartEditOperation();
                IFeature pFeature = pFeatureClass.CreateFeature();
                pFeature.Shape = pGeom;
                pFeature.Store();
                pWorkspaceEdit.StopEditOperation();

                m_pMap.SelectFeature(m_pCurrentLayer, pFeature);

                IActiveView pActiveView = (IActiveView)m_pMap;
                pActiveView.Refresh();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        ///  
        /// 瘵屏幕坐标转换为地图坐标 
        ///  
        /// 地图 
        /// 屏幕坐标 
        /// 地图坐标 
        private double ConvertPixelsToMapUnits(IActiveView pActiveView, double pixelUnits)
        {
            tagRECT pRect = pActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame();
            int pixelExtent = pRect.right - pRect.left;

            double realWorldDisplayExtent = pActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds.Width;
            double sizeOfOnePixel = realWorldDisplayExtent / pixelExtent;
            return pixelUnits * sizeOfOnePixel;
        }

        ///  
        /// 取得当前图层所在的工作空间 
        ///  
        /// 工作空间 
        private IWorkspaceEdit GetWorkspaceEdit()
        {
            if (m_pCurrentLayer == null) return null;

            IFeatureLayer pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
            IDataset pDataset = (IDataset)pFeatureClass;
            if (pDataset == null)
                return null;
            else
                return (IWorkspaceEdit)pDataset.Workspace;
        }

        ///  
        /// 取得选中的地图对象集合 
        ///  
        /// 地图对象游标 
        private IFeatureCursor GetSelectedFeatures()
        {
            if (m_pCurrentLayer == null) return null;

            IFeatureSelection pFeatSel = (IFeatureSelection)m_pCurrentLayer;
            ISelectionSet pSelectionSet = pFeatSel.SelectionSet;

            if (pSelectionSet.Count == 0)
            {
                return null;
            }

            ICursor pCursor;
            pSelectionSet.Search(null, false, out pCursor);
            return (IFeatureCursor)pCursor;
        }
        /// 测试是否击中地图对象或地图对象上的节点 
        ///  
        /// 查询容差 
        /// 点击位置 
        /// 测试对象 
        /// 查询目标点 
        /// 目标点与点击点距离 
        /// 节索引 
        /// 点索引 
        /// 是否击中点 
        /// 是否击中测试对象 
        private bool TestGeometryHit(double tolerance, IPoint pPoint, IFeature pFeature, IPoint pHitPoint,
            double hitDist, out int partIndex, out int vertexIndex, out bool vertexHit)
        {
            try
            {
                IGeometry pGeom = pFeature.Shape;
                IHitTest pHitTest = (IHitTest)pGeom;
                pHitPoint = new PointClass();
                bool bRes = true;

                partIndex = 0;
                vertexIndex = 0;
                vertexHit = false;
                // 检查节点是否被击中 
                if (pHitTest.HitTest(pPoint, tolerance, esriGeometryHitPartType.esriGeometryPartVertex, pHitPoint,
                    ref hitDist, ref partIndex, ref vertexIndex, ref bRes))
                {
                    vertexHit = true;
                    return true;
                }
                // 检边界是否被击中 
                else
                {
                    if (pHitTest.HitTest(pPoint, tolerance, esriGeometryHitPartType.esriGeometryPartBoundary, pHitPoint,
                        ref hitDist, ref partIndex, ref vertexIndex, ref bRes))
                    {
                        vertexHit = false;
                        return true;
                    }
                }
                return false;
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                partIndex = 0;
                vertexIndex = 0;
                vertexHit = false;
                return false;
            }
        }

        ///  
        /// 向图层中更新新的地图对象，并使之处于选中状态 
        ///  
        ///  
        ///  
        private void UpdateFeature(IFeature pFeature, IGeometry pGeometry)
        {
            try
            {
                IDataset pDataset = (IDataset)pFeature.Class;
                IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                if (!pWorkspaceEdit.IsBeingEdited())
                {
                    MessageBox.Show("当前图层不可编辑");
                    return;
                }

                pWorkspaceEdit.StartEditOperation();
                pFeature.Shape = pGeometry;
                pFeature.Store();
                pWorkspaceEdit.StopEditOperation();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
            }
        }

    }
    */
}
