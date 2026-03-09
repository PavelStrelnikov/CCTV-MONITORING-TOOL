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
    public partial class FormRealTimeThermometry : Form
    {
        public int m_iChannel = -1;
        public int m_lUserID = -1;
        int iRemoteHandle = -1;
        bool bGetRealTimeTherm = false;
        private CHCNetSDK.NET_DVR_REALTIME_THERMOMETRY_COND m_struRealTimeThermCond = new CHCNetSDK.NET_DVR_REALTIME_THERMOMETRY_COND();
        private CHCNetSDK.REMOTECONFIGCALLBACK m_fRealThermData;
        public delegate void UpdateListBoxCallback(IntPtr lpBuffer);
        public FormRealTimeThermometry()
        {
            InitializeComponent();
            comboBoxRemoteMode.SelectedIndex = 0;

            if(m_fRealThermData == null)
            {
                m_fRealThermData = new CHCNetSDK.REMOTECONFIGCALLBACK(GetRealtimeThermometryCallback);
            }
        }

        public void GetRealtimeThermometryCallback(uint dwType, IntPtr lpBuffer, uint dwBufLen, IntPtr pUserData)
        {
            if (dwType == (uint)CHCNetSDK.NET_SDK_CALLBACK_TYPE.NET_SDK_CALLBACK_TYPE_DATA)
            {
                //创建该控件的主线程直接更新信息列表 
                UpdateClientList(lpBuffer);
            }
        }

        public delegate void UpdateUI(IntPtr lpBuffer);

        public void UpdateClientList(IntPtr lpBuffer)
        {
            if (InvokeRequired)
            {
                UpdateUI ui = new UpdateUI(UpdateClientList);//实例化委托对象
                Invoke(ui, lpBuffer);
                return;
            }
            else
            {

                CHCNetSDK.NET_DVR_THERMOMETRY_UPLOAD struThermUpLoad = new CHCNetSDK.NET_DVR_THERMOMETRY_UPLOAD();
                struThermUpLoad = (CHCNetSDK.NET_DVR_THERMOMETRY_UPLOAD)Marshal.PtrToStructure(lpBuffer, typeof(CHCNetSDK.NET_DVR_THERMOMETRY_UPLOAD));
                uint dwRelativeTime = struThermUpLoad.dwRelativeTime;
                uint dwAbsTime = struThermUpLoad.dwAbsTime;
                char[] szRuleName = struThermUpLoad.szRuleName;
                string strRuleName = new string(szRuleName);
                uint byRuleID = struThermUpLoad.byRuleID;
                ushort wPresetNo = struThermUpLoad.wPresetNo;
                byte byRuleCalibType = struThermUpLoad.byRuleCalibType;

                Single fTemperature = 0.0F;
                string strPoint = "";
                if (0 == byRuleCalibType)
                {
                    fTemperature = struThermUpLoad.struPointThermCfg.fTemperature;
                    strPoint = "" + byRuleCalibType + "X:" + struThermUpLoad.struPointThermCfg.struPoint.fX + "Y:" + struThermUpLoad.struPointThermCfg.struPoint.fY;
                }
                string strLinePolygon = "";
                Single fMaxTemperature = 0.0F;
                Single fMinTemperature = 0.0F;
                Single fAverageTemperature = 0.0F;
                Single fTemperatureDiff = 0.0F;
                if (1 == byRuleCalibType || 2 == byRuleCalibType)
                {
                    fMaxTemperature = struThermUpLoad.struLinePolygonThermCfg.fMaxTemperature;
                    fMinTemperature = struThermUpLoad.struLinePolygonThermCfg.fMinTemperature;
                    fAverageTemperature = struThermUpLoad.struLinePolygonThermCfg.fAverageTemperature;
                    fTemperatureDiff = struThermUpLoad.struLinePolygonThermCfg.fTemperatureDiff;

                    int iPointNum = (int)struThermUpLoad.struLinePolygonThermCfg.struRegion.dwPointNum;
                    for (int i = 0; i < iPointNum; i++)
                    {
                        float fX = struThermUpLoad.struLinePolygonThermCfg.struRegion.struPos[i].fX;
                        float fY = struThermUpLoad.struLinePolygonThermCfg.struRegion.struPos[i].fY;
                        strLinePolygon = strLinePolygon + "X" + i + 1 + ":" + fX + "Y" + i + 1 + ":" + fY;
                    }
                    strLinePolygon = "" + byRuleCalibType + strLinePolygon;
                }

                string strThermometryUnit = null;
                if (0 == struThermUpLoad.byThermometryUnit)
                {
                    strThermometryUnit = "摄氏度";
                }
                if (1 == struThermUpLoad.byThermometryUnit)
                {
                    strThermometryUnit = "华氏度";
                }
                if (2 == struThermUpLoad.byThermometryUnit)
                {
                    strThermometryUnit = "开尔文";
                }

                string strDataType = null;
                if (0 == struThermUpLoad.byDataType)
                {
                    strDataType = "检测中";
                }
                if (1 == struThermUpLoad.byDataType)
                {
                    strDataType = "开始";
                }
                if (2 == struThermUpLoad.byDataType)
                {
                    strDataType = "结束";
                }

                Single fCenterPointTemperature = 0.0F;
                Single fHighestPointTemperature = 0.0F;
                Single fLowestPointTemperature = 0.0F;
                if (1 == ((struThermUpLoad.bySpecialPointThermType >> 0) & 0x01))
                {
                    fCenterPointTemperature = struThermUpLoad.fCenterPointTemperature;
                }
                if (1 == ((struThermUpLoad.bySpecialPointThermType >> 1) & 0x01))
                {
                    fHighestPointTemperature = struThermUpLoad.fHighestPointTemperature;
                }
                if (1 == ((struThermUpLoad.bySpecialPointThermType >> 2) & 0x01))
                {
                    fLowestPointTemperature = struThermUpLoad.fLowestPointTemperature;
                }

                //if (this.InvokeRequired)
                //{
                //    this.BeginInvoke(new UpdateListBoxCallback(UpdateClientList), lpBuffer);
                //}
                //else
                //{
                //    if (listViewRemote.Items.Count > 20)
                //    {
                //        listViewRemote.Items.RemoveAt(0);
                //    }

                //    listViewRemote.Items.Insert(0, new ListViewItem(new string[] { ("" + dwRelativeTime), ("" + dwAbsTime),(strRuleName),("" + byRuleID),
                //                    ("" + byRuleCalibType), ("" + wPresetNo ),(strPoint),("" + fTemperature),(strLinePolygon),("" + fMaxTemperature),("" + fMinTemperature),("" + fAverageTemperature),
                //                    ("" + fTemperatureDiff),(strThermometryUnit), (strDataType), ("" + fCenterPointTemperature), ("" + fHighestPointTemperature),
                //                    ("" + fLowestPointTemperature )}));
                //}


                listViewRemote.Items.Add(new ListViewItem(new string[] { ("" + dwRelativeTime), ("" + dwAbsTime),(strRuleName),("" + byRuleID),
                                ("" + byRuleCalibType), ("" + wPresetNo ),(strPoint),("" + fTemperature),(strLinePolygon),("" + fMaxTemperature),("" + fMinTemperature),("" + fAverageTemperature),
                                ("" + fTemperatureDiff),(strThermometryUnit), (strDataType), ("" + fCenterPointTemperature), ("" + fHighestPointTemperature),
                                ("" + fLowestPointTemperature )}));
            }
        }

        private void btnGetRealTimeThermometry_Click(object sender, EventArgs e)
        {
            if(bGetRealTimeTherm)
            {
                if(!CHCNetSDK.NET_DVR_StopRemoteConfig(iRemoteHandle))
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "Thermal：停止获取实时测温数据置失败，错误码：" + iLastErr ;
                    MessageBox.Show(strErr);
                }
                else
                {
                    btnGetRealTimeThermometry.Text = "获取";
                    bGetRealTimeTherm = false;
                }
            }
            else
            {
                int iInSize = Marshal.SizeOf(m_struRealTimeThermCond);
                m_struRealTimeThermCond.dwSize = (uint)iInSize;
                m_struRealTimeThermCond.dwChan = (uint)m_iChannel;
                m_struRealTimeThermCond.byRuleID = byte.Parse(textBoxAlarmID.Text);
                m_struRealTimeThermCond.byMode = (byte)comboBoxRemoteMode.SelectedIndex;
                IntPtr lpInBuffer = Marshal.AllocHGlobal(iInSize);
                Marshal.StructureToPtr(m_struRealTimeThermCond, lpInBuffer, false);

                iRemoteHandle = CHCNetSDK.NET_DVR_StartRemoteConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_REALTIME_THERMOMETRY, lpInBuffer, iInSize, m_fRealThermData, IntPtr.Zero);

                if (iRemoteHandle < 0)
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "Thermal：获取实时测温数据置失败，错误码：" + iLastErr ;
                    MessageBox.Show(strErr);
                }
                else
                {
                    btnGetRealTimeThermometry.Text = "停止获取";
                    bGetRealTimeTherm = true;
                }
            }

        }

        private void FormRealTimeThermometry_Load(object sender, EventArgs e)
        {
            //Control.CheckForIllegalCrossThreadCalls = false;
        }
    }
}
