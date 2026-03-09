using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDKANPR
{
    public class SDKANPR : IPlugins
    {
        public override string GetMenuName()
        {
            //throw new NotImplementedException();
            return "SDKANPR";
        }

        public override List<string> GetMenuList()
        {
            //throw new NotImplementedException();
            List<string> menuList = new List<string>();
            menuList.Add("SDKANPR");
            return menuList;
        }

        public override PluginsControl GetUserControl(string controlName)
        {
            //throw new NotImplementedException();
            if (controlName == "SDKANPR")
            {
                return new ANPRForm();
            }
            return null;
        }
    }

}
