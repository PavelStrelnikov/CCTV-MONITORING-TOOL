using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDKFaceLib
{
    public partial class FaceLibSearchByPicCtrl : UserControl
    {
        public FaceLibSearchByPicCtrl()
        {
            InitializeComponent();
        }
        public Image FacePicture
        {
            set { this.pictureBoxFace.Image = value; }
        }
        public string Similarity
        {
            set { this.labelTime.Text = value; }
        }
        public FaceLibSearchByPicForm.FaceLibSearchByPicRet.cMatchList FaceInfo { get; set; }

        private void buttonDetails_Click(object sender, EventArgs e)
        {
            FaceLibSearchByPicDetails faceLibCaptureDetails = new FaceLibSearchByPicDetails();
            faceLibCaptureDetails.FaceInfo = this.FaceInfo;
            faceLibCaptureDetails.ShowDialog();
        }


    }
}
