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
    public partial class FormBasicParam : Form
    {
        private CHCNetSDK.NET_DVR_THERMOMETRY_BASICPARAM m_thermometryBasicParam = new CHCNetSDK.NET_DVR_THERMOMETRY_BASICPARAM();
        private CHCNetSDK.NET_DVR_STD_CONFIG m_struSTDConfig = new CHCNetSDK.NET_DVR_STD_CONFIG();
        public int m_iChannel = -1;
        public int m_lUserID = -1;
        public FormBasicParam()
        {
            InitializeComponent();
        }

        private void btnThermBasicGet_Click(object sender, EventArgs e)
        {
            m_struSTDConfig.lpCondBuffer = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(m_struSTDConfig.lpCondBuffer, m_iChannel);
            m_struSTDConfig.dwCondSize = sizeof(int);
            m_struSTDConfig.lpInBuffer = IntPtr.Zero;
            m_struSTDConfig.dwInSize = 0;
            m_thermometryBasicParam.dwSize = (uint)Marshal.SizeOf(m_thermometryBasicParam);
            IntPtr ptrOutBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(m_thermometryBasicParam));
            Marshal.StructureToPtr(m_thermometryBasicParam, ptrOutBuffer, false);
            m_struSTDConfig.lpOutBuffer = ptrOutBuffer;
            m_struSTDConfig.dwOutSize = (uint)Marshal.SizeOf(m_thermometryBasicParam);

            m_struSTDConfig.lpStatusBuffer = Marshal.AllocHGlobal(CHCNetSDK.XML_ABILITY_OUT_LEN);
            m_struSTDConfig.dwStatusSize = CHCNetSDK.XML_ABILITY_OUT_LEN;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_GetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_THERMOMETRY_BASICPARAM, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：获取测温基本参数失败，错误码：" + iLastErr ;
                MessageBox.Show(strErr);
            }
            else
            {
                m_struSTDConfig = (CHCNetSDK.NET_DVR_STD_CONFIG)Marshal.PtrToStructure(ptr, typeof(CHCNetSDK.NET_DVR_STD_CONFIG));
                m_thermometryBasicParam = (CHCNetSDK.NET_DVR_THERMOMETRY_BASICPARAM)Marshal.PtrToStructure(m_struSTDConfig.lpOutBuffer, typeof(CHCNetSDK.NET_DVR_THERMOMETRY_BASICPARAM));
                
                chkThemometry.Checked = Convert.ToBoolean(m_thermometryBasicParam.byEnabled);
                chkOverlapTemperature.Checked = Convert.ToBoolean(m_thermometryBasicParam.byStreamOverlay);
                chkPicOverlapOriginal.Checked = Convert.ToBoolean(m_thermometryBasicParam.byPictureOverlay);
                chkTemperatureStrip.Checked = Convert.ToBoolean(m_thermometryBasicParam.byShowTempStripEnable);
                textTransmissivity.Text = Convert.ToString(m_thermometryBasicParam.fThermalOpticalTransmittance);
                textOpticalTemperature.Text = Convert.ToString(m_thermometryBasicParam.fExternalOpticsWindowCorrection);
                comboUnit.SelectedIndex = m_thermometryBasicParam.byThermometryUnit;
                comboRange.SelectedIndex = m_thermometryBasicParam.byThermometryRange;
            }

            Marshal.FreeHGlobal(ptrOutBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        private void btnThermBasicSet_Click(object sender, EventArgs e)
        {
            m_struSTDConfig.lpCondBuffer = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(m_struSTDConfig.lpCondBuffer, m_iChannel);
            m_struSTDConfig.dwCondSize = sizeof(int);

            m_thermometryBasicParam.dwSize = (uint)Marshal.SizeOf(m_thermometryBasicParam);
            m_thermometryBasicParam.byEnabled = Convert.ToByte(chkThemometry.Checked);
            m_thermometryBasicParam.byStreamOverlay = Convert.ToByte(chkOverlapTemperature.Checked);
            m_thermometryBasicParam.byPictureOverlay = Convert.ToByte(chkPicOverlapOriginal.Checked);
            m_thermometryBasicParam.byShowTempStripEnable = Convert.ToByte(chkTemperatureStrip.Checked);
            m_thermometryBasicParam.fThermalOpticalTransmittance = Convert.ToSingle(textTransmissivity.Text);
            m_thermometryBasicParam.fExternalOpticsWindowCorrection = Convert.ToSingle(textOpticalTemperature.Text);
            m_thermometryBasicParam.byThermometryUnit = (byte)comboUnit.SelectedIndex;
            m_thermometryBasicParam.byThermometryRange = (byte)comboRange.SelectedIndex;

            IntPtr ptrInBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(m_thermometryBasicParam));
            Marshal.StructureToPtr(m_thermometryBasicParam, ptrInBuffer, false);
            m_struSTDConfig.lpInBuffer = ptrInBuffer;
            m_struSTDConfig.dwInSize = (uint)Marshal.SizeOf(m_thermometryBasicParam);

            m_struSTDConfig.lpOutBuffer = IntPtr.Zero;
            m_struSTDConfig.dwOutSize = 0;

            m_struSTDConfig.lpStatusBuffer = Marshal.AllocHGlobal(CHCNetSDK.XML_ABILITY_OUT_LEN);
            m_struSTDConfig.dwStatusSize = CHCNetSDK.XML_ABILITY_OUT_LEN;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_SetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_THERMOMETRY_BASICPARAM, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：设置测温基本参数，错误码：" + iLastErr ;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("设置测温基本参数，成功！");
            }

            Marshal.FreeHGlobal(ptrInBuffer);
            Marshal.FreeHGlobal(ptr);
        }

    }
}
