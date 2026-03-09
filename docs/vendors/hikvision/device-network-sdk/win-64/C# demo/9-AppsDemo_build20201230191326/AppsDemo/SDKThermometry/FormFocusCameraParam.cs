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
using TINYXMLTRANS;

namespace SDKThermometry
{
    public partial class FormFocusCameraParam : Form
    {
        public int m_iChannel = -1;
        public int m_lUserID = -1;
        private CHCNetSDK.NET_DVR_FOCUSMODE_CFG m_struFocusModeCfg;
        private CHCNetSDK.NET_DVR_CAMERAPARAMCFG m_strCameraParamCfg = new CHCNetSDK.NET_DVR_CAMERAPARAMCFG();
        public FormFocusCameraParam()
        {
            InitializeComponent();

            // 聚焦参数初始化
            comboBoxFocusMode.SelectedIndex = 0;
            comboBoxAutoFocusMode.SelectedIndex = 0;
            comboBoxFocusSpeedLevel.SelectedIndex = 0;
            comboBoxZoomSpeedLevel.SelectedIndex = 0;
            comboBoxFocusDefinition.SelectedIndex = 0;
            comboBoxFocusSensitivity.SelectedIndex = 0;

            //前端参数
            //comboBoxPaletteMode.SelectedIndex = 0;
            //comboBoxEnhancedMode.SelectedIndex = 0;
        }

        // 聚焦
        private void btnGetFocus_Click(object sender, EventArgs e)
        {
            m_struFocusModeCfg = new CHCNetSDK.NET_DVR_FOCUSMODE_CFG();

            int iOutBufferSize = Marshal.SizeOf(m_struFocusModeCfg);
            IntPtr pOutBuffer = Marshal.AllocHGlobal(iOutBufferSize);
            Marshal.StructureToPtr(m_struFocusModeCfg, pOutBuffer, false);
            uint dwReturned = 0;

            if (!CHCNetSDK.NET_DVR_GetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_FOCUSMODECFG, m_iChannel, pOutBuffer, (uint)iOutBufferSize, ref dwReturned))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：获取聚焦模式，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                m_struFocusModeCfg = (CHCNetSDK.NET_DVR_FOCUSMODE_CFG)Marshal.PtrToStructure(pOutBuffer, typeof(CHCNetSDK.NET_DVR_FOCUSMODE_CFG));

                comboBoxFocusMode.SelectedIndex = m_struFocusModeCfg.byFocusMode;
                comboBoxAutoFocusMode.SelectedIndex = m_struFocusModeCfg.byAutoFocusMode;
                textBoxMinFocusDistance.Text = Convert.ToString(m_struFocusModeCfg.wMinFocusDistance);
                comboBoxFocusSpeedLevel.SelectedIndex = m_struFocusModeCfg.byFocusSpeedLevel - 1;
                textBoxOpticalZoom.Text = Convert.ToString(m_struFocusModeCfg.byOpticalZoom);
                textBoxDigtitalZoom.Text = Convert.ToString(m_struFocusModeCfg.byDigtitalZoom);
                textBoxOpticalZoomLevel.Text = Convert.ToString(m_struFocusModeCfg.fOpticalZoomLevel);
                comboBoxZoomSpeedLevel.SelectedIndex = m_struFocusModeCfg.byZoomSpeedLevel - 1;
                comboBoxFocusDefinition.SelectedIndex = m_struFocusModeCfg.byFocusDefinitionDisplay;
                comboBoxFocusSensitivity.SelectedIndex = m_struFocusModeCfg.byFocusSensitivity;
                textBoxFocusPos.Text = Convert.ToString(m_struFocusModeCfg.dwFocusPos);
                textBoxRelativeFocusPos.Text = Convert.ToString(m_struFocusModeCfg.dwRelativeFocusPos);
            }

            Marshal.FreeHGlobal(pOutBuffer);
        }

        private void btnSetFocus_Click(object sender, EventArgs e)
        {
            m_struFocusModeCfg = new CHCNetSDK.NET_DVR_FOCUSMODE_CFG();

            int iInBufferSize = Marshal.SizeOf(m_struFocusModeCfg);
            m_struFocusModeCfg.dwSize = (uint)iInBufferSize;
            m_struFocusModeCfg.byFocusMode = (byte)comboBoxFocusMode.SelectedIndex;
            m_struFocusModeCfg.byAutoFocusMode = (byte)comboBoxAutoFocusMode.SelectedIndex;
            m_struFocusModeCfg.wMinFocusDistance = Convert.ToUInt16(textBoxMinFocusDistance.Text);
            m_struFocusModeCfg.byFocusSpeedLevel = (byte)(comboBoxFocusSpeedLevel.SelectedIndex + 1);
            m_struFocusModeCfg.byOpticalZoom = Convert.ToByte(textBoxOpticalZoom.Text);
            m_struFocusModeCfg.byDigtitalZoom = Convert.ToByte(textBoxDigtitalZoom.Text);
            m_struFocusModeCfg.fOpticalZoomLevel = Convert.ToSingle(textBoxOpticalZoomLevel.Text);
            m_struFocusModeCfg.byZoomSpeedLevel = (byte)(comboBoxZoomSpeedLevel.SelectedIndex + 1);
            m_struFocusModeCfg.byFocusDefinitionDisplay = (byte)comboBoxFocusDefinition.SelectedIndex;
            m_struFocusModeCfg.byFocusSensitivity = (byte)comboBoxFocusSensitivity.SelectedIndex;
            m_struFocusModeCfg.dwFocusPos = Convert.ToUInt32(textBoxFocusPos.Text);
            m_struFocusModeCfg.dwRelativeFocusPos = Convert.ToUInt32(textBoxRelativeFocusPos.Text);

            IntPtr pInBuffer = Marshal.AllocHGlobal(iInBufferSize);
            Marshal.StructureToPtr(m_struFocusModeCfg, pInBuffer, false);

            if (!CHCNetSDK.NET_DVR_SetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_FOCUSMODECFG, m_iChannel, pInBuffer, (uint)iInBufferSize))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：设置聚焦模式，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("设置聚焦模式：成功！");
            }

            Marshal.FreeHGlobal(pInBuffer);
        }

        private void btnGetCameraParam_Click(object sender, EventArgs e)
        {
            m_strCameraParamCfg = new CHCNetSDK.NET_DVR_CAMERAPARAMCFG();

            int iOutBufferSize = Marshal.SizeOf(m_strCameraParamCfg);
            IntPtr pOutBuffer = Marshal.AllocHGlobal(iOutBufferSize);
            Marshal.StructureToPtr(m_strCameraParamCfg, pOutBuffer, false);
            uint dwReturned = 0;

            if (!CHCNetSDK.NET_DVR_GetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_CCDPARAMCFG, m_iChannel, pOutBuffer, (uint)iOutBufferSize, ref dwReturned))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：获取前端参数，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                m_strCameraParamCfg = (CHCNetSDK.NET_DVR_CAMERAPARAMCFG)Marshal.PtrToStructure(pOutBuffer, typeof(CHCNetSDK.NET_DVR_CAMERAPARAMCFG));

                comboBoxPaletteMode.SelectedIndex = m_strCameraParamCfg.byPaletteMode;
                comboBoxEnhancedMode.SelectedIndex = m_strCameraParamCfg.byEnhancedMode;
            }

            Marshal.FreeHGlobal(pOutBuffer);
        }

        private void btnSetCameraParam_Click(object sender, EventArgs e)
        {
            m_strCameraParamCfg.byPaletteMode = (byte)comboBoxPaletteMode.SelectedIndex;
            m_strCameraParamCfg.byEnhancedMode = (byte)comboBoxEnhancedMode.SelectedIndex;
            int iInBufferSize = Marshal.SizeOf(m_strCameraParamCfg);
            m_struFocusModeCfg.dwSize = (uint)iInBufferSize;
            IntPtr pInBuffer = Marshal.AllocHGlobal(iInBufferSize);
            Marshal.StructureToPtr(m_strCameraParamCfg, pInBuffer, false);

            if (!CHCNetSDK.NET_DVR_SetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_CCDPARAMCFG, m_iChannel, pInBuffer, (uint)iInBufferSize))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：设置前端参数，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("设置前端参数：成功！");
            }

            Marshal.FreeHGlobal(pInBuffer);
        }


    }
}
