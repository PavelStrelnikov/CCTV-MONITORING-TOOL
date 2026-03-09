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
    public partial class FormEventParam : Form
    {
        public int m_iChannel = -1;
        public int m_lUserID = -1;
        public int m_iPresetNo = -1;
        private CHCNetSDK.NET_DVR_THERMOMETRY_COND m_struThermometryCond = new CHCNetSDK.NET_DVR_THERMOMETRY_COND();
        private CHCNetSDK.NET_DVR_STD_CONFIG m_struSTDConfig = new CHCNetSDK.NET_DVR_STD_CONFIG();
        private CHCNetSDK.NET_DVR_EVENT_SCHEDULE m_struEventSchedule = new CHCNetSDK.NET_DVR_EVENT_SCHEDULE();
        private CHCNetSDK.NET_DVR_THERMOMETRY_TRIGGER_COND m_struThermTriggerCond = new CHCNetSDK.NET_DVR_THERMOMETRY_TRIGGER_COND();
        private CHCNetSDK.NET_DVR_EVENT_TRIGGER m_struEventTrigger = new CHCNetSDK.NET_DVR_EVENT_TRIGGER();

        public FormEventParam()
        {
            InitializeComponent();

            m_struEventSchedule.struAlarmTime = new CHCNetSDK.NET_DVR_SCHEDTIME[CHCNetSDK.MAX_DAYS * CHCNetSDK.MAX_TIMESEGMENT_V30];
            m_struEventSchedule.struHolidayAlarmTime = new CHCNetSDK.NET_DVR_SCHEDTIME[CHCNetSDK.MAX_TIMESEGMENT_V30];
            comboBoxWeekday.SelectedIndex = 0;
            comboBoxWeekdayCopy.SelectedIndex = 0;
        }

        private void btnThermScheduleCopy_Click(object sender, EventArgs e)
        {
            SetEventSchedule();

            int iDayIndex = comboBoxWeekday.SelectedIndex;
            int iCopyDayIndex = comboBoxWeekdayCopy.SelectedIndex - 1;
            if (iCopyDayIndex == -1)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (iDayIndex != j)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            m_struEventSchedule.struAlarmTime[j * CHCNetSDK.MAX_TIMESEGMENT_V30 + i] = m_struEventSchedule.struAlarmTime[iDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + i];
                        }
                    }
                }
            }
            else
            {
                if (iDayIndex != iCopyDayIndex)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        m_struEventSchedule.struAlarmTime[iCopyDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + i] = m_struEventSchedule.struAlarmTime[iDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + i];
                    }
                }
            }

            comboBoxWeekday_SelectedIndexChanged(sender, e);
        }

        private void btnThermScheduleGet_Click(object sender, EventArgs e)
        {
            int iCondSize = sizeof(int);
            m_struSTDConfig.lpCondBuffer = Marshal.AllocHGlobal(iCondSize);
            Marshal.WriteInt32(m_struSTDConfig.lpCondBuffer, m_iChannel);
            m_struSTDConfig.dwCondSize = (uint)iCondSize;

            int iOutSize = Marshal.SizeOf(m_struEventSchedule);
            m_struEventSchedule.dwSize = (uint)iOutSize;
            IntPtr ptrOutBuffer = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struEventSchedule, ptrOutBuffer, false);
            m_struSTDConfig.dwOutSize = (uint)iOutSize;
            m_struSTDConfig.lpOutBuffer = ptrOutBuffer;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_GetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_THERMOMETRY_SCHEDULE, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：获取测温布防时间配置，错误码：" + iLastErr ;
                MessageBox.Show(strErr);
            }
            else
            {
                m_struSTDConfig = (CHCNetSDK.NET_DVR_STD_CONFIG)Marshal.PtrToStructure(ptr, typeof(CHCNetSDK.NET_DVR_STD_CONFIG));
                m_struEventSchedule = (CHCNetSDK.NET_DVR_EVENT_SCHEDULE)Marshal.PtrToStructure(m_struSTDConfig.lpOutBuffer, typeof(CHCNetSDK.NET_DVR_EVENT_SCHEDULE));

                comboBoxWeekday_SelectedIndexChanged(sender, e);
            }

            Marshal.FreeHGlobal(ptrOutBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        private void comboBoxWeekday_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nDayIndex = comboBoxWeekday.SelectedIndex;
            textBoxHour11.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 0].byStartHour.ToString();
            textBoxMin11.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 0].byStartMin.ToString();
            textBoxHour12.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 0].byStopHour.ToString();
            textBoxMin12.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 0].byStopMin.ToString();

            textBoxHour21.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 1].byStartHour.ToString();
            textBoxMin21.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 1].byStartMin.ToString();
            textBoxHour22.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 1].byStopHour.ToString();
            textBoxMin22.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 1].byStopMin.ToString();

            textBoxHour31.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 2].byStartHour.ToString();
            textBoxMin31.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 2].byStartMin.ToString();
            textBoxHour32.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 2].byStopHour.ToString();
            textBoxMin32.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 2].byStopMin.ToString();

            textBoxHour41.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 3].byStartHour.ToString();
            textBoxMin41.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 3].byStartMin.ToString();
            textBoxHour42.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 3].byStopHour.ToString();
            textBoxMin42.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 3].byStopMin.ToString();

            textBoxHour51.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 4].byStartHour.ToString();
            textBoxMin51.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 4].byStartMin.ToString();
            textBoxHour52.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 4].byStopHour.ToString();
            textBoxMin52.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 4].byStopMin.ToString();

            textBoxHour61.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 5].byStartHour.ToString();
            textBoxMin61.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 5].byStartMin.ToString();
            textBoxHour62.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 5].byStopHour.ToString();
            textBoxMin62.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 5].byStopMin.ToString();

            textBoxHour71.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 6].byStartHour.ToString();
            textBoxMin71.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 6].byStartMin.ToString();
            textBoxHour72.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 6].byStopHour.ToString();
            textBoxMin72.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 6].byStopMin.ToString();

            textBoxHour81.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 7].byStartHour.ToString();
            textBoxMin81.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 7].byStartMin.ToString();
            textBoxHour82.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 7].byStopHour.ToString();
            textBoxMin82.Text = m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 7].byStopMin.ToString();

        }

        private void btnThermScheduleSet_Click(object sender, EventArgs e)
        {
            
            SetEventSchedule();

            int iCondSize = sizeof(int);
            m_struSTDConfig.lpCondBuffer = Marshal.AllocHGlobal(iCondSize);
            Marshal.WriteInt32(m_struSTDConfig.lpCondBuffer, m_iChannel);
            m_struSTDConfig.dwCondSize = (uint)iCondSize;

            int iIntSize = Marshal.SizeOf(m_struEventSchedule);
            m_struEventSchedule.dwSize = (uint)iIntSize;
            IntPtr ptrInBuffer = Marshal.AllocHGlobal(iIntSize);
            Marshal.StructureToPtr(m_struEventSchedule, ptrInBuffer, false);
            m_struSTDConfig.dwInSize = (uint)iIntSize;
            m_struSTDConfig.lpInBuffer = ptrInBuffer;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_SetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_THERMOMETRY_SCHEDULE, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：设置测温布防时间，错误码：" + iLastErr ;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("设置测温布防时间配置，成功！");
            }

            Marshal.FreeHGlobal(ptrInBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        private void SetEventSchedule()
        {
            //设置布防时间
            int nDayIndex = comboBoxWeekday.SelectedIndex;
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 0].byStartHour = byte.Parse(textBoxHour11.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 0].byStartMin = byte.Parse(textBoxMin11.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 0].byStopHour = byte.Parse(textBoxHour12.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 0].byStopMin = byte.Parse(textBoxMin12.Text);

            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 1].byStartHour = byte.Parse(textBoxHour21.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 1].byStartMin = byte.Parse(textBoxMin21.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 1].byStopHour = byte.Parse(textBoxHour22.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 1].byStopMin = byte.Parse(textBoxMin22.Text);

            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 2].byStartHour = byte.Parse(textBoxHour31.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 2].byStartMin = byte.Parse(textBoxMin31.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 2].byStopHour = byte.Parse(textBoxHour32.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 2].byStopMin = byte.Parse(textBoxMin32.Text);

            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 3].byStartHour = byte.Parse(textBoxHour41.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 3].byStartMin = byte.Parse(textBoxMin41.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 3].byStopHour = byte.Parse(textBoxHour42.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 3].byStopMin = byte.Parse(textBoxMin42.Text);

            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 4].byStartHour = byte.Parse(textBoxHour51.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 4].byStartMin = byte.Parse(textBoxMin51.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 4].byStopHour = byte.Parse(textBoxHour52.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 4].byStopMin = byte.Parse(textBoxMin52.Text);

            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 5].byStartHour = byte.Parse(textBoxHour61.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 5].byStartMin = byte.Parse(textBoxMin61.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 5].byStopHour = byte.Parse(textBoxHour62.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 5].byStopMin = byte.Parse(textBoxMin62.Text);

            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 6].byStartHour = byte.Parse(textBoxHour71.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 6].byStartMin = byte.Parse(textBoxMin71.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 6].byStopHour = byte.Parse(textBoxHour72.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 6].byStopMin = byte.Parse(textBoxMin72.Text);

            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 7].byStartHour = byte.Parse(textBoxHour81.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 7].byStartMin = byte.Parse(textBoxMin81.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 7].byStopHour = byte.Parse(textBoxHour82.Text);
            m_struEventSchedule.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 7].byStopMin = byte.Parse(textBoxMin82.Text);

        }

        private void btnThermTriggerGet_Click(object sender, EventArgs e)
        {
            int iCondSize = Marshal.SizeOf(m_struThermTriggerCond);
            m_struThermTriggerCond.dwSize = (uint)iCondSize;
            m_struThermTriggerCond.dwChannel = (uint)m_iChannel;
            m_struThermTriggerCond.wPresetNo = (ushort)m_iPresetNo;
            m_struSTDConfig.lpCondBuffer = Marshal.AllocHGlobal(iCondSize);
            IntPtr ptrCondBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(m_struThermTriggerCond));
            Marshal.StructureToPtr(m_struThermometryCond, ptrCondBuffer, false);
            m_struSTDConfig.dwCondSize = (uint)iCondSize;
            m_struSTDConfig.lpCondBuffer = ptrCondBuffer;

            int OutSize = Marshal.SizeOf(m_struEventTrigger);;
            m_struEventTrigger.dwSize = (uint)OutSize;
            IntPtr ptrOutBuffer = Marshal.AllocHGlobal(OutSize);
            Marshal.StructureToPtr(m_struEventTrigger, ptrOutBuffer, false);
            m_struSTDConfig.dwOutSize = (uint)OutSize;
            m_struSTDConfig.lpOutBuffer = ptrOutBuffer;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_GetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_THERMOMETRY_TRIGGER, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：获取测温联动配置，错误码：" + iLastErr ;
                MessageBox.Show(strErr);
            }
            else
            {
                m_struSTDConfig = (CHCNetSDK.NET_DVR_STD_CONFIG)Marshal.PtrToStructure(ptr, typeof(CHCNetSDK.NET_DVR_STD_CONFIG));
                m_struEventTrigger = (CHCNetSDK.NET_DVR_EVENT_TRIGGER)Marshal.PtrToStructure(ptrOutBuffer, typeof(CHCNetSDK.NET_DVR_EVENT_TRIGGER));

                if (1 == ((m_struEventTrigger.struHandleException.dwHandleType >> 2) & 0x01))
                {
                    checkBoxSubCenter.Checked = true;
                }

                if (1 == ((m_struEventTrigger.struHandleException.dwHandleType >> 4) & 0x01))
                {
                    checkBoxEmail.Checked = true;
                }

                if (1 == ((m_struEventTrigger.struHandleException.dwHandleType >> 9) & 0x01))
                {
                    checkBoxFTP.Checked = true;
                }
            
            }

            Marshal.FreeHGlobal(ptrCondBuffer);
            Marshal.FreeHGlobal(ptrOutBuffer);
            Marshal.FreeHGlobal(ptr);

        }

        private void btnThermTriggerSet_Click(object sender, EventArgs e)
        {
            SetThermTrigger();

            int iCondSize = Marshal.SizeOf(m_struThermTriggerCond);
            m_struThermTriggerCond.dwSize = (uint)iCondSize;
            m_struThermTriggerCond.dwChannel = (uint)m_iChannel;
            m_struThermTriggerCond.wPresetNo = (ushort)m_iPresetNo;
            m_struSTDConfig.lpCondBuffer = Marshal.AllocHGlobal(iCondSize);
            IntPtr ptrCondBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(m_struThermTriggerCond));
            Marshal.StructureToPtr(m_struThermometryCond, ptrCondBuffer, false);
            m_struSTDConfig.dwCondSize = (uint)iCondSize;
            m_struSTDConfig.lpCondBuffer = ptrCondBuffer;

            int iInSize = Marshal.SizeOf(m_struEventTrigger);
            m_struEventTrigger.dwSize = (uint)iInSize;
            IntPtr ptrOutBuffer = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(m_struEventTrigger, ptrOutBuffer, false);
            m_struSTDConfig.dwOutSize = (uint)iInSize;
            m_struSTDConfig.lpOutBuffer = ptrOutBuffer;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_SetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_THERMOMETRY_TRIGGER, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：设置测温联动，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("设置测温联动，成功！");
            }

            Marshal.FreeHGlobal(ptrCondBuffer);
            Marshal.FreeHGlobal(ptrOutBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        private void SetThermTrigger()
        {
            if (checkBoxSubCenter.Checked == true)
            {
                m_struEventTrigger.struHandleException.dwHandleType |= (0x01 << 2);
            }

            if (checkBoxEmail.Checked == true)
            {
                m_struEventTrigger.struHandleException.dwHandleType |= (0x01 << 4);
            }

            if (checkBoxFTP.Checked == true)
            {
                m_struEventTrigger.struHandleException.dwHandleType |= (0x01 << 9);
            }
        }

    }
}
