/*******************************************************
Copyright All Rights Reserved. (C) HangZhou Hikvision System Technology Co., Ltd. 
File ：    FormUploadFaceLib.cs 
Developer：    Hikvision
Author：    chenzhixue@hikvision.com
Period：    2019-07-18
Describe：    FormUploadFaceLib.cs
********************************************************/

using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using TINYXMLTRANS;

namespace SDKFaceContrast
{
    public partial class FormUploadFaceLib : Form
    {
        public bool m_bPictureUpload = false;
        public CHCNetSDK.NET_DVR_SEND_PARAM_IN m_struSendParam = new CHCNetSDK.NET_DVR_SEND_PARAM_IN();
        public CHCNetSDK.NET_DVR_UPLOAD_FILE_RET m_struFileRet = new CHCNetSDK.NET_DVR_UPLOAD_FILE_RET();
        private int m_iNum = 0;
        public int m_lUserID = -1;
        public int m_iCurChanNo = -1;
        public int m_lUploadHandle = -1;

        public FormUploadFaceLib()
        {
            InitializeComponent();
            
            cbSendNum.SelectedIndex = 0;
            m_lUserID = FormFaceContrast.m_lUserID;
            m_iCurChanNo = FormFaceContrast.m_iCurChanNo;
            m_bPictureUpload = FormFaceContrast.m_bPictureUpload;
        }
             
        private void btnPicPath_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "C# Corner Open File Dialog";
            fdlg.Filter = "JPG|*.jpg";
            fdlg.RestoreDirectory = false;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                txPicPath.Text = System.IO.Path.GetFullPath(fdlg.FileName);
            }
        }

        private void readFileData()
        {
            if (File.Exists(txXMLPath.Text))
            {
                FileStream fs = File.OpenRead(txXMLPath.Text); // OpenRead
                int iFilelength = 0;
                iFilelength = (int)fs.Length;
                Byte[] byXml = new Byte[iFilelength];
                fs.Read(byXml, 0, iFilelength);
                m_struSendParam.pSendAppendData = Marshal.AllocHGlobal(iFilelength);
                Marshal.Copy(byXml, 0, m_struSendParam.pSendAppendData, iFilelength);
                m_struSendParam.dwSendAppendDataLen = (uint)iFilelength;
                fs.Close();
            }
            else
            {
                MessageBox.Show("Read" + txXMLPath.Text + "failed! ");
                return;
            }

            if (File.Exists(txPicPath.Text))
            {
                FileStream fs = File.OpenRead(txPicPath.Text); // OpenRead
                int iFilelength = 0;
                iFilelength = (int)fs.Length;
                Byte[] byImage = new Byte[iFilelength];
                fs.Read(byImage, 0, iFilelength);
                m_struSendParam.pSendData = Marshal.AllocHGlobal(iFilelength);
                Marshal.Copy(byImage, 0, m_struSendParam.pSendData, iFilelength);
                m_struSendParam.dwSendDataLen = (uint)iFilelength;
                fs.Close();
            }
            else
            {
                MessageBox.Show("Read" + txPicPath.Text + "failed ");
                return;
            }

            m_struSendParam.byPicType = 1;
            m_struSendParam.byPicURL = 0;
        }

        private void sendUploadData()
        {
            int iSize = Marshal.SizeOf(m_struSendParam);
            IntPtr pSendParamIN = Marshal.AllocHGlobal(iSize);
            Marshal.StructureToPtr(m_struSendParam, pSendParamIN, true);
            int lUploadSend = -1;

            try
            {
                lUploadSend = CHCNetSDK.NET_DVR_UploadSend(m_lUploadHandle, pSendParamIN, IntPtr.Zero);

                if (lUploadSend < 0)
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "errorCode: " + iLastErr;
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_UploadSend", "Failed" + strErr);
                    return;
                }
                else
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_UploadSend", "Sucessed");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pSendParamIN);
            }
        }

        public int getUploadState()
        {
            Int32 dwProgress = 0;
            int iStatus = -1;
            string strStatus = "";
            IntPtr pProgress = Marshal.AllocHGlobal(4);
            Marshal.WriteInt32(pProgress, dwProgress);

            try
            {
                iStatus = CHCNetSDK.NET_DVR_GetUploadState(m_lUploadHandle, pProgress);
                dwProgress = Marshal.ReadInt32(pProgress);

                if (-1 == iStatus)
                {
                    strStatus = "Upload Failed: status[" + iStatus + "]";
                    Thread.Sleep(100);
                }
                else if (1 == iStatus)
                {
                    strStatus = "Upload Success: status[" + iStatus + "]" + " progress[" + dwProgress + "]";
                    Thread.Sleep(100);
                }
                else if (2 == iStatus)
                {
                    strStatus = "Uploading: status[" + iStatus + "]" + " progress[" + dwProgress + "]";
                    Thread.Sleep(100);
                }
                else if (35 == iStatus)
                {
                    strStatus = "URL Download Not Start !";
                    Thread.Sleep(100);
                }
                else if (36 == iStatus)
                {
                    strStatus = "customHumanID Repeat !";
                    Thread.Sleep(100);
                }
                else if (38 == iStatus)
                {
                    strStatus = "Modeling failed. Device internal error !";
                    Thread.Sleep(100);
                }
                else if (39 == iStatus)
                {
                    strStatus = "Modeling failed. Face modeling error !";
                    Thread.Sleep(100);
                }
                else if (40 == iStatus)
                {
                    strStatus = "Modeling failed. Face grading error !";
                    Thread.Sleep(100);
                }
                else if (41 == iStatus)
                {
                    strStatus = "Modeling failed. Facial feature points extracting error !";
                    Thread.Sleep(100);
                }
                else if (42 == iStatus)
                {
                    strStatus = "Modeling failed. Feature analyzing error !";
                    Thread.Sleep(100);
                }
                else if (43 == iStatus)
                {
                    strStatus = "Picture data error !";
                    Thread.Sleep(100);
                }
                else if (44 == iStatus)
                {
                    strStatus = "Attached picture information error !";
                    Thread.Sleep(100);
                }
                else
                {
                    strStatus = "Upload Failed: status[" + iStatus + "]";
                    Thread.Sleep(100);
                }

                if (statusStripUpload.InvokeRequired)
                {
                    Action<string> actionDelegate = (x) => { this.toolStripStatusLabel2.Text = x.ToString(); };
                    this.statusStripUpload.Invoke(actionDelegate, strStatus);
                    Action<string> actionDelegateTime = (x) => { this.toolStripStatusLabel2.Text = x.ToString(); };
                    this.toolStripStatusLabel3.Text = DateTime.Now.ToString();
                }
                else
                {
                    this.toolStripStatusLabel2.Text = strStatus;
                    this.toolStripStatusLabel3.Text = DateTime.Now.ToString();
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pProgress);
            }

            return iStatus;
        }

        private void stopUploadData()
        {
            if (m_lUploadHandle != -1)
            {
                if (CHCNetSDK.NET_DVR_UploadClose(m_lUploadHandle))
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "errorCode: " + iLastErr;
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_UploadClose", "Failed" + strErr);
                }
                else
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_UploadClose", "Success");
                }
                m_lUploadHandle = -1;
            }

            if (btnStartUpload.InvokeRequired)
            {
                Action<bool> actionDelegate = (x) => { this.btnStartUpload.Enabled = x; };
                this.statusStripUpload.Invoke(actionDelegate, true);
            }
            else
            {
                this.btnStartUpload.Enabled = true;
            }
        }

        private void FaceLibUpLoadThread()
        {
            int dwOutBufferSize = 0;

            sendUploadData();

            int iStatus = -1;
            int iNum = m_iNum;

            while (true)
            {
                if (-1 == m_lUploadHandle)
                {
                    return;
                }

                iStatus = getUploadState();

                if (1 == iStatus)
                {
                    dwOutBufferSize = Marshal.SizeOf(m_struFileRet);
                    IntPtr pOutBuffer = Marshal.AllocHGlobal(dwOutBufferSize);
                    Marshal.StructureToPtr(m_struFileRet, pOutBuffer, true);

                    try
                    {
                        if (CHCNetSDK.NET_DVR_GetUploadResult(m_lUploadHandle, pOutBuffer, (uint)dwOutBufferSize))
                        {
                            MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GetUploadResult", "Sucessed");
                            m_struFileRet = (CHCNetSDK.NET_DVR_UPLOAD_FILE_RET)Marshal.PtrToStructure(pOutBuffer, typeof(CHCNetSDK.NET_DVR_UPLOAD_FILE_RET));
                            string strUrl = System.Text.Encoding.Default.GetString(m_struFileRet.sUrl);

                            if (statusStripUpload.InvokeRequired)
                            {
                                Action<string> actionDelegate = (x) => { this.txPID.Text = x.ToString(); };
                                this.statusStripUpload.Invoke(actionDelegate, strUrl);
                            }
                            else
                            {
                                this.txPID.Text = strUrl;
                            }

                            if (iNum > 0)
                            {
                                sendUploadData();
                                iNum--;
                            }
                        }
                        else
                        {
                            int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                            string strErr = "errorCode: " + iLastErr;
                            MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GetUploadResult", "Failed" + strErr);
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(pOutBuffer);
                    }
                }
                else if ((iStatus >= 3 && iStatus <= 10) || iStatus == 31 || iStatus == -1)
                {
                    stopUploadData();
                    break;
                }
            }

        }

        private void SDK_UploadFaceLib()
        {
            if ("" == txXMLPath.Text)
            {
                MessageBox.Show("Please Select XML ! ");
                return;
            }

            CHCNetSDK.NET_DVR_FACELIB_COND struFaceLibCond = new CHCNetSDK.NET_DVR_FACELIB_COND();
            int iSize = Marshal.SizeOf(struFaceLibCond);
            struFaceLibCond.dwSize = (uint)iSize;
            byte[] byFDID = System.Text.Encoding.Default.GetBytes(txFDID.Text);
            struFaceLibCond.szFDID = new byte[256];
            byFDID.CopyTo(struFaceLibCond.szFDID, 0);
            struFaceLibCond.byConcurrent = Convert.ToByte(chkConcurrent.Checked);
            struFaceLibCond.byCustomFaceLibID = 0;
            byte[] byIdentityKey = System.Text.Encoding.Default.GetBytes(txIdentityKey.Text);
            struFaceLibCond.byIdentityKey = new byte[64];
            byIdentityKey.CopyTo(struFaceLibCond.byIdentityKey, 0);

            IntPtr pFacelibCond = Marshal.AllocHGlobal(iSize);
            Marshal.StructureToPtr(struFaceLibCond, pFacelibCond, true);

            try
            {
                m_lUploadHandle = CHCNetSDK.NET_DVR_UploadFile_V40(m_lUserID, CHCNetSDK.IMPORT_DATA_TO_FACELIB,
                    pFacelibCond, (uint)iSize, null, IntPtr.Zero, 0);

                if (m_lUploadHandle < 0)
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "errorCode: " + iLastErr;
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "IMPORT_DATA_TO_FACELIB", "Failed" + strErr);
                    return;
                }
                else
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "IMPORT_DATA_TO_FACELIB", "Sucessed");
                }

                readFileData();

                Thread thFaceLibUpLoad = new Thread(new ThreadStart(FaceLibUpLoadThread));

                thFaceLibUpLoad.Start();

                while (!thFaceLibUpLoad.IsAlive)
                {
                    thFaceLibUpLoad.Join();
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pFacelibCond);
            }
        }

        private void initUI(bool bProtocol)
        {
            if(bProtocol)
            {
                cbSendNum.Enabled = false;
                txIdentityKey.Enabled = false;
                txXMLPath.Enabled = false;
                btnXmlPath.Enabled = false;
                btnStopUpload.Enabled = false;
                btnStartUpload.Text = "Upload";
            }
            else
            {
                gbFaceAppendData.Enabled = false;
            }
        }

        private void btnStartUpload_Click(object sender, EventArgs e)
        {
            if ("" == txPicPath.Text)
            {
                MessageBox.Show("Please Select FacePicture ! ");
                return;
            }

            SDK_UploadFaceLib();
            btnStartUpload.Enabled = false;
        }

        private void btnStopUpload_Click(object sender, EventArgs e)
        {
            stopUploadData();
            btnStartUpload.Enabled = true;
        }

        private void btnXmlPath_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "C# Corner Open File Dialog";
            fdlg.Filter = "All files（*.*）|*.*|All files(*.*)|*.* ";
            fdlg.RestoreDirectory = false;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                txXMLPath.Text = System.IO.Path.GetFullPath(fdlg.FileName);
            }
        }

        private void FormUploadFaceLib_Load(object sender, EventArgs e)
        {
            if (null != PluginsFactory.GetDeviceTreeInstance())
            {
                initUI(false);
            }
        }

    }
}
