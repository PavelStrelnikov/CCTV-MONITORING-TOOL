using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DecoderCSharpDemo
{
    public partial class DecoderDemo : Form
    {
        public Int32 m_lUserID = -1;
        private bool m_bInitSDK = false;
        private uint iLastErr = 0;
        private string strErr;
        private int iTotalDispChanNum = 0;
        private int[] iDispChanNo = new int[32];
        public CHCNetSDK.NET_DVR_MATRIX_ABILITY_V41 m_struDecAbility = new CHCNetSDK.NET_DVR_MATRIX_ABILITY_V41();

        public DecoderDemo()
        {
            InitializeComponent();

            //SDK Init
            m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                MessageBox.Show("NET_DVR_Init error!");
                return;
            }
            else
            {
                //To save the SDK log
                CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (m_lUserID < 0)
            {
                if (textBoxIP.Text == "" || textBoxPort.Text == "" ||
                textBoxUserName.Text == "" || textBoxPassword.Text == "")
                {
                    MessageBox.Show("Please input prarameters: ");
                    return;
                }
                string DVRIPAddress = textBoxIP.Text;
                Int16 DVRPortNumber = Int16.Parse(textBoxPort.Text);
                string DVRUserName = textBoxUserName.Text;
                string DVRPassword = textBoxPassword.Text;

                // Login the device
                CHCNetSDK.NET_DVR_DEVICEINFO_V30 m_struDeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
                m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref m_struDeviceInfo);
                if (m_lUserID == -1)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    strErr = "NET_DVR_Login_V30 failed, error code= " + iLastErr;
                    // Failed to login and output the error code
                    MessageBox.Show(strErr);
                    return;
                }
                else
                {
                    btnLogin.Text = "Logout";
                    Int32 nSize = Marshal.SizeOf(m_struDecAbility);
                    IntPtr ptrDecAbility = Marshal.AllocHGlobal(nSize);
                    Marshal.StructureToPtr(m_struDecAbility, ptrDecAbility, false);
                    if (!CHCNetSDK.NET_DVR_GetDeviceAbility(m_lUserID, CHCNetSDK.MATRIXDECODER_ABILITY_V41, IntPtr.Zero, 0, ptrDecAbility, (uint)nSize))
                    {
                        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        strErr = "Get MATRIXDECODER_ABILITY_V41 failed, error code= " + iLastErr;
                        //Failed to get the capability set of decoder and output the error code
                        MessageBox.Show(strErr);
                    }
                    else
                    {
                        m_struDecAbility = (CHCNetSDK.NET_DVR_MATRIX_ABILITY_V41)Marshal.PtrToStructure(ptrDecAbility, typeof(CHCNetSDK.NET_DVR_MATRIX_ABILITY_V41));
                        int i = 0;
                        string str;

                        //Decoding channel information
                        for (i = 0; i < m_struDecAbility.byDecChanNums; i++)
                        {
                            str = String.Format("Channel{0}", i + 1);
                            comboBoxDecChan.Items.Add(str);                            
                        }

                        // Display channel information
                        for (i = 0; i < m_struDecAbility.struVgaInfo.byChanNums; i++)
                        {
                            str = String.Format("VGA{0}", i + 1);
                            comboBoxDispChan.Items.Add(str);
                            iDispChanNo[iTotalDispChanNum] = m_struDecAbility.struVgaInfo.byStartChan + i;
                            iTotalDispChanNum++;                            
                        }
                        for (i = 0; i < m_struDecAbility.struBncInfo.byChanNums; i++)
                        {
                            str = String.Format("BNC{0}", i + 1);
                            comboBoxDispChan.Items.Add(str);
                            iDispChanNo[iTotalDispChanNum] = m_struDecAbility.struBncInfo.byStartChan + i;
                            iTotalDispChanNum++;
                        }
                        for (i = 0; i < m_struDecAbility.struHdmiInfo.byChanNums; i++)
                        {
                            str = String.Format("HDMI{0}", i + 1);
                            comboBoxDispChan.Items.Add(str);
                            iDispChanNo[iTotalDispChanNum] = m_struDecAbility.struHdmiInfo.byStartChan + i;
                            iTotalDispChanNum++;
                        }
                        for (i = 0; i < m_struDecAbility.struDviInfo.byChanNums; i++)
                        {
                            str = String.Format("DVI{0}", i + 1);
                            comboBoxDispChan.Items.Add(str);
                            iDispChanNo[iTotalDispChanNum] = m_struDecAbility.struDviInfo.byStartChan + i;
                            iTotalDispChanNum++;
                        }
                        comboBoxDecChan.SelectedIndex = 0;
                        comboBoxDispChan.SelectedIndex = 0;
                    }
                    Marshal.FreeHGlobal(ptrDecAbility);
                }
            }
            else
            {
                // Logout the device
                if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    strErr = "NET_DVR_Logout failed, error code= " + iLastErr; //Failed to logout and output the error code
                    MessageBox.Show(strErr);
                }
                else
                {
                    btnLogin.Text = "Login";
                    m_lUserID = -1;
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            //Logout the device           
            CHCNetSDK.NET_DVR_Logout(m_lUserID);

            
            CHCNetSDK.NET_DVR_Cleanup();

            Application.Exit();
        }

        private void btnDispChanCfg_Click(object sender, EventArgs e)
        {
            // Display Channel Configuration
            DisplayChanCfg dlg = new DisplayChanCfg();
            if (comboBoxDispChan.SelectedIndex == -1)
            {
                return;
            }

            dlg.m_lUserID = m_lUserID;
            dlg.m_struDecAbility = m_struDecAbility;
            dlg.iDisplayIndex = comboBoxDispChan.SelectedIndex;
            dlg.iDisplayNo = iDispChanNo[comboBoxDispChan.SelectedIndex];
            dlg.ShowDialog();
        }

        private void btnDynDecCfg_Click(object sender, EventArgs e)
        {
            // Dynamic Decoding
            DynnamicDecode dlg = new DynnamicDecode();
            if (comboBoxDecChan.SelectedIndex == -1)
            {
                return;
            }

            dlg.m_lUserID = m_lUserID;
            dlg.dwDecChanNum = (uint)comboBoxDecChan.SelectedIndex + (uint)m_struDecAbility.byStartChan;
            dlg.ShowDialog();
        }

        private void btnPassiveDecCfg_Click(object sender, EventArgs e)
        {
            // Passive Decoding
            PassiveDecode dlg = new PassiveDecode();
            if (comboBoxDecChan.SelectedIndex == -1)
            {
                return;
            }

            dlg.m_lUserID = m_lUserID;
            dlg.dwDecChanNum = (uint)comboBoxDecChan.SelectedIndex + (uint)m_struDecAbility.byStartChan;
            dlg.ShowDialog();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }      
    }
}
