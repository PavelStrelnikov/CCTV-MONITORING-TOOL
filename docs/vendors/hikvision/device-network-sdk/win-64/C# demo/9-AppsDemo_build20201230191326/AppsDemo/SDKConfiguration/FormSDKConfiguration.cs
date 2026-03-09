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

namespace SDKConfiguration
{
    public partial class FormSDKConfiguration : PluginsControl
    {
        private IDeviceTree g_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        private int m_iCurDeviceIndex = -1;
        private int m_iCurChanNo = -1;
        private int m_iCurChanIndex = -1;
        private int m_lUserID = -1;
        private int m_ChannelID = -1;
        private int m_DeviceIndex = -1;
        public FormSDKConfiguration()
        {
            InitializeComponent();
            GetLoginInfo();
        }
        private void BtnReStartDev_Click(object sender, EventArgs e)
        {
            if (CHCNetSDK.NET_DVR_RebootDVR(m_lUserID))
            {
                MessageBox.Show("ReBoot Success");
            }
            else
            {
                uint error = CHCNetSDK.NET_DVR_GetLastError();
                MessageBox.Show("ReBoot Failed,Error: "+ error);
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
                m_ChannelID = (int)g_deviceTree.GetSelectedChannelInfo().iChannelIndex;
                m_DeviceIndex = (int)g_deviceTree.GetSelectedChannelInfo().iDeviceIndex;
            }
        }
        //恢复默认参数
        private void BtnRestore_Click(object sender, EventArgs e)
        {
            if (CHCNetSDK.NET_DVR_RestoreConfig(m_lUserID))
            {
                 MessageBox.Show("Restore Success,Please Reboot");
            }
            else
            {
                 MessageBox.Show("Restore Failed");
            }
        }
        //设备升级
        private void BtnUpgrade_Click(object sender, EventArgs e)
        {
            FormUpgrade DlgFormDlg = new FormUpgrade();
            DlgFormDlg.m_lServerID = m_lUserID;
            DlgFormDlg.m_lChannel = m_ChannelID;
            DlgFormDlg.m_iDeviceIndex = m_DeviceIndex;
            DlgFormDlg.ShowDialog();
        }


    }
}
