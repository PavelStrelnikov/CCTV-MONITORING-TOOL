using System;
using Common;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDKFaceLib
{
    public partial class FaceLibSearchByPicDetails : Form
    {
        public FaceLibSearchByPicDetails()
        {
            InitializeComponent();
        }

        public FaceLibSearchByPicForm.FaceLibSearchByPicRet.cMatchList FaceInfo { get; set; }

        private void FaceLibSearchByPicDetails_Load(object sender, EventArgs e)
        {
            IDeviceTree.DeviceInfo struDeviceInfo = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo();
            if (this.FaceInfo == null)
            {
                return;
            }
            string strId = "FPID:" + FaceInfo.FPID + "    FDID:" + FaceInfo.FDID + "   name:" + FaceInfo.name + "   gender:" + FaceInfo.gender+"\r\n";
            string strBornTime = string.Format("{0,-30}", "bornTime:"+ FaceInfo.bornTime);
            string strCity = string.Format("{0,-30}", "city:" + FaceInfo.city);
            string strCertificateType = string.Format("{0,-30}", "certificateType:" + FaceInfo.certificateType);
            string strCertificateNumber = string.Format("{0,-40}", "certificateNumber:" + FaceInfo.certificateNumber);
            string strCaseInfo = string.Format("{0,-30}", "caseInfo:" + FaceInfo.caseInfo);
            string tag = string.Format("{0,-30}", "tag:" + FaceInfo.tag);
            string address = string.Format("{0,-30}", "address:" + FaceInfo.address);
            string customInfo = string.Format("{0,-30}", "customInfo:" + FaceInfo.customInfo);
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append(strId);
            strBuilder.Append(strBornTime);
            strBuilder.Append(strCity);
            strBuilder.Append(strCertificateType);
            strBuilder.Append(strCertificateNumber);
            strBuilder.Append("\r\n");
            strBuilder.Append(strCaseInfo);
            strBuilder.Append(tag);
            strBuilder.Append(address);
            strBuilder.Append(customInfo);
            string strInfo = Convert.ToString(strBuilder);
            this.textBoxInfo.Text = strInfo;
            //this.textBoxInfo.Text = "FPID:" + FaceInfo.FPID + "    FDID:" + FaceInfo.FDID
            //                + "   name" + FaceInfo.name + " gender:" + FaceInfo.gender + "   \r\nbornTime:" + FaceInfo.bornTime + "   city:"
            //                + FaceInfo.city + "  certificateType:" + FaceInfo.certificateType + "   certificateNumber" + FaceInfo.certificateNumber + " \r\ncaseInfo:"
            //                + FaceInfo.caseInfo + "   tag:" + FaceInfo.tag + "   address:" + FaceInfo.address + "   customInfo:" + FaceInfo.customInfo;
            this.textBoxInfo.Select(this.textBoxInfo.Text.Length, 0);
            string picurl = FaceInfo.faceURL;
            WebRequest webreqface = WebRequest.Create(picurl);
            webreqface.Credentials = new NetworkCredential(struDeviceInfo.sUsername, struDeviceInfo.sPassword);
            WebResponse webresface = webreqface.GetResponse();
            using (Stream stream = webresface.GetResponseStream())
            {
                this.pictureBoxBackground.Image = Image.FromStream(stream);
                stream.Close();
                stream.Dispose();
                System.GC.Collect();
            }
        }
    }
}
