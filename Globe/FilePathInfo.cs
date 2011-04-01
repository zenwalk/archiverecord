using System;
using System.Collections.Generic;
using System.Text;

namespace ArchiveRecord.Globe
{
    public class FilePathInfo
    {
            /// <summary>
            /// Var
            /// </summary>
            private string _FileName = "";
            public string FileName
            {
                get
                {
                    return _FileName;
                }
                set
                {
                    _FileName = value;
                }
            }

            private string _FileFolder = "";
            public string FileFolder
            {
                get
                {
                    return _FileFolder;
                }
                set
                {
                    _FileFolder = value;
                }
            }

            private string _FileSuffix = "";
            public string FileSuffix
            {
                get
                {
                    return _FileSuffix;
                }
                set
                {
                    _FileSuffix = value;
                }
            }

            private string _FilePath = "";
            public string FilePath
            {
                get
                {
                    return _FilePath;
                }
                set
                {
                    _FilePath = value;
                }
            }
            /// <summary>
            /// EnDiningRoomName
            /// </summary>
            /// <param name="_DiningRoomName"></param>
            public FilePathInfo(string FileName)
            {
                this._FilePath = FileName;
                this._FileName = FileName.Substring(FileName.LastIndexOf("\\") + 1, (FileName.LastIndexOf(".") - FileName.LastIndexOf("\\") - 1));
                this._FileFolder = FileName.Substring(0, FileName.LastIndexOf("\\"));
                this._FileSuffix = FileName.Substring(FileName.LastIndexOf(".") + 1, (FileName.Length - FileName.LastIndexOf(".") - 1));
            }
    }
}
