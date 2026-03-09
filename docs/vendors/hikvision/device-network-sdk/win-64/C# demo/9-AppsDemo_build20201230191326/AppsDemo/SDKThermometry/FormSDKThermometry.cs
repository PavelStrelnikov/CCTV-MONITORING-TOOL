using Common;
using Common.Head;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TINYXMLTRANS;

namespace SDKThermometry
{
    public partial class FormSDKThermometry : PluginsControl
    {
        private IDeviceTree g_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        private int m_iCurDeviceIndex = -1;
        private int m_iCurChanNo = -1;
        private int m_iCurChanIndex = -1;
        private int m_lUserID = -1;
        private Int32 m_iPort = -1;
        private IntPtr m_pUser = new IntPtr();
        private int m_lRealHandle = -1;
        bool bSaveVideo = false;
        bool m_bPlay = false;
        private int m_lCaptureHandle = -1;
        private string strFilePath = "";
        private int m_iUploadHandle = -1;
        private bool m_bUpLoading = false;
        private int m_iDownloadHandle = -1;
        private bool m_bDownLoading = false;
        private bool m_bShowOSD = false;
        bool m_bBurningPrevEnable = false;
        private IDeviceTree.DeviceInfo m_deviceInfo = null;
        private IDeviceTree.ChannelInfo m_channelInfo = null;
        private CHCNetSDK.NET_DVR_STD_ABILITY m_struThermalAbility = new CHCNetSDK.NET_DVR_STD_ABILITY();
        private CHCNetSDK.NET_DVR_STD_CONFIG m_struSTDConfig = new CHCNetSDK.NET_DVR_STD_CONFIG();
        private CHCNetSDK.NET_DVR_THERMOMETRY_MODE m_struThermalMode = new CHCNetSDK.NET_DVR_THERMOMETRY_MODE();
        private CHCNetSDK.NET_DVR_PREVIEWINFO m_struPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
        private CHCNetSDK.NET_DVR_JPEGPICTURE_WITHAPPENDDATA m_JEPGWithAppendData = new CHCNetSDK.NET_DVR_JPEGPICTURE_WITHAPPENDDATA();
        private CHCNetSDK.NET_DVR_XML_CONFIG_INPUT m_struXMLConfigInput;
        private CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT m_struXMLConfigOutput;

        public class jsonPoint
        {
            public float positionX { get; set; }
            public float positionY { get; set; }
        }

        public class jsonClickToThermometryRule
        {
            public jsonPoint Point { get; set; }
        }
        public class jsonClickToThermometry
        {
            public jsonClickToThermometryRule ClickToThermometryRule { get; set; }
        }

        jsonClickToThermometry m_jsonClickToThermtry;

        public FormSDKThermometry()
        {
            InitializeComponent();

            if (g_deviceTree != null)
            {
                comboBoxStreamType.SelectedIndex = 0;
                g_deviceTree.SelectedNodeChanged += g_deviceTree_SelectedNodeChanged;
                this.GetDevicesInfo();
            }

            this.GetLoginInfo();
            this.GetThermalAbility();

            // 录像文件类型
            comboBoxVideoType.SelectedIndex = 0;
            comboThermMode.SelectedIndex = 0;
            comboBoxROIEnabled.SelectedIndex = 0;

        }

        private void GetDevicesInfo()
        {
            if (g_deviceTree != null)
            {
                this.m_deviceInfo = g_deviceTree.GetSelectedDeviceInfo();
                this.m_channelInfo = g_deviceTree.GetSelectedChannelInfo();
            }
        }

        void g_deviceTree_SelectedNodeChanged()
        {
            this.GetDevicesInfo();
            this.GetLoginInfo();
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

        // 热成像能力
        private void GetThermalAbility()
        {
            if (m_lUserID < 0)
            {
                string strErr = "请先登录！！！";
                MessageBox.Show(strErr);
                return;
            }

            int iCondSize = sizeof(int);
            m_struThermalAbility.lpCondBuffer = Marshal.AllocHGlobal(iCondSize);
            Marshal.WriteInt32(m_struThermalAbility.lpCondBuffer, (int)m_iCurChanNo);

            m_struThermalAbility.dwCondSize = (uint)iCondSize;
            m_struThermalAbility.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.XML_ABILITY_OUT_LEN);
            m_struThermalAbility.dwOutSize = CHCNetSDK.XML_ABILITY_OUT_LEN;
            m_struThermalAbility.lpStatusBuffer = Marshal.AllocHGlobal(CHCNetSDK.XML_ABILITY_OUT_LEN);
            m_struThermalAbility.dwStatusSize = CHCNetSDK.XML_ABILITY_OUT_LEN;
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struThermalAbility));
            Marshal.StructureToPtr(m_struThermalAbility, ptr, false);

            bool bEnable = CHCNetSDK.NET_DVR_GetSTDAbility(m_lUserID, CHCNetSDK.NET_DVR_GET_THERMAL_CAPABILITIES, ptr);
            string strOutputParam = Marshal.PtrToStringAnsi(m_struThermalAbility.lpOutBuffer);

            if (!bEnable)
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal: 获取热成像能力失败，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                string strTmp = String.Format("Thermal：获取热成像能力成功");
                MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "SDK:", strTmp);
            }
        }

        // 测温模式
        private void btnThermModeGet_Click(object sender, EventArgs e)
        {
            int iCondSize = sizeof(int);

            m_struSTDConfig.lpCondBuffer = Marshal.AllocHGlobal(iCondSize);
            Marshal.WriteInt32(m_struSTDConfig.lpCondBuffer, (int)m_iCurChanNo);
            m_struSTDConfig.dwCondSize = (uint)iCondSize;

            int iOutSize = Marshal.SizeOf(m_struThermalMode);
            m_struThermalMode.dwSize = (uint)iOutSize;
            IntPtr ptrOutBuffer = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struThermalMode, ptrOutBuffer, false);
            m_struSTDConfig.lpOutBuffer = ptrOutBuffer;
            m_struSTDConfig.dwOutSize = (uint)iOutSize;
            m_struSTDConfig.lpStatusBuffer = Marshal.AllocHGlobal(CHCNetSDK.XML_ABILITY_OUT_LEN);
            m_struSTDConfig.dwStatusSize = CHCNetSDK.XML_ABILITY_OUT_LEN;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_GetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_THERMOMETRY_MODE, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：获取测温模式，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                m_struSTDConfig = (CHCNetSDK.NET_DVR_STD_CONFIG)Marshal.PtrToStructure(ptr, typeof(CHCNetSDK.NET_DVR_STD_CONFIG));
                m_struThermalMode = (CHCNetSDK.NET_DVR_THERMOMETRY_MODE)Marshal.PtrToStructure(m_struSTDConfig.lpOutBuffer, typeof(CHCNetSDK.NET_DVR_THERMOMETRY_MODE));
                comboThermMode.SelectedIndex = m_struThermalMode.byMode;
                comboBoxROIEnabled.SelectedIndex = m_struThermalMode.byThermometryROIEnabled;
            }

            Marshal.FreeHGlobal(ptrOutBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        private void btnThermModeSet_Click(object sender, EventArgs e)
        {
            m_struSTDConfig.lpCondBuffer = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(m_struSTDConfig.lpCondBuffer, (int)m_iCurChanNo);
            m_struSTDConfig.dwCondSize = sizeof(int);

            int InSize = Marshal.SizeOf(m_struThermalMode);
            m_struThermalMode.dwSize = (uint)InSize;

            m_struThermalMode.byMode = (byte)comboThermMode.SelectedIndex;
            m_struThermalMode.byThermometryROIEnabled = (byte)comboBoxROIEnabled.SelectedIndex;

            IntPtr ptrInBuffer = Marshal.AllocHGlobal(InSize);
            Marshal.StructureToPtr(m_struThermalMode, ptrInBuffer, false);
            m_struSTDConfig.lpInBuffer = ptrInBuffer;
            m_struSTDConfig.dwInSize = (uint)InSize;
            m_struSTDConfig.lpOutBuffer = IntPtr.Zero;
            m_struSTDConfig.dwOutSize = 0;
            m_struSTDConfig.lpStatusBuffer = Marshal.AllocHGlobal(CHCNetSDK.XML_ABILITY_OUT_LEN);
            m_struSTDConfig.dwStatusSize = CHCNetSDK.XML_ABILITY_OUT_LEN;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_SetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_THERMOMETRY_MODE, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：设置测温模式，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("设置测温模式，成功！");
            }

            Marshal.FreeHGlobal(ptrInBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        // 测温规则配置
        private void btnThermRules_Click(object sender, EventArgs e)
        {
            FormRulesParam dlg = new FormRulesParam();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.m_iChannel = m_iCurChanNo;
            dlg.m_lUserID = m_lUserID;
            dlg.ShowDialog();
        }

        private void btnThermBasicParam_Click(object sender, EventArgs e)
        {
            FormBasicParam dlg = new FormBasicParam();

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.m_iChannel = m_iCurChanNo;
            dlg.m_lUserID = m_lUserID;
            dlg.Show();
        }

        //预览
        private void btnRealPlay_Click(object sender, EventArgs e)
        {
            m_struPreviewInfo.lChannel = m_iCurChanNo;
            m_struPreviewInfo.dwStreamType = (uint)comboBoxStreamType.SelectedIndex;
            m_struPreviewInfo.bBlocked = true;
            m_struPreviewInfo.DisplayBufNum = 0;
            m_struPreviewInfo.hPlayWnd = pictureBoxPlay.Handle;
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
                    btnRealPlay.Text = "开始预览";
                    pictureBoxPlay.Refresh();
                }
            }
            else
            {
                m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref m_struPreviewInfo, null, m_pUser);
                if (m_lRealHandle < 0)
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "Thermal：预览失败，错误码：" + iLastErr;
                    MessageBox.Show(strErr);
                }
                else
                {
                    string strTmp = String.Format("Thermal：预览成功");
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "SDK:", strTmp);
                    m_bPlay = true;
                    btnRealPlay.Text = "停止预览";
                }
            }

        }

        // 报警
        private void btnSetUpAlarmChan_Click(object sender, EventArgs e)
        {
            FormThermometryAlarm dlg = new FormThermometryAlarm();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.m_iChannel = m_iCurChanNo;
            dlg.m_lUserID = m_lUserID;
            dlg.ShowDialog();
        }

        // 测温实时数据
        private void btnThermometryUpLoad_Click(object sender, EventArgs e)
        {
            FormRealTimeThermometry dlg = new FormRealTimeThermometry();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.m_iChannel = m_iCurChanNo;
            dlg.m_lUserID = m_lUserID;
            dlg.ShowDialog();
        }

        // 手动测温
        private void btnManualTherm_Click(object sender, EventArgs e)
        {
            FormManualThermometry dlg = new FormManualThermometry();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.m_iChannel = m_iCurChanNo;
            dlg.m_lUserID = m_lUserID;
            dlg.ShowDialog();
        }

        // 抓热图
        private void btnThermCapture_Click(object sender, EventArgs e)
        {
            const int ciPictureBufSize = 2 * 1024 * 1024;//2M

            int dwSize = Marshal.SizeOf(m_JEPGWithAppendData);
            IntPtr ptr = Marshal.AllocHGlobal(dwSize);
            IntPtr JpegBuf = Marshal.AllocHGlobal(ciPictureBufSize);
            IntPtr DataBuf = Marshal.AllocHGlobal(ciPictureBufSize);
            m_JEPGWithAppendData.pJPEGPicBuff = JpegBuf;
            m_JEPGWithAppendData.pP2PDataBuff = DataBuf;
            Marshal.StructureToPtr(m_JEPGWithAppendData, ptr, false);

            m_lCaptureHandle = CHCNetSDK.NET_DVR_CaptureJPEGPicture_WithAppendData(m_lUserID, m_iCurChanNo, ptr);


            if (m_lCaptureHandle >= 0)
            {
                m_JEPGWithAppendData = (CHCNetSDK.NET_DVR_JPEGPICTURE_WITHAPPENDDATA)Marshal.PtrToStructure(ptr, typeof(CHCNetSDK.NET_DVR_JPEGPICTURE_WITHAPPENDDATA));

                string sIP = g_deviceTree.GetSelectedDeviceInfo().sDeviceIP;
                System.DateTime currentTime = new System.DateTime();
                currentTime = System.DateTime.Now;
                string strDate = currentTime.ToString("yyyyMMddhhmmss");
                DirectoryInfo dirPath = Directory.CreateDirectory("C:\\Picture\\[" + sIP + "]\\[" + strDate + "]");
                string CJpegPicName = dirPath.FullName + "\\[" + strDate + "]" + ".jpeg";

                if (m_JEPGWithAppendData.dwP2PDataLen == 4 * m_JEPGWithAppendData.dwJPEGPicHeight * m_JEPGWithAppendData.dwJPEGPicWidth)
                {
                    if (!File.Exists(CJpegPicName))
                    {
                        FileStream fs = new FileStream(CJpegPicName, FileMode.Create);
                        int iLen = (int)m_JEPGWithAppendData.dwJPEGPicLen;
                        byte[] by = new byte[iLen];
                        Marshal.Copy(m_JEPGWithAppendData.pJPEGPicBuff, by, 0, iLen);
                        fs.Write(by, 0, iLen);
                        fs.Close();
                    }

                    string CP2PDataName = dirPath.FullName + "\\[" + strDate + "]" + ".csv";
                    FileStream ps = new FileStream(CP2PDataName, FileMode.Append, FileAccess.Write);


                    int iWriteBufferLen = (int)(m_JEPGWithAppendData.dwP2PDataLen / 4);
                    float[] fWriteBuffer = new float[iWriteBufferLen];

                    Marshal.Copy(m_JEPGWithAppendData.pP2PDataBuff, fWriteBuffer, 0, iWriteBufferLen);

                    int iIndex = 0;

                    for (int iWriteHeight = 0; iWriteHeight < m_JEPGWithAppendData.dwJPEGPicHeight; iWriteHeight++)
                    {
                        string sWriteBuffer = "";

                        for (int iWriteWidth = 0; iWriteWidth < m_JEPGWithAppendData.dwJPEGPicWidth; iWriteWidth++)
                        {
                            string sfWriteBuF = "";
                            sfWriteBuF = string.Format("{0:0.00},", fWriteBuffer[iIndex]);
                            iIndex++;
                            sWriteBuffer += string.Concat(sfWriteBuF);
                        }
                        sWriteBuffer += string.Concat("\n");
                        int iiLen = sWriteBuffer.Length;
                        byte[] py = new byte[iiLen];
                        py = System.Text.Encoding.Default.GetBytes(sWriteBuffer);
                        ps.Write(py, 0, iiLen);
                        ps.Flush();
                    }
                    ps.Close();

                }
                else
                {
                    string strTmp = String.Format("抓热图：返回的数据长度有误");
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDK:", strTmp);
                }

                textBoxPicPath.Text = dirPath.FullName;
            }
            else
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：抓热图，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }

            Marshal.FreeHGlobal(ptr);
            Marshal.FreeHGlobal(JpegBuf);
            Marshal.FreeHGlobal(DataBuf);
        }

        private void btnSetVisibleInfo_Click(object sender, EventArgs e)
        {
            if (m_lRealHandle < 0)
            {
                MessageBox.Show("请先预览！");
                return;
            }
            int iPlayPort = CHCNetSDK.NET_DVR_GetRealPlayerIndex(m_lRealHandle);
            if (checkBoxVisibleInfo.Checked)
            {
                if (PlayCtrl.PlayM4_SetOverlayPriInfoFlag((uint)iPlayPort, 0x20, true))
                {
                    MessageBox.Show("开始抓图叠加温度信息！");
                }
            }
            else
            {
                if (PlayCtrl.PlayM4_SetOverlayPriInfoFlag((uint)iPlayPort, 0x20, false))
                {
                    MessageBox.Show("关闭抓图叠加温度信息！");
                }
            }


        }

        // 抓图
        private void btnCapture_Click(object sender, EventArgs e)
        {
            if (m_lRealHandle < 0)
            {
                MessageBox.Show("请先预览！");
                return;
            }
            int iPlayPort = CHCNetSDK.NET_DVR_GetRealPlayerIndex(m_lRealHandle);

            //uint nDecodeType = 0;
            //uint nDisplayEngine = 3;

            //if (!PlayCtrl.PlayM4_SetDecodeOrDisplayMode(iPlayPort, nDecodeType, nDisplayEngine))
            //{
            //    int iLastErr = Convert.ToInt32(PlayCtrl.PlayM4_GetLastError(iPlayPort));
            //    MessageBox.Show("设置D3D11渲染模式失败！ " + iLastErr);
            //}
            uint nDecodeType = 0;
            uint nDisplayEngine = 0;
//             if (!PlayCtrl.PlayM4_GetDecodeOrDisplayMode(iPlayPort, ref nDecodeType, ref nDisplayEngine))
//             {
//                 MessageBox.Show("获取渲染模式失败！");
//                 return;
//             }

            if (nDisplayEngine == 3)
            {
//                 int pWidth = 0;
//                 int pHeight = 0;
//                 if (!PlayCtrl.PlayM4_GetPictureSize(iPlayPort, ref pWidth, ref pHeight))
//                 {
//                     MessageBox.Show("获取分辨率失败！");
//                     return;
//                 }
// 
//                 PlayCtrl.D3D_PIC_INFO pstPicInfo = new PlayCtrl.D3D_PIC_INFO();
//                 pstPicInfo.pBuf = Marshal.AllocHGlobal(pWidth * pHeight * 2);
//                 pstPicInfo.nBufSize = (uint)(pWidth * pHeight * 2);
//                 pstPicInfo.pPicSize = Marshal.AllocHGlobal(4);
// 
//                 uint nType = 0;
//                 if (PlayCtrl.PlayM4_GetD3DCapture(iPlayPort, nType, ref pstPicInfo))
//                 {
//                     string sIP = g_deviceTree.GetSelectedDeviceInfo().sDeviceIP;
//                     System.DateTime currentTime = new System.DateTime();
//                     currentTime = System.DateTime.Now;
//                     string strDate = currentTime.ToString("yyyyMMddhhmmss");
//                     DirectoryInfo dirPath = Directory.CreateDirectory("C:\\Picture\\[" + sIP + "]");
//                     string strJpegPicName = dirPath.FullName + "\\[" + strDate + "]" + ".jpeg";
//                     if (!File.Exists(strJpegPicName))
//                     {
//                         FileStream fs = new FileStream(strJpegPicName, FileMode.Create);
//                         int iLen = (int)pstPicInfo.nBufSize;
//                         byte[] by = new byte[iLen];
//                         Marshal.Copy(pstPicInfo.pBuf, by, 0, iLen);
//                         fs.Write(by, 0, iLen);
//                         fs.Close();
//                     }
// 
//                     textBoxPicPath.Text = dirPath.FullName;
//                 }
//                 else
//                 {
//                     int iLastErr = Convert.ToInt32(PlayCtrl.PlayM4_GetLastError(iPlayPort));
//                     string strErr = "SDK：抓图，错误码：" + iLastErr;
//                     MessageBox.Show(strErr);
//                 }
// 
//                 Marshal.FreeHGlobal(pstPicInfo.pBuf);
//                 Marshal.FreeHGlobal(pstPicInfo.pPicSize);
//                 return;
            }


            uint nBufSize = 1024 * 1024 * sizeof(byte);
            IntPtr pJpegBuffer = Marshal.AllocHGlobal((int)nBufSize);
            uint pJpegSize = 0;
            if (PlayCtrl.PlayM4_GetJPEG(iPlayPort, pJpegBuffer, nBufSize, ref pJpegSize))
            {
                string sIP = g_deviceTree.GetSelectedDeviceInfo().sDeviceIP;
                System.DateTime currentTime = new System.DateTime();
                currentTime = System.DateTime.Now;
                string strDate = currentTime.ToString("yyyyMMddhhmmss");
                DirectoryInfo dirPath = Directory.CreateDirectory("C:\\Picture\\[" + sIP + "]");
                string strJpegPicName = dirPath.FullName + "\\[" + strDate + "]" + ".jpeg";
                if (!File.Exists(strJpegPicName))
                {
                    FileStream fs = new FileStream(strJpegPicName, FileMode.Create);
                    int iLen = (int)pJpegSize;
                    byte[] by = new byte[iLen];
                    Marshal.Copy(pJpegBuffer, by, 0, iLen);
                    fs.Write(by, 0, iLen);
                    fs.Close();
                }

                textBoxPicPath.Text = dirPath.FullName;
            }
            else
            {
                int iLastErr = Convert.ToInt32(PlayCtrl.PlayM4_GetLastError(iPlayPort));
                string strErr = "SDK：抓图，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }

            Marshal.FreeHGlobal(pJpegBuffer);
        }

        //录像
        private void btnSaveVideo_Click(object sender, EventArgs e)
        {
            if (m_lRealHandle < 0)
            {
                MessageBox.Show("请先预览！");
                return;
            }

            if (!bSaveVideo)
            {
                string sIP = g_deviceTree.GetSelectedDeviceInfo().sDeviceIP;
                System.DateTime currentTime = new System.DateTime();
                currentTime = System.DateTime.Now;
                string strDate = currentTime.ToString("yyyyMMddhhmmss");
                DirectoryInfo dirPath = Directory.CreateDirectory("C:\\Video\\[" + sIP + "]");

                string strVideoPicName = dirPath.FullName + "\\[" + strDate + "]" + comboBoxVideoType.Text;
                if (!CHCNetSDK.NET_DVR_SaveRealData(m_lRealHandle, strVideoPicName))
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "SDK：录像，错误码：" + iLastErr;
                    MessageBox.Show(strErr);
                }
                else
                {
                    textBoxVideoPath.Text = dirPath.FullName;
                    bSaveVideo = true;
                    btnSaveVideo.Text = "停止录像";
                }
            }
            else
            {
                if (!CHCNetSDK.NET_DVR_StopSaveRealData(m_lRealHandle))
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "SDK：停止录像，错误码：" + iLastErr;
                    MessageBox.Show(strErr);
                }
                else
                {
                    bSaveVideo = false;
                    btnSaveVideo.Text = "录像";
                }
            }

        }

        // 打开文件
        private void btnOpenPicFile_Click(object sender, EventArgs e)
        {
            if ("" == textBoxPicPath.Text)
            {
                MessageBox.Show("缺少文件名！");
                return;
            }
            System.Diagnostics.Process.Start(textBoxPicPath.Text);
        }

        private void btnOpenVideoFile_Click(object sender, EventArgs e)
        {
            if ("" == textBoxVideoPath.Text)
            {
                MessageBox.Show("缺少文件名！");
                return;
            }
            System.Diagnostics.Process.Start(textBoxVideoPath.Text);
        }

        // 聚焦
        private void btnFocus_Click(object sender, EventArgs e)
        {
            FormFocusCameraParam dlg = new FormFocusCameraParam();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.m_iChannel = m_iCurChanNo;
            dlg.m_lUserID = m_lUserID;
            dlg.ShowDialog();
        }

        // 手动获取测温规则温度信息
        private void btnRuleTherm_Click(object sender, EventArgs e)
        {
            FormManualRuleThermmometry dlg = new FormManualRuleThermmometry();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.m_iChannel = m_iCurChanNo;
            dlg.m_lUserID = m_lUserID;
            dlg.ShowDialog();

        }

        // 获取文件路径
        private void btnGetFilePath_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "C# Corner Open File Dialog";
            fdlg.Filter = "All files（*.*）|*.*|All files(*.*)|*.* ";
            fdlg.RestoreDirectory = false;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                strFilePath = System.IO.Path.GetFullPath(fdlg.FileName);
                textBoxFilePath.Text = strFilePath;
            }
        }

        // 上传标定文件
        private void btnUpLoad_Click(object sender, EventArgs e)
        {
            if (!m_bUpLoading)
            {
                FileInfo file = new FileInfo(strFilePath);
                if (file.Length == 0)
                {
                    MessageBox.Show("配置文件为空！");
                    return;
                }

                m_iUploadHandle = CHCNetSDK.NET_DVR_UploadFile_V40(m_lUserID, CHCNetSDK.UPLOAD_THERMOMETRIC_FILE, IntPtr.Zero, 0, strFilePath, IntPtr.Zero, 0);
                if (m_iUploadHandle < 0)
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "Thermal：上传标定文件，错误码：" + iLastErr;
                    MessageBox.Show(strErr);

                    CHCNetSDK.NET_DVR_StopUploadFile(m_iUploadHandle);

                    return;
                }
                else
                {
                    int dwProgress = 0;
                    int dwState = 0;

                    IntPtr pProgress = Marshal.AllocHGlobal(Marshal.SizeOf(dwProgress));
                    Marshal.WriteInt32(pProgress, dwProgress);

                    while (true)
                    {
                        dwState = CHCNetSDK.NET_DVR_GetUploadState(m_iUploadHandle, pProgress);
                        dwProgress = Marshal.ReadInt32(pProgress);

                        if (dwState == 1)
                        {
                            MessageBox.Show("上传成功！");
                            m_bUpLoading = false;
                            break;
                        }
                        else if (dwState == 2)
                        {
                            MessageBox.Show("正在上传,已上传: " + dwProgress);
                        }
                        else if (dwState == 3)
                        {
                            MessageBox.Show("上传失败:");
                            break;
                        }
                        else if (dwState == 4)
                        {
                            if (dwProgress == 100)
                            {
                                MessageBox.Show("上传成功！");
                                m_bUpLoading = false;
                                break;
                            }
                            else
                            {
                                MessageBox.Show("网络断开，状态未知", "Network disconnect, status unknown");
                                break;
                            }
                        }

                        if (dwState != 2 && dwState != 5)
                        {
                            CHCNetSDK.NET_DVR_UploadClose(m_iUploadHandle);   // break已经跳出循环，会执行到这儿？
                            m_bUpLoading = true;
                            btnUpLoad.Text = "停止上传";
                        }
                    }   //结束上传的过程

                }
            }
            else
            {
                CHCNetSDK.NET_DVR_UploadClose(m_iUploadHandle);
                m_bUpLoading = false;
                btnUpLoad.Text = "上传";
            }
        }

        // 下载标定文件
        private void btnStartDownload_Click(object sender, EventArgs e)
        {
            if ("" == strFilePath)
            {
                MessageBox.Show("选择文件下载 ！");
                return;
            }
            if (!m_bDownLoading)
            {
                FileInfo file = new FileInfo(strFilePath);
                if (file.Length == 0)
                {
                    MessageBox.Show("配置文件为空！");
                    return;
                }

                m_iDownloadHandle = CHCNetSDK.NET_DVR_StartDownload(m_lUserID, CHCNetSDK.NET_SDK_DOWNLOAD_THERMOMETRIC_FILE, IntPtr.Zero, 0, strFilePath);

                if (m_iDownloadHandle < 0)
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "Thermal：下载标定文件，错误码：" + iLastErr;
                    MessageBox.Show(strErr);
                    return;
                }
                else
                {
                    int dwProgress = 0;
                    int dwState = 0;

                    IntPtr pProgress = Marshal.AllocHGlobal(Marshal.SizeOf(dwProgress));
                    Marshal.WriteInt32(pProgress, dwProgress);

                    while (true)
                    {
                        dwState = CHCNetSDK.NET_DVR_GetDownloadState(m_iDownloadHandle, pProgress);
                        dwProgress = Marshal.ReadInt32(pProgress);

                        if (dwState == 1)
                        {
                            MessageBox.Show("下载成功", "Download successfully");
                            m_bDownLoading = false;
                            break;
                        }
                        else if (dwState == 2)
                        {
                            MessageBox.Show("正在下载,已下载: " + dwProgress, "Is Downloading,progress:");
                            m_bDownLoading = true;
                        }
                        else if (dwState == 3)
                        {
                            MessageBox.Show("下载失败", "Download failed");
                            break;
                        }
                        else if (dwState == 4)
                        {
                            if (dwProgress == 100)
                            {
                                MessageBox.Show("下载成功");
                                m_bDownLoading = false;
                                break;
                            }
                            else
                            {
                                MessageBox.Show("网络断开，状态未知", "Network disconnect, status unknown");
                                break;
                            }

                            if (dwState != 2 && dwState != 5)
                            {
                                CHCNetSDK.NET_DVR_StopDownload(m_iDownloadHandle);
                                m_bDownLoading = true;
                                btnStartDownload.Text = "停止下载";
                            }
                        }
                    }  // 结束下载

                }
            }
            else
            {
                CHCNetSDK.NET_DVR_StopDownload(m_iDownloadHandle);
                m_bDownLoading = false;
                btnStartDownload.Text = "下载";
            }
        }

        // 调用播放库叠加温度信息
        private void btnOverlyTherm_Click(object sender, EventArgs e)
        {
            int iPlayerPort = CHCNetSDK.NET_DVR_GetRealPlayerIndex(m_lRealHandle);
            bool bRet = false;
            int dwPRIDATA_RENDER = (int)(PlayCtrl.PLAYM4_PRIDATA_RENDER.PLAYM4_RENDER_ANA_INTEL_DATA | PlayCtrl.PLAYM4_PRIDATA_RENDER.PLAYM4_RENDER_MD | PlayCtrl.PLAYM4_PRIDATA_RENDER.PLAYM4_RENDER_ADD_POS | PlayCtrl.PLAYM4_PRIDATA_RENDER.PLAYM4_RENDER_ADD_PIC | PlayCtrl.PLAYM4_PRIDATA_RENDER.PLAYM4_RENDER_FIRE_DETCET | PlayCtrl.PLAYM4_PRIDATA_RENDER.PLAYM4_RENDER_TEM);

            if (!m_bShowOSD)
            {
                if (iPlayerPort >= 0)
                {
                    bRet = PlayCtrl.PlayM4_RenderPrivateData((uint)iPlayerPort, dwPRIDATA_RENDER, true);
                    if (!bRet)
                    {
                        MessageBox.Show("调用PlayM4_EnablePOS接口设置 打开OSD 失败");
                        return;
                    }
                    dwPRIDATA_RENDER = (int)(PlayCtrl.PLAYM4_FIRE_ALARM.PLAYM4_FIRE_FRAME_DIS | PlayCtrl.PLAYM4_FIRE_ALARM.PLAYM4_FIRE_MAX_TEMP | PlayCtrl.PLAYM4_FIRE_ALARM.PLAYM4_FIRE_MAX_TEMP_POSITION | PlayCtrl.PLAYM4_FIRE_ALARM.PLAYM4_FIRE_DISTANCE);
                    PlayCtrl.PlayM4_RenderPrivateDataEx((uint)iPlayerPort, (int)PlayCtrl.PLAYM4_PRIDATA_RENDER.PLAYM4_RENDER_FIRE_DETCET, dwPRIDATA_RENDER, true);
                    dwPRIDATA_RENDER = (int)(PlayCtrl.PLAYM4_TEM_FLAG.PLAYM4_TEM_REGION_BOX | PlayCtrl.PLAYM4_TEM_FLAG.PLAYM4_TEM_REGION_LINE | PlayCtrl.PLAYM4_TEM_FLAG.PLAYM4_TEM_REGION_POINT);
                    PlayCtrl.PlayM4_RenderPrivateDataEx((uint)iPlayerPort, (int)PlayCtrl.PLAYM4_PRIDATA_RENDER.PLAYM4_RENDER_TEM, dwPRIDATA_RENDER, true);
                    m_bShowOSD = true;
                    btnOverlyTherm.Text = "关闭温度信息";
                }
                else
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "Thermal：获取预览时用来解码显示的播放库句柄失败，错误码：" + iLastErr;
                    MessageBox.Show(strErr);
                    return;
                }
            }
            else
            {
                bRet = PlayCtrl.PlayM4_RenderPrivateData((uint)iPlayerPort, dwPRIDATA_RENDER, false);
                if (!bRet)
                {
                    MessageBox.Show("调用PlayM4_EnablePOS接口 关闭OSD 失败");
                    return;
                }
                m_bShowOSD = false;
                btnOverlyTherm.Text = "叠加温度信息";

            }
        }

        //测温矫正
        private void btnCorrectTherm_Click(object sender, EventArgs e)
        {
            FormCorrectThermometry dlg = new FormCorrectThermometry();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.m_iChannel = m_iCurChanNo;
            dlg.m_lUserID = m_lUserID;
            dlg.ShowDialog();
        }

        //智能规则
        private void btnThermIntelrule_Click(object sender, EventArgs e)
        {
            FormThermometryIntelRule dlg = new FormThermometryIntelRule();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.m_iChannel = m_iCurChanNo;
            dlg.m_lUserID = m_lUserID;
            dlg.ShowDialog();
        }

        //测温算法
        private void btnThermAlg_Click(object sender, EventArgs e)
        {
            FormThermAlg dlg = new FormThermAlg();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.m_iChannel = m_iCurChanNo;
            dlg.m_lUserID = m_lUserID;
            dlg.ShowDialog();
        }

        //防灼伤
        private void btnBurningPrev_Click(object sender, EventArgs e)
        {
            m_struXMLConfigInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
            m_struXMLConfigOutput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();
            int iInSize = Marshal.SizeOf(m_struXMLConfigInput);
            m_struXMLConfigInput.dwSize = (uint)iInSize;
            string strRequestUrl = "GET /ISAPI/Thermal/channels/" + m_iCurChanNo + "/burningPrevention\r\n";
            uint dwRequestUrlLen = (uint)strRequestUrl.Length;
            m_struXMLConfigInput.lpRequestUrl = Marshal.StringToHGlobalAnsi(strRequestUrl);
            m_struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            IntPtr lpInputParam = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(m_struXMLConfigInput, lpInputParam, false);

            int iOutSize = Marshal.SizeOf(m_struXMLConfigInput);
            m_struXMLConfigOutput.dwSize = (uint)iOutSize;
            m_struXMLConfigOutput.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.MAX_LEN_XML);
            m_struXMLConfigOutput.dwOutBufferSize = (uint)Marshal.SizeOf(CHCNetSDK.MAX_LEN_XML);
            IntPtr lpOutputParam = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struXMLConfigOutput, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(m_lUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Thermal: 获取防灼伤参数，错误码：" + iLastErr; ;
                MessageBox.Show(strErr);
            }
            else
            {
                string strOutputParam = Marshal.PtrToStringAnsi(m_struXMLConfigOutput.lpOutBuffer);

                CTinyXmlTrans XMLBASE = new CTinyXmlTrans();
                XMLBASE.Parse(strOutputParam);
                XMLBASE.SetRoot();

                if (XMLBASE.FindElemFromBegin("BurningPrevention") && XMLBASE.IntoElem())
                {
                    if (XMLBASE.FindElemFromBegin("enabled"))
                    {
                        if ("true" == XMLBASE.GetData())
                        {
                            MessageBox.Show("已启用");
                            btnBurningPrev.Text = "停止";
                            m_bBurningPrevEnable = true;
                        }
                        else if ("false" == XMLBASE.GetData())
                        {
                            string strEnabled = "true";
                            if (true == m_bBurningPrevEnable)
                            {
                                strEnabled = "<enabled>" + "false" + "</enabled>\r\n";
                            }
                            strOutputParam = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<BurningPrevention version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n" +
                                strEnabled + "</BurningPrevention>\r\n";

                            if (SetBurningPrev(strOutputParam))
                            {
                                MessageBox.Show("防灼伤，启用！");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("enabled 节点不存在");
                    }
                }
                else
                {
                    MessageBox.Show("BurningPrevention 节点不存在");
                }
            }

            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);
        }

        public bool SetBurningPrev(string strInputParam)
        {
            m_struXMLConfigInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
            m_struXMLConfigOutput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();
            bool bBurningPrev = false;
            int iInSize = Marshal.SizeOf(m_struXMLConfigInput);
            m_struXMLConfigInput.dwSize = (uint)iInSize;
            string strRequestUrl = "PUT /ISAPI/Thermal/channels/" + m_iCurChanNo + "/burningPrevention\r\n";
            uint dwRequestUrlLen = (uint)strRequestUrl.Length;
            m_struXMLConfigInput.lpRequestUrl = Marshal.StringToHGlobalAnsi(strRequestUrl);
            m_struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            IntPtr lpInputParam = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(m_struXMLConfigInput, lpInputParam, false);
            m_struXMLConfigInput.lpInBuffer = Marshal.StringToCoTaskMemAnsi(strInputParam);
            m_struXMLConfigInput.dwInBufferSize = (uint)Marshal.SizeOf(CHCNetSDK.MAX_LEN_XML);
            int iOutSize = Marshal.SizeOf(m_struXMLConfigInput);
            m_struXMLConfigOutput.dwSize = (uint)iOutSize;
            m_struXMLConfigOutput.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.MAX_LEN_XML);
            m_struXMLConfigOutput.dwOutBufferSize = (uint)Marshal.SizeOf(CHCNetSDK.MAX_LEN_XML);
            IntPtr lpOutputParam = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struXMLConfigOutput, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(m_lUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "防灼伤启用停用失败，错误码：" + iLastErr;
                MessageBox.Show(strErr);

                bBurningPrev = false;
            }
            else
            {
                bBurningPrev = true;
            }

            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);

            return bBurningPrev;
        }

        private void btnStopBurningPrev_Click(object sender, EventArgs e)
        {
            string strEnabled = "<enabled>" + "false" + "</enabled>\r\n";
            string strOutputParam = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<BurningPrevention version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n" +
                   strEnabled + "</BurningPrevention>\r\n";

            if (SetBurningPrev(strOutputParam))
            {
                MessageBox.Show("防灼伤，停用！");
            }
        }

        private void FormSDKThermometry_Load(object sender, EventArgs e)
        {
            panel1.AutoScroll = true; // 设置panel控件的自动滑动条
        }

        private void btnThermalStream_Click(object sender, EventArgs e)
        {
            FormThermalStream dlg = new FormThermalStream();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.m_iChannel = m_iCurChanNo;
            dlg.m_lUserID = m_lUserID;
            dlg.Show();
        }

        private void btnXML_Click(object sender, EventArgs e)
        {
            FormXML dlg = new FormXML();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.m_iChannel = m_iCurChanNo;
            dlg.m_lUserID = m_lUserID;
            dlg.Show();

        }

        private void btnClickToThermInit_Click(object sender, EventArgs e)
        {
            m_struXMLConfigInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
            m_struXMLConfigOutput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();

            int iInSize = Marshal.SizeOf(m_struXMLConfigInput);
            m_struXMLConfigInput.dwSize = (uint)iInSize;
            string strRequestUrl = "PUT /ISAPI/Thermal/channels/" + m_iCurChanNo + "/clickToThermometry/initialization\r\n";
            uint dwRequestUrlLen = (uint)strRequestUrl.Length;
            m_struXMLConfigInput.lpRequestUrl = Marshal.StringToHGlobalAnsi(strRequestUrl);
            m_struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            IntPtr lpInputParam = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(m_struXMLConfigInput, lpInputParam, false);

            int iOutSize = Marshal.SizeOf(m_struXMLConfigOutput);
            m_struXMLConfigOutput.dwSize = (uint)iOutSize;
            m_struXMLConfigOutput.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.MAX_LEN_XML);
            m_struXMLConfigOutput.dwOutBufferSize = CHCNetSDK.MAX_LEN_XML;
            IntPtr lpOutputParam = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struXMLConfigOutput, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(m_lUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Thermal: 初始化点击测温，错误码：" + iLastErr; ;
                MessageBox.Show(strErr);
            }

            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);
        }


        private void pictureBoxPlay_MouseDown(object sender, MouseEventArgs e)
        {
            Point point = pictureBoxPlay.PointToClient(Control.MousePosition);

            float fPositionX = ((float)point.X / pictureBoxPlay.Width);
            float fPositionY = ((float)point.Y / pictureBoxPlay.Height);
            textBoxpositionX.Text = fPositionX.ToString("#0.000");
            textBoxpositionY.Text = fPositionY.ToString("#0.000");

            //组装Json
            m_jsonClickToThermtry = new jsonClickToThermometry();
            m_jsonClickToThermtry.ClickToThermometryRule = new jsonClickToThermometryRule();
            m_jsonClickToThermtry.ClickToThermometryRule.Point = new jsonPoint();
            m_jsonClickToThermtry.ClickToThermometryRule.Point.positionX = Convert.ToSingle(textBoxpositionX.Text);
            m_jsonClickToThermtry.ClickToThermometryRule.Point.positionY = Convert.ToSingle(textBoxpositionY.Text); ;

            string strInput = JsonConvert.SerializeObject(m_jsonClickToThermtry);

            int iInSize = Marshal.SizeOf(m_struXMLConfigInput);
            m_struXMLConfigInput.dwSize = (uint)iInSize;
            string strRequestUrl = "PUT /ISAPI/Thermal/channels/" + m_iCurChanNo + "/clickToThermometry/rules/" + textBoxRuleID.Text + "?format=json\r\n";
            uint dwRequestUrlLen = (uint)strRequestUrl.Length;
            m_struXMLConfigInput.lpRequestUrl = Marshal.StringToHGlobalAnsi(strRequestUrl);
            m_struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            m_struXMLConfigInput.dwInBufferSize = (uint)strInput.Length;
            m_struXMLConfigInput.lpInBuffer = Marshal.StringToHGlobalAnsi(strInput);
            IntPtr lpInputParam = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(m_struXMLConfigInput, lpInputParam, false);

            int iOutSize = Marshal.SizeOf(m_struXMLConfigOutput);
            m_struXMLConfigOutput.dwSize = (uint)iOutSize;
            m_struXMLConfigOutput.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.MAX_LEN_XML);
            m_struXMLConfigOutput.dwOutBufferSize = CHCNetSDK.MAX_LEN_XML;
            IntPtr lpOutputParam = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struXMLConfigOutput, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(m_lUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Thermal: 设置点击测温规则，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }

            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);
        }

      
    }
}