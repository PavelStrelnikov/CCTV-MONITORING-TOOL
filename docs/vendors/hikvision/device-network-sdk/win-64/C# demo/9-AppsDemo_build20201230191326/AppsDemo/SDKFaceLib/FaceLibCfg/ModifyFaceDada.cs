using Common;
using System;
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
    public partial class ModifyFaceDada : Form
    {
        private IDeviceTree g_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        public string m_strFDID;
        public string m_strFPID;
        private BlockFDForm.xmlFaceData m_xmlFaceData = new BlockFDForm.xmlFaceData();
        public ModifyFaceDada()
        {
            InitializeComponent();
        }


        /** @fn void FaceLib.ModifyFaceDada.m_Sure_Click(object sender, EventArgs e)
         *  @brief sure
         *  @param (in)	object sender    
         *  @param (in)	EventArgs e    
         *  @return void
         */
        
        private void m_Sure_Click(object sender, EventArgs e)
        {
            ModifyFaceData();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        /** @fn void FaceLib.ModifyFaceDada.m_Cancel_Click(object sender, EventArgs e)
         *  @brief  Cancel
         *  @param (in)	object sender    
         *  @param (in)	EventArgs e    
         *  @return void
         */
        
        private void m_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void showFaceDetail(BlockFDForm.xmlFaceData xmlFaceData)
        {
            m_xmlFaceData = xmlFaceData;
            m_Name.Text = m_xmlFaceData.name;
            m_Gender.Text = m_xmlFaceData.gender;
            try
            {
                if (m_xmlFaceData.bornTime != null)
                {
                    m_BornTime.Value = DateTime.Parse(m_xmlFaceData.bornTime);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Time fomat error!");
            }
            m_City.Text = m_xmlFaceData.city;
            m_CeriNum.Text = m_xmlFaceData.certificateNumber;
            m_Type.Text = m_xmlFaceData.certificateType;
            m_textBoxUrl.Text = m_xmlFaceData.faceURL;
            m_strFPID = m_xmlFaceData.FPID;
            m_Type.Text = m_xmlFaceData.certificateType;
            string face_picurl = m_xmlFaceData.faceURL;
            WebRequest webreq = WebRequest.Create(face_picurl);
            webreq.Credentials = new NetworkCredential(this.g_deviceTree.GetSelectedDeviceInfo().sUsername, this.g_deviceTree.GetSelectedDeviceInfo().sPassword);
            WebResponse webres = webreq.GetResponse();
            using (Stream stream = webres.GetResponseStream())
            {
                Image img = Image.FromStream(stream);
                pictureBox1.Image = img;
                stream.Close();
                stream.Dispose();
                System.GC.Collect();
            }
        }
    }
}
