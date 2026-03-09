using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonBase.Head
{
    class FFmpeg
    {
        public const int FFMPEG_NOERROR = 0;//no error
        public const int FFMPEG_DATA_ERROR = 1;//Wrong data length
        public const int FFMPEG_SDL_INIT_ERROR = 2;//SDL init failed
        public const int FFMPEG_FFMPEG_INIT_ERROR = 3;//FFmpeg init failed
        public const int FFMPEG_CREATE_THREAD_ERROR = 4;//create thread failed


        public enum VideoCodecType
        {
            H264 = 0x00,
            H265 = 0x01,
            MPEG4 = 0x02
        }


        //FFmpeg
        [DllImport("FFmpeg2.dll", EntryPoint = "CreateFFmpeg")]
        public static extern IntPtr CreateFFmpeg();

        [DllImport("FFmpeg2.dll", EntryPoint = "DeleteFFmpeg")]
        public static extern void DeleteFFmpeg(IntPtr pFFmpeg);

        [DllImport("FFmpeg2.dll", EntryPoint = "AddRtpPktToQueue")]
        public static extern int AddRtpPktToQueue(IntPtr pFFmpeg, int len, IntPtr data);

        [DllImport("FFmpeg2.dll", EntryPoint = "SetAudioAndVideoParam")]
        public static extern void SetAudioAndVideoParam(IntPtr pFFmpeg, FFmpegAudioAndVideoInfo param);

        [DllImport("FFmpeg2.dll", EntryPoint = "Start")]
        public static extern void Start(IntPtr pFFmpeg);

        [DllImport("FFmpeg2.dll", EntryPoint = "StreamCreateFFmpeg")]
        public static extern IntPtr StreamCreateFFmpeg();

        [DllImport("FFmpeg2.dll", EntryPoint = "StreamStart")]
        public static extern void StreamStart(IntPtr pFFmpeg, string path, IntPtr hwnd);

        [DllImport("FFmpeg2.dll", EntryPoint = "StreamDeleteFFmpeg")]
        public static extern void StreamDeleteFFmpeg(IntPtr pFFmpeg);
    }

    public struct FFmpegAudioAndVideoInfo
    {
        public int width;
        public int height;
        public float fps;
        public int videoCodecType;
        public int audioCodecType;
        public bool audioEnable;
    }

}
