using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using TINYXMLTRANS;

namespace SDKFaceLib
{
    public partial class Face1vN : PluginsControl
    {
        private IDeviceTree g_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        const uint XML_ABILITY_OUT_LEN = 3 * 1024 * 1024;
        private bool m_dwReturnValue = false;
        public int m_lServerID = 0;
        public Face1vN()
        {
            m_lServerID = (int)g_deviceTree.GetSelectedDeviceInfo().lLoginID;
            InitializeComponent();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";//Note that you use c:\\ instead of c:\ when writing paths here
            openFileDialog.Filter = "Image files|*.jpg*|All files|*.*";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                m_textBoxPicturePath.Text = openFileDialog.FileName;
            }
        }

        private void buttonComparison_Click(object sender, EventArgs e)
        {
            CompareFace();
        }
    }
}
