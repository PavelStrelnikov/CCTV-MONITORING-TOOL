using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDKFaceLib
{
    public partial class AddFaceDada : Form
    {
        public delegate bool deUploadPic( string strPath);
        private IDeviceTree g_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        private string m_strFileName;
        public string m_strFDID;
        public string m_strFPID;

        public string m_strBornTime;
        public string m_strName;
        public string m_strCity;
        public string m_strType;
        public string m_strSex;
        public string m_strCeriNum;
       
        public AddFaceDada()
        {
            InitializeComponent();
        }

        /** @fn void FaceLib.AddFaceDada.m_UploadPic_Click(object sender, EventArgs e)
         *  @brief  upload pic
         *  @param (in)	object sender    
         *  @param (in)	EventArgs e    
         *  @return void
         */
        
        private void m_UploadPic_Click(object sender, EventArgs e)
        {
            m_struFaceData.bornTime = this.m_BornTime.Value.Year.ToString().PadLeft(4, '0') + "-" + m_BornTime.Value.Month.ToString().PadLeft(2, '0') + "-" + m_BornTime.Value.Day.ToString().PadLeft(2, '0');
            m_struFaceData.name = this.m_Name.Text;
            m_struFaceData.city = this.m_City.Text;
            m_struFaceData.certificateType = this.m_Type.Text;
            m_struFaceData.gender = this.m_Gender.SelectedText;
            m_struFaceData.certificateNumber = this.m_CeriNum.Text;
            m_struFaceData.identityKey = this.txIdentityKey.Text;
            if (this.chkConcurrent.Checked)
            {
                m_struFaceData.ifConcurrent = 1;
            }
            else
            {
                m_struFaceData.ifConcurrent = 0;
            }
            if (this.chkCover.Checked)
            {
                m_struFaceData.ifByCover = 1;
            }
            else
            {
                m_struFaceData.ifByCover = 0;
            }
            UploadPic();
            //3.31.2	/ISAPI/SDT/pictureUpload
            
        }


        /** @fn void FaceLib.AddFaceDada.m_Sure_Click(object sender, EventArgs e)
         *  @brief sure
         *  @param (in)	object sender    
         *  @param (in)	EventArgs e    
         *  @return void
         */
        private void m_Sure_Click(object sender, EventArgs e)
        {
           
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            //Thread thFacePicUpLoad = new Thread(new ThreadStart(PicUpLoadThread));
            //thFacePicUpLoad.Start();
            //while (!thFacePicUpLoad.IsAlive)
            //{
            //    thFacePicUpLoad.Join();
            //}
            
            //deUploadPic deleUpPic = new deUploadPic(PicUpLoadThread);
            //IAsyncResult res = deleUpPic.BeginInvoke(m_strFileName, null, null);
            //while (!res.IsCompleted)
            //{

            //    Thread.Sleep(100);
            //}
            //bool urlresult = deleUpPic.EndInvoke(res);
            //this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        /** @fn void FaceLib.AddFaceDada.m_Cancel_Click(object sender, EventArgs e)
         *  @brief  Cancel
         *  @param (in)	object sender    
         *  @param (in)	EventArgs e    
         *  @return void
         */
        
        private void m_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                //UploadPic(dialog.FileName);
                m_strFileName = dialog.FileName;
                pictureBox1.Image = Image.FromFile(m_strFileName);
            }
        }
    }
}
