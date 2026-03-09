using Common;
using Common.Head;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using TINYXMLTRANS;
using System.Collections.Concurrent;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;
namespace SDKPerimeterPrecaution
{
    public partial class FormPerimeterPrecaution : PluginsControl
    {
        private IDeviceTree g_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        private string strParam = "";
        private int m_iDeviceIndex = -1;
        private int m_iCurDeviceIndex = -1;
        private int m_iCurChanNo = -1;
        private int m_iCurChanIndex = -1;
        private int m_iRegionIndex = -1;
        private int m_lUserID = -1;
        private Int32 m_iPort = -1;
        private IntPtr m_pUser = new IntPtr();
        private int m_lRealHandle = -1;
        private bool m_bPlay = false;
        private int lPort = -1;
        private IDeviceTree.DeviceInfo m_deviceInfo = null;
        private IDeviceTree.ChannelInfo m_channelInfo = null;
        private CHCNetSDK.NET_DVR_PREVIEWINFO m_struPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
        private CHCNetSDK.NET_DVR_REGION_ENTRANCE_DETECTION m_struRegionEntrDetection = new CHCNetSDK.NET_DVR_REGION_ENTRANCE_DETECTION();
        private CHCNetSDK.NET_VCA_TRAVERSE_PLANE_DETECTION m_struLineDetection = new CHCNetSDK.NET_VCA_TRAVERSE_PLANE_DETECTION();
        private CHCNetSDK.NET_DVR_REGION_EXITING_DETECTION m_struRegionExitDetection = new CHCNetSDK.NET_DVR_REGION_EXITING_DETECTION();
        private CHCNetSDK.NET_VCA_FIELDDETECION m_struFieldDetection = new CHCNetSDK.NET_VCA_FIELDDETECION();
        private bool bRegionEnterFlag = false;
        private bool bLineDetectionFlag = false;
        private bool bRegionExitingFlag = false;
        private bool bFieldDetectionFlag = false;
        private int m_iPointNum = 0;
        private List<Point> m_points = new List<Point>();
        private VCA_POLYGON m_struPolygon = new VCA_POLYGON();
        private VCA_POINT m_struPoint = new VCA_POINT();
        private PlayCtrl.DRAWFUN fDrawFun = null;
        private CHCNetSDK.DRAWFUN fDrawFunc = null;

        private const int VCA_MAX_POLYGON_POINT_NUM = 10;//The detection area supports polygons with up to 10 points
        private struct VCA_POLYGON
        {
            public uint dwPointNum; //The effective point is greater than or equal to 3. If 3 points are on a line, it is considered to be an invalid area
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = VCA_MAX_POLYGON_POINT_NUM, ArraySubType = System.Runtime.InteropServices.UnmanagedType.Struct)]
            public VCA_POINT[] struPos; //Polygon boundary points, up to 10
        }
        private struct VCA_POINT
        {
            public float fX;             // X, 0.001 to 1
            public float fY;             //Y, 0.001 to 1
        }

        public FormPerimeterPrecaution()
        {
            InitializeComponent();
            m_struRegionEntrDetection.struRegion = new CHCNetSDK.NET_DVR_REGIONENTRANCE_REGION[4];
            m_struLineDetection.struAlertParam = new CHCNetSDK.NET_VCA_TRAVERSE_PLANE[8];
            m_struRegionExitDetection.struRegion = new CHCNetSDK.NET_DVR_REGIONEXITING_REGION[4];
            m_struFieldDetection.struIntrusion = new CHCNetSDK.NET_VCA_INTRUSION[8];
            if (g_deviceTree != null)
            {
                this.GetDevicesInfo();
            }
            this.GetLoginInfo();
        }

        private void GetDevicesInfo()
        {
            if (g_deviceTree != null)
            {
                IDeviceTree.EDeviceTreeType deviceTreeType = g_deviceTree.GetDeviceTreeType();
                if (deviceTreeType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
                {
                    this.m_deviceInfo = g_deviceTree.GetSelectedDeviceInfo();
                    this.m_channelInfo = g_deviceTree.GetSelectedChannelInfo();
                }
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

        //SDK preview
        private void SDK_StartPreview()
        {
            m_struPreviewInfo.lChannel = m_iCurChanNo;
            m_struPreviewInfo.dwStreamType = 0;
            m_struPreviewInfo.bBlocked = true;
            m_struPreviewInfo.DisplayBufNum = 0;
            m_struPreviewInfo.hPlayWnd = panelPlay.Handle;
            if (m_bPlay)
            {
                bool bStopPlay = CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
                if (!bStopPlay)
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKPreview", "NET_DVR_StopRealPlay");
                }
                else
                {
                    m_bPlay = false;
                    panelPlay.Refresh();
                }
            }
            else
            {
                m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref m_struPreviewInfo, null, m_pUser);
                if (m_lRealHandle < 0)
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "PerimeterPrecaution: preview failed, error code:" + iLastErr;
                    MessageBox.Show(strErr);
                }
                else
                {
                    string strTmp = String.Format("PerimeterPrecaution: preview successful");
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "PerimeterPrecautionForm", strTmp);
                    m_bPlay = true;
                }
            }
        }

        private void FormPerimeterPrecaution_Load(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceTreeType = g_deviceTree.GetDeviceTreeType();
            if (deviceTreeType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                SDK_StartPreview();
            }       
        }

        private void FormPerimeterPrecaution_FormClosing(object sender, FormClosingEventArgs e)
        {  
            if(m_bPlay)
            {
                SDK_StopPreview();
            }
        }

        //SDK close preview, release resources
        private void SDK_StopPreview()
        {
            bool bStopPlay = CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
            if (!bStopPlay)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKPreview", "NET_DVR_StopRealPlay");
            }
            else
            {
                m_bPlay = false;
                m_lRealHandle = -1;
                panelPlay.Refresh();
            }
        }

        //Alarm button function
        private void AlarmBtn_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceTreeType = g_deviceTree.GetDeviceTreeType();
            if(deviceTreeType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                FormPerimeterPrecautionAlarm_SDK dlg = new FormPerimeterPrecautionAlarm_SDK();
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.m_iChannel = m_iCurChanNo;
                dlg.m_lUserID = m_lUserID;
                dlg.Show();
            }
            
        }

        //Go to the area configuration to get the button function
        private void GetRegionEnterBtn_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceTreeType = g_deviceTree.GetDeviceTreeType();
            if (deviceTreeType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                SDK_GetRegionEnter();
            }
        }

        //Enter area configuration (SDK)
        private void SDK_GetRegionEnter()
        {
            if(-1 == comboBoxRegionEnterIndex.SelectedIndex)
            {
                MessageBox.Show("Please select the alert area!");
                return;
            }
            int i = comboBoxRegionEnterIndex.SelectedIndex;
            try
            {
                CHCNetSDK.NET_DVR_SMART_REGION_COND struCond = new CHCNetSDK.NET_DVR_SMART_REGION_COND();
                struCond.dwSize = (uint)Marshal.SizeOf(struCond);
                struCond.dwChannel = (uint)m_iCurChanNo;
                struCond.dwRegion = (uint)i + 1;

                CHCNetSDK.NET_DVR_STD_CONFIG struStdConfig = new CHCNetSDK.NET_DVR_STD_CONFIG();
                IntPtr ptrCondBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(struCond));
                Marshal.StructureToPtr(struCond, ptrCondBuffer, false);
                struStdConfig.lpCondBuffer = ptrCondBuffer;

                struStdConfig.dwCondSize = (uint)Marshal.SizeOf(struCond);              
                IntPtr ptrOutBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(m_struRegionEntrDetection.struRegion[i]));
                Marshal.StructureToPtr(m_struRegionEntrDetection.struRegion[i], ptrOutBuffer, false);
                struStdConfig.lpOutBuffer = ptrOutBuffer;

                struStdConfig.dwOutSize = (uint)Marshal.SizeOf(m_struRegionEntrDetection.struRegion[i]);
                IntPtr ptrStatusBuf = Marshal.AllocHGlobal(4096 * 4);
                ptrStatusBuf = IntPtr.Zero;
                struStdConfig.lpStatusBuffer = ptrStatusBuf;
                struStdConfig.dwStatusSize = 4096 * 4;

                IntPtr ptrStdCfg = Marshal.AllocHGlobal(Marshal.SizeOf(struStdConfig));
                Marshal.StructureToPtr(struStdConfig, ptrStdCfg, false);

                if (!CHCNetSDK.NET_DVR_GetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_REGION_ENTR_REGION, ptrStdCfg))
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GET_REGION_ENTR_REGION", "Get fail!");

                    Marshal.FreeHGlobal(ptrCondBuffer);
                    Marshal.FreeHGlobal(ptrOutBuffer);
                    Marshal.FreeHGlobal(ptrStatusBuf);
                    Marshal.FreeHGlobal(ptrStdCfg);

                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "Failure to obtain, error code:" + iLastErr;
                    MessageBox.Show(strErr);
                }
                else
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GET_REGION_ENTR_REGION", "Get succ!");

                    textBoxRegionEnterSensitivityLevel.Text = m_struRegionEntrDetection.struRegion[i].bySensitivity.ToString();

                    int iTarget = Convert.ToInt32(m_struRegionEntrDetection.struRegion[i].byDetectionTarget);
                    switch (iTarget)
                    {
                        case 0:
                            checkBoxRegionEnterTargetAll.Checked = true;
                            break;
                        case 1:
                            checkBoxRegionEnterTargetHuman.Checked = true;
                            break;
                        case 2:
                            checkBoxRegionEnterTargetVehicle.Checked = true;
                            break;
                        default:
                            break;
                    }
                    if ("1" == m_struRegionEntrDetection.byEnabled.ToString())
                    {
                        chkBoxRegionEnterEnabled.Checked = true;
                    }
                    if ("1" == m_struRegionEntrDetection.byEnableHumanMisinfoFilter.ToString())
                    {
                        chkBoxRegionEnterHumanMisinfoFilter.Checked = true;
                    }
                    if ("1" == m_struRegionEntrDetection.byEnableVehicleMisinfoFilter.ToString())
                    {
                        chkBoxRegionEnterVehicleMisinfoFilter.Checked = true;
                    }              
                }
                Marshal.FreeHGlobal(ptrCondBuffer);
                Marshal.FreeHGlobal(ptrOutBuffer);
                Marshal.FreeHGlobal(ptrStatusBuf);
                Marshal.FreeHGlobal(ptrStdCfg);
            }
            catch(Exception ex)
            {
                string err = ex.Message.ToString();
                MessageBox.Show(err);
            }           
        }

        private void GetLineDetectionBtn_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceTreeType = g_deviceTree.GetDeviceTreeType();
            if (deviceTreeType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                SDK_GetLineDetection();
            }
        }

        //Get line detection configuration (SDK)
        private void SDK_GetLineDetection()
        {
            chkBoxLineEnabled.Enabled = false;
            uint iCount = 1;
            CHCNetSDK.NET_DVR_CHANNEL_GROUP struLineDetectionCond = new CHCNetSDK.NET_DVR_CHANNEL_GROUP();
            struLineDetectionCond.dwChannel = (uint)m_iCurChanNo;
            struLineDetectionCond.dwGroup = 0;
            if (-1 == comboBoxLineDetectionIndex.SelectedIndex)
            {
                MessageBox.Show("Please select the alert surface!");
                return;
            }
            struLineDetectionCond.byID = Convert.ToByte(comboBoxLineDetectionIndex.SelectedItem.ToString());
            struLineDetectionCond.dwSize = (uint)Marshal.SizeOf(struLineDetectionCond);
            IntPtr ptrLineDetectionCond = Marshal.AllocHGlobal(Marshal.SizeOf(struLineDetectionCond));
            Marshal.StructureToPtr(struLineDetectionCond, ptrLineDetectionCond, false);
            IntPtr pStatus = (IntPtr)0;

            IntPtr ptrLineDetection = Marshal.AllocHGlobal(Marshal.SizeOf(m_struLineDetection));
            Marshal.StructureToPtr(m_struLineDetection, ptrLineDetection, false);
            uint dwOutBufferSize = (uint)Marshal.SizeOf(m_struLineDetection);
            if(!CHCNetSDK.NET_DVR_GetDeviceConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_TRAVERSE_PLANE_DETECTION, iCount, ptrLineDetectionCond, (uint)Marshal.SizeOf(struLineDetectionCond), pStatus, ptrLineDetection, dwOutBufferSize))
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GET_TRAVERSE_PLANE_DETECTION", "Get fail!");

                Marshal.FreeHGlobal(ptrLineDetectionCond);
                Marshal.FreeHGlobal(ptrLineDetection);

                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Failure to obtain, error code:" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GET_TRAVERSE_PLANE_DETECTION", "Get succ!");
                
                if(m_struLineDetection.byEnabled == 1)
                {
                    chkBoxLineDetectionEnabled.Checked = true;
                }
                textBoxLineDetectionSensitivityLevel.Text = m_struLineDetection.struAlertParam[struLineDetectionCond.byID].bySensitivity.ToString();

                int iTarget = Convert.ToInt32(m_struLineDetection.struAlertParam[struLineDetectionCond.byID].byDetectionTarget);
                switch (iTarget)
                {
                    case 0:
                        checkBoxLineDetectionTargetAll.Checked = true;
                        break;
                    case 1:
                        checkBoxLineDetectionTargetHuman.Checked = true;
                        break;
                    case 2:
                        checkBoxLineDetectionTargetVehicle.Checked = true;
                        break;
                    default:
                        break;
                }

                int iCrossDirection = Convert.ToInt32(m_struLineDetection.struAlertParam[struLineDetectionCond.byID].dwCrossDirection);
                switch(iCrossDirection)
                {
                    case 0:
                        comboBoxLineDetectionDirection.Text = comboBoxLineDetectionDirection.Items[0].ToString();
                        break;
                    case 1:
                        comboBoxLineDetectionDirection.Text = comboBoxLineDetectionDirection.Items[1].ToString();
                        break;
                    case 2:
                        comboBoxLineDetectionDirection.Text = comboBoxLineDetectionDirection.Items[2].ToString();
                        break;
                    default:
                        break;
                }    
               if ("1" == m_struLineDetection.byEnableHumanMisinfoFilter.ToString())
               {
                   chkBoxLineDetectionHumanMisinfoFilter.Checked = true;
               }
               if ("1" == m_struLineDetection.byEnableVehicleMisinfoFilter.ToString())
               {
                   chkBoxLineDetectionVehicleMisinfoFilter.Checked = true;
               }           
            }
            Marshal.FreeHGlobal(ptrLineDetectionCond);
            Marshal.FreeHGlobal(ptrLineDetection);
        }

        private void GetFieldDetectionBtn_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceTreeType = g_deviceTree.GetDeviceTreeType();
            if (deviceTreeType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                SDK_GetFieldDetection();
            }
        }

        //Get intrusion configuration (SDK)
        private void SDK_GetFieldDetection()
        {
            chkBoxFieldEnabled.Enabled = false;
            uint iCount = 1;
            CHCNetSDK.NET_DVR_CHANNEL_GROUP struFielDetectionCond = new CHCNetSDK.NET_DVR_CHANNEL_GROUP();
            struFielDetectionCond.dwChannel = (uint)m_iCurChanNo;
            struFielDetectionCond.dwGroup = 0;
            if (-1 == comboBoxFieldDetectionIndex.SelectedIndex)
            {
                MessageBox.Show("Please select the alert area!");
                return;
            }
            struFielDetectionCond.byID = Convert.ToByte(comboBoxFieldDetectionIndex.SelectedItem.ToString());
            struFielDetectionCond.dwSize = (uint)Marshal.SizeOf(struFielDetectionCond);

            IntPtr ptrFielDetectionCond = Marshal.AllocHGlobal(Marshal.SizeOf(struFielDetectionCond));
            Marshal.StructureToPtr(struFielDetectionCond, ptrFielDetectionCond, false);
            IntPtr pStatus = IntPtr.Zero;

            IntPtr ptrFieldDetection = Marshal.AllocHGlobal(Marshal.SizeOf(m_struFieldDetection));
            Marshal.StructureToPtr(m_struFieldDetection, ptrFieldDetection, false);
            uint dwOutBufferSize = (uint)Marshal.SizeOf(m_struFieldDetection);
            if(!CHCNetSDK.NET_DVR_GetDeviceConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_FIELD_DETECTION, iCount, ptrFielDetectionCond, (uint)Marshal.SizeOf(struFielDetectionCond), pStatus, ptrFieldDetection,dwOutBufferSize))
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GET_FIELD_DETECTION", "Get fail!");

                Marshal.FreeHGlobal(ptrFielDetectionCond);
                Marshal.FreeHGlobal(ptrFieldDetection);

                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Failure to obtain, error code:" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GET_FIELD_DETECTION", "Get succ!");

                if (m_struFieldDetection.byEnabled == 1)
                {
                    chkBoxFieldDetectionEnabled.Checked = true;
                }
                textBoxFieldDetectionSensitivityLevel.Text = m_struFieldDetection.struIntrusion[struFielDetectionCond.byID].bySensitivity.ToString();
                textBoxFieldDectectionTimeThreshold.Text = m_struFieldDetection.struIntrusion[struFielDetectionCond.byID].wDuration.ToString();

                int iTarget = Convert.ToInt32(m_struFieldDetection.struIntrusion[struFielDetectionCond.byID].byDetectionTarget);
                switch (iTarget)
                {
                    case 0:
                        checkBoxFieldDetectionTargetAll.Checked = true;
                        break;
                    case 1:
                        checkBoxFieldDetectionTargetHuman.Checked = true;
                        break;
                    case 2:
                        checkBoxFieldDetectionTargetVehicle.Checked = true;
                        break;
                    default:
                        break;
                }
              
                if ("1" == m_struFieldDetection.byEnableHumanMisinfoFilter.ToString())
                {
                    chkBoxFieldDetectionHumanMisinfoFilter.Checked = true;
                }
                if ("1" == m_struFieldDetection.byEnableVehicleMisinfoFilter.ToString())
                {
                    chkBoxFieldDetectionVehicleMisinfoFilter.Checked = true;
                }
            }
            Marshal.FreeHGlobal(ptrFielDetectionCond);
            Marshal.FreeHGlobal(ptrFieldDetection);
        }

        private void GetRegionExitingBtn_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceTreeType = g_deviceTree.GetDeviceTreeType();
            if (deviceTreeType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                SDK_GetRegionExiting();
            }
        }

        //Get the exit area configuration (SDK)
        private void SDK_GetRegionExiting()
        {
            if (-1 == comboBoxRegionExitingIndex.SelectedIndex)
            {
                MessageBox.Show("Please select the alert area!");
                return;
            }
            int i = comboBoxRegionExitingIndex.SelectedIndex;
            try
            {
                CHCNetSDK.NET_DVR_SMART_REGION_COND struCond = new CHCNetSDK.NET_DVR_SMART_REGION_COND();
                struCond.dwSize = (uint)Marshal.SizeOf(struCond);
                struCond.dwChannel = (uint)m_iCurChanNo;
                struCond.dwRegion = (uint)i + 1;

                CHCNetSDK.NET_DVR_STD_CONFIG struStdConfig = new CHCNetSDK.NET_DVR_STD_CONFIG();
                IntPtr ptrCondBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(struCond));
                Marshal.StructureToPtr(struCond, ptrCondBuffer, false);
                struStdConfig.lpCondBuffer = ptrCondBuffer;

                struStdConfig.dwCondSize = (uint)Marshal.SizeOf(struCond);
                IntPtr ptrOutBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(m_struRegionExitDetection.struRegion[i]));
                Marshal.StructureToPtr(m_struRegionExitDetection.struRegion[i], ptrOutBuffer, false);
                struStdConfig.lpOutBuffer = ptrOutBuffer;

                struStdConfig.dwOutSize = (uint)Marshal.SizeOf(m_struRegionExitDetection.struRegion[i]);
                IntPtr ptrStatusBuf = Marshal.AllocHGlobal(4096 * 4);
                ptrStatusBuf = IntPtr.Zero;
                struStdConfig.lpStatusBuffer = ptrStatusBuf;
                struStdConfig.dwStatusSize = 4096 * 4;

                IntPtr ptrStdCfg = Marshal.AllocHGlobal(Marshal.SizeOf(struStdConfig));
                Marshal.StructureToPtr(struStdConfig, ptrStdCfg, false);

                if (!CHCNetSDK.NET_DVR_GetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_REGION_EXITING_REGION, ptrStdCfg))
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GET_REGION_EXITING_REGION", "Get fail!");

                    Marshal.FreeHGlobal(ptrCondBuffer);
                    Marshal.FreeHGlobal(ptrOutBuffer);
                    Marshal.FreeHGlobal(ptrStatusBuf);
                    Marshal.FreeHGlobal(ptrStdCfg);

                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "Failure to obtain, error code:" + iLastErr; 
                    MessageBox.Show(strErr);
                }
                else
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_GET_REGION_EXITING_REGION", "Get succ!");

                    textBoxRegionExitingSensitivityLevel.Text = m_struRegionExitDetection.struRegion[i].bySensitivity.ToString();

                    int iTarget = Convert.ToInt32(m_struRegionExitDetection.struRegion[i].byDetectionTarget);
                    switch (iTarget)
                    {
                        case 0:
                            checkBoxRegionExitingTargetAll.Checked = true;
                            break;
                        case 1:
                            checkBoxRegionExitingTargetHuman.Checked = true;
                            break;
                        case 2:
                            checkBoxRegionExitingTargetVehicle.Checked = true;
                            break;
                        default:
                            break;
                    }
                    if ("1" == m_struRegionExitDetection.byEnabled.ToString())
                    {
                        chkBoxRegionExitingEnabled.Checked = true;
                    }
                    if ("1" == m_struRegionExitDetection.byEnableHumanMisinfoFilter.ToString())
                    {
                        chkBoxRegionExitingHumanMisinfoFilter.Checked = true;
                    }
                    if ("1" == m_struRegionExitDetection.byEnableVehicleMisinfoFilter.ToString())
                    {
                        chkBoxRegionExitingVehicleMisinfoFilter.Checked = true;
                    }
                }
                Marshal.FreeHGlobal(ptrCondBuffer);
                Marshal.FreeHGlobal(ptrOutBuffer);
                Marshal.FreeHGlobal(ptrStatusBuf);
                Marshal.FreeHGlobal(ptrStdCfg);
            }
            catch (Exception ex)
            {
                string err = ex.Message.ToString();
                MessageBox.Show(err);
            }           
        }

        private void panelPlay_MouseDown(object sender, MouseEventArgs e)
        {
                if (e.Button == MouseButtons.Left)
                {
                    Point point = panelPlay.PointToClient(Control.MousePosition);
                    float fPointX = Convert.ToSingle(((float)point.X / panelPlay.Width).ToString("#0.000"));
                    float fPointY = Convert.ToSingle(((float)point.Y / panelPlay.Height).ToString("#0.000"));
                    if (-1 == comboBoxPaintType.SelectedIndex)
                    {
                        return;
                    }
                    if (0 == comboBoxPaintType.SelectedIndex)//Draw point
                    {
                        lock (m_points)
                        {
                            if (m_points.Count >= 1)
                            {
                                m_points = new List<Point>();
                            }

                            m_struPoint.fX = fPointX;
                            m_struPoint.fY = fPointY;

                            m_points.Add(point);

                            if (m_points.Count == 1 && fDrawFun == null)
                            {
                                IDeviceTree.EDeviceTreeType deviceTreeType = g_deviceTree.GetDeviceTreeType();
                                if (deviceTreeType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
                                {
                                    fDrawFunc = new CHCNetSDK.DRAWFUN(cbDrawFunc);
                                    if (!CHCNetSDK.NET_DVR_RigisterDrawFun(m_lRealHandle, fDrawFunc, 0))
                                    {
                                        int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                                        string strErr = "Preview screen overlay configuration failed! Error code:" + iLastErr;
                                        MessageBox.Show(strErr);
                                    }
                                }
                            }
                        }
                    }
                    if (1 == comboBoxPaintType.SelectedIndex)//Draw line
                    {
                        m_struPolygon.struPos = new VCA_POINT[VCA_MAX_POLYGON_POINT_NUM];
                        
                        if (m_iPointNum >= 2)
                        {     
                            m_iPointNum = 0;
                            m_points = new List<Point>();
                        }

                        if (m_points.Count >= 0 && m_points.Count < 2)
                        {
                            m_struPolygon.struPos[m_iPointNum].fX = fPointX;
                            m_struPolygon.struPos[m_iPointNum].fY = fPointY;
                            ++m_iPointNum;
                            m_struPolygon.dwPointNum = (uint)m_iPointNum;
                            m_points.Add(point);

                            if (m_points.Count == 2 && fDrawFun == null)
                            {
                                IDeviceTree.EDeviceTreeType deviceTreeType = g_deviceTree.GetDeviceTreeType();
                                if (deviceTreeType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
                                {
                                    fDrawFunc = new CHCNetSDK.DRAWFUN(cbDrawFunc);
                                    if (!CHCNetSDK.NET_DVR_RigisterDrawFun(m_lRealHandle, fDrawFunc, 0))
                                    {
                                        int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                                        string strErr = "Preview screen overlay configuration failed! Error code:" + iLastErr;
                                        MessageBox.Show(strErr);
                                    }
                                }
                            }
                        }
                    }
                    if (2 == comboBoxPaintType.SelectedIndex)//Draw polygon
                    {
                        m_struPolygon.struPos = new VCA_POINT[VCA_MAX_POLYGON_POINT_NUM];
                        if (m_iPointNum >= VCA_MAX_POLYGON_POINT_NUM)
                        {
                            m_iPointNum = 0;
                            m_points = new List<Point>();
                        }

                        if (m_points.Count < VCA_MAX_POLYGON_POINT_NUM)
                        {
                            m_struPolygon.struPos[m_iPointNum].fX = fPointX;
                            m_struPolygon.struPos[m_iPointNum].fY = fPointY;
                            ++m_iPointNum;
                            m_struPolygon.dwPointNum = (uint)m_iPointNum;
                            m_points.Add(point);

                            if (fDrawFun == null)
                            {
                                IDeviceTree.EDeviceTreeType deviceTreeType = g_deviceTree.GetDeviceTreeType();
                                if (deviceTreeType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
                                {
                                    fDrawFunc = new CHCNetSDK.DRAWFUN(cbDrawFunc);
                                    if (!CHCNetSDK.NET_DVR_RigisterDrawFun(m_lRealHandle, fDrawFunc, 0))
                                    {
                                        int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                                        string strErr = "Preview screen overlay configuration failed! Error code:" + iLastErr;
                                        MessageBox.Show(strErr);
                                    }
                                }
                            }
                        }
                    }
               }
         }

        // Callback function overlay image characters, play library interface format
        private void cbDrawFun(int port, System.IntPtr hDc, int nUser)
        {
            DrawFun(port, hDc);
        }

        // Callback function overlay image character, SDK interface format
        private void cbDrawFunc(int port, IntPtr hDc, uint nUser)
        {
            DrawFun(port, hDc);
        }

        //Callback function overlay image character implementation
        private void DrawFun(int port, IntPtr hDc)
        {
            Graphics g = Graphics.FromHdc(hDc);
            Pen pen = new Pen(Color.Red, 2);
            Brush brush = new SolidBrush(Color.FromArgb(50, 255, 255, 255));
            if (g == null)
            {
                return;
            }
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            lock (m_points)
            {
                Point[] pPolygon = m_points.ToArray();

                int pointsCount = pPolygon.Length;
                if (pointsCount > 0)
                {
                    if (1 == pointsCount)
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(255, 0, 0)), pPolygon[0].X, pPolygon[0].Y, 5, 5);
                    }

                    for (int index = 0; index < pointsCount - 1; ++index)
                    {
                        g.DrawLine(new Pen(Color.Red, 2), new Point(pPolygon[index].X, pPolygon[index].Y),
                            new Point(pPolygon[index + 1].X, pPolygon[index + 1].Y));
                    }
                    g.DrawLine(pen, new Point(pPolygon[pointsCount - 1].X, pPolygon[pointsCount - 1].Y),
                            new Point(pPolygon[0].X, pPolygon[0].Y));

                    g.FillPolygon(brush, pPolygon);
                }
            }
        }

        private void BtnPaintOK_Click(object sender, EventArgs e)
        {
            comboBoxPaintType.SelectedIndex = -1;
        }

        private void BtnPaintClear_Click(object sender, EventArgs e)
        {
           lock(m_points)
           {
               m_points.Clear();
           }
        }

        private void SetLineDetectionBtn_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceTreeType = g_deviceTree.GetDeviceTreeType();
            if (deviceTreeType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                SDK_SetLineDetection();
            }
        }

        //set line detection(SDK)
        private void SDK_SetLineDetection()
        {
            uint iCount = 0;
            CHCNetSDK.NET_DVR_CHANNEL_GROUP struLineDetectionCond = new CHCNetSDK.NET_DVR_CHANNEL_GROUP();
            struLineDetectionCond.dwChannel = (uint)m_iCurChanNo;
            struLineDetectionCond.dwGroup = 0;
            struLineDetectionCond.byID = Convert.ToByte(comboBoxLineDetectionIndex.SelectedItem.ToString());
            struLineDetectionCond.dwSize = (uint)Marshal.SizeOf(struLineDetectionCond);
            
            IntPtr ptrLineDetectionCond = Marshal.AllocHGlobal(Marshal.SizeOf(struLineDetectionCond));
            Marshal.StructureToPtr(struLineDetectionCond, ptrLineDetectionCond, false);
            IntPtr pStatus = (IntPtr)0;

            m_struLineDetection.dwSize = (uint)Marshal.SizeOf(m_struLineDetection);
            m_struLineDetection.byEnabled = Convert.ToByte(chkBoxLineDetectionEnabled.Checked);
            m_struLineDetection.byEnableHumanMisinfoFilter = Convert.ToByte(chkBoxLineDetectionHumanMisinfoFilter.Checked);
            m_struLineDetection.byEnableVehicleMisinfoFilter = Convert.ToByte(chkBoxLineDetectionVehicleMisinfoFilter.Checked);
            m_struLineDetection.struAlertParam[struLineDetectionCond.byID].bySensitivity = Convert.ToByte(textBoxLineDetectionSensitivityLevel.Text);
            m_struLineDetection.struAlertParam[struLineDetectionCond.byID].dwCrossDirection = (CHCNetSDK.VCA_CROSS_DIRECTION)comboBoxLineDetectionDirection.SelectedIndex;
            if (checkBoxLineDetectionTargetAll.Checked)
            {
                m_struRegionEntrDetection.struRegion[struLineDetectionCond.byID].byDetectionTarget = Convert.ToByte("0");
            }
            if (checkBoxLineDetectionTargetHuman.Checked)
            {
                m_struRegionEntrDetection.struRegion[struLineDetectionCond.byID].byDetectionTarget = Convert.ToByte("1");
            }
            if (checkBoxLineDetectionTargetVehicle.Checked)
            {
                m_struRegionEntrDetection.struRegion[struLineDetectionCond.byID].byDetectionTarget = Convert.ToByte("2");
            }

            IntPtr ptrLineDetection = Marshal.AllocHGlobal(Marshal.SizeOf(m_struLineDetection));
            Marshal.StructureToPtr(m_struLineDetection, ptrLineDetection, false);

            if(!CHCNetSDK.NET_DVR_SetDeviceConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_TRAVERSE_PLANE_DETECTION, iCount, ptrLineDetectionCond, (uint)Marshal.SizeOf(struLineDetectionCond), pStatus, ptrLineDetection, (uint)Marshal.SizeOf(m_struLineDetection)))
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_SET_TRAVERSE_PLANE_DETECTION", "Set fail!");

                Marshal.FreeHGlobal(ptrLineDetectionCond);
                Marshal.FreeHGlobal(ptrLineDetection);

                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Setting failed, error code:" + iLastErr; 
                MessageBox.Show(strErr);
            }
            else
            {
                Marshal.FreeHGlobal(ptrLineDetectionCond);
                Marshal.FreeHGlobal(ptrLineDetection);
                MessageBox.Show("Setup successful!");
            }
        }

        //private string GetXmlNamespace(string strXml)
        //{
        //    if (string.IsNullOrEmpty(strXml)) return string.Empty;
        //    int iBegin = 0;
        //    int iEnd = 0;
        //    if ((iBegin =  strXml.IndexOf("xmlns=\"")) > 0)
        //    {
        //        iEnd = strXml.IndexOf("\"", iBegin + 7);
        //        if (iEnd > iBegin + 7)
        //        {
        //            return strXml.Substring(iBegin + 7, iEnd - iBegin - 7);
        //        }
        //    }
        //    return String.Empty;
        //}
        private void SetFieldDetectionBtn_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceTreeType = g_deviceTree.GetDeviceTreeType();
            if (deviceTreeType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                SDK_SetFieldDetection();
            }
        }  

        //set field detection(SDK)
        private void SDK_SetFieldDetection()
        {
            uint iCount = 1;
            CHCNetSDK.NET_DVR_CHANNEL_GROUP struFieldDetectionCond = new CHCNetSDK.NET_DVR_CHANNEL_GROUP();
            struFieldDetectionCond.dwChannel = (uint)m_iCurChanNo;
            struFieldDetectionCond.dwGroup = 0;
            struFieldDetectionCond.byID = Convert.ToByte(comboBoxFieldDetectionIndex.SelectedItem.ToString());
            struFieldDetectionCond.dwSize = (uint)Marshal.SizeOf(struFieldDetectionCond);

            IntPtr ptrFieldDetectionCond = Marshal.AllocHGlobal(Marshal.SizeOf(struFieldDetectionCond));
            Marshal.StructureToPtr(struFieldDetectionCond, ptrFieldDetectionCond, false);
            IntPtr pStatus = IntPtr.Zero;

            m_struFieldDetection.dwSize = (uint)Marshal.SizeOf(m_struFieldDetection);
            m_struFieldDetection.byEnabled = Convert.ToByte(chkBoxFieldDetectionEnabled.Checked);
            m_struFieldDetection.byEnableHumanMisinfoFilter = Convert.ToByte(chkBoxFieldDetectionHumanMisinfoFilter.Checked);
            m_struFieldDetection.byEnableVehicleMisinfoFilter = Convert.ToByte(chkBoxFieldDetectionVehicleMisinfoFilter.Checked);
            m_struFieldDetection.struIntrusion[struFieldDetectionCond.byID].bySensitivity = Convert.ToByte(textBoxFieldDetectionSensitivityLevel.Text);
            m_struFieldDetection.struIntrusion[struFieldDetectionCond.byID].wDuration = Convert.ToUInt16(textBoxFieldDectectionTimeThreshold.Text);

            if (checkBoxFieldDetectionTargetAll.Checked)
            {
                m_struFieldDetection.struIntrusion[struFieldDetectionCond.byID].byDetectionTarget = Convert.ToByte("0");
            }
            if (checkBoxFieldDetectionTargetHuman.Checked)
            {
                m_struFieldDetection.struIntrusion[struFieldDetectionCond.byID].byDetectionTarget = Convert.ToByte("1");
            }
            if (checkBoxFieldDetectionTargetVehicle.Checked)
            {
                m_struFieldDetection.struIntrusion[struFieldDetectionCond.byID].byDetectionTarget = Convert.ToByte("2");
            }

            IntPtr ptrFieldDetection = Marshal.AllocHGlobal(Marshal.SizeOf(m_struFieldDetection));
            Marshal.StructureToPtr(m_struFieldDetection, ptrFieldDetection, false);
            if(!CHCNetSDK.NET_DVR_SetDeviceConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_FIELD_DETECTION, iCount, ptrFieldDetectionCond, (uint)Marshal.SizeOf(struFieldDetectionCond), pStatus, ptrFieldDetection, (uint)Marshal.SizeOf(m_struFieldDetection)))
            {
                Marshal.FreeHGlobal(ptrFieldDetectionCond);
                Marshal.FreeHGlobal(ptrFieldDetection);

                MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_SET_FIELD_DETECTION", "Set fail!");
               
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Gets the exit areaconfiguration" + iLastErr; // setting failed, output error code
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("Setup successful!");
            }

            Marshal.FreeHGlobal(ptrFieldDetectionCond);
            Marshal.FreeHGlobal(ptrFieldDetection);
        }

        private void SetRegionEnterBtn_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceTreeType = g_deviceTree.GetDeviceTreeType();
            if (deviceTreeType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                SDK_SetRegionEnter();
            }
        }

        //set enter area(SDK)
        private void SDK_SetRegionEnter()
        {
            if (-1 == comboBoxRegionEnterIndex.SelectedIndex)
            {
                MessageBox.Show("Please select the alert area!");
                return;
            }
            int i = comboBoxRegionEnterIndex.SelectedIndex;
            try
            {
                CHCNetSDK.NET_DVR_SMART_REGION_COND struCond = new CHCNetSDK.NET_DVR_SMART_REGION_COND();
                struCond.dwSize = (uint)Marshal.SizeOf(struCond);
                struCond.dwChannel = (uint)m_iCurChanNo;
                struCond.dwRegion = (uint)i + 1;

                m_struRegionEntrDetection.byEnabled = Convert.ToByte(chkBoxRegionEnterEnabled.Checked);
                m_struRegionEntrDetection.struRegion[i].bySensitivity = Convert.ToByte(textBoxRegionEnterSensitivityLevel.Text);
                if(checkBoxRegionEnterTargetAll.Checked)
                {
                    m_struRegionEntrDetection.struRegion[i].byDetectionTarget = Convert.ToByte("0");
                }
                if(checkBoxRegionEnterTargetHuman.Checked)
                {
                    m_struRegionEntrDetection.struRegion[i].byDetectionTarget = Convert.ToByte("1");
                }
                if(checkBoxRegionEnterTargetVehicle.Checked)
                {
                    m_struRegionEntrDetection.struRegion[i].byDetectionTarget = Convert.ToByte("2");
                }

                CHCNetSDK.NET_DVR_STD_CONFIG struStdConfig = new CHCNetSDK.NET_DVR_STD_CONFIG();
                IntPtr ptrCondBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(struCond));
                Marshal.StructureToPtr(struCond, ptrCondBuffer, false);
                struStdConfig.lpCondBuffer = ptrCondBuffer;

                struStdConfig.dwCondSize = (uint)Marshal.SizeOf(struCond);

                IntPtr ptrInBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(m_struRegionEntrDetection.struRegion[i]));
                Marshal.StructureToPtr(m_struRegionEntrDetection.struRegion[i], ptrInBuffer, false);
                struStdConfig.lpInBuffer = ptrInBuffer;

                struStdConfig.dwInSize = (uint)Marshal.SizeOf(m_struRegionEntrDetection.struRegion[i]);

                IntPtr ptrStatusBuf = Marshal.AllocHGlobal(4096 * 4);
                ptrStatusBuf = IntPtr.Zero;
                struStdConfig.lpStatusBuffer = ptrStatusBuf;
                struStdConfig.dwStatusSize = 4096 * 4;

                IntPtr ptrStdCfg = Marshal.AllocHGlobal(Marshal.SizeOf(struStdConfig));
                Marshal.StructureToPtr(struStdConfig, ptrStdCfg, false);
                if (!CHCNetSDK.NET_DVR_SetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_REGION_ENTR_REGION, ptrStdCfg))
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_SET_REGION_ENTR_REGION", "Set fail!");

                    Marshal.FreeHGlobal(ptrCondBuffer);
                    Marshal.FreeHGlobal(ptrInBuffer);
                    Marshal.FreeHGlobal(ptrStatusBuf);
                    Marshal.FreeHGlobal(ptrStdCfg);

                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "Failure to obtain, error code:" + iLastErr; 
                    MessageBox.Show(strErr);
                }
                else
                {
                    MessageBox.Show("Setup successful!");
                }
                Marshal.FreeHGlobal(ptrCondBuffer);
                Marshal.FreeHGlobal(ptrInBuffer);
                Marshal.FreeHGlobal(ptrStatusBuf);
                Marshal.FreeHGlobal(ptrStdCfg);        
            }
            catch (Exception ex)
            {
                string err = ex.Message.ToString();
                MessageBox.Show(err);
            }           
        }

        private void SetRegionExitingBtn_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceTreeType = g_deviceTree.GetDeviceTreeType();
            if (deviceTreeType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                SDK_SetRegionExiting();
            }
        }

        //set exit area(SDK)
        private void SDK_SetRegionExiting()
        {
            if (-1 == comboBoxRegionExitingIndex.SelectedIndex)
            {
                MessageBox.Show("Please select the alert area!");
                return;
            }
            int i = comboBoxRegionExitingIndex.SelectedIndex;
            try
            {
                CHCNetSDK.NET_DVR_SMART_REGION_COND struCond = new CHCNetSDK.NET_DVR_SMART_REGION_COND();
                struCond.dwSize = (uint)Marshal.SizeOf(struCond);
                struCond.dwChannel = (uint)m_iCurChanNo;
                struCond.dwRegion = (uint)i + 1;

                m_struRegionExitDetection.byEnabled = Convert.ToByte(chkBoxRegionExitingEnabled.Checked);
                m_struRegionExitDetection.struRegion[i].bySensitivity = Convert.ToByte(textBoxRegionExitingSensitivityLevel.Text);
                if (checkBoxRegionExitingTargetAll.Checked)
                {
                    m_struRegionExitDetection.struRegion[i].byDetectionTarget = Convert.ToByte("0");
                }
                if (checkBoxRegionExitingTargetHuman.Checked)
                {
                    m_struRegionExitDetection.struRegion[i].byDetectionTarget = Convert.ToByte("1");
                }
                if (checkBoxRegionExitingTargetVehicle.Checked)
                {
                    m_struRegionExitDetection.struRegion[i].byDetectionTarget = Convert.ToByte("2");
                }

                CHCNetSDK.NET_DVR_STD_CONFIG struStdConfig = new CHCNetSDK.NET_DVR_STD_CONFIG();
                IntPtr ptrCondBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(struCond));
                Marshal.StructureToPtr(struCond, ptrCondBuffer, false);
                struStdConfig.lpCondBuffer = ptrCondBuffer;

                struStdConfig.dwCondSize = (uint)Marshal.SizeOf(struCond);

                IntPtr ptrInBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(m_struRegionExitDetection.struRegion[i]));
                Marshal.StructureToPtr(m_struRegionExitDetection.struRegion[i], ptrInBuffer, false);
                struStdConfig.lpInBuffer = ptrInBuffer;

                struStdConfig.dwInSize = (uint)Marshal.SizeOf(m_struRegionExitDetection.struRegion[i]);

                IntPtr ptrStatusBuf = Marshal.AllocHGlobal(4096 * 4);
                ptrStatusBuf = IntPtr.Zero;
                struStdConfig.lpStatusBuffer = ptrStatusBuf;
                struStdConfig.dwStatusSize = 4096 * 4;

                IntPtr ptrStdCfg = Marshal.AllocHGlobal(Marshal.SizeOf(struStdConfig));
                Marshal.StructureToPtr(struStdConfig, ptrStdCfg, false);
                if (!CHCNetSDK.NET_DVR_SetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_REGION_EXITING_REGION, ptrStdCfg))
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "NET_DVR_SET_REGION_EXITING_REGION", "Set fail!");

                    Marshal.FreeHGlobal(ptrCondBuffer);
                    Marshal.FreeHGlobal(ptrInBuffer);
                    Marshal.FreeHGlobal(ptrStatusBuf);
                    Marshal.FreeHGlobal(ptrStdCfg);

                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "Gets the exit areaconfiguration" + iLastErr; 
                    MessageBox.Show(strErr);
                }
                else
                {
                    MessageBox.Show("Setup successful!");
                }
                Marshal.FreeHGlobal(ptrCondBuffer);
                Marshal.FreeHGlobal(ptrInBuffer);
                Marshal.FreeHGlobal(ptrStatusBuf);
                Marshal.FreeHGlobal(ptrStdCfg);
            }
            catch (Exception ex)
            {
                string err = ex.Message.ToString();
                MessageBox.Show(err);
            }           
        }
    }
 }

        

