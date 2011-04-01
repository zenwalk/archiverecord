namespace ArchiveRecord
{
    partial class frmlayerToc
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmlayerToc));
            this.AxLicenseControl1 = new ESRI.ArcGIS.Controls.AxLicenseControl();
            this.MapHawkEye = new ESRI.ArcGIS.Controls.AxMapControl();
            this.TOCControl = new ESRI.ArcGIS.Controls.AxTOCControl();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.属性管理ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.AxLicenseControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MapHawkEye)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TOCControl)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // AxLicenseControl1
            // 
            this.AxLicenseControl1.Enabled = true;
            this.AxLicenseControl1.Location = new System.Drawing.Point(8, 7);
            this.AxLicenseControl1.Name = "AxLicenseControl1";
            this.AxLicenseControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("AxLicenseControl1.OcxState")));
            this.AxLicenseControl1.Size = new System.Drawing.Size(32, 32);
            this.AxLicenseControl1.TabIndex = 5;
            // 
            // MapHawkEye
            // 
            this.MapHawkEye.Location = new System.Drawing.Point(24, 311);
            this.MapHawkEye.Name = "MapHawkEye";
            this.MapHawkEye.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("MapHawkEye.OcxState")));
            this.MapHawkEye.Size = new System.Drawing.Size(150, 150);
            this.MapHawkEye.TabIndex = 4;
            this.MapHawkEye.OnMouseDown += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnMouseDownEventHandler(this.MapHawkEye_OnMouseDown);
            // 
            // TOCControl
            // 
            this.TOCControl.Location = new System.Drawing.Point(24, 31);
            this.TOCControl.Name = "TOCControl";
            this.TOCControl.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("TOCControl.OcxState")));
            this.TOCControl.Size = new System.Drawing.Size(152, 264);
            this.TOCControl.TabIndex = 3;
            this.TOCControl.OnEndLabelEdit += new ESRI.ArcGIS.Controls.ITOCControlEvents_Ax_OnEndLabelEditEventHandler(this.TOCControl_OnEndLabelEdit);
            this.TOCControl.OnMouseDown += new ESRI.ArcGIS.Controls.ITOCControlEvents_Ax_OnMouseDownEventHandler(this.TOCControl_OnMouseDown);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.属性管理ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(119, 26);
            // 
            // 属性管理ToolStripMenuItem
            // 
            this.属性管理ToolStripMenuItem.Name = "属性管理ToolStripMenuItem";
            this.属性管理ToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.属性管理ToolStripMenuItem.Text = "属性管理";
            this.属性管理ToolStripMenuItem.Click += new System.EventHandler(this.属性管理ToolStripMenuItem_Click);
            // 
            // frmlayerToc
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.AxLicenseControl1);
            this.Controls.Add(this.MapHawkEye);
            this.Controls.Add(this.TOCControl);
            this.Name = "frmlayerToc";
            this.Size = new System.Drawing.Size(184, 468);
            this.Load += new System.EventHandler(this.frmlayerToc_Load);
            this.Resize += new System.EventHandler(this.frmlayerToc_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.AxLicenseControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MapHawkEye)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TOCControl)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal ESRI.ArcGIS.Controls.AxLicenseControl AxLicenseControl1;
        internal ESRI.ArcGIS.Controls.AxMapControl MapHawkEye;
        internal ESRI.ArcGIS.Controls.AxTOCControl TOCControl;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 属性管理ToolStripMenuItem;
    }
}
