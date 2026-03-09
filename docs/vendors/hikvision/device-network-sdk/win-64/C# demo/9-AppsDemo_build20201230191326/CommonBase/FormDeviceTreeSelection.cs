using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common
{
    public partial class FormDeviceTreeSelection : Form
    {
        public FormDeviceTreeSelection()
        {
            InitializeComponent();
        }

        public List<string> DeviceTreeList { get; set; }
        public int SelectedDeviceTreeIndex { get; set; }

        private void listBoxDeviceTree_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxDeviceTree.SelectedIndex < 0)
            {
                return;
            }
            SelectedDeviceTreeIndex = listBoxDeviceTree.SelectedIndex;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void FormDeviceTreeSelection_Load(object sender, EventArgs e)
        {
            foreach (string item in DeviceTreeList)
            {
                this.listBoxDeviceTree.Items.Add(item);
            }
        }
    }
}
