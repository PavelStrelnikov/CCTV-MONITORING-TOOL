using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common
{
    public abstract class IDeviceTree
    {
        public IDeviceTree() { }


        public class DeviceInfo
        {
            //device index
            public int iDeviceIndex { get; set; }
            //ID
            public long lLoginID { get; set; }
            //device IP: IP,pppoe address, or network gate address, etc
            public string sDeviceIP { get; set; }
            public int sDevicePort { get; set; }

            public string sDeviceName { get; set; }
            //multi-cast group address
            public string sDeviceMultiIP { get; set; }
            //SN
            public string sSerialNumber { get; set; }
            //channel numder  (analog + ip)
            public int iDeviceChanNum { get; set; }
            //device type
            public int iDeviceType { get; set; }
            public string sUsername { get; set; }
            public string sPassword { get; set; }
            public bool bHttps { get; set; }

        }
        public class ChannelInfo
        {
            public int iDeviceIndex { get; set; }
            public int iChannelIndex { get; set; }
            public int iChannelNo { get; set; }
            public string sChannelName { get; set; }
            public int iRealPlayHandle { get; set; }
            public bool bEnabled { get; set; }
            public string sAccessChannelIP { get; set; }
            public uint iStreamType { get; set; }
            public int iChannelType { get; set; }
        }


        public enum EDeviceTreeType
        {
            UnknownDeviceTree = 0, //表示没有类型，没有意义
            SDKDeviceTree,
            ISAPIDeviceTree,
        }

        public delegate void SelectedNodeChangedHandler();
        public abstract event SelectedNodeChangedHandler SelectedNodeChanged;

        public abstract UserControl GetDeviceTreeControl();

        public abstract DeviceInfo GetSelectedDeviceInfo();
        public abstract ChannelInfo GetSelectedChannelInfo();
        public abstract bool SetChannelPreviewHandle(int deviceIndex, int channelIndex, int previewHandle);
        public abstract EDeviceTreeType GetDeviceTreeType();
        public abstract string GetDeviceTreeName();
    }
}
