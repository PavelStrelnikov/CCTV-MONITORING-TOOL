using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using Common;

namespace SDKDeviceTree
{
    public partial class LoginForm
    {

        private bool LoginCallBackFlag = false;
        CHCNetSDK.NET_DVR_DEVICEINFO_V30 m_struDeviceInfo;
        private uint m_AysnLoginResult = 0;
        private int m_iUserID = -1;
        private bool AysnLoginFlag = false;
        private CtrlDeviceTree g_deviceTree = CtrlDeviceTree.Instance();
        public int m_iDeviceIndex = -1;
        private bool m_bInitSDK = false;

        public void SDK_Init()
        {
            m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (!m_bInitSDK)
            {
                MessageBox.Show("NET_DVR_Init error!");
                return;
            }
            else
            {
                CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);
            }
        }


        //异步登陆，不常用
        public void AsynLoginMsgCallback(Int32 lUserID, UInt32 dwResult, IntPtr lpDeviceInfo, IntPtr pUser)
        {

            if (dwResult == 1)
            {

                m_struDeviceInfo = (CHCNetSDK.NET_DVR_DEVICEINFO_V30)Marshal.PtrToStructure(lpDeviceInfo, typeof(CHCNetSDK.NET_DVR_DEVICEINFO_V30));

            }

            m_AysnLoginResult = dwResult;
            m_iUserID = lUserID;
            LoginCallBackFlag = true;
        }

        public bool SDK_Login(bool bStatus)//bStatus true-添加新设备；false - 离线设备上线
        {
            LoginCallBackFlag = false;
            m_struDeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();

            CHCNetSDK.NET_DVR_DEVICEINFO_V30 struDeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
            struDeviceInfo.sSerialNumber = new byte[CHCNetSDK.SERIALNO_LEN];

            CHCNetSDK.NET_DVR_NETCFG_V30 struNetCfg = new CHCNetSDK.NET_DVR_NETCFG_V30();
            struNetCfg.init();
            CHCNetSDK.NET_DVR_DEVICECFG_V40 struDevCfg = new CHCNetSDK.NET_DVR_DEVICECFG_V40();
            struDevCfg.sDVRName = new byte[CHCNetSDK.NAME_LEN];
            struDevCfg.sSerialNumber = new byte[CHCNetSDK.SERIALNO_LEN];
            struDevCfg.byDevTypeName = new byte[CHCNetSDK.DEV_TYPE_NAME_LEN];
            CHCNetSDK.NET_DVR_USER_LOGIN_INFO struLoginInfo = new CHCNetSDK.NET_DVR_USER_LOGIN_INFO();
            CHCNetSDK.NET_DVR_DEVICEINFO_V40 struDeviceInfoV40 = new CHCNetSDK.NET_DVR_DEVICEINFO_V40();
            struDeviceInfoV40.struDeviceV30.sSerialNumber = new byte[CHCNetSDK.SERIALNO_LEN];
            uint dwReturned = 0;
            int lUserID = -1;
            int iIPCChanGroups = -1;//Dynamic IP Channels Groups

            //异步登陆控制接口，不常用
            struLoginInfo.bUseAsynLogin = AysnLoginFlag;
            struLoginInfo.cbLoginResult = new CHCNetSDK.LOGINRESULTCALLBACK(AsynLoginMsgCallback);

            if (bStatus)
            {
                //从登陆配置界面读取参数（新添加设备）
                struLoginInfo.sDeviceAddress = textBoxDeviceAddress.Text;
                struLoginInfo.sUserName = textBoxUserName.Text;
                struLoginInfo.sPassword = textBoxPassword.Text;
                struLoginInfo.wPort = ushort.Parse(textBoxPort.Text);
                struLoginInfo.byHttps = (byte)(m_checkBoxTLS.Checked ? 1 : 0);
            }
            else
            {
                //从设备数中进行读取离线设备的登陆参数（离线设备上线）
                struLoginInfo.sDeviceAddress = g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].chDeviceIP;
                struLoginInfo.sUserName = g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].chLoginUserName;
                struLoginInfo.sPassword = g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].chLoginPwd;
                struLoginInfo.wPort = (ushort)g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].lDevicePort;
                struLoginInfo.byHttps = g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].byLoginMode;
            }

            lUserID = CHCNetSDK.NET_DVR_Login_V40(ref struLoginInfo, ref struDeviceInfoV40);
            //异步登陆返回处理，不常用
            if (struLoginInfo.bUseAsynLogin)
            {
                for (int i = 0; i < 1000; i++)
                {
                    if (!LoginCallBackFlag)
                    {
                        Thread.Sleep(5);
                    }
                    else
                    {
                        break;
                    }
                }
                if (!LoginCallBackFlag)
                {
                    MessageBox.Show("Asynchronous login callback time out!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                if (m_AysnLoginResult == 1)
                {
                    lUserID = m_iUserID;
                    struDeviceInfoV40.struDeviceV30 = m_struDeviceInfo;
                }
                else
                {
                    MessageBox.Show("Asynchronous login failed!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

            }

            if (lUserID < 0)
            {
                uint nErr = CHCNetSDK.NET_DVR_GetLastError();
                string strTemp = string.Format("NET_DVR_Login_V40 [{0}]", textBoxDeviceAddress.Text);
                CHCNetSDK.AddLog(-1, CHCNetSDK.OPERATION_FAIL_T, strTemp);
                if (nErr == CHCNetSDK.NET_DVR_PASSWORD_ERROR)
                {
                    MessageBox.Show("user name or password error!");
                    if (1 == struDeviceInfoV40.bySupportLock)
                    {
                        string strTemp1 = string.Format("Left {0} try opportunity", struDeviceInfoV40.byRetryLoginTime);
                        MessageBox.Show(strTemp1);
                    }
                }
                else if (nErr == CHCNetSDK.NET_DVR_USER_LOCKED)
                {
                    if (1 == struDeviceInfoV40.bySupportLock)
                    {
                        string strTemp1 = string.Format("IP is locked, the remaining lock time is {0}", struDeviceInfoV40.dwSurplusLockTime);
                        MessageBox.Show(strTemp1);
                    }
                }
                else
                {
                    MessageBox.Show("net error or dvr is busy!");
                }
                return false;
            }
            else
            {
                if (1 == struDeviceInfoV40.byPasswordLevel)
                {
                    MessageBox.Show("default password, please change the password");
                }
                else if (3 == struDeviceInfoV40.byPasswordLevel)
                {
                    MessageBox.Show("risk password, please change the password");
                }
                struDeviceInfo = struDeviceInfoV40.struDeviceV30;
            }

            if (bStatus)
            {
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].chLocalNodeName = textBoxDeviceAddress.Text;
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].chLoginPwd = textBoxPassword.Text;
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].chDeviceIP = textBoxDeviceAddress.Text;
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].chLoginUserName = textBoxUserName.Text;
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].lDevicePort = int.Parse(textBoxPort.Text);
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].byLoginMode = (byte)(m_checkBoxTLS.Checked ? 1 : 0);
            }

            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].byCharaterEncodeType = struDeviceInfoV40.byCharEncodeType;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].lLoginID = lUserID;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].chSerialNumber = System.Text.Encoding.UTF8.GetString(struDeviceInfo.sSerialNumber).TrimEnd('\0');
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iDeviceIndex = m_iDeviceIndex;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iDeviceType = (int)struDeviceInfo.wDevType;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iDeviceChanNum = (int)(struDeviceInfo.byChanNum + struDeviceInfo.byIPChanNum + struDeviceInfo.byHighDChanNum * 256);
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iStartChan = (int)struDeviceInfo.byStartChan;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iDiskNum = (int)struDeviceInfo.byDiskNum;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iAlarmInNum = (int)struDeviceInfo.byAlarmInPortNum;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iAlarmOutNum = (int)struDeviceInfo.byAlarmOutPortNum;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iAudioNum = (int)struDeviceInfo.byAlarmOutPortNum;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iAnalogChanNum = (int)struDeviceInfo.byChanNum;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iIPChanNum = (int)(struDeviceInfo.byIPChanNum + struDeviceInfo.byHighDChanNum * 256);
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].byZeroChanNum = struDeviceInfo.byZeroChanNum;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].byStartDTalkChan = struDeviceInfo.byStartDTalkChan;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].byLanguageType = struDeviceInfo.byLanguageType;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].byMirrorChanNum = struDeviceInfo.byMirrorChanNum;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].wStartMirrorChanNo = struDeviceInfo.wStartMirrorChanNo;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].byAudioInputChanNum = struDeviceInfo.byVoiceInChanNum;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].byStartAudioInputChanNo = struDeviceInfo.byStartVoiceInChanNo;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iStartDChan = (int)struDeviceInfo.byStartDChan;

            //main stream protocol type 0-Private, 1-rtp/tcp, 2-rtp/rtsp（def）
            //sub stream protocol type 0-Private, 1-rtp/tcp, 2-rtp/rtsp（def）
            if (1 == (struDeviceInfo.bySupport & 0x80))
            {
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].byMainProto = (byte)(struDeviceInfo.byMainProto + 2);
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].bySubProto = (byte)(struDeviceInfo.bySubProto + 2);
            }
            else
            {
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].byMainProto = struDeviceInfo.byMainProto;
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].bySubProto = struDeviceInfo.bySubProto;
            }

            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].bySupport1 = struDeviceInfo.bySupport1;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].bySupport2 = struDeviceInfo.bySupport2;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].bySupport7 = struDeviceInfo.bySupport7;
            g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].byLanguageType = struDeviceInfo.byLanguageType;

            /*
             * 获取设备信息（NET_DVR_GET_DEVICECFG_V40）
             */
            uint dwSize2 = (uint)Marshal.SizeOf(struDevCfg);
            IntPtr ptrDevCfg = Marshal.AllocHGlobal((int)dwSize2);
            Marshal.StructureToPtr(struDevCfg, ptrDevCfg, false);

            if (!CHCNetSDK.NET_DVR_GetDVRConfig(lUserID, CHCNetSDK.NET_DVR_GET_DEVICECFG_V40, 0, ptrDevCfg, dwSize2, ref dwReturned))
            {
                CHCNetSDK.AddLog(lUserID, CHCNetSDK.OPERATION_FAIL_T, "NET_DVR_GET_DEVICECFG_V40");
            }
            else
            {
                struDevCfg = (CHCNetSDK.NET_DVR_DEVICECFG_V40)Marshal.PtrToStructure(ptrDevCfg, typeof(CHCNetSDK.NET_DVR_DEVICECFG_V40));
                if (g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iDeviceType != (int)struDevCfg.wDevType)
                {
                    string strShow = null;
                    strShow = "returned device type is different between login and get device config" + g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iDeviceType.ToString() + struDevCfg.wDevType.ToString();
                    MessageBox.Show(strShow);
                }
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].chDeviceName = System.Text.Encoding.UTF8.GetString(struDevCfg.byDevTypeName).Trim('\0');
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].dwDevSoftVer = struDevCfg.dwSoftwareVersion;
                //g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].chDeviceMultiIP = struNetCfg.struMulticastIpAddr.sIpV4.ToString();
            }
            Marshal.FreeHGlobal(ptrDevCfg);

            if (g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iIPChanNum >= 0)
            {
                /*
                 *iIPChanNum = 0的这种情况存在于XVR设备中，此款设备IP通道和模拟通道是可以动态进行变化的；
                 *但是在NET_DVR_IPPARACFG_V40接口结构体中可以告知，设备支持的最大模拟通道和数字通道的通道数信息；
                 */
                if (g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iIPChanNum == 0)
                {
                    iIPCChanGroups = 1;
                    g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].pStruIPParaCfgV40 = new CHCNetSDK.NET_DVR_IPPARACFG_V40[iIPCChanGroups];                  
                }
                else
                {
                    /*
                     * CHCNetSDK.MAX_CHANNUM_V30 == 64；
                     * 在结构体NET_DVR_IPPARACFG_V40获取的时候，一组最大是64，所以使用64作为被除数，通过计划IP通道和64的余数，来计算需要升级存在动态IP通道的内存空间；
                     */
                    iIPCChanGroups = g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iIPChanNum / CHCNetSDK.MAX_CHANNUM_V30;
                    if (g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iIPChanNum % CHCNetSDK.MAX_CHANNUM_V30 != 0)
                    {
                        iIPCChanGroups = iIPCChanGroups + 1;                      
                    }
                    g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].pStruIPParaCfgV40 = new CHCNetSDK.NET_DVR_IPPARACFG_V40[iIPCChanGroups];
                }
            }

            DoGetDeviceResoureCfg(m_iDeviceIndex, iIPCChanGroups);
            return true;
        }

        public bool DoGetDeviceResoureCfg(int iDeviceIndex, int iGroupNO)
        {
            int i = 0, j = 0;
            uint dwReturned = 0;
            int dwSize = 0;

            for (j = 0; j < iGroupNO; j++)
            {
                CHCNetSDK.NET_DVR_IPPARACFG_V40 struIPAccessCfgV40 = new CHCNetSDK.NET_DVR_IPPARACFG_V40();
                //iGroupNO = j;
                dwSize = Marshal.SizeOf(struIPAccessCfgV40);
                IntPtr ptrIPAccessCfgV40 = Marshal.AllocHGlobal(dwSize);
                Marshal.StructureToPtr(struIPAccessCfgV40, ptrIPAccessCfgV40, false);
                g_deviceTree.g_struDeviceInfo[iDeviceIndex].bIPRet =
                    CHCNetSDK.NET_DVR_GetDVRConfig(g_deviceTree.g_struDeviceInfo[iDeviceIndex].lLoginID, CHCNetSDK.NET_DVR_GET_IPPARACFG_V40, j, ptrIPAccessCfgV40, (uint)dwSize, ref dwReturned);
                if (!g_deviceTree.g_struDeviceInfo[iDeviceIndex].bIPRet)
                {	///device no support ip access
                    ///
                    uint iErrCode = CHCNetSDK.NET_DVR_GetLastError();
                    //g_deviceTree.g_struDeviceInfo[iDeviceIndex].lFirstEnableChanIndex = 0;
                    //CHCNetSDK.AddLog(iDeviceIndex, CHCNetSDK.OPERATION_FAIL_T, "NET_DVR_GET_IPPARACFG_V40");
                    i = j + j * CHCNetSDK.MAX_CHANNUM_V30; //计算NET_DVR_GET_IPPARACFG_V40对应结构体的数组下标索引值
                    if (i < g_deviceTree.g_struDeviceInfo[iDeviceIndex].iAnalogChanNum)
                    {
                        g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i].iDeviceIndex = iDeviceIndex;
                        g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i].iChanIndex = i;
                        g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i].iChannelNO = i + g_deviceTree.g_struDeviceInfo[iDeviceIndex].iStartChan;
                        g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i].bEnable = true;
                        g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i].iChanType = CHCNetSDK.DEMO_CHANNEL_TYPE.DEMO_CHANNEL_TYPE_ANALOG;
                        g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].pStruChanInfo[i].chChanName = string.Format("Camera{0}", i + g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iStartChan); ;
                    }
                    else//clear the state of other channel
                    {
                        g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i].iDeviceIndex = -1;
                        g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i].iChanIndex = -1;
                        g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i].bEnable = false;
                        g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i].chChanName = "";
                    }
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].iGroupNO = -1;
                }
                else
                {
                    struIPAccessCfgV40 = (CHCNetSDK.NET_DVR_IPPARACFG_V40)Marshal.PtrToStructure(ptrIPAccessCfgV40, typeof(CHCNetSDK.NET_DVR_IPPARACFG_V40));
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruIPParaCfgV40[j] = struIPAccessCfgV40;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].iGroupNO = j;
                    RefreshIPDevLocalCfg(iDeviceIndex);
                }
                
                Marshal.FreeHGlobal(ptrIPAccessCfgV40);
            }

            for (i = 0; i < g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iAnalogChanNum; i++)
            {
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].pStruChanInfo[i].iDeviceIndex = m_iDeviceIndex;
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].pStruChanInfo[i].iChanIndex = i;
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].pStruChanInfo[i].iChannelNO = i + g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iStartChan;
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].pStruChanInfo[i].chChanName = string.Format("Camera{0}", i + g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iStartChan);

                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].pStruChanInfo[i].bEnable = true;
                g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i].iChanType = CHCNetSDK.DEMO_CHANNEL_TYPE.DEMO_CHANNEL_TYPE_ANALOG;
                g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].pStruChanInfo[i].dwImageType = CHCNetSDK.CHAN_ORIGINAL;

            }

            if ((g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].byMirrorChanNum > 0) &&
                (g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].wStartMirrorChanNo > (g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].iDeviceChanNum - 1)))
            {
                for (i = 0; i < g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].byMirrorChanNum && i < 16; i++)
                {
                    g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].struMirrorChan[i].iDeviceIndex = m_iDeviceIndex;
                    g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].struMirrorChan[i].iChanIndex = i + CHCNetSDK.MIRROR_CHAN_INDEX;
                    g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].struMirrorChan[i].iChannelNO = i + g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].wStartMirrorChanNo;
                    g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].struMirrorChan[i].chChanName = string.Format("MirrorChan{0}", i + 1);

                    //analog devices
                    g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].struMirrorChan[i].bEnable = true;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i].iChanType = CHCNetSDK.DEMO_CHANNEL_TYPE.DEMO_CHANNEL_TYPE_IP;
                    g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].struMirrorChan[i].dwImageType = CHCNetSDK.CHAN_ORIGINAL;
                }
            }
            if (g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].byZeroChanNum > 0)
            {
                for (i = 0; i < g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].byZeroChanNum; i++)
                {
                    g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].struZeroChan[i].iDeviceIndex = m_iDeviceIndex;
                    g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].struZeroChan[i].iChanIndex = i + CHCNetSDK.ZERO_CHAN_INDEX;
                    g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].struZeroChan[i].chChanName = string.Format("ZeroChan{0}", i);

                    g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].struZeroChan[i].bEnable = true;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i].iChanType = CHCNetSDK.DEMO_CHANNEL_TYPE.DEMO_CHANNEL_TYPE_MIRROR;
                    g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].struZeroChan[i].dwImageType = CHCNetSDK.CHAN_ORIGINAL;

                }
            }
            return g_deviceTree.g_struDeviceInfo[iDeviceIndex].bIPRet;
        }

        public void RefreshIPDevLocalCfg(int iDeviceIndex)
        {
            CHCNetSDK.NET_DVR_IPPARACFG_V40 struIPAccessCfgV40 = g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruIPParaCfgV40[g_deviceTree.g_struDeviceInfo[iDeviceIndex].iGroupNO];
            uint dwChanShow = 0;
            int iIPChanIndex = 0;
            int i = 0;

            g_deviceTree.g_struDeviceInfo[iDeviceIndex].iIPChanNum = (int)struIPAccessCfgV40.dwDChanNum;
            int iIPChanNum = (int)struIPAccessCfgV40.dwDChanNum;

            int iAnalogChanCount = 0;
            int iIPChanCount = 0;
            int iGroupNO = g_deviceTree.g_struDeviceInfo[iDeviceIndex].iGroupNO;    //Group NO.
            int iGroupNum = (int)struIPAccessCfgV40.dwGroupNum;

            int iChanID = 0;
            /*
            CHCNetSDK.MAX_CHANNUM_V30 = 64
            */

            for (i = 0; i < CHCNetSDK.MAX_CHANNUM_V30; i++)
            {

                //analog channel
                if (iAnalogChanCount < g_deviceTree.g_struDeviceInfo[iDeviceIndex].iAnalogChanNum)
                {
                    dwChanShow = (uint)(iAnalogChanCount + g_deviceTree.g_struDeviceInfo[iDeviceIndex].iStartChan + iGroupNO * 64);

                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].iDeviceIndex = iDeviceIndex;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].iChanIndex = i;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].iChanType = CHCNetSDK.DEMO_CHANNEL_TYPE.DEMO_CHANNEL_TYPE_ANALOG;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].iChannelNO = (int)dwChanShow;

                    iChanID = i + g_deviceTree.g_struDeviceInfo[iDeviceIndex].iStartChan - g_deviceTree.g_struDeviceInfo[iDeviceIndex].iAnalogChanNum;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i].chChanName = string.Format("Camera{0}", iChanID);

                    //analog devices
                    if (struIPAccessCfgV40.byAnalogChanEnable[i] > 0)
                    {
                        g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].bEnable = true;
                        g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].dwImageType = CHCNetSDK.CHAN_ORIGINAL;
                        //g_struDeviceInfo[iDeviceIndex].iEnableChanNum ++;
                    }
                    else
                    {
                        g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].bEnable = false;
                        g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].dwImageType = CHCNetSDK.CHAN_OFF_LINE;
                    }

                    iAnalogChanCount++;
                }
                else if (iGroupNO >= 0 && ((iIPChanCount + iGroupNO * 64) < iIPChanNum))
                {
                    dwChanShow = (uint)(iIPChanCount + iGroupNO * 64 + struIPAccessCfgV40.dwStartDChan);

                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].iChanType = CHCNetSDK.DEMO_CHANNEL_TYPE.DEMO_CHANNEL_TYPE_IP;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].iChannelNO = (int)dwChanShow;

                    iIPChanIndex = iIPChanCount;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].iDeviceIndex = iDeviceIndex;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].iChanIndex = i + iGroupNO * 64;
                    g_deviceTree.g_struDeviceInfo[m_iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].chChanName =
                        string.Format("IPCamera{0}", iIPChanCount + iGroupNO * 64 + g_deviceTree.g_struDeviceInfo[iDeviceIndex].iStartDChan);

                    if (struIPAccessCfgV40.struIPDevInfo[iIPChanIndex].byEnable == 1)
                    {
                        g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].bEnable = true;//
                        if (struIPAccessCfgV40.struStreamMode[iIPChanIndex].uGetStream.struChanInfo.byEnable > 0)
                        {
                            g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].dwImageType = CHCNetSDK.CHAN_ORIGINAL;
                        }
                        else
                        {
                            g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].dwImageType = CHCNetSDK.CHAN_OFF_LINE;
                        }

                        //g_struDeviceInfo[iDeviceIndex].iEnableChanNum ++;
                    }
                    else
                    {
                        g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].dwImageType = CHCNetSDK.CHAN_OFF_LINE;
                        g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].bEnable = false;
                        //g_struDeviceInfo[iDeviceIndex].struChanInfo[i].bAlarm = FALSE;
                    }

                    iIPChanCount++;
                }
                else
                {
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].iDeviceIndex = -1;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].iChanIndex = -1;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].iChanType = CHCNetSDK.DEMO_CHANNEL_TYPE.DEMO_CHANNEL_TYPE_INVALID;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].iChannelNO = -1;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].bEnable = false;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].bAlarm = false;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].bLocalManualRec = false;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].lRealHandle = -1;
                    g_deviceTree.g_struDeviceInfo[iDeviceIndex].pStruChanInfo[i + iGroupNO * 64].chChanName = "";
                }
            }
        }

        public static bool SDK_CloseAlarmChan(int lAlarmHandle)
        {
            return CHCNetSDK.NET_DVR_CloseAlarmChan_V30(lAlarmHandle);
        }

        public static bool SDK_Logout(Int32 lUserID)
        {
            return CHCNetSDK.NET_DVR_Logout_V30(lUserID);
        }
       
    }
}
