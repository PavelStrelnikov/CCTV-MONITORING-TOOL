/*******************************************************
Copyright All Rights Reserved. (C) HangZhou Hikvision System Technology Co., Ltd. 
File ：    FormFaceContrastAlarm.cs 
Developer：    Hikvision
Author：    chenzhixue@hikvision.com
Period：    2019-07-18
Describe：    FormFaceContrastAlarm.cs
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

namespace SDKFaceContrast
{
    public partial class FormFaceContrastAlarm : Form
    {
        private IDeviceTree g_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        private int m_iCurDeviceIndex = -1;
        private int m_iCurChanNo = -1;
        private int m_iCurChanIndex = -1;
        public int m_iStreamType = 0;
        private IDeviceTree.DeviceInfo m_deviceInfo = null;
        private IDeviceTree.ChannelInfo m_channelInfo = null;
        private static SynchronizationContext m_SyncContext = null;
        private int m_lUserID = -1;
        private CHCNetSDK.PREVIEW_IFNO m_strPanelInfo = new CHCNetSDK.PREVIEW_IFNO();
        private List<byte> byBuffer = new List<byte>();
        public delegate bool ProcessLongLinkData(byte[] data, string boundary);
        public delegate bool ProcessSendDate(ref byte[] byBuffer);
        public static int m_iHttpTimeOut = 5000;
        const int BUFFER_SIZE = 3 * 1024 * 1024;
        public ProcessLongLinkData processLongLinkData = null;
        public bool m_bIsAlarmStart = false;
        private Dictionary<long, int> m_dAlarmHanldes = new Dictionary<long, int>();
        private CHCNetSDK.MSGCallBack m_falarmData = null;
        public delegate void UpdateListBoxCallback(string strAlarmTime, string strDevIP, string strAlarmMsg);

        public FormFaceContrastAlarm()
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

        /// <summary>
        /// DeviceTree_SelectedNodeChanged 
        /// </summary>
        void g_deviceTree_SelectedNodeChanged()
        {
            this.GetDevicesInfo();
            this.GetLoginInfo();
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
                struAlarmParam.byAlarmInfoType =Convert.ToByte(cbAlarmRank.SelectedIndex + 1);
                struAlarmParam.byFaceAlarmDetection = 1;
                struAlarmParam.byDeployType = Convert.ToByte(cbAlarmType.SelectedIndex);
                struAlarmParam.byAlarmTypeURL |= Convert.ToByte((cbFaceMatchDataType.SelectedIndex) << 2);
                struAlarmParam.byBrokenNetHttp |= Convert.ToByte((Convert.ToByte(chkFaceMatchBroken.Checked)) << 2);

                long lAlarmHandle = CHCNetSDK.NET_DVR_SetupAlarmChan_V50(Convert.ToInt32(deviceInfo.lLoginID), ref struAlarmParam,IntPtr.Zero,0);
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

        // Parse SDK comparison alarm
        private int iPicCount = 0;
        private void ProcessCommAlarm_FaceMatch(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_VCA_FACESNAP_MATCH_ALARM struFaceMatchAlarm = new CHCNetSDK.NET_VCA_FACESNAP_MATCH_ALARM();
            uint dwSize = (uint)Marshal.SizeOf(struFaceMatchAlarm);
            struFaceMatchAlarm = (CHCNetSDK.NET_VCA_FACESNAP_MATCH_ALARM)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_VCA_FACESNAP_MATCH_ALARM));
            string strAlarmType = "COMM_SNAP_MATCH_ALARM";
            string strIP = pAlarmer.sDeviceIP;
            string strFileSavePath = "C:\\SDK_Alarm_Guard" + "\\" + strIP;
            if (!Directory.Exists(strFileSavePath))
            {
                Directory.CreateDirectory(strFileSavePath);
            }

            string strTimeYear = ((struFaceMatchAlarm.struSnapInfo.dwAbsTime >> 26) + 2000).ToString();
            string strTimeMonth = ((struFaceMatchAlarm.struSnapInfo.dwAbsTime >> 22) & 15).ToString("d2");
            string strTimeDay = ((struFaceMatchAlarm.struSnapInfo.dwAbsTime >> 17) & 31).ToString("d2");
            string strTimeHour = ((struFaceMatchAlarm.struSnapInfo.dwAbsTime >> 12) & 31).ToString("d2");
            string strTimeMinute = ((struFaceMatchAlarm.struSnapInfo.dwAbsTime >> 6) & 63).ToString("d2");
            string strTimeSecond = ((struFaceMatchAlarm.struSnapInfo.dwAbsTime >> 0) & 63).ToString("d2");
            string strTime = strTimeYear + "-" + strTimeMonth + "-" + strTimeDay + "-" + strTimeHour + "-" + strTimeMinute + "-" + strTimeSecond;

            if ((struFaceMatchAlarm.struSnapInfo.dwSnapFacePicLen != 0) && (struFaceMatchAlarm.struSnapInfo.pBuffer1 != IntPtr.Zero))
            {
                iPicCount++;
                string str = strFileSavePath + "\\[" + strTime + "]" + "[" + DateTime.Now.Ticks / 1000 + "[SnapFace]" + ".jpg";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)struFaceMatchAlarm.struSnapInfo.dwSnapFacePicLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struFaceMatchAlarm.struSnapInfo.pBuffer1, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();

                PicShow(iPicCount, str);
            }

            if ((struFaceMatchAlarm.struBlockListInfo.dwBlockListPicLen != 0) && (struFaceMatchAlarm.struBlockListInfo.pBuffer1 != IntPtr.Zero))
            {
                iPicCount++;
                string str = strFileSavePath + "\\[" + strTime + "]" + "[" + DateTime.Now.Ticks / 1000 + "[BlockList]" + ".jpg";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)struFaceMatchAlarm.struBlockListInfo.dwBlockListPicLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struFaceMatchAlarm.struBlockListInfo.pBuffer1, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();

                PicShow(iPicCount, str);
            }

            string stringAlarm = "FaceMatch,DevIP：" + struFaceMatchAlarm.struSnapInfo.struDevInfo.struDevIP.sIpV4 + ",SnapTime："
                + strTime + ",Similarity：" + struFaceMatchAlarm.fSimilarity;

            UpdateClientList(DateTime.Now.ToString(), strAlarmType, stringAlarm);
        }


        private void PicShow(int Count, string strPicPath)
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

        // SDK MsgCallback
        public void MsgCallback(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            switch (lCommand)
            {
                case CHCNetSDK.COMM_SNAP_MATCH_ALARM: 
                        ProcessCommAlarm_FaceMatch(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                        break;
                default:
                        {
                            string strIP = pAlarmer.sDeviceIP;
                            string stringAlarm = "Alarm upload, AlarmType：" + lCommand;

                            UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
                        }
                        break;
            }
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            String sRet = "Start failed!";
            SDK_StartAlarm();
        }
        string strdatatime = "";
        int piccount = 0;

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
