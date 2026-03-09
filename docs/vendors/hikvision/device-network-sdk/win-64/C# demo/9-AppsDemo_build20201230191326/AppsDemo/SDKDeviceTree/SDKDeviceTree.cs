using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDKDeviceTree
{
    public class SDKDeviceTree : IDeviceTree
    {
        public SDKDeviceTree()
        {
        }

        public override event SelectedNodeChangedHandler SelectedNodeChanged;

        private CtrlDeviceTree m_DeviceTree = CtrlDeviceTree.Instance();

        public override UserControl GetDeviceTreeControl()
        {
            if (m_DeviceTree != null)
            {
                m_DeviceTree.SelectedNodeChanged += m_DeviceTree_SelectedNodeChanged;
            }
            return m_DeviceTree;
        }

        void m_DeviceTree_SelectedNodeChanged()
        {
            if (SelectedNodeChanged != null)
            {
                SelectedNodeChanged();
            }
        }

        public override IDeviceTree.DeviceInfo GetSelectedDeviceInfo()
        {
            if (m_DeviceTree == null)
            {
                return null;
            }
            IDeviceTree.DeviceInfo deviceInfo = new DeviceInfo();
            CHCNetSDK.STRU_DEVICE_INFO struDeviceInfo = m_DeviceTree.SelectedDeviceInfo;
            ConvertDeviceInfo(ref deviceInfo, ref struDeviceInfo);
            return deviceInfo;
        }

        public override ChannelInfo GetSelectedChannelInfo()
        {
            ChannelInfo channelInfo = new ChannelInfo();
            CHCNetSDK.STRU_CHANNEL_INFO struChannelInfo = m_DeviceTree.GetCurChanInfo();

            ConvertChannelInfo(ref channelInfo, ref struChannelInfo);
            return channelInfo;
        }

        //public override DeviceInfo GetDeviceInfoByIndex(int deviceIndex)
        //{
        //    IDeviceTree.DeviceInfo deviceInfo = new DeviceInfo();
        //    CHCNetSDK.STRU_DEVICE_INFO struDeviceInfo = m_DeviceTree.GetCurDeviceInfobyIndex(deviceIndex);
        //    ConvertDeviceInfo(ref deviceInfo, ref struDeviceInfo);
        //    return deviceInfo;

        //}

        //public override IDeviceTree.ChannelInfo GetChannelInfoByIndex(int deviceIndex, int channelIndex)
        //{
        //    ChannelInfo channelInfo = new ChannelInfo();
        //    CHCNetSDK.STRU_CHANNEL_INFO struChannelInfo = m_DeviceTree.GetCurChanInfoByIndex(deviceIndex, channelIndex);
        //    ConvertChannelInfo(ref channelInfo, ref struChannelInfo);
        //    return channelInfo;
        //}

        public override bool SetChannelPreviewHandle(int deviceIndex, int channelIndex, int previewHandle)
        {
            return m_DeviceTree.SetPreHandle(deviceIndex, channelIndex, previewHandle);
        }
        private void ConvertDeviceInfo(ref DeviceInfo deviceInfo, ref CHCNetSDK.STRU_DEVICE_INFO struDeviceInfo)
        {
            deviceInfo.iDeviceIndex = struDeviceInfo.iDeviceIndex;
            deviceInfo.lLoginID = struDeviceInfo.lLoginID;
            deviceInfo.sDeviceIP = struDeviceInfo.chDeviceIP;
            deviceInfo.sDeviceName = struDeviceInfo.chDeviceName;
            deviceInfo.sDeviceMultiIP = struDeviceInfo.chDeviceMultiIP;
            deviceInfo.sSerialNumber = struDeviceInfo.chSerialNumber;
            deviceInfo.iDeviceType = struDeviceInfo.iDeviceType;
            deviceInfo.sUsername = struDeviceInfo.chLoginUserName;
            deviceInfo.sPassword = struDeviceInfo.chLoginPwd;
            deviceInfo.sDevicePort = struDeviceInfo.lDevicePort;
            deviceInfo.bHttps = struDeviceInfo.bHttps;
            deviceInfo.iDeviceChanNum = struDeviceInfo.iDeviceChanNum;
        }

        private void ConvertChannelInfo(ref ChannelInfo channelInfo, ref CHCNetSDK.STRU_CHANNEL_INFO struChannelInfo)
        {
            channelInfo.iDeviceIndex = struChannelInfo.iDeviceIndex;
            channelInfo.iChannelIndex = struChannelInfo.iChanIndex;
            channelInfo.iChannelNo = struChannelInfo.iChannelNO;
            channelInfo.sChannelName = struChannelInfo.chChanName;
            channelInfo.iRealPlayHandle = struChannelInfo.lRealHandle;
            channelInfo.bEnabled = struChannelInfo.bEnable;
            channelInfo.sAccessChannelIP = struChannelInfo.chAccessChanIP;
            channelInfo.iStreamType = struChannelInfo.dwStreamType;
            channelInfo.iChannelType = (int)struChannelInfo.iChanType;
        }

        public override EDeviceTreeType GetDeviceTreeType()
        {
            return EDeviceTreeType.SDKDeviceTree;
        }


        public override string GetDeviceTreeName()
        {
            return "SDK DeviceTree";
        }
    }
}
