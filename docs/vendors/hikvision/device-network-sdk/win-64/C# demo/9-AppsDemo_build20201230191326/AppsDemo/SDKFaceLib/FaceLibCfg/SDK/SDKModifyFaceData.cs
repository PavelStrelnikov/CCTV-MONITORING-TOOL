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

namespace SDKFaceLib
{
    public partial class ModifyFaceDada
    {
        public class xmlFaceRecord
        {
            public string faceURL { get; set; }
            public string name { get; set; }
            public string gender { get; set; }
            public string bornTime { get; set; }
            public string city { get; set; }
            public string certificateType  { get; set; }
            public string certificateNumber { get; set; }
        }
        xmlFaceRecord m_struFaceRecord;

        public void ModifyFaceData()
        {
            m_struFaceRecord = new xmlFaceRecord();
            if (m_textBoxUrl.Text.Length == 0)
            {
                MessageBox.Show("Please upload a picture!");
                return;
            }
            m_struFaceRecord.faceURL = m_textBoxUrl.Text;
            m_struFaceRecord.name = m_Name.Text;
            switch (m_Gender.SelectedIndex)
            {
                case 0:
                    m_struFaceRecord.gender = "male";
                    break;
                case 1:
                    m_struFaceRecord.gender = "female";
                    break;           
                default:                   
                    break;
            }
            m_struFaceRecord.bornTime = m_BornTime.Value.Year.ToString().PadLeft(4, '0') + "-" + m_BornTime.Value.Month.ToString().PadLeft(2, '0') + "-" + m_BornTime.Value.Day.ToString().PadLeft(2, '0');
            m_struFaceRecord.city = m_City.Text;
            switch (m_Type.SelectedIndex)
            {
                case 0:
                    m_struFaceRecord.certificateType = "officerID";
                    break;
                case 1:
                    m_struFaceRecord.certificateType = "ID";
                    break;
                default:
                    m_struFaceRecord.certificateType = "";
                    break;
            }
            m_struFaceRecord.certificateNumber = m_CeriNum.Text;

            IDeviceTree.DeviceInfo struDeviceInfo = g_deviceTree.GetSelectedDeviceInfo();
            string strBornTime = "<bornTime>" + m_struFaceRecord.bornTime + "</bornTime>\r\n";
            string strName = "<name>" + m_struFaceRecord.name + "</name>\r\n";
            string strSex = "<sex>" + m_struFaceRecord.gender + "</sex>\r\n";
            string strCity = "<city>" + m_struFaceRecord.gender + "</city>\r\n";
            string strCertificateType = "<certificateType>" + m_struFaceRecord.certificateType + "</certificateType>\r\n";
            string strCertificateNumber = "<certificateNumber>" + m_struFaceRecord.certificateNumber + "</certificateNumber>\r\n";
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<FaceAppendData version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n");
            strBuilder.Append(strBornTime);
            strBuilder.Append(strName);
            strBuilder.Append(strSex);
            strBuilder.Append(strCity);
            strBuilder.Append(strCertificateType);
            strBuilder.Append(strCertificateNumber);
            strBuilder.Append("</FaceAppendData>\r\n");
            string strInput = strBuilder.ToString();
            //string strInput = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<FaceAppendData version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n" +
            //                  strBornTime + strName + strSex + strCity + strCertificateType + strCertificateNumber + "</FaceAppendData>\r\n";
            string strOutput = "";
            string strRequestUrl = "/ISAPI/Intelligent/FDLib/"+m_strFDID+"/picture/"+m_strFPID+"\r\n";
            string strMethod = "PUT";
            bool res = CommonMethod.DoRequest(struDeviceInfo, strMethod, strRequestUrl, strInput, out strOutput);
            if (!res)
            {

                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Face data modification failed ,Error number：" + iLastErr; //人脸数据修改失败，输出错误号
                MessageBox.Show(strErr);
            }

        }
    }
}
