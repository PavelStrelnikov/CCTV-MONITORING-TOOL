using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace DecoderCSharpDemo
{
    public partial class PassiveDecode : Form
    {
        public Int32 m_lUserID = -1;
        public uint dwDecChanNum;
        private uint iLastErr = 0;
        private string strErr;
        private Int32 lPassiveHandle = -1;
        private CHCNetSDK.NET_DVR_MATRIX_PASSIVEMODE m_struPassivePara = new CHCNetSDK.NET_DVR_MATRIX_PASSIVEMODE();
        public Int32 lUserID = -1;
        private int lRealHandle = -1;
        public FileStream hFileHandle = null;
        public int iStreamSize = 0;
        public bool hExitThread = false;
        public Thread filetrd = null;

        public PassiveDecode()
        {
            InitializeComponent();
        }

        private void PassiveDecode_Load(object sender, EventArgs e)
        {
            comboBoxTransProtol.SelectedIndex = 0;
            comboBoxStreamMode.SelectedIndex = 0;
            comboBoxStreamType.SelectedIndex = 0;
            comboBoxRealProtol.SelectedIndex = 0;
        }

        private void btnStartDecode_Click(object sender, EventArgs e)
        {
            m_struPassivePara.wTransProtol = (ushort)comboBoxTransProtol.SelectedIndex; //protocol：0-TCP，1-UDP，2-MCAST
            m_struPassivePara.wPassivePort = ushort.Parse(textBoxPort.Text); 
            m_struPassivePara.byStreamType = (byte)(comboBoxStreamMode.SelectedIndex + 1);  

            lPassiveHandle = CHCNetSDK.NET_DVR_MatrixStartPassiveDecode(m_lUserID, dwDecChanNum, ref m_struPassivePara);
            if (lPassiveHandle == -1)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                strErr = "NET_DVR_MatrixStartPassiveDecode failed, error code= " + iLastErr;
                //Failed to start dynamic decoding and output the error code
                MessageBox.Show(strErr);
                return;
            }
            else
            {
                MessageBox.Show("Successful to start passive decoding!");
                hExitThread = false;
            }
        }

        public void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, IntPtr pBuffer, UInt32 dwBufSize, IntPtr pUser)
        {
       
            if (!CHCNetSDK.NET_DVR_MatrixSendData(lPassiveHandle, pBuffer, dwBufSize))
            {
                // Failed to send data to the decoder
            }
        }


        private void btnSendData_Click(object sender, EventArgs e)
        {
            if (comboBoxStreamMode.SelectedIndex == 0) //Real-time Stream Decoding: Logon Encoder obtains the real-time stream and forwards it to the decoder
            {
                string DVRIPAddress = textBoxDevAddr.Text;
                Int16 DVRPortNumber = Int16.Parse(textBoxDevPort.Text);
                string DVRUserName = textBoxUserName.Text;
                string DVRPassword = textBoxPassword.Text;

                // Login the device
                CHCNetSDK.NET_DVR_DEVICEINFO_V30 m_struDeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
                lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref m_struDeviceInfo);
                if (lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    strErr = "NET_DVR_Login_V30 failed, error code= " + iLastErr;
                    // Failed to login and output the error code
                    MessageBox.Show(strErr);
                    return;
                }
                else
                {
                    CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                    lpPreviewInfo.hPlayWnd = IntPtr.Zero;//The preview window is set to NULL and only stream is taken without decoding
                    lpPreviewInfo.lChannel = Int32.Parse(textBoxChannel.Text);//Preview device channel
                    lpPreviewInfo.dwStreamType = (uint)comboBoxStreamType.SelectedIndex;//stream Type：0-main，1-sub-stream，2-3rdStream，and so no
                    lpPreviewInfo.dwLinkMode = (uint)comboBoxRealProtol.SelectedIndex;//connect mode：0- TCP，1- UDP，2- Multicast ，3- RTP，4-RTP/RTSP，5-RSTP/HTTP 
                    lpPreviewInfo.bBlocked = true; //0-Non-blocking stream，1- Blocking stream

                    CHCNetSDK.REALDATACALLBACK RealData = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack);
                    IntPtr pUser = new IntPtr();//user data

                    //Start live view 
                    lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(lUserID, ref lpPreviewInfo, RealData, pUser);
                    if (lRealHandle < 0)
                    {
                        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        strErr = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr;
                        // Failed to start live view and output the error code
                        MessageBox.Show(strErr);
                        return;
                    }
                    else
                    {
                        // Success to start live view  
                        MessageBox.Show("Start to send the steam data to the decoder!");
                    }
                }

            }
            else //File Stream Decoding: Read the data in the file and send it to the decoder
            {
                int iDataSize = Int32.Parse(textBoxReadSize.Text);
                if (iDataSize < 1 || iDataSize > 512)
                {
                    MessageBox.Show("Please set a applicable value!");
                    return;
                }

                iStreamSize = int.Parse(textBoxReadSize.Text);

                if (hFileHandle != null)
                {
                    hFileHandle.Close();
                    hFileHandle = null;
                }
                hFileHandle = new FileStream(textBoxFilePath.Text, FileMode.Open, FileAccess.Read); // Open the file             
                if (hFileHandle == null)
                {
                    return;                
                }
              
                int lHandle = lPassiveHandle;
                filetrd = new Thread(FileThreadTask); //Create threads to read file data (for reference only
                filetrd.IsBackground = true;
                filetrd.Start((object)lHandle);
                MessageBox.Show("Start to send the file data to the decoder!");
            }
        }

        private void FileThreadTask(object lHandle)
        {

            long left = hFileHandle.Length;
            byte[] tmpFile = new byte[iStreamSize * 1024];
            int maxLength = tmpFile.Length;
            int istart = 0;
            int iRealSize = 0;

            while (left > 0)
            {
                if (hExitThread)
                    break;

                hFileHandle.Position = istart;
                iRealSize = 0;
                if (left < maxLength)
                    iRealSize = hFileHandle.Read(tmpFile, 0, Convert.ToInt32(left));
                else
                    iRealSize = hFileHandle.Read(tmpFile, 0, maxLength);
                
                if (iRealSize == 0)
                    break;

                istart += iRealSize;
                left -= iRealSize;

                //Send read file data to decoder 
                IntPtr pBuffer = Marshal.AllocHGlobal((Int32)iRealSize);
                Marshal.Copy(tmpFile, 0, pBuffer, iRealSize);
                if (!CHCNetSDK.NET_DVR_MatrixSendData((int)lHandle, pBuffer, (uint)iRealSize))
                {
                    // Failed to send data to the decoder
                    
                }
                Marshal.FreeHGlobal(pBuffer);
                Thread.Sleep(10);
            }

            if (!hExitThread)
            {
                MessageBox.Show("Finished to read data from the video file!");
            }

            filetrd.Abort();
            filetrd = null;
        }

        private void btnStopDecode_Click(object sender, EventArgs e)
        {
            if (comboBoxStreamMode.SelectedIndex == 0) //Real-time Stream Decoding: Logon Encoder obtains the real-time stream and forwards it to the decoder
            {
                //Stop live view 
                if (lRealHandle >= 0)
                {
                    if (!CHCNetSDK.NET_DVR_StopRealPlay(lRealHandle))
                    {
                        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        strErr = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                        MessageBox.Show(strErr);
                        return;
                    }
                }

                // Logout the device
                if (lUserID >= 0)
                {
                    if (!CHCNetSDK.NET_DVR_Logout(lUserID))
                    {
                        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        strErr = "NET_DVR_Logout failed, error code= " + iLastErr;
                        MessageBox.Show(strErr);
                        return;
                    }
                }

                // Stop the passive decoding
                if (!CHCNetSDK.NET_DVR_MatrixStopPassiveDecode(lPassiveHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    strErr = "NET_DVR_MatrixStopPassiveDecode failed, error code= " + iLastErr;
                    MessageBox.Show(strErr);
                    return;
                }
                lPassiveHandle = -1;
                MessageBox.Show("Successful to stop the passive decoding!");
            }
            else //File Stream Decoding: Read the data in the file and send it to the decode
            {
                hExitThread = true;

                if (hFileHandle != null)
                {
                    hFileHandle.Close();
                    hFileHandle = null;
                }

                // Stop the passive decoding
                if (!CHCNetSDK.NET_DVR_MatrixStopPassiveDecode(lPassiveHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    strErr = "NET_DVR_MatrixStopPassiveDecode failed, error code= " + iLastErr;
                    MessageBox.Show(strErr);
                    return;
                }
                lPassiveHandle = -1;
                MessageBox.Show("Successful to stop the passive decoding!");
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (filetrd != null)
            {
                filetrd.Abort();
                filetrd = null;
            }
            
            if (hFileHandle != null)
            {
                hFileHandle.Close();
                hFileHandle = null;
            }

            this.Close();
            this.Dispose(); 
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All files (*.*)|*.*|All files (*.*)|*.*";
            dlg.FilterIndex = 2;
            dlg.RestoreDirectory = true;
            if (dlg.ShowDialog() == DialogResult.OK) 
            {
                textBoxFilePath.Text = dlg.FileName;  
	        }
        }
    }
}
