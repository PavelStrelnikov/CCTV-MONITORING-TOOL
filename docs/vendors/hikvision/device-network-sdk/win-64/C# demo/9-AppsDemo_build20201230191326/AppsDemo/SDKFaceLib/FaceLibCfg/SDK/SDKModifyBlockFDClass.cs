using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Common;
using System.Xml;
namespace SDKFaceLib
{

    public partial class ModifyBlockFD
    {
        public void GetBlockFD()
        {
            IDeviceTree.DeviceInfo struDeviceInfo = g_deviceTree.GetSelectedDeviceInfo();
            string strRequestUrl = "/ISAPI/Intelligent/FDLib/" + strFDID + "\r\n";
            string strMethod = "GET";
            string strInputParam = "";
            string strOutputParam = "";
            bool res = CommonMethod.DoRequest(struDeviceInfo, strMethod, strRequestUrl, strInputParam, out strOutputParam);
            if (!res)
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Get face datalib failed ,Error number：" + iLastErr; //人脸库获取失败，输出错误号
                MessageBox.Show(strErr);
            }
            else
            {
                try
                {
                    XmlDocument fdlibxml = new XmlDocument();//新建对象
                    fdlibxml.LoadXml(strOutputParam);
                    if (fdlibxml.GetElementsByTagName("name").Count > 0)
                    { 
                        m_BalckFDName.Text = fdlibxml.GetElementsByTagName("name").Item(0).InnerText; 
                    }
                    if (fdlibxml.GetElementsByTagName("customInfo").Count > 0)
                    {
                        m_CustomInfo.Text = fdlibxml.GetElementsByTagName("customInfo").Item(0).InnerText;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }

        }

        public void ModifyFD()
        {
            IDeviceTree.DeviceInfo struDeviceInfo = g_deviceTree.GetSelectedDeviceInfo();
            if (m_BalckFDName.Text == "")
            {
                MessageBox.Show("Please input the Block FD Name");
                return;
            }
            string strgetId = "";
            string strInputGet = "";
            string strMethodGet = "GET";
            string strOutGet = "";
            string strRequestUrl = "/ISAPI/Intelligent/FDLib/" + strFDID + "\r\n";
            bool resGet = CommonMethod.DoRequest(struDeviceInfo, strMethodGet, strRequestUrl, strInputGet, out strOutGet);
            try
            {
                XmlDocument fdlibxml = new XmlDocument();//新建对象
                fdlibxml.LoadXml(strOutGet);
                if (fdlibxml.GetElementsByTagName("id").Count > 0)
                {
                    strgetId = fdlibxml.GetElementsByTagName("id").Item(0).InnerText;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            string strMethodPut = "PUT";
            string strId = "<id>" + strgetId + "</id>\r\n";
            string strFDID1 = "<FDID>" + strFDID + "</FDID>\r\n";
            string strName = "<name>" + m_BalckFDName.Text + "</name>\r\n";
            string strCustomInfo = "<customInfo>" + m_CustomInfo.Text + "</customInfo>\r\n";
            string strInputPut = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<FDLibBaseCfg version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n" + strId
                                + strFDID1 + strName + strCustomInfo + "</FDLibBaseCfg>\r\n";
            string strOutput = "";
           
            bool res = CommonMethod.DoRequest(struDeviceInfo, strMethodPut, strRequestUrl, strInputPut, out strOutput);
            if (!res)
            {

                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Face datalib modification failed，Error number：" + iLastErr; //人脸库修改失败，输出错误号
                MessageBox.Show(strErr);
            }
            //bool resGet1 = CommonMethod.DoRequest(struDeviceInfo, strMethodGet, strRequestUrl, strInputGet, out strOutGet);
        }
    }
}
