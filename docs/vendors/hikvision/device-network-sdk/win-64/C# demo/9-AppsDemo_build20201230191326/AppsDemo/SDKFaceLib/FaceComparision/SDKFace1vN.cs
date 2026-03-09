using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using Common;
using Common.Head;

namespace SDKFaceLib
{
    public partial class Face1vN
    {
        public class jsonTargetPicture
        {
            public string picData { get; set; }
        }
        public class jsonComparisionSearch
        {
            public string dataType { get; set; }
            public jsonTargetPicture TargetPicture { get; set; }
        }
        public class jsonPictureCaptureComparision
        {
            public jsonComparisionSearch ComparisionSearch { get; set; }

        }
        jsonPictureCaptureComparision m_jsonPictureCaptureComparision;
        public class jsonComparisionResultInfo
        {
            public bool status { get; set; }
            public int similarity { get; set; }
        }
        public class jsonComparisionResult
        {
            public jsonComparisionResultInfo ComparisionResult { get; set; }
        }
        jsonComparisionResult m_jsonComparisionResult;
        private bool CompareFace()
        {
            m_jsonPictureCaptureComparision = new jsonPictureCaptureComparision();
            m_jsonComparisionResult = new jsonComparisionResult();
            m_jsonComparisionResult.ComparisionResult = new jsonComparisionResultInfo();

            string inurl = "POST /ISAPI/Intelligent/channels/"+ m_textBoxChannelNo.Text +"/pictureCaptureComparision/face/result?format=json";
            IntPtr ptrinurl;
            IntPtr m_pIntBuf;
            IntPtr m_pOutBuf;
            ptrinurl = Marshal.StringToCoTaskMemAnsi(inurl);
            m_pOutBuf = Marshal.AllocHGlobal((Int32)XML_ABILITY_OUT_LEN);
            CHCNetSDK.NET_DVR_XML_CONFIG_INPUT struInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
            CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT struOuput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();
            struInput.dwSize = (uint)Marshal.SizeOf(struInput);
            struInput.lpRequestUrl = ptrinurl;
            struInput.dwRequestUrlLen = (uint)inurl.Length;
            struInput.dwRecvTimeOut = 10000;

            m_jsonPictureCaptureComparision.ComparisionSearch = new jsonComparisionSearch();
            m_jsonPictureCaptureComparision.ComparisionSearch.dataType = "binary";

            FileStream file = new FileStream(m_textBoxPicturePath.Text, FileMode.Open);
            if (file.Length <= 0)
            {
                return false;
            }
            byte[] byData = new byte[file.Length];
            file.Seek(0, SeekOrigin.Begin);
            file.Read(byData, 0, (int)file.Length);
            file.Close();

            string strData = Convert.ToBase64String(byData, 0, byData.Length);

            m_jsonPictureCaptureComparision.ComparisionSearch.TargetPicture = new jsonTargetPicture();
            m_jsonPictureCaptureComparision.ComparisionSearch.TargetPicture.picData = strData;

            string pInBuf = JsonConvert.SerializeObject(m_jsonPictureCaptureComparision);
            m_pIntBuf = Marshal.StringToCoTaskMemAnsi(pInBuf);

            struInput.lpInBuffer = m_pIntBuf;
            struInput.dwInBufferSize = (uint)pInBuf.Length;

            struOuput.dwSize = (uint)Marshal.SizeOf(struOuput);
            struOuput.lpOutBuffer = m_pOutBuf;
            struOuput.dwOutBufferSize = XML_ABILITY_OUT_LEN;

            IntPtr ptrInput = Marshal.AllocHGlobal(Marshal.SizeOf(struInput));
            Marshal.StructureToPtr(struInput, ptrInput, false);
            IntPtr ptrOuput = Marshal.AllocHGlobal(Marshal.SizeOf(struOuput));
            Marshal.StructureToPtr(struOuput, ptrOuput, false);
            m_dwReturnValue = CHCNetSDK.NET_DVR_STDXMLConfig(m_lServerID, ptrInput, ptrOuput);

            try
            {
                if (m_dwReturnValue)
                {
                    string pOutBuf = Marshal.PtrToStringAnsi(m_pOutBuf, (int)XML_ABILITY_OUT_LEN);

                    m_jsonComparisionResult = JsonConvert.DeserializeObject<jsonComparisionResult>(pOutBuf);

                    if (m_jsonComparisionResult.ComparisionResult.status)
                    {
                        m_textBoxStatus.Text = "true";
                    }
                    else
                    {
                        m_textBoxStatus.Text = "false";
                    }

                    m_textBoxSimilarity.Text = m_jsonComparisionResult.ComparisionResult.similarity.ToString();

                    return true;
                }
                else
                {
                    string msg = "Operate failure" + CHCNetSDK.NET_DVR_GetLastError();
                    MessageBox.Show(msg);
                    return false;
                }

            }
            catch (Exception)
            {
                string msg = "The return message is incorrect";
                MessageBox.Show(msg);
                return false;
            }
            finally
            {
                Marshal.FreeHGlobal(ptrInput);
                Marshal.FreeHGlobal(m_pOutBuf);
                Marshal.FreeHGlobal(ptrOuput);
            }

        }
    }
}
