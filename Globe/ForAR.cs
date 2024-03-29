﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Drawing;
using System.Data;
using System.Runtime.InteropServices;
using System.IO;

namespace ArchiveRecord.Globe
{
    class ForAR
    {
        #region "常量定义"
        public const int Pan_Layer = 1; //图层可停靠面板
        public const int Pan_Archive = 10; //站点可停靠面板
        public const int Pan_Query = 11; //站线可停靠面板


        public const int Map3D_PolySelect = 1071;//多边形选择
        public const int Map3D_PointSelect = 1072;//点选
        public const int Map3D_CircleSelect = 1070;//圆形选择
        public const int Record_Input = 1073;//工程入库
        public const int BusInfo_ParaSet = 1068;//参数设置

        public const int Map3D_Select = 1065; //拉框选择
        public const int Map3D_ZoomIn = 10211; //放大
        public const int Map3D_ZoomOut = 10212; //缩小
        public const int Map3D_Full = 10213; //全图
        public const int Map3D_Pan = 10214; //漫游
        public const int Map3D_Vector = 10215; //加载矢量
        public const int Map3D_OpenMode = 10216; //
        public const int Map3D_PreView = 10217; //上一屏
        public const int Map3D_NextView = 10218; //下一屏
        public const int Map3D_Reflash = 10219; //刷新
        public const int Map3D_Distance = 10222; //距离
        public const int Map3D_Area = 10224; //面积

        public static string Login_name = "admin"; //登录名
        public static string Login_Operation = "";//允许操作
        public static frmMainNew m_FrmMain; //主窗体类
        public enum GridSetType { Archive_FillPan = 1, Archive_Fill};
        public static string Connect_Sql = "";//链接数据库字符串
        public static string Mxd_Name = "";//加载mxd
        public static int Connect_Type = 1;//链接数据库类型1sde,2本地

        #endregion

        public static void SetRowColor_Alternation(DataGridViewRowCollection RowCollection)
        {
            DataGridViewCellStyle RowColor = new DataGridViewCellStyle();
            RowColor.BackColor = Color.Aqua;
            int nRowCount = 0;
            foreach (DataGridViewRow eRow in RowCollection)
            {
                if (0 == nRowCount)
                {
                    eRow.DefaultCellStyle = RowColor;
                    nRowCount = 1;
                }
                else
                {
                    nRowCount = 0;
                }
            }
        }

        public static void Add_Log(string strName, string strOperation, string strField, string Description)
        {
            OleDbConnection mycon = new OleDbConnection(Connect_Sql);
            mycon.Open();
            try
            {
                OleDbCommand pCom;
                if (Connect_Type == 1)
                    pCom = new OleDbCommand(String.Format("insert into sde.OperationLog(Name,LogTime,Field,Operation,LogScribe) values('{0}','{1}','{2}','{3}','{4}')"
                          , strName, DateTime.Now.ToString(), strField, strOperation, Description), mycon);
                else
                    pCom = new OleDbCommand(String.Format("insert into OperationLog(Name,LogTime,Field,Operation,LogScribe) values('{0}','{1}','{2}','{3}','{4}')"
                      , strName, DateTime.Now.ToString(), strField, strOperation, Description), mycon);
                pCom.ExecuteNonQuery();
                mycon.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("添加日志文件出错\n" + ex.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static OleDbDataAdapter CreateCustomerAdapter(OleDbConnection connection, string strSELECT, string strINSERT, string strDele)
        {
            OleDbDataAdapter adapter = new OleDbDataAdapter();
            OleDbCommand command;

            // Create the SelectCommand.
            command = new OleDbCommand(strSELECT, connection);
            adapter.SelectCommand = command;

            // Create the InsertCommand.
            command = new OleDbCommand(strINSERT, connection);
            adapter.InsertCommand = command;

            command = new OleDbCommand(strDele, connection);
            adapter.DeleteCommand = command;

            return adapter;
        }

        public static void ArchiveFill(DataGridView grid, GridSetType emunType, string strQuery,string[] strShow)
        {
            OleDbConnection mycon = new OleDbConnection(Connect_Sql);
            mycon.Open();
             string strStationSQL, strRoadSQL;
             if (Connect_Type == 1)
             {
                 strStationSQL = @"SELECT  * FROM sde.KCGC";
             }
             else
             {
                 strStationSQL = @"SELECT  * FROM Project";
             }
            try
            {
                switch (emunType)
                {
                    case GridSetType.Archive_FillPan:
                        OleDbDataAdapter da = ForAR.CreateCustomerAdapter(mycon, strStationSQL + strQuery , "", "");
                        da.SelectCommand.ExecuteNonQuery();
                        DataSet ds = new DataSet();
                        int nQueryCount = da.Fill(ds, "Archive");
                        grid.DataSource = ds;
                        grid.DataMember = "Archive";
                        foreach (DataGridViewColumn eCol in grid.Columns)
                        {
                            eCol.Visible = false;
                            eCol.ReadOnly = true;
                            eCol.Resizable = DataGridViewTriState.False;
                            eCol.SortMode = DataGridViewColumnSortMode.NotSortable;
                        }
                        if (strShow[0] == "")
                        {
                            grid.Columns["任务号"].Visible = true;
                        }
                        else
                        {
                            grid.Columns[strShow[0]].Visible = true;
                        }
                        SetRowNo(grid);
                        break;
                    case GridSetType.Archive_Fill:
                        da = ForAR.CreateCustomerAdapter(mycon, strStationSQL + strQuery , "", "");
                        da.SelectCommand.ExecuteNonQuery();
                        ds = new DataSet();
                        nQueryCount = da.Fill(ds, "Archive");

                        grid.DataSource = ds;
                        grid.DataMember = "Archive";
                        foreach (DataGridViewColumn eCol in grid.Columns)
                        {
                            eCol.ReadOnly = true;
                            eCol.Resizable = DataGridViewTriState.False;
                            if (eCol.Name != "ID" && !ForAR.IsChineseLetter(eCol.Name, 0))
                            {
                                eCol.Visible = false;
                            }
                            eCol.SortMode = DataGridViewColumnSortMode.NotSortable;
                        }
                        SetRowNo(grid);
                        break;
                    default :
                        break;
                }
                mycon.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("查询数据出错\n" + ex.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        //控制列排序模式
        public static void SetColSortMode(DataGridView grid, DataGridViewColumnSortMode ColSortMode)
        {
            foreach (DataGridViewColumn eColumn in grid.Columns)
            {

                eColumn.SortMode = ColSortMode;
            }
        }
        //在rowheard添加序号
        public static void SetRowNo(DataGridView grid)
        {
            int nNum = 1;
            foreach (DataGridViewRow eRow in grid.Rows)
            {
                eRow.HeaderCell.Value = nNum++.ToString();
            }
        }

        public static bool IsChineseLetter(string input, int index)
        {
            int code = 0;
            int chfrom = Convert.ToInt32("4e00", 16);    //范围（0x4e00～0x9fff）转换成int（chfrom～chend）
            int chend = Convert.ToInt32("9fff", 16);
            if (input != "")
            {
                code = Char.ConvertToUtf32(input, index);    //获得字符串input中指定索引index处字符unicode编码

                if (code >= chfrom && code <= chend)
                {
                    return true;     //当code在中文范围内返回true

                }
                else
                {
                    return false;    //当code不在中文范围内返回false
                }
            }
            return false;
        }

        //采用递归的方式遍历，文件夹和子文件中的所有文件。
        public static void FindFile(string dirPath, ref List<FilePathInfo> FilePath,int nKillPathLen) //参数dirPath为指定的目录
        {
            //在指定目录及子目录下查找文件,在listBox1中列出子目录及文件
            DirectoryInfo Dir = new DirectoryInfo(dirPath);
            try
            {
                foreach (DirectoryInfo d in Dir.GetDirectories())//查找子目录 
                {
                    FindFile(Dir + d.ToString() + "\\", ref FilePath, nKillPathLen);
                    //listBox1.Items.Add(Dir+d.ToString()+"\"); //listBox1中填加目录名
                }
                foreach (FileInfo f in Dir.GetFiles("*.*")) //查找文件“*.---”指要访问的文件的类型的扩展名
                {
                    FilePath.Add(new FilePathInfo(Dir + f.ToString(),nKillPathLen));
                    //listBox1.Items.Add(Dir+f.ToString()); //listBox1中填加文件名
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        //声明读写INI文件的API函数
        [DllImport("kernel32")]
        public static extern bool WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, byte[] retVal, int size, string filePath);

        public static string GetProfileString(string section, string key, string filePath)
        {
            Byte[] Buffer = new Byte[65535];
            int bufLen = GetPrivateProfileString(section, key, "", Buffer, Buffer.GetUpperBound(0), filePath);
            //必须设定0（系统默认的代码页）的编码方式，否则无法支持中文
            string s = Encoding.GetEncoding(0).GetString(Buffer);
            s = s.Substring(0, bufLen);
            return s.Trim();
        }

        public static void AppIni()
        {
            string strType = "..\\..";
            if (Connect_Type == 1)
            {
                Connect_Sql = "Provider=sqloledb;Data Source = 172.16.34.120;Initial Catalog=sde;User Id = sa;Password = sa";
                Mxd_Name = strType + "\\data\\Sde.mxd";

            }
            else
            {
                Connect_Sql = "provider=Microsoft.Jet.OLEDB.4.0;data source=" + strType + "\\data\\Data.mdb";
                Mxd_Name = strType + "\\data\\Data.mxd";
            }
        }

    }
}
