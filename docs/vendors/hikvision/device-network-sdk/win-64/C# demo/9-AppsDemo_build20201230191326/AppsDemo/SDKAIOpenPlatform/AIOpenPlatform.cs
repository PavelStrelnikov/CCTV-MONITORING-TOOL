using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDKAIOpenPlatform
{
    public class SDKAIOpenPlatform : IPlugins
    {
        public SDKAIOpenPlatform()
        {

        }
        public override string GetMenuName()
        {
            return "SDKAIOpenPlatform";
        }

        public override List<string> GetMenuList()
        {
            List<string> menuList = new List<string>();
            menuList.Add("SDKAIOpenPlatform");
            return menuList;
        }

        public override PluginsControl GetUserControl(string controlName)
        {
            if (controlName == "SDKAIOpenPlatform")
            {
                return new FormAIOpenPlatform();
            }
            return null;
        }
    }
}
