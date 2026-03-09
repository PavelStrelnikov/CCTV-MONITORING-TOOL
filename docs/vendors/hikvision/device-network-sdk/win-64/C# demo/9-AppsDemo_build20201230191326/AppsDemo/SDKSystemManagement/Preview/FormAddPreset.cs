using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDKSystemManagement.Preview
{
    public partial class FormAddPreset : Form
    {
        public FormAddPreset()
        {
            InitializeComponent();
        }

        public string PresetName { get; set; }

        private void buttonYes_Click(object sender, EventArgs e)
        {
            this.PresetName = this.textBoxPresetName.Text;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void buttonNo_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
