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
    public partial class FormThermometryIntelRule : Form
    {
        public int m_iChannel = -1;
        public int m_lUserID = -1;
        private CHCNetSDK.NET_DVR_STD_CONFIG m_struSTDConfig = new CHCNetSDK.NET_DVR_STD_CONFIG();
        private CHCNetSDK.NET_DVR_THERMAL_INTELRULE_DISPLAY m_struThermalIntelRuleDisplay = new CHCNetSDK.NET_DVR_THERMAL_INTELRULE_DISPLAY();


        public FormThermometryIntelRule()
        {
            InitializeComponent();

            m_struThermalIntelRuleDisplay.struNormalRulesLineCfg = new CHCNetSDK.NET_DVR_RULESLINE_CFG();
            m_struThermalIntelRuleDisplay.struAlarmRulesLineCfg = new CHCNetSDK.NET_DVR_RULESLINE_CFG();
            m_struThermalIntelRuleDisplay.struAlertRulesLineCfg = new CHCNetSDK.NET_DVR_RULESLINE_CFG();
            comboBoxFontSize.SelectedIndex = 0;
            comboBoxRuleLineColor.SelectedIndex = 0;
        }

       
        private void btnGetThermIntelRule_Click(object sender, EventArgs e)
        {
            int iCondSize = sizeof(int);
            m_struSTDConfig.dwCondSize = (uint)iCondSize;
            m_struSTDConfig.lpCondBuffer = Marshal.AllocHGlobal(iCondSize);
            Marshal.WriteInt32(m_struSTDConfig.lpCondBuffer, m_iChannel);

            int iOutSize = Marshal.SizeOf(m_struThermalIntelRuleDisplay);
            m_struThermalIntelRuleDisplay.dwSize = (uint)iOutSize;
            IntPtr ptrOutBuffer = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struThermalIntelRuleDisplay, ptrOutBuffer, false);
            m_struSTDConfig.dwOutSize = (uint)iOutSize;
            m_struSTDConfig.lpOutBuffer = ptrOutBuffer;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_GetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_THERMAL_INTELRULE_DISPLAY, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：获取热成像智能规则，错误码：" + iLastErr  ;
                MessageBox.Show(strErr);
            }
            else
            {
                m_struSTDConfig = (CHCNetSDK.NET_DVR_STD_CONFIG)Marshal.PtrToStructure(ptr, typeof(CHCNetSDK.NET_DVR_STD_CONFIG));
                m_struThermalIntelRuleDisplay = (CHCNetSDK.NET_DVR_THERMAL_INTELRULE_DISPLAY)Marshal.PtrToStructure(m_struSTDConfig.lpOutBuffer, typeof(CHCNetSDK.NET_DVR_THERMAL_INTELRULE_DISPLAY));
                comboBoxFontSize.SelectedIndex = m_struThermalIntelRuleDisplay.byFontSizeType;
            }

            comboBoxRuleLineColor_SelectedIndexChanged(sender,e);
            Marshal.FreeHGlobal(ptrOutBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        private void btnSaveLineRuleColor_Click(object sender, EventArgs e)
        {
            m_struThermalIntelRuleDisplay.byFontSizeType = (byte)comboBoxFontSize.SelectedIndex;

            if (0 == comboBoxRuleLineColor.SelectedIndex)
            {
                m_struThermalIntelRuleDisplay.struNormalRulesLineCfg.struRGB.byRed = byte.Parse(textBoxLineR.Text);
                m_struThermalIntelRuleDisplay.struNormalRulesLineCfg.struRGB.byGreen = byte.Parse(textBoxLineG.Text);
                m_struThermalIntelRuleDisplay.struNormalRulesLineCfg.struRGB.byBlue = byte.Parse(textBoxLineB.Text);
            }
            if (1 == comboBoxRuleLineColor.SelectedIndex)
            {
                m_struThermalIntelRuleDisplay.struAlertRulesLineCfg.struRGB.byRed = byte.Parse(textBoxLineR.Text);
                m_struThermalIntelRuleDisplay.struAlertRulesLineCfg.struRGB.byGreen = byte.Parse(textBoxLineG.Text);
                m_struThermalIntelRuleDisplay.struAlertRulesLineCfg.struRGB.byBlue = byte.Parse(textBoxLineB.Text);
            }
            if (2 == comboBoxRuleLineColor.SelectedIndex)
            {
                m_struThermalIntelRuleDisplay.struAlarmRulesLineCfg.struRGB.byRed = byte.Parse(textBoxLineR.Text);
                m_struThermalIntelRuleDisplay.struAlarmRulesLineCfg.struRGB.byGreen = byte.Parse(textBoxLineG.Text);
                m_struThermalIntelRuleDisplay.struAlarmRulesLineCfg.struRGB.byBlue = byte.Parse(textBoxLineB.Text);
            }
        }

        private void btnSetThermIntelRule_Click(object sender, EventArgs e)
        {
            btnSaveLineRuleColor.PerformClick();

            m_struSTDConfig.dwCondSize = sizeof(int);
            m_struSTDConfig.lpCondBuffer = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(m_struSTDConfig.lpCondBuffer, m_iChannel);

            int iInSize = Marshal.SizeOf(m_struThermalIntelRuleDisplay);
            m_struThermalIntelRuleDisplay.dwSize = (uint)iInSize;
            IntPtr ptrInBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(m_struThermalIntelRuleDisplay));
            Marshal.StructureToPtr(m_struThermalIntelRuleDisplay, ptrInBuffer, false);
            m_struSTDConfig.dwInSize = (uint)iInSize;
            m_struSTDConfig.lpInBuffer = ptrInBuffer;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, true);

            if (!CHCNetSDK.NET_DVR_SetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_THERMAL_INTELRULE_DISPLAY, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：设置热成像智能规则，错误码：" + iLastErr  ;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("设置热成像智能规则, 成功");
            }

            Marshal.FreeHGlobal(ptrInBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        private void comboBoxRuleLineColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (0 == comboBoxRuleLineColor.SelectedIndex)
            {
                textBoxLineR.Text = m_struThermalIntelRuleDisplay.struNormalRulesLineCfg.struRGB.byRed.ToString();
                textBoxLineG.Text = m_struThermalIntelRuleDisplay.struNormalRulesLineCfg.struRGB.byGreen.ToString();
                textBoxLineB.Text = m_struThermalIntelRuleDisplay.struNormalRulesLineCfg.struRGB.byBlue.ToString();
            }
            if (1 == comboBoxRuleLineColor.SelectedIndex)
            {
                textBoxLineR.Text = m_struThermalIntelRuleDisplay.struAlertRulesLineCfg.struRGB.byRed.ToString();
                textBoxLineG.Text = m_struThermalIntelRuleDisplay.struAlertRulesLineCfg.struRGB.byGreen.ToString();
                textBoxLineB.Text = m_struThermalIntelRuleDisplay.struAlertRulesLineCfg.struRGB.byBlue.ToString();
            }
            if (2 == comboBoxRuleLineColor.SelectedIndex)
            {
                textBoxLineR.Text = m_struThermalIntelRuleDisplay.struAlarmRulesLineCfg.struRGB.byRed.ToString();
                textBoxLineG.Text = m_struThermalIntelRuleDisplay.struAlarmRulesLineCfg.struRGB.byGreen.ToString();
                textBoxLineB.Text = m_struThermalIntelRuleDisplay.struAlarmRulesLineCfg.struRGB.byBlue.ToString();
            }
        }

         
    }
}
