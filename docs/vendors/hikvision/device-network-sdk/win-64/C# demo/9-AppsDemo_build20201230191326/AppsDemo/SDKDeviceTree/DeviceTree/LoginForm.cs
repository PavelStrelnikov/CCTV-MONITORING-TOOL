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

namespace SDKDeviceTree
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            LoginTltle.Parent = loginpictureBox;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (textBoxDeviceAddress.Text.Length <= 0 || textBoxDeviceAddress.Text.Length > 128)
            {
                MessageBox.Show("please input the device address in 1 and 128 characters!");
                return;
            }
            if (int.Parse(textBoxPort.Text) <= 0)
            {
                MessageBox.Show("illeage port!");
                return;
            }
            if (textBoxUserName.Text.Length > 32 || textBoxPassword.Text.Length > 16)
            {
                MessageBox.Show("user name should be shorter than 32 BYTES and pwd should be shorter than 16 BYTES!");
                return;
            }

            if (!SDK_Login(true))
            {
                return;
            }            
            this.DialogResult = DialogResult.OK;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            string inifile = "";
            inifile = Application.StartupPath + "\\Config.ini";
            CHCNetSDK.ProtocolType = CHCNetSDK.ReadIniData("Protocol", "ProtocolType", CHCNetSDK.ProtocolType, inifile);
            SDK_Init();           
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AysnLogincheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (AysnLogincheckBox.Checked)
            {
                AysnLoginFlag = true;
            }
            else
            {
                AysnLoginFlag = false;
            }
        }

        private void m_checkBoxTLS_CheckedChanged(object sender, EventArgs e)
        {
            if(m_checkBoxTLS.Checked)
            {
                textBoxPort.Text = "8443";
            }
            else
            {
                textBoxPort.Text = "8000";
            }
        }
    }
}
