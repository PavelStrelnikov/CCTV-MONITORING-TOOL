using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.Head
{
    public class AudioIntercom
    {
        public enum AudioEncodeType
        {
            AUDIO_TYPE_PCM_S16K = 0x00,
            AUDIO_TYPE_G711A_S8K = 0x01,
            AUDIO_TYPE_G711U_S8K = 0x02,
            AUDIO_TYPE_G722_S16K = 0x03,
            AUDIO_TYPE_G726_S8K = 0x04,
            AUDIO_TYPE_MPEG2_S16K = 0x05,
            AUDIO_TYPE_AAC_S32K = 0x06,
            AUDIO_TYPE_PCM_S8K = 0x07,
            AUDIO_TYPE_PCM_S32K = 0x08,
            AUDIO_TYPE_AAC_S16K = 0x09
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SOUND_CARD_INFO
        {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 128)]
            public char[] byDeviceName;     ///<设备名称
            public uint dwFrequency;      ///<采集频率
            public uint dwRefresh;        ///<刷新频率
            public uint dwSync;           ///<同步
            public uint dwMonoSources;    ///<单声道源数量
            public uint dwStereoSources;  ///<多声道源数量
            public uint dwMajorVersion;   ///<主版本号
            public uint dwMinorVersion;   ///<次版本号
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.U4)]
            public uint[] dwReserved;       ///<保留参数
        }

        public struct AudioParam
        {
            public System.UInt16 nChannel;           ///<PCM声道数
            public System.UInt16 nBitWidth;          ///<PCM位宽
            public uint nSampleRate;        ///<PCM采样率
            public uint nBitRate;           ///<编码比特率
            public uint enAudioEncodeTypeEx;///<编解码类别
        }
        public class Two_Way_Audio_Channel
        {
            public string strID; // Channel ID
            public string strOpen;// is Channel open?
            public string strAudioCompressionType;
            public string strAudioInboundCompressionType;
            public string strSpeakerVolume;
            public string strMicrophoneVolume;
            public string strNoisereduce;
            public string strAudioBitRate; // kbs
            public string strAudioInputType;
            public string strAssociateVideoInputs_enabled;
            public List<string> listVideoInputChannelID;
            public string strAudioSamplingRate; // kHz 

            public Dictionary<string, string> dictCapability;
            public Two_Way_Audio_Channel()
            {
                strID = string.Empty;
                strOpen = string.Empty;
                strAudioCompressionType = string.Empty;
                strAudioInboundCompressionType = string.Empty;
                strSpeakerVolume = string.Empty;
                strMicrophoneVolume = string.Empty;
                strNoisereduce = string.Empty;
                strAudioBitRate = string.Empty;
                strAudioInputType = string.Empty;
                strAssociateVideoInputs_enabled = string.Empty;
                listVideoInputChannelID = new List<string>();
                strAudioSamplingRate = string.Empty;

                dictCapability = new Dictionary<string, string>();

            }
        }
        public enum BitRateEncode
        {
            BITRATE_ENCODE_8k = 8000,
            BITRATE_ENCODE_16k = 16000,     ///<16k比特率
            BITRATE_ENCODE_32k = 32000,     ///<32k比特率
            BITRATE_ENCODE_64k = 64000,      ///<64k比特率
            BITRATE_ENCODE_128k = 128000,
            BITRATE_ENCODE_192k = 192000,
            BITRATE_ENCODE_40k = 40000,
            BITRATE_ENCODE_48k = 48000,
            BITRATE_ENCODE_56k = 56000,
            BITRATE_ENCODE_80k = 80000,
            BITRATE_ENCODE_96k = 96000,
            BITRATE_ENCODE_112k = 112000,
            BITRATE_ENCODE_144k = 144000,
            BITRATE_ENCODE_160k = 160000
        }
        public enum SampleratePcm
        {
            SAMPLERATE_08K = 8000,         ///<8k采样率
            SAMPLERATE_16K = 16000,         ///<16k采样率
            SAMPLERATE_32K = 32000,         ///<32k采样率
            SAMPLERATE_44K1 = 44100,         ///<44.1k采样率
            SAMPLERATE_48K = 48000          ///<48k采样率
        }
        public enum BitsPcm
        {
            BITS_08 = 8,                    ///<编解码库不支持
            BITS_16 = 16                    ///<16位
        }
        public enum AudioEncodeTypeEx
        {
            AUDIO_TYPE_PCM = 0x00,
            AUDIO_TYPE_G711A = 0x01,
            AUDIO_TYPE_G711U = 0x02,
            AUDIO_TYPE_G722 = 0x03,
            AUDIO_TYPE_G726 = 0x04,
            AUDIO_TYPE_MPEG2 = 0x05,
            AUDIO_TYPE_AAC = 0x06
        }
        public struct OutputDataInfo
        {
            public IntPtr pData;
            public uint dwDataLen;
            public uint enDataType;
        }

        public delegate void DataCallBack(int code, IntPtr rtpPacket, int len);
        public delegate void OutputDataCallBack(IntPtr pstDataInfo, IntPtr pUser);

        //AudioIntercom.dll
        [DllImport("AudioIntercom.dll", EntryPoint = "AUDIOCOM_GetSoundCardNum")]
        public static extern int AUDIOCOM_GetSoundCardNum(ref uint pdwDeviceNum);
        [DllImport("AudioIntercom.dll", EntryPoint = "AUDIOCOM_GetLastError")]
        public static extern int AUDIOCOM_GetLastError(int nPort);
        [DllImport("AudioIntercom.dll", EntryPoint = "AUDIOCOM_GetOneSoundCardInfo")]
        public static extern int AUDIOCOM_GetOneSoundCardInfo(uint dwDeviceIndex, ref SOUND_CARD_INFO pstDeviceInfo);
        [DllImport("AudioIntercom.dll", EntryPoint = "AUDIOCOM_CreateCaptureHandle")]
        public static extern int AUDIOCOM_CreateCaptureHandle(ref int piCapturePort, string pDeviceName);
        [DllImport("AudioIntercom.dll", EntryPoint = "AUDIOCOM_RegisterOutputDataCallBackEx")]
        public static extern int AUDIOCOM_RegisterOutputDataCallBackEx(int iCapturePort, ref AudioParam pstAudioParam, OutputDataCallBack pfnOutputDataCallBack, IntPtr pUser);
        [DllImport("AudioIntercom.dll", EntryPoint = "AUDIOCOM_StartCapture")]
        public static extern int AUDIOCOM_StartCapture(int iCapturePort);
        [DllImport("AudioIntercom.dll", EntryPoint = "AUDIOCOM_StopCapture")]
        public static extern int AUDIOCOM_StopCapture(int iCapturePort);
        [DllImport("AudioIntercom.dll", EntryPoint = "AUDIOCOM_ReleaseCaptureHandle")]
        public static extern int AUDIOCOM_ReleaseCaptureHandle(int iCapturePort);
        [DllImport("AudioIntercom.dll", EntryPoint = "AUDIOCOM_CreatePlayHandle")]
        public static extern int AUDIOCOM_CreatePlayHandle(ref int nPlayPort, string pDeviceName);
        [DllImport("AudioIntercom.dll", EntryPoint = "AUDIOCOM_OpenStream")]
        public static extern int AUDIOCOM_OpenStream(int nPlayPort, AudioEncodeType enDataType);
        [DllImport("AudioIntercom.dll", EntryPoint = "AUDIOCOM_InputStreamData")]
        public static extern int AUDIOCOM_InputStreamData(int nPlayPort, IntPtr pData, uint dwDataLen);
        [DllImport("AudioIntercom.dll", EntryPoint = "AUDIOCOM_StartPlay")]
        public static extern int AUDIOCOM_StartPlay(int nPlayPort);
        [DllImport("AudioIntercom.dll", EntryPoint = "AUDIOCOM_StopPlay")]
        public static extern int AUDIOCOM_StopPlay(int nPlayPort);
        [DllImport("AudioIntercom.dll", EntryPoint = "AUDIOCOM_ReleasePlayHandle")]
        public static extern int AUDIOCOM_ReleasePlayHnadle(int nPlayPort);


        private static bool GetSoundCardInfo(ref SOUND_CARD_INFO soundCardInfo)
        {
            soundCardInfo = new SOUND_CARD_INFO();
            uint iDeviceNum = 0;
            int iVal = AUDIOCOM_GetSoundCardNum(ref iDeviceNum);
            if (iVal == 0)
            {
                return false;
            }

            for (uint i = 1; i < iDeviceNum + 1; i++)
            {
                if (AUDIOCOM_GetOneSoundCardInfo(i, ref soundCardInfo) > 0)
                {
                    return true;
                }
            }
            return false;
        }
        private static bool CreateAudioIntercomHandle(ref int capturePort, string deviceName)
        {
            int iVal = AUDIOCOM_CreateCaptureHandle(ref capturePort, deviceName);
            if (iVal == 0)
            {
                return false;
            }
            return true;
        }
        private static bool StartRecvFromMicln(int capturePort, Two_Way_Audio_Channel channel, OutputDataCallBack outputDataCallback)
        {
            
            return true;
        }
        private static bool StartPlayAudio(ref int playPort, string deviceName, AudioEncodeType enDataType)
        {
            if (AUDIOCOM_CreatePlayHandle(ref playPort, deviceName) == 0)
            {
                return false;
            }
            if (AUDIOCOM_OpenStream(playPort, enDataType) == 0)
            {
                return false;
            }
            return true;
        }
        private static void GetAudioParam(Two_Way_Audio_Channel channel, ref AudioParam audioParam, out AudioEncodeType enDataType)
        {
            if (!uint.TryParse(channel.strAudioBitRate, out audioParam.nBitRate))
            {
                audioParam.nBitRate = (uint)BitRateEncode.BITRATE_ENCODE_16k;
            }
            else
            {
                audioParam.nBitRate = audioParam.nBitRate * 1000;
            }

            float flSampleRate;
            if (!float.TryParse(channel.strAudioSamplingRate, out flSampleRate))
            {
                audioParam.nSampleRate = (uint)SampleratePcm.SAMPLERATE_16K;
            }
            else
            {
                audioParam.nSampleRate = (uint)(flSampleRate * 1000.0);
            }
            if (channel.strAudioCompressionType == "G.711ulaw")
            {
                audioParam.nBitWidth = (ushort)BitsPcm.BITS_16;
                audioParam.nSampleRate = (uint)SampleratePcm.SAMPLERATE_08K;
                audioParam.enAudioEncodeTypeEx = (uint)AudioEncodeTypeEx.AUDIO_TYPE_G711U;
                enDataType = AudioEncodeType.AUDIO_TYPE_G711U_S8K;
                return;
            }

            if (channel.strAudioCompressionType == "G.711alaw")
            {
                audioParam.nBitWidth = (ushort)BitsPcm.BITS_16;
                audioParam.nSampleRate = (uint)SampleratePcm.SAMPLERATE_08K;
                audioParam.enAudioEncodeTypeEx = (uint)AudioEncodeTypeEx.AUDIO_TYPE_G711A;
                enDataType = AudioEncodeType.AUDIO_TYPE_G711A_S8K;
                return;
            }

            if (channel.strAudioCompressionType == "G.726")
            {
                audioParam.nBitWidth = (ushort)BitsPcm.BITS_16;
                audioParam.nSampleRate = (uint)SampleratePcm.SAMPLERATE_08K;
                audioParam.enAudioEncodeTypeEx = (uint)AudioEncodeTypeEx.AUDIO_TYPE_G726;
                enDataType = AudioEncodeType.AUDIO_TYPE_G726_S8K;
                return;
            }

            if (channel.strAudioCompressionType == "MP2L2")
            {
                audioParam.nBitWidth = (ushort)BitsPcm.BITS_16;
                audioParam.nSampleRate = GetSamplingRate(audioParam.nSampleRate);
                audioParam.enAudioEncodeTypeEx = (uint)AudioEncodeTypeEx.AUDIO_TYPE_MPEG2;
                enDataType = AudioEncodeType.AUDIO_TYPE_MPEG2_S16K;
                return;
            }

            if (channel.strAudioCompressionType == "AAC")
            {
                audioParam.nBitWidth = (ushort)BitsPcm.BITS_16;
                audioParam.nSampleRate = GetSamplingRate(audioParam.nSampleRate);
                audioParam.enAudioEncodeTypeEx = (uint)AudioEncodeTypeEx.AUDIO_TYPE_AAC;
                enDataType = AudioEncodeType.AUDIO_TYPE_AAC_S32K;
                return;
            }

            if (channel.strAudioCompressionType == "PCM")
            {
                audioParam.nBitWidth = (ushort)BitsPcm.BITS_16;
                audioParam.nSampleRate = GetSamplingRate(audioParam.nSampleRate);
                audioParam.enAudioEncodeTypeEx = (uint)AudioEncodeTypeEx.AUDIO_TYPE_PCM;
                enDataType = AudioEncodeType.AUDIO_TYPE_PCM_S16K;
                return;
            }

            //G722
            audioParam.nBitWidth = (ushort)BitsPcm.BITS_16;
            audioParam.enAudioEncodeTypeEx = (uint)AudioEncodeTypeEx.AUDIO_TYPE_G722;
            enDataType = AudioEncodeType.AUDIO_TYPE_G722_S16K;
        }
        private static uint GetSamplingRate(uint SamplingRate)
        {
            if (0 == SamplingRate || SamplingRate == 1)
            {
                //AUDIO_TYPE_AAC_S16K   <——> {1, 16, 16000, 32000,   AUDIO_TYPE_AAC}
                return (uint)SampleratePcm.SAMPLERATE_16K;
            }
            else if (2 == SamplingRate)
            {
                //AUDIO_TYPE_AAC_S32K   <——> {1, 16, 32000, 32000,   AUDIO_TYPE_AAC}
                return (uint)SampleratePcm.SAMPLERATE_32K;
            }
            else if (3 == SamplingRate)
            {
                //AAC sample:48k bitrate:32k   {1, 16, 48000, 32000,   AUDIO_TYPE_AAC}
                return (uint)SampleratePcm.SAMPLERATE_48K;
            }
            else if (4 == SamplingRate)
            {
                //AUDIO_TYPE_PCM_S44.1K   <——> {1, 16, 44100, 0,   AUDIO_TYPE_AAC}
                return (uint)SampleratePcm.SAMPLERATE_44K1;
            }
            return SamplingRate;
        }
        public static bool StartAudioIntercom(Two_Way_Audio_Channel channel, OutputDataCallBack outputDataCallback, out int capturePort, out int playPort)
        {
            capturePort = -1;
            playPort = -1;
            do
            {
                SOUND_CARD_INFO soundCardInfo = new SOUND_CARD_INFO();
                if (!GetSoundCardInfo(ref soundCardInfo))
                {
                    break;
                }
                string strDeviceName = new string(soundCardInfo.byDeviceName);
                if (!CreateAudioIntercomHandle(ref capturePort, strDeviceName))
                {
                    break;
                }
                AudioParam audioParam = new AudioParam();
                AudioEncodeType enDataType = AudioEncodeType.AUDIO_TYPE_G722_S16K;
                GetAudioParam(channel, ref audioParam, out enDataType);
                if (audioParam.nChannel == 0)
                {
                    audioParam.nChannel = 1;
                }
                if (AUDIOCOM_RegisterOutputDataCallBackEx(capturePort, ref audioParam, outputDataCallback, IntPtr.Zero) != 1)
                {
                    break;
                }
                if (!StartPlayAudio(ref playPort, strDeviceName, enDataType))
                {
                    break;
                }
                if (AUDIOCOM_StartCapture(capturePort) == 0)
                {
                    break;
                }
                if (AUDIOCOM_StartPlay(capturePort) == 0)
                {
                    break;
                }
                //到此成功，返回true
                return true;
            } while (false);

            //错误，释放资源
            StopAudioIntercom(ref capturePort, ref playPort);
            return false;
        }

        public static bool StopAudioIntercom(ref int capturePort,ref int playPort)
        {
            if (capturePort < 0)
            {
                return false;
            }
            if (AUDIOCOM_StopCapture(capturePort) != 1)
            {
                return false;
            }

            if (AUDIOCOM_ReleaseCaptureHandle(capturePort) == 0)
            {
                return false;
            }
            capturePort = -1;

            if (playPort < 0)
            {
                return false;
            }
            AUDIOCOM_StopPlay(playPort);
            AUDIOCOM_ReleasePlayHnadle(playPort);
            playPort = -1;
            return true;
        }
    }
}
