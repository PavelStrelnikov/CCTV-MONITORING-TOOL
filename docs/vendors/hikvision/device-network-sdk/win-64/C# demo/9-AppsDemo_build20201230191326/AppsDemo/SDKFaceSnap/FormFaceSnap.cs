/*******************************************************
Copyright All Rights Reserved. (C) HangZhou Hikvision System Technology Co., Ltd. 
File ：    FormFaceSnap.cs 
Developer：    Hikvision
Author：    chenzhixue@hikvision.com
Period：    2019-07-18
Describe：    FormFaceSnap.cs
********************************************************/

using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TINYXMLTRANS;

namespace SDKFaceSnap
{
    public partial class FormFaceSnap : PluginsControl
    {
        private IDeviceTree g_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        private int m_iCurDeviceIndex = -1;
        private int m_iCurChanIndex = -1;
        private IDeviceTree.DeviceInfo m_deviceInfo = null;
        private IDeviceTree.ChannelInfo m_channelInfo = null;
        public static int m_iCurChanNo = -1;
        public static int m_lUserID = -1;
        public static bool m_bMultiScene = false;
        public static int m_iMultiSceneID = 0;


        public FormFaceSnap()
        {
            InitializeComponent();

            if (g_deviceTree != null)
            {
                g_deviceTree.SelectedNodeChanged += g_deviceTree_SelectedNodeChanged;
                this.GetDevicesInfo();
            }
            this.GetLoginInfo();
        }

        private void GetDevicesInfo()
        {
            this.m_deviceInfo = g_deviceTree.GetSelectedDeviceInfo();
            this.m_channelInfo = g_deviceTree.GetSelectedChannelInfo();
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

        void g_deviceTree_SelectedNodeChanged()
        {
            this.GetDevicesInfo();
            this.GetLoginInfo();
        }

        private void FormFaceSnap_Load(object sender, EventArgs e)
        {
            MainTabControl.DrawItem += new DrawItemEventHandler(this.MainTabControl_DrawItem);
            this.MainTabControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainTabControl_MouseDown);
            Show_Form("Alarm", MainTabControl);

            toolStripStatusTime.Text = DateTime.Now.ToString();
        }

        public static void Show_Form(string FrmName, System.Windows.Forms.TabControl tab)
        {
            if (FrmName == "Alarm")
            {
                FormFaceSnapAlarm Alarm = new FormFaceSnapAlarm();
                Add_TabPage(FrmName, Alarm, tab);
            }
            else
            {
                MessageBox.Show("Set TAB page failed !");
                return;
            }
        }

        public static void Add_TabPage(string str, Form myForm, System.Windows.Forms.TabControl tab)
        {
            if (tabControlCheckHave(tab, str))
            {
                return;
            }
            else
            {
                String strtemp = "    " + str + "    ";
                tab.TabPages.Add(strtemp);
                tab.SelectTab(tab.TabPages.Count - 1);
                myForm.Dock = DockStyle.Fill;
                myForm.TopLevel = false;
                myForm.Show();
                myForm.Parent = tab.SelectedTab;
            }
        }

        public static bool tabControlCheckHave(System.Windows.Forms.TabControl tab, String tabName)
        {
            for (int i = 0; i < tab.TabCount; i++)
            {
                if (tab.TabPages[i].Text.Trim() == tabName)
                {
                    tab.SelectedIndex = i;
                    return true;
                }
            }
            return false;
        }

        const int CLOSE_SIZE = 14;
        private void MainTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                Rectangle TabMainrec = MainTabControl.ClientRectangle;
                SolidBrush TabBoxBac = new SolidBrush(Color.WhiteSmoke);
                e.Graphics.FillRectangle(TabBoxBac, TabMainrec);

                for (int i = 0; i < MainTabControl.TabPages.Count; i++)
                {
                    Color recColor = MainTabControl.SelectedIndex == i ? Color.DodgerBlue : Color.White;
                    SolidBrush recColorBrush = new SolidBrush(recColor);
                    Rectangle myTabRect = this.MainTabControl.GetTabRect(i);
                    e.Graphics.FillRectangle(recColorBrush, myTabRect);

                    Color StringColor = MainTabControl.SelectedIndex == i ? Color.White : Color.Gray;
                    SolidBrush StringColorBrush = new SolidBrush(StringColor);
                    e.Graphics.DrawString(this.MainTabControl.TabPages[i].Text
                    , this.Font, StringColorBrush, myTabRect.X + 2, myTabRect.Y + 2);

                    using (Pen p = new Pen(recColor))
                    {
                        myTabRect.Offset(myTabRect.Width - (CLOSE_SIZE + 3), 2);
                        myTabRect.Width = CLOSE_SIZE;
                        myTabRect.Height = CLOSE_SIZE;
                        e.Graphics.DrawRectangle(p, myTabRect);
                    }

                    using (Brush b = new SolidBrush(recColor))
                    {
                        e.Graphics.FillRectangle(b, myTabRect);
                    }

                    Color closeColor = MainTabControl.SelectedIndex == i ? Color.White : Color.Gray;
                    using (Pen p = new Pen(closeColor))
                    {
                        Point p1 = new Point(myTabRect.X + 3, myTabRect.Y + 3);
                        Point p2 = new Point(myTabRect.X + myTabRect.Width - 3, myTabRect.Y + myTabRect.Height - 3);
                        e.Graphics.DrawLine(p, p1, p2);

                        Point p3 = new Point(myTabRect.X + 3, myTabRect.Y + myTabRect.Height - 3);
                        Point p4 = new Point(myTabRect.X + myTabRect.Width - 3, myTabRect.Y + 3);
                        e.Graphics.DrawLine(p, p3, p4);
                    }
                }
                e.Graphics.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);   //提示信息
            }
        }

        private void MainTabControl_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    int x = e.X;
                    int y = e.Y;

                    Rectangle tabRect = this.MainTabControl.GetTabRect(this.MainTabControl.SelectedIndex);
                    tabRect.Offset(tabRect.Width - 0x12, 2);
                    tabRect.Width = 15;
                    tabRect.Height = 15;
                    if ((((x > tabRect.X) && (x < tabRect.Right)) && (y > tabRect.Y)) && (y < tabRect.Bottom))
                    {
                        this.MainTabControl.TabPages.Remove(this.MainTabControl.SelectedTab);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

        }

        private void MenuItemAlarm_Click(object sender, EventArgs e)
        {
            Show_Form("Alarm", MainTabControl);
        }

        private void timerStatus_Tick(object sender, EventArgs e)
        {
            toolStripStatusTime.Text = DateTime.Now.ToString();
        }

    }
}
