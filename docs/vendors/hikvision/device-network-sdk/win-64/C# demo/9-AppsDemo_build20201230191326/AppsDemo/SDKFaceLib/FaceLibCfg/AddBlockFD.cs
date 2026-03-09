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
    public partial class AddBlockFD : Form
    {
        private IDeviceTree g_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        public AddBlockFD()
        {
            InitializeComponent();
            //m_struFDlib = new jsonFDLib();
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.m_CustomInfo.Enabled = false;
        }

        private void btn_Cancle_Click(object sender, EventArgs e)
        {
            this.Close();
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btn_Sure_Click(object sender, EventArgs e)
        {
            AddFD();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
