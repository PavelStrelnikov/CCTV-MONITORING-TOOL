using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Common.Head
{
    public class RTSP
    {
        public enum DataTypeCode
        {
            enumMediaInfo = 0,
            enumRtpPacket,
            enumMetadata,
            enumError,
            enumThermalData
        }

        public enum RtspReturn
        {
            enumRtspFalse = 0,
            enumRtspTrue
        }

        public struct RtspDeviceInfo
        {
            public int iIP;
            public int iPort;
            public int iChannel;
            public int iChannelType;
            public string strUsername;
            public string strPassword;
            public string strUrl;
            public string strParam; // have ?
            public float scale;
            public bool bGetMetadata;
            public int nRtspConnectTimeout;//ms
            public int nRtspReceiveTimeout;//ms
        }

        public delegate void DataCallBack(int code, IntPtr rtpPacket, int len);

        // RTSP.dll
        [DllImport(@"RTSP.dll", EntryPoint = "CreateRtsp")]
        public static extern IntPtr CreateRtsp(bool store);

        [DllImport(@"RTSP.dll", EntryPoint = "DeleteRtsp")]
        public static extern IntPtr DeleteRtsp(IntPtr pRtsp);

        // Return:		if success return 1, otherwise return 0.
        [DllImport(@"RTSP.dll", EntryPoint = "StartGetRtspData")]
        public static extern int StartGetRtspData(IntPtr pRtsp, ref RtspDeviceInfo struRtspDeviceInfo, DataCallBack callback);

        // Return:		if success return 1, otherwise return 0.
        [DllImport(@"RTSP.dll", EntryPoint = "StopGetRtspData")]
        public static extern int StopGetRtspData(IntPtr pRtsp);

        [DllImport(@"RTSP.dll", EntryPoint = "SetScale")]
        public static extern int SetScale(IntPtr pRtsp, float scale);

        [DllImport(@"RTSP.dll", EntryPoint = "PauseGetRtspData")]
        public static extern int PauseGetRtspData(IntPtr pRtsp);

        [DllImport(@"RTSP.dll", EntryPoint = "ContinueGetRtspData")]
        public static extern int ContinueGetRtspData(IntPtr pRtsp);
    }
}
