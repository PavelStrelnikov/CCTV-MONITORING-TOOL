using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TINYXMLTRANS;

namespace SDKThermometry
{
    public partial class FormCorrectThermometry : Form
    {
        public int m_iChannel = -1;
        public int m_lUserID = -1;
        private string m_strFilePath = "";
        string[] m_strTempPointList = new string[4];
        bool m_bSetCorrectTherm = false;
        private CHCNetSDK.NET_DVR_XML_CONFIG_INPUT struXMLConfigInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
        private CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT struXMLConfigOutput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();

        public FormCorrectThermometry()
        {
            InitializeComponent();
        }

        private void btnGetCorrectTherm_Click(object sender, EventArgs e)
        {
            int iInSize = Marshal.SizeOf(struXMLConfigInput);
            struXMLConfigInput.dwSize = (uint)iInSize;
            string strRequestUrl = "GET /ISAPI/Thermal/channels/" + m_iChannel + "/CorrectionParam\r\n";
            uint dwRequestUrlLen = (uint)strRequestUrl.Length;
            struXMLConfigInput.lpRequestUrl = Marshal.StringToHGlobalAnsi(strRequestUrl);
            struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            IntPtr lpInputParam = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(struXMLConfigInput, lpInputParam, false);

            int iOutSize = Marshal.SizeOf(struXMLConfigInput);
            struXMLConfigOutput.dwSize = (uint)iOutSize;
            struXMLConfigOutput.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.MAX_LEN_XML);
            struXMLConfigOutput.dwOutBufferSize = CHCNetSDK.MAX_LEN_XML;
            IntPtr lpOutputParam = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(struXMLConfigOutput, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(m_lUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Thermal: 获取测温矫正参数，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                string strOutputParam = Marshal.PtrToStringAnsi(struXMLConfigOutput.lpOutBuffer);

                //保存XML体
                //FileStream fs = new FileStream("E:\\文档\\strOutputParam.xml", FileMode.Create, FileAccess.Write, FileShare.Write);
                //int iLen = strOutputParam.Length;
                //byte[] by = new byte[iLen];
                //by = System.Text.Encoding.Default.GetBytes(strOutputParam);
                //fs.Write(by, 0, iLen);
                //fs.Close();

                CorrectThermXML(strOutputParam);
            }

            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);
        }

        private bool CorrectThermXML(string strCorrectThermXML)
        {
            if (strCorrectThermXML != "")
            {
                CTinyXmlTrans XMLBASE = new CTinyXmlTrans();
                XMLBASE.Parse(strCorrectThermXML);
                XMLBASE.SetRoot();

                if (XMLBASE.FindElemFromBegin("CorrectionParam") && XMLBASE.IntoElem())
                {
                    if (XMLBASE.FindElemFromBegin("distance"))
                    {
                        textBoxDistance.Text = XMLBASE.GetData();
                    }
                    if (XMLBASE.FindElemFromBegin("enviroTemperature"))
                    {
                        textBoxEnviroTemperature.Text = XMLBASE.GetData();
                    }
                    if (XMLBASE.FindElemFromBegin("emissivity"))
                    {
                        textBoxEmissivity.Text = XMLBASE.GetData();
                    }

                    if (XMLBASE.FindElemFromBegin("TemperaturePointList") && XMLBASE.IntoElem())
                    {
                        int i = 0;
                        //解析温度点
                        while (XMLBASE.FindElem("TemperaturePoint") && XMLBASE.IntoElem())
                        {
                            if (XMLBASE.FindElem("id"))
                            {
                                string strID = "" + (++i);
                                if (strID == XMLBASE.GetData())
                                {
                                    if (XMLBASE.FindElem("presetTemperature"))
                                    {
                                        m_strTempPointList[i - 1] = XMLBASE.GetData();

                                        //解析温度点坐标
                                        if (XMLBASE.FindElem("Coordinates") && XMLBASE.IntoElem())
                                        {
                                            if (XMLBASE.FindElem("x"))
                                            {
                                                textBoxTemperaturePointX.Text = XMLBASE.GetData();
                                            }
                                            if (XMLBASE.FindElem("y"))
                                            {
                                                textBoxTemperaturePointY.Text = XMLBASE.GetData();
                                            }
                                            XMLBASE.OutOfElem();
                                        }
                                    }
                                }
                            }

                            XMLBASE.OutOfElem();
                            if (!XMLBASE.NextSibElem())
                            {
                                break;
                            }
                        }

                        textBoxTemperaturePointOne.Text = m_strTempPointList[0];
                        textBoxTemperaturePointTwo.Text = m_strTempPointList[1];
                        textBoxTemperaturePointThird.Text = m_strTempPointList[2];
                        textBoxTemperaturePointFour.Text = m_strTempPointList[3];
                    }
                }
                else
                {
                    MessageBox.Show("CorrectionParam：不存在");
                    return false;
                }
            }
            else
            {
                MessageBox.Show("XML：空");
                return false;
            }

            return true;
        }

        private bool SetCorrectTempPoint(int iPoint)
        {
            string strDistance = "<distance>" + textBoxDistance.Text + "</distance>\r\n";
            string strEnviroTemperature = "<enviroTemperature>" + textBoxEnviroTemperature.Text + "</enviroTemperature>\r\n";
            string strEmissivity = "<emissivity>" + textBoxEmissivity.Text + "</emissivity>\r\n";
            string strId = "<id>" + (iPoint + 1) + "</id>\r\n";
            string strPresetTemperature = "<presetTemperature>" + m_strTempPointList[iPoint] + "</presetTemperature>\r\n";
            string strCoordinates = "<Coordinates>\r\n<x>" + textBoxTemperaturePointX.Text + "</x>\r\n<y>" + textBoxTemperaturePointY.Text + "</y>\r\n</Coordinates>\r\n";
            string strTemperaturePoint = "<TemperaturePoint>\r\n" + strId + strPresetTemperature + strCoordinates + "</TemperaturePoint>\r\n";
            string strTemperaturePointList = "<TemperaturePointList>\r\n" + strTemperaturePoint + "</TemperaturePointList>\r\n";
            string strInput = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<CorrectionParam version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n" +
                             strDistance + strEnviroTemperature + strEmissivity + strTemperaturePointList + "</CorrectionParam>\r\n";

            int iInSize = Marshal.SizeOf(struXMLConfigInput);
            struXMLConfigInput.dwSize = (uint)iInSize;
            string strRequestUrl = "PUT /ISAPI/Thermal/channels/" + m_iChannel + "/CorrectionParam\r\n";
            uint dwRequestUrlLen = (uint)strRequestUrl.Length;
            struXMLConfigInput.lpRequestUrl = Marshal.StringToHGlobalAnsi(strRequestUrl);
            struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            struXMLConfigInput.lpInBuffer = Marshal.StringToCoTaskMemAnsi(strInput);
            struXMLConfigInput.dwInBufferSize = (uint)strInput.Length;
            struXMLConfigInput.dwRecvTimeOut = 30000; //0秒超时,设置温度点设备返回比较慢，在15秒左右
            IntPtr lpInputParam = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(struXMLConfigInput, lpInputParam, false);

            int iOutSize = Marshal.SizeOf(struXMLConfigOutput);
            struXMLConfigOutput.dwSize = (uint)iOutSize;
            struXMLConfigOutput.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.MAX_LEN_XML);
            struXMLConfigOutput.dwOutBufferSize = (uint)Marshal.SizeOf(CHCNetSDK.MAX_LEN_XML);
            IntPtr lpOutputParam = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(struXMLConfigOutput, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(m_lUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Thermal: 设置测温矫正参数，错误码：" + iLastErr; ;
                MessageBox.Show(strErr);

                m_bSetCorrectTherm = false;
            }
            else
            {
                m_bSetCorrectTherm = true;
            }

            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);

            return m_bSetCorrectTherm;
        }

        private void btnSetCorrectTempPointOne_Click(object sender, EventArgs e)
        {

            checkBoxPointOne.Checked = SetCorrectTempPoint(0);
        }

        private void btnSetCorrectTempPointTwo_Click(object sender, EventArgs e)
        {
            checkBoxPointTwo.Checked = SetCorrectTempPoint(1);
        }

        private void btnSetCorrectTempPointThird_Click(object sender, EventArgs e)
        {
            checkBoxPointThird.Checked = SetCorrectTempPoint(2);
        }

        private void btnSetCorrectTempPointFour_Click(object sender, EventArgs e)
        {
            checkBoxPointFour.Checked = SetCorrectTempPoint(3);
        }

        private void btnGetFilePath_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "C# Corner Open File Dialog";
            fdlg.Filter = "All files（*.*）|*.*|All files(*.*)|*.* ";
            fdlg.RestoreDirectory = false;

            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                m_strFilePath = System.IO.Path.GetFullPath(fdlg.FileName);
                textBoxFilePath.Text = m_strFilePath;
            }
        }

        private void btnPostCorrectTherm_Click(object sender, EventArgs e)
        {
            FileStream fileStream = new FileStream(m_strFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] bytes = new byte[fileStream.Length];   // 读取文件的 byte[]   
            fileStream.Read(bytes, 0, bytes.Length);
            fileStream.Close();

            int iInSize = Marshal.SizeOf(struXMLConfigInput);
            struXMLConfigInput.dwSize = (uint)iInSize;
            string strRequestUrl = "PUT /ISAPI/Thermal/channels/" + m_iChannel + "/CorrectionParam/import\r\n";
            uint dwRequestUrlLen = (uint)strRequestUrl.Length;
            struXMLConfigInput.lpRequestUrl = Marshal.StringToHGlobalAnsi(strRequestUrl);
            struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            struXMLConfigInput.dwInBufferSize = (uint)bytes.Length;
            struXMLConfigInput.lpInBuffer = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, struXMLConfigInput.lpInBuffer, bytes.Length);
            IntPtr lpInputParam = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(struXMLConfigInput, lpInputParam, false);

            int iOutSize = Marshal.SizeOf(struXMLConfigOutput);
            struXMLConfigOutput.dwSize = (uint)iOutSize;
            struXMLConfigOutput.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.MAX_LEN_XML);
            struXMLConfigOutput.dwOutBufferSize = (uint)Marshal.SizeOf(CHCNetSDK.MAX_LEN_XML);
            IntPtr lpOutputParam = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(struXMLConfigOutput, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(m_lUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Thermal: 导入测温矫正参数，错误码：" + iLastErr; ;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("导入矫正参数：成功");
            }

            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);

            btnStartCorrectTherm.Enabled = false;
        }

        private void btnStartCorrectTherm_Click(object sender, EventArgs e)
        {
            int iInSize = Marshal.SizeOf(struXMLConfigInput);
            struXMLConfigInput.dwSize = (uint)iInSize;
            string strRequestUrl = "PUT /ISAPI/Thermal/channels/" + m_iChannel + "//Correction\r\n";
            uint dwRequestUrlLen = (uint)strRequestUrl.Length;
            struXMLConfigInput.lpRequestUrl = Marshal.StringToHGlobalAnsi(strRequestUrl);
            struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            IntPtr lpInputParam = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(struXMLConfigInput, lpInputParam, false);

            int iOutSize = Marshal.SizeOf(struXMLConfigInput);
            struXMLConfigOutput.dwSize = (uint)iOutSize;
            struXMLConfigOutput.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.MAX_LEN_XML);
            struXMLConfigOutput.dwOutBufferSize = (uint)Marshal.SizeOf(CHCNetSDK.MAX_LEN_XML);
            IntPtr lpOutputParam = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(struXMLConfigOutput, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(m_lUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Thermal: 开始测温矫正，错误码：" + iLastErr; ;
                MessageBox.Show(strErr);
            }

            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);
        }


    }
}
