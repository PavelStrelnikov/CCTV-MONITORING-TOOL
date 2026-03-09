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
using System.Threading;


namespace SDKConfiguration
{
    public partial class FormUpgrade : PluginsControl
    {
        #region "dll region"
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        #endregion
        public const int WM_LISTENED_UPGRADE = 100;
        private FormUpgrade m_UpgradeHandle;
        public string MessageInfo = string.Empty;
        public int UpgradePos = 0;

        public int m_lUserID = -1;
        public int m_lServerID = -1;
        public int m_lChannel = -1;
        public int m_iDeviceIndex = -1;
        public int m_lUpgradeHandle = -1;
        public String m_szFileName = null;
        public bool m_UpgradeState = false;
     
        public const int ENUM_UPGRADE_DVR = 0;//普通设备升级
        public const int ENUM_UPGRADE_ADAPTER = 1;//DVR适配器升级
        public const int ENUM_UPGRADE_VCALIB = 2; //智能库升级
        public const int ENUM_UPGRADE_OPTICAL = 3; //光端机升级
        public const int ENUM_UPGRADE_ACS = 4; //门禁系统升级
        public const int ENUM_UPGRADE_AUXILIARY_DEV = 5;//辅助设备升级
        public const int ENUM_UPGRADE_LED = 6; //LED发送卡和接收卡升级
        public const int ENUM_UPGRADE_INTELLIGENT = 7; //中心智能设备升级
        public const int UPGRADE_TIMER = 6;		//update timer

        public const int   STEP_READY       =   0;     //准备升级
        public const int   STEP_RECV_DATA   =    1;    //接收升级包数据
        public const int   STEP_UPGRADE     =    2;    //升级系统
        public const int   STEP_BACKUP      =    3;    //备份系统
        public const int   STEP_SEARCH      =  255;  //搜索升级文件

        public FormUpgrade()
        {
            InitializeComponent();
            ComAssistDev.SelectedIndex = 0;
            ComNetEnv.SelectedIndex = 0;
            ComUpgradeType.SelectedIndex = 0;
            BtnChoseDev.Hide();
            labelSequence.Hide();
            SequenceNum.Hide();
            labelCardType.Hide();
            ComCardType.Hide();
            labelChannel.Hide();
            ComChannel.Hide();
            BtnUpgradeCopy.Hide();
            BtnStopUpgrade.Hide();
        }

        private void ComUpgradeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ComUpgradeType.SelectedIndex == 3)
            {
                BtnChoseDev.Show();
                labelSequence.Show();
                SequenceNum.Show();
                labelChannel.Hide();
                ComChannel.Hide();
                labelCardType.Hide();
                ComCardType.Hide();
                BtnUpgradeCopy.Show();
                BtnStopUpgrade.Show();
            }
            else if (ComUpgradeType.SelectedIndex == 4)
            {
                labelChannel.Show();
                ComChannel.Show();
                labelCardType.Hide();
                ComCardType.Hide();
                BtnChoseDev.Hide();
                labelSequence.Hide();
                SequenceNum.Hide();
                BtnUpgradeCopy.Hide();
                BtnStopUpgrade.Hide();
            }
            else if (ComUpgradeType.SelectedIndex == 5)
            {
                labelCardType.Show();
                ComCardType.Show();
                labelChannel.Hide();
                ComChannel.Hide();
                BtnChoseDev.Hide();
                labelSequence.Hide();
                SequenceNum.Hide();
                BtnUpgradeCopy.Hide();
                BtnStopUpgrade.Hide();
            }
            else
            {
                BtnChoseDev.Hide();
                labelSequence.Hide();
                SequenceNum.Hide();
                labelChannel.Hide();
                ComChannel.Hide();
                labelCardType.Hide();
                ComCardType.Hide();
                BtnUpgradeCopy.Hide();
                BtnStopUpgrade.Hide();
            }
        }
        //寻找升级文件
        private void BtnScan_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog.Filter = "Fingerprint file|*.dat|All documents|*.*";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                UpgradeFilePath.Text = openFileDialog.FileName;
                m_szFileName = UpgradeFilePath.Text;
            }
        }
        //子线程访问主线程中的控件，需要使用代理来实现。
        delegate void SetTextCallback();
        public void UpdateUpgradeInfo()
        {
            this.labelUpgradeState.Text = MessageInfo;
            this.UpgradeBar.Increment(UpgradePos);
        }

        private void BtnUpgrade_Click(object sender, EventArgs e)
        {
            if (ComUpgradeType.SelectedIndex == 0) // DVR upgrade 
            {
                m_lUpgradeHandle = CHCNetSDK.NET_DVR_Upgrade(m_lServerID, m_szFileName);
            }
            else if (ComUpgradeType.SelectedIndex == 1) // adapter upgrade
            {
                m_lUpgradeHandle = CHCNetSDK.NET_DVR_AdapterUpgrade(m_lServerID, m_szFileName);
            }
            else if (ComUpgradeType.SelectedIndex == 2) // vca lib upgrade
            {
                m_lUpgradeHandle = CHCNetSDK.NET_DVR_VcalibUpgrade(m_lServerID, ComChannel.SelectedIndex + 1, m_szFileName);
            }
            else if (ComUpgradeType.SelectedIndex == 3)
            {
                int SequenceNo = 0;
                IntPtr SequencePtr = IntPtr.Zero;
                SequencePtr = Marshal.AllocHGlobal(Marshal.SizeOf(SequenceNo));
                for (int i = 0; i < Marshal.SizeOf(SequenceNo); i++)
                {
                    Marshal.WriteByte(SequencePtr, i, 0);
                }
                m_lUpgradeHandle = CHCNetSDK.NET_DVR_Upgrade_V40(m_lServerID, ENUM_UPGRADE_ACS, m_szFileName, SequencePtr, 4);
                int error = (int)CHCNetSDK.NET_DVR_GetLastError();
            }
            else if (ComUpgradeType.SelectedIndex  == 4)
            {
                CHCNetSDK.NET_DVR_AUXILIARY_DEV_UPGRADE_PARAM struAuxiliaryDevUpgradeParam = new CHCNetSDK.NET_DVR_AUXILIARY_DEV_UPGRADE_PARAM();
                struAuxiliaryDevUpgradeParam.dwSize = (uint)Marshal.SizeOf(struAuxiliaryDevUpgradeParam);
                //struAuxiliaryDevUpgradeParam.byDevType = 0;//目前视频报警主机辅助设备类型只有键盘
                struAuxiliaryDevUpgradeParam.byDevType = (byte)ComAssistDev.SelectedIndex;
                struAuxiliaryDevUpgradeParam.dwDevNo = (byte)ComChannel.SelectedIndex;

                IntPtr struAuxiliaryDevUpgradeParamPtr = Marshal.AllocHGlobal(Marshal.SizeOf(struAuxiliaryDevUpgradeParam));
                m_lUpgradeHandle = CHCNetSDK.NET_DVR_Upgrade_V40(m_lServerID, ENUM_UPGRADE_AUXILIARY_DEV, m_szFileName,struAuxiliaryDevUpgradeParamPtr, (ushort)Marshal.SizeOf(struAuxiliaryDevUpgradeParam));
            }
            else if (ComUpgradeType.SelectedIndex == 5)
            {
                int dwCardType = ComCardType.SelectedIndex + 1;
                IntPtr CardTypePtr = new IntPtr(dwCardType);
               m_lUpgradeHandle = CHCNetSDK.NET_DVR_Upgrade_V40(m_lServerID, ENUM_UPGRADE_LED, m_szFileName,CardTypePtr, 4);
            }
            else if (ComUpgradeType.SelectedIndex == 6)
            {
                int dwCardType = ComCardType.SelectedIndex + 1;
                CHCNetSDK.NET_DVR_UPGRADE_PARAM struUpgradeParam = new CHCNetSDK.NET_DVR_UPGRADE_PARAM();
                struUpgradeParam.dwUpgradeType = ENUM_UPGRADE_INTELLIGENT;
                struUpgradeParam.sFileName = Marshal.StringToHGlobalAnsi(m_szFileName);
                string unitIDStr = unitID.Text;
                IntPtr unitIDPtr = Marshal.StringToHGlobalAnsi(unitIDStr);
                struUpgradeParam.pUnitIdList[0] =  unitIDPtr;
                IntPtr struUpgradeParamPtr = Marshal.AllocHGlobal(Marshal.SizeOf(struUpgradeParam));
                m_lUpgradeHandle = (int)CHCNetSDK.NET_DVR_Upgrade_V50(m_lServerID, struUpgradeParamPtr);
            }
            if (m_lUpgradeHandle < 0)
            {
                MessageBox.Show("Upgrade Failed");
            }
            else
            {
                m_UpgradeState = true;
                this.labelUpgradeState.Text = "Status: Server is upgrading, please wait......";
                //定义一个定时器
                System.Timers.Timer t = new System.Timers.Timer(5000);  //每隔5秒执行一次，查询升级进度
                t.Elapsed += new System.Timers.ElapsedEventHandler(showProgress);//到达时间的时候执行事件
                t.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
                t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            }	     
        }
       

        public void showProgress(object source,System.Timers.ElapsedEventArgs e)
        {
            if(m_UpgradeState)
            {
                int UpgradeStatic = CHCNetSDK.NET_DVR_GetUpgradeState(m_lUpgradeHandle);
                int dwError = (int)CHCNetSDK.NET_DVR_GetLastError();
                int iPos = CHCNetSDK.NET_DVR_GetUpgradeProgress(m_lUpgradeHandle);
                int PrePos = 0;

                int iSubProgress = -1;
                IntPtr SubProgressPtr = IntPtr.Zero;
                SubProgressPtr = Marshal.AllocHGlobal(Marshal.SizeOf(iSubProgress));
                for (int i = 0; i < Marshal.SizeOf(iSubProgress); i++)
                {
                    Marshal.WriteByte(SubProgressPtr, i, 0);
                }
           
                int iStep = (int)CHCNetSDK.NET_DVR_GetUpgradeStep(m_lUpgradeHandle, SubProgressPtr);
                if (iStep !=  -1)
                {
                    iSubProgress = (int)SubProgressPtr;
                    switch (iStep)
                    {
                        case STEP_READY:
                            MessageInfo = "Ready to upgrade file";
                            this.Invoke(new SetTextCallback(UpdateUpgradeInfo));
                            break;
                        case STEP_RECV_DATA:
                            MessageInfo = "Receving upgrade file";
                            this.Invoke(new SetTextCallback(UpdateUpgradeInfo));
                            break;
                        case STEP_UPGRADE:
                            MessageInfo = "Upgrading system";
                            this.Invoke(new SetTextCallback(UpdateUpgradeInfo));
                            break;
                        case STEP_BACKUP:
                            MessageInfo = "Backuping system";
                            this.Invoke(new SetTextCallback(UpdateUpgradeInfo));
                            break;
                        case STEP_SEARCH:
                            MessageInfo = "Searching  upgrade file";
                            this.Invoke(new SetTextCallback(UpdateUpgradeInfo));
                            break;
                        default:
                            MessageInfo = "Unknow step";
                            this.Invoke(new SetTextCallback(UpdateUpgradeInfo));
                            break;
                    }
                }
                if (iPos > 0)
                {
                    int TempPos = iPos - PrePos;
                    UpgradePos = TempPos;
                    PrePos = iPos;
                }
                if (UpgradeStatic == 2)
                {
                    MessageInfo = labelUpgradeState.Text = "Status: Device is upgrading, please wait......";
                    this.Invoke(new SetTextCallback(UpdateUpgradeInfo));
                }
                else
                {
                    bool modelFileNeedUpdate = false;
				switch (UpgradeStatic)
				{
				case -1:
					MessageBox.Show("升级设备失败");	
					break;
                case 1:
                    if (true)
                    {
                        CHCNetSDK.NET_DVR_XML_CONFIG_INPUT xmlInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
                        CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT xmlOutput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();
                        xmlInput.dwSize = (uint)Marshal.SizeOf(xmlInput);
                        xmlOutput.dwSize = (uint)Marshal.SizeOf(xmlOutput);
                        string strUrl = "GET /ISAPI/ITC/AlgorithmsState\r\n";
                        byte[] UrlArr = System.Text.Encoding.Default.GetBytes(strUrl);
                        IntPtr pUrl = Marshal.AllocHGlobal(strUrl.Length);
                        for (int i = 0; i < strUrl.Length; i++)
                        {
                            Marshal.WriteByte(pUrl,i,0);
                        }
                        pUrl = Marshal.StringToHGlobalAnsi(strUrl);


                        xmlInput.lpRequestUrl = pUrl;
                        xmlInput.dwRequestUrlLen = (uint)strUrl.Length;
                        xmlInput.lpInBuffer = IntPtr.Zero;
                        xmlInput.dwInBufferSize = 0;
                        xmlInput.dwRecvTimeOut = 1000;

                        IntPtr pxmlInput = Marshal.AllocHGlobal(Marshal.SizeOf(xmlInput));
                        Marshal.StructureToPtr(xmlInput,pxmlInput,false);


                        IntPtr pOutBuf = Marshal.AllocHGlobal(5*1024);
                        for(int i = 0; i < 5*1024; i++)
                        {
                            Marshal.WriteByte(pOutBuf,i,0);
                        }
                        xmlOutput.lpOutBuffer = pOutBuf;
                        xmlOutput.dwOutBufferSize = 5 * 1024;

                        IntPtr pxmlOutput = Marshal.AllocHGlobal(Marshal.SizeOf(xmlOutput));
                        Marshal.StructureToPtr(xmlOutput,pxmlOutput,false);

                        if (CHCNetSDK.NET_DVR_STDXMLConfig(m_lServerID, pxmlInput,pxmlOutput))
                        {
                            xmlOutput = (CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT)Marshal.PtrToStructure(pxmlOutput,typeof(CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT));

                            string strRetXml = Marshal.PtrToStringAnsi(xmlOutput.lpOutBuffer);
                            if (strRetXml.Contains("modelFileNeedUpdate"))
                            {
                                modelFileNeedUpdate = true;
                            }
                        }
                        else
                        {
                            //MessageBox.Show("NET_DVR_STDXMLConfig Failed");
                        }
                        Marshal.FreeHGlobal(pUrl);
                        Marshal.FreeHGlobal(pxmlInput);
                        Marshal.FreeHGlobal(pOutBuf);
                        Marshal.FreeHGlobal(pxmlOutput);
                    }
                    if (ComCardType.SelectedIndex == 4)
                    {
                        if (modelFileNeedUpdate)
                        {
                            MessageBox.Show("Status:upgrade successfully and model file need update");
                        }
                        else
                        {
                            MessageBox.Show("Status:upgrade successfully");
                            m_UpgradeState = false;
                        }
                    }
                    else
                    {
                        if (modelFileNeedUpdate)
                        {
                            MessageBox.Show("Status:upgrade successfully, update mode file and reboot please");
                        }
                        else
                        {
                            MessageBox.Show("Status:upgrade successfully, reboot please");
                            m_UpgradeState = false;
                        }
                    }
                    UpgradeBar.Increment(100);
					break;			
				case 3:
                    MessageBox.Show("Status:upgrade failed");
					break;
				case 4:
                    MessageBox.Show("Status:get data with probrem from device, status unknown");				
					break;
				case 5:
                    MessageBox.Show("Status:Upgrade file language mismatch");			
					break;
				case 6:
                    MessageBox.Show("Status:Upgrade file write Flash Fail!");					
					break;
                case 7:
                    MessageBox.Show("Status:Upgrade Pack Type Mismatch!");		
					break;
                case 8:
                    MessageBox.Show("Status:Upgrade Pack Version Mismatch!");			
					break;
                case 9:
                    MessageBox.Show("Status:System has been locked (file lock)!");
                    break;
                case 10:
                    MessageBox.Show("Status:Backup regional anomaly!");
                    break;
                case 11:
                    MessageBox.Show("Status:System card is full!");
                    break;
                case 12:
                    MessageBox.Show("Status:Reconnect failed(Invalid SessionID)!");
                    break;
                case 13:
                    MessageBox.Show("Status:Server is Busy!");
                    break;
				default: 
					break;
				}
                  StopUpgrade();
             }
        }
      }

       public void StopUpgrade()
       {
            m_UpgradeState = false;
            m_lUpgradeHandle = -1;
       }

       private void BtnStopUpgrade_Click(object sender, EventArgs e)
       {
            StopUpgrade();
       }

       private void BtnExit_Click(object sender, EventArgs e)
       {
           if (m_UpgradeState)
           {
               m_UpgradeState = false;
           }
           if (m_lUpgradeHandle != -1)
           {
               m_lUpgradeHandle = -1;
           }
           this.Close();
       }

       //private void BtnUpgradeCopy_Click(object sender, EventArgs e)
       //{

       //}
    }
}
