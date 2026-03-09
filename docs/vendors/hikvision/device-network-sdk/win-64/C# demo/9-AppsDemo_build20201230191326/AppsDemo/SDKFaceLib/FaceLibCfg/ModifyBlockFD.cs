using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDKFaceLib
{
    public partial class ModifyBlockFD : Form
    {
        private IDeviceTree g_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        public string strFDID
        {
            get;
            set;
        }
        public ModifyBlockFD()
        {   
            strFDID = "";
            InitializeComponent();
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.m_CustomInfo.Enabled = false;
        }

        private void btn_Cancle_Click(object sender, EventArgs e)
        {
            this.Close();
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void ModifyBlockFD_Load(object sender, EventArgs e)
        {
            
            GetBlockFD();
        }

        private void btn_Sure_Click(object sender, EventArgs e)
        {
            ModifyFD();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
