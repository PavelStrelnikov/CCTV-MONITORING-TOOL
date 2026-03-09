using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeifenLuo.WinFormsUI.Docking;

namespace Common
{
    public class PluginsControl : DockContent
    {

    }

    public abstract class IPlugins
    {
        public IPlugins() { }
        public abstract string GetMenuName();

        public abstract List<string> GetMenuList();
        public abstract PluginsControl GetUserControl(string controlName);
    }
}
