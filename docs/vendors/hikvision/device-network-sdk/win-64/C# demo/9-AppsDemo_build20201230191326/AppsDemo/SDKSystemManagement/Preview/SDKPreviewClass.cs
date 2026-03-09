using Common;
using Common.Head;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace SDKSystemManagement
{
    public partial class FormPreview
    {
        public void SDK_StopPlay(int PanelNo)
        {

            m_iCurDeviceIndex = g_deviceTree.GetSelectedDeviceInfo().iDeviceIndex;
            m_iCurChanNo = g_deviceTree.GetSelectedChannelInfo().iChannelNo;
            if (PanelNo != -1)
            {
                if (m_strPanelInfo[PanelNo].lRealHandle >= 0 && g_deviceTree.GetSelectedChannelInfo().iRealPlayHandle != m_strPanelInfo[PanelNo].lRealHandle)
                {
                    bool bStopReal = CHCNetSDK.NET_DVR_StopRealPlay(m_strPanelInfo[PanelNo].lRealHandle);
                    if (!bStopReal)
                    {
                        MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKPreview", "NET_DVR_StopRealPlay");
                    }
                    else
                    {
                        g_deviceTree.SetChannelPreviewHandle(m_strPanelInfo[PanelNo].iDeviceIndex, m_strPanelInfo[PanelNo].iChanIndex, -1);
                        m_panelOne.Refresh();
                        m_panelTwo.Refresh();
                        m_panelThree.Refresh();
                        m_panelFour.Refresh();
                    }
                }
            }

            if (m_iCurDeviceIndex > -1 && m_iCurChanNo > -1)
            {
                if (g_deviceTree.GetSelectedChannelInfo().iRealPlayHandle >= 0)
                {
                    bool bStopReal = CHCNetSDK.NET_DVR_StopRealPlay(g_deviceTree.GetSelectedChannelInfo().iRealPlayHandle);
                    if (!bStopReal)
                    {
                        MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKPreview", "NET_DVR_StopRealPlay");
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (m_strPanelInfo[i].lRealHandle == (int)g_deviceTree.GetSelectedChannelInfo().iRealPlayHandle)
                            {
                                m_strPanelInfo[i].lRealHandle = -1;
                            }
                        }
                        g_deviceTree.SetChannelPreviewHandle(m_iCurDeviceIndex, m_iCurChanIndex, -1);
                        m_panelOne.Refresh();
                        m_panelTwo.Refresh();
                        m_panelThree.Refresh();
                        m_panelFour.Refresh();
                    }
                }
                return;
            }
        }

        public bool SDK_StartPlay(int PanelNo, int dwLinkMode, int dwStreamType, byte byProtoType)
        {
            SDK_StopPlay(PanelNo);
            CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
            m_iCurDeviceIndex = g_deviceTree.GetSelectedDeviceInfo().iDeviceIndex;
            m_iCurChanIndex = g_deviceTree.GetSelectedChannelInfo().iChannelIndex;
            m_iCurChanNo = g_deviceTree.GetSelectedChannelInfo().iChannelNo;

            if (m_iCurDeviceIndex > -1 && m_iCurChanNo > -1)
            {
                m_lUserID = (int)g_deviceTree.GetSelectedDeviceInfo().lLoginID;
                IntPtr pUser = new IntPtr(PanelNo);
                lpPreviewInfo.lChannel = m_iCurChanNo;
                lpPreviewInfo.bBlocked = true;
                lpPreviewInfo.DisplayBufNum = 0;
                lpPreviewInfo.dwStreamType = g_deviceTree.GetSelectedChannelInfo().iStreamType;
                lpPreviewInfo.dwLinkMode = (uint)dwLinkMode;
                lpPreviewInfo.dwStreamType = (uint)dwStreamType;
                lpPreviewInfo.byProtoType = byProtoType;

                if (DecodeInRealPlayrBtn.Checked)
                {
                    lpPreviewInfo.hPlayWnd = m_strPanelInfo[PanelNo].hPlayWnd;
                    m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null, pUser);
                }
                else if (StandardCallBackradioBtn.Checked) //call back standard stream or not
                {
                    m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null, pUser);
                    if (m_lRealHandle >= 0)
                    {
                        if (m_StdDataCallBack == null)
                        {
                            m_StdDataCallBack = new CHCNetSDK.STDDATACALLBACK(StdDataCallBack);
                        }

                        if (!CHCNetSDK.NET_DVR_SetStandardDataCallBack(m_lRealHandle, m_StdDataCallBack, (uint)pUser))
                        {
                            MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKPreview", "NET_DVR_SetStandardDataCallBack");
                        }
                    }
                }
                else if (ConvertCallBackradioBtn.Checked)
                {
                    if (m_RealDataCallBack == null)
                    {
                        m_RealDataCallBack = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack);
                    }
                    m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, m_RealDataCallBack, pUser);
                }

                if (m_lRealHandle <= -1)
                {
                    MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKPreview", "NET_DVR_RealPlay_V40");
                }
                else
                {
                    g_deviceTree.SetChannelPreviewHandle(m_iCurDeviceIndex, m_iCurChanIndex, m_lRealHandle);
                    m_strPanelInfo[PanelNo].iDeviceIndex = g_deviceTree.GetSelectedChannelInfo().iDeviceIndex;
                    m_strPanelInfo[PanelNo].iChanIndex = g_deviceTree.GetSelectedChannelInfo().iChannelIndex;
                    m_strPanelInfo[PanelNo].lRealHandle = m_lRealHandle;
                    //g_deviceTree.g_struDeviceInfo[m_iCurDeviceIndex].byPanelNo = (byte)PanelNo;
                    return true;
                }
            }
            return false;
        }

        public bool SdkTURN(uint turn,int PanelNo)
        {
            CHCNetSDK.NET_DVR_PTZControlWithSpeed(m_lRealHandle,turn, 0, 5);
            Thread.Sleep(300);
            CHCNetSDK.NET_DVR_PTZControlWithSpeed(m_lRealHandle, turn, 1, 5);
            if (m_lRealHandle <= -1)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKPreview", "NET_DVR_RealPlay_V40");
            }
            else
            {
                g_deviceTree.SetChannelPreviewHandle(m_iCurDeviceIndex, m_iCurChanIndex, m_lRealHandle);
                m_strPanelInfo[PanelNo].iDeviceIndex = g_deviceTree.GetSelectedChannelInfo().iDeviceIndex;
                m_strPanelInfo[PanelNo].iChanIndex = g_deviceTree.GetSelectedChannelInfo().iChannelIndex;
                m_strPanelInfo[PanelNo].lRealHandle = m_lRealHandle;
                //g_deviceTree.g_struDeviceInfo[m_iCurDeviceIndex].byPanelNo = (byte)PanelNo;
                return true;
            }
            return false;
        }
        public void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, IntPtr pBuffer, UInt32 dwBufSize, IntPtr pUser)
        {
            lPort = m_iPort[pUser.ToInt32()];

            switch (dwDataType)
            {
                case CHCNetSDK.NET_DVR_SYSHEAD://coming the stream header, open stream
                    //soft decode
                    if (lPort < 0)
                    {
                        if (!PlayCtrl.PlayM4_GetPort(ref lPort))
                        {
                            String strTmp;
                            strTmp = String.Format("PlayM4_GetPort err: {0}", PlayCtrl.PlayM4_GetLastError((int)lPort));
                            MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKPreview", strTmp);
                            m_iPort[pUser.ToInt32()] = -1;
                            break;
                        }
                        m_iPort[pUser.ToInt32()] = lPort;
                    }

                    if (dwBufSize > 0)
                    {
                        //set as stream mode, real-time stream under preview
                        //start player
                        if (!PlayCtrl.PlayM4_OpenStream(lPort, pBuffer, dwBufSize, 2 * 1024 * 1024))
                        {
                            String strTmp;
                            strTmp = String.Format("PlayM4_OpenStream err: {0}", PlayCtrl.PlayM4_GetLastError((int)lPort));
                            MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKPreview", strTmp);

                            m_iPort[pUser.ToInt32()] = -1;
                            break;
                        }
                        //start play, set play window
                        if (!PlayCtrl.PlayM4_Play((int)lPort, m_strPanelInfo[(uint)pUser].hPlayWnd))
                        {
                            String strTmp;
                            strTmp = String.Format("PlayM4_Play err: {0}", PlayCtrl.PlayM4_GetLastError((int)lPort));
                            MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKPreview", strTmp);

                            m_iPort[pUser.ToInt32()] = -1;
                            break;
                        }
                        if (dwBufSize > 0 && lPort != -1)
                        {
                            if (!PlayCtrl.PlayM4_InputData((int)lPort, pBuffer, (uint)dwBufSize))
                            {
                                String strTmp;
                                strTmp = String.Format("PlayM4_InputData err: {0}", PlayCtrl.PlayM4_GetLastError((int)lPort));
                                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKPreview", strTmp);

                                m_iPort[pUser.ToInt32()] = -1;
                            }
                            break;
                        }
                    }
                    break;
                case CHCNetSDK.NET_DVR_STREAMDATA:   //如果是rtp流类型，可能不在上述范围之内，也是需要将数据塞到播放库中的
                    if (dwBufSize > 0 && lPort != -1)
                    {
                        if (!PlayCtrl.PlayM4_InputData((int)lPort, pBuffer, (uint)dwBufSize))
                        {
                            String strTmp;
                            strTmp = String.Format("PlayM4_InputData err: {0}", PlayCtrl.PlayM4_GetLastError((int)lPort));
                            MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKPreview", strTmp);
                        }
                        //break;
                    }
                    break;
            }
            return;
        }
        public void StdDataCallBack(int lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, uint dwUser)
        {
            RealDataCallBack(lRealHandle, dwDataType, pBuffer, dwBufSize, (IntPtr)dwUser);
            return;
        }
    }
}
