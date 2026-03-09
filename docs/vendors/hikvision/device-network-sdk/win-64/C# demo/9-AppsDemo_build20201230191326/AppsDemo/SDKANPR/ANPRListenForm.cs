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
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Web;
using TINYXMLTRANS;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;

namespace SDKANPR
{
    public partial class ANPRListenForm : Form
    {
        public int m_iChannel = -1;
        public int m_lUserID = -1;
        private Int32 m_iListenHandle = -1;
        private CHCNetSDK.MSGCallBack m_falarmData = null;
        public delegate void UpdateListBoxCallback(string strAlarmTime, string strDevIP, string strAlarmMsg);

        public ANPRListenForm()
        {
            InitializeComponent();
            
            //设置报警回调函数
            if (m_falarmData == null)
            {
                m_falarmData = new CHCNetSDK.MSGCallBack(AlarmMessage);
            }
            CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V31(m_falarmData, IntPtr.Zero);

            Control.CheckForIllegalCrossThreadCalls = false;

        }

        //设备树对象映射
        IDeviceTree m_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        //本地存储路径
        public string strLocalFilePath;
        private IDeviceTree g_deviceTree = PluginsFactory.GetDeviceTreeInstance();

        private void ANPRListenForm_Load(object sender, EventArgs e)
        {
            //engineIDcomboBox.SelectedIndex = 0;
            strLocalFilePath = Application.StartupPath + "\\ANPR\\" + DateTime.Now.ToString("yyyy-MM-dd");
            if (!Directory.Exists(strLocalFilePath))
                Directory.CreateDirectory(strLocalFilePath);

        }

        public void AlarmMessage(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            switch (lCommand)
            {
                // 车牌识别报警上传
                case CHCNetSDK.COMM_UPLOAD_PLATE_RESULT:
                    break;
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

//             if (this.InvokeRequired)
//             {
//                 ProcessCommAlarm_ITS_Handle handle = new ProcessCommAlarm_ITS_Handle(ProcessCommAlarm_ITS);
//                 this.BeginInvoke(handle, pAlarmer, pAlarmInfo, dwBufLen, pUser);
//             }
//             else
//             {
            //pAlarmInfo数据异常导致无法解析到正常数据，注释掉if...else...后正常


                //获取当前系统时间
                string datatimenow = DateTime.Now.ToString("yyyyMMddhhmmssfff");
                //车牌图片存储路径
                string strPicFilePath = "";
                //车辆检测图片存储路径
                string strPicFilePathB = "";
                //ANPR展示数据存储路径
                string strShowDataPath = "";

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
                            if (struITSPlateResult.struPicInfo[i].byType == 0)
                            {
                                strPicFilePath = strLocalFilePath + "\\ANPRListenInfo" + datatimenow + "Pic.jpg";
                                int iLen = (int)struITSPlateResult.struPicInfo[i].dwDataLen;
                                byte[] byDataTempBuffer = new byte[iLen];
                                Marshal.Copy(struITSPlateResult.struPicInfo[i].pBuffer, byDataTempBuffer, 0, iLen);
                                Image img = System.Drawing.Image.FromStream(new System.IO.MemoryStream(byDataTempBuffer));
                                Bitmap bmpImage = new System.Drawing.Bitmap(img);
                                bmpImage.Save(strPicFilePath);
                                ANPRPicBox.Image = Image.FromFile(strPicFilePath);
                                img.Dispose();
                                bmpImage.Dispose();
                            }
                            else if (struITSPlateResult.struPicInfo[i].byType == 1)
                            {
                                strPicFilePathB = strLocalFilePath + "\\ANPRListenInfo" + datatimenow + "DPic.jpg";
                                int iLen = (int)struITSPlateResult.struPicInfo[i].dwDataLen;
                                byte[] byDataTempBuffer = new byte[iLen];
                                Marshal.Copy(struITSPlateResult.struPicInfo[i].pBuffer, byDataTempBuffer, 0, iLen);
                                Image img = System.Drawing.Image.FromStream(new System.IO.MemoryStream(byDataTempBuffer));
                                Bitmap bmpImage = new System.Drawing.Bitmap(img);
                                bmpImage.Save(strPicFilePathB);
                                ANPRDPicBox.Image = Image.FromFile(strPicFilePathB);
                                img.Dispose();
                                bmpImage.Dispose();
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
                    strShowDataPath = strLocalFilePath + "\\ANPRListenInfo" + datatimenow + "ShowData.txt";
                    FileStream fs = new FileStream(strShowDataPath, FileMode.Create);
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
                    strShowDataPath = strLocalFilePath + "\\ANPRListenInfo" + datatimenow + "ShowData.txt";
                    //File.Create(strShowDataPath);
                    File.WriteAllText(@strShowDataPath, InfoPlateResult);
                }


                //车牌报警信息展示
                ListViewItem lvi = new ListViewItem();
                //报警IP
                lvi.Text = deviceInfo.sDeviceIP;
                //报警触发时间
                lvi.SubItems.Add(datatimenow);
                //车辆类型
                switch (struITSPlateResult.byVehicleType)
                {
                    case 0: lvi.SubItems.Add("未知");
                        break;
                    case 1: lvi.SubItems.Add("客车");
                        break;
                    case 2: lvi.SubItems.Add("货车");
                        break;
                    case 3: lvi.SubItems.Add("轿车");
                        break;
                    case 4: lvi.SubItems.Add("面包车");
                        break;
                    case 5: lvi.SubItems.Add("小货车");
                        break;
                    default: lvi.SubItems.Add("--");
                        break;
                }
                //车牌地区
                switch (struITSPlateResult.struPlateInfo.byRegion)
                {
                    case 0: lvi.SubItems.Add("--");
                        break;
                    case 1: lvi.SubItems.Add("欧洲(EU)");
                        break;
                    case 2: lvi.SubItems.Add("俄语区域(ER)");
                        break;
                    case 3: lvi.SubItems.Add("欧洲&俄罗斯(EU&CIS)");
                        break;
                    case 4: lvi.SubItems.Add("中东(ME)");
                        break;
                    case 0xff: lvi.SubItems.Add("所有");
                        break;
                    default:
                        lvi.SubItems.Add("--");
                        break;
                }
                //检测场景号
                lvi.SubItems.Add(struITSPlateResult.byDetSceneID.ToString());
                //车牌号码
                lvi.SubItems.Add(strLicense);
                //识别车道号
                lvi.SubItems.Add(struITSPlateResult.byDriveChan.ToString());
                //检测方向
                switch (struITSPlateResult.byDir)
                {
                    case 0: lvi.SubItems.Add("--");
                        break;
                    case 1: lvi.SubItems.Add("上行(反向)");
                        break;
                    case 2: lvi.SubItems.Add("下行(正向)");
                        break;
                    case 3: lvi.SubItems.Add("双向");
                        break;
                    case 4: lvi.SubItems.Add("由东向西");
                        break;
                    case 5: lvi.SubItems.Add("由南向北");
                        break;
                    case 6: lvi.SubItems.Add("由西向东");
                        break;
                    case 7: lvi.SubItems.Add("由北向南");
                        break;
                    case 8: lvi.SubItems.Add("其他");
                        break;
                    default:
                        break;
                }
                //详细信息路径
                lvi.SubItems.Add(strShowDataPath);
                //车牌检测图片路径
                lvi.SubItems.Add(strPicFilePath);
                //车辆检测图片路径
                lvi.SubItems.Add(strPicFilePathB);

                ANPRAlarmInfoListView.Items.Insert(0, lvi);
//            }
            
        }

        private void StartListenBtn_Click(object sender, EventArgs e)
        {

            //获取当前设备登录方式
            IDeviceTree.EDeviceTreeType deviceType = m_deviceTree.GetDeviceTreeType();
            if (deviceType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                //SDK私有协议交互实现
                SDK_Listen();
            }
        }

        private void SDK_Listen()
        {
            //获取设备信息
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();

            if (ListenIPTextBox.Text == null || ListenPortTextBox.Text == null)
            {
                return;
            }
            string sLocalIP = ListenIPTextBox.Text;
            ushort wLocalPort = ushort.Parse(ListenPortTextBox.Text);

            CHCNetSDK.NET_DVR_NETCFG_V30 struNetCfg = new CHCNetSDK.NET_DVR_NETCFG_V30();
            struNetCfg.init();

            uint dwReturned = 0;
            uint dwSize = (uint)Marshal.SizeOf(struNetCfg);
            IntPtr ptrNetCfg = Marshal.AllocHGlobal((int)dwSize);
            Marshal.StructureToPtr(struNetCfg, ptrNetCfg, false);

            if (!CHCNetSDK.NET_DVR_GetDVRConfig((int)deviceInfo.lLoginID, CHCNetSDK.NET_DVR_GET_NETCFG_V30, 0, ptrNetCfg, dwSize, ref dwReturned))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
            }

            struNetCfg = (CHCNetSDK.NET_DVR_NETCFG_V30)Marshal.PtrToStructure(ptrNetCfg, typeof(CHCNetSDK.NET_DVR_NETCFG_V30));
            struNetCfg.dwSize = (uint)Marshal.SizeOf(struNetCfg);
            struNetCfg.struAlarmHostIpAddr.sIpV4 = sLocalIP;
            struNetCfg.wAlarmHostIpPort = wLocalPort;

            int iInBufferSize = Marshal.SizeOf(struNetCfg);
            IntPtr pInBuffer = Marshal.AllocHGlobal(iInBufferSize);
            Marshal.StructureToPtr(struNetCfg, pInBuffer, false);

            if (!CHCNetSDK.NET_DVR_SetDVRConfig((int)deviceInfo.lLoginID, CHCNetSDK.NET_DVR_SET_NETCFG_V30, deviceInfo.iDeviceChanNum, pInBuffer, (uint)iInBufferSize))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
            }
            Marshal.FreeHGlobal(ptrNetCfg);
            Marshal.FreeHGlobal(pInBuffer);


            if (m_iListenHandle < 0)
            {               
                try
                {                 
                    if (m_falarmData == null)
                    {
                        m_falarmData = new CHCNetSDK.MSGCallBack(AlarmMessage);
                    }

                    m_iListenHandle = CHCNetSDK.NET_DVR_StartListen_V30(sLocalIP, wLocalPort, m_falarmData, IntPtr.Zero);

                    if (m_iListenHandle < 0)
                    {
                        int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                        string strErr = "启动监听失败，错误码：" + iLastErr; //启动监听失败，输出错误码
                        MessageBox.Show(strErr);
                    }
                    else
                    {
                        MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_StartListen_V30", "启动监听成功！");
                        MessageBox.Show("NET_DVR_StartListen_V30 ：启动监听成功！");
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
                
            }
        }

        public class ContantData
        {
            public string name { get; set; }
            public string filename { get; set; }
            public string ContentType { get; set; }
            public byte[] Content { get; set; }
        }

        /// <summary>
        /// 匹配相同的子byte数组
        /// </summary>
        /// <param name="src">目标byte序列</param>
        /// <param name="index">从目标序列的index位置开始匹配</param>
        /// <param name="value">用来匹配的序列</param>
        /// <returns>参数错误或未匹配到返回-1，否则返回value在src上出现的位置</returns>
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

        private void StopListenBtn_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceType = m_deviceTree.GetDeviceTreeType();
            if (deviceType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                //SDK私有协议交互实现
                SDK_StopListen();
            }

        }

        private void SDK_StopListen()
        {
            if (m_iListenHandle >= 0)
            {
                if (!CHCNetSDK.NET_DVR_StopListen_V30(m_iListenHandle))
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "停止监听失败，错误码：" + iLastErr; //撤防失败，输出错误码
                    MessageBox.Show(strErr);
                }
                else
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "Thermometry", "停止监听成功！");
                    m_iListenHandle = -1;
                }
            }
        }

        private void ANPRAlarmInfoListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string TextPath = ANPRAlarmInfoListView.SelectedItems[0].SubItems[8].Text;
            string PicPath = ANPRAlarmInfoListView.SelectedItems[0].SubItems[9].Text;
            string PicPathB = ANPRAlarmInfoListView.SelectedItems[0].SubItems[10].Text;
            if (TextPath != "" && TextPath != "--")
            {
                string text = File.ReadAllText(@TextPath);
                ITSPlateInfoTextBox.Text = text;
            }
            if (PicPath != "" &&PicPath != "--")
            {
                ANPRPicBox.Image = Image.FromFile(PicPath);
            }
            if (!PicPathB.Equals("") || !PicPathB.Equals("--"))
            {
                ANPRDPicBox.Image = Image.FromFile(PicPathB);
            }
        }

    }
}
