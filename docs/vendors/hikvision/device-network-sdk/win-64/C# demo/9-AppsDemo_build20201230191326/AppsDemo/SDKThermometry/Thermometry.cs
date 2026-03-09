using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDKThermometry
{
    public class SDKThermometry : IPlugins
    {
        public override string GetMenuName()
        {
            return "SDKThermometry";
        }
        public override List<string> GetMenuList()
        {
            List<string> menuList = new List<string>();
            menuList.Add("SDKThermometry");
            return menuList;
        }
        public override PluginsControl GetUserControl(string controlName)
        {
            if (controlName == "SDKThermometry")
            {
                return new FormSDKThermometry();
            }
            return null;
        }

    }
}
