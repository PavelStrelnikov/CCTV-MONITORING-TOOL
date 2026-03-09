using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using Common;

namespace SDKAIOpenPlatform
{
    public partial class AIAlarmForm : Form
    {
        public AIAlarmForm()
        {
            InitializeComponent();
            m_lFortifyHandle = -1;
            Control.CheckForIllegalCrossThreadCalls = false;

            GetDictionary();
        }

        //设备树对象映射
        IDeviceTree m_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        //报警布防回调
        private CHCNetSDK.MSGCallBack m_falarmData = null;
        //本地文件存储根目录
        public string strLocalFilePath;
        //存储字典
        private Dictionary<string, Dictionary<string, List<string>>> DataDictionary = new Dictionary<string, Dictionary<string, List<string>>>();

        //布防句柄
        private int m_lFortifyHandle;
        private void AIAlarmForm_Load(object sender, EventArgs e)
        {
            strLocalFilePath = Application.StartupPath + "\\AIOP\\" + DateTime.Now.ToString("yyyy-MM-dd");
            if (!Directory.Exists(strLocalFilePath))
                Directory.CreateDirectory(strLocalFilePath);
        }

        //布防
        private void GurdBtn_Click(object sender, EventArgs e)
        {
            //SDK私有协议交互实现
            SDK_alarmGuard(); 
        }

        //私有协议-布防
        public void SDK_alarmGuard()
        {
            //获取设备信息
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();

            CHCNetSDK.NET_DVR_SETUPALARM_PARAM struSetupAlarmParam = new CHCNetSDK.NET_DVR_SETUPALARM_PARAM();
            struSetupAlarmParam.dwSize = (uint)Marshal.SizeOf(struSetupAlarmParam);
            struSetupAlarmParam.byLevel = 1;
            struSetupAlarmParam.byAlarmInfoType = 1;

            m_lFortifyHandle = (int)CHCNetSDK.NET_DVR_SetupAlarmChan_V41((int)deviceInfo.lLoginID, ref struSetupAlarmParam);
            if (-1 == m_lFortifyHandle)
            {
                MessageBox.Show("建立布防通道失败！", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                m_falarmData = new CHCNetSDK.MSGCallBack(MsgCallback);
                if (CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V30(m_falarmData, IntPtr.Zero))
                {
                    MessageBox.Show("布防成功！", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("布防失败！", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        //报警布防回调
        public void MsgCallback(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            switch (lCommand)
            {
                case CHCNetSDK.COMM_UPLOAD_AIOP_VIDEO:
                    ProcessCommAlarm_AIOPVideo(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case CHCNetSDK.COMM_UPLOAD_AIOP_PICTURE:
                    ProcessCommAlarm_AIOPPicture(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case CHCNetSDK.COMM_UPLOAD_AIOP_POLLING_SNAP:
                    ProcessCommAlarm_AIOPPollingSnap(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case CHCNetSDK.COMM_UPLOAD_AIOP_POLLING_VIDEO:
                    ProcessCommAlarm_AIOPPollingVideo(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                default:
                    break;
            }
        }

        //设备支持AI开放平台接入，上传视频检测数据
        public class AIOPData
        {
            public string width { get; set; }/*必填，string，maxLen=32，图片宽*/
            public string height { get; set; }/*必填，string，maxLen=32，图片高*/
            public AIOPDataTarget[] targets { get; set; }/*必填，array，输出目标*/
        }

        public class AIOPDataTarget
        {
            public AIOPDataTargetObj obj { get; set; } /*选填，object，检测_输出目标*/
            public AIOPDataTargetPropertie[] properties { get; set; } /*选填，object，分类结果*/
        }

        public class AIOPDataTargetObj
        {
            public string modelID { get; set; }/*选填，string，模型id  当前版本检测只有一个模型*/
            public int id { get; set; }/*选填，int，目标id，图片选填，视频必填*/
            public int type { get; set; }/*必填，int，算法分析返回结果值*/
            public int confidence { get; set; }/*必填，int，range=[0,1000]，分类结果置信度*/
            public int valid { get; set; }/*必填，int，目标是否有效 0无效  1 有效*/
            public int visible { get; set; }/*必填，int，目标是否可见 0不可见，1 可见*/
            public AIOPDataTargetObjRect rect { get; set; }/*选填，object，目标框，检测的时候必填，分类的时候选填*/
        }

        public class AIOPDataTargetObjRect
        {
            public float x { get; set; }/*必填，string，maxLen=32，目标框x值*/
            public float y { get; set; }/*必填，string，maxLen=32，目标框y值*/
            public float w { get; set; }/*必填，string，maxLen=32，目标框W值*/
            public float h { get; set; } /*必填，string，maxLen=32，目标框h值*/
        }

        public class AIOPDataTargetPropertie
        {
            public string modelID { get; set; }/*必填，string，模型id*/
            public AIOPDataTargetPropertieClassfy classify { get; set; }/*必填，object，分类结果*/
        }

        public class AIOPDataTargetPropertieClassfy
        {
            public int attrType { get; set; }/*必填，int，分类类型*/
            public int attrValue { get; set; }/*选填，int，分类属性，多标签必填*/
            public string attrConf { get; set; }/*必填，int，range=[0,1000]，分类结果置信度*/
        }
        public class AIModelInfo
        {
            public string MPID { get; set; }
            public string MPName { get; set; }
            public int status { get; set; }
            public string[] engine { get; set; }
            public description[] description { get; set; }
        }
        public class description
        {
            public string[] labels { get; set; }
            public string modelId { get; set; }
            public relation relation { get; set; }
            public int type { get; set; }
        }
        public class relation
        {
            public int label { get; set; }
            public string modelId { get; set; }
        }
        public class AIModelInfoList
        {
            public AIModelInfo[] AlgorithmModels { get; set; }
        }
        private void ProcessCommAlarm_AIOPVideo(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //获取当前系统时间
            string datatimenow = DateTime.Now.ToString("yyyyMMddhhmmssfff");
            //检测图片存储路径
            string strPicFilePath = "";
            //AIOP数据存储路径
            string strAIOPDataPath = "";
            //AIOP展示数据存储路径
            string strShowDataPath = "";
            //AIOP数结构体声明
            AIOPData m_AIOPData = new AIOPData();

            //报警信息结构体&数据转换
            CHCNetSDK.NET_AIOP_VIDEO_HEAD struAIOPVideoHead = new CHCNetSDK.NET_AIOP_VIDEO_HEAD();
            uint dwSize = (uint)Marshal.SizeOf(struAIOPVideoHead);
            struAIOPVideoHead = (CHCNetSDK.NET_AIOP_VIDEO_HEAD)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_AIOP_VIDEO_HEAD));

            //拼接报警信息中的时间信息
            string strTime = struAIOPVideoHead.struTime.wYear.ToString() + "-" + struAIOPVideoHead.struTime.wMonth.ToString()
                + "-" + struAIOPVideoHead.struTime.wDay.ToString() + " " + struAIOPVideoHead.struTime.wHour.ToString()
                + ":" + struAIOPVideoHead.struTime.wMinute.ToString() + ":" + struAIOPVideoHead.struTime.wSecond.ToString()
                + ":" + struAIOPVideoHead.struTime.wMilliSec.ToString();

            //AIOPDdata数据
            if (struAIOPVideoHead.dwAIOPDataSize > 0 && struAIOPVideoHead.pBufferAIOPData != IntPtr.Zero)
            {
                //保存文件到本地s
                strAIOPDataPath = strLocalFilePath + "\\AIOPVideo" + datatimenow + "AIOP.txt";
                FileStream fs = new FileStream(strAIOPDataPath, FileMode.Create);
                int iLen = (int)struAIOPVideoHead.dwAIOPDataSize;
                byte[] byDataTempBuffer = new byte[iLen];
                Marshal.Copy(struAIOPVideoHead.pBufferAIOPData, byDataTempBuffer, 0, iLen);
                fs.Write(byDataTempBuffer, 0, iLen);
                fs.Close();

                //解析数据
                string strAIOPData = System.Text.Encoding.Default.GetString(byDataTempBuffer);
                m_AIOPData = JsonConvert.DeserializeObject<AIOPData>(strAIOPData);
                //AIOPDatatBox.Text = strAIOPData;

                strShowDataPath = strLocalFilePath + "\\AIOPVideo" + datatimenow + "ShowData.txt";
                AnalysizeAIOPData(m_AIOPData, strShowDataPath);
            }

            //检测图片
            if (struAIOPVideoHead.dwPictureSize > 0 && struAIOPVideoHead.pBufferPicture != IntPtr.Zero)
            {
                strPicFilePath = strLocalFilePath + "\\AIOPVideo" + datatimenow + "Pic.jpg";

                int iLen = (int)struAIOPVideoHead.dwPictureSize;
                byte[] byDataTempBuffer = new byte[iLen];
                Marshal.Copy(struAIOPVideoHead.pBufferPicture, byDataTempBuffer, 0, iLen);
                Image img = System.Drawing.Image.FromStream(new System.IO.MemoryStream(byDataTempBuffer));
                Bitmap bmpImage = new System.Drawing.Bitmap(img);

                if (m_AIOPData.targets.Length > 0)
                {
                    for (int i = 0; i < m_AIOPData.targets.Length; i++)
                    {
                        if (m_AIOPData.targets[i].obj.rect.w > 0)
                        {
                            Graphics g = Graphics.FromImage(bmpImage);
                            int x = (int)(m_AIOPData.targets[i].obj.rect.x * img.Width);
                            int y = (int)(m_AIOPData.targets[i].obj.rect.y * img.Height);
                            int width = (int)(m_AIOPData.targets[i].obj.rect.w * img.Width);
                            int height = (int)(m_AIOPData.targets[i].obj.rect.w * img.Height);

                            if (x <= img.Width && y <= img.Height && width <= img.Width && height <= img.Height)
                            {
                                //draw rect                             
                                Brush brush = new SolidBrush(Color.Red);
                                Pen pen = new Pen(brush, 3);
                                g.DrawRectangle(pen, new Rectangle(x, y, width, height));

                                string str = "target:" + i;
                                Font font = new Font("粗体", 40);
                                SolidBrush sbrush = new SolidBrush(Color.Black);
                                g.DrawString(str, font, sbrush, new PointF(x, y));
                            }

                            g.Dispose();
                        }
                    }
                }

                bmpImage.Save(strPicFilePath);
                analysisPictureBox.Image = Image.FromFile(strPicFilePath);

                img.Dispose();
                bmpImage.Dispose();

            }

            ListViewItem lvi = new ListViewItem();

            string strAlarmType = "上传视频检测数据";
            lvi.Text = strAlarmType;//报警类型
            lvi.SubItems.Add(struAIOPVideoHead.dwChannel.ToString());//通道号
            lvi.SubItems.Add(strTime);//时间
            lvi.SubItems.Add(struAIOPVideoHead.szTaskID);//任务ID
            lvi.SubItems.Add(strPicFilePath);//检测图片
            //lvi.SubItems.Add(strAIOPDataPath);//设备模型检测数据
            lvi.SubItems.Add(strShowDataPath);//设备模型检测数据
            lvi.SubItems.Add(struAIOPVideoHead.szMPID);//模型ID
            lvi.SubItems.Add("--");
            lvi.SubItems.Add("--");
            AlarmInfolistView.Items.Insert(0, lvi);
        }

        //设备支持AI开放平台接入，上传图片检测数据
        private void ProcessCommAlarm_AIOPPicture(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //获取当前系统时间
            string datatimenow = DateTime.Now.ToString("yyyyMMddhhmmssfff");
            //检测图片存储路径
            string strPicFilePath = "";
            //AIOP数据存储路径
            string strAIOPDataPath = "";
            //AIOP展示数据存储路径
            string strShowDataPath = "";
            //AIOP数结构体声明
            AIOPData m_AIOPData = new AIOPData();

            //报警信息结构体&数据转换
            CHCNetSDK.NET_AIOP_PICTURE_HEAD struAIOPPictureHead = new CHCNetSDK.NET_AIOP_PICTURE_HEAD();
            uint dwSize = (uint)Marshal.SizeOf(struAIOPPictureHead);
            struAIOPPictureHead = (CHCNetSDK.NET_AIOP_PICTURE_HEAD)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_AIOP_PICTURE_HEAD));

            //拼接报警信息中的时间信息
            string strTime = struAIOPPictureHead.struTime.wYear.ToString() + "-" + struAIOPPictureHead.struTime.wMonth.ToString() + "-" + struAIOPPictureHead.struTime.wDay.ToString() + " " + struAIOPPictureHead.struTime.wHour.ToString() + ":" + struAIOPPictureHead.struTime.wMinute.ToString() + ":" + struAIOPPictureHead.struTime.wSecond.ToString() + ":" + struAIOPPictureHead.struTime.wMilliSec.ToString();

            //AIOPDdata数据
            if (struAIOPPictureHead.dwAIOPDataSize > 0 && struAIOPPictureHead.pBufferAIOPData != null)
            {
                //保存文件到本地
                strAIOPDataPath = strLocalFilePath + "\\AIOPPicture" + datatimenow + "AIOP.txt";
                FileStream fs = new FileStream(strAIOPDataPath, FileMode.Create);
                int iLen = (int)struAIOPPictureHead.dwAIOPDataSize;
                byte[] byDataTempBuffer = new byte[iLen];
                Marshal.Copy(struAIOPPictureHead.pBufferAIOPData, byDataTempBuffer, 0, iLen);
                fs.Write(byDataTempBuffer, 0, iLen);
                fs.Close();

                //解析数据
                string strAIOPData = System.Text.Encoding.Default.GetString(byDataTempBuffer);
                m_AIOPData = JsonConvert.DeserializeObject<AIOPData>(strAIOPData);
                AIOPDatatBox.Text = strAIOPData;

                //AIOPData解析
                strShowDataPath = strLocalFilePath + "\\AIOPPicture" + datatimenow + "ShowData.txt";
                AnalysizeAIOPData(m_AIOPData, strShowDataPath);
            }

            ListViewItem lvi = new ListViewItem();

            string strAlarmType = "上传图片检测数据";
            lvi.Text = strAlarmType;//报警类型
            lvi.SubItems.Add("--");//通道号
            lvi.SubItems.Add(strTime);//时间
            lvi.SubItems.Add("--");//任务ID
            lvi.SubItems.Add("--");//检测图片
            lvi.SubItems.Add(strShowDataPath);//设备模型检测数据
            lvi.SubItems.Add(struAIOPPictureHead.szMPID);//模型ID
            lvi.SubItems.Add(struAIOPPictureHead.szPID);//图片ID

            string strStatus = "";
            if (struAIOPPictureHead.byStatus == 0)
            {
                strStatus = "成功";
            }
            else if (struAIOPPictureHead.byStatus == 1)
            {
                strStatus = "图片大小错误";
            }
            lvi.SubItems.Add(strStatus);//状态信息

            AlarmInfolistView.Items.Insert(0, lvi);
        }

        //设备支持AI开放平台接入，上传轮询抓图图片检测数据
        private void ProcessCommAlarm_AIOPPollingSnap(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //获取当前系统时间
            string datatimenow = DateTime.Now.ToString("yyyyMMddhhmmssfff");
            //检测图片存储路径
            string strPicFilePath = "";
            //AIOP数据存储路径
            string strAIOPDataPath = "";
            //AIOP展示数据存储路径
            string strShowDataPath = "";
            //AIOP数结构体声明
            AIOPData m_AIOPData = new AIOPData();

            //报警信息结构体&数据转换
            CHCNetSDK.NET_AIOP_POLLING_SNAP_HEAD struAIOPPollingSnapHead = new CHCNetSDK.NET_AIOP_POLLING_SNAP_HEAD();
            uint dwSize = (uint)Marshal.SizeOf(struAIOPPollingSnapHead);
            struAIOPPollingSnapHead = (CHCNetSDK.NET_AIOP_POLLING_SNAP_HEAD)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_AIOP_POLLING_SNAP_HEAD));

            //拼接报警信息中的时间信息
            string strTime = struAIOPPollingSnapHead.struTime.wYear.ToString() + "-" + struAIOPPollingSnapHead.struTime.wMonth.ToString() + "-" + struAIOPPollingSnapHead.struTime.wDay.ToString() + " " + struAIOPPollingSnapHead.struTime.wHour.ToString() + ":" + struAIOPPollingSnapHead.struTime.wMinute.ToString() + ":" + struAIOPPollingSnapHead.struTime.wSecond.ToString() + ":" + struAIOPPollingSnapHead.struTime.wMilliSec.ToString();

            //AIOPDdata数据
            if (struAIOPPollingSnapHead.dwAIOPDataSize > 0 && struAIOPPollingSnapHead.pBufferAIOPData != null)
            {
                //保存文件到本地
                strAIOPDataPath = strLocalFilePath + "\\AIOPPollingSnap" + datatimenow + "AIOP.txt";
                FileStream fs = new FileStream(strAIOPDataPath, FileMode.Create);
                int iLen = (int)struAIOPPollingSnapHead.dwAIOPDataSize;
                byte[] byDataTempBuffer = new byte[iLen];
                Marshal.Copy(struAIOPPollingSnapHead.pBufferAIOPData, byDataTempBuffer, 0, iLen);
                fs.Write(byDataTempBuffer, 0, iLen);
                fs.Close();

                //解析数据
                string strAIOPData = System.Text.Encoding.Default.GetString(byDataTempBuffer);
                m_AIOPData = JsonConvert.DeserializeObject<AIOPData>(strAIOPData);
                //AIOPDatatBox.Text = strAIOPData;

                //AIOPData数据解析
                strShowDataPath = strLocalFilePath + "\\AIOPPollingSnap" + datatimenow + "ShowData.txt";
                AnalysizeAIOPData(m_AIOPData, strShowDataPath);
            }

            //检测图片
            if (struAIOPPollingSnapHead.dwPictureSize > 0 && struAIOPPollingSnapHead.pBufferPicture != null)
            {
                strPicFilePath = strLocalFilePath + "\\AIOPPollingSnap" + datatimenow + "Pic.jpg";

                int iLen = (int)struAIOPPollingSnapHead.dwPictureSize;
                byte[] byDataTempBuffer = new byte[iLen];
                Marshal.Copy(struAIOPPollingSnapHead.pBufferPicture, byDataTempBuffer, 0, iLen);
                Image img = System.Drawing.Image.FromStream(new System.IO.MemoryStream(byDataTempBuffer));
                Bitmap bmpImage = new System.Drawing.Bitmap(img);

                if (m_AIOPData.targets.Length > 0)
                {
                    for (int i = 0; i < m_AIOPData.targets.Length; i++)
                    {
                        if (m_AIOPData.targets[i].obj.rect.w > 0)
                        {
                            Graphics g = Graphics.FromImage(bmpImage);
                            int x = (int)(m_AIOPData.targets[i].obj.rect.w * img.Width);
                            int y = (int)(m_AIOPData.targets[i].obj.rect.w * img.Height);
                            int width = (int)(m_AIOPData.targets[i].obj.rect.w * img.Width);
                            int height = (int)(m_AIOPData.targets[i].obj.rect.w * img.Height);

                            if (x <= img.Width && y <= img.Height && width <= img.Width && height <= img.Height)
                            {
                                //draw rect                             
                                Brush brush = new SolidBrush(Color.Red);
                                Pen pen = new Pen(brush, 3);
                                g.DrawRectangle(pen, new Rectangle(x, y, width, height));

                                string str = "target:" + i;
                                Font font = new Font("粗体", 25);
                                SolidBrush sbrush = new SolidBrush(Color.Black);
                                g.DrawString(str, font, sbrush, new PointF(x, y));
                            }

                            g.Dispose();
                        }
                    }
                }

                bmpImage.Save(strPicFilePath);
                analysisPictureBox.Image = Image.FromFile(strPicFilePath);

                img.Dispose();
                bmpImage.Dispose();

            }

            ListViewItem lvi = new ListViewItem();
            string strAlarmType = "上传轮询抓图图片检测数据";
            lvi.Text = strAlarmType;//报警类型
            lvi.SubItems.Add(struAIOPPollingSnapHead.dwChannel.ToString());//通道号
            lvi.SubItems.Add(strTime);//时间
            lvi.SubItems.Add(struAIOPPollingSnapHead.szTaskID);//任务ID
            lvi.SubItems.Add(strPicFilePath);//检测图片
            lvi.SubItems.Add(strShowDataPath);//设备模型检测数据
            lvi.SubItems.Add(struAIOPPollingSnapHead.szMPID);//模型ID
            lvi.SubItems.Add("--");
            lvi.SubItems.Add("--");
            AlarmInfolistView.Items.Insert(0, lvi);
        }

        //设备支持AI开放平台接入，上传轮询视频检测数据
        private void ProcessCommAlarm_AIOPPollingVideo(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //获取当前系统时间
            string datatimenow = DateTime.Now.ToString("yyyyMMddhhmmssfff");
            //检测图片存储路径
            string strPicFilePath = "";
            //AIOP数据存储路径
            string strAIOPDataPath = "";
            //AIOP展示数据存储路径
            string strShowDataPath = "";
            //AIOP数结构体声明
            AIOPData m_AIOPData = new AIOPData();

            //报警信息结构体&数据转换
            CHCNetSDK.NET_AIOP_POLLING_VIDEO_HEAD struAIOPPollingVideoHead = new CHCNetSDK.NET_AIOP_POLLING_VIDEO_HEAD();
            uint dwSize = (uint)Marshal.SizeOf(struAIOPPollingVideoHead);
            struAIOPPollingVideoHead = (CHCNetSDK.NET_AIOP_POLLING_VIDEO_HEAD)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_AIOP_POLLING_VIDEO_HEAD));

            //拼接报警信息中的时间信息
            string strTime = struAIOPPollingVideoHead.struTime.wYear.ToString() + "-" + struAIOPPollingVideoHead.struTime.wMonth.ToString() + "-" + struAIOPPollingVideoHead.struTime.wDay.ToString() + " " + struAIOPPollingVideoHead.struTime.wHour.ToString() + ":" + struAIOPPollingVideoHead.struTime.wMinute.ToString() + ":" + struAIOPPollingVideoHead.struTime.wSecond.ToString() + ":" + struAIOPPollingVideoHead.struTime.wMilliSec.ToString();

            //AIOPDdata数据
            if (struAIOPPollingVideoHead.dwAIOPDataSize > 0 && struAIOPPollingVideoHead.pBufferAIOPData != null)
            {
                //保存文件到本地
                strAIOPDataPath = strLocalFilePath + "\\AIOPPollingVideo" + datatimenow + "AIOP.txt";
                FileStream fs = new FileStream(strAIOPDataPath, FileMode.Create);
                int iLen = (int)struAIOPPollingVideoHead.dwAIOPDataSize;
                byte[] byDataTempBuffer = new byte[iLen];
                Marshal.Copy(struAIOPPollingVideoHead.pBufferAIOPData, byDataTempBuffer, 0, iLen);
                fs.Write(byDataTempBuffer, 0, iLen);
                fs.Close();

                //解析数据
                string strAIOPData = System.Text.Encoding.Default.GetString(byDataTempBuffer);
                m_AIOPData = JsonConvert.DeserializeObject<AIOPData>(strAIOPData);

                //AOPData数据解析
                strShowDataPath = strLocalFilePath + "\\AIOPPollingVideo" + datatimenow + "ShowData.txt";
                AnalysizeAIOPData(m_AIOPData, strShowDataPath);
            }

            //检测图片
            if (struAIOPPollingVideoHead.dwPictureSize > 0 && struAIOPPollingVideoHead.pBufferPicture != null)
            {
                strPicFilePath = strLocalFilePath + "\\AIOPPollingVideo" + datatimenow + "Pic.jpg";

                int iLen = (int)struAIOPPollingVideoHead.dwPictureSize;
                byte[] byDataTempBuffer = new byte[iLen];
                Marshal.Copy(struAIOPPollingVideoHead.pBufferPicture, byDataTempBuffer, 0, iLen);
                Image img = System.Drawing.Image.FromStream(new System.IO.MemoryStream(byDataTempBuffer));
                Bitmap bmpImage = new System.Drawing.Bitmap(img);

                if (m_AIOPData.targets.Length > 0)
                {
                    for (int i = 0; i < m_AIOPData.targets.Length; i++)
                    {
                        if (m_AIOPData.targets[i].obj.rect.w > 0)
                        {
                            Graphics g = Graphics.FromImage(bmpImage);
                            int x = (int)(m_AIOPData.targets[i].obj.rect.w * img.Width);
                            int y = (int)(m_AIOPData.targets[i].obj.rect.w * img.Height);
                            int width = (int)(m_AIOPData.targets[i].obj.rect.w * img.Width);
                            int height = (int)(m_AIOPData.targets[i].obj.rect.w * img.Height);

                            if (x <= img.Width && y <= img.Height && width <= img.Width && height <= img.Height)
                            {
                                //draw rect                             
                                Brush brush = new SolidBrush(Color.Red);
                                Pen pen = new Pen(brush, 3);
                                g.DrawRectangle(pen, new Rectangle(x, y, width, height));

                                string str = "target:" + i;
                                Font font = new Font("粗体", 25);
                                SolidBrush sbrush = new SolidBrush(Color.Black);
                                g.DrawString(str, font, sbrush, new PointF(x, y));
                            }

                            g.Dispose();
                        }
                    }
                }
                bmpImage.Save(strPicFilePath);
                analysisPictureBox.Image = Image.FromFile(strPicFilePath);
                img.Dispose();
                bmpImage.Dispose();
            }
            ListViewItem lvi = new ListViewItem();
            string strAlarmType = "上传轮询视频检测数据";
            lvi.Text = strAlarmType;//报警类型
            lvi.SubItems.Add(struAIOPPollingVideoHead.dwChannel.ToString());//通道号
            lvi.SubItems.Add(strTime);//时间
            lvi.SubItems.Add(struAIOPPollingVideoHead.szTaskID);//任务ID
            lvi.SubItems.Add(strPicFilePath);//检测图片
            lvi.SubItems.Add(strShowDataPath);//设备模型检测数据
            lvi.SubItems.Add(struAIOPPollingVideoHead.szMPID);//模型ID
            lvi.SubItems.Add("--"); //图片ID
            lvi.SubItems.Add("--"); //状态信息
            AlarmInfolistView.Items.Insert(0, lvi);
        }
        //创建存储字典
        private void GetDictionary()
        {
            string strOutput = string.Empty;
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            if (deviceInfo == null)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "ISAPIDebugTool", "have not selected a device");
                return ;
            }
            string strUrl = "/ISAPI/Intelligent/AIOpenPlatform/algorithmModel/management?format=json";
            //通过透传接口发送请求
            //组装输入
            CHCNetSDK.NET_DVR_XML_CONFIG_INPUT struInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
            struInput.dwSize = (uint)Marshal.SizeOf(struInput);
            string strRequestUrl = "GET" + " " + strUrl;
            IntPtr ptrUrl = Marshal.StringToCoTaskMemAnsi(strRequestUrl);
            struInput.lpRequestUrl = ptrUrl;
            struInput.dwRequestUrlLen = (uint)strRequestUrl.Length;
            struInput.dwRecvTimeOut = 3000;
          
            struInput.lpInBuffer = IntPtr.Zero;
            struInput.dwInBufferSize = 0;
            
            IntPtr ptrInput = Marshal.AllocHGlobal(Marshal.SizeOf(struInput));
            Marshal.StructureToPtr(struInput, ptrInput, false);

            //组装输出
            CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT struOutput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();
            struOutput.dwSize = (uint)Marshal.SizeOf(struOutput);
            const int ciOutSize = 1024 * 1024; //预留1M接收数据
            IntPtr ptrOut = Marshal.AllocHGlobal(ciOutSize);
            struOutput.lpOutBuffer = ptrOut;
            struOutput.dwOutBufferSize = ciOutSize;
            struOutput.lpStatusBuffer = ptrOut;
            struOutput.dwStatusSize = ciOutSize;
            IntPtr ptrOutput = Marshal.AllocHGlobal(Marshal.SizeOf(struOutput));
            Marshal.StructureToPtr(struOutput, ptrOutput, false);
            bool bRet = CHCNetSDK.NET_DVR_STDXMLConfig((int)deviceInfo.lLoginID, ptrInput, ptrOutput);
            if (!bRet)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "ISAPIDebugTool", "NET_DVR_STDXMLConfig failed[" + CHCNetSDK.NET_DVR_GetLastError().ToString() + "]");
                Marshal.FreeHGlobal(ptrInput);
                Marshal.FreeHGlobal(ptrOut);
                Marshal.FreeHGlobal(ptrOutput);
                return ;
            }
            MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "ISAPIDebugTool", "NET_DVR_STDXMLConfig succeed");
            strOutput = Marshal.PtrToStringAnsi(ptrOut);
            Marshal.FreeHGlobal(ptrInput);
            Marshal.FreeHGlobal(ptrOut);
            Marshal.FreeHGlobal(ptrOutput);
            //解析设备本地模型包信息，存储到DataDictionary中
            AIModelInfoList m_AIModelInfoList;
            m_AIModelInfoList = JsonConvert.DeserializeObject<AIModelInfoList>(strOutput);
            try
            {
                for (int i = 0; i < m_AIModelInfoList.AlgorithmModels.Length; i++)
                {
                    //获得MPID；
                    string MPID = m_AIModelInfoList.AlgorithmModels[i].MPID;
                    //对模型包中的description字段解析，并进行存储
                    for (int j = 0; j < m_AIModelInfoList.AlgorithmModels[i].description.Length; j++)
                    {
                        //描述字段的存储，键为所描述类型，值为属性数组
                        Dictionary<string, List<string>> Descibe = new Dictionary<string, List<string>>();
                        //获取模型ID
                        string modeId = m_AIModelInfoList.AlgorithmModels[i].description[j].modelId;
                        //获取标签数组大小，即分类属性大小
                        int length = m_AIModelInfoList.AlgorithmModels[i].description[j].labels.Length;
                        //构建标签数组缓存
                        for (int k = 0; k < length; k++)
                        {
                            //提取单个标签，进行字典存储
                            string SingleLabel = m_AIModelInfoList.AlgorithmModels[i].description[j].labels[k];
                            //保存分类类型，为键
                            string Singlekey = "";
                            //分类属性数组，为值
                            List<string> Singlevalue = new List<string>();
                            //对单个标签的字符串当成字符数组处理
                            for (int n = 0, num = 0; n < SingleLabel.Length; n++)
                            {
                                //若字符串遍历到‘ ’空格，则从下个字符到下个空格之间的字符串为要检查的内容
                                if (SingleLabel[n] == ' ')
                                {
                                    n++;
                                    num++;   //用来判断当前所截取的字符串内容是分类类型还是属性
                                    string ValueBuff = "";
                                    //组合字符为一个字符串
                                    while (n < SingleLabel.Length && SingleLabel[n] != ' ')
                                    {
                                        ValueBuff += SingleLabel[n];
                                        n++;
                                    }
                                    //若截取的字符串不为空
                                    if (ValueBuff != "")
                                    {
                                        if (1 == num) //表示分类类型
                                        {
                                            Singlekey = ValueBuff;
                                        }
                                        else
                                        {
                                            //表示分类属性
                                            int I;
                                            //判断截取内容是否为数字，若不是数字则可判定为分类属性
                                            if (!int.TryParse(ValueBuff, out I))
                                            {
                                                //属性链表增加
                                                Singlevalue.Add(ValueBuff);
                                            }
                                        }
                                    }
                                    n--;
                                }
                            }
                            //描述字典增加
                            Descibe.Add(Singlekey, Singlevalue);
                        }
                        bool SameKey = false;
                        if (DataDictionary.Count != 0)
                        {
                            foreach (string key in DataDictionary.Keys)
                            {
                                //检查该模型是否已经在list结构中
                                if (key == modeId)
                                {
                                    SameKey = true;
                                    break;
                                }
                            }
                        }
                        //对于不同模型包中的相同模型，只存储一个
                        if (!SameKey)
                        {
                            DataDictionary.Add(modeId, Descibe);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            int p = 0;
        }
        //AIOPData解析
        private void AnalysizeAIOPData(AIOPData Data, string SDPath)
        {
            string ShowData = "";  //用于展示的字符串
            string spaces = new string(' ', 4);  //四个空格
            Dictionary<string, List<string>> Descibe = new Dictionary<string, List<string>>();
            for (int i = 0; i < Data.targets.Length; i++)
            {
                //获取上传报警的模型ID
                string modelID = Data.targets[i].obj.modelID;
                //获取报警类型，1表示检测，2表示分类
                int type = Data.targets[i].obj.type;
                string targetType = ""; //检测类型
                if (1 == type)
                {
                    foreach (string keyID in DataDictionary.Keys)
                    {
                        if (keyID == modelID)
                        {
                            //通过模型ID找到描述信息
                            Descibe = DataDictionary[keyID];
                            //获取检测类型，“-1”的原因：数组起始下标为 0，模型描述中检测类型起始下标为 1
                            var buff = Descibe.ElementAt(type - 1);
                            //检测类型对应键
                            targetType = buff.Key;
                        }
                    }
                }
                //分类信息解析
                if (Data.targets[i].properties != null)
                {
                    //单个目标的属性特征
                    List<string> Value = new List<string>();
                    //单个目标具有多个属性特征
                    for (int j = 0; j < Data.targets[i].properties.Length; j++)
                    {
                        //获取模型ID
                        string modelid = Data.targets[i].properties[j].modelID;
                        foreach (string key in DataDictionary.Keys)
                        {
                            if (key == modelid)
                            {
                                //根据模型ID，获取分类类型、分类属性
                                int atrrType = Data.targets[i].properties[j].classify.attrType;
                                int attrValue = Data.targets[i].properties[j].classify.attrValue;

                                //获取对应模型ID的描述信息
                                Descibe = DataDictionary[key];

                                //通过分类类型获取对应的分类属性
                                var buf = Descibe.ElementAt(atrrType);
                                Value.Add(buf.Value[attrValue]);
                                break;
                            }
                        }
                        //string attrConf = Data.targets[i].properties[i].classify.attrConf;
                    }
                    //对目标进行属性归整
                    int confidence = Data.targets[i].obj.confidence;    //分类结果置信度
                    int vaild = Data.targets[i].obj.valid;    //目标是否有效
                    int visible = Data.targets[i].obj.visible;    //目标是否可见

                    //有多个目标情况下，用于展示的数据需将多个目标用换行隔开
                    if (ShowData != "")
                    {
                        ShowData += "\r\n";
                    }
                    ShowData += "target " + i + ":\r\n" + spaces;    //检测结果
                    ShowData += "模型id:" + modelID + "\r\n" + spaces;
                    ShowData += "算法结果返回值:" + type + "\r\n" + spaces;
                    ShowData += "检测目标类型:" + targetType + "\r\n" + spaces;

                    switch (vaild)
                    {
                        case 0: ShowData += "目标是否有效: 否\r\n" + spaces;
                            break;
                        case 1: ShowData += "目标是否有效: 是\r\n" + spaces;
                            break;
                        default:
                            break;
                    }
                    switch (visible)
                    {
                        case 0: ShowData += "目标是否可见: 否\r\n" + spaces;
                            break;
                        case 1: ShowData += "目标是否可见: 是\r\n" + spaces;
                            break;
                        default:
                            break;
                    }
                    //组装目标属性
                    string targetValue = "";
                    foreach (string str in Value)
                    {
                        targetValue += str + " ";
                    }
                    ShowData += "目标属性： " + targetValue + spaces;
                }
            }
            //窗口展示
            AIOPDatatBox.Text = ShowData;

            //展示数据保存
            byte[] SDbyte = System.Text.Encoding.UTF8.GetBytes(ShowData);
            FileStream fs = new FileStream(SDPath, FileMode.Create);
            fs.Write(SDbyte, 0, SDbyte.Length);
            fs.Close();
        }

        //撤防
        private void UNGuardBtn_Click(object sender, EventArgs e)
        {
            SDK_alarmUNGuard();
        }

        //私有协议-撤防
        public void SDK_alarmUNGuard()
        {
            //获取设备信息
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            if (m_lFortifyHandle != -1)
            {
                CHCNetSDK.NET_DVR_CloseAlarmChan_V30(m_lFortifyHandle);
                m_lFortifyHandle = -1;
            }
        }
        //历史记录查看
        private void AlarmInfolistView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string PicPath = AlarmInfolistView.SelectedItems[0].SubItems[4].Text;
            string TextPath = AlarmInfolistView.SelectedItems[0].SubItems[5].Text;

            if (PicPath != "" && TextPath != "")
            {
                string text = File.ReadAllText(@TextPath);
                analysisPictureBox.Image = Image.FromFile(PicPath);
                AIOPDatatBox.Text = text;
            }
        }
    }
}
