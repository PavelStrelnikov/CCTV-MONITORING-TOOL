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
    public partial class FormThermAlg : Form
    {
        public int m_iChannel = -1;
        public int m_lUserID = -1;
        private CHCNetSDK.NET_DVR_STD_CONFIG m_struSTDConfig = new CHCNetSDK.NET_DVR_STD_CONFIG();
        private CHCNetSDK.NET_DVR_THERMAL_ALGINFO m_struThermalAlgInfo = new CHCNetSDK.NET_DVR_THERMAL_ALGINFO();
        public FormThermAlg()
        {
            InitializeComponent();
        }

        private void btnGetThermalAlg_Click(object sender, EventArgs e)
        {

            int iOutSize = Marshal.SizeOf(m_struThermalAlgInfo);
            m_struThermalAlgInfo.dwSize = (uint)iOutSize;
            IntPtr ptrOutBuffer = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struThermalAlgInfo, ptrOutBuffer, false);
            m_struSTDConfig.dwOutSize = (uint)iOutSize;
            m_struSTDConfig.lpOutBuffer = ptrOutBuffer;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_GetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_THERMAL_ALGVERSION, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：获取热成像智能规则，错误码：" + iLastErr  ;
                MessageBox.Show(strErr);
            }
            else
            {
                m_struSTDConfig = (CHCNetSDK.NET_DVR_STD_CONFIG)Marshal.PtrToStructure(ptr, typeof(CHCNetSDK.NET_DVR_STD_CONFIG));
                m_struThermalAlgInfo = (CHCNetSDK.NET_DVR_THERMAL_ALGINFO)Marshal.PtrToStructure(m_struSTDConfig.lpOutBuffer, typeof(CHCNetSDK.NET_DVR_THERMAL_ALGINFO));

                textBoxShipsAlg.Text = new string(m_struThermalAlgInfo.sShipsAlgName);
                textBoxThermometryAlg.Text = new string(m_struThermalAlgInfo.sThermometryAlgName);
                textBoxFireAlg.Text = new string(m_struThermalAlgInfo.sFireAlgName);
            }

            Marshal.FreeHGlobal(ptrOutBuffer);
            Marshal.FreeHGlobal(ptr);
        }
    }
}
