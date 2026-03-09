/*******************************************************
Copyright All Rights Reserved. (C) HangZhou Hikvision System Technology Co., Ltd. 
File ：    FormFaceSnapAlarm.cs 
Developer：    Hikvision
Author：    chenzhixue@hikvision.com
Period：    2019-07-18
Describe：    FormFaceSnapAlarm.cs
********************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Common;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;

namespace SDKFaceSnap
{
    public partial class FormFaceSnapAlarm : Form
    {
        private IDeviceTree g_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        private int m_iCurDeviceIndex = -1;
        private int m_iCurChanNo = -1;
        private int m_iCurChanIndex = -1;
        public int m_iStreamType = 0;
        private IDeviceTree.DeviceInfo m_deviceInfo = null;
        private IDeviceTree.ChannelInfo m_channelInfo = null;
        private static SynchronizationContext m_SyncContext = null;
        private IntPtr m_pRtsp = IntPtr.Zero;
        private int m_lUserID = -1;
        private CHCNetSDK.PREVIEW_IFNO m_strPanelInfo = new CHCNetSDK.PREVIEW_IFNO();
        private List<byte> byBuffer = new List<byte>();
        public delegate bool ProcessLongLinkData(byte[] data, string boundary);
        private CredentialCache _credentialCache = null;
        private string strURL = string.Empty;
        public delegate bool ProcessSendDate(ref byte[] byBuffer);
        public static int m_iHttpTimeOut = 5000;
        const int BUFFER_SIZE = 3 * 1024 * 1024;
        public ProcessLongLinkData processLongLinkData = null;
        public bool m_bIsAlarmStart = false;
        private Dictionary<long, int> m_dAlarmHanldes = new Dictionary<long, int>();
        private CHCNetSDK.MSGCallBack m_falarmData = null;
        public delegate void UpdateListBoxCallback(string strAlarmTime, string strDevIP, string strAlarmMsg);

        public enum HttpStatus
        {
            Http200 = 0,
            HttpOther,
            HttpTimeOut
        }

        public class jsonAlarmInfo
        {
            public string ipAddress { get; set; }
            public int portNo { get; set; }
            public string protocol { get; set; }
            public string macAddress { get; set; }
            public int channelID { get; set; }
            public string dateTime { get; set; }
            public int activePostCount { get; set; }
            public string eventType { get; set; }
            public string eventState { get; set; }
            public string eventDescription { get; set; }
        }
        jsonAlarmInfo m_jsonAlarmInfo;

        public class RequestState
        {
            // This class stores the State of the request.
            const int BUFFER_SIZE = 3 * 1024 * 1024;
            public StringBuilder requestData;
            public byte[] BufferRead;
            public HttpWebRequest request;
            public HttpWebResponse response;
            public Stream streamResponse;
            public ProcessLongLinkData processLongLinkData;
            public string strBoundary;
            public ProcessSendDate processSendData;
            public WebException eStatus;

            public RequestState()
            {
                BufferRead = new byte[BUFFER_SIZE];
                requestData = new StringBuilder("");
                request = null;
                streamResponse = null;
                processLongLinkData = null;
                strBoundary = string.Empty;
                processSendData = null;
                eStatus = null;
            }
        }

        public FormFaceSnapAlarm()
        {
            InitializeComponent();

            cbFaceMatchDataType.SelectedIndex = 0;
            cbAlarmRank.SelectedIndex = 0;
            cbAlarmType.SelectedIndex = 0;

            m_strPanelInfo.lRealHandle = -1;
            m_SyncContext = SynchronizationContext.Current;
            if (null != g_deviceTree)
            {
                g_deviceTree.SelectedNodeChanged += g_deviceTree_SelectedNodeChanged;
            }
            this.GetLoginInfo();

            if (m_falarmData == null)
            {
                m_falarmData = new CHCNetSDK.MSGCallBack(MsgCallback);
            }

            CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V51(0, m_falarmData, IntPtr.Zero);
        }

        private void GetDevicesInfo()
        {
            if (g_deviceTree != null)
            {
                this.m_deviceInfo = g_deviceTree.GetSelectedDeviceInfo();
                this.m_channelInfo = g_deviceTree.GetSelectedChannelInfo();
            }
        }

        private void GetLoginInfo()
        {
            m_iCurDeviceIndex = g_deviceTree.GetSelectedDeviceInfo().iDeviceIndex;
            m_iCurChanIndex = g_deviceTree.GetSelectedChannelInfo().iChannelIndex;
            m_iCurChanNo = g_deviceTree.GetSelectedChannelInfo().iChannelNo;

            if (m_iCurDeviceIndex > -1 && g_deviceTree.GetSelectedChannelInfo().iChannelNo > -1)
            {
                m_lUserID = (int)g_deviceTree.GetSelectedDeviceInfo().lLoginID;
            }
        }

        void g_deviceTree_SelectedNodeChanged()
        {
            this.GetDevicesInfo();
            this.GetLoginInfo();
        }

        public static long IpToInt(string ip)
        {
            char[] separator = new char[] { '.' };
            string[] items = ip.Split(separator);
            return long.Parse(items[0]) << 24
                    | long.Parse(items[1]) << 16
                    | long.Parse(items[2]) << 8
                    | long.Parse(items[3]);
        }

        public int StartEventLongHttp()
        {
            string strDeviceIp = "";
            int sPort = 80;
            string strUsername = "";
            string strPassword = "";

            IDeviceTree.DeviceInfo deviceInfo = g_deviceTree.GetSelectedDeviceInfo();
            strDeviceIp = deviceInfo.sDeviceIP;
            sPort = deviceInfo.sDevicePort;
            strUsername = deviceInfo.sUsername;
            strPassword = deviceInfo.sPassword;

            string strHttpMethod = "";

            string strUrl = "";
            string strparam = "";

            strHttpMethod = "GET";
            strUrl = "http://" + strDeviceIp + ":" + sPort + "/ISAPI/Event/notification/alertStream";
            strparam = "";

            string strResponse = string.Empty;
            //szBuffer.Clear();
            byBuffer.Clear();
            m_bIsAlarmStart = true;

            processLongLinkData = new ProcessLongLinkData(ParseAlarmData);

            int iRet = StartHttpLongLink(strUsername, strPassword, strUrl, strHttpMethod
                , strparam, processLongLinkData, ref strResponse, false);

            if (iRet == (int)HttpStatus.HttpOther)
            {
                string statusCode = string.Empty;
                string statusString = string.Empty;
                ParserResponseStatus(strResponse, ref statusCode, ref statusString);
            }
            //bIsAlarmStart = false;
            return iRet;
        }

        private static void ReceiveData(IAsyncResult asyncResult)
        {
            try
            {
                RequestState myRequestState = (RequestState)asyncResult.AsyncState;
                Stream responseStream = myRequestState.streamResponse;
                int read = responseStream.EndRead(asyncResult);
                if (read > 0)
                {
                    if (myRequestState.processLongLinkData != null)
                    {
                        //                         Console.WriteLine("Recive Buffer:" + System.Text.Encoding.Default.GetString (myRequestState.BufferRead));
                        Byte[] pBuf = new Byte[read];
                        Array.Copy(myRequestState.BufferRead, pBuf, read);

                        if (!myRequestState.processLongLinkData(pBuf
                            , myRequestState.strBoundary))
                        {
                            MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "ReceiveData", "delegate processLongLinkData return false!");
                            responseStream.Close();
                            return;
                        }
                    }
                    IAsyncResult asynchronousResult = responseStream.BeginRead(myRequestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReceiveData), myRequestState);
                    return;
                }
                else
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "ReceiveData", "responseStream.EndRead length is  ! " + read);
                    responseStream.Close();
                }

            }
            catch (WebException e)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "ReceiveData", e.Status + e.Message, e.ToString());
            }
            catch (IOException e)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "ReceiveData", e.Source + e.Message, e.ToString());
            }
        }

        // Request a callback
        private static void RespCallback(IAsyncResult asynchronousResult)
        {
            // State of request is asynchronous.
            RequestState myRequestState = (RequestState)asynchronousResult.AsyncState;
            try
            {
                HttpWebRequest myHttpWebRequest = myRequestState.request;
                myRequestState.response = (HttpWebResponse)myHttpWebRequest.EndGetResponse(asynchronousResult);

                string strBoundary = myRequestState.response.ContentType;
                int nIndex = strBoundary.IndexOf("boundary=");
                if (nIndex >= 0)
                {
                    strBoundary = strBoundary.Substring(nIndex + "boundary=".Length);
                    myRequestState.strBoundary = strBoundary;
                }

                // Read the response into a Stream object.
                Stream responseStream = myRequestState.response.GetResponseStream();
                myRequestState.streamResponse = responseStream;

                // Begin the Reading of the contents of the HTML page and print it to the console.
                IAsyncResult asynchronousInputRead = responseStream.BeginRead(myRequestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReceiveData), myRequestState);
                return;
            }
            catch (WebException e)
            {
                myRequestState.eStatus = e;
            }
        }

        // HTTP long connection
        public int StartHttpLongLink(string strUserName, string strPassword, string strUrl, string strHttpMethod
   , string strparam, ProcessLongLinkData processLongLinkData, ref string strResponse, bool bBlock = true)
        {

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(strUrl);
            request.Credentials = GetCredentialCache(strUrl, strUserName, strPassword);
            request.Method = strHttpMethod;

            if (!string.IsNullOrEmpty(strparam))
            {
                byte[] bs = Encoding.ASCII.GetBytes(strparam);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = bs.Length;
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(bs, 0, bs.Length);
                }
            }

            try
            {
                RequestState myRequestState = new RequestState();
                myRequestState.request = request;
                myRequestState.processLongLinkData = processLongLinkData;
                IAsyncResult ret = request.BeginGetResponse(new AsyncCallback(RespCallback), myRequestState);

                if (bBlock)
                {
                    int nTimeoutLimit = m_iHttpTimeOut / 100;
                    int nTimeoutCount = 0;
                    while (!ret.IsCompleted && nTimeoutCount < nTimeoutLimit)
                    {
                        Thread.Sleep(100);
                        nTimeoutCount++;
                    }

                    if (nTimeoutCount == nTimeoutLimit)
                    {
                        request.Abort();
                    }

                    if (myRequestState.response != null && myRequestState.response.StatusCode == HttpStatusCode.OK)
                    {
                        return (int)HttpStatus.Http200;
                    }
                    else
                    {
                        if (myRequestState.eStatus != null)
                        {
                            if (myRequestState.eStatus.Response != null)
                            {
                                Stream st = myRequestState.eStatus.Response.GetResponseStream();
                                StreamReader sr = new StreamReader(st, System.Text.Encoding.Default);
                                strResponse = sr.ReadToEnd();
                                sr.Close();
                                st.Close();
                                return (int)HttpStatus.HttpOther;
                            }
                            else
                            {
                                strResponse = myRequestState.eStatus.Status.ToString();
                                return (int)HttpStatus.HttpTimeOut;
                            }

                        }
                        return (int)HttpStatus.HttpOther;
                    }
                }
                else
                {
                    return (int)HttpStatus.Http200;
                }
            }
            catch (WebException ex)
            {
                WebResponse wr = ex.Response;
                if (wr != null)
                {
                    return (int)HttpStatus.HttpOther;
                }
                else
                {
                    return (int)HttpStatus.HttpTimeOut;
                }
            }
        }

        // parsing ResponseStatus
        public int ParserResponseStatus(string httpBody, ref string statusCode, ref string statusString)
        {
            if (httpBody == "Timeout")
            {
                return 0;
            }

            try
            {
                if (httpBody != string.Empty)
                {
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(httpBody);
                    if (xml.DocumentElement != null && xml.DocumentElement.Name == "ResponseStatus")
                    {
                        XmlNodeList childNode = xml.DocumentElement.ChildNodes;
                        foreach (XmlNode node in childNode)
                        {
                            if (node.Name == "statusCode")
                            {
                                statusCode = node.InnerText;
                            }
                            if (node.Name == "statusString")
                            {
                                statusString = node.InnerText;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                statusString = httpBody;
            }
            return 0;
        }

        // Add the certification
        private CredentialCache GetCredentialCache(string sUrl, string strUserName, string strPassword)
        {
            if (_credentialCache == null)
            {
                _credentialCache = new CredentialCache();
                _credentialCache.Add(new Uri(sUrl), "Digest", new NetworkCredential(strUserName, strPassword));
                strURL = sUrl;
            }
            // Judge certification
            if (_credentialCache.GetCredential(new Uri(sUrl), "Digest") == null)
            {
                _credentialCache.Add(new Uri(sUrl), "Digest", new NetworkCredential(strUserName, strPassword));
            }

            return _credentialCache;
        }

        private void SDK_StartAlarm()
        {
            IDeviceTree.DeviceInfo deviceInfo = g_deviceTree.GetSelectedDeviceInfo();
            if (this.m_dAlarmHanldes.ContainsKey(deviceInfo.lLoginID))
            {
                MessageBox.Show("The Selected Device is in Alarm state!");
            }
            else
            {
                CHCNetSDK.NET_DVR_SETUPALARM_PARAM_V50 struAlarmParam = new CHCNetSDK.NET_DVR_SETUPALARM_PARAM_V50();

                struAlarmParam.dwSize = (uint)Marshal.SizeOf(struAlarmParam);
                struAlarmParam.byLevel = 1;
                struAlarmParam.byAlarmInfoType = Convert.ToByte(cbAlarmRank.SelectedIndex + 1);
                struAlarmParam.byFaceAlarmDetection = 1;
                struAlarmParam.byDeployType = Convert.ToByte(cbAlarmType.SelectedIndex);
                struAlarmParam.byAlarmTypeURL |= Convert.ToByte((cbFaceMatchDataType.SelectedIndex) << 2);
                struAlarmParam.byBrokenNetHttp |= Convert.ToByte((Convert.ToByte(chkFaceMatchBroken.Checked)) << 2);

                long lAlarmHandle = CHCNetSDK.NET_DVR_SetupAlarmChan_V50(Convert.ToInt32(deviceInfo.lLoginID), ref struAlarmParam, IntPtr.Zero, 0);
                if (lAlarmHandle < 0)
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "errCode" + iLastErr + "." + CHCNetSDK.NET_DVR_GetErrorMsg(ref iLastErr);
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_SetupAlarmChan_V50", "failed :" + strErr);
                    return;
                }
                else
                {
                    this.m_dAlarmHanldes.Add(deviceInfo.lLoginID, Convert.ToInt32(lAlarmHandle));
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_SetupAlarmChan_V50", "success ! ");
                }
            }
        }

        // Asynchronous access to a control's delegate
        public void UpdateClientList(string strAlarmTime, string strIPAlarmType, string strAlarmMsg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new UpdateListBoxCallback(UpdateClientList), strAlarmTime, strIPAlarmType, strAlarmMsg);
            }
            else
            {
                if (AlarmabsListView.Items.Count > 200)
                {
                    AlarmabsListView.Items.RemoveAt(0);
                }
                AlarmabsListView.Items.Add(new ListViewItem(new string[] { strAlarmTime, strIPAlarmType, strAlarmMsg }));
            }
        }

        // SDK MsgCallback
        public void MsgCallback(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            switch (lCommand)
            {
                case CHCNetSDK.COMM_UPLOAD_FACESNAP_RESULT:
                    ProcessCommAlarm_FaceSnap(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                default:
                    {
                        string strIP = pAlarmer.sDeviceIP;
                        string stringAlarm = "Alarm upload, AlarmType：" + lCommand;

//                        UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
                    }
                    break;
            }
        }

        private int iPicCount = 0;

        // Parse SDK alarm
        private void ProcessCommAlarm_FaceSnap(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_VCA_FACESNAP_RESULT struFaceSnapInfo = new CHCNetSDK.NET_VCA_FACESNAP_RESULT();
            uint dwSize = (uint)Marshal.SizeOf(struFaceSnapInfo);
            struFaceSnapInfo = (CHCNetSDK.NET_VCA_FACESNAP_RESULT)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_VCA_FACESNAP_RESULT));
            string strAlarmType = "COMM_UPLOAD_FACESNAP_RESULT";
            string strIP = pAlarmer.sDeviceIP;
            string strFileSavePath = "C:\\SDK_Alarm_Guard" + "\\" + strIP;
            if (!Directory.Exists(strFileSavePath))
            {
                Directory.CreateDirectory(strFileSavePath);
            }

            string strTimeYear = ((struFaceSnapInfo.dwAbsTime >> 26) + 2000).ToString();
            string strTimeMonth = ((struFaceSnapInfo.dwAbsTime >> 22) & 15).ToString("d2");
            string strTimeDay = ((struFaceSnapInfo.dwAbsTime >> 17) & 31).ToString("d2");
            string strTimeHour = ((struFaceSnapInfo.dwAbsTime >> 12) & 31).ToString("d2");
            string strTimeMinute = ((struFaceSnapInfo.dwAbsTime >> 6) & 63).ToString("d2");
            string strTimeSecond = ((struFaceSnapInfo.dwAbsTime >> 0) & 63).ToString("d2");
            string strTime = strTimeYear + "-" + strTimeMonth + "-" + strTimeDay + "-" + strTimeHour + "-" + strTimeMinute + "-" + strTimeSecond;

            if ((struFaceSnapInfo.dwBackgroundPicLen != 0) && (struFaceSnapInfo.pBuffer2 != IntPtr.Zero))
            {
                iPicCount++;
                string str = strFileSavePath + "\\[" + strTime + "][" + DateTime.Now.Ticks / 1000 + "]Background" + ".jpg";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)struFaceSnapInfo.dwBackgroundPicLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struFaceSnapInfo.pBuffer2, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();

                PicShow(iPicCount, str);
            }

            if ((struFaceSnapInfo.dwFacePicLen != 0) && (struFaceSnapInfo.pBuffer1 != IntPtr.Zero))
            {
                iPicCount++;
                string str = "";
                if (1 == struFaceSnapInfo.byUploadEventDataType)
                {
                    str = strFileSavePath + "\\[" + strTime + "]" + "[" + DateTime.Now.Ticks / 1000 + "]Face" + ".txt";
                }
                else
                {
                    str = strFileSavePath + "\\[" + strTime + "]" + "[" + DateTime.Now.Ticks / 1000 + "]Face" + ".jpg";
                }
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)struFaceSnapInfo.dwFacePicLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struFaceSnapInfo.pBuffer1, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();

                if (0 == struFaceSnapInfo.byUploadEventDataType)
                {
                    PicShow(iPicCount, str);
                }
            }

            if ((struFaceSnapInfo.byUIDLen != 0) && (struFaceSnapInfo.pUIDBuffer != IntPtr.Zero))
            {
                string str = strFileSavePath + "\\[" + strTime + "][" + DateTime.Now.Ticks / 1000 + "]UIDBuffer" + ".txt";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)struFaceSnapInfo.byUIDLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struFaceSnapInfo.pUIDBuffer, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();
            }

            string stringAlarm = "";
            if (1 == struFaceSnapInfo.byAddInfo)
            {
                CHCNetSDK.NET_VCA_FACESNAP_ADDINFO m_struAddInfoInfo = new CHCNetSDK.NET_VCA_FACESNAP_ADDINFO();
                m_struAddInfoInfo = (CHCNetSDK.NET_VCA_FACESNAP_ADDINFO)Marshal.PtrToStructure(struFaceSnapInfo.pAddInfoBuffer, typeof(CHCNetSDK.NET_VCA_FACESNAP_ADDINFO));
                if ((m_struAddInfoInfo.dwThermalPicLen != 0) && (m_struAddInfoInfo.pThermalPicBuff != IntPtr.Zero))
                {
                    iPicCount++;
                    string str = strFileSavePath + "\\[" + strTime + "][" + DateTime.Now.Ticks / 1000 + "]ThermalPic" + ".jpg";
                    FileStream fs = new FileStream(str, FileMode.Create);
                    int iLen = (int)m_struAddInfoInfo.dwThermalPicLen;
                    byte[] by = new byte[iLen];
                    Marshal.Copy(m_struAddInfoInfo.pThermalPicBuff, by, 0, iLen);
                    fs.Write(by, 0, iLen);
                    fs.Close();

                    PicShow(iPicCount, str);
                }

                string csMask = "";
                if (2 == struFaceSnapInfo.struFeature.byMask)
                {
                    csMask = "Yes";
                }
                else if (1 == struFaceSnapInfo.struFeature.byMask)
                {
                    csMask = "NO";
                }
                else
                {
                    csMask = "Unknow";
                }

                stringAlarm = "FaceSnap,DevIP：" + struFaceSnapInfo.struDevInfo.struDevIP.sIpV4 + "\r\nAlarmTime：" + strTime + "\r\nfFaceTemperature: " + m_struAddInfoInfo.fFaceTemperature.ToString() +
                    "\r\nfAlarmTemperature: " + m_struAddInfoInfo.fAlarmTemperature.ToString() + "\r\nIs Mark：" + csMask;
            }
            else
            {
                stringAlarm = "FaceSnap,DevIP：" + struFaceSnapInfo.struDevInfo.struDevIP.sIpV4 + "\r\nAlarmTime：" + strTime;
            }           

            UpdateClientList(DateTime.Now.ToString(), strAlarmType, stringAlarm);
        }

        private void PicShow(int Count,string strPicPath)
        {
            try
            {
                switch (Count % 4)
                {
                    case 1:
                        if (this.VisiblePicBox.Image != null)
                        {
                            this.VisiblePicBox.Image.Dispose();
                        }
                        this.VisiblePicBox.Image = Image.FromFile(strPicPath);
                        break;
                    case 2:
                        if (this.ThermalPicBox.Image != null)
                        {
                            this.ThermalPicBox.Image.Dispose();
                        }
                        this.ThermalPicBox.Image = Image.FromFile(strPicPath);
                        break;
                    case 3:
                        if (this.pictureBox3.Image != null)
                        {
                            this.pictureBox3.Image.Dispose();
                        }
                        this.pictureBox3.Image = Image.FromFile(strPicPath);
                        break;
                    case 0:
                        if (this.pictureBox4.Image != null)
                        {
                            this.pictureBox4.Image.Dispose();
                        }
                        this.pictureBox4.Image = Image.FromFile(strPicPath);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "PicShow", e.Source + e.Message, e.ToString());
                return;
            }
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            String sRet = "Start failed!";
            SDK_StartAlarm();
        }

        public class CHeartBeat
        {
            public CHeartBeat()
            {
                EventNotificationAlert = new CEventNotificationAlert();
            }
            public class CEventNotificationAlert
            {
                public string dataTime { get; set; }
                public int activePostCount { get; set; }
                public string eventType { get; set; }
                public string eventState { get; set; }
                public string eventDescription { get; set; }
            }
            public CEventNotificationAlert EventNotificationAlert { get; set; }
        }

        internal int IndexOf(byte[] src, int index, byte[] value)
        {
            if (src == null || value == null)
            {
                return -1;
            }

            if (src.Length == 0 || src.Length < index
                || value.Length == 0 || src.Length < value.Length)
            {
                return -1;
            }
            for (int i = index; i < src.Length - value.Length; i++)
            {
                if (src[i] == value[0])
                {
                    if (value.Length == 1)
                    {
                        return i;
                    }
                    bool flag = true;
                    for (int j = 1; j < value.Length; j++)
                    {
                        if (src[i + j] != value[j])
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private string getAlarmPicName(string strDisposition)
        {
            string strAlarmPicName = "";
            int iIndexPic = strDisposition.IndexOf("filename");
            if (iIndexPic >= 0)
            {
                strAlarmPicName = strDisposition.Substring(iIndexPic);
                Regex regex = new Regex("=\"(\\S+?.jpg)\"");
                Match match = regex.Match(strAlarmPicName);
                strAlarmPicName = match.Groups[1].Value;
            }
            return strAlarmPicName;
        }

        private void SetTextSafePost(object lvi)
        {
            if (AlarmabsListView.Items.Count > 100)
            {
                AlarmabsListView.Items.RemoveAt(100);
            }
            ListViewItem temp = lvi as ListViewItem;
            AlarmabsListView.Items.Insert(0, temp);
            //AlarmdetailTBox.Text = temp.SubItems[3].Text;
        }

        public string m_Boundary = "boundary";
        string strdatatime = "";
        int piccount = 0;

        // Parse HTTP alarm
        public bool ParseAlarmData(byte[] sourcedata, string boundary)
        {
            try
            {
                byBuffer.AddRange(sourcedata);

                byte[] data = byBuffer.ToArray();

                if (boundary.Length <= 0)
                {
                    return false;
                }

                if (!m_bIsAlarmStart)
                {
                    return false;
                }

                byte[] bBoundary = System.Text.Encoding.Default.GetBytes("--" + boundary + "\r\n");

                int iIndex = this.IndexOf(data, 0, bBoundary);
                int iLen = 0;
                while (iIndex >= 0)
                {
                    iIndex += bBoundary.Length;
                    //
                    byte[] bContenttype = System.Text.Encoding.Default.GetBytes("Content-Type:");
                    int iIndexNext = this.IndexOf(data, iIndex, bContenttype);
                    if (iIndexNext < 0)
                    {
                        break;
                    }
                    // Get filename
                    string strDisposition = System.Text.Encoding.UTF8.GetString(data, iIndex, iIndexNext - iIndex).Trim();
                    string strAlarmPicName = getAlarmPicName(strDisposition);

                    iIndexNext += bContenttype.Length;

                    byte[] brn = System.Text.Encoding.Default.GetBytes("\r\n");
                    int iIndexEnd = this.IndexOf(data, iIndexNext, brn);
                    if (iIndexEnd < 0)
                    {
                        break;
                    }
                    iIndexEnd += brn.Length;

                    string strType = System.Text.Encoding.Default.GetString(data, iIndexNext, iIndexEnd - iIndexNext).Trim();

                    ///////
                    byte[] bContentlen = System.Text.Encoding.Default.GetBytes("Content-Length:");

                    iIndexNext = this.IndexOf(data, iIndex, bContentlen);

                    if (iIndexNext < 0)
                    {
                        break;
                    }
                    iIndexNext += bContentlen.Length;

                    brn = System.Text.Encoding.Default.GetBytes("\r\n");
                    iIndexEnd = this.IndexOf(data, iIndexNext, brn);
                    if (iIndexEnd < 0)
                    {
                        break;
                    }
                    iIndexEnd += brn.Length;

                    string strLen = System.Text.Encoding.Default.GetString(data, iIndexNext, iIndexEnd - iIndexNext).Trim();

                    byte[] brnrn = System.Text.Encoding.Default.GetBytes("\r\n\r\n");   // Content-ID
                    iIndexEnd = this.IndexOf(data, iIndexNext, brnrn);
                    if (iIndexEnd < 0)
                    {
                        break;
                    }
                    iIndexEnd += brnrn.Length;

                    int iLendata = 0;
                    if (!int.TryParse(strLen, out iLendata) || (iIndexEnd + iLendata) > data.Length)
                    {
                        break;
                    }
                    iIndexNext = iIndexEnd + iLendata;

                    strdatatime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff");

                    if (strType.Contains("xml"))
                    {
                        string strXml = System.Text.Encoding.UTF8.GetString(data, iIndexEnd, iLendata).Trim();
                        //string strXml = strXml.Substring(iIndexEnd, iLenXML);
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(strXml);

                        XmlNode rootNode = xmlDoc.DocumentElement;
                        {
                            string strIP = string.Empty;
                            string strPort = string.Empty;
                            string strTime = string.Empty;
                            string strChannel = string.Empty;
                            string strAlarmType = string.Empty;
                            XmlNode eventStateNode = null;
                            for (int i = 0; i < rootNode.ChildNodes.Count; i++)
                            {
                                if (rootNode.ChildNodes[i].Name == "eventState")
                                {
                                    eventStateNode = rootNode.ChildNodes[i];
                                }
                                if (rootNode.ChildNodes[i].Name == "ipAddress")
                                {
                                    strIP = rootNode.ChildNodes[i].InnerText;
                                }
                                if (rootNode.ChildNodes[i].Name == "portNo")
                                {
                                    strPort = rootNode.ChildNodes[i].InnerText;
                                }
                                if (rootNode.ChildNodes[i].Name == "dateTime")
                                {
                                    strTime = rootNode.ChildNodes[i].InnerText;
                                    string strTmp = strTime.Substring(0, strTime.IndexOf("T"));
                                    strTime = strTime.Substring(strTime.IndexOf("T") + 1) + " " + strTmp;
                                }
                                if (rootNode.ChildNodes[i].Name == "channelID")
                                {
                                    strChannel = rootNode.ChildNodes[i].InnerText;
                                }
                                if (rootNode.ChildNodes[i].Name == "eventType")
                                {
                                    strAlarmType = rootNode.ChildNodes[i].InnerText;
                                }
                            }

                            if (eventStateNode != null && eventStateNode.InnerText.ToLower() != "inactive")
                            {
                                piccount = 0;
                                ListViewItem lvi = new ListViewItem();
                                lvi.Text = strdatatime;
                                lvi.SubItems.Add(strAlarmType);
                                lvi.SubItems.Add(strXml);

                                //Save XML
                                string strFileSavePath = "C:\\ISAPI_Alarm_Guard";
                                if (!Directory.Exists(strFileSavePath))
                                {
                                    Directory.CreateDirectory(strFileSavePath);
                                }
                                strFileSavePath = strFileSavePath + "\\" + strAlarmType + "_XML[" + strdatatime + "].xml";
                                FileStream fs = new FileStream(strFileSavePath, FileMode.Create);
                                byte[] szXml = System.Text.Encoding.UTF8.GetBytes(strXml);
                                fs.Write(szXml, 0, szXml.Length);
                                fs.Close();
                                m_SyncContext.Post(SetTextSafePost, lvi);
                            }
                        }

                    }
                    else if (strType.Contains("json"))
                    {
                        string strJson = System.Text.Encoding.UTF8.GetString(data, iIndexEnd, iLendata).Trim();
                        if (strJson.Contains("SubscribeDeviceMgmtRsp"))
                        {
                            // Subscribe
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = "";
                            lvi.SubItems.Add("SubscribeDeviceMgmtRsp");
                            lvi.SubItems.Add(strJson);

                            m_SyncContext.Post(SetTextSafePost, lvi);
                        }
                        else if (strJson.ToLower().Contains("\"heartbeat\""))
                        {
                            //Heartbeat
                            CHeartBeat heartBeat = JsonConvert.DeserializeObject<CHeartBeat>(strJson);
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = heartBeat.EventNotificationAlert.dataTime;
                            lvi.SubItems.Add(heartBeat.EventNotificationAlert.eventType);
                            lvi.SubItems.Add(strJson);

                            m_SyncContext.Post(SetTextSafePost, lvi);
                        }
                        else
                        {
                            strdatatime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff");
                            m_jsonAlarmInfo = JsonConvert.DeserializeObject<jsonAlarmInfo>(strJson);
                            string strIP = m_jsonAlarmInfo.ipAddress;
                            if (null != m_jsonAlarmInfo.eventType)
                            {
                                ListViewItem lvi = new ListViewItem();
                                lvi.Text = strdatatime;
                                lvi.SubItems.Add(m_jsonAlarmInfo.eventType);
                                lvi.SubItems.Add(strJson);

                                m_SyncContext.Post(SetTextSafePost, lvi);
                            }
                            else
                            {
                                ListViewItem lvi = new ListViewItem();
                                lvi.Text = strdatatime;
                                lvi.SubItems.Add("Unknown Alarm");
                                lvi.SubItems.Add(strJson);
                                m_SyncContext.Post(SetTextSafePost, lvi);
                            }

                            //Save json
                            string strFileSavePath = "C:\\ISAPI_Alarm_Guard" + "\\" + strIP;
                            if (!Directory.Exists(strFileSavePath))
                            {
                                Directory.CreateDirectory(strFileSavePath);
                            }
                            strFileSavePath = strFileSavePath + "\\[" + strdatatime + "].json";
                            FileStream fs = new FileStream(strFileSavePath, FileMode.Create);
                            byte[] szJson = System.Text.Encoding.UTF8.GetBytes(strJson);
                            fs.Write(szJson, 0, szJson.Length);
                            fs.Close();
                        }
                    }
                    else if (strType.Contains("image"))
                    {
                        piccount = piccount + 1;
                        string strIP = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo().sDeviceIP;
                        string strFileSavePath = "C:\\ISAPI_Alarm_Guard\\" + strIP;
                        if (!Directory.Exists(strFileSavePath))
                        {
                            Directory.CreateDirectory(strFileSavePath);
                        }

                        byte[] imagetemp = new byte[iLendata];
                        Array.Copy(data, iIndexEnd, imagetemp, 0, iLendata);

                        //Save Pic
                        if (strAlarmPicName != "")
                        {
                            strFileSavePath = strFileSavePath + "\\" + strdatatime + "_" + strAlarmPicName;

                            FileStream fs = new FileStream(strFileSavePath, FileMode.Create);
                            fs.Write(imagetemp, 0, iLendata);
                            fs.Close();

                            if (!strAlarmPicName.Contains("plateBinaryPicture"))
                            {
                                PicShow(piccount, strFileSavePath);
                            }
                        }
                    }

                    iLen = iIndexNext;
                    if (iIndexNext + bBoundary.Length < data.Length)
                    {
                        iIndex = this.IndexOf(data, iIndexNext, bBoundary);
                    }
                    else
                    {
                        iIndex = -1;
                    }

                }
                if (iLen > 0)
                {
                    byBuffer.RemoveRange(0, iLen);
                }
                return true;
            }
            catch (Exception e)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "ParseAlarmData", e.Source + e.Message, e.ToString());
                return false;
            }

        }

        private void StopBtn_Click(object sender, EventArgs e)
        {

            if (g_deviceTree != null)
            {
                IDeviceTree.DeviceInfo deviceInfo = g_deviceTree.GetSelectedDeviceInfo();
                if (!CHCNetSDK.NET_DVR_CloseAlarmChan_V30(this.m_dAlarmHanldes[deviceInfo.lLoginID]))
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "errcode = " + iLastErr + "." + CHCNetSDK.NET_DVR_GetErrorMsg(ref iLastErr);
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_CloseAlarmChan_V30", "failed " + "strErr! ");
                    return;
                }
                else
                {
                    this.m_dAlarmHanldes.Remove(deviceInfo.lLoginID);
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_CloseAlarmChan_V30", "success！");
                }
            }
        }

        private void listViewAlarmInfo_Click(object sender, EventArgs e)
        {
            if (AlarmabsListView.SelectedItems.Count > 0)
            {
                AlarmdetailTBox.Text = AlarmabsListView.SelectedItems[0].SubItems[2].Text;
            }
        }

        private void FormAlarm_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_bIsAlarmStart = false;
            MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "FaceContrast Alarm", "Stop success!");
        }

    }
}
