using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TINYXMLTRANS;
using System.Runtime.InteropServices;
using System.Xml;
using Common;

namespace SDKANPR
{
    public partial class ANPRForm : PluginsControl
    {
        public ANPRForm()
        {
            InitializeComponent();
        }

        //http通信客户端
        public static WebClient m_webClient = new WebClient();
        //设备树对象映射
        IDeviceTree m_deviceTree = PluginsFactory.GetDeviceTreeInstance();

        private void FormANPRform_Load(object sender, EventArgs e)
        {
            //获取设备IP、用户名、密码
            IDeviceTree.DeviceInfo loginInfo = m_deviceTree.GetSelectedDeviceInfo();

            //挂载主页
            this.MainF_TabBOX.DrawItem += new DrawItemEventHandler(this.MainF_TabBOX_DrawItem);
            this.MainF_TabBOX.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainF_TabBOX_MouseDown);
            Show_Form("block allow list config", MainF_TabBOX);
            this.panelBase.Enabled = true;
        }

        #region  窗体的调用
        /// <summary>
        /// 窗体的调用.
        /// </summary>
        /// <param name="FrmName">调用窗体的Text属性值</param>
        public static void Show_Form(string FrmName, System.Windows.Forms.TabControl tab)
        {
            if (FrmName == "block allow list config")  //判断当前要打开的窗体
            {
                LicensePlateAuditForm LicenseForm = new LicensePlateAuditForm();
                Add_TabPage(FrmName, LicenseForm, tab);
            }
            else if (FrmName == "ANPR布防")  //判断当前要打开的窗体
            {
                ANPRAlarmForm ANPRAlarmForm = new ANPRAlarmForm();
                Add_TabPage(FrmName, ANPRAlarmForm, tab);
            }
            else if (FrmName == "车牌识别")  //判断当前要打开的窗体
            {
                licensePlateRecognitionForm LPRForm = new licensePlateRecognitionForm();
                Add_TabPage(FrmName, LPRForm, tab);
            }
            else if (FrmName == "ANPR监听")  //判断当前要打开的窗体
            {
                ANPRListenForm ANPRListenForm = new ANPRListenForm();
                Add_TabPage(FrmName, ANPRListenForm, tab);
            }
            else if (FrmName == "ManualSnap")
            {
                ManualSnapForm ManualSnapForm = new ManualSnapForm();
                Add_TabPage(FrmName, ManualSnapForm, tab);
            }
        }
        #endregion

        #region  主窗口菜单控制
        const int CLOSE_SIZE = 14;
        private void MainF_TabBOX_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            try
            {
                //修改主控件背景色
                Rectangle TabMainrec = MainF_TabBOX.ClientRectangle;
                SolidBrush TabBoxBac = new SolidBrush(Color.WhiteSmoke);
                e.Graphics.FillRectangle(TabBoxBac, TabMainrec);

                for (int i = 0; i < MainF_TabBOX.TabPages.Count; i++)
                {
                    //绘制标签背景色
                    Color recColor = MainF_TabBOX.SelectedIndex == i ? Color.DodgerBlue : Color.White;
                    SolidBrush recColorBrush = new SolidBrush(recColor);
                    Rectangle myTabRect = this.MainF_TabBOX.GetTabRect(i);
                    e.Graphics.FillRectangle(recColorBrush, myTabRect);

                    //先添加TabPage字体属性
                    Color StringColor = MainF_TabBOX.SelectedIndex == i ? Color.White : Color.Gray;
                    SolidBrush StringColorBrush = new SolidBrush(StringColor);
                    e.Graphics.DrawString(this.MainF_TabBOX.TabPages[i].Text
                    , this.Font, StringColorBrush, myTabRect.X + 2, myTabRect.Y + 2);

                    //再画一个矩形框
                    using (Pen p = new Pen(recColor))
                    {
                        myTabRect.Offset(myTabRect.Width - (CLOSE_SIZE + 3), 2);
                        myTabRect.Width = CLOSE_SIZE;
                        myTabRect.Height = CLOSE_SIZE;
                        e.Graphics.DrawRectangle(p, myTabRect);
                    }

                    //填充矩形框                   
                    using (Brush b = new SolidBrush(recColor))
                    {
                        e.Graphics.FillRectangle(b, myTabRect);
                    }

                    //画关闭符号
                    Color closeColor = MainF_TabBOX.SelectedIndex == i ? Color.White : Color.Gray;
                    using (Pen p = new Pen(closeColor))
                    {
                        //画"/"线
                        Point p1 = new Point(myTabRect.X + 3, myTabRect.Y + 3);
                        Point p2 = new Point(myTabRect.X + myTabRect.Width - 3, myTabRect.Y + myTabRect.Height - 3);
                        e.Graphics.DrawLine(p, p1, p2);

                        //画"/"线
                        Point p3 = new Point(myTabRect.X + 3, myTabRect.Y + myTabRect.Height - 3);
                        Point p4 = new Point(myTabRect.X + myTabRect.Width - 3, myTabRect.Y + 3);
                        e.Graphics.DrawLine(p, p3, p4);
                    }
                }
                e.Graphics.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);   //提示信息
            }
        }
        //*4，*/在tabControl的MouseDown事件中添加关闭的动作，OK
        private void MainF_TabBOX_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int x = e.X, y = e.Y;

                //计算关闭区域   
                Rectangle myTabRect = this.MainF_TabBOX.GetTabRect(this.MainF_TabBOX.SelectedIndex);

                myTabRect.Offset(myTabRect.Width - (CLOSE_SIZE + 3), 2);
                myTabRect.Width = CLOSE_SIZE;
                myTabRect.Height = CLOSE_SIZE;

                //如果鼠标在区域内就关闭选项卡   
                bool isClose = x > myTabRect.X && x < myTabRect.Right
                 && y > myTabRect.Y && y < myTabRect.Bottom;

                if (isClose == true)
                {
                    int index = this.MainF_TabBOX.SelectedIndex;
                    //this.MainF_TabBOX.TabPages.Remove(this.MainF_TabBOX.SelectedTab);                   
                    if (index > 0)
                    {
                        this.MainF_TabBOX.SelectedIndex = index - 1;
                    }
                    ((Form)this.MainF_TabBOX.TabPages[index].Controls[0]).Close();
                    this.MainF_TabBOX.TabPages.RemoveAt(index);
                }
            }
        }
        public static void Add_TabPage(string str, Form myForm, System.Windows.Forms.TabControl tab)
        {
            if (tabControlCheckHave(tab, str))
            {
                return;
            }
            else
            {
                String strtemp = "    " + str + "    ";
                tab.TabPages.Add(strtemp);
                tab.SelectTab(tab.TabPages.Count - 1);
                myForm.Dock = DockStyle.Fill;
                myForm.TopLevel = false;
                myForm.Show();
                myForm.Parent = tab.SelectedTab;
            }

        }
        public static bool tabControlCheckHave(System.Windows.Forms.TabControl tab, String tabName)
        {
            for (int i = 0; i < tab.TabCount; i++)
            {
                if (tab.TabPages[i].Text.Trim() == tabName)
                {
                    tab.SelectedIndex = i;
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region  导航栏切换
        private void LicensePlateAuditMenuItem_Click(object sender, EventArgs e)
        {
            Show_Form("block allow list config", MainF_TabBOX);
        }

        private void ANPRAlarmMenuItem_Click(object sender, EventArgs e)
        {
            Show_Form("ANPR布防", MainF_TabBOX);
        }

        private void licensePlateRecognitionItem_Click(object sender, EventArgs e)
        {
            Show_Form("车牌识别", MainF_TabBOX);
        }

        private void ANPRListenMenuItem_Click(object sender, EventArgs e)
        {
            Show_Form("ANPR监听", MainF_TabBOX);
        }

        private void ITCEntranceMenuItem_Click(object sender, EventArgs e)
        {
            Show_Form("ITC block allow list config", MainF_TabBOX);
        }

        private void ManualSnapMenuItem_Click(object sender, EventArgs e)
        {
            Show_Form("ManualSnap", MainF_TabBOX);
        }

        #endregion
      
    }
}
