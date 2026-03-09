using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TINYXMLTRANS;
using Newtonsoft.Json;
using System.Net;
using Common;
using System.IO;
using System.Runtime.InteropServices;

namespace SDKAIOpenPlatform
{
    public partial class ModelManageForm : Form
    {
        public ModelManageForm()
        {
            InitializeComponent();
        }
        private void ModelManageForm_Load(object sender, EventArgs e)
        {
        }
        //预设处理，支持URL模式和推送模式
        private void modelUploadbutton_Click(object sender, EventArgs e)
        {
            //推送模式
            ModelUploadByLocalFileForm modelUploadByLocalFileForm = new ModelUploadByLocalFileForm();
            modelUploadByLocalFileForm.ShowDialog();
        }
    }
}
