using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDKPerimeterPrecaution
{
    public class SDKPerimeterPrecaution : IPlugins
    {
        public override string GetMenuName()
        {
            return "SDKPerimeterPrecaution";
        }

        public override List<string> GetMenuList()
        {
            List<string> menuList = new List<string>();
            menuList.Add("SDKPerimeterPrecaution");
            return menuList;
        }

        public override PluginsControl GetUserControl(string controlName)
        {
            if (controlName == "SDKPerimeterPrecaution")
            {
                return new FormPerimeterPrecaution();
            }
            return null;
        }
    }
}
