using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDKConfiguration
{
    public class SDKConfiguration : IPlugins
    {
        public override string GetMenuName()
        {
            return "SDKConfiguration";
        }
        public override List<string> GetMenuList()
        {
            List<string> menuList = new List<string>();
            menuList.Add("SDKConfiguration");
            return menuList;
        }
        public override PluginsControl GetUserControl(string controlName)
        {
            if (controlName == "SDKConfiguration")
            {
                return new FormSDKConfiguration();
            }
            return null;
        }

    }
}
