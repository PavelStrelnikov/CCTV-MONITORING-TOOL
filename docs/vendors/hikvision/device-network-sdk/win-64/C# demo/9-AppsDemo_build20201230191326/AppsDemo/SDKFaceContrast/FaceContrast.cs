using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDKFaceContrast
{
    public class SDKFaceContrast :IPlugins
    {
        public override string GetMenuName()
        {
            return "SDKFaceContrast";
        }
        public override List<string> GetMenuList()
        {
            List<string> menuList = new List<string>();
            menuList.Add("SDKFaceContrast");
            return menuList;
        }
        public override PluginsControl GetUserControl(string controlName)
        {
            if (controlName == "SDKFaceContrast")
            {
                return new FormFaceContrast();
            }
            return null;
        }
    }
}
