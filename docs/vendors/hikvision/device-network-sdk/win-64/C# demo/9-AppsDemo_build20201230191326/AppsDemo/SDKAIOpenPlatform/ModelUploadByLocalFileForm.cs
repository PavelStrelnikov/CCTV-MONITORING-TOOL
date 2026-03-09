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
using Common;
using System.IO;
using System.Runtime.InteropServices;

namespace SDKAIOpenPlatform
{
    public partial class ModelUploadByLocalFileForm : Form
    {
        public ModelUploadByLocalFileForm()
        {
            InitializeComponent();
        }

        //设备树对象映射
        IDeviceTree m_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        //文件上传标识
        private bool m_bUpLoading = false;

        //选择参数文件路径
        private void UploadParamFileBtn_Click(object sender, EventArgs e)
        {
            if (paramOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                ParamFilePathtBox.Text = paramOpenFileDialog.FileName;
            }
        }

        //选择模型文件路径
        private void UploadModelFileBtn_Click(object sender, EventArgs e)
        {
            if (modelOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                ModelFilePathtBox.Text = modelOpenFileDialog.FileName;
            }
        }

        public class OriginalParameter
        {
            public int dataSetId {get ; set;}
            public List<description> description { get; set; }
            public string key { get; set; }
            public string modelUrl { get; set; }
            public string trainModelId { get; set; }
            public int transfromType { get; set; }
            public string version { get; set; }
            public string versionId { get; set; }
        }
        public class NewParameter
        {
            public string MPID { get; set; }
            public string MPName { get; set; }
            public List<description> description { get; set; }
        }

        public class MPParameter
        {
            public string MPID { get; set; }
            public string MPName { get; set; }
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
            public int lable { get; set; }
            public string modelId { get; set; }
        }
        private void ModelUploadBtn_Click(object sender, EventArgs e)
        {
            //预设推送模式，上传文件
            modelUploadByLocalFile();
        }
        //预设推送模式，上传文件
        public void modelUploadByLocalFile()
        {
            SDK_modelUploadByLocalFile();
        }

        //预设推送模式 SDK私有协议实现
        public void SDK_modelUploadByLocalFile()
        {
            string paraFilePath = ParamFilePathtBox.Text.ToString();
            string modelFilePath = ModelFilePathtBox.Text.ToString();

            //上传文件句柄
            int m_iUploadHandle = -1;
            //获取设备信息
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            IntPtr ptrInput = IntPtr.Zero;
            try
            {
                if (!m_bUpLoading)
                {
                    //读取参数文件
                    FileStream fs = File.OpenRead(paraFilePath); //OpenRead
                    int filelength = 0;
                    filelength = (int)fs.Length; //Get file length 
                    byte[] modelFile = new byte[filelength]; //Create a byte array
                    fs.Read(modelFile, 0, filelength); //Read by byte stream
                    fs.Close();

                    //对原始的参数文件做转换，用户可手动输入MPID和MPName
                    string strDescrption = System.Text.Encoding.UTF8.GetString(modelFile);
                    int dstart = strDescrption.IndexOf("description") - 1;
                    int dend = strDescrption.LastIndexOf("]")+1;
                    string description = strDescrption.Substring(dstart, dend-dstart);
                    MPParameter mparam = new MPParameter();
                    mparam.MPID = MPIDtextBox.Text.ToString();
                    mparam.MPName = MPNametextBox.Text.ToString();
                    string modelDestr = JsonConvert.SerializeObject(mparam);
                    int mend = modelDestr.LastIndexOf("}");
                    modelDestr = modelDestr.Insert(mend, ",");
                    modelDestr=modelDestr.Insert(mend+1, description);

                    JsonSerializer serializer = new JsonSerializer();
                    TextReader tr = new StringReader(modelDestr);
                    JsonTextReader jtr = new JsonTextReader(tr);
                    object obj = serializer.Deserialize(jtr);
                    if (obj != null)
                    {
                        StringWriter textWriter = new StringWriter();
                        JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                        {
                            Formatting = Formatting.Indented,
                            Indentation = 4,
                            IndentChar = ' '
                        };
                        serializer.Serialize(jsonWriter, obj);
                        modelDestr=textWriter.ToString();
                    }


                    byte[] modelPara = modelPara=System.Text.Encoding.UTF8.GetBytes(modelDestr);
                                       
                    //上传文件
                    CHCNetSDK.NET_DVR_AI_ALGORITHM_MODEL struAIAlgorithmModel = new CHCNetSDK.NET_DVR_AI_ALGORITHM_MODEL();
                    struAIAlgorithmModel.dwSize = (uint)Marshal.SizeOf(struAIAlgorithmModel);
                    struAIAlgorithmModel.dwDescribeLength = (uint)modelPara.Length;

                    IntPtr buffer = Marshal.AllocHGlobal(modelPara.Length);
                    Marshal.Copy(modelPara, 0, buffer, modelPara.Length);
                    struAIAlgorithmModel.pDescribeBuffer = buffer;

                    ptrInput = Marshal.AllocHGlobal(Marshal.SizeOf(struAIAlgorithmModel));
                    Marshal.StructureToPtr(struAIAlgorithmModel, ptrInput, false);

                    m_iUploadHandle = CHCNetSDK.NET_DVR_UploadFile_V40((int)deviceInfo.lLoginID, CHCNetSDK.UPLOAD_AI_ALGORITHM_MODEL, ptrInput,
                        (uint)Marshal.SizeOf(struAIAlgorithmModel), modelFilePath, IntPtr.Zero, 0);

                    if (m_iUploadHandle < 0)
                    {
                        int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                        string strErr = "上传失败，错误码：" + iLastErr;
                        MessageBox.Show(strErr, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        CHCNetSDK.NET_DVR_StopUploadFile(m_iUploadHandle);

                        return;
                    }
                    else
                    {
                        int dwProgress = 0;
                        int dwState = 0;

                        IntPtr pProgress = Marshal.AllocHGlobal(Marshal.SizeOf(dwProgress));
                        Marshal.WriteInt32(pProgress, dwProgress);

                        while (true)
                        {
                            dwState = CHCNetSDK.NET_DVR_GetUploadState(m_iUploadHandle, pProgress);
                            dwProgress = Marshal.ReadInt32(pProgress);

                            if (dwState == 1)
                            {
                                uploadstateLab.Text = "上传成功！";
                                m_bUpLoading = false;
                                break;
                            }
                            else if (dwState == 2)
                            {
                                uploadstateLab.Text = "正在上传,已上传: " + dwProgress;
                                uploadstateLab.Update();
                            }
                            else if (dwState == 3)
                            {
                                uploadstateLab.Text = "上传失败！";
                                break;
                            }
                            else if (dwState == 4)
                            {
                                if (dwProgress == 100)
                                {
                                    uploadstateLab.Text = "上传成功！";
                                    m_bUpLoading = false;
                                    break;
                                }
                                else
                                {
                                    uploadstateLab.Text = "网络断开，状态未知";
                                    break;
                                }
                            }

                            if (dwState != 2 && dwState != 5)
                            {
                                CHCNetSDK.NET_DVR_UploadClose(m_iUploadHandle);   // break已经跳出循环，会执行到这儿？
                                m_bUpLoading = true;
                                ModelUploadBtn.Text = "停止上传";
                            }
                        }   //结束上传的过程

                    }
                }
                else
                {
                    CHCNetSDK.NET_DVR_UploadClose(m_iUploadHandle);
                    m_bUpLoading = false;
                    ModelUploadBtn.Text = "上传";
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
