using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDKFaceLib
{
    public class SDKFaceLib : IPlugins
    {
        public override string GetMenuName()
        {
            return "SDKFaceLib";
        }
        public override List<string> GetMenuList()
        {
            List<string> menuList = new List<string>();
            menuList.Add("SDKFace1VN");
            menuList.Add("SDKFaceLib");
            menuList.Add("SDKFaceLibSearchByPic");
            return menuList;
        }

        public override PluginsControl GetUserControl(string controlName)
        {
            if (controlName == "SDKFace1VN")
             {
                 return new Face1vN();
             }
            else if (controlName == "SDKFaceLib")
            {
                return new BlockFDForm();
            }
            else if (controlName == "SDKFaceLibSearchByPic")
            {
                return new FaceLibSearchByPicForm();
            }
            return null;
        }
    }
}
