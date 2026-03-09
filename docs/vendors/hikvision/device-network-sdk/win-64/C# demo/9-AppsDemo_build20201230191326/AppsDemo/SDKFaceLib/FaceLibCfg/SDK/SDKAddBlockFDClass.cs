using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using Common;
using System.Runtime.InteropServices;

namespace SDKFaceLib
{
    public partial class AddBlockFD
    {
        private uint iLastErr = 0;
        public void AddFD()
        {
            //m_struFDlib = new jsonFDLib();
            if (m_BalckFDName.Text == "")
            {
                MessageBox.Show("Please input the Block FD Name");
                return;
            }
            IDeviceTree.DeviceInfo struDeviceInfo = g_deviceTree.GetSelectedDeviceInfo();
            string strFDName = "<name>" + m_BalckFDName.Text + "</name>\r\n";
            string strCustomInfo = "<customInfo>" + m_CustomInfo.Text + "</customInfo>\r\n";
            StringBuilder strInputBuilder= new StringBuilder();
            strInputBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<CreateFDLibList version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n <CreateFDLib>\r\n");
            strInputBuilder.Append(strFDName);
            strInputBuilder.Append(strCustomInfo);
            strInputBuilder.Append("</CreateFDLib>\r\n</CreateFDLibList>\r\n");
            string strInput = strInputBuilder.ToString();
            //string strInput = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<CreateFDLibList version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n <CreateFDLib>\r\n" +
            //                  strFDName + strCustomInfo + "</CreateFDLib>\r\n</CreateFDLibList>\r\n";
            string strOutput = "";
            string strRequestUrl = "/ISAPI/Intelligent/FDLib\r\n";
            string strMethod = "POST";
            bool res = CommonMethod.DoRequest(struDeviceInfo, strMethod, strRequestUrl, strInput, out strOutput);
            if (!res)
            {

                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Create face datalib failed , Error number：" + iLastErr; //人脸库创建失败，输出错误号
                MessageBox.Show(strErr);
            }
        }
    }
    
}
