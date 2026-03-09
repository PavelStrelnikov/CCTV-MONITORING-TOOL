using Common;
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

namespace SDKThermometry
{
    public partial class FormXML : Form
    {
        public int m_iChannel = -1;
        public int m_lUserID = -1;
        CHCNetSDK.NET_DVR_XML_CONFIG_INPUT strInputXml = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
        CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT strOutputXml = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();

        public FormXML()
        {
            InitializeComponent();

            comboBoxOpera.SelectedIndex = 0;
        }

        private void btnXML_Click(object sender, EventArgs e)
        {
            Int32 nInSize = Marshal.SizeOf(strInputXml);
            strInputXml.dwSize = (uint)nInSize;

            string strRequestUrl = comboBoxOpera.Text + " " + textBoxRequest.Text;
            uint dwRequestUrlLen = (uint)strRequestUrl.Length;
            strInputXml.lpRequestUrl = Marshal.StringToHGlobalAnsi(strRequestUrl);
            strInputXml.dwRequestUrlLen = dwRequestUrlLen;

            string strInputParam = textBoxInXML.Text;

            strInputXml.lpInBuffer = Marshal.StringToHGlobalAnsi(strInputParam);
            strInputXml.dwInBufferSize = (uint)strInputParam.Length;
            strOutputXml.dwSize = (uint)Marshal.SizeOf(strInputXml);
            IntPtr lpInputParam = Marshal.AllocHGlobal(nInSize);
            Marshal.StructureToPtr(strInputXml, lpInputParam, false);

            Int32 nOutSize = Marshal.SizeOf(strOutputXml);
            strOutputXml.lpOutBuffer = Marshal.AllocHGlobal(3 * 1024 * 1024);
            strOutputXml.dwOutBufferSize = 3 * 1024 * 1024;
            strOutputXml.lpStatusBuffer = Marshal.AllocHGlobal(4096 * 4);
            strOutputXml.dwStatusSize = 4096 * 4;
            IntPtr lpOutputParam = Marshal.AllocHGlobal(nOutSize);
            Marshal.StructureToPtr(strOutputXml, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(m_lUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "NET_DVR_STDXMLConfig failed, error code= " + iLastErr;
                //XML透传失败，输出错误号 Failed to send XML data and output the error code
                MessageBox.Show(strErr);
            }

            string strOutputParam = Marshal.PtrToStringAnsi(strOutputXml.lpOutBuffer);
            textBoxOutXML.Text = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(strOutputParam));
            textBoxStatus.Text = Marshal.PtrToStringAnsi(strOutputXml.lpStatusBuffer);

            Marshal.FreeHGlobal(strInputXml.lpRequestUrl);
            Marshal.FreeHGlobal(strOutputXml.lpOutBuffer);
            Marshal.FreeHGlobal(strOutputXml.lpStatusBuffer);
        }
    }
}
