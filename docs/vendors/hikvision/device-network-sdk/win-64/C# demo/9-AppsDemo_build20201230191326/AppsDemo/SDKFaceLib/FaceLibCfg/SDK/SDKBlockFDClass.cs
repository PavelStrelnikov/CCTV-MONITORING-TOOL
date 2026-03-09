using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Drawing;
using Common;
using System.Xml;
namespace SDKFaceLib
{
    public partial class BlockFDForm
    {
        private uint iLastErr = 0;
        public string m_strFDID;
        public class xmlAllFDLibtInfo
        {
            public int errorCode { get; set; }
            public string errorMsg { get; set; }
            public List<xmlFDLib> FDLib { get; set; }

        }
        public class xmlFDLib
        {
            public string FDID { get; set; }
            public string faceLibType { get; set; }
            public string name { get; set; }
            public string customInfo { get; set; }
        }
        public class CErrorInfo
        {
            public int errorCode { get; set; }
            public string errorMsg { get; set; }
        }
        xmlAllFDLibtInfo m_xmlFDLibInfo;

        public class xmlFaceData
        {
            public int searchResultPosition { get; set; }
            public int maxResults { get; set; }
            public string faceURL { get; set; }
            public string faceLibType { get; set; }
            public string FDID { get; set; }
            public string startTime { get; set; }
            public string endTime { get; set; }
            public string FPID { get; set; }
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
  
        }
        xmlFaceData m_XmlFaceData;
        
        public class xmlFaceDataList
        {
            public int errorCode { get; set; }
            public string errorMsg { get; set; }
            public string responseStatusStrg { get; set; }
            public int searchResultPosition { get; set; }
            public int numOfMatches { get; set; }
            public int totalMatches { get; set; }
            public List<xmlFaceData> MatchList { get; set; }

        }
        xmlFaceDataList m_XmlFaceDataList;


        public class xmlValue
        {
            public string value { get; set; }
        }
        public class xmlFPID
        {
            public List<xmlValue> FPID { get; set; }

        }
        xmlFPID m_xmlFPID;
        public bool GetSearchAbility()
        {
            IDeviceTree.DeviceInfo struDeviceInfo = g_deviceTree.GetSelectedDeviceInfo();
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
                XmlNode fdliblist = xmlAbility.GetElementsByTagName("FDSearchDescription").Item(0);
                if (fdliblist.InnerText != string.Empty)
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
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Failed to get face query capability ,Error number：" + iLastErr; //人脸库查询失败，输出错误号
                MessageBox.Show(strErr);
                return false;
            }

        }
        public void GetBlockFDList()
        {
            IDeviceTree.DeviceInfo struDeviceInfo = g_deviceTree.GetSelectedDeviceInfo();
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
                    //int n = fdliblist.Count;
                    //XmlNode node11 = null;
                    //XmlNode node22 = null;
                    //for (int i = 0; i < n; i++)
                    //{
                    //    node11 = fdliblist.Item(i);
                    //    string shortname = node11["name"].InnerText;
                    //    Console.WriteLine("{0}", shortname);
                    //}

                    //var a = fdliblist.length;
                    foreach (XmlNode fdlibnode in fdliblist)
                    {
                        xmlFDLib xmlfdlib=new xmlFDLib();
                        int n = fdlibnode.ChildNodes.Count;
                        //xmlFaceData jsfd = new xmlFaceData();
                        for (int i = 0; i < n; i++)
                        {
                            if (fdlibnode.ChildNodes[i].Name == "name")
                            {
                                xmlfdlib.name = fdlibnode["name"].InnerText;
                            }
                            if (fdlibnode.ChildNodes[i].Name == "FDID")
                            {
                                xmlfdlib.FDID = fdlibnode["FDID"].InnerText;
                            }
                            if (fdlibnode.ChildNodes[i].Name == "customInfo")
                            {
                                xmlfdlib.customInfo = fdlibnode["customInfo"].InnerText;
                            }
                            //xmlfdlib.customInfo = fdlibnode["customInfo"].InnerText;                         
                        }
                        m_xmlFDLibInfo.FDLib.Add(xmlfdlib);
                    }
                    this.listView_FD.BeginUpdate();   //By updating the data, the UI is suspended until the slipdate is drawn, the flash can be avoided effectively and the loading speed is greatly increased
                    //Remove all items
                    this.listView_FD.Items.Clear();
                    for (int i = 0; i < m_xmlFDLibInfo.FDLib.Count; i++)   //Add row data
                    {
                    
                        ListViewItem lvi = new ListViewItem();
                        lvi.ImageIndex = 0;     //By binding to the imageList
                        lvi.Text = "  " + m_xmlFDLibInfo.FDLib[i].name;
                        this.listView_FD.Items.Add(lvi);
                    }
                    this.listView_FD.EndUpdate();  //End data processing, the UI interface is drawn once.
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
            else 
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Get facelib failed ,Error number：" + iLastErr; //人脸库查询失败，输出错误号
                MessageBox.Show(strErr);
            }
        }

        public void DeleteFD()
        {
            IDeviceTree.DeviceInfo struDeviceInfo = g_deviceTree.GetSelectedDeviceInfo();
            string strRequestUrl = "/ISAPI/Intelligent/FDLib/"+m_strFDID+"\r\n";
            string strMethod = "DELETE";
            string strInputParam = "";
            string strOutputParam = "";
            bool res = CommonMethod.DoRequest(struDeviceInfo, strMethod, strRequestUrl, strInputParam, out strOutputParam);
            if (!res)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "人脸库删除失败，错误号：" + iLastErr; //人脸库删除失败，输出错误号
                MessageBox.Show(strErr);
            }
        }

        public void DeleteFacePicture()
        {
            string strFPID = m_XmlFaceDataList.MatchList[m_iSelectedFaceDataIndex].FPID;
            IDeviceTree.DeviceInfo struDeviceInfo = g_deviceTree.GetSelectedDeviceInfo();
            string strRequestUrl = "/ISAPI/Intelligent/FDLib/" + m_strFDID + "/picture/" + strFPID + "\r\n";
            string strMethod = "DELETE";
            string strInputParam = "";
            string strOutputParam = "";
            bool res = CommonMethod.DoRequest(struDeviceInfo, strMethod, strRequestUrl, strInputParam, out strOutputParam);
            if (!res)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Face datalib delete failed ，Error number：" + iLastErr; //人脸照片删除失败，输出错误号
                MessageBox.Show(strErr);
            }
        }



        public void GetAllFaceData(string strFDID)
         {
                IDeviceTree.DeviceInfo struDeviceInfo = g_deviceTree.GetSelectedDeviceInfo();
                this.m_flowLayoutPanelPictureData.Controls.Clear();

                string strSearchID = Guid.NewGuid().ToString();
                string strxmlSID = "<searchID>" + strSearchID + "</searchID>\r\n";
                string strxmlSRP = "<searchResultPosition>" + "0" + "</searchResultPosition>\r\n";
                string strxmlMR = "<maxResults>" + "100" + "</maxResults>\r\n";
                string strxmlFDID = "<FDID>" + strFDID + "</FDID>\r\n";
                string strxmlName = "<name>" + "" + "</name>\r\n";
                StringBuilder strBuilder = new StringBuilder();
                strBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<FDSearchDescription version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n");
                strBuilder.Append(strxmlSID);
                strBuilder.Append(strxmlSRP);
                strBuilder.Append(strxmlMR);
                strBuilder.Append(strxmlFDID);
                strBuilder.Append(strxmlName);
                strBuilder.Append("</FDSearchDescription>\r\n");
                string strInput = strBuilder.ToString();
                //string strInput = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<FDSearchDescription version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n" +
                //                strxmlSID + strxmlSRP + strxmlMR + strxmlFDID + strxmlStartTime + strxmlEndTime + strxmlName + strxmlCity + strxmlcertificateNumber + "</FDSearchDescription>\r\n";
                string strOutput = "";
                string strUrl = "/ISAPI/Intelligent/FDLib/FDSearch";
                string strMethod = "POST";
                bool res = CommonMethod.DoRequest(struDeviceInfo, strMethod, strUrl, strInput, out strOutput);
                m_XmlFaceDataList = new xmlFaceDataList();
                m_XmlFaceDataList.MatchList = new List<xmlFaceData>();
                if (!res)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    string strErr = "Face image query failed ,Error number：" + iLastErr; //人脸照片删除失败，输出错误号
                    MessageBox.Show(strErr);
                }
                else
                {
                    try
                    {
                        XmlDocument fdxml = new XmlDocument();//新建对象
                        fdxml.LoadXml(strOutput);
                        //寻找所有<MatchElement>节点
                        XmlNodeList fdlist = fdxml.GetElementsByTagName("MatchElement");
                        foreach (XmlNode fdnode in fdlist)
                        {
                            int n = fdnode.ChildNodes.Count;
                            xmlFaceData xmlfd = new xmlFaceData();
                            for (int i = 0; i < n; i++)
                            {
                                if (fdnode.ChildNodes[i].Name == "name")
                                {
                                    xmlfd.name = fdnode["name"].InnerText;
                                }
                                else if (fdnode.ChildNodes[i].Name == "picURL")
                                {
                                    xmlfd.faceURL = fdnode["picURL"].InnerText;
                                }
                                else if (fdnode.ChildNodes[i].Name == "FDID")
                                {
                                    xmlfd.FDID = fdnode["FDID"].InnerText;
                                }
                                else if (fdnode.ChildNodes[i].Name == "bornTime")
                                {
                                    xmlfd.bornTime = fdnode["bornTime"].InnerText;
                                }
                                else if (fdnode.ChildNodes[i].Name == "city")
                                {
                                    xmlfd.city = fdnode["city"].InnerText;
                                }
                                else if (fdnode.ChildNodes[i].Name == "sex")
                                {
                                    xmlfd.gender = fdnode["sex"].InnerText;
                                }
                                else if (fdnode.ChildNodes[i].Name == "PID")
                                {
                                    xmlfd.FPID = fdnode["PID"].InnerText;
                                }
                                else if (fdnode.ChildNodes[i].Name == "certificateType")
                                {
                                    xmlfd.certificateType = fdnode["certificateType"].InnerText;
                                }
                                else if (fdnode.ChildNodes[i].Name == "certificateNumber")
                                {
                                    xmlfd.certificateNumber = fdnode["certificateNumber"].InnerText;
                                }

                            }                           
                            string face_picurl = fdnode["picURL"].InnerText;
                            m_XmlFaceDataList.MatchList.Add(xmlfd);
                            WebRequest webreq = WebRequest.Create(face_picurl);
                            webreq.Credentials = new NetworkCredential(this.g_deviceTree.GetSelectedDeviceInfo().sUsername, this.g_deviceTree.GetSelectedDeviceInfo().sPassword);
                            WebResponse webres = webreq.GetResponse();
                            using (Stream stream = webres.GetResponseStream())
                            {
                                Image img = null;
                                try
                                {
                                    img = Image.FromStream(stream, true, false);
                                }
                                catch (Exception ex) { }
                                this.AddPicture(img);
                                stream.Close();
                                stream.Dispose();
                                System.GC.Collect();
                            }
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                }
         }

         public class FaceInfo
         {
             public int errorCode { get; set; }
             public string errorMsg { get; set; }
             public string responseStatusStrg { get; set; }
             public int numOfMatches { get; set; }
             public int totalMatches { get; set; }
             public class MatchItem
             {
                 public string FPID { get; set; }
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
             }
             public List<MatchItem> MatchList { get; set; }
         }
    }
}
