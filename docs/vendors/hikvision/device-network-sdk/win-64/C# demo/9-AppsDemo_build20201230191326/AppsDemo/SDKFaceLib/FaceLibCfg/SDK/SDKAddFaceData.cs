using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Collections;
using System.Drawing;
using Common;
using System.Runtime.InteropServices;

namespace SDKFaceLib
{
    public partial class AddFaceDada
    {
        public static int m_lUploadHandle = -1;
        public static CHCNetSDK.NET_DVR_SEND_PARAM_IN m_struSendParam = new CHCNetSDK.NET_DVR_SEND_PARAM_IN();
        public static CHCNetSDK.NET_DVR_UPLOAD_FILE_RET m_struFileRet = new CHCNetSDK.NET_DVR_UPLOAD_FILE_RET();
        public class CErrorInfo
        {
            public int errorCode { get; set; }
            public string errorMsg { get; set; }
        }
        public class jsonPicUrl
        {
            public int errorCode { get; set; }
            public string errorMsg { get; set; }
            public string URL { get; set; }

        }
        jsonPicUrl m_jsonPicUrl;
        public class xmlFaceData
        {
            public string faceURL { get; set; }
            public string identityKey { get; set; }
            public string FDID { get; set; }
            public string name { get; set; }
            public string gender { get; set; }
            public string bornTime { get; set; }
            public string city { get; set; }
            public string certificateType  { get; set; }
            public string certificateNumber { get; set; }
            public string caseInfo { get; set; }
            public string tag { get; set; }
            public Byte ifConcurrent{ get; set; }
            public Byte ifByCover { get; set; }
        }
        xmlFaceData m_struFaceData=new xmlFaceData();

        public class jsonPicID
        {
            public int errorCode { get; set; }
            public string errorMsg { get; set; }
            public string FPID { get; set; }

        }
        jsonPicID m_jsonPicID;

        private void UploadPic()
        {   
            //bool ifConcurrent=false;
            IDeviceTree.DeviceInfo struDeviceInfo = g_deviceTree.GetSelectedDeviceInfo();      
            string strbornTime = "<bornTime>" + m_struFaceData.bornTime + "</bornTime>\r\n";
            string strName = "<name>" + m_struFaceData.name+ "</name>\r\n";
            string strSex = "<sex>" + m_struFaceData.gender + "</sex>\r\n";
            string strCity = "<city>" + m_struFaceData.city + "</city>\r\n";
            string strcertificateType = "<certificateType>" + m_struFaceData.certificateType + "</certificateType>\r\n";
            string strcertificateNumber = "<certificateNumber>" + m_struFaceData.certificateNumber + "</certificateNumber>\r\n";
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<FaceAppendData version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n");
            strBuilder.Append(strbornTime);
            strBuilder.Append(strName);
            strBuilder.Append(strSex);
            strBuilder.Append(strCity);
            strBuilder.Append(strcertificateType);
            strBuilder.Append(strcertificateNumber);
            strBuilder.Append("</FaceAppendData>\r\n");
            string strInput = strBuilder.ToString();
            IntPtr ptrAppendData = Marshal.StringToCoTaskMemAnsi(strInput);
            Byte[] byAppendData = System.Text.Encoding.UTF8.GetBytes(strInput);
            int datasize = byAppendData.Length;

            CHCNetSDK.NET_DVR_FACELIB_COND struFaceLibCond = new CHCNetSDK.NET_DVR_FACELIB_COND();
            int iSize = Marshal.SizeOf(struFaceLibCond);
            struFaceLibCond.dwSize = (uint)iSize;
            byte[] byFDID = System.Text.Encoding.Default.GetBytes(m_strFDID);
            struFaceLibCond.szFDID = new byte[256];
            byFDID.CopyTo(struFaceLibCond.szFDID, 0);
            struFaceLibCond.byConcurrent = m_struFaceData.ifConcurrent;
            struFaceLibCond.byConcurrent = m_struFaceData.ifByCover;
            struFaceLibCond.byCustomFaceLibID = 0;
            byte[] byIdentityKey = System.Text.Encoding.Default.GetBytes(m_struFaceData.identityKey);
            struFaceLibCond.byIdentityKey = new byte[64];
            byIdentityKey.CopyTo(struFaceLibCond.byIdentityKey, 0);

            IntPtr pFacelibCond = Marshal.AllocHGlobal(iSize);
            Marshal.StructureToPtr(struFaceLibCond, pFacelibCond, true);

            try
            {
                m_lUploadHandle = CHCNetSDK.NET_DVR_UploadFile_V40((int)struDeviceInfo.lLoginID, CHCNetSDK.IMPORT_DATA_TO_FACELIB,
                    pFacelibCond, (uint)iSize, null, IntPtr.Zero, 0);

                if (m_lUploadHandle < 0)
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "errorCode: " + iLastErr;
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "IMPORT_DATA_TO_FACELIB", "Failed" + strErr);
                }
                else
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "IMPORT_DATA_TO_FACELIB", "Sucessed");
                }
                try
                {
                    FileStream fs = File.OpenRead(m_strFileName); // OpenRead
                    int iFilelength = 0;
                    iFilelength = (int)fs.Length;
                    Byte[] byImage = new Byte[iFilelength];
                    fs.Read(byImage, 0, iFilelength);
                    m_struSendParam.pSendData = Marshal.AllocHGlobal(iFilelength);
                    Marshal.Copy(byImage, 0, m_struSendParam.pSendData, iFilelength);
                    m_struSendParam.dwSendDataLen = (uint)iFilelength;
                    fs.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                m_struSendParam.byPicType = 1;
                m_struSendParam.byPicURL = 0;
                m_struSendParam.pSendAppendData = ptrAppendData;
                m_struSendParam.dwSendAppendDataLen = (uint)datasize;
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
            pictureBox1.Image = Image.FromFile(m_strFileName);
            return;
        }


        private void FaceLibUpLoadThread()
        {
            int dwOutBufferSize = 0;
            sendUploadData();
            int iStatus = -1;
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

                            if (this.m_textBoxUrl.InvokeRequired)
                            {
                                Action<string> actionDelegate = (x) => { this.m_textBoxUrl.Text = x.ToString(); };
                                this.m_textBoxUrl.Invoke(actionDelegate, strUrl);
                            }
                            else
                            {
                                this.m_textBoxUrl.Text = strUrl;
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
                        stopUploadData();
                    }
                    break;
                }
                else if ((iStatus >= 3 && iStatus <= 10) || iStatus == 31 || iStatus == -1)
                {
                    stopUploadData();
                    MessageBox.Show(Convert.ToString(iStatus));
                    break;
                }
            }

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

        }
        
    }
}
