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
using System.Net;
using System.Xml;
using System.Xml.Linq;
using TINYXMLTRANS;
using System.Runtime.InteropServices;
using System.IO;

namespace SDKANPR
{
    public partial class licensePlateRecognitionForm : Form
    {
        public licensePlateRecognitionForm()
        {
            InitializeComponent();
        }

        //设备树对象映射
        IDeviceTree m_deviceTree = PluginsFactory.GetDeviceTreeInstance();

        private CHCNetSDK.NET_DVR_XML_CONFIG_INPUT m_struXMLConfigInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
        private CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT m_struXMLConfigOutput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();
        private void Get_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceType = m_deviceTree.GetDeviceTreeType();
            if (deviceType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                //SDK私有协议交互实现
                SDK_GetCurVehicleDetectMode();
            }
        }

        private void SDK_GetCurVehicleDetectMode()
        {
            IDeviceTree.ChannelInfo channelInfo = m_deviceTree.GetSelectedChannelInfo();
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            int iUserID = (int)deviceInfo.lLoginID;

            int iInSize = Marshal.SizeOf(m_struXMLConfigInput);
            m_struXMLConfigInput.dwSize = (uint)iInSize;
            string strUrl = "GET /ISAPI/Traffic/channels/" + channelInfo.iChannelNo + "/CurVehicleDetectMode";
            uint dwRequestUrlLen = (uint)strUrl.Length;
            m_struXMLConfigInput.lpRequestUrl = Marshal.StringToHGlobalAnsi(strUrl);
            m_struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            IntPtr lpInputParam = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(m_struXMLConfigInput, lpInputParam, false);

            int iOutSize = Marshal.SizeOf(m_struXMLConfigOutput);
            m_struXMLConfigOutput.dwSize = (uint)iOutSize;
            m_struXMLConfigOutput.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.MAX_LEN_XML);
            m_struXMLConfigOutput.dwOutBufferSize = (uint)CHCNetSDK.MAX_LEN_XML;
            IntPtr lpOutputParam = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struXMLConfigOutput, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(iUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "LPListAuditContrast: 获取检测模式失败，错误码：" + iLastErr; ;
                MessageBox.Show(strErr);
            }
            else
            {
                m_struXMLConfigOutput = (CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT)Marshal.PtrToStructure(lpOutputParam, typeof(CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT));
                string strOutputParam = Marshal.PtrToStringAnsi(m_struXMLConfigOutput.lpOutBuffer);
                ShowCurVehicleDetectMode(strOutputParam);
            }
            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);
        }

        private void ShowCurVehicleDetectMode(string strResult)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strResult);
            RemoveXmlDocNamespace(ref xmlDoc);
            XmlNode rootNode = xmlDoc.SelectSingleNode("CurVehicleDetectMode");
            foreach (XmlNode node in rootNode)
            {
                string nodeName = node.Name;
                if (nodeName.Equals("CurMode") && node.InnerText.Equals("hvtVehicleDetection"))
                {
                    DetectModeBox.SelectedIndex = 0;
                }
                else if (nodeName.Equals("CurMode") && node.InnerText.Equals("vehicleDetection"))
                {
                    DetectModeBox.SelectedIndex = 1;
                }
            }
        }

        //车辆检测模式配置
        private void SET_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceType = m_deviceTree.GetDeviceTreeType();
            if (deviceType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                //SDK私有协议交互实现
                SDK_SetCurVehicleDetectMode();
            }
        }

       //SDK方式车辆检测模式配置
        private void SDK_SetCurVehicleDetectMode()
        {
            m_struXMLConfigInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();

            IDeviceTree.ChannelInfo channelInfo = m_deviceTree.GetSelectedChannelInfo();
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            int iUserID = (int)deviceInfo.lLoginID;

            string strRequestData = GETCurVehicleDetectMode();
            int iInSize = Marshal.SizeOf(m_struXMLConfigInput);
            m_struXMLConfigInput.dwSize = (uint)iInSize;
            string strUrl = "PUT /ISAPI/Traffic/channels/" + channelInfo.iChannelNo + "/CurVehicleDetectMode";
            uint dwRequestUrlLen = (uint)strUrl.Length;
            m_struXMLConfigInput.lpRequestUrl = Marshal.StringToCoTaskMemAnsi(strUrl);
            m_struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            m_struXMLConfigInput.lpInBuffer = Marshal.StringToCoTaskMemAnsi(strRequestData);
            m_struXMLConfigInput.dwInBufferSize = (uint)strRequestData.Length;
            m_struXMLConfigInput.dwRecvTimeOut = 3000;
            IntPtr lpInputParam = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(m_struXMLConfigInput, lpInputParam, false);
            

            int iOutSize = Marshal.SizeOf(m_struXMLConfigOutput);
            m_struXMLConfigOutput.dwSize = (uint)iOutSize;
            m_struXMLConfigOutput.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.MAX_LEN_XML);
            m_struXMLConfigOutput.dwOutBufferSize = (uint)CHCNetSDK.MAX_LEN_XML;
            IntPtr lpOutputParam = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struXMLConfigOutput, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(iUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "LicensePlateRecongnition: 车牌检测错误，错误码：" + iLastErr; ;
                MessageBox.Show(strErr);
            }

            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);
        }

        private string GETCurVehicleDetectMode()
        { 
            //组装XML报文
            XmlDocument xmldoc;
            XmlElement xmlelem;
            xmldoc = new XmlDocument();
            //加入XML的声明段落,<?xml version="1.0" encoding="utf-8"?>
            XmlDeclaration xmldecl;
            xmldecl = xmldoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmldoc.AppendChild(xmldecl);

            //加入一个根元素
            xmlelem = xmldoc.CreateElement("", "CurVehicleDetectMode", "");
            xmlelem.SetAttribute("version", "2.0");
            xmlelem.SetAttribute("xmlns", "http://www.isapi.org/ver20/XMLSchema");
            xmldoc.AppendChild(xmlelem);

            XmlNode root = xmldoc.SelectSingleNode("CurVehicleDetectMode");//查找<CurVehicleDetectMode>

            //加入元素isContrastLicensePlate
            XmlElement xesub = xmldoc.CreateElement("CurMode");
            if (DetectModeBox.SelectedIndex==0)
            {
                xesub.InnerText = "hvtVehicleDetection";//混行检测
            }
            else if (DetectModeBox.SelectedIndex == 1)
            {
                xesub.InnerText = "vehicleDetection";//车辆检测
            }
            
            root.AppendChild(xesub);//添加到<CurVehicleDetectMode>节点中

            string strTemp = xmldoc.InnerXml.ToString();

            return strTemp;
        }

        private void CommandBtn_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceType = m_deviceTree.GetDeviceTreeType();
            if (deviceType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                //SDK私有协议交互实现
                SDK_Command();
            }
        }

        private void SDK_Command()
        {
            m_struXMLConfigInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();

            IDeviceTree.ChannelInfo channelInfo = m_deviceTree.GetSelectedChannelInfo();
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            int iUserID = (int)deviceInfo.lLoginID;

            string strRequestData = RequestTextBox.Text;
            int iInSize = Marshal.SizeOf(m_struXMLConfigInput);
            m_struXMLConfigInput.dwSize = (uint)iInSize;
            string strUrl="";

            switch (CommandBox.SelectedIndex)
            {
                case 0:
                    strUrl = "GET " + URLTextBox.Text;
                    m_struXMLConfigInput.lpInBuffer = IntPtr.Zero;
                    m_struXMLConfigInput.dwInBufferSize = 0;
                    break;
                case 1:
                    strUrl = "PUT " + URLTextBox.Text;
                    m_struXMLConfigInput.lpInBuffer = Marshal.StringToCoTaskMemAnsi(strRequestData);
                    m_struXMLConfigInput.dwInBufferSize = (uint)strRequestData.Length;
                    break;
                case 2:
                    strUrl = "POST " + URLTextBox.Text;
                    m_struXMLConfigInput.dwInBufferSize = (uint)strRequestData.Length;
                    m_struXMLConfigInput.lpInBuffer = Marshal.StringToCoTaskMemAnsi(strRequestData);
                    break;
                case 3:
                    strUrl = "DELETE " + URLTextBox.Text;
                    m_struXMLConfigInput.lpInBuffer = IntPtr.Zero;
                    m_struXMLConfigInput.dwInBufferSize = 0;
                    break;
                default:
                    break;
            }
            

            uint dwRequestUrlLen = (uint)strUrl.Length;
            m_struXMLConfigInput.lpRequestUrl = Marshal.StringToCoTaskMemAnsi(strUrl);
            m_struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            m_struXMLConfigInput.dwRecvTimeOut = 3000;
            IntPtr lpInputParam = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(m_struXMLConfigInput, lpInputParam, false);
           
            int iOutSize = Marshal.SizeOf(m_struXMLConfigOutput);
            m_struXMLConfigOutput.dwSize = (uint)iOutSize;
            m_struXMLConfigOutput.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.MAX_LEN_XML);
            m_struXMLConfigOutput.dwOutBufferSize = (uint)CHCNetSDK.MAX_LEN_XML;
            IntPtr lpOutputParam = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struXMLConfigOutput, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(iUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "协议透传失败，错误码：" + iLastErr; ;
                MessageBox.Show(strErr);
            }
            else
            {
                m_struXMLConfigOutput = (CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT)Marshal.PtrToStructure(lpOutputParam, typeof(CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT));
                string strOutputParam = Marshal.PtrToStringAnsi(m_struXMLConfigOutput.lpOutBuffer);
                ResultTextBox.Text = strOutputParam;
                MessageBox.Show("协议透传成功");
            }

            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);
            
        }

        private static bool RemoveXmlDocNamespace(ref XmlDocument xmlDoc)
        {
            if (xmlDoc == null)
            {
                return false;
            }
            string strXml = xmlDoc.InnerXml;
            const string strXMLNS = "xmlns=\"";

            int iXmlns = 0;
            while ((iXmlns = strXml.IndexOf(strXMLNS)) >= 0)
            {
                int iXmlnsEnd = strXml.IndexOf("\"", iXmlns + strXMLNS.Length);
                if (iXmlnsEnd < 0)
                {
                    break;
                }
                strXml = strXml.Remove(iXmlns, iXmlnsEnd - iXmlns + 1);
            }
            xmlDoc.InnerXml = strXml;
            return true;
        }
    }
}
