﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Windows.Forms;

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


    public class FtpState
    {
        private ManualResetEvent wait;
        private FtpWebRequest request;
        private string fileName;
        private Exception operationException = null;
        string status;

        public FtpState()
        {
            wait = new ManualResetEvent(false);
        }

        public ManualResetEvent OperationComplete
        {
            get {return wait;}
        }

        public FtpWebRequest Request
        {
            get {return request;}
            set {request = value;}
        }

        public string FileName
        {
            get {return fileName;}
            set {fileName = value;}
        }
        public Exception OperationException
        {
            get {return operationException;}
            set {operationException = value;}
        }
        public string StatusDescription
        {
            get {return status;}
            set {status = value;}
        }
    }
    public class FtpUpLoader
    {  
        // Command line arguments are two strings:
        // 1. The url that is the name of the file being uploaded to the server.
        // 2. The name of the file on the local machine.
        //
        public static void UpLoader(string[] args)
        {
            // Create a Uri instance with the specified URI string.
            // If the URI is not correctly formed, the Uri constructor
            // will throw an exception.
            ManualResetEvent waitObject;

            Uri target = new Uri (args[0]);
            string fileName = args[1];
            FtpState state = new FtpState();
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(target);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example uses anonymous logon.
            // The request is anonymous by default; the credential does not have to be specified. 
            // The example specifies the credential only to
            // control how actions are logged on the server.

            request.Credentials = new NetworkCredential ("anonymous","janeDoe@contoso.com");

            // Store the request in the object that we pass into the
            // asynchronous operations.
            state.Request = request;
            state.FileName = fileName;

            // Get the event to wait on.
            waitObject = state.OperationComplete;

            // Asynchronously get the stream for the file contents.
            request.BeginGetRequestStream(
                new AsyncCallback (EndGetStreamCallback), 
                state
            );

            // Block the current thread until all operations are complete.
            waitObject.WaitOne();

            // The operations either completed or threw an exception.
            if (state.OperationException != null)
            {
                throw state.OperationException;
            }
            else
            {
                MessageBox.Show("The operation completed : " + state.StatusDescription, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static void EndGetStreamCallback(IAsyncResult ar)
        {
            FtpState state = (FtpState) ar.AsyncState;

            Stream requestStream = null;
            // End the asynchronous call to get the request stream.
            try
            {
                requestStream = state.Request.EndGetRequestStream(ar);
                // Copy the file contents to the request stream.
                const int bufferLength = 2048;
                byte[] buffer = new byte[bufferLength];
                int count = 0;
                int readBytes = 0;
                FileStream stream = File.OpenRead(state.FileName);
                do
                {
                    readBytes = stream.Read(buffer, 0, bufferLength);
                    requestStream.Write(buffer, 0, readBytes);
                    count += readBytes;
                }
                while (readBytes != 0);
                MessageBox.Show("Writing  bytes to the stream: " + count, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // IMPORTANT: Close the request stream before sending the request.
                requestStream.Close();
                // Asynchronously get the response to the upload request.
                state.Request.BeginGetResponse(
                    new AsyncCallback (EndGetResponseCallback), 
                    state
                );
            } 
            // Return exceptions to the main application thread.
            catch (Exception e)
            {
                MessageBox.Show("Could not get the request stream ", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                state.OperationException = e;
                state.OperationComplete.Set();
                return;
            }

        }

        // The EndGetResponseCallback method  
        // completes a call to BeginGetResponse.
        private static void EndGetResponseCallback(IAsyncResult ar)
        {
            FtpState state = (FtpState) ar.AsyncState;
            FtpWebResponse response = null;
            try 
            {
                response = (FtpWebResponse) state.Request.EndGetResponse(ar);
                response.Close();
                state.StatusDescription = response.StatusDescription;
                // Signal the main application thread that 
                // the operation is complete.
                state.OperationComplete.Set();
            }
            // Return exceptions to the main application thread.
            catch (Exception e)
            {
                Console.WriteLine ("Error getting response.");
                state.OperationException = e;
                state.OperationComplete.Set();
            }
        }
    }

}
