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
    public partial class FormThermalStream : Form
    {
        public int m_iChannel = -1;
        public int m_lUserID = -1;
        private CHCNetSDK.NET_DVR_STD_CONFIG m_struSTDConfig = new CHCNetSDK.NET_DVR_STD_CONFIG();
        private CHCNetSDK.NET_DVR_BAREDATAOVERLAY_CFG m_struBaredataOverlay = new CHCNetSDK.NET_DVR_BAREDATAOVERLAY_CFG();
        private CHCNetSDK.NET_DVR_XML_CONFIG_INPUT struXMLConfigInput;
        private CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT struXMLConfigOutput;
         
        public class CTemperatureLimit
        {
            public float fmaxTemperature { get; set; }
            public float fminTemperature { get; set; }
        }

        public class CColor
        {
            public int nColorR { get; set; }
            public int nColorG { get; set; }
            public int nColorB { get; set; }
        }

        public class CColorateTargetMode
        {
            public int nId { get; set; }
            public string strMode { get; set; }
            public bool bEnabled { get; set; }
            public CTemperatureLimit TemperatureLimit { get; set; }
            public CColor Color { get; set; }
        }

        public class CColorateTarget
        {
            public CColorateTargetMode ColorateTargetMode { get; set; }
        }
        CColorateTarget[] ColorateTarget;
        public FormThermalStream()
        {
            InitializeComponent();

            ColorateTarget = new CColorateTarget[3];
            for (int i = 0; i < 3; i++)
            {
                ColorateTarget[i] = new CColorateTarget();
                ColorateTarget[i].ColorateTargetMode = new CColorateTargetMode();
                ColorateTarget[i].ColorateTargetMode.TemperatureLimit = new CTemperatureLimit();
                ColorateTarget[i].ColorateTargetMode.Color = new CColor();
            }

            comboBoxDistanceUnit.SelectedIndex = 0;
            comboBoxVideoCodingType.SelectedIndex = 0;
            comboBoxThermalDataConv.SelectedIndex = 0;
        }

        private void btnGetBaredataOverlay_Click(object sender, EventArgs e)
        {
            m_struSTDConfig.lpCondBuffer = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(m_struSTDConfig.lpCondBuffer, m_iChannel);
            m_struSTDConfig.dwCondSize = sizeof(int);
            m_struSTDConfig.lpInBuffer = IntPtr.Zero;
            m_struSTDConfig.dwInSize = 0;
            m_struBaredataOverlay.dwSize = (uint)Marshal.SizeOf(m_struBaredataOverlay);
            IntPtr ptrOutBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(m_struBaredataOverlay));
            Marshal.StructureToPtr(m_struBaredataOverlay, ptrOutBuffer, false);
            m_struSTDConfig.lpOutBuffer = ptrOutBuffer;
            m_struSTDConfig.dwOutSize = (uint)Marshal.SizeOf(m_struBaredataOverlay);

            m_struSTDConfig.lpStatusBuffer = Marshal.AllocHGlobal(CHCNetSDK.XML_ABILITY_OUT_LEN);
            m_struSTDConfig.dwStatusSize = CHCNetSDK.XML_ABILITY_OUT_LEN;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_GetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_BAREDATAOVERLAY_CFG, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：获取热成像裸数据叠加，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                m_struSTDConfig = (CHCNetSDK.NET_DVR_STD_CONFIG)Marshal.PtrToStructure(ptr, typeof(CHCNetSDK.NET_DVR_STD_CONFIG));
                m_struBaredataOverlay = (CHCNetSDK.NET_DVR_BAREDATAOVERLAY_CFG)Marshal.PtrToStructure(m_struSTDConfig.lpOutBuffer, typeof(CHCNetSDK.NET_DVR_BAREDATAOVERLAY_CFG));

                checkBoxBaredataOverlay.Checked = Convert.ToBoolean(m_struBaredataOverlay.byEnable);
                textBoxIntervalTime.Text = m_struBaredataOverlay.byIntervalTime.ToString();
            }

            Marshal.FreeHGlobal(ptrOutBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        private void btnSetBaredataOverlay_Click(object sender, EventArgs e)
        {
            m_struSTDConfig.lpCondBuffer = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(m_struSTDConfig.lpCondBuffer, m_iChannel);
            m_struSTDConfig.dwCondSize = sizeof(int);

            m_struBaredataOverlay.byEnable = Convert.ToByte(checkBoxBaredataOverlay.Checked);
            m_struBaredataOverlay.byIntervalTime = byte.Parse(textBoxIntervalTime.Text);

            IntPtr ptrInBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(m_struBaredataOverlay));
            Marshal.StructureToPtr(m_struBaredataOverlay, ptrInBuffer, false);
            m_struSTDConfig.lpInBuffer = ptrInBuffer;
            m_struSTDConfig.dwInSize = (uint)Marshal.SizeOf(m_struBaredataOverlay);

            m_struSTDConfig.lpOutBuffer = IntPtr.Zero;
            m_struSTDConfig.dwOutSize = 0;
            m_struSTDConfig.lpStatusBuffer = Marshal.AllocHGlobal(CHCNetSDK.XML_ABILITY_OUT_LEN);
            m_struSTDConfig.dwStatusSize = CHCNetSDK.XML_ABILITY_OUT_LEN;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_SetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_BAREDATAOVERLAY_CFG, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：设置热成像裸数据叠加，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("设置热成像裸数据叠加，成功！");
            }

            Marshal.FreeHGlobal(ptrInBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        private void btnGetPixelToPixelParam_Click(object sender, EventArgs e)
        {
            struXMLConfigInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
            struXMLConfigOutput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();

            int iInSize = Marshal.SizeOf(struXMLConfigInput);
            struXMLConfigInput.dwSize = (uint)iInSize;
            string strRequestUrl = "GET /ISAPI/Thermal/channels/" + m_iChannel + "/thermometry/pixelToPixelParam\r\n";
            uint dwRequestUrlLen = (uint)strRequestUrl.Length;
            struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            struXMLConfigInput.lpRequestUrl = Marshal.StringToHGlobalAnsi(strRequestUrl);
            IntPtr lpInputParam = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(struXMLConfigInput, lpInputParam, false);

            int iOutSize = Marshal.SizeOf(struXMLConfigInput);
            struXMLConfigOutput.dwSize = (uint)iOutSize;
            struXMLConfigOutput.dwOutBufferSize = CHCNetSDK.MAX_LEN_XML;
            struXMLConfigOutput.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.MAX_LEN_XML);
            IntPtr lpOutputParam = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(struXMLConfigOutput, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(m_lUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Thermal: 获取全屏测温配置参数，错误码：" + iLastErr;
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

            Marshal.FreeHGlobal(struXMLConfigInput.lpRequestUrl);
            Marshal.FreeHGlobal(struXMLConfigOutput.lpOutBuffer);
            Marshal.FreeHGlobal(struXMLConfigOutput.lpStatusBuffer);
        }

        private bool CorrectThermXML(string strCorrectThermXML)
        {
            if (strCorrectThermXML != "")
            {
                CTinyXmlTrans XMLBASE = new CTinyXmlTrans();
                XMLBASE.Parse(strCorrectThermXML);
                XMLBASE.SetRoot();

                if (XMLBASE.FindElemFromBegin("PixelToPixelParam") && XMLBASE.IntoElem())
                {
                    if (XMLBASE.FindElem("maxFrameRate"))
                    {
                        textBoxMaxFrameRate.Text = XMLBASE.GetData();
                    }
                    if (XMLBASE.FindElem("reflectiveEnable"))
                    {
                        checkBoxReflectiveEnable.Checked = Convert.ToBoolean(XMLBASE.GetData());
                    }
                    if (XMLBASE.FindElem("reflectiveTemperature"))
                    {
                        textBoxReflectiveTemperature.Text = XMLBASE.GetData();
                    }
                    if (XMLBASE.FindElem("emissivity"))
                    {
                        textBoxEmissivity.Text = XMLBASE.GetData();
                    }
                    if (XMLBASE.FindElem("distance"))
                    {
                        textBoxDistance.Text = XMLBASE.GetData();
                    }
                    if (XMLBASE.FindElem("refreshInterval"))
                    {
                        textBoxRefreshInterval.Text = XMLBASE.GetData();
                    }
                    if (XMLBASE.FindElem("distanceUnit"))
                    {
                        string strDistanceUint = XMLBASE.GetData();
                        if ("meter" == strDistanceUint)
                        {
                        comboBoxDistanceUnit.SelectedIndex = 0;
                        }
                        if ("feet" == strDistanceUint)
                        {
                            comboBoxDistanceUnit.SelectedIndex = 1;
                        }
                        if ("centimeter" == strDistanceUint)
                        {
                            comboBoxDistanceUnit.SelectedIndex = 2;
                        }
                    }

                }
                else
                {
                    MessageBox.Show("PixelToPixelParam：不存在");
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

        private void btnSerPixelToPixelParam_Click(object sender, EventArgs e)
        {
            struXMLConfigInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
            struXMLConfigOutput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();

            string strId = "<id>" + m_iChannel + "</id>\r\n";
            string strMaxFrameRate = "<maxFrameRate>" + textBoxMaxFrameRate.Text + "</maxFrameRate>\r\n";
            string strReflectiveEnable = "false";
            if (checkBoxReflectiveEnable.Checked)
            {
                strReflectiveEnable = "true";
            }
            strReflectiveEnable = "<reflectiveEnable>" + strReflectiveEnable + "</reflectiveEnable>\r\n";
            string strReflectiveTemperature = "<reflectiveTemperature>" + textBoxReflectiveTemperature.Text + "</reflectiveTemperature>\r\n";
            string strEmissivity = "<emissivity>" + textBoxEmissivity.Text + "</emissivity>\r\n";
            string strDistance = "<distance>" + textBoxDistance.Text + "</distance>\r\n";
            string strRefreshInterval = "<refreshInterval>" + textBoxRefreshInterval.Text + "</refreshInterval>\r\n";
            string strUint = "";
            if (0 == comboBoxDistanceUnit.SelectedIndex)
            {
                strUint = "meter";
            }
            if (1 == comboBoxDistanceUnit.SelectedIndex)
            {
                strUint = "feet";
            }
            if (2 == comboBoxDistanceUnit.SelectedIndex)
            {
                strUint = "centimeter";
            }

            string strDistanceUnit = "<distanceUnit>" + strUint + "</distanceUnit>\r\n";
            string strDataLength = "<temperatureDataLength>" + 4 + "</temperatureDataLength>\r\n";

            string strInput = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<PixelToPixelParam version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n" +
                             strId + strMaxFrameRate + strReflectiveEnable + strReflectiveTemperature + strEmissivity +
                             strDistance + strRefreshInterval + strDistanceUnit + strDataLength + "</PixelToPixelParam>\r\n";

            int iInSize = Marshal.SizeOf(struXMLConfigInput);
            struXMLConfigInput.dwSize = (uint)iInSize;
            string strRequestUrl = "PUT /ISAPI/Thermal/channels/" + m_iChannel + "/thermometry/pixelToPixelParam\r\n";
            uint dwRequestUrlLen = (uint)strRequestUrl.Length;
            struXMLConfigInput.lpRequestUrl = Marshal.StringToHGlobalAnsi(strRequestUrl);
            struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            struXMLConfigInput.dwInBufferSize = (uint)strInput.Length;
            struXMLConfigInput.lpInBuffer = Marshal.StringToCoTaskMemAnsi(strInput);
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
                string strErr = "Thermal: 设置全屏测温配置参数，错误码：" + iLastErr; ;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("设置全屏测温参数：成功");
            }

            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);
        }

        private void btnGetStreamParam_Click(object sender, EventArgs e)
        {
            struXMLConfigInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
            struXMLConfigOutput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();

            int iInSize = Marshal.SizeOf(struXMLConfigInput);
            struXMLConfigInput.dwSize = (uint)iInSize;
            string strRequestUrl = "GET /ISAPI/Thermal/channels/" + m_iChannel + "/streamParam\r\n";
            uint dwRequestUrlLen = (uint)strRequestUrl.Length;
            struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            struXMLConfigInput.lpRequestUrl = Marshal.StringToHGlobalAnsi(strRequestUrl);
            IntPtr lpInputParam = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(struXMLConfigInput, lpInputParam, false);

            int iOutSize = Marshal.SizeOf(struXMLConfigInput);
            struXMLConfigOutput.dwSize = (uint)iOutSize;
            struXMLConfigOutput.dwOutBufferSize = CHCNetSDK.MAX_LEN_XML;
            struXMLConfigOutput.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.MAX_LEN_XML);
            IntPtr lpOutputParam = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(struXMLConfigOutput, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(m_lUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Thermal: 获取热成像码流参数，错误码：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                string strOutputParam = Marshal.PtrToStringAnsi(struXMLConfigOutput.lpOutBuffer);
                StreamParamXML(strOutputParam);
            }

            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);
        }

        private bool StreamParamXML(string strStreamParamXML)
        {
            if (strStreamParamXML != "")
            {
                CTinyXmlTrans XMLBASE = new CTinyXmlTrans();
                XMLBASE.Parse(strStreamParamXML);
                XMLBASE.SetRoot();

                string strVideoCodingType = "";
                if (XMLBASE.FindElemFromBegin("ThermalStreamParam") && XMLBASE.IntoElem())
                {
                    if (XMLBASE.FindElem("videoCodingType"))
                    {
                        strVideoCodingType = XMLBASE.GetData();
                        if ("thermal_raw_data" == strVideoCodingType)
                        {
                            comboBoxVideoCodingType.SelectedIndex = 0;
                        }
                        if ("pixel-to-pixel_thermometry_data" == strVideoCodingType)
                        {
                            comboBoxVideoCodingType.SelectedIndex = 1;
                        }
                        if ("real-time_raw_data" == strVideoCodingType)
                        {
                            comboBoxVideoCodingType.SelectedIndex = 2;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("ThermalStreamParam：不存在");
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

        private void btnSetStreamParam_Click(object sender, EventArgs e)
        {
            struXMLConfigInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
            struXMLConfigOutput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();

            string strVideoCodingType = "<videoCodingType>" + comboBoxVideoCodingType.Text + "</videoCodingType>\r\n";
            string strInput = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<PixelToPixelParam version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n" +
                             strVideoCodingType + "</PixelToPixelParam>\r\n";

            int iInSize = Marshal.SizeOf(struXMLConfigInput);
            struXMLConfigInput.dwSize = (uint)iInSize;
            string strRequestUrl = "PUT /ISAPI/Thermal/channels/" + m_iChannel + "/streamParam\r\n";
            uint dwRequestUrlLen = (uint)strRequestUrl.Length;
            struXMLConfigInput.lpRequestUrl = Marshal.StringToHGlobalAnsi(strRequestUrl);
            struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            struXMLConfigInput.dwInBufferSize = (uint)strInput.Length;
            struXMLConfigInput.lpInBuffer = Marshal.StringToCoTaskMemAnsi(strInput);
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
                string strErr = "Thermal: 设置热成像码流参数，错误码：" + iLastErr; ;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("设置热成像码流参数：成功");
            }

            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);
        }

        // 热成像码流数据转换
        private void btnThermalDataConv_Click(object sender, EventArgs e)
        {
            struXMLConfigInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
            struXMLConfigOutput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();

            string strConvMethod = "";
            string strTemp = "";
            if (1 == comboBoxThermalDataConv.SelectedIndex)
            {
                strConvMethod = "convertGrayscaleToTemperature";
                strTemp = "<grayscale>" + textBoxGrayscale.Text + "</grayscale>\r\n";
            }
            if (0 == comboBoxThermalDataConv.SelectedIndex)
            {
                strConvMethod = "convertTemperatureToGrayscale";
                strTemp = "<temperature>" + textBoxTemperature.Text + "</temperature>\r\n";
            }
            string strConversionMethod = "<conversionMethod>" + strConvMethod + "</conversionMethod>\r\n";
            string strInput = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ThermalDataConversionDescription version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n" +
                             strConversionMethod + strTemp + "</ThermalDataConversionDescription>\r\n";

            int iInSize = Marshal.SizeOf(struXMLConfigInput);
            struXMLConfigInput.dwSize = (uint)iInSize;
            string strRequestUrl = "POST /ISAPI/Thermal/channels/" + m_iChannel + "/thermalDataConversion\r\n";
            uint dwRequestUrlLen = (uint)strRequestUrl.Length;
            struXMLConfigInput.lpRequestUrl = Marshal.StringToHGlobalAnsi(strRequestUrl);
            struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            struXMLConfigInput.dwInBufferSize = (uint)strInput.Length;
            struXMLConfigInput.lpInBuffer = Marshal.StringToHGlobalAnsi(strInput);
            IntPtr lpInputParam = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(struXMLConfigInput, lpInputParam, false);

            int iOutSize = Marshal.SizeOf(struXMLConfigOutput);
            struXMLConfigOutput.dwSize = (uint)iOutSize;
            struXMLConfigOutput.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.MAX_LEN_XML);
            struXMLConfigOutput.dwOutBufferSize = CHCNetSDK.MAX_LEN_XML;
            IntPtr lpOutputParam = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(struXMLConfigOutput, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(m_lUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Thermal: 热成像温度灰度转换，错误码：" + iLastErr; ;
                MessageBox.Show(strErr);
            }
            else
            {
                struXMLConfigOutput = (CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT)Marshal.PtrToStructure(lpOutputParam, typeof(CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT));
                string strOutputParam = Marshal.PtrToStringAnsi(struXMLConfigOutput.lpOutBuffer);

                ThermalDataConversionXML(strOutputParam);
            }

            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);
        }

        private bool ThermalDataConversionXML(string strCorrectThermXML)
        {
            if (strCorrectThermXML != "")
            {
                CTinyXmlTrans XMLBASE = new CTinyXmlTrans();
                XMLBASE.Parse(strCorrectThermXML);
                XMLBASE.SetRoot();

                if (XMLBASE.FindElemFromBegin("ThermalDataConversionResult") && XMLBASE.IntoElem())
                {
                    if (XMLBASE.FindElem("temperature"))
                    {
                        textBoxTemperature.Text = XMLBASE.GetData();
                    }
                    if (XMLBASE.FindElem("grayscale"))
                    {
                        textBoxGrayscale.Text = XMLBASE.GetData();
                    }
                }
                else
                {
                    MessageBox.Show("ThermalDataConversionResult：不存在");
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

        

    }
}
