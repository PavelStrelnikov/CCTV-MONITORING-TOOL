using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDKThermometry
{
    public partial class FormRulesParam : Form
    {
        public int m_iChannel = -1;
        public int m_lUserID = -1;
        private ConcurrentQueue<Point> m_points = new ConcurrentQueue<Point>();
        private CHCNetSDK.NET_DVR_THERMOMETRY_COND m_struThermometryCond = new CHCNetSDK.NET_DVR_THERMOMETRY_COND();
        private CHCNetSDK.NET_DVR_THERMOMETRY_PRESETINFO m_struThermometryPrestInfo = new CHCNetSDK.NET_DVR_THERMOMETRY_PRESETINFO();
        private CHCNetSDK.NET_DVR_STD_CONFIG m_struSTDConfig = new CHCNetSDK.NET_DVR_STD_CONFIG();
        private CHCNetSDK.NET_DVR_THERMOMETRY_ALARMRULE m_struThermAlarmRule = new CHCNetSDK.NET_DVR_THERMOMETRY_ALARMRULE();
        private CHCNetSDK.NET_DVR_THERMOMETRY_DIFFCOMPARISON m_struThermDiffComparsion = new CHCNetSDK.NET_DVR_THERMOMETRY_DIFFCOMPARISON();
        private CHCNetSDK.NET_DVR_PREVIEWINFO m_struPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
        private int m_lRealHandle = -1;
        private int m_iPointNum = 0;
        private CHCNetSDK.DRAWFUN fDrawFun = null;
        private CHCNetSDK.NET_VCA_POINT m_struPoint = new CHCNetSDK.NET_VCA_POINT();
        private CHCNetSDK.NET_VCA_POLYGON m_struPolygon = new CHCNetSDK.NET_VCA_POLYGON();


        public FormRulesParam()
        {
            InitializeComponent();

            m_struThermometryPrestInfo.struPresetInfo = new CHCNetSDK.NET_DVR_THERMOMETRY_PRESETINFO_PARAM[CHCNetSDK.THERMOMETRY_ALARMRULE_NUM];
            m_struThermAlarmRule.struThermometryAlarmRuleParam = new CHCNetSDK.NET_DVR_THERMOMETRY_ALARMRULE_PARAM[CHCNetSDK.MAX_THERMOMETRY_REGION_NUM];
            m_struThermDiffComparsion.struDiffComparison = new CHCNetSDK.NET_DVR_THERMOMETRY_DIFFCOMPARISON_PARAM[CHCNetSDK.THERMOMETRY_ALARMRULE_NUM];
            m_struPolygon.struPos = new CHCNetSDK.NET_VCA_POINT[CHCNetSDK.VCA_MAX_POLYGON_POINT_NUM];

            this.InitUI();
        }

        private void InitUI()
        {
            for (int i = 0; i < 300; i++)
            {
                string sComboText = (i + 1).ToString();
                comboBoxPTZ.Items.Add(sComboText);
            }
            comboBoxPTZ.SelectedIndex  = 0;

            for (int i = 0; i < 40; i++)
            {
                string sComboText = "Rule " + (i + 1).ToString();
                comboBoxRules.Items.Add(sComboText);
            }
            comboBoxRules.SelectedIndex = 0;
            comboBoxRuleCalibType.SelectedIndex = 0;

            for (int i = 0; i < 40; i++)
            {
                string sComboText = "Rule " + (i + 1).ToString();
                comboBoxAlarmRules.Items.Add(sComboText);
            }
            comboBoxAlarmRules.SelectedIndex = 0;
            comboBoxAlarm.SelectedIndex = 0;

            for (int i = 0; i < 40; i++)
            {
                string sComboText = "Rule " + (i + 1).ToString();
                comboBoxDiffAlarmRules.Items.Add(sComboText);
            }
            comboBoxDiffAlarmRules.SelectedIndex = 0;
            comboBoxDiffAlarm.SelectedIndex = 0;
        }

        //预置点关联信息规则配置
        private void btnRuleSave_Click(object sender, EventArgs e)
        {
            int iRule = comboBoxRules.SelectedIndex;

            m_struThermometryPrestInfo.wPresetNo = (ushort)(comboBoxPTZ.SelectedIndex + 1);

            m_struThermometryPrestInfo.struPresetInfo[iRule].byEnabled = Convert.ToByte(chkRuleEnable.Checked);
            m_struThermometryPrestInfo.struPresetInfo[iRule].byRuleID = Convert.ToByte(textBoxRuleID.Text);
            byte[] byName = System.Text.Encoding.Default.GetBytes(textBoxRuleName.Text);
            m_struThermometryPrestInfo.struPresetInfo[iRule].szRuleName = new byte[32];
            byName.CopyTo(m_struThermometryPrestInfo.struPresetInfo[iRule].szRuleName, 0);
            m_struThermometryPrestInfo.struPresetInfo[iRule].wDistance = Convert.ToUInt16(textBoxDistance.Text);
            m_struThermometryPrestInfo.struPresetInfo[iRule].fEmissivity = Convert.ToSingle(textBoxEmissivity.Text);
            m_struThermometryPrestInfo.struPresetInfo[iRule].byRuleCalibType = (byte)comboBoxRuleCalibType.SelectedIndex;
            m_struThermometryPrestInfo.struPresetInfo[iRule].byReflectiveEnabled = Convert.ToByte(chkReflectiveTemp.Checked);
            m_struThermometryPrestInfo.struPresetInfo[iRule].fReflectiveTemperature = Convert.ToSingle(textboxReflectiveTemp.Text);
            m_struThermometryPrestInfo.struPresetInfo[iRule].struPoint = m_struPoint;
            m_struThermometryPrestInfo.struPresetInfo[iRule].struRegion = m_struPolygon;
        }

        private void btn_PresentInfoGet_Click(object sender, EventArgs e)
        {
            int iCondSize = Marshal.SizeOf(m_struThermometryCond);
            m_struThermometryCond.dwSize = (uint)iCondSize;
            m_struThermometryCond.dwChannel = (uint)m_iChannel;
            m_struThermometryCond.wPresetNo = (ushort)(comboBoxPTZ.SelectedIndex + 1);
            IntPtr ptrCondBuffer = Marshal.AllocHGlobal(iCondSize);
            Marshal.StructureToPtr(m_struThermometryCond, ptrCondBuffer, false);
            m_struSTDConfig.dwCondSize = (uint)iCondSize;
            m_struSTDConfig.lpCondBuffer = ptrCondBuffer;

            int iOutSize = Marshal.SizeOf(m_struThermometryPrestInfo);
            m_struThermometryPrestInfo.dwSize = (uint)iOutSize;
            m_struThermometryPrestInfo.wPresetNo = (ushort)(comboBoxPTZ.SelectedIndex + 1);
            IntPtr ptrOutBuffer = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struThermometryPrestInfo, ptrOutBuffer, false);
            m_struSTDConfig.dwOutSize = (uint)iOutSize;
            m_struSTDConfig.lpOutBuffer = ptrOutBuffer;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_GetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_THERMOMETRY_PRESETINFO, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：获取预置点关联信息配置，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                int iRule = comboBoxRules.SelectedIndex;
                m_struSTDConfig = (CHCNetSDK.NET_DVR_STD_CONFIG)Marshal.PtrToStructure(ptr, typeof(CHCNetSDK.NET_DVR_STD_CONFIG));
                m_struThermometryPrestInfo = (CHCNetSDK.NET_DVR_THERMOMETRY_PRESETINFO)Marshal.PtrToStructure(ptrOutBuffer, typeof(CHCNetSDK.NET_DVR_THERMOMETRY_PRESETINFO));

                chkRuleEnable.Checked = Convert.ToBoolean(m_struThermometryPrestInfo.struPresetInfo[iRule].byEnabled);
                textBoxRuleID.Text = Convert.ToString(m_struThermometryPrestInfo.struPresetInfo[iRule].byRuleID);
                textBoxRuleName.Text = System.Text.Encoding.UTF8.GetString(m_struThermometryPrestInfo.struPresetInfo[iRule].szRuleName);
                textBoxDistance.Text = Convert.ToString(m_struThermometryPrestInfo.struPresetInfo[iRule].wDistance);
                textBoxEmissivity.Text = Convert.ToString(m_struThermometryPrestInfo.struPresetInfo[iRule].fEmissivity);
                comboBoxRuleCalibType.SelectedIndex = m_struThermometryPrestInfo.struPresetInfo[iRule].byRuleCalibType;
                chkReflectiveTemp.Checked = Convert.ToBoolean(m_struThermometryPrestInfo.struPresetInfo[iRule].byReflectiveEnabled);
                textboxReflectiveTemp.Text = Convert.ToString(m_struThermometryPrestInfo.struPresetInfo[iRule].fReflectiveTemperature);
            }

            Marshal.FreeHGlobal(ptrCondBuffer);
            Marshal.FreeHGlobal(ptrOutBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        private void btn_PresentInfoSet_Click(object sender, EventArgs e)
        {
            int iCondSize = Marshal.SizeOf(m_struThermometryCond);
            m_struThermometryCond.dwSize = (uint)iCondSize;
            m_struThermometryCond.dwChannel = (uint)m_iChannel;
            m_struThermometryCond.wPresetNo = (ushort)(comboBoxPTZ.SelectedIndex + 1);
            IntPtr ptrCondBuffer = Marshal.AllocHGlobal(iCondSize);
            Marshal.StructureToPtr(m_struThermometryCond, ptrCondBuffer, false);
            m_struSTDConfig.dwCondSize = (uint)iCondSize;
            m_struSTDConfig.lpCondBuffer = ptrCondBuffer;

            btnRuleSave.PerformClick();

            int iInSize = Marshal.SizeOf(m_struThermometryPrestInfo);
            m_struThermometryPrestInfo.dwSize = (uint)iInSize;
            m_struThermometryPrestInfo.wPresetNo = (ushort)(comboBoxPTZ.SelectedIndex + 1);
            IntPtr ptrInBuffer = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(m_struThermometryPrestInfo, ptrInBuffer, false);
            m_struSTDConfig.dwInSize = (uint)iInSize;
            m_struSTDConfig.lpInBuffer = ptrInBuffer;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_SetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_THERMOMETRY_PRESETINFO, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：设置预置点关联信息配置，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("设置预置点关联信息，成功！");
            }

            Marshal.FreeHGlobal(ptrCondBuffer);
            Marshal.FreeHGlobal(ptrInBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        // 测温报警
        private void btnAlarmRuleSave_Click(object sender, EventArgs e)
        {
            int iRule = comboBoxAlarmRules.SelectedIndex;

            m_struThermAlarmRule.struThermometryAlarmRuleParam[iRule].byEnable = Convert.ToByte(chkAlarmRule.Checked);
            m_struThermAlarmRule.struThermometryAlarmRuleParam[iRule].byRuleID = Convert.ToByte(textBoxAlarmRuleID.Text);
            byte[] byName = System.Text.Encoding.Default.GetBytes(textBoxAlarmRuleName.Text);
            m_struThermAlarmRule.struThermometryAlarmRuleParam[iRule].szRuleName = new byte[32];
            byName.CopyTo(m_struThermAlarmRule.struThermometryAlarmRuleParam[iRule].szRuleName, 0);
            m_struThermAlarmRule.struThermometryAlarmRuleParam[iRule].byRule = (byte)comboBoxAlarm.SelectedIndex;
            m_struThermAlarmRule.struThermometryAlarmRuleParam[iRule].fAlert = Convert.ToSingle(textBoxAlert.Text);
            m_struThermAlarmRule.struThermometryAlarmRuleParam[iRule].fAlarm = Convert.ToSingle(textBoxAlarm.Text);
            m_struThermAlarmRule.struThermometryAlarmRuleParam[iRule].fThreshold = Convert.ToSingle(textBoxThreshold.Text);
        }

        private void btnAlarmRuleGet_Click(object sender, EventArgs e)
        {
            int iCondSize = Marshal.SizeOf(m_struThermometryCond);

            m_struThermometryCond.dwSize = (uint)iCondSize;
            m_struThermometryCond.dwChannel = (uint)m_iChannel;
            m_struThermometryCond.wPresetNo = (ushort)(comboBoxPTZ.SelectedIndex + 1);
            IntPtr ptrCondBuffer = Marshal.AllocHGlobal(iCondSize);
            Marshal.StructureToPtr(m_struThermometryCond, ptrCondBuffer, false);
            m_struSTDConfig.dwCondSize = (uint)iCondSize;
            m_struSTDConfig.lpCondBuffer = ptrCondBuffer;

            int iOutSize = Marshal.SizeOf(m_struThermAlarmRule);
            m_struThermAlarmRule.dwSize = (uint)iOutSize;
            IntPtr ptrOutBuffer = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struThermAlarmRule, ptrOutBuffer, false);
            m_struSTDConfig.dwOutSize = (uint)iOutSize;
            m_struSTDConfig.lpOutBuffer = ptrOutBuffer;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_GetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_THERMOMETRY_ALARMRULE, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：获取预置点报警信息配置，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                int iRule = comboBoxAlarmRules.SelectedIndex;
                m_struSTDConfig = (CHCNetSDK.NET_DVR_STD_CONFIG)Marshal.PtrToStructure(ptr, typeof(CHCNetSDK.NET_DVR_STD_CONFIG));
                m_struThermAlarmRule = (CHCNetSDK.NET_DVR_THERMOMETRY_ALARMRULE)Marshal.PtrToStructure(ptrOutBuffer, typeof(CHCNetSDK.NET_DVR_THERMOMETRY_ALARMRULE));

                chkAlarmRule.Checked = Convert.ToBoolean(m_struThermAlarmRule.struThermometryAlarmRuleParam[iRule].byEnable);
                textBoxAlarmRuleID.Text = Convert.ToString(m_struThermAlarmRule.struThermometryAlarmRuleParam[iRule].byRuleID);
                textBoxAlarmRuleName.Text = System.Text.Encoding.UTF8.GetString(m_struThermAlarmRule.struThermometryAlarmRuleParam[iRule].szRuleName);
                comboBoxAlarm.SelectedIndex = m_struThermAlarmRule.struThermometryAlarmRuleParam[iRule].byRule;
                textBoxAlert.Text = Convert.ToString(m_struThermAlarmRule.struThermometryAlarmRuleParam[iRule].fAlert);
                textBoxAlarm.Text = Convert.ToString(m_struThermAlarmRule.struThermometryAlarmRuleParam[iRule].fAlarm);
                textBoxThreshold.Text = Convert.ToString(m_struThermAlarmRule.struThermometryAlarmRuleParam[iRule].fThreshold);
            }

            Marshal.FreeHGlobal(ptrCondBuffer);
            Marshal.FreeHGlobal(ptrOutBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        private void btnAlarmRuleSet_Click(object sender, EventArgs e)
        {
            int iCondSize = Marshal.SizeOf(m_struThermometryCond);

            m_struThermometryCond.dwSize = (uint)iCondSize;
            m_struThermometryCond.dwChannel = (uint)m_iChannel;
            m_struThermometryCond.wPresetNo = (ushort)(comboBoxPTZ.SelectedIndex + 1);
            IntPtr ptrCondBuffer = Marshal.AllocHGlobal(iCondSize);
            Marshal.StructureToPtr(m_struThermometryCond, ptrCondBuffer, false);
            m_struSTDConfig.dwCondSize = (uint)iCondSize;
            m_struSTDConfig.lpCondBuffer = ptrCondBuffer;

            btnAlarmRuleSave.PerformClick();

            int iInSize = Marshal.SizeOf(m_struThermAlarmRule);
            m_struThermAlarmRule.dwSize = (uint)iInSize;
            IntPtr ptrInBuffer = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(m_struThermAlarmRule, ptrInBuffer, false);
            m_struSTDConfig.dwInSize = (uint)iInSize;
            m_struSTDConfig.lpInBuffer = ptrInBuffer;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_SetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_THERMOMETRY_ALARMRULE, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：设置预置点报警规则，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("设置预置点报警规则，成功！");
            }

            Marshal.FreeHGlobal(ptrCondBuffer);
            Marshal.FreeHGlobal(ptrInBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        // 温差报警
        private void btnDiffAlarmRuleSave_Click(object sender, EventArgs e)
        {
            int iRule = comboBoxAlarmRules.SelectedIndex;

            m_struThermDiffComparsion.struDiffComparison[iRule].byEnabled = Convert.ToByte(chkDiffAlarmRule.Checked);
            m_struThermDiffComparsion.struDiffComparison[iRule].byRuleID = Convert.ToByte(textBoxDiffAlarmRuleID.Text);
            m_struThermDiffComparsion.struDiffComparison[iRule].byAlarmID1 = Convert.ToByte(textBoxAlarmRuleID1.Text);
            m_struThermDiffComparsion.struDiffComparison[iRule].byAlarmID2 = Convert.ToByte(textBoxAlarmRuleID2.Text);
            m_struThermDiffComparsion.struDiffComparison[iRule].byRule = (byte)comboBoxDiffAlarm.SelectedIndex;
            m_struThermDiffComparsion.struDiffComparison[iRule].fTemperatureDiff = Convert.ToSingle(textBoxTemperatureDiff.Text);

        }

        private void btnDiffAlarmRuleGet_Click(object sender, EventArgs e)
        {
            int iCondSize = Marshal.SizeOf(m_struThermometryCond);

            m_struThermometryCond.dwSize = (uint)iCondSize;
            m_struThermometryCond.dwChannel = (uint)m_iChannel;
            m_struThermometryCond.wPresetNo = (ushort)(comboBoxPTZ.SelectedIndex + 1);
            IntPtr ptrCondBuffer = Marshal.AllocHGlobal(iCondSize);
            Marshal.StructureToPtr(m_struThermometryCond, ptrCondBuffer, false);
            m_struSTDConfig.dwCondSize = (uint)iCondSize;
            m_struSTDConfig.lpCondBuffer = ptrCondBuffer;

            int iOutSize = Marshal.SizeOf(m_struThermDiffComparsion);
            m_struThermDiffComparsion.dwSize = (uint)iOutSize;
            IntPtr ptrOutBuffer = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struThermDiffComparsion, ptrOutBuffer, false);
            m_struSTDConfig.dwOutSize = (uint)iOutSize;
            m_struSTDConfig.lpOutBuffer = ptrOutBuffer;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_GetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_THERMOMETRY_DIFFCOMPARISON, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：获取温差报警信息配置，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                int iRule = comboBoxDiffAlarmRules.SelectedIndex;
                m_struSTDConfig = (CHCNetSDK.NET_DVR_STD_CONFIG)Marshal.PtrToStructure(ptr, typeof(CHCNetSDK.NET_DVR_STD_CONFIG));
                m_struThermDiffComparsion = (CHCNetSDK.NET_DVR_THERMOMETRY_DIFFCOMPARISON)Marshal.PtrToStructure(ptrOutBuffer, typeof(CHCNetSDK.NET_DVR_THERMOMETRY_DIFFCOMPARISON));

                chkDiffAlarmRule.Checked = Convert.ToBoolean(m_struThermDiffComparsion.struDiffComparison[iRule].byEnabled);
                textBoxDiffAlarmRuleID.Text = Convert.ToString(m_struThermDiffComparsion.struDiffComparison[iRule].byRuleID);
                textBoxAlarmRuleID1.Text = Convert.ToString(m_struThermDiffComparsion.struDiffComparison[iRule].byAlarmID1);
                textBoxAlarmRuleID2.Text = Convert.ToString(m_struThermDiffComparsion.struDiffComparison[iRule].byAlarmID1);
                comboBoxDiffAlarm.SelectedIndex = m_struThermDiffComparsion.struDiffComparison[iRule].byRule;
                textBoxTemperatureDiff.Text = Convert.ToString(m_struThermDiffComparsion.struDiffComparison[iRule].fTemperatureDiff);

            }

            Marshal.FreeHGlobal(ptrCondBuffer);
            Marshal.FreeHGlobal(ptrOutBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        private void btnDiffAlarmRuleSet_Click(object sender, EventArgs e)
        {
            int iCondSize = Marshal.SizeOf(m_struThermometryCond);

            m_struThermometryCond.dwSize = (uint)iCondSize;
            m_struThermometryCond.dwChannel = (uint)m_iChannel;
            m_struThermometryCond.wPresetNo = (ushort)(comboBoxPTZ.SelectedIndex + 1);
            IntPtr ptrCondBuffer = Marshal.AllocHGlobal(iCondSize);
            Marshal.StructureToPtr(m_struThermometryCond, ptrCondBuffer, false);
            m_struSTDConfig.dwCondSize = (uint)iCondSize;
            m_struSTDConfig.lpCondBuffer = ptrCondBuffer;

            btnDiffAlarmRuleSave.PerformClick();

            int iInSize = Marshal.SizeOf(m_struThermDiffComparsion);
            m_struThermDiffComparsion.dwSize = (uint)iInSize;
            IntPtr ptrInBuffer = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(m_struThermDiffComparsion, ptrInBuffer, false);
            m_struSTDConfig.dwInSize = (uint)iInSize;
            m_struSTDConfig.lpInBuffer = ptrInBuffer;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_SetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_THERMOMETRY_DIFFCOMPARISON, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：设置温差报警规则，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("设置温差报警规则，成功！");
            }

            Marshal.FreeHGlobal(ptrCondBuffer);
            Marshal.FreeHGlobal(ptrInBuffer);
            Marshal.FreeHGlobal(ptr);
        }



        // 预置点配置
        private void btnSetPreset_Click(object sender, EventArgs e)
        {
            if (!CHCNetSDK.NET_DVR_PTZPreset_Other(m_lUserID, m_iChannel, (uint)CHCNetSDK.SET_PRESET, (uint)(comboBoxPTZ.SelectedIndex + 1)))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：设置预置点，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
        }

        private void btnClePreset_Click(object sender, EventArgs e)
        {
            if (!CHCNetSDK.NET_DVR_PTZPreset_Other(m_lUserID, m_iChannel, (uint)CHCNetSDK.CLE_PRESET, (uint)(comboBoxPTZ.SelectedIndex + 1)))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：清除预置点，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
        }

        private void btnGotoPreset_Click(object sender, EventArgs e)
        {
            if (!CHCNetSDK.NET_DVR_PTZPreset_Other(m_lUserID, m_iChannel, (uint)CHCNetSDK.GOTO_PRESET, (uint)(comboBoxPTZ.SelectedIndex + 1)))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：设置预置点配置，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
        }

        private void btnEventParam_Click(object sender, EventArgs e)
        {
            FormEventParam dlg = new FormEventParam();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.m_lUserID = m_lUserID;
            dlg.m_iChannel = m_iChannel;
            dlg.m_iPresetNo = comboBoxPTZ.SelectedIndex + 1;
            dlg.ShowDialog();
        }
        // 画规则区域
        private void pictureBoxPlay_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point point = pictureBoxPlay.PointToClient(Control.MousePosition);
                float fPointX = Convert.ToSingle(((float)point.X / pictureBoxPlay.Width).ToString("#0.000"));
                float fPointY = Convert.ToSingle(((float)point.Y / pictureBoxPlay.Height).ToString("#0.000"));
                if (0 == comboBoxRuleCalibType.SelectedIndex)
                {
                    if (m_points.Count >= 1)
                    {
                        m_points = new ConcurrentQueue<Point>();
                    }

                    m_struPoint.fX = fPointX;
                    m_struPoint.fY = fPointY;

                    m_points.Enqueue(point);

                    if (m_points.Count == 1 && fDrawFun == null)
                    {
                        fDrawFun = new CHCNetSDK.DRAWFUN(cbDrawFun);
                        if (!CHCNetSDK.NET_DVR_RigisterDrawFun(m_lRealHandle, fDrawFun, 0))
                        {
                            int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                            string strErr = "Thermal：预览画面叠加配置，错误码：" + iLastErr;
                            MessageBox.Show(strErr);
                        }
                    }
                }

                if (2 == comboBoxRuleCalibType.SelectedIndex)  // 画线
                {
                    if (m_iPointNum >= 2)
                    {
                        m_iPointNum = 0;
                        m_points = new ConcurrentQueue<Point>();
                    }

                    if (m_points.Count >= 0 && m_points.Count < 2)
                    {
                        m_struPolygon.struPos[m_iPointNum].fX = fPointX;
                        m_struPolygon.struPos[m_iPointNum].fY = fPointY;
                        ++m_iPointNum;
                        m_struPolygon.dwPointNum = (uint)m_iPointNum;
                        m_points.Enqueue(point);

                        if (m_points.Count == 2 && fDrawFun == null)
                        {
                            fDrawFun = new CHCNetSDK.DRAWFUN(cbDrawFun);
                            if (!CHCNetSDK.NET_DVR_RigisterDrawFun(m_lRealHandle, fDrawFun, 0))
                            {
                                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                                string strErr = "Thermal：预览画面叠加配置，错误码：" + iLastErr;
                                MessageBox.Show(strErr);
                            }
                        }
                    }

                }

                if (1 == comboBoxRuleCalibType.SelectedIndex)  // 画框
                {
                    if (m_iPointNum >= CHCNetSDK.VCA_MAX_POLYGON_POINT_NUM)
                    {
                        m_struPolygon.struPos = new CHCNetSDK.NET_VCA_POINT[CHCNetSDK.VCA_MAX_POLYGON_POINT_NUM];
                        m_iPointNum = 0;
                        m_points = new ConcurrentQueue<Point>();
                    }

                    if (m_points.Count < CHCNetSDK.VCA_MAX_POLYGON_POINT_NUM)
                    {
                        m_struPolygon.struPos[m_iPointNum].fX = fPointX;
                        m_struPolygon.struPos[m_iPointNum].fY = fPointY;
                        ++m_iPointNum;
                        m_struPolygon.dwPointNum = (uint)m_iPointNum;
                        m_points.Enqueue(point);

                        if (fDrawFun == null)
                        {
                            fDrawFun = new CHCNetSDK.DRAWFUN(cbDrawFun);
                            if (!CHCNetSDK.NET_DVR_RigisterDrawFun(m_lRealHandle, fDrawFun, 0))
                            {
                                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                                string strErr = "Thermal：预览画面叠加配置，错误码：" + iLastErr;
                                MessageBox.Show(strErr);
                            }
                        }
                    }
                }
            }
           
        }

        // 回调函数叠加图像字符
        private void cbDrawFun(int port, IntPtr hDc, uint nUser)
        {
            Graphics g = Graphics.FromHdc(hDc);
            Pen pen = new Pen(Color.Red, 2);
            Brush brush = new SolidBrush(Color.FromArgb(50, 255, 255, 255));
            if (g == null)
            {
                return;
            }
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

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

        private void FormRulesParam_Load(object sender, EventArgs e)
        {
            m_struPreviewInfo.lChannel = m_iChannel;
            m_struPreviewInfo.dwStreamType = 0;
            m_struPreviewInfo.bBlocked = true;
            m_struPreviewInfo.DisplayBufNum = 0;
            m_struPreviewInfo.hPlayWnd = pictureBoxPlay.Handle;

            m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref m_struPreviewInfo, null, IntPtr.Zero);
        }

        private void comboBoxRuleCalibType_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_points = new ConcurrentQueue<Point>();
        }

    }
}
