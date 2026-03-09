using Common;
using Common.Head;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SDKSystemManagement.Preview;

namespace SDKSystemManagement
{

    public partial class FormPreview : PluginsControl
    {
        private IDeviceTree g_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        private CHCNetSDK.PREVIEW_IFNO[] m_strPanelInfo = new CHCNetSDK.PREVIEW_IFNO[4];
        private int m_iCurDeviceIndex = -1;
        private int m_iCurChanNo = -1;
        private int m_iCurChanIndex = -1;
        private int m_lUserID = -1;
        private int m_lRealHandle = -1;
        public CHCNetSDK.REALDATACALLBACK m_RealDataCallBack = null;
        private CHCNetSDK.STDDATACALLBACK m_StdDataCallBack = null;
        private int lPort = -1;
        private Int32[] m_iPort = new Int32[4];
        private IDeviceTree.DeviceInfo m_deviceInfo = null;
        private IDeviceTree.ChannelInfo m_channelInfo = null;
        private int m_iCurPane = 0;
        public int m_iRtspPort = 554;
        //public HttpClient.RtspDeviceInfo m_struRtspDeviceInfo;
        public int m_iStreamType = 0;
        private RTSP.DataCallBack m_fnCallback = null;
        private IntPtr[] m_pRtsp = new IntPtr[4];
        private string[] m_gatewayChannel = new string[4];
        private string[] m_sdkChannel = new string[4];

        public FormPreview()
        {
            InitializeComponent();

            for (int i = 0; i < 4; i++)
            {
                m_strPanelInfo[i].lRealHandle = -1;

                m_iPort[i] = -1;
            }
            m_strPanelInfo[0].hPlayWnd = m_panelOne.Handle;
            m_strPanelInfo[1].hPlayWnd = m_panelTwo.Handle;
            m_strPanelInfo[2].hPlayWnd = m_panelThree.Handle;
            m_strPanelInfo[3].hPlayWnd = m_panelFour.Handle;
            m_panelOne.BackColor = Color.WhiteSmoke;
            if (g_deviceTree != null)
            { 
                //g_deviceTree.SelectedNodeChanged += g_deviceTree_SelectedNodeChanged;
                this.GetDevicesInfo();
            }

            m_comboBoxLinkMode.SelectedIndex = 0;
            m_comboBoxProtoType.SelectedIndex = 0;
            m_comboBoxStreamType.SelectedIndex = 0;

            groupBox5.Visible = true;

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
        }

        private void m_btnPlay_Click(object sender, EventArgs e)
        {
            m_strPanelInfo[0].hPlayWnd = m_panelOne.Handle;
            m_strPanelInfo[1].hPlayWnd = m_panelTwo.Handle;
            m_strPanelInfo[2].hPlayWnd = m_panelThree.Handle;
            m_strPanelInfo[3].hPlayWnd = m_panelFour.Handle;
            this.GetDevicesInfo();
            if (g_deviceTree != null)
            {
                int dwLinkMode = 0;
                switch (m_comboBoxLinkMode.SelectedIndex)
                {
                    case 0:
                        dwLinkMode = 0;
                        break;
                    case 1:
                        dwLinkMode = 1;
                        break;
                    case 2:
                        dwLinkMode = 7;
                        break;
                    default:
                        break;
                }

                int dwStreamType = (int)m_comboBoxStreamType.SelectedIndex;
                byte byProtoType = (byte)m_comboBoxProtoType.SelectedIndex;
                SDK_StartPlay(m_iCurPane, dwLinkMode, dwStreamType, byProtoType);               
            }
        }



        private void m_btnStop_Click(object sender, EventArgs e)
        {
            SDK_StopPlay(-1);
        }

        private void m_panelOne_MouseDown(object sender, MouseEventArgs e)
        {
            this.m_panelOne.BackColor = Color.WhiteSmoke;
            this.m_panelTwo.BackColor = Color.White;
            this.m_panelThree.BackColor = Color.White;
            this.m_panelFour.BackColor = Color.White;
            m_iCurPane = 0;
        }

        private void m_panelTwo_MouseDown(object sender, MouseEventArgs e)
        {
            this.m_panelOne.BackColor = Color.White;
            this.m_panelTwo.BackColor = Color.WhiteSmoke;
            this.m_panelThree.BackColor = Color.White;
            this.m_panelFour.BackColor = Color.White;
            m_iCurPane = 1;
        }

        private void m_panelThree_MouseDown(object sender, MouseEventArgs e)
        {
            this.m_panelOne.BackColor = Color.White;
            this.m_panelTwo.BackColor = Color.White;
            this.m_panelThree.BackColor = Color.WhiteSmoke;
            this.m_panelFour.BackColor = Color.White;
            m_iCurPane = 2;
        }

        private void m_panelFour_MouseDown(object sender, MouseEventArgs e)
        {
            this.m_panelOne.BackColor = Color.White;
            this.m_panelTwo.BackColor = Color.White;
            this.m_panelThree.BackColor = Color.White;
            this.m_panelFour.BackColor = Color.WhiteSmoke;
            m_iCurPane = 3;
        }

        private void m_panelOne_Paint(object sender, PaintEventArgs e)
        {
            if (this.m_iCurPane == 0)
            {
                ControlPaint.DrawBorder(e.Graphics, m_panelOne.ClientRectangle, Color.Red, ButtonBorderStyle.Solid);
            }
            else
            {
                ControlPaint.DrawBorder(e.Graphics, m_panelOne.ClientRectangle, Color.Black, ButtonBorderStyle.None);
            }
        }

        private void m_panelTwo_Paint(object sender, PaintEventArgs e)
        {
            if (this.m_iCurPane == 1)
            {
                ControlPaint.DrawBorder(e.Graphics, m_panelTwo.ClientRectangle, Color.Red, ButtonBorderStyle.Solid);
            }
            else
            {
                ControlPaint.DrawBorder(e.Graphics, m_panelTwo.ClientRectangle, Color.Black, ButtonBorderStyle.None);
            }
        }

        private void m_panelThree_Paint(object sender, PaintEventArgs e)
        {
            if (this.m_iCurPane == 2)
            {
                ControlPaint.DrawBorder(e.Graphics, m_panelThree.ClientRectangle, Color.Red, ButtonBorderStyle.Solid);
            }
            else
            {
                ControlPaint.DrawBorder(e.Graphics, m_panelThree.ClientRectangle, Color.Black, ButtonBorderStyle.None);
            }
        }

        private void m_panelFour_Paint(object sender, PaintEventArgs e)
        {
            if (this.m_iCurPane == 3)
            {
                ControlPaint.DrawBorder(e.Graphics, m_panelFour.ClientRectangle, Color.Red, ButtonBorderStyle.Solid);
            }
            else
            {
                ControlPaint.DrawBorder(e.Graphics, m_panelFour.ClientRectangle, Color.Black, ButtonBorderStyle.None);
            }
        }

        private void FormPreview_Load(object sender, EventArgs e)
        {
            this.checkBoxShowPTZPanel.Checked = false;
            this.splitContainerPreviewAndPTZ.Panel2Collapsed = true;

            for (int index = 0; index < 64; ++index)
            {
                ListViewItem item = new ListViewItem();
                item.SubItems[0].Text = (index + 1).ToString();
                item.SubItems.Add("");
                this.listViewPresets.Items.Add(item);
            }

        }

        private void checkBoxShowPTZPanel_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBoxShowPTZPanel.Checked)
            {
                this.splitContainerPreviewAndPTZ.Panel2Collapsed = false;
            }
            else
            {
                this.splitContainerPreviewAndPTZ.Panel2Collapsed = true;
            }
        }

        private void listViewPresets_SizeChanged(object sender, EventArgs e)
        {
            this.AdjustListViewPresetsColWidth();
        }

        private void AdjustListViewPresetsColWidth()
        {
            int colCount = this.listViewPresets.Columns.Count;
            if (colCount == 2)
            {
                this.listViewPresets.Columns[1].Width = this.listViewPresets.Width - this.listViewPresets.Columns[0].Width;
            }
        }

        private void listViewPresets_DoubleClick(object sender, EventArgs e)
        {
            this.gotoToolStripMenuItem_Click(null, null);
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.m_deviceInfo == null)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "Preview", "Should Select a Device Firstly");
            }
            MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "Preview", "Adding Preset");


        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void gotoToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            if (this.g_deviceTree == null)
            {
                return;
            }
            if (this.m_deviceInfo == null)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "Preview", "Should Select a Device Firstly");
            }
            uint up = 21;
            if (this.m_iCurPane >= 0)
            {
                SdkTURN(up, m_iCurPane);
            }
        }

        private void buttonLeft_Click(object sender, EventArgs e)
        {
            if (this.g_deviceTree == null)
            {
                return;
            }
            if (this.m_deviceInfo == null)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "Preview", "Should Select a Device Firstly");
            }
            uint left = 23;
            if (this.m_iCurPane >= 0)
            {
                SdkTURN(left, m_iCurPane);
            }
        }

        private void buttonRight_Click(object sender, EventArgs e)
        {
            if (this.g_deviceTree == null)
            {
                return;
            }
            if (this.m_deviceInfo == null)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "Preview", "Should Select a Device Firstly");
            }
            uint right = 24;
            if (this.m_iCurPane >= 0)
            {
                SdkTURN(right, m_iCurPane);
            }          
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            if (this.g_deviceTree == null)
            {
                return;
            }
            if (this.m_deviceInfo == null)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "Preview", "Should Select a Device Firstly");
            }
            uint down = 22;
            if (this.m_iCurPane >= 0)
            {
                SdkTURN(down, m_iCurPane);
            }      
        }

        private void buttonFocus_Click(object sender, EventArgs e)
        {
            if (this.g_deviceTree == null)
            {
                return;
            }
            uint i=12;
            SdkTURN(i, m_iCurPane);
            var deviceTreeType = g_deviceTree.GetDeviceTreeType();
 

        }
    }
}
