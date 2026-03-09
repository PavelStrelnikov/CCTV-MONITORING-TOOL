using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using TINYXMLTRANS;
using System.Net;
using WeifenLuo.WinFormsUI.Docking;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Common;

namespace AppsDemo
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
        }

        private FormDeviceContainer m_formDeviceTreeContainer = new FormDeviceContainer();
        private FormMenuContainer m_formMenuContainer = new FormMenuContainer();
        private FormLogs m_formLogs = new FormLogs();

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        void PluginsFactory_OnPluginsChanged()
        {
            this.LoadPlugins();
        }

        private delegate void LoadPluginsHandler();

        private void LoadPlugins()
        {
            if (this.InvokeRequired)
            {
                LoadPluginsHandler handler = new LoadPluginsHandler(this.LoadPlugins);
                this.BeginInvoke(handler);
            }
            else
            {
                try
                {
                    //先把原来的删掉
                    this.m_formDeviceTreeContainer.Controls.Clear();
                    //获取设备树
                    IDeviceTree iDeviceTree = PluginsFactory.GetDeviceTreeInstance();
                    IDeviceTree.EDeviceTreeType deviceType = iDeviceTree.GetDeviceTreeType();

                    if (iDeviceTree != null)
                    {
                        this.m_formDeviceTreeContainer.Text = iDeviceTree.GetDeviceTreeName();
                        UserControl deviceTree = iDeviceTree.GetDeviceTreeControl();
                        if (deviceTree != null)
                        {
                            //添加设备树
                            this.m_formDeviceTreeContainer.Controls.Add(deviceTree);
                            deviceTree.Dock = DockStyle.Fill;
                            
                        }
                    }

                    //获取菜单
                    List<string> menuLists = new List<string>();
                    List<PluginsFactory.PluginMenu> menuList = PluginsFactory.GetPluginsMenus();
                    foreach (PluginsFactory.PluginMenu menu in menuList)
                    {
                        foreach (string subMenu in menu.SubMenuList)
                        {
                            //mix
                            if (deviceType == IDeviceTree.EDeviceTreeType.ISAPIDeviceTree)
                            {
                                //ISAPIDeviceTree
                                if (subMenu.Contains("ISAPI"))
                                {
                                    menuLists.Add(subMenu);
                                }
                            }
                            else if (deviceType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
                            {
                                //SDKDeviceTree
                                if (subMenu.Contains("SDK"))
                                {
                                    menuLists.Add(subMenu);
                                }
                            } 
//                             menuLists.Add(subMenu);
                        }
                    }
                    menuLists.Sort();
                    this.m_formMenuContainer.MenuList = menuLists;
                }
                catch (Exception ex)
                {
                    this.labelStatus.Text = ex.Message;
                }
            }
        }

        public delegate void SetStatusStringHandler(MainFormHandler.Level level, string module, string message, string details);

        public void SetStatusString(MainFormHandler.Level level, string module, string message, string details)
        {
#if !DEBUG
            //非Debug模式不显示Debug日志
            if (level == MainFormHandler.Level.Debug)
            {
                return;
            }
#endif
            //防止异步造成崩溃
            if (this.InvokeRequired)
            {
                SetStatusStringHandler handler = new SetStatusStringHandler(SetStatusString);
                this.BeginInvoke(handler, level, module, message, details);
            }
            else
            {
                Color color = this.BackColor;
                switch (level)
                {
                    case MainFormHandler.Level.Debug:
                        color = Color.LightBlue;
                        break;
                    case MainFormHandler.Level.Info:
                        color = Color.LightGreen;
                        break;
                    case MainFormHandler.Level.Error:
                        color = Color.LightPink;
                        break;
                    default:
                        break;
                }
                this.labelStatus.BackColor = color;
                this.labelStatus.Text = "[" + level.ToString() + "] " + module + ":" + message;
                this.m_formLogs.AddLogs(level.ToString(), color, module, message, details);
            }
        }

        //private string m_strDockPanelCfg = Path.Combine(Application.StartupPath, "DockPanel.config");

        private IDockContent GetContentFromPersistString(string persistString)
        {
            return null;
        }

        DeserializeDockContent m_deserializeDockContent = null;
        private void MainForm_Load(object sender, EventArgs e)
        {
            PluginsFactory.LoadPlugins();
            this.LoadPlugins();
            PluginsFactory.OnPluginsChanged += PluginsFactory_OnPluginsChanged;
            MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "MainForm", "Please Login!");
            m_Documents.Clear();
            //if (File.Exists(m_strDockPanelCfg))
            //{
            //    m_deserializeDockContent = new DeserializeDockContent(GetContentFromPersistString);
            //    dockPanel.LoadFromXml(m_strDockPanelCfg, m_deserializeDockContent);
            //}

            m_formLogs.Show(this.dockPanel, DockState.DockBottom);

            m_formDeviceTreeContainer.Show(this.dockPanel, DockState.DockLeft);
            m_formMenuContainer.Text = "Menu";
            m_formMenuContainer.Show(this.dockPanel, DockState.DockRight);
            m_formMenuContainer.MenuItemClicked += m_formMenuContainer_MenuItemClicked;
            m_formMenuContainer.MenuItemDoubleClicked += m_formMenuContainer_MenuItemDoubleClicked;

            //获取设备树
            IDeviceTree iDeviceTree = PluginsFactory.GetDeviceTreeInstance();
            if (iDeviceTree == null)
            {
                return;
            }
            IDeviceTree.EDeviceTreeType deviceType = iDeviceTree.GetDeviceTreeType();
            if (deviceType == IDeviceTree.EDeviceTreeType.ISAPIDeviceTree)
            {
                //ISAPIDeviceTree
                m_formMenuContainer_MenuItemDoubleClicked("ISAPIPreview");
            }
            else if (deviceType == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
            {
                //SDKDeviceTree
                m_formMenuContainer_MenuItemDoubleClicked("SDKPreview");

            } 
//             foreach (string menu in this.m_formMenuContainer.MenuList)
//             {
//                 m_formMenuContainer_MenuItemDoubleClicked(menu);
//             }
        }

        private List<DockContent> m_Documents = new List<DockContent>();

        void m_formMenuContainer_MenuItemDoubleClicked(string menuItem)
        {
            //每个控件只能显示一个实例
            foreach (DockContent doc in m_Documents)
            {
                if (doc.Text == menuItem)
                {
                    doc.Show(this.dockPanel);
                    return;
                }
            }

            PluginsControl ctrl = PluginsFactory.GetPluginsUserControl(menuItem);
            if (ctrl != null)
            {
                ctrl.Text = menuItem;
                ctrl.DockAreas = DockAreas.Document | DockAreas.Float;
                ctrl.Show(this.dockPanel, DockState.Document);
                m_Documents.Add(ctrl);
                m_formMenuContainer.Focus();
            }
            else
            {
                this.labelStatus.Text = "Get plugins[" + menuItem + "] failed!";
            }
        }

        void m_formMenuContainer_MenuItemClicked(string menuItem)
        {
            //每个控件只能显示一个实例
            foreach (DockContent doc in m_Documents)
            {
                if (doc.Text == menuItem)
                {
                    doc.Show(this.dockPanel);
                    return;
                }
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //保存布局
            //dockPanel.SaveAsXml(m_strDockPanelCfg);
        }
    }
}
