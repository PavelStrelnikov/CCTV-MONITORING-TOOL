using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common
{
    public class PluginsFactory
    {
        public class PluginMenu
        {
            public string MenuName { get; set; }
            public List<string> SubMenuList { get; set; }
        }

        private class CPluginsInfo
        {
            public IPlugins PluginInstance { get; set; }
            public string MenuName { get; set; }
            public List<string> MenuList { get; set; }
        }

        private static System.IO.FileSystemWatcher m_fileWatcher = new System.IO.FileSystemWatcher();
        private static bool m_bFileWatching = false;

        public delegate void OnPluginsChangedHandler();
        public static event OnPluginsChangedHandler OnPluginsChanged;
        private static IDeviceTree m_sDeviceTreeInstance = null;
        //key为plugins名称
        private static Dictionary<string, CPluginsInfo> m_dicPluginsInfo = new Dictionary<string, CPluginsInfo>();

        public static IDeviceTree GetDeviceTreeInstance()
        {
            return m_sDeviceTreeInstance;
        }

        /// <summary>
        /// 本接口要由主线程运行，而且在主窗体启动之后
        /// </summary>
        /// <returns></returns>
        public static bool LoadPlugins()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string strPluginsPath = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, @"..\Plugins");
            try
            {
                m_dicPluginsInfo.Clear();
                m_sDeviceTreeInstance = null;

                System.IO.DirectoryInfo dirPlugins = new System.IO.DirectoryInfo(strPluginsPath);
                System.IO.FileInfo[] filePlugins = dirPlugins.GetFiles("*.dll");
                List<IDeviceTree> deviceTreeList = new List<IDeviceTree>();
                foreach (System.IO.FileInfo fileInfo in filePlugins)
                {
                    string strPluginsName = fileInfo.Name;
                    string strPluginsNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(strPluginsName);

                    Assembly assembly = Assembly.LoadFile(fileInfo.FullName);
                    Type type = assembly.GetType(strPluginsNameWithoutExtension + "." + strPluginsNameWithoutExtension);
                    if (type == null)
                    {
                        continue;
                    }
                    object oPlugin = Activator.CreateInstance(type);
                    if (oPlugin is IDeviceTree)
                    {
                        deviceTreeList.Add(oPlugin as IDeviceTree);
                    }
                    else if (oPlugin is IPlugins)
                    {
                        CPluginsInfo pluginsInfo = new CPluginsInfo();
                        pluginsInfo.PluginInstance = oPlugin as IPlugins;
                        pluginsInfo.MenuName = pluginsInfo.PluginInstance.GetMenuName();
                        pluginsInfo.MenuList = pluginsInfo.PluginInstance.GetMenuList();
                        //文件肯定无同名
                        m_dicPluginsInfo.Add(strPluginsName, pluginsInfo);
                    }
                }
                if (deviceTreeList.Count == 1)
                {
                    m_sDeviceTreeInstance = deviceTreeList[0];
                }
                else if (deviceTreeList.Count > 1)
                {
                    FormDeviceTreeSelection selection = new FormDeviceTreeSelection();
                    selection.DeviceTreeList = new List<string>();
                    foreach (var deviceTree in deviceTreeList)
                    {
                        selection.DeviceTreeList.Add(deviceTree.GetDeviceTreeName());
                    }
                    if (selection.ShowDialog() == DialogResult.OK)
                    {
                        m_sDeviceTreeInstance = deviceTreeList[selection.SelectedDeviceTreeIndex];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LoadPlugins");
            }
            finally
            {
                if (!m_bFileWatching)
                {
                    m_bFileWatching = true;
                    m_fileWatcher.Path = strPluginsPath;
                    m_fileWatcher.Filter = "*.dll";
                    m_fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
                    m_fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
                    m_fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
                    m_fileWatcher.Renamed += new RenamedEventHandler(OnFileRenamed);
                    m_fileWatcher.EnableRaisingEvents = true;
                    m_fileWatcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess
                                  | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;
                    m_fileWatcher.IncludeSubdirectories = false;

                }

            }
            return true;
        }

        static void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            LoadPlugins();
            OnPluginsChanged();
        }
        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            LoadPlugins();
            OnPluginsChanged();
        }

        public static List<PluginMenu> GetPluginsMenus()
        {
            List<PluginMenu> pluginMenuList = new List<PluginMenu>();
            foreach (KeyValuePair<string, CPluginsInfo> pluginsItem in m_dicPluginsInfo)
            {
                PluginMenu pluginMenu = new PluginMenu();
                pluginMenu.MenuName = pluginsItem.Value.MenuName;
                pluginMenu.SubMenuList = pluginsItem.Value.MenuList;
                pluginMenuList.Add(pluginMenu);
            }
            return pluginMenuList;
        }

        /// <summary>
        /// 根据菜单名称获取控件
        /// </summary>
        /// <param name="mainMenuName"></param>
        /// <param name="subMenuName"></param>
        /// <returns></returns>
        public static PluginsControl GetPluginsUserControl(string subMenuName)
        {
            foreach (KeyValuePair<string, CPluginsInfo> pluginsItem in m_dicPluginsInfo)
            {
                foreach (string subMenu in pluginsItem.Value.MenuList)
                {
                    if (subMenu == subMenuName)
                    {
                        return pluginsItem.Value.PluginInstance.GetUserControl(subMenuName);
                    }
                }
            }
            return null;
        }

    }
}
