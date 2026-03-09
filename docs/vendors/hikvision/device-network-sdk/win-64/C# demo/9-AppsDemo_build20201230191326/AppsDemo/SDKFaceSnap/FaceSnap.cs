using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDKFaceSnap
{
    public class SDKFaceSnap:IPlugins
    {
            public override string GetMenuName()
            {
                return "SDKFaceSnap";
            }
            public override List<string> GetMenuList()
            {
                List<string> menuList = new List<string>();
                menuList.Add("SDKFaceSnap");
                return menuList;
            }
            public override PluginsControl GetUserControl(string controlName)
            {
                if (controlName == "SDKFaceSnap")
                {
                    return new FormFaceSnap();
                }
                return null;
            }
        }
}
