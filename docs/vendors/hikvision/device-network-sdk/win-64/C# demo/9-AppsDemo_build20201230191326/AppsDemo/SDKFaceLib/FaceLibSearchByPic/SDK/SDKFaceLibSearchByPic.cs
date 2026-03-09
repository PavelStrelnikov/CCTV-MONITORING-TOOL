using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SDKFaceLib
{
    public partial class FaceLibSearchByPicForm
    {
        public class CErrorInfo
        {
            public int errorCode { get; set; }
            public string errorMsg { get; set; }
        }
        public class FaceLibSearchByPicCaps
        {
            public bool isSuportFSsearchByPic { get; set; }
        }
        public bool GetFaceLibSearchByPicCaps(out FaceLibSearchByPicCaps caps)
        {
            caps = null;
            IDeviceTree.DeviceInfo struDeviceInfo = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo();
            string strFDLibRequestUrl = "/ISAPI/Intelligent/FDLib/capabilities\r\n";
            string strFDLibMethod = "GET";
            string strFDLibInputParam = "";
            string strFDLibOutputParam = "";
            bool resFDLib = CommonMethod.DoRequest(struDeviceInfo, strFDLibMethod, strFDLibRequestUrl, strFDLibInputParam, out strFDLibOutputParam);
            string strCapRequestUrl = "/ISAPI/Intelligent/capabilities\r\n";
            string strCapInputParam = "";
            string strCapOutputParam = "";
            bool resCap = CommonMethod.DoRequest(struDeviceInfo, strFDLibMethod, strCapRequestUrl, strCapInputParam, out strCapOutputParam);
            if (resFDLib && resCap)
            {
                XmlDocument xmlFDLibAbility = new XmlDocument();//新建对象
                xmlFDLibAbility.LoadXml(strFDLibOutputParam);
                XmlNode fdliblist = xmlFDLibAbility.GetElementsByTagName("FDSearchDescription").Item(0);
                string isSuportAnalysisFace = xmlFDLibAbility.GetElementsByTagName("isSuportAnalysisFace").Item(0).InnerText;
                string isSuportFCSearch = xmlFDLibAbility.GetElementsByTagName("isSuportFCSearch").Item(0).InnerText;
                XmlDocument xmlAbility = new XmlDocument();//新建对象
                xmlAbility.LoadXml(strCapOutputParam);
                string isSupUpFPByForm = xmlAbility.GetElementsByTagName("isSupportUploadFacePictureByForm").Item(0).InnerText;
                if (fdliblist.InnerText != string.Empty && isSuportAnalysisFace.ToUpper() == "TRUE" && isSuportFCSearch.ToUpper() == "TRUE" && isSupUpFPByForm.ToUpper() == "TRUE")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Get face image query capability ，Error number：" + iLastErr; //人脸库查询失败，输出错误号
                MessageBox.Show(strErr);
                return false;
            }

        }
        public void GetSearchAbility()
        {
            IDeviceTree.DeviceInfo struDeviceInfo = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo();
            string strRequestUrl = "/ISAPI/Intelligent/FDLib/capabilities\r\n";
            string strMethod = "GET";
            string strInputParam = "";
            string strOutputParam = "";
            bool res = CommonMethod.DoRequest(struDeviceInfo, strMethod, strRequestUrl, strInputParam, out strOutputParam);
            if (res)
            {
                XmlDocument xmlAbility = new XmlDocument();//新建对象
                xmlAbility.LoadXml(strOutputParam);
                //寻找所有<FDLibBaseCfg>节点
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlAbility.NameTable);
                nsMgr.AddNamespace("ns", "http://www.isapi.org/ver20/XMLSchema");
                try
                {
                    XmlNode singleNode = xmlAbility.SelectSingleNode("/ns:FDLibCap/ns:FDSearchDescription/ns:maxResults", nsMgr);
                    if (singleNode == null)
                    {
                        this.textBoxMaxResults.Enabled = false;
                    }
                    singleNode = xmlAbility.SelectSingleNode("/ns:FDLibCap/ns:FDSearchDescription/ns:searchResultPosition", nsMgr);
                    if (singleNode == null)
                    {
                        this.textBoxSearchResultPos.Enabled = false;
                    }
                    singleNode = xmlAbility.SelectSingleNode("/ns:FDLibCap/ns:FDSearchDescription/ns:startTime", nsMgr);
                    if (singleNode == null)
                    {
                        this.dtStartTime.Enabled = false;
                        this.dtEndTime.Enabled = false;
                    }
                    singleNode = xmlAbility.SelectSingleNode("/ns:FDLibCap/ns:FDSearchDescription/ns:name", nsMgr);
                    if (singleNode == null)
                    {
                        this.textBoxName.Enabled = false;
                    }
                    singleNode = xmlAbility.SelectSingleNode("/ns:FDLibCap/ns:FDSearchDescription/ns:sex", nsMgr);
                    if (singleNode != null)
                    {
                        string opt = singleNode.Attributes["opt"].Value;
                        string[] optsex = opt.Split(',');
                        comboBoxGender.Items.Add("");
                        foreach (string str in optsex)
                        {
                            comboBoxGender.Items.Add(str);
                        }
                    }
                    else
                    {
                        this.comboBoxGender.Enabled = false;
                    }
                    singleNode = xmlAbility.SelectSingleNode("/ns:FDLibCap/ns:FDSearchDescription/ns:city", nsMgr);
                    if (singleNode == null)
                    {
                        this.textBoxCity.Enabled = false;
                    }
                    singleNode = xmlAbility.SelectSingleNode("/ns:FDLibCap/ns:FDSearchDescription/ns:certificateType", nsMgr);
                    if (singleNode != null)
                    {
                        string opt = singleNode.Attributes["opt"].Value;
                        string[] optsex = opt.Split(',');
                        comboBoxCertificateType.Items.Add("");
                        foreach (string str in optsex)
                        {
                            comboBoxCertificateType.Items.Add(str);
                        }
                    }
                    else
                    {
                        this.comboBoxCertificateType.Enabled = false;
                        this.textBoxCertificateNumber.Enabled = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        public class FaceLibSearchByPicCon
        {
            public int searchResultPosition { get; set; }
            public int maxResults { get; set; }
            public class cFDID
            {
                public string FDID { get; set; }
            }
            public List<cFDID> FDLib { get; set; }
            public string dataType { get; set; }
            public string faceURL { get; set; }
            public string startTime { get; set; }
            public string endTime { get; set; }
            public string name { get; set; }
            public string gender { get; set; }
            public string city { get; set; }
            public string certificateType { get; set; }
            public string certificateNumber { get; set; }
            public float similarity { get; set; }
            public string taskID { get; set; }
        }
        public class FaceLibSearchByPicRet
        {
            public string responseStatusStrg { get; set; }
            public string taskID { get; set; }
            public int numOfMatches { get; set; }
            public int totalMatches { get; set; }
            public class cMatchList
            {
                public string FPID { get; set; }
                public string FDID { get; set; }
                public string FDName { get; set; }
                public string faceURL { get; set; }
                public string name { get; set; }
                public string gender { get; set; }
                public string bornTime { get; set; }
                public string city { get; set; }
                public string certificateType { get; set; }
                public string certificateNumber { get; set; }
                public string caseInfo { get; set; }
                public string tag { get; set; }
                public string address { get; set; }
                public string customInfo { get; set; }
                public float similarity { get; set; }
            }
            public List<cMatchList> MatchList { get; set; }
        }

        public bool FaceLibSearchByPic(FaceLibSearchByPicCon con, out FaceLibSearchByPicRet ret)
        {
            string str = string.Empty;
            ret = null;
            if (null == con)
            {
                return false;
            }
            IDeviceTree.DeviceInfo struDeviceInfo = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo();
            string strSearchID = Guid.NewGuid().ToString();
            string strxmlSID = "<searchID>" + strSearchID + "</searchID>\r\n";
            string strxmlSRP = "<searchResultPosition>" + con.searchResultPosition + "</searchResultPosition>\r\n";
            string strxmlMR = "<maxResults>" + con.maxResults + "</maxResults>\r\n";
            string strFDID = string.Empty;
            if (con.FDLib.Count > 0)
            {
                for (int i = 0; i < con.FDLib.Count; i++)
                {
                    if (i == 0)
                    {
                        strFDID = con.FDLib[0].FDID;
                    }
                    else
                    {
                        strFDID = strFDID+","+con.FDLib[i].FDID;
                    }
                }
            }
            string strxmlFDID = "<FDID>" + strFDID + "</FDID>\r\n";
            string strxmlStartTime = "<startTime>" + con.startTime + "</startTime>\r\n";
            string strxmlEndTime = "<endTime>" + con.endTime + "</endTime>\r\n";
            string strxmlName = "<name>" + con.name + "</name>\r\n";
            string strxmlSex = "<sex>" + con.gender + "</sex>\r\n";
            string strxmlCity = "<city>" + con.city + "</city>\r\n";
            string strxmlCertificateType = "<certificateType>" + con.certificateType + "</certificateType>\r\n";
            string strxmlCertificateNumber = "<certificateNumber>" + con.certificateNumber + "</certificateNumber>\r\n";
            string strModeData = "<FaceModeList>\r\n<FaceMode>\r\n<ModeInfo>\r\n<similarity>" + con.similarity + "</similarity>\r\n<modeData>" + m_strModeData + "</modeData>\r\n</ModeInfo>\r\n</FaceMode>\r\n</FaceModeList>";
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<FDSearchDescription version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n");
            strBuilder.Append(strxmlSID);
            strBuilder.Append(strxmlSRP);
            strBuilder.Append(strxmlMR);
            strBuilder.Append(strxmlFDID);
            strBuilder.Append(strxmlStartTime);
            strBuilder.Append(strxmlEndTime);
            strBuilder.Append(strxmlName);
            strBuilder.Append(strxmlSex);
            strBuilder.Append(strxmlCity);
            strBuilder.Append(strxmlCertificateType);
            strBuilder.Append(strxmlCertificateNumber);
            strBuilder.Append(strModeData);
            strBuilder.Append("</FDSearchDescription>\r\n");
            string strInput = strBuilder.ToString();       
            string strUrl = "/ISAPI/Intelligent/FDLib/FDSearch";
            string strMethod = "POST";
            bool res = CommonMethod.DoRequest(struDeviceInfo, strMethod, strUrl, strInput, out str);
            if (!res)
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Face image query failed ，Error number：" + iLastErr; //人脸照片删除失败，输出错误号
                MessageBox.Show(strErr);
                return false;
            }
            else
            {    
                try
                {
                    ret= new FaceLibSearchByPicRet();
                    ret.MatchList = new List<FaceLibSearchByPicRet.cMatchList>();
                    XmlDocument fdxml = new XmlDocument();//新建对象
                    fdxml.LoadXml(str);
                    ret.responseStatusStrg = fdxml.GetElementsByTagName("responseStatusStrg").Item(0).InnerText;
                    ret.numOfMatches = Convert.ToInt32(fdxml.GetElementsByTagName("numOfMatches").Item(0).InnerText);
                    ret.totalMatches = Convert.ToInt32(fdxml.GetElementsByTagName("totalMatches").Item(0).InnerText);
                    //寻找所有<MatchElement>节点
                    XmlNodeList fdlist = fdxml.GetElementsByTagName("MatchElement");
                    foreach (XmlNode fdnode in fdlist)
                    {
                        FaceLibSearchByPicRet.cMatchList matchlist = new FaceLibSearchByPicRet.cMatchList();
                        int n = fdnode.ChildNodes.Count;
                        for (int i = 0; i < n; i++)
                        {
                            if (fdnode.ChildNodes[i].Name == "name")
                            {
                                matchlist.name = fdnode["name"].InnerText;
                            }
                            else if (fdnode.ChildNodes[i].Name == "picURL")
                            {
                                matchlist.faceURL = fdnode["picURL"].InnerText;
                            }
                            else if (fdnode.ChildNodes[i].Name == "FDID")
                            {
                                matchlist.FDID = fdnode["FDID"].InnerText;
                            }
                            else if (fdnode.ChildNodes[i].Name == "bornTime")
                            {
                                matchlist.bornTime = fdnode["bornTime"].InnerText;
                            }
                            else if (fdnode.ChildNodes[i].Name == "city")
                            {
                                matchlist.city = fdnode["city"].InnerText;
                            }
                            else if (fdnode.ChildNodes[i].Name == "sex")
                            {
                                matchlist.gender = fdnode["sex"].InnerText;
                            }
                            else if (fdnode.ChildNodes[i].Name == "PID")
                            {
                                matchlist.FPID = fdnode["PID"].InnerText;
                            }
                            else if (fdnode.ChildNodes[i].Name == "certificateNumber")
                            {
                                matchlist.certificateNumber = fdnode["certificateNumber"].InnerText;
                            }
                            else if (fdnode.ChildNodes[i].Name == "certificateType")
                            {
                                matchlist.certificateType = fdnode["certificateType"].InnerText;
                            }
                            else if (fdnode.ChildNodes[i].Name == "similarity")
                            {
                                matchlist.similarity = Convert.ToSingle(fdnode["similarity"].InnerText);
                            }
                        }
                        string face_picurl = fdnode["picURL"].InnerText;
                        ret.MatchList.Add(matchlist);
                    }
                    return true;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }
            }
           
        }
        public class xmlFDLib
        {
            public string FDID { get; set; }
            public string faceLibType { get; set; }
            public string name { get; set; }
            public string customInfo { get; set; }
        }
        public class xmlAllFDLibtInfo
        {
            public int errorCode { get; set; }
            public string errorMsg { get; set; }
            public List<xmlFDLib> FDLib { get; set; }

        }
        xmlAllFDLibtInfo m_xmlFDLibInfo;

        public void GetBlockFDList()
        {
            IDeviceTree.DeviceInfo struDeviceInfo = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo();
            string strRequestUrl = "/ISAPI/Intelligent/FDLib\r\n";
            string strMethod = "GET";
            string strInputParam = "";
            string strOutputParam = "";
            bool res = CommonMethod.DoRequest(struDeviceInfo, strMethod, strRequestUrl, strInputParam, out strOutputParam);
            m_xmlFDLibInfo = new xmlAllFDLibtInfo();
            m_xmlFDLibInfo.FDLib = new List<xmlFDLib>();
            if (res)
            {
                try
                {
                    XmlDocument fdlibxml = new XmlDocument();//新建对象
                    fdlibxml.LoadXml(strOutputParam);
                    //寻找所有<FDLibBaseCfg>节点
                    XmlNodeList fdliblist = fdlibxml.GetElementsByTagName("FDLibBaseCfg");
                    foreach (XmlNode fdlibnode in fdliblist)
                    {
                        int n = fdlibnode.ChildNodes.Count;
                        xmlFDLib xfdlib = new xmlFDLib();
                        for (int i = 0; i < n; i++)
                        {
                            if (fdlibnode.ChildNodes[i].Name == "name")
                            {
                                xfdlib.name = fdlibnode["name"].InnerText;
                            }
                            if (fdlibnode.ChildNodes[i].Name == "FDID")
                            {
                                xfdlib.FDID = fdlibnode["FDID"].InnerText;
                            }
                            if (fdlibnode.ChildNodes[i].Name == "customInfo")
                            {
                                xfdlib.customInfo = fdlibnode["customInfo"].InnerText;
                            }
                            if (fdlibnode.ChildNodes[i].Name == "faceLibType")
                            {
                                xfdlib.faceLibType = fdlibnode["faceLibType"].InnerText;
                            }                       
                        }
                        m_xmlFDLibInfo.FDLib.Add(xfdlib);
                    }
                    for (int i = 0; i < m_xmlFDLibInfo.FDLib.Count; i++)   //Add row data
                    {

                        this.checkedListBoxFDlib.Items.Add(m_xmlFDLibInfo.FDLib[i].name);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
            else
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Face datalib query failed ,Error number：" + iLastErr; //人脸库查询失败，输出错误号
                MessageBox.Show(strErr);
            }
        }
    }
}
