using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace SDKPerimeterPrecaution
{
    public partial class FormPerimeterPrecautionAlarm_SDK : Form
    {

        public int m_iChannel = -1;
        public int m_lUserID = -1;

        private Int32 iListenHandle = -1;

        private Dictionary<long, int> m_dAlarmHanldes = new Dictionary<long, int>();

        private CHCNetSDK.MSGCallBack m_falarmData = null;

        CHCNetSDK.NET_VCA_TRAVERSE_PLANE m_struTraversePlane = new CHCNetSDK.NET_VCA_TRAVERSE_PLANE();
        CHCNetSDK.NET_VCA_AREA m_struVcaArea = new CHCNetSDK.NET_VCA_AREA();
        CHCNetSDK.NET_VCA_INTRUSION m_struIntrusion = new CHCNetSDK.NET_VCA_INTRUSION();

        public delegate void UpdateListBoxCallback(string strAlarmTime, string strDevIP, string strAlarmMsg);

        private IDeviceTree m_deviceTree = PluginsFactory.GetDeviceTreeInstance();

        public FormPerimeterPrecautionAlarm_SDK()
        {
            InitializeComponent();

            //Set the alarm callback function
            if (m_falarmData == null)
            {
                m_falarmData = new CHCNetSDK.MSGCallBack(MsgCallback);
            }
            CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V31(m_falarmData, IntPtr.Zero);
        }

        /** @fn void buttonAlarm_Click(object sender, EventArgs e)
        *  @brief Enable the alarm
        *  @param (in)	object sender    
        *  @param (in)	EventArgs e    
        *  @return void
        */
        private void buttonAlarm_Click(object sender, EventArgs e)
        {
            if (this.m_deviceTree != null && this.m_deviceTree.GetDeviceTreeType() == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
                //IDeviceTree.ChannelInfo channelInfo = m_deviceTree.GetSelectedChannelInfo();
                if (this.m_dAlarmHanldes.ContainsKey(deviceInfo.lLoginID))
                {
                    MessageBox.Show("The Selected Device is in Alarm state!");
                }
                else
                {
                    CHCNetSDK.NET_DVR_SETUPALARM_PARAM struAlarmParam = new CHCNetSDK.NET_DVR_SETUPALARM_PARAM();

                    struAlarmParam.dwSize = (uint)Marshal.SizeOf(struAlarmParam);
                    struAlarmParam.byLevel = 1; //0- level 1,1- level 2
                    struAlarmParam.byAlarmInfoType = 1;//Intelligent traffic equipment effective, new type of alarm information
                    struAlarmParam.byFaceAlarmDetection = 1;//1- face detection

                    long lAlarmHandle = CHCNetSDK.NET_DVR_SetupAlarmChan_V41(Convert.ToInt32(deviceInfo.lLoginID), ref struAlarmParam);
                    if (lAlarmHandle < 0)
                    {
                        int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                        string strErr = "Arming failure, error code:" + iLastErr + "." + CHCNetSDK.NET_DVR_GetErrorMsg(ref iLastErr);
                        MessageBox.Show(strErr);
                    }
                    else
                    {
                        this.m_dAlarmHanldes.Add(deviceInfo.lLoginID, Convert.ToInt32(lAlarmHandle));
                        // buttonAlarm.Enabled = false;
                        //buttonRemoveAlarm.Enabled = true;
                        MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "SDKAlarm", "Arming success!");
                    }

                }
            }
        }

        /** @fn void buttonRemoveAlarm_Click(object sender, EventArgs e)
        *  @brief Close the alarm
        *  @param (in)	object sender    
        *  @param (in)	EventArgs e    
        *  @return void
        */
        private void buttonRemoveAlarm_Click(object sender, EventArgs e)
        {
            if (this.m_deviceTree != null && this.m_deviceTree.GetDeviceTreeType() == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
                //IDeviceTree.ChannelInfo channelInfo = m_deviceTree.GetSelectedChannelInfo();
                if (this.m_dAlarmHanldes.ContainsKey(deviceInfo.lLoginID))
                {
                    if (!CHCNetSDK.NET_DVR_CloseAlarmChan_V30(this.m_dAlarmHanldes[deviceInfo.lLoginID]))
                    {
                        int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                        string strErr = "Disarming failed, error code:" + iLastErr + "." + CHCNetSDK.NET_DVR_GetErrorMsg(ref iLastErr);
                        MessageBox.Show(strErr);
                    }
                    else
                    {
                        this.m_dAlarmHanldes.Remove(deviceInfo.lLoginID);
                        MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "SDKAlarm", "Disarming success!");
                    }

                }
                else
                {
                    MessageBox.Show("The Selected Device is not in Alarm state!");
                }
            }
        }

        /** @fn void buttonListen_Click(object sender, EventArgs e)
         *  @brief Turn on or off listening
         *  @param (in)	object sender    
         *  @param (in)	EventArgs e    
         *  @return void
         */
        private void buttonListen_Click(object sender, EventArgs e)
        {
            if (this.m_deviceTree != null && this.m_deviceTree.GetDeviceTreeType() == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                if (iListenHandle >= 0)
                {
                    if (!CHCNetSDK.NET_DVR_StopListen_V30(iListenHandle))
                    {
                        int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                        string strErr = "Stop listening failed, error code:" + iLastErr + "." + CHCNetSDK.NET_DVR_GetErrorMsg(ref iLastErr);
                        MessageBox.Show(strErr);
                    }
                    else
                    {
                        MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "SDKAlarm", "Stop listening successfully!");
                        iListenHandle = -1;
                        buttonListen.Text = "Start Listen";
                    }
                }
                else
                {
                    IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
                    IDeviceTree.ChannelInfo channelInfo = m_deviceTree.GetSelectedChannelInfo();

                    string sLocalIP = textBoxListenIP.Text;
                    ushort wLocalPort = ushort.Parse(textBoxListenPort.Text);

                    if (m_falarmData == null)
                    {
                        m_falarmData = new CHCNetSDK.MSGCallBack(MsgCallback);
                    }
                    iListenHandle = CHCNetSDK.NET_DVR_StartListen_V30(sLocalIP, wLocalPort, m_falarmData, IntPtr.Zero);
                    if (iListenHandle < 0)
                    {
                        int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                        string strErr = "Start listening failed, error code:" + iLastErr + "." + CHCNetSDK.NET_DVR_GetErrorMsg(ref iLastErr);
                        MessageBox.Show(strErr);
                    }
                    else
                    {
                        MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "SDKAlarm", "Start listening successfully!");
                        buttonListen.Text = "Stop Listen";
                    }
                }
            }
        }

        public void MsgCallback(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //The types of alarm information received can be determined by lCommand. Different lcommands correspond to different pAlarmInfo contents
            AlarmMessageHandle(lCommand, ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
        }

        public void AlarmMessageHandle(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //The types of alarm information received can be determined by lCommand. Different lcommands correspond to different pAlarmInfo contents
            switch(lCommand)
            {
                case CHCNetSDK.COMM_ALARM_RULE://Behavior analysis and alarm information such as entering and leaving area, invasion, wandering, people gathering, etc
                    ProcessCommAlarm_RULE(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                default:
                    {
                        //IP address of alarm device
                        string strIP = pAlarmer.sDeviceIP;

                        //Alarm information type
                        string stringAlarm = "Alarm upload, information type:" + lCommand;

                        //The main thread that creates the control updates the list of information directly 
                        UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
                    }
                    break;
            }
        }

        /// <summary>
        /// _ and alarm information such as entering and leaving area, invasion, wandering, people gathering, etc
        /// </summary>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        private void
            ProcessCommAlarm_RULE(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_VCA_RULE_ALARM struRuleAlarmInfo = new CHCNetSDK.NET_VCA_RULE_ALARM();
            struRuleAlarmInfo = (CHCNetSDK.NET_VCA_RULE_ALARM)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_VCA_RULE_ALARM));

            //报警信息
            string stringAlarm = "";
            uint dwSize = (uint)Marshal.SizeOf(struRuleAlarmInfo.struRuleInfo.uEventParam);

            switch (struRuleAlarmInfo.struRuleInfo.wEventTypeEx)
            {
                case (ushort)CHCNetSDK.VCA_RULE_EVENT_TYPE_EX.ENUM_VCA_EVENT_TRAVERSE_PLANE:
                    IntPtr ptrTraverseInfo = Marshal.AllocHGlobal((Int32)dwSize);
                    Marshal.StructureToPtr(struRuleAlarmInfo.struRuleInfo.uEventParam, ptrTraverseInfo, false);
                    m_struTraversePlane = (CHCNetSDK.NET_VCA_TRAVERSE_PLANE)Marshal.PtrToStructure(ptrTraverseInfo, typeof(CHCNetSDK.NET_VCA_TRAVERSE_PLANE));
                    stringAlarm = "Traverse plane, target ID:" + struRuleAlarmInfo.struTargetInfo.dwID;
                    break;
                case (ushort)CHCNetSDK.VCA_RULE_EVENT_TYPE_EX.ENUM_VCA_EVENT_ENTER_AREA:
                    IntPtr ptrEnterInfo = Marshal.AllocHGlobal((Int32)dwSize);
                    Marshal.StructureToPtr(struRuleAlarmInfo.struRuleInfo.uEventParam, ptrEnterInfo, false);
                    m_struVcaArea = (CHCNetSDK.NET_VCA_AREA)Marshal.PtrToStructure(ptrEnterInfo, typeof(CHCNetSDK.NET_VCA_AREA));
                    stringAlarm = "Enter area, target ID:" + struRuleAlarmInfo.struTargetInfo.dwID;
                    break;
                case (ushort)CHCNetSDK.VCA_RULE_EVENT_TYPE_EX.ENUM_VCA_EVENT_EXIT_AREA:
                    IntPtr ptrExitInfo = Marshal.AllocHGlobal((Int32)dwSize);
                    Marshal.StructureToPtr(struRuleAlarmInfo.struRuleInfo.uEventParam, ptrExitInfo, false);
                    m_struVcaArea = (CHCNetSDK.NET_VCA_AREA)Marshal.PtrToStructure(ptrExitInfo, typeof(CHCNetSDK.NET_VCA_AREA));
                    stringAlarm = "Exit area, target ID:" + struRuleAlarmInfo.struTargetInfo.dwID;
                    break;
                case (ushort)CHCNetSDK.VCA_RULE_EVENT_TYPE_EX.ENUM_VCA_EVENT_INTRUSION:
                    IntPtr ptrIntrusionInfo = Marshal.AllocHGlobal((Int32)dwSize);
                    Marshal.StructureToPtr(struRuleAlarmInfo.struRuleInfo.uEventParam, ptrIntrusionInfo, false);
                    m_struIntrusion = (CHCNetSDK.NET_VCA_INTRUSION)Marshal.PtrToStructure(ptrIntrusionInfo, typeof(CHCNetSDK.NET_VCA_INTRUSION));

                    int i = 0;
                    string strRegion = "";
                    for (i = 0; i < m_struIntrusion.struRegion.dwPointNum; i++)
                    {
                        strRegion = strRegion + "(" + m_struIntrusion.struRegion.struPos[i].fX + "," + m_struIntrusion.struRegion.struPos[i].fY + ")";
                    }
                    stringAlarm = "Intrusion, target ID:" + struRuleAlarmInfo.struTargetInfo.dwID + ", regional scope:" + strRegion;
                    break;
                default:
                    stringAlarm = "Other _ alarm, target ID:" + struRuleAlarmInfo.struTargetInfo.dwID;
                    break;
            }

            //Save alarm picture
            if (struRuleAlarmInfo.dwPicDataLen > 0)
            {
                Random ran = new Random();
                int indexNum = ran.Next(1, 1000);
                string strfn = "_ alarm capture_" + indexNum + ".jpg";
                FileStream fs = new FileStream(strfn, FileMode.Create);
                int iLen = (int)struRuleAlarmInfo.dwPicDataLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struRuleAlarmInfo.pImage, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();
            }

            //Alarm time: time, month, day, hour, minute and second
            string strTimeYear = ((struRuleAlarmInfo.dwAbsTime >> 26) + 2000).ToString();
            string strTimeMonth = ((struRuleAlarmInfo.dwAbsTime >> 22) & 15).ToString("d2");
            string strTimeDay = ((struRuleAlarmInfo.dwAbsTime >> 17) & 31).ToString("d2");
            string strTimeHour = ((struRuleAlarmInfo.dwAbsTime >> 12) & 31).ToString("d2");
            string strTimeMinute = ((struRuleAlarmInfo.dwAbsTime >> 6) & 63).ToString("d2");
            string strTimeSecond = ((struRuleAlarmInfo.dwAbsTime >> 0) & 63).ToString("d2");
            string strTime = strTimeYear + "-" + strTimeMonth + "-" + strTimeDay + " " + strTimeHour + ":" + strTimeMinute + ":" + strTimeSecond;

            //IP address of alarm device
            string strIP = struRuleAlarmInfo.struDevInfo.struDevIP.sIpV4;

            //The main thread that creates the control updates the list of information directly
            UpdateClientList(strTime, strIP, stringAlarm);
        }

        /// <summary>
        /// Commission events and update alarm information in real time
        /// </summary>
        /// <param name="strAlarmTime"></param>
        /// <param name="strDevIP"></param>
        /// <param name="strAlarmMsg"></param>
        public void UpdateClientList(string strAlarmTime, string strDevIP, string strAlarmMsg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new UpdateListBoxCallback(UpdateClientList), strAlarmTime, strDevIP, strAlarmMsg);
            }
            else
            {
                if (listViewAlarmInfoShow.Items.Count > 200)
                {
                    listViewAlarmInfoShow.Items.RemoveAt(0);
                }
                //Added alarm information to the list
                listViewAlarmInfoShow.Items.Add(new ListViewItem(new string[] { strAlarmTime, strDevIP, strAlarmMsg }));
            }
        }
    }
}
