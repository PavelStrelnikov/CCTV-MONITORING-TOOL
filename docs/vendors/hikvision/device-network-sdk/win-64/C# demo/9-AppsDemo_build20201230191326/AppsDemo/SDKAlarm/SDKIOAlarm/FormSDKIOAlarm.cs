using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDKAlarm
{
    public partial class FormSDKIOAlarm : PluginsControl
    {
        private Int32 iListenHandle = -1;

        private Dictionary<long, int> m_dAlarmHanldes = new Dictionary<long, int>();
        private int iDeviceNumber = 0; //添加设备个数


        private CHCNetSDK.MSGCallBack m_falarmData = null;

        public delegate void UpdateListBoxCallback(string strAlarmTime, string strDevIP, string strAlarmMsg, bool bUpdate, int iAlarmInputNumber, int lUserID);

        public FormSDKIOAlarm()
        {
            InitializeComponent();
            if (m_falarmData == null)
            {
                m_falarmData = new CHCNetSDK.MSGCallBack(MsgCallback);
            }
            CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V31(m_falarmData, IntPtr.Zero);

            InitStartIPAlarmInNo(-1);//初始化使用设备树的UserID
            InitIPAlarmInInfo(-1);
            InitIPAccessCfgV40(-1);
        }

        private IDeviceTree m_deviceTree = PluginsFactory.GetDeviceTreeInstance();

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
                    struAlarmParam.byLevel = 0; //0- 一级布防,1- 二级布防
                    struAlarmParam.byAlarmInfoType = 0;//智能交通设备有效，新报警信息类型
                    struAlarmParam.byFaceAlarmDetection = 1;//1-人脸侦测

                    long lAlarmHandle = CHCNetSDK.NET_DVR_SetupAlarmChan_V41(Convert.ToInt32(deviceInfo.lLoginID), ref struAlarmParam);
                    if (lAlarmHandle < 0)
                    {
                        int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                        string strErr = "布防失败，错误码：" + iLastErr + "." + CHCNetSDK.NET_DVR_GetErrorMsg(ref iLastErr); //布防失败，输出错误码
                        MessageBox.Show(strErr);
                    }
                    else
                    {
                        this.m_dAlarmHanldes.Add(deviceInfo.lLoginID, Convert.ToInt32(lAlarmHandle));
                        // buttonAlarm.Enabled = false;
                        //buttonRemoveAlarm.Enabled = true;
                        MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "SDKAlarm", "布防成功！");
                    }

                }
            }
        }

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
                        string strErr = "撤防失败，错误码：" + iLastErr + "." + CHCNetSDK.NET_DVR_GetErrorMsg(ref iLastErr); //撤防失败，输出错误码
                        MessageBox.Show(strErr);
                    }
                    else
                    {
                        this.m_dAlarmHanldes.Remove(deviceInfo.lLoginID);
                        MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "SDKAlarm", "撤防成功！");
                    }

                }
                else
                {
                    MessageBox.Show("The Selected Device is not in Alarm state!");
                }
            }
        }

        public void MsgCallback(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //通过lCommand来判断接收到的报警信息类型，不同的lCommand对应不同的pAlarmInfo内容
            AlarmMessageHandle(lCommand, ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
        }
        public void AlarmMessageHandle(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //通过lCommand来判断接收到的报警信息类型，不同的lCommand对应不同的pAlarmInfo内容
            switch (lCommand)
            {
                case CHCNetSDK.COMM_ALARM_V30://移动侦测、视频丢失、遮挡、IO信号量等报警信息
                    ProcessCommAlarm_V30(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                default:
                    {
                        //报警设备IP地址
                        string strIP = pAlarmer.sDeviceIP;

                        //报警信息类型
                        string stringAlarm = "报警上传，信息类型：" + lCommand;

                        //创建该控件的主线程直接更新信息列表 
                        UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm, false, 0, pAlarmer.lUserID);
                    }
                    break;
            }
        }

        /// <summary>
        /// 接收V30报警
        /// </summary>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        /// <returns></returns>
        private void ProcessCommAlarm_V30(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {

            CHCNetSDK.NET_DVR_ALARMINFO_V30 struAlarmInfoV30 = new CHCNetSDK.NET_DVR_ALARMINFO_V30();

            struAlarmInfoV30 = (CHCNetSDK.NET_DVR_ALARMINFO_V30)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_DVR_ALARMINFO_V30));

            string strIP = pAlarmer.sDeviceIP;
            string stringAlarm = "";
            int i;

            switch (struAlarmInfoV30.dwAlarmType)
            {
                case 0:
                    //设备返回的报警输入口号需要加1才得到实际输入口号
                    stringAlarm = "信号量报警，报警输入口：" + (struAlarmInfoV30.dwAlarmInputNumber + 1) + "，触发录像通道：";
                    for (i = 0; i < CHCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byAlarmRelateChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + "\\";
                        }
                    }
                    break;
                case 1:
                    stringAlarm = "硬盘满，报警硬盘号：";
                    for (i = 0; i < CHCNetSDK.MAX_DISKNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byDiskNumber[i] == 1)
                        {
                            stringAlarm += (i + 1) + " ";
                        }
                    }
                    break;
                case 2:
                    stringAlarm = "信号丢失，报警通道：";
                    for (i = 0; i < CHCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 3:
                    stringAlarm = "移动侦测，报警通道：";
                    for (i = 0; i < CHCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 4:
                    stringAlarm = "硬盘未格式化，报警硬盘号：";
                    for (i = 0; i < CHCNetSDK.MAX_DISKNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byDiskNumber[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 5:
                    stringAlarm = "读写硬盘出错，报警硬盘号：";
                    for (i = 0; i < CHCNetSDK.MAX_DISKNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byDiskNumber[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 6:
                    stringAlarm = "遮挡报警，报警通道：";
                    for (i = 0; i < CHCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 7:
                    stringAlarm = "制式不匹配，报警通道";
                    for (i = 0; i < CHCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 8:
                    stringAlarm = "非法访问";
                    break;
                case 9:
                    stringAlarm = "视频信号异常，报警通道";
                    for (i = 0; i < CHCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 10:
                    stringAlarm = "录像/抓图异常，报警通道";
                    for (i = 0; i < CHCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 11:
                    stringAlarm = "智能场景变化，报警通道";
                    for (i = 0; i < CHCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 12:
                    stringAlarm = "阵列异常";
                    break;
                case 13:
                    stringAlarm = "前端/录像分辨率不匹配，报警通道";
                    for (i = 0; i < CHCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 15:
                    stringAlarm = "智能侦测，报警通道";
                    for (i = 0; i < CHCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                default:
                    stringAlarm = "其他未知报警信息";
                    break;
            }

            //创建该控件的主线程直接更新信息列表 
            if (struAlarmInfoV30.dwAlarmType == 0)
            {
                UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm, true, struAlarmInfoV30.dwAlarmInputNumber + 1, pAlarmer.lUserID);//当前约定，设备返回的报警输入口号需要加1才得到实际输入口号
            }
            else
            {
                UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm, false, 0, pAlarmer.lUserID);
            }

        }

        /// <summary>
        /// 初始化起始IP报警输入列表
        /// </summary>
        /// <param name="iAlarmUserID"></param>
        /// <returns></returns>
        public void InitStartIPAlarmInNo(int iAlarmUserID)
        {
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            IDeviceTree.EDeviceTreeType deviceTreeType = m_deviceTree.GetDeviceTreeType();
            if (deviceInfo == null || deviceTreeType != IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                return;
            }
            IntPtr ptrDevCfg = IntPtr.Zero;

            try
            {
                CHCNetSDK.NET_DVR_DEVICECFG_V40 struDevCfg = new CHCNetSDK.NET_DVR_DEVICECFG_V40();
                struDevCfg.sDVRName = new byte[CHCNetSDK.NAME_LEN];
                struDevCfg.sSerialNumber = new byte[CHCNetSDK.SERIALNO_LEN];
                struDevCfg.byDevTypeName = new byte[CHCNetSDK.DEV_TYPE_NAME_LEN];

                uint dwSize = (uint)Marshal.SizeOf(struDevCfg);
                ptrDevCfg = Marshal.AllocHGlobal((int)dwSize);
                Marshal.StructureToPtr(struDevCfg, ptrDevCfg, false);
                uint dwReturned = 0;
                int lUserID = (int)deviceInfo.lLoginID;
                if (iAlarmUserID >= 0)
                {
                    lUserID = iAlarmUserID;//报警返回时，需要使用报警设备对应的userID
                }

                if (!CHCNetSDK.NET_DVR_GetDVRConfig(lUserID, CHCNetSDK.NET_DVR_GET_DEVICECFG_V40, 0, ptrDevCfg, dwSize, ref dwReturned))
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GET_DEVICECFG_V40", "Get fail");
                }
                else
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GET_DEVICECFG_V40", "Get succ");
                    struDevCfg = (CHCNetSDK.NET_DVR_DEVICECFG_V40)Marshal.PtrToStructure(ptrDevCfg, typeof(CHCNetSDK.NET_DVR_DEVICECFG_V40));
                    if (struDevCfg.byStartIPAlarmInNo == 0)//如果设备返回的起始IP报警输入号为0，直接转为32
                    {
                        m_textBoxIPAlarmIn.Text = "32";
                    }
                    else
                    {
                        m_textBoxIPAlarmIn.Text = struDevCfg.byStartIPAlarmInNo.ToString();
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptrDevCfg);
            }
        }

        /// <summary>
        /// 初始化IP报警输入列表
        /// </summary>
        /// <param name="iAlarmUserID"></param>
        /// <returns></returns>
        public void InitIPAlarmInInfo(int iAlarmUserID)
        {
            m_listViewAlarmIn.Items.Clear();
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            IDeviceTree.EDeviceTreeType deviceTreeType = m_deviceTree.GetDeviceTreeType();
            if (deviceInfo == null || deviceTreeType != IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                return;
            }

            IntPtr ptrIpAlarmInCfg = IntPtr.Zero;

            try
            {
                CHCNetSDK.NET_DVR_IPALARMINCFG_V40 struIpAlarmInCfg = new CHCNetSDK.NET_DVR_IPALARMINCFG_V40();
                struIpAlarmInCfg.struIPAlarmInInfo = new CHCNetSDK.NET_DVR_IPALARMININFO_V40[CHCNetSDK.MAX_IP_ALARMIN_V40];

                uint dwSize = (uint)Marshal.SizeOf(struIpAlarmInCfg);
                ptrIpAlarmInCfg = Marshal.AllocHGlobal((int)dwSize);
                Marshal.StructureToPtr(struIpAlarmInCfg, ptrIpAlarmInCfg, false);
                uint dwReturned = 0;
                int lUserID = (int)deviceInfo.lLoginID;
                if (iAlarmUserID >= 0)
                {
                    lUserID = iAlarmUserID;//报警返回时，需要使用报警设备对应的userID
                }

                if (!CHCNetSDK.NET_DVR_GetDVRConfig(lUserID, CHCNetSDK.NET_DVR_GET_IPALARMINCFG_V40, 0, ptrIpAlarmInCfg, dwSize, ref dwReturned))
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GET_IPALARMINCFG_V40", "Get fail");
                }
                else
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GET_IPALARMINCFG_V40", "Get succ");
                    struIpAlarmInCfg = (CHCNetSDK.NET_DVR_IPALARMINCFG_V40)Marshal.PtrToStructure(ptrIpAlarmInCfg, typeof(CHCNetSDK.NET_DVR_IPALARMINCFG_V40));
                    for (int i = 0; i < struIpAlarmInCfg.dwCurIPAlarmInNum && i < CHCNetSDK.MAX_IP_ALARMIN_V40; i++)
                    {
                        //struIpAlarmInCfg.struIPAlarmInInfo[i].dwAlarmIn 这个字段实际没有用，通过struDevCfg.byStartIPAlarmInNo累加即可
                        m_listViewAlarmIn.Items.Add(new ListViewItem(new string[] { i.ToString(), struIpAlarmInCfg.struIPAlarmInInfo[i].dwIPID.ToString(), 
                        (i + int.Parse(m_textBoxIPAlarmIn.Text) + 1).ToString() }));
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptrIpAlarmInCfg);
            }

        }

        /// <summary>
        /// 初始化IP通道列表
        /// </summary>
        /// <param name="iAlarmUserID"></param>
        /// <returns></returns>
        public void InitIPAccessCfgV40(int iAlarmUserID)
        {
            m_listViewChannel.Items.Clear();
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            IDeviceTree.EDeviceTreeType deviceTreeType = m_deviceTree.GetDeviceTreeType();
            if (deviceInfo == null || deviceTreeType != IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                return;
            }

            int dwSize, iGroupNO = 0;
            uint dwReturned = 0;
            int lUserID = (int)deviceInfo.lLoginID;
            if (iAlarmUserID >= 0)
            {
                lUserID = iAlarmUserID;//报警返回时，需要使用报警设备对应的userID
            }
            //获取设备通道分组号，64个通道为一组，根据设备最大通道数计算得出
            iGroupNO = deviceInfo.iDeviceChanNum / CHCNetSDK.MAX_CHANNUM_V30;
            if (deviceInfo.iDeviceChanNum % CHCNetSDK.MAX_CHANNUM_V30 != 0)
            {
                iGroupNO = iGroupNO + 1;
            }


            IntPtr ptrIPAccessCfgV40 = IntPtr.Zero;

            for (int j = 0; j < iGroupNO; j++)
            {

                try
                {
                    CHCNetSDK.NET_DVR_IPPARACFG_V40 struIPAccessCfgV40 = new CHCNetSDK.NET_DVR_IPPARACFG_V40();
                    dwSize = Marshal.SizeOf(struIPAccessCfgV40);
                    ptrIPAccessCfgV40 = Marshal.AllocHGlobal(dwSize);
                    Marshal.StructureToPtr(struIPAccessCfgV40, ptrIPAccessCfgV40, false);
                    if (!CHCNetSDK.NET_DVR_GetDVRConfig(lUserID, CHCNetSDK.NET_DVR_GET_IPPARACFG_V40, j, ptrIPAccessCfgV40, (uint)dwSize, ref dwReturned))
                    {	///device no support ip access
                        uint iErrCode = CHCNetSDK.NET_DVR_GetLastError();
                        MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GET_IPPARACFG_V40", "Get fail");
                    }
                    else
                    {
                        MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GET_IPPARACFG_V40", "Get succ");
                        int iIPID = 0;
                        struIPAccessCfgV40 = (CHCNetSDK.NET_DVR_IPPARACFG_V40)Marshal.PtrToStructure(ptrIPAccessCfgV40, typeof(CHCNetSDK.NET_DVR_IPPARACFG_V40));
                        for (int i = 0; i < CHCNetSDK.MAX_CHANNUM_V30; i++)
                        {
                            if (j == iGroupNO - 1 && i >= struIPAccessCfgV40.dwDChanNum % CHCNetSDK.MAX_CHANNUM_V30)
                            {
                                break;
                            }
                            switch (struIPAccessCfgV40.struStreamMode[i].byGetStreamType)
                            {
                                case 0:
                                    iIPID = struIPAccessCfgV40.struStreamMode[i].uGetStream.struChanInfo.byIPID + struIPAccessCfgV40.struStreamMode[i].uGetStream.struChanInfo.byIPIDHigh * 256;
                                    break;
                                default:
                                    break;
                            }
                            m_listViewChannel.Items.Add(new ListViewItem(new string[] { i.ToString(), iIPID.ToString(), (i + (iGroupNO - 1) * 64 + struIPAccessCfgV40.dwStartDChan).ToString() }));
                        }
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(ptrIPAccessCfgV40);
                }

            }
        }

        /// <summary>
        /// 更新界面列表
        /// </summary>
        /// <param name="strAlarmTime"></param>
        /// <param name="strDevIP"></param>
        /// <param name="strAlarmMsg"></param>
        /// <param name="bUpdate"></param>
        /// <param name="iAlarmInputNumber"></param>
        /// <param name="lUserID"></param>
        /// <returns></returns>
        public void UpdateClientList(string strAlarmTime, string strDevIP, string strAlarmMsg, bool bUpdate, int iAlarmInputNumber, int lUserID)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new UpdateListBoxCallback(UpdateClientList), strAlarmTime, strDevIP, strAlarmMsg, bUpdate, iAlarmInputNumber, lUserID);
            }
            else
            {
                if (m_listViewAlarmInfo.Items.Count > 200)
                {
                    m_listViewAlarmInfo.Items.RemoveAt(0);
                }
                //列表新增报警信息
                m_listViewAlarmInfo.Items.Add(new ListViewItem(new string[] { strAlarmTime, strDevIP, strAlarmMsg }));

                if (bUpdate)
                {
                    InitStartIPAlarmInNo(lUserID);
                    InitIPAlarmInInfo(lUserID);
                    InitIPAccessCfgV40(lUserID);
                    FindIPChannel(iAlarmInputNumber);
                }
            }
        }

        /// <summary>
        /// 根据报警输入号找到IP通道
        /// </summary>
        /// <param name="iAlarmInputNumber"></param>
        /// <returns></returns>
        public void FindIPChannel(int iAlarmInputNumber)
        {
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();

            string  strIPID = null;

            ListViewItem tempItem = null;

            foreach (ListViewItem item in m_listViewAlarmIn.Items)
            {
                if (item.SubItems[2].Text == iAlarmInputNumber.ToString())
                {
                    tempItem = item;
                    break;
                }
            }

            if (tempItem != null)
            {
                this.m_listViewAlarmIn.TopItem = tempItem;  //定位到该项
                tempItem.ForeColor = Color.Red;
                strIPID = tempItem.SubItems[1].Text;

                ListViewItem tempItem2 = null;

                foreach (ListViewItem item in m_listViewChannel.Items)
                {
                    if (item.SubItems[1].Text == strIPID)
                    {
                        tempItem2 = item;
                        break;
                    }
                }

                if (tempItem2 != null)
                {
                    this.m_listViewChannel.TopItem = tempItem2;  //定位到该项
                    tempItem2.ForeColor = Color.Red;
                }
            }
        }
    }
}
