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

namespace ArchiveRecord
{
    public partial class frmArchivePane : UserControl
    {
        public List<FilePathInfo> m_FilePath = new List<FilePathInfo>();
        public frmArchivePane()
        {
            InitializeComponent();
        }

        public void ReflashGrid()
        {
            dataGridView1.DataSource = m_FilePath;
            dataGridView1.Columns["FileFolder"].Visible = false;
            dataGridView1.Columns["FilePath"].Visible = false;
            dataGridView1.Columns["FileName"].HeaderText = "文件名";
            dataGridView1.Columns["FileName"].ReadOnly = true;
            dataGridView1.Columns["FileSuffix"].HeaderText = "类型";
            dataGridView1.Columns["FileSuffix"].ReadOnly = true;
            dataGridView1.Columns[0].ReadOnly = false;
        }

        private void frmArchivePane_Load(object sender, EventArgs e)
        {
            
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
            string strFileName = "C:\\Drawing1.dwg";
            AcadApplication acadApp = new AcadApplicationClass();
            acadApp.Documents.Open(strFileName);
          
            DocumentCollection acDocMgr = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            //if (File.Exists(strFileName))
            //{
            //    acDocMgr.Open(strFileName, false);
            //}
            //else
            //{
            //    acDocMgr.MdiActiveDocument.Editor.WriteMessage("File " + strFileName +
            //                                                   " does not exist.");
            //}
            //Database db = acDocMgr.MdiActiveDocument.Database;
            //Editor ed = acDocMgr.MdiActiveDocument.Editor;
            //Entity entity = null;
            //DBObjectCollection EntityCollection = new DBObjectCollection();
            //PromptSelectionResult ents = ed.SelectAll();
            //if (ents.Status == PromptStatus.OK)
            //{
            //    using (Transaction transaction = db.TransactionManager.StartTransaction())
            //    {
            //        SelectionSet ss = ents.Value;
            //        foreach (ObjectId id in ss.GetObjectIds())
            //        {
            //            entity = transaction.GetObject(id, OpenMode.ForWrite, true) as Entity;
            //            if (entity != null)
            //                EntityCollection.Add(entity);
            //        }
            //        transaction.Commit();
            //    }
            //}
            
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
