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
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using System.Threading;

namespace SDKANPR
{
    public partial class ANPRAlarmForm : Form
    {

        private List<byte> byBuffer = new List<byte>();
        public delegate bool ProcessLongLinkData(byte[] data, string boundary);
        public delegate bool ProcessSendDate(ref byte[] byBuffer);
        public static int m_iHttpTimeOut = 5000;
        const int BUFFER_SIZE = 3 * 1024 * 1024;
        public bool m_bIsAlarmStart = false;
        private string strURL = string.Empty;

        public ANPRAlarmForm()
        {
            InitializeComponent();
            m_lFortifyHandle = -1;
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        //设备树对象映射
        IDeviceTree m_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        //报警布防回调
        private CHCNetSDK.MSGCallBack m_falarmData = null;
        //布防句柄
        private int m_lFortifyHandle;
        //本地存储路径
        public string strLocalFilePath;

        private void ANPRAlarmForm_Load(object sender, EventArgs e)
        {

            strLocalFilePath = Application.StartupPath + "\\ANPR\\" + DateTime.Now.ToString("yyyy-MM-dd");
            if (!Directory.Exists(strLocalFilePath))
                Directory.CreateDirectory(strLocalFilePath);
        }

        public void MsgCallback(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            switch (lCommand)
            {

                case CHCNetSDK.COMM_ITS_PLATE_RESULT:
                    ProcessCommAlarm_ITS(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                default:
                    break;
            }
        }

        public delegate void ProcessCommAlarm_ITS_Handle(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser);

        private void ProcessCommAlarm_ITS(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            if (this.InvokeRequired)
            {
                ProcessCommAlarm_ITS_Handle handle = new ProcessCommAlarm_ITS_Handle(ProcessCommAlarm_ITS);
                this.BeginInvoke(handle, pAlarmer, pAlarmInfo, dwBufLen, pUser);
            }
            else
            {
                //获取当前系统时间
                string datatimenow = DateTime.Now.ToString("yyyyMMddhhmmssfff");
                //车牌图片存储路径
                string strPicFilePathA = "";
                //车辆检测图片存储路径
                string strPicFilePathB = "";
                //ANPR展示数据存储路径
                string strShowDataPathA = "";

                //报警信息解析
                IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
                CHCNetSDK.NET_ITS_PLATE_RESULT struITSPlateResult = new Common.CHCNetSDK.NET_ITS_PLATE_RESULT();
                uint dwSize = (uint)Marshal.SizeOf(struITSPlateResult);
                struITSPlateResult = (CHCNetSDK.NET_ITS_PLATE_RESULT)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_ITS_PLATE_RESULT));
                CHCNetSDK.NET_DVR_PLATE_INFO struPlateInfo = struITSPlateResult.struPlateInfo;

                string strLicense = System.Text.Encoding.Default.GetString(struITSPlateResult.struPlateInfo.sLicense);


                //车牌报警图片信息展示
                if (struITSPlateResult.dwPicNum != 0)
                {
                    int picCount = 0;
                    for (int i = 0; i < struITSPlateResult.dwPicNum; i++)
                    {
                        if (struITSPlateResult.struPicInfo[i].dwDataLen > 0 && struITSPlateResult.struPicInfo[i].pBuffer != IntPtr.Zero)
                        {
                            picCount++;
                            if (picCount == 1)
                            {
                                strPicFilePathA = strLocalFilePath + "\\ANPRAlarmInfo" + datatimenow + "Pic.jpg";
                                int iLen = (int)struITSPlateResult.struPicInfo[i].dwDataLen;
                                byte[] byDataTempBuffer = new byte[iLen];
                                Marshal.Copy(struITSPlateResult.struPicInfo[i].pBuffer, byDataTempBuffer, 0, iLen);
                                using (FileStream fsWrite = new FileStream(strPicFilePathA, FileMode.Create))
                                {
                                    fsWrite.Write(byDataTempBuffer, 0, iLen);
                                };
                                ANPRPicBox.Image = Image.FromFile(strPicFilePathA);
                            }
                            else if (picCount == 2)
                            {
                                strPicFilePathB = strLocalFilePath + "\\ANPRAlarmInfo" + datatimenow + "DPic.jpg";
                                int iLen = (int)struITSPlateResult.struPicInfo[i].dwDataLen;
                                byte[] byDataTempBuffer = new byte[iLen];
                                Marshal.Copy(struITSPlateResult.struPicInfo[i].pBuffer, byDataTempBuffer, 0, iLen);
                                using (FileStream fsWrite = new FileStream(strPicFilePathA, FileMode.Create))
                                {
                                    fsWrite.Write(byDataTempBuffer, 0, iLen);
                                };
                                ANPRDPicBox.Image = Image.FromFile(strPicFilePathB);
                            }
                        }
                    }
                }

                //车牌报警详细信息展示
                string Info = struPlateInfo.pXmlBuf.ToString();
                String InfoPlateResult = string.Format("ITS Plate Alarm Channel NO[{0}] DriveChan[{1}] IllegalFromatType[{2}] IllegalInfo[{3}] Analysis[{4}] YellowLabel[{5}] DangerousVeh[{6}] MatchNo[{7}] IllegalType[{8}] IllegalSubType[{9}] MonitoringSiteID[{10}] DeviceID[{11}] Dir[{12}] PicNum[{13}] DetSceneID[{14}] VehicleType[{15}] DetectType[{16}]",
                    struITSPlateResult.byChanIndexEx * 256 + struITSPlateResult.byChanIndex, struITSPlateResult.byDriveChan, struITSPlateResult.byIllegalFromatType, struITSPlateResult.pIllegalInfoBuf,
                    struITSPlateResult.byDataAnalysis, struITSPlateResult.byYellowLabelCar, struITSPlateResult.byDangerousVehicles, struITSPlateResult.dwMatchNo, struITSPlateResult.wIllegalType,
                    struITSPlateResult.byIllegalSubType, struITSPlateResult.byMonitoringSiteID, struITSPlateResult.byDeviceID, struITSPlateResult.byDir, struITSPlateResult.dwPicNum, struITSPlateResult.byDetSceneID,
                    struITSPlateResult.byVehicleType, struITSPlateResult.byDetectType);
                String InfoPlate = string.Format("", "");
                if (struPlateInfo.dwXmlLen != 0)
                {
                    ITSPlateInfoTextBox.Text = Info;
                    //车牌报警详细信息储存
                    strShowDataPathA = strLocalFilePath + "\\ANPRAlarmInfo" + datatimenow + "ShowData.txt";
                    FileStream fs = new FileStream(strShowDataPathA, FileMode.Create);
                    int iLen = (int)struPlateInfo.dwXmlLen;
                    byte[] byDataTempBuffer = new byte[iLen];
                    Marshal.Copy(struPlateInfo.pXmlBuf, byDataTempBuffer, 0, iLen);
                    fs.Write(byDataTempBuffer, 0, iLen);
                    fs.Close();
                }
                else
                {
                    ITSPlateInfoTextBox.Text = InfoPlateResult;
                    //车牌报警详细信息储存
                    strShowDataPathA = strLocalFilePath + "\\ANPRAlarmInfo" + datatimenow + "ShowData.txt";

                    File.WriteAllText(@strShowDataPathA, InfoPlateResult);
                }


                //车牌报警信息展示
                ListViewItem lviA = new ListViewItem();
                //报警IP
                lviA.Text = deviceInfo.sDeviceIP;
                //报警触发时间
                lviA.SubItems.Add(datatimenow);
                //车辆类型
                switch (struITSPlateResult.byVehicleType)
                {
                    case 0: lviA.SubItems.Add("未知");
                        break;
                    case 1: lviA.SubItems.Add("客车");
                        break;
                    case 2: lviA.SubItems.Add("货车");
                        break;
                    case 3: lviA.SubItems.Add("轿车");
                        break;
                    case 4: lviA.SubItems.Add("面包车");
                        break;
                    case 5: lviA.SubItems.Add("小货车");
                        break;
                    default: lviA.SubItems.Add("--");
                        break;
                }
                //车牌地区
                switch (struITSPlateResult.struPlateInfo.byRegion)
                {
                    case 0: lviA.SubItems.Add("--");
                        break;
                    case 1: lviA.SubItems.Add("欧洲(EU)");
                        break;
                    case 2: lviA.SubItems.Add("俄语区域(ER)");
                        break;
                    case 3: lviA.SubItems.Add("欧洲&俄罗斯(EU&CIS)");
                        break;
                    case 4: lviA.SubItems.Add("中东(ME)");
                        break;
                    case 0xff: lviA.SubItems.Add("所有");
                        break;
                    default:
                        lviA.SubItems.Add("--");
                        break;
                }
                //检测场景号
                lviA.SubItems.Add(struITSPlateResult.byDetSceneID.ToString());
                //车牌号码
                lviA.SubItems.Add(strLicense);
                //识别车道号
                lviA.SubItems.Add(struITSPlateResult.byDriveChan.ToString());
                //检测方向
                switch (struITSPlateResult.byDir)
                {
                    case 0: lviA.SubItems.Add("--");
                        break;
                    case 1: lviA.SubItems.Add("上行(反向)");
                        break;
                    case 2: lviA.SubItems.Add("下行(正向)");
                        break;
                    case 3: lviA.SubItems.Add("双向");
                        break;
                    case 4: lviA.SubItems.Add("由东向西");
                        break;
                    case 5: lviA.SubItems.Add("由南向北");
                        break;
                    case 6: lviA.SubItems.Add("由西向东");
                        break;
                    case 7: lviA.SubItems.Add("由北向南");
                        break;
                    case 8: lviA.SubItems.Add("其他");
                        break;
                    default:
                        break;
                }
                //详细信息路径
                lviA.SubItems.Add(strShowDataPathA);
                //图片路径
                lviA.SubItems.Add(strPicFilePathA);
                //检测图片路径
                lviA.SubItems.Add(strPicFilePathB);

                ANPRAlarmInfoListView.Items.Insert(0, lviA);
            }
        }

        private void GurdBtn_Click(object sender, EventArgs e)
        {
            //获取当前设备登录方式
            IDeviceTree.EDeviceTreeType deviceType = m_deviceTree.GetDeviceTreeType();
            if (deviceType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                //SDK私有协议交互实现
                SDK_alarmGuard();
            }

        }

        int piccount = 0;

        //车牌报警图片存储路径
        string strPicFilePath = "--";

        //车辆检测报警图片存储路径
        string strDPicFilePath = "--";

        //ANPR展示数据存储路径
        string strShowDataPath = "--";
        //用于展示
        ListViewItem lvi = new ListViewItem();
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

        public void SDK_alarmGuard()
        {
            //获取设备信息
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();

            CHCNetSDK.NET_DVR_SETUPALARM_PARAM struSetupAlarmParam = new CHCNetSDK.NET_DVR_SETUPALARM_PARAM();
            struSetupAlarmParam.dwSize = (uint)Marshal.SizeOf(struSetupAlarmParam);
            struSetupAlarmParam.byLevel = 1;
            struSetupAlarmParam.byAlarmInfoType = 1;

            m_lFortifyHandle = (int)CHCNetSDK.NET_DVR_SetupAlarmChan_V41((int)deviceInfo.lLoginID, ref struSetupAlarmParam);
            if (-1 == m_lFortifyHandle)
            {
                MessageBox.Show("建立布防通道失败！", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                m_falarmData = new CHCNetSDK.MSGCallBack(MsgCallback);
                if (CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V30(m_falarmData, IntPtr.Zero))
                {
                    MessageBox.Show("布防成功！", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("布防失败！", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        //撤防
        private void UNGuardBtn_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceType = m_deviceTree.GetDeviceTreeType();
            if (deviceType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                //SDK私有协议交互实现
                SDK_alarmUNGuard();
            }
            
        }

        //私有协议-撤防
        public void SDK_alarmUNGuard()
        {
            //获取设备信息
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            if (m_lFortifyHandle != -1)
            {
                CHCNetSDK.NET_DVR_CloseAlarmChan_V30(m_lFortifyHandle);
                m_lFortifyHandle = -1;
                MessageBox.Show("撤防成功！", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        //历史记录查看
        private void ANPRAlarmInfoListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string TextPath = ANPRAlarmInfoListView.SelectedItems[0].SubItems[8].Text;
            string PicPath = ANPRAlarmInfoListView.SelectedItems[0].SubItems[9].Text;
            string PicPathB = ANPRAlarmInfoListView.SelectedItems[0].SubItems[10].Text;
            if (PicPath != "" && TextPath != "")
            {
                ANPRDPicBox.Image.Dispose();
                string text = File.ReadAllText(@TextPath);
                ANPRPicBox.Image = Image.FromFile(PicPath);
                ITSPlateInfoTextBox.Text = text;
                if(!PicPathB.Equals("")||!PicPathB.Equals("--"))
                {
                    ANPRDPicBox.Image = Image.FromFile(PicPathB);
                }
            }
        }
    }
}