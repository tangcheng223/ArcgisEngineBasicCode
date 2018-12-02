using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POI推荐
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            axTOCControl1.SetBuddyControl(axMapControl1);
        }

        private void sysbnt_closesys_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DialogResult dr = MessageBox.Show("确定退出系统吗？", "！", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Hand);
            if (dr == DialogResult.Yes)
            {
                Environment.Exit(0);
            }
        }

        private void axMapControl1_OnMouseDown(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e)
        {
            #region 多边形选择工具
            if (strTool == "多边形选择" && e.button == 1)
            {
                try
                {
                    IMapControlDefault pMCD;
                    pMCD = axMapControl1.Object as IMapControlDefault;
                    IMap pMap;
                    pMap = pMCD.Map;
                    IGeometry pGeom;
                    pGeom = pMCD.TrackPolygon();
                    pMap.SelectByShape(pGeom, null, false);
                    pMCD.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                }
                catch { }
            }
            #endregion

            #region 圆选择
            if (strTool == "圆选择" && e.button == 1)
            {
                IMapControlDefault pMCD;
                pMCD = axMapControl1.Object as IMapControlDefault;
                IMap pMap;
                pMap = pMCD.Map;
                IGeometry pGeom;
                pGeom = pMCD.TrackCircle();
                pMap.SelectByShape(pGeom, null, false);
                pMCD.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            }
            #endregion

            //图上度量单位与实际单位换算，用于面积测量与长度测量
            IPoint pPoint = null;
            double x;
            x = (6371110 * 2 * Math.PI) / 360;
            IActiveView pActiveView = axMapControl1.ActiveView.FocusMap as IActiveView;
            pPoint = pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);

            #region 面积测量
            if (strTool == "面积量测" && e.button == 1)
            {
                try
                {
                    IMapControlDefault pMCD;
                    pMCD = axMapControl1.Object as IMapControlDefault;
                    IMap pMap;
                    pMap = pMCD.Map;
                    IGeometry pGeom;
                    pGeom = pMCD.TrackPolygon();
                    //画多边形
                    IPolygon pPolygon = pGeom as IPolygon;
                    IFillShapeElement pPolygonElement = new PolygonElementClass();
                    ISimpleFillSymbol pSimpleFillSymbol = new SimpleFillSymbolClass();
                    pSimpleFillSymbol.Style = esriSimpleFillStyle.esriSFSBackwardDiagonal;

                    IRgbColor pColor = new RgbColorClass();
                    pColor.Red = 175;
                    pColor.Green = 0;
                    pColor.Blue = 175;
                    pSimpleFillSymbol.Color = pColor as IColor;
                    pPolygonElement.Symbol = pSimpleFillSymbol;
                    IElement pElement = pPolygonElement as IElement;
                    pElement.Geometry = pPolygon;
                    IGraphicsContainer pGraphicsContainer = pMap as IGraphicsContainer;
                    pGraphicsContainer.AddElement(pElement, 0);
                    axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                    IArea pArea = (IArea)pGeom;
                    double s;
                    s = Math.Abs(Math.Round(pArea.Area * x * x / 1000000, 2));
                    DialogResult dr = MessageBox.Show("您量测的面积为" + s.ToString() + "平方公里。", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (dr == DialogResult.OK)
                    {
                        axMapControl1.ActiveView.GraphicsContainer.DeleteAllElements();
                    }
                    axMapControl1.Refresh();
                }
                catch { }
            }
            #endregion

            #region 长度测量
            if (strTool == "长度量测" && e.button == 1)
            {
                IPolyline pPolyline = null;
                pPolyline = (IPolyline)axMapControl1.TrackLine();
                double l;
                l = Math.Abs(Math.Round(pPolyline.Length * x / 1000, 2));
                //画直线
                ILineElement pLineElement = new LineElementClass();
                ISimpleLineSymbol pSimpleLineSymbol = new SimpleLineSymbol();
                IRgbColor pColor = new RgbColorClass();
                pColor.Red = 175;
                pColor.Green = 0;
                pColor.Blue = 175;
                pSimpleLineSymbol.Color = pColor as IColor;
                pSimpleLineSymbol.Width = 3;
                pSimpleLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                pLineElement.Symbol = pSimpleLineSymbol;
                IElement pElement = pLineElement as IElement;
                pElement.Geometry = pPolyline;
                axMapControl1.ActiveView.GraphicsContainer.AddElement(pElement, 0);
                axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                DialogResult dr = MessageBox.Show("您量测的距离为" + l.ToString() + "公里。", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (dr == DialogResult.OK)
                {
                    axMapControl1.ActiveView.GraphicsContainer.DeleteAllElements();
                }
                axMapControl1.Refresh();
            }
            #endregion

        }
        string initPath = "";
        private void Form1_Load(object sender, EventArgs e)
        {
            int index = Environment.CurrentDirectory.IndexOf(@"\bin");
            initPath = Environment.CurrentDirectory.Substring(0, index);
            //加载地图（相对路径）
            axMapControl1.LoadMxFile(initPath + @"\map\biye.mxd");
            ICommand Cmd = new ControlsMapFullExtentCommand();
            Cmd.OnCreate(this.axMapControl1.Object);
            Cmd.OnClick();
            //加载鹰眼图（相对路径）
            axMapControl2.LoadMxFile(initPath + @"\map\biye.mxd");
            ICommand Cmd1 = new ControlsMapFullExtentCommand();
            Cmd1.OnCreate(this.axMapControl2.Object);
            Cmd1.OnClick();
        }

        private void axMapControl1_OnExtentUpdated(object sender, IMapControlEvents2_OnExtentUpdatedEvent e)
        {
            //定义边界对象
            IEnvelope pEnv;
            pEnv = e.newEnvelope as IEnvelope;
            IGraphicsContainer pGraphicsContainer;
            IActiveView pActiveView;
            //获取鹰眼图地图数据的图形容器句柄
            pGraphicsContainer = axMapControl2.Map as IGraphicsContainer;
            pActiveView = pGraphicsContainer as IActiveView;
            pGraphicsContainer.DeleteAllElements();
            IRectangleElement pRectangleEle;
            pRectangleEle = new RectangleElementClass();
            IElement pEle;
            pEle = pRectangleEle as IElement;
            pEle.Geometry = pEnv;
            IRgbColor pColor;
            pColor = new RgbColor();
            pColor.RGB = 255;
            pColor.Transparency = 255;
            ILineSymbol pOutline;
            pOutline = new SimpleLineSymbol();
            pOutline.Width = 1;
            pOutline.Color = pColor;
            pColor = new RgbColor();
            pColor.RGB = 255;
            pColor.Transparency = 0;
            IFillSymbol pFillSymbol;
            pFillSymbol = new SimpleFillSymbol();
            pFillSymbol.Color = pColor;
            pFillSymbol.Outline = pOutline;
            IFillShapeElement pFillshapeEle;
            pFillshapeEle = pEle as IFillShapeElement;
            pFillshapeEle.Symbol = pFillSymbol;
            pEle = pFillshapeEle as IElement;
            pGraphicsContainer.AddElement(pEle, 0);
            pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        private void axMapControl2_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            IPoint Pnt;
            Pnt = new ESRI.ArcGIS.Geometry.Point();
            Pnt.PutCoords(e.mapX, e.mapY);
            axMapControl1.CenterAt(Pnt);
            axMapControl1.Refresh();
        }
        #region  全局变量
        ILayer layer;
        ILayer pMoveLayer;//控制toc图层移动事件的layer
        int toIndex;


        #endregion
        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            //右键toc图层出现菜单和图层关联
            axTOCControl1.ContextMenuStrip = null;
            IBasicMap map = new MapClass();
            System.Object other = null;
            System.Object index = null;
            esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
            axTOCControl1.HitTest(e.x, e.y, ref item, ref map, ref layer, ref other, ref index);
            if (item == esriTOCControlItem.esriTOCControlItemLayer && e.button == 2)
            {
                System.Drawing.Point pt = new System.Drawing.Point();
                pt.X = e.x;
                pt.Y = e.y;
                pt = this.axTOCControl1.PointToScreen(pt);
                this.contextMenuStrip1.Show(pt);
            }
            else if (item == esriTOCControlItem.esriTOCControlItemLayer && e.button == 1)
            {
                if (layer is IAnnotationSublayer)   //注记层在表层，不参与移动
                    return;
                else
                    pMoveLayer = layer;
            }
        }

        private void axTOCControl1_OnMouseUp(object sender, ITOCControlEvents_OnMouseUpEvent e)
        {
            if (e.button == 1)
            {
                esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap map = null;
                ILayer layer = null;
                object other = null;
                object index = null;
                axTOCControl1.HitTest(e.x, e.y, ref item, ref map, ref layer, ref other, ref index);
                IMap pMap = axMapControl1.ActiveView.FocusMap;
                if (item == esriTOCControlItem.esriTOCControlItemLayer || layer != null)
                {
                    //预移动图层和鼠标当前位置图层不一致时
                    if (pMoveLayer != layer)
                    {
                        ILayer pTempLayer;
                        for (int i = 0; i < pMap.LayerCount; i++)
                        {
                            pTempLayer = pMap.get_Layer(i);
                            //获取鼠标当前位置图层的索引值
                            if (pTempLayer == layer)
                            {
                                toIndex = i;
                            }
                        }
                        try
                        {
                            pMap.MoveLayer(pMoveLayer, toIndex);
                            axMapControl1.ActiveView.Refresh();
                            axTOCControl1.Update();
                        }
                        catch { }
                    }
                }
            }
        }

        private void mxd_open_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = initPath;
            ofd.Filter = "Map Document|*.mxd||*.*";
            ofd.RestoreDirectory = true;
            ofd.FilterIndex = 1;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                IMapControlDefault pMCD;//定义一个接口
                pMCD = axMapControl1.Object as IMapControlDefault;//向上转型，实现接口的方法
                string strFileName = ofd.FileName;
                bool bRet = pMCD.CheckMxFile(strFileName);
                if (bRet)
                {
                    pMCD.LoadMxFile(strFileName, null, Type.Missing);
                    ICommand Cmd = new ControlsMapFullExtentCommand();
                    Cmd.OnCreate(this.axMapControl1.Object);
                    Cmd.OnClick();
                }
                pMCD = axMapControl2.Object as IMapControlDefault;
                if (bRet)
                {
                    pMCD.LoadMxFile(strFileName, null, Type.Missing);
                    ICommand Cmd = new ControlsMapFullExtentCommand();
                    Cmd.OnCreate(this.axMapControl2.Object);
                    Cmd.OnClick();
                }
            }
        }

        private void mxd_save_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            IMxdContents pMxdC;

            pMxdC = axMapControl1.Map as IMxdContents;

            IMapDocument pMapDocument = new MapDocumentClass();

            pMapDocument.Open(axMapControl1.DocumentFilename, "");

            IActiveView pActiveView = axMapControl1.Map as IActiveView;

            pMapDocument.ReplaceContents(pMxdC);

            pMapDocument.Save(true, true);

            MessageBox.Show("保存成功！");
        }

        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            IMxdContents pMxdC;
            pMxdC = axMapControl1.Map as IMxdContents;
            IMapDocument pMapDocument;
            pMapDocument = new MapDocumentClass();
            pMapDocument.Open(axMapControl1.DocumentFilename, "");
            pMapDocument.ReplaceContents(pMxdC);
            SaveFileDialog sfd = new SaveFileDialog();
            //sfd.InitialDirectory = "D:\\esri\\chart\\辽东湾海图\\";
            sfd.InitialDirectory = initPath;
            sfd.Title = "地图文档另存";
            sfd.Filter = "地图文档（*.mxd)|*.mxd";
            sfd.RestoreDirectory = true;
            sfd.FilterIndex = 1;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                pMapDocument.SaveAs(sfd.FileName);
                MessageBox.Show("另存成功");
            }
        }

        private void filebtn_load_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //ofd.InitialDirectory = "D:\\esri\\chart\\辽东湾海图\\辽东湾海图.mxd";
            ofd.InitialDirectory = initPath;
            //ofd.Filter = "*.*";
            ofd.RestoreDirectory = true;
            ofd.FilterIndex = 1;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string pathName = System.IO.Path.GetDirectoryName(ofd.FileName);
                string filename = System.IO.Path.GetFileName(ofd.FileName);
                //string filename = "haibing.shp";
                axMapControl1.AddShapeFile(pathName, filename + ".shp");
            }
        }

        private void sysbtn_restart_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DialogResult dr = MessageBox.Show("确定重启系统吗", "！", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Hand);
            if (dr == DialogResult.Yes)
            {
                this.Dispose(false);
                new Form1().Show();
            }
        }

        private void 查看属性表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                gridView1.Columns.Clear();
                ITable lyrtable = (ITable)layer;
                DataTable table = new DataTable();
                IField field;
                this.dock_list.Text = "\"" + layer.Name + "\" 属性表";
                this.dock_list.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
                for (int i = 0; i < lyrtable.Fields.FieldCount; i++)
                {
                    field = lyrtable.Fields.get_Field(i);
                    table.Columns.Add(field.Name);
                }
                object[] values = new object[lyrtable.Fields.FieldCount];
                IQueryFilter queryFilter = new QueryFilterClass();
                ICursor cursor = lyrtable.Search(queryFilter, true);
                IRow row;
                while ((row = cursor.NextRow()) != null)
                {
                    for (int j = 0; j < lyrtable.Fields.FieldCount; j++)
                    {
                        object ob = row.get_Value(j);
                        values[j] = ob;
                    } table.Rows.Add(values);
                }
                gridControl1.DataSource = table;
            }
            catch
            {
                MessageBox.Show("无法显示属性表！");
            }
        }

        private void 删除图层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("确定要删除图层吗，删除后将不能恢复！", "！", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Hand);
            if (dr == DialogResult.Yes)
            {
                for (int i = 0; i < this.axMapControl1.Map.LayerCount; i++)
                {
                    if (this.axMapControl1.Map.get_Layer(i) == layer)
                    {
                        this.axMapControl1.DeleteLayer(i);
                    }
                }
            }
        }

        private void 缩放到图层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ITable lyrtable = (ITable)layer;
                zoom(layer);
            }
            catch { MessageBox.Show("图层无数据，无法缩放"); }
        }

        //自定义遍历所有图层事件
        private IEnumLayer Get_layers()
        {
            UID uid = new UIDClass();
            uid.Value = "{40A9E885-5533-11d0-98BE-00805F7CED21}";
            IEnumLayer layers = axMapControl1.Map.get_Layers(uid, true);
            return layers;
        }
        //自定义缩放到图层事件
        private void zoom(ILayer layer)
        {
            IEnumLayer layers = Get_layers();
            ILayer pLayer;
            while ((pLayer = layers.Next()) != null)
            {
                if (pLayer.Name == layer.Name)
                {
                    IEnvelope envelope = layer.AreaOfInterest;
                    axMapControl1.ActiveView.Extent = envelope;

                    IFeatureLayer pFeatureLayer = layer as IFeatureLayer;
                    IFeatureCursor pFeatureCursor = pFeatureLayer.FeatureClass.Search(null, false);

                    IFeatureClass pFeatureClass = pFeatureCursor as IFeatureClass;
                    IFeature pFeature = pFeatureCursor.NextFeature();
                    while (pFeature != null)
                    {
                        IGeometry geometry = pFeature.Shape;
                        IEnvelope featureExtent = geometry.Envelope;
                        envelope.Union(featureExtent);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeature);
                        pFeature = pFeatureCursor.NextFeature();
                    }
                    axMapControl1.ActiveView.FullExtent = envelope;
                    axMapControl1.Refresh();
                }
            }
        }
        private string strTool;
        #region 基本操作（e）
        //自定义按钮弹起事件
        public void Check()
        {
            strTool = "";
            Bbtn_pan.Down = false;
            Bbtn_rotation.Down = false;
            Bbtn_full.Down = false;
            Bbtn_zoomin.Down = false;
            Bbtn_roomout.Down = false;
            Bbtn_callen.Down = false;
            Bbtn_ident.Down = false;
            Bbtn_ksel.Down = false;
            Bbtn_Ysel.Down = false;
            Bbtn_calmianj.Down = false;
            Bbtn_Msel.Down = false;
        }
        private void Bbtn_rotation_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            Check();
            ICommand Cmd = new ControlsMapClearMapRotationCommand();
            Cmd.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = Cmd as ITool;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;

        }

        private void Bbtn_pan_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Check();
            Bbtn_pan.Down = true;
            ICommand Cmd = new ControlsMapPanTool();
            Cmd.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = Cmd as ITool;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerPan;
        }

        private void Bbtn_zoomin_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Check();
            Bbtn_zoomin.Down = true;
            ICommand Cmd = new ControlsMapZoomInTool();
            Cmd.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = Cmd as ITool;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerZoomIn;
        }

        private void Bbtn_roomout_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Check();
            Bbtn_roomout.Down = true;
            ICommand Cmd = new ControlsMapZoomOutTool();
            Cmd.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = Cmd as ITool;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerZoomOut;
        }

        private void Bbtn_full_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Check();
            ICommand Cmd = new ControlsMapFullExtentCommand();
            Cmd.OnCreate(this.axMapControl1.Object);
            Cmd.OnClick();
        }

        private void Bbtn_forward_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Check();
            ICommand Cmd = new ControlsMapZoomToLastExtentBackCommand();
            Cmd.OnCreate(this.axMapControl1.Object);
            Cmd.OnClick();
        }

        private void Bbtn_back_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Check();
            ICommand Cmd = new ControlsMapZoomToLastExtentForwardCommand();
            Cmd.OnCreate(this.axMapControl1.Object);
            Cmd.OnClick();
        }

        private void Bbtn_refresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Check();
            //清除主图添加的图形要素
            axMapControl1.ActiveView.GraphicsContainer.DeleteAllElements();
            //清除选择要素
            IMap pMap = axMapControl1.Map;
            pMap.ClearSelection();
            axMapControl1.Refresh();
        }

        //框选
        private void Bbtn_ksel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Check();
            Bbtn_ksel.Down = true;
            ICommand Cmd = new ControlsSelectFeaturesTool();
            Cmd.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = Cmd as ITool;
        }
        //多边形选取
        private void Bbtn_Msel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Check();
            ICommand Cmd = new ControlsMapClearMapRotationCommand();
            Cmd.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = Cmd as ITool;
            Bbtn_Msel.Down = true;
            strTool = "多边形选择";
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
        }
        //yuanxuan
        private void Bbtn_Ysel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Check();
            ICommand Cmd = new ControlsMapClearMapRotationCommand();
            Cmd.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = Cmd as ITool;
            Bbtn_Ysel.Down = true;
            strTool = "圆选择";
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
        }
        //identify
        private void Bbtn_ident_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Check();
            Bbtn_ident.Down = true;
            ICommand Cmd = new ControlsMapIdentifyTool();
            Cmd.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = Cmd as ITool;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerIdentify;
        }
        // length
        private void Bbtn_callen_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Check();
            axMapControl1.ActiveView.GraphicsContainer.DeleteAllElements();
            ICommand Cmd = new ControlsMapClearMapRotationCommand();
            Cmd.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = Cmd as ITool;
            Bbtn_callen.Down = true;
            strTool = "长度量测";
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
        }
        //area
        private void Bbtn_calmianj_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Check();
            axMapControl1.ActiveView.GraphicsContainer.DeleteAllElements();
            ICommand Cmd = new ControlsMapClearMapRotationCommand();
            Cmd.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = Cmd as ITool;
            Bbtn_calmianj.Down = true;
            strTool = "面积量测";
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
        }
        //选择定位图层
        private void Bbtn_layerloc_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            /*
            Cbxe_selectpoint.Properties.Items.Clear();

            string StrLyrName = Cbxe_selectlayer.EditValue.ToString();
            IEnumLayer layers = Get_layers();

            while ((layer = layers.Next()) != null)
            {
                if (layer.Name == StrLyrName)
                {
                    IFeatureLayer pFeatureLayer = layer as IFeatureLayer;
                    IFeatureClass pFC = pFeatureLayer.FeatureClass;
                    IFeatureCursor pFeatureCursor = pFeatureLayer.FeatureClass.Search(null, false);
                    IFeature pFeature = pFeatureCursor.NextFeature();
                    try
                    {
                        if (pFeature.Fields != null)
                        {
                            IFields pFields = pFeature.Fields;

                            while (pFeature != null)
                            {
                                int IntName = 0;
                                IntName = pFeature.Fields.FindField("NAME");
                                string w = pFeature.get_Value(IntName).ToString();
                                if (w != "")
                                {
                                    Cbxe_selectpoint.Properties.Items.Add(w);
                                }
                                pFeature = pFeatureCursor.NextFeature();
                            }
                        }
                    }
                    catch { MessageBox.Show("所选图层为空！"); }
                }
            }
            Cbxe_selectpoint.Enabled = true;
             * */
        }

        private void Bbtn_XYloc_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (this.dock_positionbyxy.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
            { this.dock_positionbyxy.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden; }
            else
            {
                this.dock_positionbyxy.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
            }
        }

        private void Cbxe_beginposition2_Click(object sender, EventArgs e)
        {
            axMapControl1.ActiveView.GraphicsContainer.DeleteAllElements();
            string strscale = Cbxe_setScale.Text;
            string strsubscale = strscale.Substring(2, strscale.Length - 2);
            if (Tbxe_lat.Text != null && Tbxe_log.Text != null)
            {
                try
                {
                    double Dbllong = Convert.ToDouble(Tbxe_log.Text);
                    double Dbllat = Convert.ToDouble(Tbxe_lat.Text);
                    IPoint pPoint = new PointClass();
                    pPoint.PutCoords(Dbllong, Dbllat);
                    IMarkerElement pMarkerElement;
                    pMarkerElement = new MarkerElementClass();
                    ISimpleMarkerSymbol pMarkerSymbol;
                    pMarkerSymbol = new SimpleMarkerSymbolClass();
                    IRgbColor rgb = new RgbColorClass();
                    {
                        rgb.Red = 0;
                        rgb.Green = 100;
                        rgb.Blue = 249;
                    }
                    pMarkerSymbol.Color = rgb as IColor;
                    pMarkerSymbol.Size = 10;
                    pMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSDiamond;
                    IElement pElement;
                    pElement = pMarkerElement as IElement;
                    pElement.Geometry = pPoint;
                    pMarkerElement.Symbol = pMarkerSymbol;
                    IMap pMap = axMapControl1.Map;
                    IGraphicsContainer pGraphicsContainer = pMap as IGraphicsContainer;
                    pGraphicsContainer.AddElement(pElement, 0);
                    axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                    axMapControl1.MapScale = Convert.ToInt32(strsubscale);
                    axMapControl1.CenterAt(pPoint);
                    axMapControl1.Refresh();
                }
                catch { }
            }
        
        }
        #endregion
        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            string XYTxt = "经度:" + e.mapX.ToString("#0.0000") + ",纬度:" + e.mapY.ToString("#0.0000") + "    地图比例尺:1:" + axMapControl1.MapScale.ToString("#0");
            this.bSI_xy.Caption = XYTxt;
        }
    }
}
