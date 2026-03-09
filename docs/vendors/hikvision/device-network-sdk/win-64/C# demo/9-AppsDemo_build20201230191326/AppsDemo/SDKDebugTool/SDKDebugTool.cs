using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDKDebugTool
{
    public class SDKDebugTool : IPlugins
    {
        public override string GetMenuName()
        {
            return "SDKDebugTool";
        }

        public override List<string> GetMenuList()
        {
            List<string> menuList = new List<string>();
            menuList.Add("SDKDebugTool");
            return menuList;
        }

        public override PluginsControl GetUserControl(string controlName)
        {
            if (controlName == "SDKDebugTool")
            {
                return new FormSDKDebugTool();
            }
            return null;
        }
    }
}
