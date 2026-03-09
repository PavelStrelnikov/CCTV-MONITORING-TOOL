using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;

namespace SDKDeviceTree
{
    public partial class CtrlDeviceTree : UserControl
    {
        private CtrlDeviceTree()
        {
            InitializeComponent();
            for (int i = 0; i < g_struDeviceInfo.Length; i++)
            {
                if (g_struDeviceInfo[i].bInit)
                {
                    break;
                }
                g_struDeviceInfo[i].Init();
                g_struDeviceInfo[i].pStruChanInfo = new CHCNetSDK.STRU_CHANNEL_INFO[CHCNetSDK.MAX_CHANNUM_V40];
                g_struDeviceInfo[i].struZeroChan = new CHCNetSDK.STRU_CHANNEL_INFO[16];
                g_struDeviceInfo[i].struMirrorChan = new CHCNetSDK.STRU_CHANNEL_INFO[16];
                for (int j = 0; j < CHCNetSDK.MAX_CHANNUM_V40; j++)
                {
                    g_struDeviceInfo[i].pStruChanInfo[j].init();
                }
                for (int j = 0; j < 16; j++)
                {
                    g_struDeviceInfo[i].struZeroChan[j].init();
                    g_struDeviceInfo[i].struMirrorChan[j].init();
                }
                g_struDeviceInfo[i].struPassiveDecode = new CHCNetSDK.PASSIVEDECODE_CHANINFO[256];
                for (int j = 0; j < 256; j++)
                {
                    g_struDeviceInfo[i].struPassiveDecode[j].init();
                }
            }
        }

        public CHCNetSDK.STRU_DEVICE_INFO[] g_struDeviceInfo = new CHCNetSDK.STRU_DEVICE_INFO[CHCNetSDK.MAX_DEVICES];
        public bool g_bTreenodeChange = false;
        private int m_iCurDeviceIndex = -1;
        private int m_iCurChanIndex = -1;
        private long g_lVoiceHandle = -1;	//voice talk index
        private static CtrlDeviceTree g_DeviceTree = new CtrlDeviceTree();

        private int iSelectedDeviceIndex
        {
            get
            {
                return m_iCurDeviceIndex;
            }
            set
            {
                if (m_iCurDeviceIndex != value)
                {
                    m_iCurDeviceIndex = value;
                    if (SelectedNodeChanged != null)
                    {
                        SelectedNodeChanged();
                    }
                }
            }
        }

        private int iSelectedChannelIndex
        {
            get
            {
                return m_iCurChanIndex;
            }
            set
            {
                if (m_iCurChanIndex != value)
                {
                    m_iCurChanIndex = value;
                    if (SelectedNodeChanged != null)
                    {
                        SelectedNodeChanged();
                    }
                }
            }
        }

        public delegate void SelectedNodeChangedHandler();
        public event SelectedNodeChangedHandler SelectedNodeChanged;

        public static CtrlDeviceTree Instance()
        {
            return g_DeviceTree;
        }
        public int GetCurlChannel()
        {
            if (iSelectedChannelIndex >= 0 && iSelectedChannelIndex < CHCNetSDK.MAX_CHANNUM_V40)
            {
                if (iSelectedDeviceIndex >= 0 || iSelectedDeviceIndex < CHCNetSDK.MAX_DEVICES)
                {
                    return g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[iSelectedChannelIndex].iChannelNO;
                }

            }
            return -1;
        }
        public String GetCurDeviceIp()
        {
            if (iSelectedDeviceIndex < 0 || iSelectedDeviceIndex > CHCNetSDK.MAX_DEVICES)
            {
                return null;
            }
            return g_struDeviceInfo[iSelectedDeviceIndex].chDeviceIP;
        }
        public long GetCurLoginID()
        {
            if (iSelectedDeviceIndex < 0 || iSelectedDeviceIndex > CHCNetSDK.MAX_DEVICES)
            {
                return -1;
            }
            return g_struDeviceInfo[iSelectedDeviceIndex].lLoginID;
        }

        public String GetCurLocalNodeName()
        {
            if (iSelectedDeviceIndex < 0 || iSelectedDeviceIndex > CHCNetSDK.MAX_DEVICES)
            {
                return null;
            }
            return g_struDeviceInfo[iSelectedDeviceIndex].chLocalNodeName;
        }

        public int GetCurDeviceIndex()
        {
            if (iSelectedDeviceIndex < 0 || iSelectedDeviceIndex > CHCNetSDK.MAX_DEVICES)
            {
                return 0;
            }

            return iSelectedDeviceIndex;
        }

        public int GetCurChanIndex()
        {
            if (iSelectedDeviceIndex <= 0 || iSelectedDeviceIndex > CHCNetSDK.MAX_DEVICES)
            {
                return 0;
            }
            return iSelectedChannelIndex;
        }
        public int GetCurChanNo()
        {
            if (iSelectedChannelIndex < 0 || iSelectedChannelIndex > CHCNetSDK.MAX_CHANNUM_V40)
            {
                if (iSelectedDeviceIndex < 0 || iSelectedDeviceIndex > CHCNetSDK.MAX_DEVICES)
                {
                    return -1;
                }
                if (g_struDeviceInfo[iSelectedDeviceIndex].lLoginID >= 0)
                {
                    return 1;
                }
                return -1;
            }
            return g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[iSelectedChannelIndex].iChannelNO;
        }

        public String GetCurChanName()
        {
            if (iSelectedChannelIndex >= 0 && iSelectedChannelIndex < CHCNetSDK.MAX_CHANNUM_V40)
            {
                if (iSelectedDeviceIndex >= 0 || iSelectedDeviceIndex < CHCNetSDK.MAX_DEVICES)
                {
                    return g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[iSelectedChannelIndex].chChanName;
                }

            }
            return null;
        }

        public int GetCurPreChanNo()
        {
            if (iSelectedChannelIndex < 0 || iSelectedChannelIndex > CHCNetSDK.MAX_CHANNUM_V40)
            {
                if (iSelectedDeviceIndex < 0 || iSelectedDeviceIndex > CHCNetSDK.MAX_DEVICES)
                {
                    return -1;
                }
                return -1;
            }
            return g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[iSelectedChannelIndex].iChannelNO;
        }

        public bool SetPreHandle(int deviceIndex, int channelIndex, int previewHandle)
        {
            if (deviceIndex < 0 || deviceIndex > CHCNetSDK.MAX_CHANNUM_V40)
            {
                if (channelIndex < 0 || channelIndex > CHCNetSDK.MAX_DEVICES)
                {
                    return false;
                }
                return false;
            }
            g_struDeviceInfo[deviceIndex].pStruChanInfo[channelIndex].lRealHandle = previewHandle;
            return true;
        }

        public int SetCurRealHandle(Int32 lRealHandle)
        {
            if (iSelectedChannelIndex >= 0 && iSelectedChannelIndex < CHCNetSDK.MAX_CHANNUM_V40)
            {
                if (iSelectedDeviceIndex >= 0 || iSelectedDeviceIndex < CHCNetSDK.MAX_DEVICES)
                {
                    g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[iSelectedChannelIndex].lRealHandle = lRealHandle;
                    return lRealHandle;
                }
                
            }
            return -1;
        }
        public long GetCurRealHandle()
        {
            if (iSelectedChannelIndex >= 0 && iSelectedChannelIndex < CHCNetSDK.MAX_CHANNUM_V40)
            {
                if (iSelectedDeviceIndex >= 0 || iSelectedDeviceIndex < CHCNetSDK.MAX_DEVICES)
                {
                    return g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[iSelectedChannelIndex].lRealHandle;
                }

            }
            return -1;
        }

        public CHCNetSDK.STRU_DEVICE_INFO SelectedDeviceInfo
        {
            get
            {
                if (iSelectedDeviceIndex < 0 || iSelectedDeviceIndex > CHCNetSDK.MAX_DEVICES)
                {
                    return g_struDeviceInfo[0];
                }
                return g_struDeviceInfo[iSelectedDeviceIndex];
            }
        }

        public CHCNetSDK.STRU_DEVICE_INFO GetCurDeviceInfobyIndex(int iDeviceIndex)
        {
            if (iDeviceIndex < 0 || iDeviceIndex > CHCNetSDK.MAX_DEVICES)
            {
                return g_struDeviceInfo[0];
            }
            return g_struDeviceInfo[iDeviceIndex];
        }

        public CHCNetSDK.STRU_CHANNEL_INFO GetCurChanInfo()
        {
            if (iSelectedChannelIndex < 0 || iSelectedChannelIndex > CHCNetSDK.MAX_CHANNUM_V40)
            {
                if (iSelectedDeviceIndex < 0 || iSelectedDeviceIndex > CHCNetSDK.MAX_DEVICES)
                {
                    return g_struDeviceInfo[0].pStruChanInfo[0];
                }
                return g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[0];
            }
            return g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[iSelectedChannelIndex];
        }

        public CHCNetSDK.STRU_CHANNEL_INFO GetCurChanInfoByIndex(int iChanIndex)
        {
            if (iChanIndex < 0 || iChanIndex > CHCNetSDK.MAX_CHANNUM_V40)
            {
                if (iSelectedDeviceIndex < 0 || iSelectedDeviceIndex > CHCNetSDK.MAX_DEVICES)
                {
                    return g_struDeviceInfo[0].pStruChanInfo[0];
                }
                return g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[0];
            }
            return g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[iChanIndex];
        }

        public CHCNetSDK.STRU_CHANNEL_INFO GetCurChanInfoByIndex(int iDeviceIndex, int iChanIndex)
        {
            if (iChanIndex < 0 || iChanIndex > CHCNetSDK.MAX_CHANNUM_V40)
            {
                if (iDeviceIndex < 0 || iDeviceIndex > CHCNetSDK.MAX_DEVICES)
                {
                    return g_struDeviceInfo[0].pStruChanInfo[0];
                }
                return g_struDeviceInfo[iDeviceIndex].pStruChanInfo[0];
            }
            return g_struDeviceInfo[iDeviceIndex].pStruChanInfo[iChanIndex];
        }

        private void treeViewDevice_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeViewDevice.SelectedNode = e.Node;
            if (e.Button == MouseButtons.Left)
            {
                if (1 == treeViewDevice.SelectedNode.Level)
                {
                    iSelectedDeviceIndex = int.Parse(e.Node.Name) % 1000;
                    iSelectedChannelIndex = -1;
                }
                else if (2 == treeViewDevice.SelectedNode.Level)
                {
                    iSelectedChannelIndex = int.Parse(e.Node.Name) % 1000;
                    iSelectedDeviceIndex = int.Parse(e.Node.Parent.Name) % 1000;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (0 == treeViewDevice.SelectedNode.Level)
                {
                    int i = 0;
                    for (i = 0; i < CHCNetSDK.MAX_DEVICES; i++)
                    {
                        if (-1 == g_struDeviceInfo[i].iDeviceChanNum)
                        {
                            LoginForm addDevice = new LoginForm();
                            addDevice.m_iDeviceIndex = i;
                            //iSelectedDeviceIndex = i;
                            if (addDevice.ShowDialog() == DialogResult.OK)
                            {
                                //e.Node.Nodes.Add();
                                TreeNode deviceNode = new TreeNode();
                                deviceNode.Text = g_struDeviceInfo[i].chLocalNodeName;
                                deviceNode.Name = (CHCNetSDK.DEVICETYPE * 1000 + g_struDeviceInfo[i].iDeviceIndex).ToString();
                                deviceNode.ImageIndex = 6;
                                deviceNode.SelectedImageIndex = 6;

                                for (int j = 0; j < CHCNetSDK.MAX_CHANNUM_V40; j++ )
                                {
                                    if (g_struDeviceInfo[i].pStruChanInfo[j].iChanIndex != -1)
                                    {
                                        TreeNode chanNode = new TreeNode();
                                        chanNode.Text = g_struDeviceInfo[i].pStruChanInfo[j].chChanName;
                                        chanNode.Name = (CHCNetSDK.CHANNELTYPE * 1000 + g_struDeviceInfo[i].pStruChanInfo[j].iChanIndex).ToString();

                                        if (g_struDeviceInfo[i].pStruChanInfo[j].bEnable)
                                        {
                                            chanNode.ImageIndex = 1;
                                            chanNode.SelectedImageIndex = 1;
                                        }
                                        else
                                        {
                                            chanNode.ImageIndex = 5;
                                            chanNode.SelectedImageIndex = 5;
                                        }

                                        deviceNode.Nodes.Add(chanNode);
                                    }
                                }
                                //zero
                                for (int j = 0; j < g_struDeviceInfo[i].byZeroChanNum; j++ )
                                {
                                    TreeNode chanNode = new TreeNode();
                                    chanNode.Text = g_struDeviceInfo[i].struZeroChan[j].chChanName;
                                    chanNode.Name = (CHCNetSDK.CHANNELTYPE * 1000 + g_struDeviceInfo[i].struZeroChan[j].iChanIndex).ToString();

                                    if (g_struDeviceInfo[i].struZeroChan[j].bEnable)
                                    {
                                        chanNode.ImageIndex = 1;
                                        chanNode.SelectedImageIndex = 1;
                                    }
                                    else
                                    {
                                        chanNode.ImageIndex = 5;
                                        chanNode.SelectedImageIndex = 5;
                                    }

                                    deviceNode.Nodes.Add(chanNode);
                                }
                                for (int j = 0; j < g_struDeviceInfo[i].byMirrorChanNum && j < 16; j++ )
                                {
                                    TreeNode chanNode = new TreeNode();
                                    chanNode.Text = g_struDeviceInfo[i].struMirrorChan[j].chChanName;
                                    chanNode.Name = (CHCNetSDK.CHANNELTYPE * 1000 + g_struDeviceInfo[i].struMirrorChan[j].iChanIndex).ToString();

                                    if (g_struDeviceInfo[i].struMirrorChan[j].bEnable)
                                    {
                                        chanNode.ImageIndex = 1;
                                        chanNode.SelectedImageIndex = 1;
                                    }
                                    else
                                    {
                                        chanNode.ImageIndex = 5;
                                        chanNode.SelectedImageIndex = 5;
                                    }

                                    deviceNode.Nodes.Add(chanNode);
                                }

                                treeViewDevice.SelectedNode.Nodes.Add(deviceNode);

                                //iSelectedDeviceIndex = addDevice.m_iDeviceIndex;
                                treeViewDevice.SelectedNode.ExpandAll();
                            }
                            return;
                        }
                    }
                    if (i == CHCNetSDK.MAX_DEVICES)
                    {
                        MessageBox.Show("Exceeds the maximum number of Devices");
                    }
                }
                else if (1 == treeViewDevice.SelectedNode.Level)
                {
                    iSelectedDeviceIndex = int.Parse(e.Node.Name) % 1000;
                    iSelectedChannelIndex = -1;
                    e.Node.ContextMenuStrip = contextMenuStripDevice;
                }
                else if (2 == treeViewDevice.SelectedNode.Level)
                {
                    iSelectedChannelIndex = int.Parse(e.Node.Name) % 1000;
                    iSelectedDeviceIndex = int.Parse(e.Node.Parent.Name) % 1000;
                    e.Node.ContextMenuStrip = contextMenuStripChan;
                }
            }
        }

        private void treeViewDevice_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (treeViewDevice.SelectedNode.Level == 1)//login and logout 
            {
                iSelectedDeviceIndex = int.Parse(treeViewDevice.SelectedNode.Name) % 1000;
                iSelectedChannelIndex = -1;
                if (g_struDeviceInfo[iSelectedDeviceIndex].lLoginID < 0)
                {
                    Login();
                }
                else
                {
                    LoginOut();
                }
            }
            else if (2 == treeViewDevice.SelectedNode.Level)//the preview and stop the preview 
            {
                iSelectedChannelIndex = int.Parse(e.Node.Name) % 1000;
                iSelectedDeviceIndex = int.Parse(e.Node.Parent.Name) % 1000;
            }
        }

        private void ToolStripMenuItemLog_Click(object sender, EventArgs e)
        {
            Login();
        }

        private void ToolStripMenuItemLogOff_Click(object sender, EventArgs e)
        {
            LoginOut();
        }



        private void Login()
        {
            LoginForm addDevice = new LoginForm();
            addDevice.m_iDeviceIndex = iSelectedDeviceIndex;
            if (!addDevice.SDK_Login(false))
            {
                MessageBox.Show("login failed!");
                return;
            }
            //device
            treeViewDevice.SelectedNode.SelectedImageIndex = 6;
            treeViewDevice.SelectedNode.ImageIndex = 6;
            treeViewDevice.SelectedNode.Nodes.Clear();
            //channnel
            for (int j = 0; j < CHCNetSDK.MAX_CHANNUM_V40; j++)
            {
                if (g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[j].iChanIndex != -1)
                {
                    TreeNode chanNode = new TreeNode();
                    chanNode.Text = g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[j].chChanName;
                    chanNode.Name = (CHCNetSDK.CHANNELTYPE * 1000 + g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[j].iChanIndex).ToString();

                    if (g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[j].bEnable)
                    {
                        chanNode.ImageIndex = 1;
                        chanNode.SelectedImageIndex = 1;
                    }
                    else
                    {
                        chanNode.ImageIndex = 5;
                        chanNode.SelectedImageIndex = 5;
                    }

                    treeViewDevice.SelectedNode.Nodes.Add(chanNode);
                }
            }
            //zero
            for (int j = 0; j < g_struDeviceInfo[iSelectedDeviceIndex].byZeroChanNum; j++)
            {
                TreeNode chanNode = new TreeNode();
                chanNode.Text = g_struDeviceInfo[iSelectedDeviceIndex].struZeroChan[j].chChanName;
                chanNode.Name = (CHCNetSDK.CHANNELTYPE * 1000 + g_struDeviceInfo[iSelectedDeviceIndex].struZeroChan[j].iChanIndex).ToString();

                if (g_struDeviceInfo[iSelectedDeviceIndex].struZeroChan[j].bEnable)
                {
                    chanNode.ImageIndex = 1;
                    chanNode.SelectedImageIndex = 1;
                }
                else
                {
                    chanNode.ImageIndex = 5;
                    chanNode.SelectedImageIndex = 5;
                }

                treeViewDevice.SelectedNode.Nodes.Add(chanNode);
            }
            for (int j = 0; j < g_struDeviceInfo[iSelectedDeviceIndex].byMirrorChanNum && j < 16; j++)
            {
                TreeNode chanNode = new TreeNode();
                chanNode.Text = g_struDeviceInfo[iSelectedDeviceIndex].struMirrorChan[j].chChanName;
                chanNode.Name = (CHCNetSDK.CHANNELTYPE * 1000 + g_struDeviceInfo[iSelectedDeviceIndex].struMirrorChan[j].iChanIndex).ToString();

                if (g_struDeviceInfo[iSelectedDeviceIndex].struMirrorChan[j].bEnable)
                {
                    chanNode.ImageIndex = 1;
                    chanNode.SelectedImageIndex = 1;
                }
                else
                {
                    chanNode.ImageIndex = 5;
                    chanNode.SelectedImageIndex = 5;
                }

                treeViewDevice.SelectedNode.Nodes.Add(chanNode);
            }
            treeViewDevice.SelectedNode.ExpandAll();
        }
        private bool LoginOut()
        {
            for (int i = 0; i < g_struDeviceInfo[iSelectedDeviceIndex].iDeviceChanNum; i++)
            {
                if ((g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[i].lRealHandle >= 0 && i < CHCNetSDK.MAX_CHANNUM_V30 * 2) || g_struDeviceInfo[m_iCurDeviceIndex].pStruChanInfo[i].bLocalManualRec)
                {
                    MessageBox.Show("Some channels of this device is recording or preview");
                    return false;
                }
            }

            for (int i = 0; i < 256; i++ )
            {
                if (g_struDeviceInfo[iSelectedDeviceIndex].struPassiveDecode[i].lPassiveHandle >= 0 || g_struDeviceInfo[iSelectedDeviceIndex].struPassiveDecode[i].hFileThread != IntPtr.Zero)
                {
                    MessageBox.Show("some channels of this device is passive decode");
                    return false;
                }
            }

            if (g_lVoiceHandle >= 0)
            {
                MessageBox.Show("Some channels of this device is voice talk");
                return false;
            }
            if (g_struDeviceInfo[iSelectedDeviceIndex].lLoginID >= 0)
            {
                //remove guard
                if (g_struDeviceInfo[iSelectedDeviceIndex].lFortifyHandle >= 0)
                {
                    if (LoginForm.SDK_CloseAlarmChan((int)g_struDeviceInfo[iSelectedDeviceIndex].lFortifyHandle))
                    {
                        g_struDeviceInfo[iSelectedDeviceIndex].lFortifyHandle = -1;
                    }
                }

                if (!LoginForm.SDK_Logout((int)g_struDeviceInfo[iSelectedDeviceIndex].lLoginID))
                {
                    MessageBox.Show("Logout Failed");
                    return false;
                }
            }

            g_struDeviceInfo[iSelectedDeviceIndex].lLoginID = -1;
            g_struDeviceInfo[iSelectedDeviceIndex].bPlayDevice = false;

            treeViewDevice.SelectedNode.ImageIndex = 7;
            treeViewDevice.SelectedNode.SelectedImageIndex = 7;

            for (int i = 0; i < g_struDeviceInfo[iSelectedDeviceIndex].iDeviceChanNum; i++)
            {
                g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[i].lRealHandle = -1;
                g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[i].bAlarm = false;
                g_struDeviceInfo[iSelectedDeviceIndex].pStruChanInfo[i].bLocalManualRec = false;
            }
            treeViewDevice.SelectedNode.Collapse();

            return true;
        }

        private void treeViewDevice_Leave(object sender, EventArgs e)
        {
            if (treeViewDevice.SelectedNode != null)
            {
                //let selected back view appear blue 
                treeViewDevice.SelectedNode.BackColor = Color.DodgerBlue;
                //the foreground color to white 
                treeViewDevice.SelectedNode.ForeColor = Color.White;
            }

        }

        private void treeViewDevice_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (treeViewDevice.SelectedNode != null)
            {
                //return a selected node on the background color (no color) original 
                treeViewDevice.SelectedNode.BackColor = Color.Empty;
                //restore the foreground 
                treeViewDevice.SelectedNode.ForeColor = Color.Black;
            }

        }
    }
}
