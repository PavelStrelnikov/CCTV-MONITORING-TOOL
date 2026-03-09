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

namespace SDKThermometry
{
    public partial class FormThermometryAlarm : Form
    {
        public int m_iChannel = -1;
        public int m_lUserID = -1;
        private Int32 m_iListenHandle = -1;
        private CHCNetSDK.MSGCallBack m_falarmData = null;
        public delegate void UpdateListBoxCallback(string strAlarmTime, string strDevIP, string strAlarmMsg);

        public FormThermometryAlarm()
        {
            InitializeComponent();

            //设置报警回调函数
            if (m_falarmData == null)
            {
                m_falarmData = new CHCNetSDK.MSGCallBack(AlarmMessage);
            }
            CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V31(m_falarmData, IntPtr.Zero);
        }

        //回调
        public void AlarmMessage(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            string strAlarmInfo;

            switch (lCommand)
            {
                // 温度检测报警
                case CHCNetSDK.COMM_THERMOMETRY_ALARM:
                    {
                        CHCNetSDK.NET_DVR_THERMOMETRY_ALARM struThermometryAlarm = new CHCNetSDK.NET_DVR_THERMOMETRY_ALARM();
                        struThermometryAlarm = (CHCNetSDK.NET_DVR_THERMOMETRY_ALARM)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_DVR_THERMOMETRY_ALARM));

                        if (0 == struThermometryAlarm.byRuleCalibType)
                        {
                            strAlarmInfo = "" + "Channel:" + struThermometryAlarm.dwChannel + "ThermometryUnit" + struThermometryAlarm.byThermometryUnit +
                                         "PresetNo:" + struThermometryAlarm.wPresetNo + "PTZ Info[Pan:" + struThermometryAlarm.struPtzInfo.fPan + "Tilt:" +
                                         struThermometryAlarm.struPtzInfo.fTilt + "Zoom:" + struThermometryAlarm.struPtzInfo.fZoom + "AlarmLevel:" +
                                         struThermometryAlarm.byAlarmLevel + "AlarmType:" + struThermometryAlarm.byAlarmType + "AlarmRule:" +
                                         struThermometryAlarm.byAlarmRule + "RuleCalibType:" + struThermometryAlarm.byRuleCalibType + "Point[x:" +
                                         struThermometryAlarm.struPoint.fX + "y:" + struThermometryAlarm.struPoint.fY + "]" + "RuleTemperature:" +
                                         struThermometryAlarm.fRuleTemperature.ToString("#0.0") + "CurrTemperature:" + struThermometryAlarm.fCurrTemperature.ToString("#0.0") +
                                         "PicLen:" + struThermometryAlarm.dwPicLen + "ThermalPicLen:" + struThermometryAlarm.dwThermalPicLen + "ThermalInfoLen:"
                                         + struThermometryAlarm.dwThermalInfoLen + "\r\n";
                        }
                        else if (1 == struThermometryAlarm.byRuleCalibType || 2 == struThermometryAlarm.byRuleCalibType)
                        {
                            int iPointNum = (int)struThermometryAlarm.struRegion.dwPointNum;
                            string strRegionInfo = "";
                            for (int i = 0; i < iPointNum; i++)
                            {
                                float fX = struThermometryAlarm.struRegion.struPos[i].fX;
                                float fY = struThermometryAlarm.struRegion.struPos[i].fY;
                                strRegionInfo = "" + (iPointNum + 1) + ":" + fX + (iPointNum + 1) + ":" + fY;
                            }

                            strAlarmInfo = "" + "Channel:" + struThermometryAlarm.dwChannel + "ThermometryUnit" + struThermometryAlarm.byThermometryUnit +
                                        "PresetNo:" + struThermometryAlarm.wPresetNo + "PTZ Info[Pan:" + struThermometryAlarm.struPtzInfo.fPan + "Tilt:" +
                                        struThermometryAlarm.struPtzInfo.fTilt + "Zoom:" + struThermometryAlarm.struPtzInfo.fZoom + "AlarmLevel:" +
                                        struThermometryAlarm.byAlarmLevel + "AlarmType:" + struThermometryAlarm.byAlarmType + "AlarmRule:" +
                                        struThermometryAlarm.byAlarmRule + "RuleCalibType:" + struThermometryAlarm.byRuleCalibType + "Region" + strRegionInfo +
                                        "PicLen:" + struThermometryAlarm.dwPicLen + "ThermalPicLen:" + struThermometryAlarm.dwThermalPicLen + "ThermalInfoLen:"
                                        + struThermometryAlarm.dwThermalInfoLen + "HighestPoint [X:" + struThermometryAlarm.struHighestPoint.fX + "Y:" +
                                        struThermometryAlarm.struHighestPoint.fY + "]" + "HighestTemperature:" + struThermometryAlarm.fHighestTemperature.ToString("#0.0") +
                                        "LowestPoint [X:" + struThermometryAlarm.struLowestPoint.fX + "Y:" + struThermometryAlarm.struLowestPoint.fY + "]" +
                                        "LowestTemperature:" + struThermometryAlarm.fLowestTemperature.ToString("#0.0") + "\r\n";
                        }
                        break;
                    }

                // 温差报警
                case CHCNetSDK.COMM_THERMOMETRY_DIFF_ALARM:
                    {
                        CHCNetSDK.NET_DVR_THERMOMETRY_DIFF_ALARM struThermometryDiffAlarm = new CHCNetSDK.NET_DVR_THERMOMETRY_DIFF_ALARM();
                        struThermometryDiffAlarm = (CHCNetSDK.NET_DVR_THERMOMETRY_DIFF_ALARM)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_DVR_THERMOMETRY_DIFF_ALARM));

                        if (0 == struThermometryDiffAlarm.byRuleCalibType)
                        {
                            strAlarmInfo = "" + "Channel:" + struThermometryDiffAlarm.dwChannel + "AlarmID1:" + struThermometryDiffAlarm.byAlarmID1 +
                                        "AlarmID2:" + struThermometryDiffAlarm.byAlarmID2 +
                                        "PresetNo:" + struThermometryDiffAlarm.wPresetNo + "PTZ Info[Pan:" + struThermometryDiffAlarm.struPtzInfo.fPan + "Tilt:" +
                                        struThermometryDiffAlarm.struPtzInfo.fTilt + "Zoom:" + struThermometryDiffAlarm.struPtzInfo.fZoom + "AlarmLevel:" +
                                        struThermometryDiffAlarm.byAlarmLevel + "AlarmType:" + struThermometryDiffAlarm.byAlarmType + "AlarmRule:" +
                                        struThermometryDiffAlarm.byAlarmRule + "RuleCalibType:" + struThermometryDiffAlarm.byRuleCalibType + "Point1[x:" +
                                        struThermometryDiffAlarm.struPoint[0].fX + "y:" + struThermometryDiffAlarm.struPoint[0].fY + "]" + "Point1[x:" +
                                        struThermometryDiffAlarm.struPoint[1].fX + "y:" + struThermometryDiffAlarm.struPoint[1].fY + "]" + "RuleTemperatureDiff:" +
                                        struThermometryDiffAlarm.fRuleTemperatureDiff.ToString("#0.0") + "CurTemperatureDiff:" + struThermometryDiffAlarm.fCurTemperatureDiff.ToString("#0.0") +
                                        "PicLen:" + struThermometryDiffAlarm.dwPicLen + "ThermalPicLen:" + struThermometryDiffAlarm.dwThermalPicLen + "ThermalInfoLen:"
                                        + struThermometryDiffAlarm.dwThermalInfoLen + "ThermometryUnit" + struThermometryDiffAlarm.byThermometryUnit + "\r\n";
                        }
                        else if (1 == struThermometryDiffAlarm.byRuleCalibType || 2 == struThermometryDiffAlarm.byRuleCalibType)
                        {
                            int i = 0;
                            int iPointNum = (int)struThermometryDiffAlarm.struRegion[0].dwPointNum;
                            string strRegionInfo = "";
                            for (i = 0; i < iPointNum; i++)
                            {
                                float fX = struThermometryDiffAlarm.struRegion[0].struPos[i].fX;
                                float fY = struThermometryDiffAlarm.struRegion[0].struPos[i].fY;
                                strRegionInfo = "" + (iPointNum + 1) + ":" + fX + (iPointNum + 1) + ":" + fY;
                            }
                            iPointNum = (int)struThermometryDiffAlarm.struRegion[1].dwPointNum;
                            string strRegionInfo1 = "";
                            for (i = 0; i < iPointNum; i++)
                            {
                                float fX = struThermometryDiffAlarm.struRegion[1].struPos[i].fX;
                                float fY = struThermometryDiffAlarm.struRegion[1].struPos[i].fY;
                                strRegionInfo1 = "" + (iPointNum + 1) + ":" + fX + (iPointNum + 1) + ":" + fY;
                            }
                            strAlarmInfo = "" + "Channel:" + struThermometryDiffAlarm.dwChannel + "AlarmID1:" + struThermometryDiffAlarm.byAlarmID1 +
                                        "AlarmID2:" + struThermometryDiffAlarm.byAlarmID2 + "ThermometryUnit" + struThermometryDiffAlarm.byThermometryUnit +
                                        "PresetNo:" + struThermometryDiffAlarm.wPresetNo + "PTZ Info[Pan:" + struThermometryDiffAlarm.struPtzInfo.fPan + "Tilt:" +
                                        struThermometryDiffAlarm.struPtzInfo.fTilt + "Zoom:" + struThermometryDiffAlarm.struPtzInfo.fZoom + "AlarmLevel:" +
                                        struThermometryDiffAlarm.byAlarmLevel + "AlarmType:" + struThermometryDiffAlarm.byAlarmType + "AlarmRule:" +
                                        struThermometryDiffAlarm.byAlarmRule + "RuleCalibType:" + struThermometryDiffAlarm.byRuleCalibType + "Region1:" + strRegionInfo +
                                        "Region2:" + strRegionInfo1 + "RuleTemperatureDiff:" +struThermometryDiffAlarm.fRuleTemperatureDiff.ToString("#0.0") + "CurTemperatureDiff:" +
                                        struThermometryDiffAlarm.fCurTemperatureDiff.ToString("#0.0") + "PicLen:" + struThermometryDiffAlarm.dwPicLen + "ThermalPicLen:" +
                                        struThermometryDiffAlarm.dwThermalPicLen + "ThermalInfoLen:" + struThermometryDiffAlarm.dwThermalInfoLen  + "\r\n";
                        }

                        break;
                    }
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

        // 委托事件，实时更新报警信息
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

        //布防
        private void btnThermometryAlarm_Click(object sender, EventArgs e)
        {
            if (m_lUserID < 0)
            {
                MessageBox.Show("请先登录!");
            }
            else
            {
                CHCNetSDK.NET_DVR_SETUPALARM_PARAM struAlarmParam = new CHCNetSDK.NET_DVR_SETUPALARM_PARAM();

                struAlarmParam.dwSize = (uint)Marshal.SizeOf(struAlarmParam);
                struAlarmParam.byLevel = 1; // 0- 一级布防,1- 二级布防
                struAlarmParam.byAlarmInfoType = 1;// 智能交通设备有效，新报警信息类型
                struAlarmParam.byFaceAlarmDetection = 1;// 1-人脸侦测

                long lAlarmHandle = CHCNetSDK.NET_DVR_SetupAlarmChan_V41(m_lUserID, ref struAlarmParam);
                if (lAlarmHandle < 0)
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "布防失败，错误码：" + iLastErr  ; //布防失败，输出错误码
                    MessageBox.Show(strErr);
                }
                else
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "Thermometry", "布防成功！");
                    MessageBox.Show("Thermometry : 布防成功！");
                }

            }
        }

        private void btnStopThermometryAlarm_Click(object sender, EventArgs e)
        {

            if (!CHCNetSDK.NET_DVR_CloseAlarmChan_V30(m_lUserID))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "撤防失败，错误码：" + iLastErr  ; //撤防失败，输出错误码
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("Thermometry ：撤防成功！");
                MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "Thermometry", "撤防成功！");
            }

        }

        //监听
        private void btnThermometryListen_Click(object sender, EventArgs e)
        {
            
                if (m_iListenHandle >= 0)
                {
                    if (!CHCNetSDK.NET_DVR_StopListen_V30(m_iListenHandle))
                    {
                        int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                        string strErr = "停止监听失败，错误码：" + iLastErr  ; //撤防失败，输出错误码
                        MessageBox.Show(strErr);
                    }
                    else
                    {
                        MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "Thermometry", "停止监听成功！");
                        m_iListenHandle = -1;
                        btnThermometryListen.Text = "监听";
                    }
                }
                else
                {
                    string sLocalIP = textBoxListenIP.Text;
                    ushort wLocalPort = ushort.Parse(textBoxListenPort.Text);

                    if (m_falarmData == null)
                    {
                        m_falarmData = new CHCNetSDK.MSGCallBack(AlarmMessage);
                    }

                    m_iListenHandle = CHCNetSDK.NET_DVR_StartListen_V30(sLocalIP, wLocalPort, m_falarmData, IntPtr.Zero);

                    if (m_iListenHandle < 0)
                    {
                        int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                        string strErr = "启动监听失败，错误码：" + iLastErr  ; //启动监听失败，输出错误码
                        MessageBox.Show(strErr);
                    }
                    else
                    {
                        MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "Thermometry", "启动监听成功！");
                        MessageBox.Show("Thermometry ：启动监听成功！");
                        btnThermometryListen.Text = "停止监听";
                    }
                }
            }

    }
}
