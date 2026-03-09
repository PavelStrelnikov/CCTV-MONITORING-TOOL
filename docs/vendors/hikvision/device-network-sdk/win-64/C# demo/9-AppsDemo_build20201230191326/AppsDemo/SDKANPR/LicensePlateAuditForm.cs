using Common;
using System;
using System.IO;
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

namespace SDKANPR
{
    public partial class LicensePlateAuditForm : Form
    {
        public LicensePlateAuditForm()
        {
            InitializeComponent();
        }

        //设备树对象映射
        IDeviceTree m_deviceTree = PluginsFactory.GetDeviceTreeInstance();

        //上传文件名
        private string uploadFile="";
        //下载文件名
        private string downloadFile="";
        //文件上传标识符，用于表示当前文件是否上传完成
        private bool m_bUpload=false;
        //文件下载标识符,用于表示当前文件是否下载完成
        private bool m_bDownload = false;
        //文件上传句柄
        private int m_lUploadHandle=-1;
        //文件下载句柄
        private int m_lDownloadHandle=-1;
        //输出XML长度
        const uint XML_ABILITY_OUT_LEN = 3 * 1024 * 1024;


        private string m_id = "";
        private string m_licensePlate = "";
        private string m_type = "";
        private string m_createTime = "";
        private string m_direction = "";
        private string m_laneNo = "";
        private string m_plateCategory = "";
        private string m_country = "";
        private string m_area = "";
        private string m_effictiveTime = "";
        private string m_countryIndex = "";

        //
        private CHCNetSDK.NET_DVR_XML_CONFIG_INPUT m_struXMLConfigInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
        private CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT m_struXMLConfigOutput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();

        private void InitLPListAuditResultData()
        {
            m_id = "--";
            m_licensePlate = "--";
            m_type = "--";
            m_createTime = "--";
            m_direction = "--";
            m_laneNo = "--";
            m_plateCategory = "--";
            m_country = "--";
            m_area = "--";
            m_effictiveTime = "--";
            m_countryIndex = "--";
        }

        private void LicensePlateAuditForm_Load(object sender, EventArgs e)
        {

            if (!checkAbility())
            {
                MessageBox.Show("import block allow list not support");
            }
        }

        private bool checkAbility()
        {
            bool flag = false;
            IDeviceTree.EDeviceTreeType deviceType = m_deviceTree.GetDeviceTreeType();
            if (deviceType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {

                flag = SDK_checkAbility();
            }
            return flag;
        }


        private bool SDK_checkAbility()
        {
            IntPtr ptrCfgVer;
            IntPtr pOutBuf;
            string strOutBuf;
            uint dwinSize = 0;
            int nID = 0;
            bool bReturnValue = false;
            uint dwLastError = 0;
            CTinyXmlTrans XMLBASE = new CTinyXmlTrans();

            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();

            //[out]
            pOutBuf = Marshal.AllocHGlobal((Int32)XML_ABILITY_OUT_LEN);

            CHCNetSDK.NET_DVR_STD_ABILITY struSTDAbility = new CHCNetSDK.NET_DVR_STD_ABILITY();
            dwinSize = sizeof(int);
            ptrCfgVer = Marshal.AllocHGlobal((int)dwinSize);
            Marshal.WriteInt32(ptrCfgVer, nID);

            struSTDAbility.lpCondBuffer = ptrCfgVer;
            struSTDAbility.dwCondSize = dwinSize;
            struSTDAbility.lpOutBuffer = pOutBuf;
            struSTDAbility.dwOutSize = XML_ABILITY_OUT_LEN;
            struSTDAbility.lpStatusBuffer = pOutBuf;
            struSTDAbility.dwStatusSize = XML_ABILITY_OUT_LEN;

            int dwSTDSize = Marshal.SizeOf(struSTDAbility);
            IntPtr ptrSTDCfg = Marshal.AllocHGlobal(dwSTDSize);
            Marshal.StructureToPtr(struSTDAbility, ptrSTDCfg, false);

            bReturnValue = CHCNetSDK.NET_DVR_GetSTDAbility((int)deviceInfo.lLoginID, CHCNetSDK.NET_DVR_GET_TRAFFIC_CAP, ptrSTDCfg);

            Marshal.FreeHGlobal(ptrCfgVer);
            Marshal.FreeHGlobal(ptrSTDCfg);

            if (bReturnValue)
            {

                int lentemp = (int)XML_ABILITY_OUT_LEN;
                strOutBuf = Marshal.PtrToStringAnsi(pOutBuf, lentemp);
                XMLBASE.Parse(strOutBuf);
                Marshal.FreeHGlobal(pOutBuf);
                if (XMLBASE.FindElem("TrafficCap") && XMLBASE.IntoElem())
                {
                    if (XMLBASE.FindElem("isSupportPlateList") && XMLBASE.GetData() == "true")
                    {
                        return true;
                    }
                }
                
                return false;
            }
            else
            {
                dwLastError = CHCNetSDK.NET_DVR_GetLastError();
                MessageBox.Show("Device not support this function!");
                return false;
            }
            
        }


        private void UpLoadBrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofile = new OpenFileDialog();
            ofile.InitialDirectory = ".";
            ofile.Filter = "所有文件(*.*)|*.*";
            if (ofile.ShowDialog() == DialogResult.OK)
            {
                if (ofile.FileName != string.Empty)
                {
                    try
                    {
                        uploadFile = ofile.FileName;
                        UpLoadFileText.Text = uploadFile;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            
        }


        private void UpLoadButton_Click(object sender, EventArgs e)
        {
            //判断上传文件是否为空
            if (UpLoadFileText.Text.Length == 0)
            {
                MessageBox.Show("File path is null");
                return;
            }
            IDeviceTree.EDeviceTreeType deviceType = m_deviceTree.GetDeviceTreeType();
            if (deviceType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                //SDK私有协议交互实现
                SDK_UploadBlocklistAllowlist();
            }
        }
        

        private void SDK_UploadBlocklistAllowlist()
        {
            IDeviceTree.DeviceInfo deviceInfo=m_deviceTree.GetSelectedDeviceInfo();

            if(m_bUpload)
            {
                CHCNetSDK.NET_DVR_UploadClose(m_lUploadHandle);
                m_lUploadHandle = -1;
                UpLoadButton.Text = "上传";
            }
            else
            {
                try
                {
                    m_lUploadHandle = CHCNetSDK.NET_DVR_UploadFile_V40((int)deviceInfo.lLoginID, (uint)CHCNetSDK.UPLOAD_VEHICLE_BLOCKALLOWLIST_FILE, IntPtr.Zero, 0, uploadFile, IntPtr.Zero, 0);
                    if (m_lUploadHandle < 0)
                    {
                        int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                        string strErr = "上传失败，错误码：" + iLastErr;
                        MessageBox.Show(strErr, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        CHCNetSDK.NET_DVR_StopUploadFile(m_lUploadHandle);

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
                            dwState = CHCNetSDK.NET_DVR_GetUploadState(m_lUploadHandle, pProgress);
                            dwProgress = Marshal.ReadInt32(pProgress);

                            if (dwState == 1)
                            {
                                m_labelUpload.Text = "上传成功！";
                                m_bUpload = false;
                                break;
                            }
                            else if (dwState == 2)
                            {
                                m_labelUpload.Text = "正在上传,已上传: " + dwProgress;
                                m_labelUpload.Update();
                            }
                            else if (dwState == 3)
                            {
                                m_labelUpload.Text = "上传失败！";
                                break;
                            }
                            else if (dwState == 4)
                            {
                                if (dwProgress == 100)
                                {
                                    m_labelUpload.Text = "上传成功！";
                                    m_bUpload = false;
                                    break;
                                }
                                else
                                {
                                    m_labelUpload.Text = "网络断开，状态未知";
                                    break;
                                }
                            }

                            if (dwState != 2 && dwState != 5)
                            {
                                CHCNetSDK.NET_DVR_UploadClose(m_lUploadHandle);   // break已经跳出循环，会执行到这儿？
                                m_bUpload = true;
                                UpLoadButton.Text = "停止上传";
                            }
                        }   //结束上传的过程

                        Marshal.FreeHGlobal(pProgress);
                    }
                }catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                m_bUpload = false;
            }
            
        }


        private void DownLoadBrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofile = new OpenFileDialog();
            ofile.InitialDirectory = ".";
            ofile.Filter = "所有文件(*.*)|*.*";
            if (ofile.ShowDialog() == DialogResult.OK)
            {
                if (ofile.FileName != string.Empty)
                {
                    try
                    {
                        downloadFile = ofile.FileName;
                        DownLoadFileText.Text = downloadFile;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void DownLoad_Click(object sender, EventArgs e)
        {
            if (downloadFile.Length == 0)
            {
                MessageBox.Show("File path is null");
                return;
            }
            IDeviceTree.EDeviceTreeType deviceType = m_deviceTree.GetDeviceTreeType();
            if (deviceType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                //SDK私有协议交互实现
                SDK_DownloadBlocklistAllowlist();
            }
        }
        

        private void SDK_DownloadBlocklistAllowlist()
        {
            if (m_bDownload)
            {
                CHCNetSDK.NET_DVR_StopDownload(m_lDownloadHandle);
                m_lDownloadHandle = -1;
                m_bDownload = false;
                DownLoad.Text = "下载";
            }else
            {
                IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
                m_lDownloadHandle = CHCNetSDK.NET_DVR_StartDownload((int)deviceInfo.lLoginID, (uint)CHCNetSDK.NET_SDK_DOWNLOAD_VEHICLE_BLOCKALLOWLIST_FILE, IntPtr.Zero, 0, downloadFile);
                
                if(m_lDownloadHandle<0)
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "下载失败，错误码：" + iLastErr;
                    MessageBox.Show(strErr, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        dwState = CHCNetSDK.NET_DVR_GetDownloadState(m_lDownloadHandle, pProgress);
                        dwProgress = Marshal.ReadInt32(pProgress);

                        if (dwState == 1)
                        {
                            MessageBox.Show("下载成功", "Download successfully");
                            m_bDownload = false;
                            break;
                        }
                        else if (dwState == 2)
                        {
                            MessageBox.Show("正在下载,已下载: " + dwProgress, "Is Downloading,progress:");
                            m_bDownload = true;
                        }
                        else if (dwState == 3)
                        {
                            MessageBox.Show("下载失败", "Download failed");
                            break;
                        }
                        else if (dwState == 4)
                        {
                            if (dwProgress == 100)
                            {
                                MessageBox.Show("下载成功");
                                m_bDownload = false;
                                break;
                            }
                            else
                            {
                                MessageBox.Show("网络断开，状态未知", "Network disconnect, status unknown");
                                break;
                            }
                        }
                        if (dwState != 2 && dwState != 5)
                        {
                            CHCNetSDK.NET_DVR_StopDownload(m_lDownloadHandle);
                            m_bDownload = true;
                            DownLoad.Text = "停止下载";
                        }
                    }  // 结束下载
                    Marshal.FreeHGlobal(pProgress);
                }
            }
        }

        //获取车牌对比配置
        private void LPListAuditContrastGetButton_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceType = m_deviceTree.GetDeviceTreeType();
            if (deviceType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                //SDK私有协议交互实现
                SDK_GetLPListAuditContrast();
            }
        }

        //SDK登录方式获取车牌对比配置        
        private void SDK_GetLPListAuditContrast()
        {
            IDeviceTree.ChannelInfo channelInfo = m_deviceTree.GetSelectedChannelInfo();
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            int iUserID = (int)deviceInfo.lLoginID;

            int iInSize = Marshal.SizeOf(m_struXMLConfigInput);
            m_struXMLConfigInput.dwSize = (uint)iInSize;
            string strUrl = "GET /ISAPI/Traffic/channels/" + channelInfo.iChannelNo + "/searchLPListAudit/contrast";
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
                string strErr = "LPListAuditContrast: 获取车牌对比配置，错误码：" + iLastErr; ;
                MessageBox.Show(strErr);
            }
            else
            {
                m_struXMLConfigOutput = (CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT)Marshal.PtrToStructure(lpOutputParam, typeof(CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT));
                string strOutputParam = Marshal.PtrToStringAnsi(m_struXMLConfigOutput.lpOutBuffer);
                LPListAuditConstrastShowData(strOutputParam);
            }
            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);
            Marshal.FreeHGlobal(m_struXMLConfigOutput.lpOutBuffer);
        }

        //车牌对比配置展示
        private void LPListAuditConstrastShowData(string strResult)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strResult);
            XmlNode rootNode = xmlDoc.SelectSingleNode("LPListAuditContrast");
            foreach (XmlNode node in rootNode.ChildNodes)
            {
                string nodeName = node.Name;
                if (nodeName.Equals("isContrastLicensePlate") && node.Value.Equals("true"))
                {
                    ContrastLicensePlateCheckBox.Checked = true;
                }
                if (nodeName.Equals("isContrastPlateCategory") && node.Value.Equals("true"))
                {
                    ContrastPlateCategoryCheckBox.Checked = true;
                }
                if (nodeName.Equals("isContrastCountryArea") && node.Value.Equals("true"))
                {
                    ContrastCountryAreaCheckBox.Checked = true;
                }
            }
        }

        //车牌对比设置
        private void LPListAuditContrastSetButton_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceType = m_deviceTree.GetDeviceTreeType();
            if (deviceType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                //SDK私有协议交互实现
                SDK_SetLPListAuditContrast();
            }
        }

        //车牌对比设置(SDK方式)
        private void SDK_SetLPListAuditContrast()
        {
            IDeviceTree.ChannelInfo channelInfo = m_deviceTree.GetSelectedChannelInfo();
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            int iUserID = (int)deviceInfo.lLoginID;

            string strRequestData = GETLPLAuditContrastData();
            int iInSize = Marshal.SizeOf(m_struXMLConfigInput);
            m_struXMLConfigInput.dwSize = (uint)iInSize;
            string strUrl = "PUT /ISAPI/Traffic/channels/" + channelInfo.iChannelNo + "/searchLPListAudit/contrast";
            uint dwRequestUrlLen = (uint)strUrl.Length;
            m_struXMLConfigInput.lpRequestUrl = Marshal.StringToHGlobalAnsi(strUrl);
            m_struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            IntPtr lpInputParam = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(m_struXMLConfigInput, lpInputParam, false);
            m_struXMLConfigInput.lpInBuffer = Marshal.StringToCoTaskMemAnsi(strRequestData);
            m_struXMLConfigInput.dwInBufferSize = (uint)strRequestData.Length;

            int iOutSize = Marshal.SizeOf(m_struXMLConfigOutput);
            m_struXMLConfigOutput.dwSize = (uint)iOutSize;
            m_struXMLConfigOutput.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.MAX_LEN_XML);
            m_struXMLConfigOutput.dwOutBufferSize = (uint)CHCNetSDK.MAX_LEN_XML;
            IntPtr lpOutputParam = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struXMLConfigOutput, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(iUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "LPListAuditContrast: 车牌对比配置，错误码：" + iLastErr; ;
                MessageBox.Show(strErr);
            }
            
            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);
            Marshal.FreeHGlobal(m_struXMLConfigOutput.lpOutBuffer);
        }

        //获取车牌对比配置报文
        private string GETLPLAuditContrastData()
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
            xmlelem = xmldoc.CreateElement("", "LPListAuditContrast", "");
            xmlelem.SetAttribute("version", "2.0");
            xmlelem.SetAttribute("xmlns", "http://www.isapi.org/ver20/XMLSchema");
            xmldoc.AppendChild(xmlelem);

            XmlNode root = xmldoc.SelectSingleNode("LPListAuditContrast");//查找<LPListAuditContrast>

            //加入元素isContrastLicensePlate
            XmlElement xesub1 = xmldoc.CreateElement("isContrastLicensePlate");
            if (ContrastLicensePlateCheckBox.Checked)
            {
                xesub1.InnerText = "true";//设置文本节点
            }
            else
            {
                xesub1.InnerText = "false";//设置文本节点
            }
            root.AppendChild(xesub1);//添加到<LPListAuditContrast>节点中

            //加入元素isContrastPlateCategory
            XmlElement xesub2 = xmldoc.CreateElement("isContrastPlateCategory");
            if (ContrastPlateCategoryCheckBox.Checked)
            {
                xesub2.InnerText = "true";//设置文本节点
            }
            else
            {
                xesub2.InnerText = "false";//设置文本节点
            }
            root.AppendChild(xesub2);

            //加入元素isContrastCountryArea
            XmlElement xesub3 = xmldoc.CreateElement("isContrastCountryArea");
            if (ContrastCountryAreaCheckBox.Checked)
            {
                xesub3.InnerText = "true";//设置文本节点
            }
            else
            {
                xesub3.InnerText = "false";//设置文本节点
            }
            root.AppendChild(xesub3);

            string strTemp = xmldoc.InnerXml.ToString();

            return strTemp;
        }

        private void SearchLPListAudit_Button_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceType = m_deviceTree.GetDeviceTreeType();
            if (deviceType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                //SDK私有协议交互实现
                SDK_SearchLPListAudit();
            }
        }


        private void SDK_SearchLPListAudit()
        {
            IDeviceTree.ChannelInfo channelInfo = m_deviceTree.GetSelectedChannelInfo();
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            int iUserID = (int)deviceInfo.lLoginID;

            string strRequestData = GetSearchConditionData();
            int iInSize = Marshal.SizeOf(m_struXMLConfigInput);
            m_struXMLConfigInput.dwSize = (uint)iInSize;
            string strUrl = "POST /ISAPI/Traffic/channels/" + channelInfo.iChannelNo + "/searchLPListAudit";
            uint dwRequestUrlLen = (uint)strUrl.Length;
            m_struXMLConfigInput.lpRequestUrl = Marshal.StringToHGlobalAnsi(strUrl);
            m_struXMLConfigInput.dwRequestUrlLen = dwRequestUrlLen;
            IntPtr lpInputParam = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(m_struXMLConfigInput, lpInputParam, false);
            m_struXMLConfigInput.lpInBuffer = Marshal.StringToCoTaskMemAnsi(strRequestData);
            m_struXMLConfigInput.dwInBufferSize = (uint)strRequestData.Length;

            int iOutSize = Marshal.SizeOf(m_struXMLConfigOutput);
            m_struXMLConfigOutput.dwSize = (uint)iOutSize;
            m_struXMLConfigOutput.lpOutBuffer = Marshal.AllocHGlobal(CHCNetSDK.MAX_LEN_XML);
            m_struXMLConfigOutput.dwOutBufferSize = (uint)CHCNetSDK.MAX_LEN_XML;
            IntPtr lpOutputParam = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struXMLConfigOutput, lpOutputParam, false);

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(iUserID, lpInputParam, lpOutputParam))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "LPListAudit: err：" + iLastErr; ;
                MessageBox.Show(strErr);
            }
            else
            {
                m_struXMLConfigOutput = (CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT)Marshal.PtrToStructure(lpOutputParam, typeof(CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT));
                string strOutputParam = Marshal.PtrToStringAnsi(m_struXMLConfigOutput.lpOutBuffer);
                AnaliyzeLPListAuditData(strOutputParam);
            }
            Marshal.FreeHGlobal(lpInputParam);
            Marshal.FreeHGlobal(lpOutputParam);
            Marshal.FreeHGlobal(m_struXMLConfigOutput.lpOutBuffer);
        }


        private string GetSearchConditionData()
        {
            string strRequest="";

            //组装查询报文
            XmlDocument xmldoc;
            XmlElement xmlelem;
            xmldoc = new XmlDocument();
            //加入XML的声明段落,<?xml version="1.0" encoding="utf-8"?>
            XmlDeclaration xmldecl;
            xmldecl = xmldoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmldoc.AppendChild(xmldecl);

            //加入一个根元素
            xmlelem = xmldoc.CreateElement("", "LPListAuditSearchDescription", "");
            xmlelem.SetAttribute("version", "2.0");
            xmlelem.SetAttribute("xmlns", "http://www.isapi.org/ver20/XMLSchema");
            xmldoc.AppendChild(xmlelem);

            XmlNode root = xmldoc.SelectSingleNode("LPListAuditSearchDescription");//查找<LPListAuditContrast>

            //加入元素searchID
            XmlElement xesub1 = xmldoc.CreateElement("searchID");
            if (searchIDTextBox.Text != "")
            {
                xesub1.InnerText = searchIDTextBox.Text;//设置文本节点
            }
            root.AppendChild(xesub1);//添加到<LPListAuditSearchDescription>节点中

            //加入元素searchResultPosition
            XmlElement xesub2 = xmldoc.CreateElement("searchResultPosition");
            if (searchResultPositionTextBox.Text != "")
            {
                xesub2.InnerText = searchResultPositionTextBox.Text;//设置文本节点
            }
            root.AppendChild(xesub2);

            //加入元素maxResults
            XmlElement xesub3 = xmldoc.CreateElement("maxResults");
            if (ContrastCountryAreaCheckBox.Checked)
            {
                xesub3.InnerText = maxResultsTextBox.Text;//设置文本节点
            }
            root.AppendChild(xesub3);

            strRequest = xmldoc.InnerXml.ToString();

            return strRequest;
        }

        private void AnaliyzeLPListAuditData(string strLPData)
        {
            //初始化查询结果
            InitLPListAuditResultData();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strLPData);
            RemoveXmlDocNamespace(ref xmlDoc);
            XmlNode rootNode = xmlDoc.SelectSingleNode("LPListAuditSearchResult");
            foreach (XmlNode node in rootNode.ChildNodes)
            {
                string nodename = node.Name;
                if (nodename.Equals("LicensePlateInfoList"))
                {
                    foreach (XmlNode nodel in node.ChildNodes)
                    {
                        string nodeName = nodel.Name;
                        ListViewItem lvi = new ListViewItem();
                        if (nodeName.Equals("LicensePlateInfo"))
                        {
                            foreach (XmlNode xnode in nodel.ChildNodes)
                            {
                                string xnodeName = xnode.Name;
                                if (xnodeName.Equals("id"))
                                {
                                    m_id = xnode.InnerText;
                                }
                                else if (xnodeName.Equals("LicensePlate"))
                                {
                                    m_licensePlate = xnode.InnerText;
                                }
                                else if (xnodeName.Equals("type"))
                                {
                                    m_type = xnode.InnerText;
                                }
                                else if (xnodeName.Equals("createTime"))
                                {
                                    m_createTime = xnode.InnerText;
                                }
                                else if (xnodeName.Equals("direction"))
                                {
                                    m_direction = xnode.InnerText;
                                }
                                else if (xnodeName.Equals("laneNo"))
                                {
                                    m_laneNo = xnode.InnerText;
                                }
                                else if (xnodeName.Equals("plateCategory"))
                                {
                                    m_plateCategory = xnode.InnerText;
                                }
                                else if (xnodeName.Equals("country"))
                                {
                                    m_country = xnode.InnerText;
                                }
                                else if (xnodeName.Equals("area"))
                                {
                                    m_area = xnode.InnerText;
                                }
                                else if (xnodeName.Equals("effectiveTime"))
                                {
                                    m_effictiveTime = xnode.InnerText;
                                }
                                else if (xnodeName.Equals("countryIndex"))
                                {
                                    m_countryIndex = xnode.InnerText;
                                }
                            }
                            lvi.Text = m_id;
                            //lvi.SubItems.Add(m_id);
                            lvi.SubItems.Add(m_licensePlate);
                            lvi.SubItems.Add(m_type);
                            lvi.SubItems.Add(m_createTime);
                            lvi.SubItems.Add(m_direction);
                            lvi.SubItems.Add(m_laneNo);
                            lvi.SubItems.Add(m_plateCategory);
                            lvi.SubItems.Add(m_country);
                            lvi.SubItems.Add(m_area);
                            lvi.SubItems.Add(m_effictiveTime);
                            lvi.SubItems.Add(m_countryIndex);
                            LicensePlateInfoListView.Items.Insert(0, lvi);
                            InitLPListAuditResultData();
                        }

                    }
                }
            }
        }

        private void SetBarrierGateCtrlButton_Click(object sender, EventArgs e)
        {
            IDeviceTree.EDeviceTreeType deviceType = m_deviceTree.GetDeviceTreeType();
            if (deviceType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                //SDK私有协议交互实现
                SDK_SetBarrierGateCtrl();
            }
        }
       
        private void SDK_SetBarrierGateCtrl()
        {
            IntPtr ptrInput = IntPtr.Zero;
            IDeviceTree.ChannelInfo channelInfo = m_deviceTree.GetSelectedChannelInfo();
            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            int iUserID = (int)deviceInfo.lLoginID;

            CHCNetSDK.NET_DVR_BARRIERGATE_CFG struGateCFG = new CHCNetSDK.NET_DVR_BARRIERGATE_CFG();
            struGateCFG.dwSize = (uint)Marshal.SizeOf(struGateCFG);
            struGateCFG.dwChannel = (uint)channelInfo.iChannelNo;
            struGateCFG.byBarrierGateCtrl = (byte)m_comBarrierGateCtrl.SelectedIndex;
            //struGateCFG = Byte.Parse(m_textBoxEntranceNo.Text);

            ptrInput = Marshal.AllocHGlobal(Marshal.SizeOf(struGateCFG));
            Marshal.StructureToPtr(struGateCFG, ptrInput, false);

            if (!CHCNetSDK.NET_DVR_RemoteControl(iUserID, CHCNetSDK.NET_DVR_BARRIERGATE_CTRL,ptrInput,(uint)Marshal.SizeOf(struGateCFG)))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "远程道闸控制失败，错误码：" + iLastErr; ;
                MessageBox.Show(strErr);
            }
            else
            {
                string strSuss = "远程道闸控制成功";
                MessageBox.Show(strSuss);
            }
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
