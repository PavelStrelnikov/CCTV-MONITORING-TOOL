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
    public partial class FormManualRuleThermmometry : Form
    {
        public int m_iChannel = -1;
        public int m_lUserID = -1;
        public int m_iRuleID = -1;

        public FormManualRuleThermmometry()
        {
            InitializeComponent();
        }

        private void btnGetRuleTherm_Click(object sender, EventArgs e)
        {
            uint dwReturned = 0;

            if ("" == textBoxRuleID.Text)
            {
                MessageBox.Show("输入“RuleID”值");
                return;
            }
            m_iRuleID = Convert.ToInt32(textBoxRuleID.Text);
           
            if(m_iRuleID < 0)
            {
                MessageBox.Show("输入有效的“RuleID”值");
                return;
            }

            CHCNetSDK.NET_DVR_THERMOMETRYRULE_TEMPERATURE_INFO struThermRuleThermInfo = new CHCNetSDK.NET_DVR_THERMOMETRYRULE_TEMPERATURE_INFO();
            int iOutBufferSize = Marshal.SizeOf(struThermRuleThermInfo);
            IntPtr pOutBuffer = Marshal.AllocHGlobal(iOutBufferSize);
            Marshal.StructureToPtr(struThermRuleThermInfo, pOutBuffer, false);
            if (!CHCNetSDK.NET_DVR_GetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_THERMOMETRYRULE_TEMPERATURE_INFO, m_iRuleID, pOutBuffer, (uint)iOutBufferSize, ref dwReturned))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：手动获取测温规则温度信息，错误码：" + iLastErr  ;
                MessageBox.Show(strErr);
            }
            else 
            {
                struThermRuleThermInfo = (CHCNetSDK.NET_DVR_THERMOMETRYRULE_TEMPERATURE_INFO)Marshal.PtrToStructure(pOutBuffer, typeof(CHCNetSDK.NET_DVR_THERMOMETRYRULE_TEMPERATURE_INFO));

                textBoxMaxTherm.Text =  struThermRuleThermInfo.fMaxTemperature.ToString();
                textBoxMinTherm.Text = struThermRuleThermInfo.fMinTemperature.ToString();
                textBoxAverTherm.Text = struThermRuleThermInfo.fAverageTemperature.ToString();
                textBoxMaxThermX.Text = struThermRuleThermInfo.struHighestPoint.fX.ToString();
                textBoxMaxThermY.Text = struThermRuleThermInfo.struHighestPoint.fY.ToString();
                textBoxMinThermX.Text = struThermRuleThermInfo.struLowestPoint.fX.ToString();
                textBoxMinThermY.Text = struThermRuleThermInfo.struLowestPoint.fY.ToString();
            }

        }
    }
}
