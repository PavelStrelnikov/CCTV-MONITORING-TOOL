using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SDKAlarm
{
    public partial class FormSDKAlarm : PluginsControl
    {

        private Int32 iListenHandle = -1;
        
        //private Int32[] m_lAlarmHandle = new Int32[200];
        private Dictionary<long, int> m_dAlarmHanldes = new Dictionary<long, int>();
        private int iDeviceNumber = 0; //添加设备个数


        private CHCNetSDK.MSGCallBack m_falarmData = null;
        private CHCNetSDK.MSGCallBack_V31 m_falarmData_V31 = null;

        public delegate void UpdateListBoxCallback(string strAlarmTime, string strDevIP, string strAlarmMsg);


        CHCNetSDK.NET_VCA_TRAVERSE_PLANE m_struTraversePlane = new CHCNetSDK.NET_VCA_TRAVERSE_PLANE();
        CHCNetSDK.NET_VCA_AREA m_struVcaArea = new CHCNetSDK.NET_VCA_AREA();
        CHCNetSDK.NET_VCA_INTRUSION m_struIntrusion = new CHCNetSDK.NET_VCA_INTRUSION();
        CHCNetSDK.UNION_STATFRAME m_struStatFrame = new CHCNetSDK.UNION_STATFRAME();
        CHCNetSDK.UNION_STATTIME m_struStatTime = new CHCNetSDK.UNION_STATTIME();

        public FormSDKAlarm()
        {
            InitializeComponent();

            //设置报警回调函数
            if (m_falarmData == null)
            {
                m_falarmData = new CHCNetSDK.MSGCallBack(MsgCallback);
            }
            CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V31(m_falarmData, IntPtr.Zero);


        }
        private IDeviceTree m_deviceTree = PluginsFactory.GetDeviceTreeInstance();


        /** @fn void buttonListen_Click(object sender, EventArgs e)
         *  @brief 开启或者关闭监听
         *  @param (in)	object sender    
         *  @param (in)	EventArgs e    
         *  @return void
         */
        private void buttonListen_Click(object sender, EventArgs e)
        {
            if (this.m_deviceTree != null && this.m_deviceTree.GetDeviceTreeType() == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                if(iListenHandle >= 0)
                {
                    if (!CHCNetSDK.NET_DVR_StopListen_V30(iListenHandle))
                    {
                        int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                        string strErr = "停止监听失败，错误码：" + iLastErr+"."+CHCNetSDK.NET_DVR_GetErrorMsg(ref iLastErr); //撤防失败，输出错误码
                        MessageBox.Show(strErr);
                    }
                    else
                    {
                        //MessageBox.Show("停止监听！");
                        MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "SDKAlarm", "停止监听成功！");
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
                        string strErr = "启动监听失败，错误码：" + iLastErr + "." + CHCNetSDK.NET_DVR_GetErrorMsg(ref iLastErr); //启动监听失败，输出错误码
                        MessageBox.Show(strErr);
                    }
                    else
                    {
                        MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "SDKAlarm", "启动监听成功！");
                        buttonListen.Text = "Stop Listen";
                    }
                }
            }
        }

        /** @fn void buttonAlarm_Click(object sender, EventArgs e)
         *  @brief 启用报警
         *  @param (in)	object sender    
         *  @param (in)	EventArgs e    
         *  @return void
异常行为检测         */
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
                    struAlarmParam.byLevel = 1; //0- 一级布防,1- 二级布防
                    struAlarmParam.byAlarmInfoType = 1;//智能交通设备有效，新报警信息类型
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

        /** @fn void buttonRemoveAlarm_Click(object sender, EventArgs e)
        *  @brief 关闭报警
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
                if(this.m_dAlarmHanldes.ContainsKey(deviceInfo.lLoginID) )
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

        /** @fn void FormSDKAlarm_Shown(object sender, EventArgs e)
        *  @brief 显示报警界面
        *  @param (in)	object sender    
        *  @param (in)	EventArgs e    
        *  @return void
        */
        private void FormSDKAlarm_Shown(object sender, EventArgs e)
        {
            if (this.m_deviceTree == null || this.m_deviceTree.GetDeviceTreeType() != IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKAlarm", "just for SDK DeviceTree");
                this.Close();
            }
        }

        public void MsgCallback(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //通过lCommand来判断接收到的报警信息类型，不同的lCommand对应不同的pAlarmInfo内容
            AlarmMessageHandle(lCommand, ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
        }
        public bool MsgCallback_V31(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //通过lCommand来判断接收到的报警信息类型，不同的lCommand对应不同的pAlarmInfo内容
            AlarmMessageHandle(lCommand, ref pAlarmer, pAlarmInfo, dwBufLen, pUser);

            return true; //回调函数需要有返回，表示正常接收到数据
        }
        public void AlarmMessageHandle(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //通过lCommand来判断接收到的报警信息类型，不同的lCommand对应不同的pAlarmInfo内容
            switch (lCommand)
            {
                case CHCNetSDK.COMM_ALARM: //(DS-8000老设备)移动侦测、视频丢失、遮挡、IO信号量等报警信息
                    ProcessCommAlarm(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case CHCNetSDK.COMM_ALARM_V30://移动侦测、视频丢失、遮挡、IO信号量等报警信息
                    ProcessCommAlarm_V30(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case CHCNetSDK.COMM_ALARM_RULE://进出区域、入侵、徘徊、人员聚集等行为分析报警信息
                    ProcessCommAlarm_RULE(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case CHCNetSDK.COMM_UPLOAD_PLATE_RESULT://交通抓拍结果上传(老报警信息类型)
                    ProcessCommAlarm_Plate(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case CHCNetSDK.COMM_ITS_PLATE_RESULT://交通抓拍结果上传(新报警信息类型)
                    ProcessCommAlarm_ITSPlate(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                //case CHCNetSDK.COMM_ALARM_PDC://客流量统计报警信息
                //    ProcessCommAlarm_PDC(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                //    break;
                case CHCNetSDK.COMM_ITS_PARK_VEHICLE://客流量统计报警信息
                    ProcessCommAlarm_PARK(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case CHCNetSDK.COMM_DIAGNOSIS_UPLOAD://VQD报警信息
                    ProcessCommAlarm_VQD(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case CHCNetSDK.COMM_UPLOAD_FACESNAP_RESULT://人脸抓拍结果信息
                    ProcessCommAlarm_FaceSnap(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case CHCNetSDK.COMM_SNAP_MATCH_ALARM://人脸比对结果信息
                    ProcessCommAlarm_FaceMatch(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case CHCNetSDK.COMM_ALARM_FACE_DETECTION://人脸侦测报警信息
                    ProcessCommAlarm_FaceDetect(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case CHCNetSDK.COMM_ALARMHOST_CID_ALARM://报警主机CID报警上传
                    ProcessCommAlarm_CIDAlarm(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case CHCNetSDK.COMM_ALARM_ACS://门禁主机报警上传
                    ProcessCommAlarm_AcsAlarm(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case CHCNetSDK.COMM_ID_INFO_ALARM://身份证刷卡信息上传
                    ProcessCommAlarm_IDInfoAlarm(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                default:
                    {
                        //报警设备IP地址
                        string strIP = pAlarmer.sDeviceIP;

                        //报警信息类型
                        string stringAlarm = "报警上传，信息类型：" + lCommand;

                        //创建该控件的主线程直接更新信息列表 
                        UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
                    }
                    break;
            }
        }

        /// <summary>
        /// (DS-8000老设备)移动侦测、视频丢失、遮挡、IO信号量等报警信息
        /// </summary>
        /// <param name="pAl异常行为检测r"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        public void ProcessCommAlarm(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_DVR_ALARMINFO struAlarmInfo = new CHCNetSDK.NET_DVR_ALARMINFO();

            struAlarmInfo = (CHCNetSDK.NET_DVR_ALARMINFO)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_DVR_ALARMINFO));

            string strIP = pAlarmer.sDeviceIP;
            string stringAlarm = "";
            int i = 0;

            switch (struAlarmInfo.dwAlarmType)
            {
                case 0:
                    stringAlarm = "信号量报警，报警报警输入口：" + struAlarmInfo.dwAlarmInputNumber + "，触发录像通道：";
                    for (i = 0; i < CHCNetSDK.MAX_CHANNUM; i++)
                    {
                        if (struAlarmInfo.dwAlarmRelateChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 1:
                    stringAlarm = "硬盘异常行为检测硬盘号：";
                    for (i = 0; i < CHCNetSDK.MAX_DISKNUM; i++)
                    {
                        if (struAlarmInfo.dwDiskNumber[i] == 1)
异常行为检测                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 2:
                    stringAlarm = "信号丢失，报警通道：";
                    for (i = 0; i < CHCNetSDK.MAX_CHANNUM; i++)
                    {
                        if (struAlarmInfo.dwChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 3:
                    stringAlarm = "移动侦测，报警通道：";
                    for (i = 0; i < CHCNetSDK.MAX_CHANNUM; i++)
                    {
                        if (struAlarmInfo.dwChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 4:
                    stringAlarm = "硬盘未格式化，报警硬盘号：";
                    for (i = 0; i < CHCNetSDK.MAX_DISKNUM; i++)
                    {
                        if (struAlarmInfo.dwDiskNumber[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 5:
                    stringAlarm = "读写硬盘出错，报警硬盘号：";
                    for (i = 0; i < CHCNetSDK.MAX_DISKNUM; i++)
                    {
                        if (struAlarmInfo.dwDiskNumber[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 6:
                    stringAlarm = "遮挡报警，报警通道：";
                    for (i = 0; i < CHCNetSDK.MAX_CHANNUM; i++)
                    {
                        if (struAlarmInfo.dwChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 7:
                    stringAlarm = "制式不匹配，报警通道";
                    for (i = 0; i < CHCNetSDK.MAX_CHANNUM; i++)
                    {
                        if (struAlarmInfo.dwChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 8:
                    stringAlarm = "非法访问";
                    break;
                default:
                    stringAlarm = "其他未知报警信息";
                    break;
            }


            //创建该控件的主线程直接更新信息列表 
            UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);

        }

        /// <summary>
        /// 移动侦测、视频丢失、遮挡、IO信号量等报警信息
        /// </summary>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
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
                    stringAlarm = "信号量报警，报警报警输入口：" + struAlarmInfoV30.dwAlarmInputNumber + "，触发录像通道：";
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
            UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);

        }

        /// <summary>
        /// 进出区域、入侵、徘徊、人员聚集等行为分析报警信息
        /// </summary>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        private void ProcessCommAlarm_RULE(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
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
                    stringAlarm = "穿越警戒面，目标ID：" + struRuleAlarmInfo.struTargetInfo.dwID;
                    //警戒面边线起点坐标: (m_struTraversePlane.struPlaneBottom.struStart.fX, m_struTraversePlane.struPlaneBottom.struStart.fY)
                    //警戒面边线终点坐标: (m_struTraversePlane.struPlaneBottom.struEnd.fX, m_struTraversePlane.struPlaneBottom.struEnd.fY)
                    break;
                case (ushort)CHCNetSDK.VCA_RULE_EVENT_TYPE_EX.ENUM_VCA_EVENT_ENTER_AREA:
                    IntPtr ptrEnterInfo = Marshal.AllocHGlobal((Int32)dwSize);
                    Marshal.StructureToPtr(struRuleAlarmInfo.struRuleInfo.uEventParam, ptrEnterInfo, false);
                    m_struVcaArea = (CHCNetSDK.NET_VCA_AREA)Marshal.PtrToStructure(ptrEnterInfo, typeof(CHCNetSDK.NET_VCA_AREA));
                    stringAlarm = "目标进入区域，目标ID：" + struRuleAlarmInfo.struTargetInfo.dwID;
                    //m_struVcaArea.struRegion 多边形区域坐标
                    break;
                case (ushort)CHCNetSDK.VCA_RULE_EVENT_TYPE_EX.ENUM_VCA_EVENT_EXIT_AREA:
                    IntPtr ptrExitInfo = Marshal.AllocHGlobal((Int32)dwSize);
                    Marshal.StructureToPtr(struRuleAlarmInfo.struRuleInfo.uEventParam, ptrExitInfo, false);
                    m_struVcaArea = (CHCNetSDK.NET_VCA_AREA)Marshal.PtrToStructure(ptrExitInfo, typeof(CHCNetSDK.NET_VCA_AREA));
                    stringAlarm = "目标离开区域，目标ID：" + struRuleAlarmInfo.struTargetInfo.dwID;
                    //m_struVcaArea.struRegion 多边形区域坐标
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
                    stringAlarm = "周界入侵，目标ID：" + struRuleAlarmInfo.struTargetInfo.dwID + "，区域范围：" + strRegion;
                    //m_struIntrusion.struRegion 多边形区域坐标
                    break;
                default:
                    stringAlarm = "其他行为分析报警，目标ID：" + struRuleAlarmInfo.struTargetInfo.dwID;
                    break;
            }

            //报警图片保存
            if (struRuleAlarmInfo.dwPicDataLen > 0)
            {
                FileStream fs = new FileStream("行为分析报警抓图.jpg", FileMode.Create);
                int iLen = (int)struRuleAlarmInfo.dwPicDataLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struRuleAlarmInfo.pImage, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();
            }

            //报警时间：年月日时分秒
            string strTimeYear = ((struRuleAlarmInfo.dwAbsTime >> 26) + 2000).ToString();
            string strTimeMonth = ((struRuleAlarmInfo.dwAbsTime >> 22) & 15).ToString("d2");
            string strTimeDay = ((struRuleAlarmInfo.dwAbsTime >> 17) & 31).ToString("d2");
            string strTimeHour = ((struRuleAlarmInfo.dwAbsTime >> 12) & 31).ToString("d2");
            string strTimeMinute = ((struRuleAlarmInfo.dwAbsTime >> 6) & 63).ToString("d2");
            string strTimeSecond = ((struRuleAlarmInfo.dwAbsTime >> 0) & 63).ToString("d2");
            string strTime = strTimeYear + "-" + strTimeMonth + "-" + strTimeDay + " " + strTimeHour + ":" + strTimeMinute + ":" + strTimeSecond;

            //报警设备IP地址
            string strIP = struRuleAlarmInfo.struDevInfo.struDevIP.sIpV4;

            //将报警信息添加进列表

            //创建该控件的主线程直接更新信息列表 
            UpdateClientList(strTime, strIP, stringAlarm);
        }

        /// <summary>
        /// 交通抓拍结果上传(老报警信息类型)
        /// </summary>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        private void ProcessCommAlarm_Plate(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_DVR_PLATE_RESULT struPlateResultInfo = new CHCNetSDK.NET_DVR_PLATE_RESULT();
            uint dwSize = (uint)Marshal.SizeOf(struPlateResultInfo);

            struPlateResultInfo = (CHCNetSDK.NET_DVR_PLATE_RESULT)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_DVR_PLATE_RESULT));

            //保存抓拍图片
            string str = "";
            if (struPlateResultInfo.byResultType == 1 && struPlateResultInfo.dwPicLen != 0)
            {
                str = "Plate_UserID_" + pAlarmer.lUserID + "_近景图.jpg";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)struPlateResultInfo.dwPicLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struPlateResultInfo.pBuffer1, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();
            }
            if (struPlateResultInfo.dwPicPlateLen != 0)
            {
                str = "Plate_UserID_" + pAlarmer.lUserID + "_车牌图.jpg";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)struPlateResultInfo.dwPicPlateLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struPlateResultInfo.pBuffer2, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();
            }
            if (struPlateResultInfo.dwFarCarPicLen != 0)
            {
                str = "Plate_UserID_" + pAlarmer.lUserID + "_远景图.jpg";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)struPlateResultInfo.dwFarCarPicLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struPlateResultInfo.pBuffer5, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();
            }

            //报警设备IP地址
            string strIP = pAlarmer.sDeviceIP;

            //抓拍时间：年月日时分秒
            string strTimeYear = System.Text.Encoding.UTF8.GetString(struPlateResultInfo.byAbsTime);

            //上传结果
            string stringPlateLicense = System.Text.Encoding.GetEncoding("GBK").GetString(struPlateResultInfo.struPlateInfo.sLicense).TrimEnd('\0');
            string stringAlarm = "抓拍上传，" + "车牌：" + stringPlateLicense + "，车辆序号：" + struPlateResultInfo.struVehicleInfo.dwIndex;


            //创建该控件的主线程直接更新信息列表 
            UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
        }

        /// <summary>
        /// 交通抓拍结果上传(新报警信息类型)
        /// </summary>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        private void ProcessCommAlarm_ITSPlate(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_ITS_PLATE_RESULT struITSPlateResult = new CHCNetSDK.NET_ITS_PLATE_RESULT();
            uint dwSize = (uint)Marshal.SizeOf(struITSPlateResult);

            struITSPlateResult = (CHCNetSDK.NET_ITS_PLATE_RESULT)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_ITS_PLATE_RESULT));

            //保存抓拍图片
            for (int i = 0; i < struITSPlateResult.dwPicNum; i++)
            {
                if (struITSPlateResult.struPicInfo[i].dwDataLen != 0)
                {
                    string str = "ITS_UserID_[" + pAlarmer.lUserID + "]_Pictype_" + struITSPlateResult.struPicInfo[i].byType + "_Num" + (i + 1) + ".jpg";
                    FileStream fs = new FileStream(str, FileMode.Create);
                    int iLen = (int)struITSPlateResult.struPicInfo[i].dwDataLen;
                    byte[] by = new byte[iLen];
                    Marshal.Copy(struITSPlateResult.struPicInfo[i].pBuffer, by, 0, iLen);
                    fs.Write(by, 0, iLen);
                    fs.Close();
                }
            }
            //报警设备IP地址
            string strIP = pAlarmer.sDeviceIP;

            //抓拍时间：年月日时分秒
            string strTimeYear = string.Format("{0:D4}", struITSPlateResult.struSnapFirstPicTime.wYear) +
                string.Format("{0:D2}", struITSPlateResult.struSnapFirstPicTime.byMonth) +
                string.Format("{0:D2}", struITSPlateResult.struSnapFirstPicTime.byDay) + " "
                + string.Format("{0:D2}", struITSPlateResult.struSnapFirstPicTime.byHour) + ":"
                + string.Format("{0:D2}", struITSPlateResult.struSnapFirstPicTime.byMinute) + ":"
                + string.Format("{0:D2}", struITSPlateResult.struSnapFirstPicTime.bySecond) + ":"
                + string.Format("{0:D3}", struITSPlateResult.struSnapFirstPicTime.wMilliSec);

            //上传结果
            string stringPlateLicense = System.Text.Encoding.GetEncoding("GBK").GetString(struITSPlateResult.struPlateInfo.sLicense).TrimEnd('\0');
            string stringAlarm = "抓拍上传，" + "车牌：" + stringPlateLicense + "，车辆序号：" + struITSPlateResult.struVehicleInfo.dwIndex;


            //创建该控件的主线程直接更新信息列表 
            UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
        }

        /// <summary>
        /// 客流量统计报警信息
        /// </summary>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        //private void ProcessCommAlarm_PDC(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        //{
        //    CHCNetSDK.NET_DVR_PDC_ALRAM_INFO struPDCInfo = new CHCNetSDK.NET_DVR_PDC_ALRAM_INFO();
        //    uint dwSize = (uint)Marshal.SizeOf(struPDCInfo);
        //    struPDCInfo = (CHCNetSDK.NET_DVR_PDC_ALRAM_INFO)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_DVR_PDC_ALRAM_INFO));

        //    string stringAlarm = "客流量统计，进入人数：" + struPDCInfo.dwEnterNum + "，离开人数：" + struPDCInfo.dwLeaveNum;

        //    uint dwUnionSize = (uint)Marshal.SizeOf(struPDCInfo.uStatModeParam);
        //    IntPtr ptrPDCUnion = Marshal.AllocHGlobal((Int32)dwUnionSize);
        //    Marshal.StructureToPtr(struPDCInfo.uStatModeParam, ptrPDCUnion, false);

        //    if (struPDCInfo.byMode == 0) //单帧统计结果，此处为UTC时间
        //    {
        //        m_struStatFrame = (CHCNetSDK.UNION_STATFRAME)Marshal.PtrToStructure(ptrPDCUnion, typeof(CHCNetSDK.UNION_STATFRAME));
        //        stringAlarm = stringAlarm + "，单帧统计，相对时标：" + m_struStatFrame.dwRelativeTime + "，绝对时标：" + m_struStatFrame.dwAbsTime;
        //    }
        //    if (struPDCInfo.byMode == 1) //最小时间段统计结果
        //    {
        //        m_struStatTime = (CHCNetSDK.UNION_STATTIME)Marshal.PtrToStructure(ptrPDCUnion, typeof(CHCNetSDK.UNION_STATTIME));

        //        //开始时间
        //        string strStartTime = string.Format("{0:D4}", m_struStatTime.tmStart.dwYear) +
        //        string.Format("{0:D2}", m_struStatTime.tmStart.dwMonth) +
        //        string.Format("{0:D2}", m_struStatTime.tmStart.dwDay) + " "
        //        + string.Format("{0:D2}", m_struStatTime.tmStart.dwHour) + ":"
        //        + string.Format("{0:D2}", m_struStatTime.tmStart.dwMinute) + ":"
        //        + string.Format("{0:D2}", m_struStatTime.tmStart.dwSecond);

        //        //结束时间
        //        string strEndTime = string.Format("{0:D4}", m_struStatTime.tmEnd.dwYear) +
        //        string.Format("{0:D2}", m_struStatTime.tmEnd.dwMonth) +
        //        string.Format("{0:D2}", m_struStatTime.tmEnd.dwDay) + " "
        //        + string.Format("{0:D2}", m_struStatTime.tmEnd.dwHour) + ":"
        //        + string.Format("{0:D2}", m_struStatTime.tmEnd.dwMinute) + ":"
        //        + string.Format("{0:D2}", m_struStatTime.tmEnd.dwSecond);

        //        stringAlarm = stringAlarm + "，最小时间段统计，开始时间：" + strStartTime + "，结束时间：" + strEndTime;
        //    }
        //    Marshal.FreeHGlobal(ptrPDCUnion);

        //    //报警设备IP地址
        //    string strIP = pAlarmer.sDeviceIP;


        //    if (InvokeRequired)
        //    {
        //        object[] paras = new object[3];
        //        paras[0] = DateTime.Now.ToString(); //当前PC系统时间
        //        paras[1] = strIP;
        //        paras[2] = stringAlarm;
        //        listViewAlarmInfo.BeginInvoke(new UpdateListBoxCallback(UpdateClientList), paras);
        //    }
        //    else
        //    {
        //        //创建该控件的主线程直接更新信息列表 
        //        UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
        //    }
        //}

        //停车报警信息处理
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        private void ProcessCommAlarm_PARK(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_ITS_PARK_VEHICLE struParkInfo = new CHCNetSDK.NET_ITS_PARK_VEHICLE();
            uint dwSize = (uint)Marshal.SizeOf(struParkInfo);
            struParkInfo = (CHCNetSDK.NET_ITS_PARK_VEHICLE)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_ITS_PARK_VEHICLE));

            //报警设备IP地址
            string strIP = pAlarmer.sDeviceIP;

            //保存抓拍图片
            for (int i = 0; i < struParkInfo.dwPicNum; i++)
            {
                if ((struParkInfo.struPicInfo[i].dwDataLen != 0) && (struParkInfo.struPicInfo[i].pBuffer != IntPtr.Zero))
                {
                    string str = "Device_Park_[" + strIP + "]_lUerID_[" + pAlarmer.lUserID + "]_Pictype_" + struParkInfo.struPicInfo[i].byType + "_Num" + (i + 1) + ".jpg";
                    FileStream fs = new FileStream(str, FileMode.Create);
                    int iLen = (int)struParkInfo.struPicInfo[i].dwDataLen;
                    byte[] by = new byte[iLen];
                    Marshal.Copy(struParkInfo.struPicInfo[i].pBuffer, by, 0, iLen);
                    fs.Write(by, 0, iLen);
                    fs.Close();
                }
            }

            string stringAlarm = "停车场数据上传，异常状态：" + struParkInfo.byParkError + "，车位编号：" + struParkInfo.byParkingNo +
                ", 车辆状态：" + struParkInfo.byLocationStatus + "，车牌号码：" +
                System.Text.Encoding.GetEncoding("GBK").GetString(struParkInfo.struPlateInfo.sLicense).TrimEnd('\0');


            //创建该控件的主线程直接更新信息列表 
            UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
        }

        //视频质量诊断报警信息处理
        private void ProcessCommAlarm_VQD(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_DVR_DIAGNOSIS_UPLOAD struVQDInfo = new CHCNetSDK.NET_DVR_DIAGNOSIS_UPLOAD();
            uint dwSize = (uint)Marshal.SizeOf(struVQDInfo);
            struVQDInfo = (CHCNetSDK.NET_DVR_DIAGNOSIS_UPLOAD)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_DVR_DIAGNOSIS_UPLOAD));

            //报警设备IP地址
            string strIP = pAlarmer.sDeviceIP;

            //开始时间
            string strCheckTime = string.Format("{0:D4}", struVQDInfo.struCheckTime.dwYear) +
            string.Format("{0:D2}", struVQDInfo.struCheckTime.dwMonth) +
            string.Format("{0:D2}", struVQDInfo.struCheckTime.dwDay) + " "
            + string.Format("{0:D2}", struVQDInfo.struCheckTime.dwHour) + ":"
            + string.Format("{0:D2}", struVQDInfo.struCheckTime.dwMinute) + ":"
            + string.Format("{0:D2}", struVQDInfo.struCheckTime.dwSecond);

            string stringAlarm = "视频质量诊断结果，流ID：" + struVQDInfo.sStreamID + "，监测点IP：" + struVQDInfo.sMonitorIP + "，_点通道号：" + struVQDInfo.dwChanIndex +
                "，检测时间：" + strCheckTime + "，byResult：" + struVQDInfo.byResult + "，bySignalResult：" + struVQDInfo.bySignalResult + "，byBlurResult：" + struVQDInfo.byBlurResult;


            //创建该控件的主线程直接更新信息列表 
            UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
        }

        /// <summary>
        /// 人脸抓拍报警信息处理
        /// </summary>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        private void ProcessCommAlarm_FaceSnap(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_VCA_FACESNAP_RESULT struFaceSnapInfo = new CHCNetSDK.NET_VCA_FACESNAP_RESULT();
            uint dwSize = (uint)Marshal.SizeOf(struFaceSnapInfo);
            struFaceSnapInfo = (CHCNetSDK.NET_VCA_FACESNAP_RESULT)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_VCA_FACESNAP_RESULT));

            //报警设备IP地址
            string strIP = pAlarmer.sDeviceIP;

            //保存抓拍图片数据
            if ((struFaceSnapInfo.dwBackgroundPicLen != 0) && (struFaceSnapInfo.pBuffer2 != IntPtr.Zero))
            {
                string str = "Device_FaceSnap_CapPic_[" + strIP + "]_lUerID_[" + pAlarmer.lUserID + "]" + ".jpg";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)struFaceSnapInfo.dwBackgroundPicLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struFaceSnapInfo.pBuffer2, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();
            }

            //报警时间：年月日时分秒
            string strTimeYear = ((struFaceSnapInfo.dwAbsTime >> 26) + 2000).ToString();
            string strTimeMonth = ((struFaceSnapInfo.dwAbsTime >> 22) & 15).ToString("d2");
            string strTimeDay = ((struFaceSnapInfo.dwAbsTime >> 17) & 31).ToString("d2");
            string strTimeHour = ((struFaceSnapInfo.dwAbsTime >> 12) & 31).ToString("d2");
            string strTimeMinute = ((struFaceSnapInfo.dwAbsTime >> 6) & 63).ToString("d2");
            string strTimeSecond = ((struFaceSnapInfo.dwAbsTime >> 0) & 63).ToString("d2");
            string strTime = strTimeYear + "-" + strTimeMonth + "-" + strTimeDay + " " + strTimeHour + ":" + strTimeMinute + ":" + strTimeSecond;

            string stringAlarm = "人脸抓拍结果，前端设备：" + struFaceSnapInfo.struDevInfo.struDevIP.sIpV4 + "，报警时间：" + strTime;

            //创建该控件的主线程直接更新信息列表 
            UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
        }

        /// <summary>
        /// 人脸对比报警信息处理
        /// </summary>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        private void ProcessCommAlarm_FaceMatch(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_VCA_FACESNAP_MATCH_ALARM struFaceMatchAlarm = new CHCNetSDK.NET_VCA_FACESNAP_MATCH_ALARM();
            uint dwSize = (uint)Marshal.SizeOf(struFaceMatchAlarm);
            struFaceMatchAlarm = (CHCNetSDK.NET_VCA_FACESNAP_MATCH_ALARM)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_VCA_FACESNAP_MATCH_ALARM));

            //报警设备IP地址
            string strIP = pAlarmer.sDeviceIP;

            //保存抓拍人脸子图图片数据
            if ((struFaceMatchAlarm.struSnapInfo.dwSnapFacePicLen != 0) && (struFaceMatchAlarm.struSnapInfo.pBuffer1 != IntPtr.Zero))
            {
                string str = "Device_FaceMatch_FacePic_[" + strIP + "]_lUerID_[" + pAlarmer.lUserID + "]" + ".jpg";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)struFaceMatchAlarm.struSnapInfo.dwSnapFacePicLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struFaceMatchAlarm.struSnapInfo.pBuffer1, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();
            }

            //保存比对结果人脸库人脸图片数据
            if ((struFaceMatchAlarm.struBlockListInfo.dwBlockListPicLen != 0) && (struFaceMatchAlarm.struBlockListInfo.pBuffer1 != IntPtr.Zero))
            {
                string str = "Device_FaceMatch_BlockListPic_[" + strIP + "]_lUerID_[" + pAlarmer.lUserID + "]" + "_fSimilarity[" + struFaceMatchAlarm.fSimilarity + "].jpg";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)struFaceMatchAlarm.struBlockListInfo.dwBlockListPicLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struFaceMatchAlarm.struBlockListInfo.pBuffer1, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();
            }

            //抓拍时间：年月日时分秒
            string strTimeYear = ((struFaceMatchAlarm.struSnapInfo.dwAbsTime >> 26) + 2000).ToString();
            string strTimeMonth = ((struFaceMatchAlarm.struSnapInfo.dwAbsTime >> 22) & 15).ToString("d2");
            string strTimeDay = ((struFaceMatchAlarm.struSnapInfo.dwAbsTime >> 17) & 31).ToString("d2");
            string strTimeHour = ((struFaceMatchAlarm.struSnapInfo.dwAbsTime >> 12) & 31).ToString("d2");
            string strTimeMinute = ((struFaceMatchAlarm.struSnapInfo.dwAbsTime >> 6) & 63).ToString("d2");
            string strTimeSecond = ((struFaceMatchAlarm.struSnapInfo.dwAbsTime >> 0) & 63).ToString("d2");
            string strTime = strTimeYear + "-" + strTimeMonth + "-" + strTimeDay + " " + strTimeHour + ":" + strTimeMinute + ":" + strTimeSecond;

            string stringAlarm = "人脸比对报警，抓拍设备：" + struFaceMatchAlarm.struSnapInfo.struDevInfo.struDevIP.sIpV4 + "，抓拍时间："
                + strTime + "，相似度：" + struFaceMatchAlarm.fSimilarity;

            //创建该控件的主线程直接更新信息列表 
            UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
        }

        /// <summary>
        /// 人脸侦测报警信息处理
        /// </summary>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        private void ProcessCommAlarm_FaceDetect(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_DVR_FACE_DETECTION struFaceDetectInfo = new CHCNetSDK.NET_DVR_FACE_DETECTION();
            uint dwSize = (uint)Marshal.SizeOf(struFaceDetectInfo);
            struFaceDetectInfo = (CHCNetSDK.NET_DVR_FACE_DETECTION)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_DVR_FACE_DETECTION));

            //报警设备IP地址
            string strIP = pAlarmer.sDeviceIP;

            //报警时间：年月日时分秒
            string strTimeYear = ((struFaceDetectInfo.dwAbsTime >> 26) + 2000).ToString();
            string strTimeMonth = ((struFaceDetectInfo.dwAbsTime >> 22) & 15).ToString("d2");
            string strTimeDay = ((struFaceDetectInfo.dwAbsTime >> 17) & 31).ToString("d2");
            string strTimeHour = ((struFaceDetectInfo.dwAbsTime >> 12) & 31).ToString("d2");
            string strTimeMinute = ((struFaceDetectInfo.dwAbsTime >> 6) & 63).ToString("d2");
            string strTimeSecond = ((struFaceDetectInfo.dwAbsTime >> 0) & 63).ToString("d2");
            string strTime = strTimeYear + "-" + strTimeMonth + "-" + strTimeDay + " " + strTimeHour + ":" + strTimeMinute + ":" + strTimeSecond;

            string stringAlarm = "人脸抓拍结果结果，前端设备：" + struFaceDetectInfo.struDevInfo.struDevIP + "，报警时间：" + strTime;

            //创建该控件的主线程直接更新信息列表 
            UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
        }

        /// <summary>
        /// 报警主机CID报告
        /// </summary>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        private void ProcessCommAlarm_CIDAlarm(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_DVR_CID_ALARM struCIDAlarm = new CHCNetSDK.NET_DVR_CID_ALARM();
            uint dwSize = (uint)Marshal.SizeOf(struCIDAlarm);
            struCIDAlarm = (CHCNetSDK.NET_DVR_CID_ALARM)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_DVR_CID_ALARM));

            //报警设备IP地址
            string strIP = pAlarmer.sDeviceIP;

            //报警时间：年月日时分秒
            string strTimeYear = (struCIDAlarm.struTriggerTime.wYear).ToString();
            string strTimeMonth = (struCIDAlarm.struTriggerTime.byMonth).ToString("d2");
            string strTimeDay = (struCIDAlarm.struTriggerTime.byDay).ToString("d2");
            string strTimeHour = (struCIDAlarm.struTriggerTime.byHour).ToString("d2");
            string strTimeMinute = (struCIDAlarm.struTriggerTime.byMinute).ToString("d2");
            string strTimeSecond = (struCIDAlarm.struTriggerTime.bySecond).ToString("d2");
            string strTime = strTimeYear + "-" + strTimeMonth + "-" + strTimeDay + " " + strTimeHour + ":" + strTimeMinute + ":" + strTimeSecond;

            string stringAlarm = "报警主机CID报告，sCIDCode：" + System.Text.Encoding.UTF8.GetString(struCIDAlarm.sCIDCode).TrimEnd('\0')
                + "，sCIDDescribe：" + System.Text.Encoding.UTF8.GetString(struCIDAlarm.sCIDDescribe).TrimEnd('\0')
                + "，报告类型：" + struCIDAlarm.byReportType + "，防区号：" + struCIDAlarm.wDefenceNo + "，报警触发时间：" + strTime;

            //创建该控件的主线程直接更新信息列表 
            UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
        }

        /// <summary>
        /// 门禁主机报警信息
        /// </summary>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        private void ProcessCommAlarm_AcsAlarm(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_DVR_ACS_ALARM_INFO struAcsAlarm = new CHCNetSDK.NET_DVR_ACS_ALARM_INFO();
            uint dwSize = (uint)Marshal.SizeOf(struAcsAlarm);
            struAcsAlarm = (CHCNetSDK.NET_DVR_ACS_ALARM_INFO)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_DVR_ACS_ALARM_INFO));

            //报警设备IP地址
            string strIP = pAlarmer.sDeviceIP;

            //保存抓拍图片
            if ((struAcsAlarm.dwPicDataLen != 0) && (struAcsAlarm.pPicData != IntPtr.Zero))
            {
                string str = "Device_Acs_CapturePic_[" + strIP + "]_lUerID_[" + pAlarmer.lUserID + "]" + ".jpg";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)struAcsAlarm.dwPicDataLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struAcsAlarm.pPicData, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();
            }

            //报警时间：年月日时分秒
            string strTimeYear = (struAcsAlarm.struTime.dwYear).ToString();
            string strTimeMonth = (struAcsAlarm.struTime.dwMonth).ToString("d2");
            string strTimeDay = (struAcsAlarm.struTime.dwDay).ToString("d2");
            string strTimeHour = (struAcsAlarm.struTime.dwHour).ToString("d2");
            string strTimeMinute = (struAcsAlarm.struTime.dwMinute).ToString("d2");
            string strTimeSecond = (struAcsAlarm.struTime.dwSecond).ToString("d2");
            string strTime = strTimeYear + "-" + strTimeMonth + "-" + strTimeDay + " " + strTimeHour + ":" + strTimeMinute + ":" + strTimeSecond;

            string stringAlarm = "门禁主机报警信息，dwMajor：" + struAcsAlarm.dwMajor + "，dwMinor：" + struAcsAlarm.dwMinor + "，卡号："
                + System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0') + "，读卡器编号：" +
                struAcsAlarm.struAcsEventInfo.dwCardReaderNo + "，报警触发时间：" + strTime;

                //创建该控件的主线程直接更新信息列表 
            UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);

        }

        /// <summary>
        /// 身份证刷卡信息上传
        /// </summary>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        private void ProcessCommAlarm_IDInfoAlarm(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_DVR_ID_CARD_INFO_ALARM struIDInfoAlarm = new CHCNetSDK.NET_DVR_ID_CARD_INFO_ALARM();
            uint dwSize = (uint)Marshal.SizeOf(struIDInfoAlarm);
            struIDInfoAlarm = (CHCNetSDK.NET_DVR_ID_CARD_INFO_ALARM)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_DVR_ID_CARD_INFO_ALARM));

            //报警设备IP地址
            string strIP = pAlarmer.sDeviceIP;

            //保存抓拍图片
            if ((struIDInfoAlarm.dwCapturePicDataLen != 0) && (struIDInfoAlarm.pCapturePicData != IntPtr.Zero))
            {
                string str = "Device_ID_CapturePic_[" + strIP + "]_lUerID_[" + pAlarmer.lUserID + "]" + ".jpg";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)struIDInfoAlarm.dwCapturePicDataLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struIDInfoAlarm.pCapturePicData, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();
            }

            //保存身份证图片数据
            if ((struIDInfoAlarm.dwPicDataLen != 0) && (struIDInfoAlarm.pPicData != IntPtr.Zero))
            {
                string str = "Device_ID_IDPic_[" + strIP + "]_lUerID_[" + pAlarmer.lUserID + "]" + ".jpg";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)struIDInfoAlarm.dwPicDataLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struIDInfoAlarm.pPicData, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();
            }

            //保存指纹数据
            if ((struIDInfoAlarm.dwFingerPrintDataLen != 0) && (struIDInfoAlarm.pFingerPrintData != IntPtr.Zero))
            {
                string str = "Device_ID_FingerPrint_[" + strIP + "]_lUerID_[" + pAlarmer.lUserID + "]" + ".data";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)struIDInfoAlarm.dwFingerPrintDataLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struIDInfoAlarm.pFingerPrintData, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();
            }

            //报警时间：年月日时分秒
            string strTimeYear = (struIDInfoAlarm.struSwipeTime.wYear).ToString();
            string strTimeMonth = (struIDInfoAlarm.struSwipeTime.byMonth).ToString("d2");
            string strTimeDay = (struIDInfoAlarm.struSwipeTime.byDay).ToString("d2");
            string strTimeHour = (struIDInfoAlarm.struSwipeTime.byHour).ToString("d2");
            string strTimeMinute = (struIDInfoAlarm.struSwipeTime.byMinute).ToString("d2");
            string strTimeSecond = (struIDInfoAlarm.struSwipeTime.bySecond).ToString("d2");
            string strTime = strTimeYear + "-" + strTimeMonth + "-" + strTimeDay + " " + strTimeHour + ":" + strTimeMinute + ":" + strTimeSecond;

            string stringAlarm = "身份证刷卡信息，dwMajor：" + struIDInfoAlarm.dwMajor + "，dwMinor：" + struIDInfoAlarm.dwMinor
                + "，身份证号：" + System.Text.Encoding.UTF8.GetString(struIDInfoAlarm.struIDCardCfg.byIDNum).TrimEnd('\0') +
                "，姓名：" + System.Text.Encoding.UTF8.GetString(struIDInfoAlarm.struIDCardCfg.byName).TrimEnd('\0') +
                "，刷卡时间：" + strTime;

                //创建该控件的主线程直接更新信息列表 
                UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
        }

        /// <summary>
        /// 委托事件，实时更新报警信息
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
                if (listViewAlarmInfo.Items.Count > 200)
                {
                    listViewAlarmInfo.Items.RemoveAt(0);
                }
                //列表新增报警信息
                listViewAlarmInfo.Items.Add(new ListViewItem(new string[] { strAlarmTime, strDevIP, strAlarmMsg }));
            }
        }


    }
}
