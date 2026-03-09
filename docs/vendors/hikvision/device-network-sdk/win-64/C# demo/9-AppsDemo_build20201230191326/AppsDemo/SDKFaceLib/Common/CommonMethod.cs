using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SDKFaceLib
{
    public class CommonMethod
    {
        public static CHCNetSDK.NET_DVR_SEND_PARAM_IN m_struSendParam = new CHCNetSDK.NET_DVR_SEND_PARAM_IN();
        public static CHCNetSDK.NET_DVR_UPLOAD_FILE_RET m_struFileRet = new CHCNetSDK.NET_DVR_UPLOAD_FILE_RET();
        public static int m_lUploadHandle = -1;
        public delegate bool GetURLDelegate(out string strURL);
        /// <summary>
        /// Upload cloud storage storage type enumeration
        /// </summary>
        public enum StorageTypeEnum
        {
            DYNAMIC_TYPE,    //Dynamic pool (overwrite storage space for snap images)
            STATIC_TYPE      //Static pool (for the face library image storage space is not overwritten)
        }
        private class UploadStorageInfo
        {
            public string FDID { get; set; }
            public string storageType { get; set; }
        }
        private class UploadStorageRet
        {
            public int errorCode { get; set; }
            public string errorMsg { get; set; }
            public string URL { get; set; }
        }
        /// <summary>
        /// Picture upload to cloud storage
        /// </summary>
        /// <param name="picturePath">Image file path</param>
        /// <param name="FDID">Picture library ID</param>
        /// <param name="storageType">Storage type</param>
        /// <returns></returns>
        
        //xml字符串拼接
        public static string xmlString(string strroot,List<string> strList)
        {
            string strret = string.Empty;
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
            string root = "<" + strroot + " version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n";
            strBuilder.Append(root);
            for (int i = 0; i < strList.Count; i++)
            {
                strBuilder.Append(strList[i]);
            }
            strBuilder.Append("</" + strroot + ">\r\n");
            strret = strBuilder.ToString();
            return strret;
        }


        //设备交互
        public static bool DoRequest(IDeviceTree.DeviceInfo deviceInfo,string strMethod, string strUri, string strInput, out string strOutput)
        {
            strOutput = string.Empty;

            //IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            if (deviceInfo == null)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKDebugTool", "have not selected a device");
                return false;
            }
            //通过透传接口发送请求
            //组装输入
            CHCNetSDK.NET_DVR_XML_CONFIG_INPUT struInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
            struInput.dwSize = (uint)Marshal.SizeOf(struInput);
            string strRequestUrl = strMethod + " " + strUri;
            IntPtr ptrUrl = Marshal.StringToCoTaskMemAnsi(strRequestUrl);
            struInput.lpRequestUrl = ptrUrl;
            struInput.dwRequestUrlLen = (uint)strRequestUrl.Length;
            struInput.dwRecvTimeOut = 3000;
            if (strMethod == "PUT" || strMethod == "POST")
            {
                struInput.lpInBuffer = Marshal.StringToCoTaskMemAnsi(strInput);
                struInput.dwInBufferSize = (uint)strInput.Length;
            }
            else
            {
                struInput.lpInBuffer = IntPtr.Zero;
                struInput.dwInBufferSize = 0;
            }
            IntPtr ptrInput = Marshal.AllocHGlobal(Marshal.SizeOf(struInput));
            Marshal.StructureToPtr(struInput, ptrInput, false);

            //组装输出
            CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT struOutput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();
            struOutput.dwSize = (uint)Marshal.SizeOf(struOutput);
            const int ciOutSize = 1024 * 1024; //预留1M接收数据
            IntPtr ptrOut = Marshal.AllocHGlobal(ciOutSize);
            struOutput.lpOutBuffer = ptrOut;
            struOutput.dwOutBufferSize = ciOutSize;
            struOutput.lpStatusBuffer = ptrOut;
            struOutput.dwStatusSize = ciOutSize;
            IntPtr ptrOutput = Marshal.AllocHGlobal(Marshal.SizeOf(struOutput));
            Marshal.StructureToPtr(struOutput, ptrOutput, false);
            bool bRet = CHCNetSDK.NET_DVR_STDXMLConfig((int)deviceInfo.lLoginID, ptrInput, ptrOutput);
            if (!bRet)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKDebugTool", "NET_DVR_STDXMLConfig failed[" + CHCNetSDK.NET_DVR_GetLastError().ToString() + "]");
                Marshal.FreeHGlobal(ptrInput);
                Marshal.FreeHGlobal(ptrOut);
                Marshal.FreeHGlobal(ptrOutput);
                return false;
            }
            MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "SDKDebugTool", "NET_DVR_STDXMLConfig succeed");
            strOutput = Marshal.PtrToStringAnsi(ptrOut);
            Marshal.FreeHGlobal(ptrInput);
            Marshal.FreeHGlobal(ptrOut);
            Marshal.FreeHGlobal(ptrOutput);

            return true;
        }
        public static bool DoImageRequest(IDeviceTree.DeviceInfo deviceInfo, string strMethod, string strUri, IntPtr ptrInputImage, int iImageLen, out string strOutput)
        {
            strOutput = string.Empty;

            //IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            if (deviceInfo == null)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKDebugTool", "have not selected a device");
                return false;
            }
            //通过透传接口发送请求
            //组装输入
            CHCNetSDK.NET_DVR_XML_CONFIG_INPUT struInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
            struInput.dwSize = (uint)Marshal.SizeOf(struInput);
            string strRequestUrl = strMethod + " " + strUri;
            IntPtr ptrUrl = Marshal.StringToCoTaskMemAnsi(strRequestUrl);
            struInput.lpRequestUrl = ptrUrl;
            struInput.dwRequestUrlLen = (uint)strRequestUrl.Length;
            struInput.dwRecvTimeOut = 3000;
            if (strMethod == "PUT" || strMethod == "POST")
            {
                struInput.lpInBuffer = ptrInputImage;
                struInput.dwInBufferSize = (uint)iImageLen;
            }
            else
            {
                struInput.lpInBuffer = IntPtr.Zero;
                struInput.dwInBufferSize = 0;
            }
            IntPtr ptrInput = Marshal.AllocHGlobal(Marshal.SizeOf(struInput));
            Marshal.StructureToPtr(struInput, ptrInput, false);

            //组装输出
            CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT struOutput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();
            struOutput.dwSize = (uint)Marshal.SizeOf(struOutput);
            const int ciOutSize = 1024 * 1024; //预留1M接收数据
            IntPtr ptrOut = Marshal.AllocHGlobal(ciOutSize);
            struOutput.lpOutBuffer = ptrOut;
            struOutput.dwOutBufferSize = ciOutSize;
            struOutput.lpStatusBuffer = ptrOut;
            struOutput.dwStatusSize = ciOutSize;
            IntPtr ptrOutput = Marshal.AllocHGlobal(Marshal.SizeOf(struOutput));
            Marshal.StructureToPtr(struOutput, ptrOutput, false);
            bool bRet = CHCNetSDK.NET_DVR_STDXMLConfig((int)deviceInfo.lLoginID, ptrInput, ptrOutput);
            if (!bRet)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKDebugTool", "NET_DVR_STDXMLConfig failed[" + CHCNetSDK.NET_DVR_GetLastError().ToString() + "]");
                Marshal.FreeHGlobal(ptrInput);
                Marshal.FreeHGlobal(ptrOut);
                Marshal.FreeHGlobal(ptrOutput);
                return false;
            }
            MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "SDKDebugTool", "NET_DVR_STDXMLConfig succeed");
            strOutput = Marshal.PtrToStringAnsi(ptrOut);
            Marshal.FreeHGlobal(ptrInput);
            Marshal.FreeHGlobal(ptrOut);
            Marshal.FreeHGlobal(ptrOutput);

            return true;
        }
        public static int getUploadState()
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
        private static void sendUploadData()
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
        private static bool FaceLibUpLoadThread(out string str)
        {
            str = string.Empty;
            int dwOutBufferSize = 0;
            sendUploadData();
            int iStatus = -1;
            while (true)
            {
                if (-1 == m_lUploadHandle)
                {
                    return false;
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
                            str = System.Text.Encoding.Default.GetString(m_struFileRet.sUrl);
                            return true;
                        }
                        else
                        {
                            int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                            string strErr = "errorCode: " + iLastErr;
                            MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GetUploadResult", "Failed" + strErr);
                            return false;
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
            return false;
        }
        private static void stopUploadData()
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
        public static string UploadStorageCloud(IDeviceTree.DeviceInfo deviceInfo, string picturePath, string FDID, bool ifConcurrent, string strIdentityKey, IntPtr appendData)
        {        
            CHCNetSDK.NET_DVR_FACELIB_COND struFaceLibCond = new CHCNetSDK.NET_DVR_FACELIB_COND();
            int iSize = Marshal.SizeOf(struFaceLibCond);
            struFaceLibCond.dwSize = (uint)iSize;
            byte[] byFDID = System.Text.Encoding.Default.GetBytes(FDID);
            struFaceLibCond.szFDID = new byte[256];
            byFDID.CopyTo(struFaceLibCond.szFDID, 0);
            struFaceLibCond.byConcurrent = Convert.ToByte(ifConcurrent);
            struFaceLibCond.byCustomFaceLibID = 0;
            byte[] byIdentityKey = System.Text.Encoding.Default.GetBytes(strIdentityKey);
            struFaceLibCond.byIdentityKey = new byte[64];
            byIdentityKey.CopyTo(struFaceLibCond.byIdentityKey, 0);

            IntPtr pFacelibCond = Marshal.AllocHGlobal(iSize);
            Marshal.StructureToPtr(struFaceLibCond, pFacelibCond, true);

            try
            {
                m_lUploadHandle = CHCNetSDK.NET_DVR_UploadFile_V40((int)deviceInfo.lLoginID, CHCNetSDK.IMPORT_DATA_TO_FACELIB,
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

                FileStream fs = File.OpenRead(picturePath); // OpenRead
                int iFilelength = 0;
                iFilelength = (int)fs.Length;
                Byte[] byImage = new Byte[iFilelength];
                fs.Read(byImage, 0, iFilelength);
                m_struSendParam.pSendData = Marshal.AllocHGlobal(iFilelength);
                Marshal.Copy(byImage, 0, m_struSendParam.pSendData, iFilelength);
                m_struSendParam.dwSendDataLen = (uint)iFilelength;
                fs.Close();
                m_struSendParam.byPicType = 1;
                m_struSendParam.byPicURL = 0;
                m_struSendParam.pSendAppendData = appendData;
                m_struSendParam.dwSendAppendDataLen = (uint)Marshal.SizeOf(appendData);
                string strURL = "";
                GetURLDelegate deleURL = new GetURLDelegate(FaceLibUpLoadThread);
                IAsyncResult res = deleURL.BeginInvoke(out strURL, null, null);
                while (!res.IsCompleted)
                {
                   
                    Thread.Sleep(100);
                }
                bool urlresult = deleURL.EndInvoke(out strURL, res);
                if (urlresult)
                {
                    return strURL;
                }
                else
                {
                    return string.Empty;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pFacelibCond);
            }
        }
        public static bool DeleteStorageCloud(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            IDeviceTree.DeviceInfo struDeviceInfo = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo();
            string strUrl = null;
            if (struDeviceInfo.bHttps)
            {
                strUrl = "https://" + struDeviceInfo.sDeviceIP + "/ISAPI/Intelligent/uploadStorageCloud/Delete?format=json";
            }
            else
            {
                strUrl = "http://" + struDeviceInfo.sDeviceIP + "/ISAPI/Intelligent/uploadStorageCloud/Delete?format=json";
            }

            WebClient client = new WebClient();
            client.Credentials = new NetworkCredential(struDeviceInfo.sUsername, struDeviceInfo.sPassword);
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

            string strJson = "{\"URL\",\"" + url + "\"}";
            byte[] byJson = Encoding.UTF8.GetBytes(strJson);
            try
            {
                byte[] responseData = client.UploadData(strUrl, "PUT", byJson);

                if (responseData != null)
                {
                    string strRes = Encoding.UTF8.GetString(responseData);
                    if (strRes.ToUpper().Contains("OK"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "DeleteStorageCloud", exception.Message);
            }
            return false;
        }
        public class SystemCaps
        {
            public int errorCode { get; set; }
            public string errorMsg { get; set; }
            public bool isSupportBlockFDControl { get; set; }  
            public bool isSupportExecuteControl { get; set; }   //Whether support layout control management
            public bool isSupportEventUpload { get; set; }      //Whether event reporting is supported
            public bool isSupportSDKServer { get; set; }        //Whether SDK service configuration is supported
        }

        public static bool GetSystemCaps(out SystemCaps systemCpas)
        {
            systemCpas = null;
            IDeviceTree.DeviceInfo struDeviceInfo = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo();
            string strUrl = null;
            if (struDeviceInfo.bHttps)
            {
                strUrl = "https://" + struDeviceInfo.sDeviceIP + "/ISAPI/System/capabilities?format=json";
            }
            else
            {
                strUrl = "http://" + struDeviceInfo.sDeviceIP + "/ISAPI/System/capabilities?format=json";
            }

            WebClient client = new WebClient();

            client.Credentials = new NetworkCredential(struDeviceInfo.sUsername, struDeviceInfo.sPassword);
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

            try
            {
                byte[] responseData = client.DownloadData(strUrl);

                if (responseData != null)
                {
                    string strRes = Encoding.UTF8.GetString(responseData);
                    systemCpas = JsonConvert.DeserializeObject<SystemCaps>(strRes);
                    if (systemCpas.errorMsg.ToUpper().Contains("OK"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "GetSystemCaps", exception.Message);
            }
            return false;
        }

        public class DeviceInfo
        {
            public int errorCode { get; set; }
            public string errorMsg { get; set; }
            public string deviceName { get; set; }              //Device name
            public string deviceID { get; set; }                //Device ID no.
            public string deviceDescription { get; set; }       //Device description
            public string deviceLocation { get; set; }            //Is the location of the device defined in RFC1213
            public string systemContact { get; set; }           //Is defined in RFC1213 device communication information
            public string model { get; set; }                   //Device model
            public string serialNumber { get; set; }            //Device serial number
            public string macAddress { get; set; }              //The MAC address
            public string firmwareVersion { get; set; }         //Software version
            public string firmwareReleasedDate { get; set; }    //Estimated release date
            public string bootVersion { get; set; }             //boot version
            public string bootReleasedDate { get; set; }        //boot release date
            public string hardwareVersion { get; set; }         //Hardware version
            public string encoderVersion { get; set; }          //Encoder version
            public string encoderReleasedDate { get; set; }     //Encoder release date
            public string decoderVersion { get; set; }          //Decoder version
            public string decoderReleasedDate { get; set; }     //Decoder release date
            public string deviceType { get; set; }              //Device type
            public string telecontrolID { get; set; }           //Telecontrol ID
            public string supportBeep { get; set; }             //Whether support beep
            public string language { get; set; }                //Supported languages: English, simplified Chinese
            public class IllageCharacter
            {
                public string illageCharacter { get; set; }
            }
            public LinkedList<IllageCharacter> illageCharacterList { get; set; }    //Invalid characters (this node represents a list of characters not supported by the device)
        }
        public static bool GetDeviceInfo(out DeviceInfo deviceInfo)
        {
            deviceInfo = null;
            IDeviceTree.DeviceInfo struDeviceInfo = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo();
            string strUrl = null;
            if (struDeviceInfo.bHttps)
            {
                strUrl = "https://" + struDeviceInfo.sDeviceIP + "/ISAPI/System/deviceInfo?format=json";
            }
            else
            {
                strUrl = "http://" + struDeviceInfo.sDeviceIP + "/ISAPI/System/deviceInfo?format=json";
            }

            WebClient client = new WebClient();

            client.Credentials = new NetworkCredential(struDeviceInfo.sUsername, struDeviceInfo.sPassword);
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

            try
            {
                byte[] responseData = client.DownloadData(strUrl);

                if (responseData != null)
                {
                    string strRes = Encoding.UTF8.GetString(responseData);
                    deviceInfo = JsonConvert.DeserializeObject<DeviceInfo>(strRes);
                    if (deviceInfo.errorMsg.ToUpper().Contains("OK"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "GetDeviceInfo", exception.Message);
            }
            return false;
        }

        public class PictureAnalysisCaps
        {
            public int errorCode { get; set; }
            public string errorMsg { get; set; }
            public class Values
            {
                public string value { get; set; }
            }
            public List<Values> imagesType { get; set; }
            public string modelRectSupport { get; set; }
            public List<Values> algorithmType { get; set; }
        }

        public static bool GetPictureAnalysisCaps(out PictureAnalysisCaps caps)
        {
            caps = null;
            IDeviceTree.DeviceInfo struDeviceInfo = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo();
            string strUrl = null;
            if (struDeviceInfo.bHttps)
            {
                strUrl = "https://" + struDeviceInfo.sDeviceIP + "/ISAPI/SDT/Face/pictureAnalysis/capabilities";
            }
            else
            {
                strUrl = "http://" + struDeviceInfo.sDeviceIP + "/ISAPI/SDT/Face/pictureAnalysis/capabilities";
            }

            WebClient client = new WebClient();

            client.Credentials = new NetworkCredential(struDeviceInfo.sUsername, struDeviceInfo.sPassword);
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

            try
            {
                byte[] responseData = client.DownloadData(strUrl);

                if (responseData != null)
                {
                    string strRes = Encoding.UTF8.GetString(responseData);
                    caps = JsonConvert.DeserializeObject<PictureAnalysisCaps>(strRes);
                    if (caps.errorMsg.ToUpper().Contains("OK"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "GetPictureAnalysisCaps", exception.Message);
            }
            return false;
        }

        public class PictureAnalysisCon
        {
            public string imagesType { get; set; }
            public string imagesData { get; set; }
            public string algorithmType { get; set; }
            public string mode { get; set; }
            public class Rect
            {
                public float height { get; set; }
                public float width { get; set; }
                public float x { get; set; }
                public float y { get; set; }
            }
            public List<Rect> roi { get; set; }
            public List<Rect> faceRect { get; set; }
            public class FaceMark
            {
                public class Point
                {
                    public float x { get; set; }
                    public float y { get; set; }
                }
                public Point leftEye { get; set; }
                public Point rightEye { get; set; }
                public Point noseTip { get; set; }
                public Point leftMouth { get; set; }
                public Point rightMouth { get; set; }
            }
            public FaceMark faceMark { get; set; }
        }
        public class PictureAnalysisRet
        {
            public int errorCode { get; set; }
            public string errorMsg { get; set; }
            public class Targets
            {
                public int id { get; set; }
                public int age { get; set; }
                public int ageRange { get; set; }
                public string ageGroup { get; set; }
                public string gender { get; set; }
                public string glasses { get; set; }
                public string smile { get; set; }
                public class FacePose
                {
                    public float pitch { get; set; }
                    public float yaw { get; set; }
                    public float roll { get; set; }
                }
                public FacePose facePose { get; set; }
                public string targetModelData { get; set; }
                public class Rect
                {
                    public float height { get; set; }
                    public float width { get; set; }
                    public float x { get; set; }
                    public float y { get; set; }
                }
                public Rect facetRect { get; set; }
                public Rect recommendFaceRect { get; set; }
                public class FaceMark
                {
                    public class Point
                    {
                        public float x { get; set; }
                        public float y { get; set; }
                    }
                    public Point leftEye { get; set; }
                    public Point rightEye { get; set; }
                    public Point noseTip { get; set; }
                    public Point leftMouth { get; set; }
                    public Point rightMouth { get; set; }
                }
                public FaceMark faceMark { get; set; }
            }
            public List<Targets> targets { get; set; }
        }

        public static bool PictureAnalysis(PictureAnalysisCon con, out PictureAnalysisRet ret)
        {
            ret = new PictureAnalysisRet();
            if (null == con)
            {
                return false;
            }
            IDeviceTree.DeviceInfo struDeviceInfo = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo();
            string strUrl = null;
            if (struDeviceInfo.bHttps)
            {
                strUrl = "https://" + struDeviceInfo.sDeviceIP + "/ISAPI/SDT/Face/pictureAnalysis";
            }
            else
            {
                strUrl = "http://" + struDeviceInfo.sDeviceIP + "/ISAPI/SDT/Face/pictureAnalysis";
            }

            WebClient client = new WebClient();
            client.Credentials = new NetworkCredential(struDeviceInfo.sUsername, struDeviceInfo.sPassword);
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

            string strJson = JsonConvert.SerializeObject(con);
            byte[] byJson = Encoding.UTF8.GetBytes(strJson);
            try
            {
                byte[] responseData = client.UploadData(strUrl, "POST", byJson);

                if (responseData != null)
                {
                    string strRes = Encoding.UTF8.GetString(responseData);
                    ret = JsonConvert.DeserializeObject<PictureAnalysisRet>(strRes);
                    if (ret.errorMsg.ToUpper().Contains("OK"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                ret.errorMsg = exception.Message;
            }
            return false;
        }

        private class jsonPicUrl
        {
            public int errorCode { get; set; }
            public string errorMsg { get; set; }
            public string URL { get; set; }
        }
        private class CErrorInfo
        {
            public int errorCode { get; set; }
            public string errorMsg { get; set; }
        }
        public static bool UploadPic(string path, out string urlOrErrMessage)
        {
            urlOrErrMessage = string.Empty;
            try
            {
                FileStream fs = File.OpenRead(path); //OpenRead
                if (fs == null)
                {
                    urlOrErrMessage = "Open File Failed!";
                    return false;
                }
                int filelength = 0;
                filelength = (int)fs.Length;
                byte[] image = new byte[filelength];
                fs.Read(image, 0, filelength);
                fs.Close();

                IDeviceTree.DeviceInfo struDeviceInfo = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo();
                //Assemble the url
                string strUrl = null;
                if (struDeviceInfo.bHttps)
                {
                    strUrl = "https://" + struDeviceInfo.sDeviceIP + ":" + struDeviceInfo.sDevicePort + "/ISAPI/SDT/pictureUpload";
                }
                else
                {
                    strUrl = "http://" + struDeviceInfo.sDeviceIP + ":" + struDeviceInfo.sDevicePort + "/ISAPI/SDT/pictureUpload";
                }

                //Upload form data
                HttpRequestClient httpRequestClient = new HttpRequestClient();

                httpRequestClient.SetFieldValue("imageFile", Path.GetFileName(path), "image/jpeg", image);
                ICredentials Credentials = new NetworkCredential(struDeviceInfo.sUsername, struDeviceInfo.sPassword);
                string responseText = string.Empty;
                if (httpRequestClient.Upload(Credentials, strUrl, out responseText))
                {
                    if (string.IsNullOrEmpty(responseText))
                    {
                        urlOrErrMessage = "response message is empty!";
                        return false;
                    }
                    //Parsing json data
                    jsonPicUrl jsonPicUrl = JsonConvert.DeserializeObject<jsonPicUrl>(responseText);
                    if (jsonPicUrl != null)
                    {
                        urlOrErrMessage = jsonPicUrl.URL;
                        return true;
                    }
                    else
                    {
                        urlOrErrMessage = "Upload succeed but no URL returned!";
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(responseText))
                    {
                        CErrorInfo ErrorInfo = JsonConvert.DeserializeObject<CErrorInfo>(responseText);
                        urlOrErrMessage = ErrorInfo.errorMsg;
                    }
                    else
                    {
                        urlOrErrMessage = "Unknow Error";
                    }
                }
            }
            catch (Exception exception)
            {
                urlOrErrMessage = exception.Message;
            }
            return false;
        }

        public static bool GetPicTargetModelData(string picName, out string targetModelDataOrErrMessage)
        {
            targetModelDataOrErrMessage = string.Empty;
            string picUrl = string.Empty;
            if (!UploadPic(picName, out picUrl))
            {
                targetModelDataOrErrMessage = picUrl;
                return false;
            }
            PictureAnalysisCon picAnalysisCon = new PictureAnalysisCon();
            picAnalysisCon.imagesType = "URL";
            picAnalysisCon.imagesData = picUrl;
            picAnalysisCon.algorithmType = "faceStruct";//FaceDetect,face structuring,faceStruct, face modeling faceModel
            picAnalysisCon.mode = "singleface"; //Singleface attribute value: singleface, multiple face attribute value: mutiface

            PictureAnalysisRet picAnalysisRet = null;
            if (PictureAnalysis(picAnalysisCon, out picAnalysisRet))
            {
                if (picAnalysisRet != null && picAnalysisRet.targets != null && picAnalysisRet.targets.Count > 0)
                {
                    if (!string.IsNullOrEmpty(picAnalysisRet.targets[0].targetModelData))
                    {
                        targetModelDataOrErrMessage = picAnalysisRet.targets[0].targetModelData;
                        return true;
                    }
                    else
                    {
                        targetModelDataOrErrMessage = "targetModelData is null";
                    }
                }
                else
                {
                    targetModelDataOrErrMessage = "no recv targetModelData";
                }
            }
            else
            {
                if (picAnalysisRet != null)
                {
                    targetModelDataOrErrMessage = picAnalysisRet.errorMsg;
                }
            }

            return false;
        }

        /// <summary>
        /// List update
        /// </summary>
        /// <param name="dateVersion">Data version stamp</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool FaceFDNotice(string dateVersion, int type, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(dateVersion))
            {
                errorMessage = "dateVersion is empty";
                return false;
            }
            IDeviceTree.DeviceInfo struDeviceInfo = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo();
            string strUrl = null;
            if (struDeviceInfo.bHttps)
            {
                strUrl = "https://" + struDeviceInfo.sDeviceIP + "/ISAPI/SDT/Face/FD/notice";
            }
            else
            {
                strUrl = "http://" + struDeviceInfo.sDeviceIP + "/ISAPI/SDT/Face/FD/notice";
            }

            WebClient client = new WebClient();
            client.Credentials = new NetworkCredential(struDeviceInfo.sUsername, struDeviceInfo.sPassword);
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

            string strJson = "{\"dataVersion\":\"" + dateVersion + "\",\"type\":" + type.ToString() + "}";
            byte[] byJson = Encoding.UTF8.GetBytes(strJson);
            try
            {
                byte[] responseData = client.UploadData(strUrl, "POST", byJson);

                if (responseData != null)
                {
                    string strRes = Encoding.UTF8.GetString(responseData);
                    if (strRes.ToUpper().Contains("OK"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
            }
            return false;
        }
    }
}
