/*******************************************************
Copyright All Rights Reserved. (C) HangZhou Hikvision System Technology Co., Ltd. 
文  件：    CommandDef.h 
开发单位：    杭州海康威视
编  写：    qianshuo@hikvision.com
日  期：    2018-03-15
描  述：    SystemManagement.cs
修  改：
********************************************************/

using Common;
using Common.Head;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDKSystemManagement
{
    public class SDKSystemManagement : IPlugins
    {
        public SDKSystemManagement()
        {

        }

        public override string GetMenuName()
        {
            return "SDKSystemManagement";
        }
        public override List<string> GetMenuList()
        {
            List<string> menuList = new List<string>();
            menuList.Add("SDKPreview");
            return menuList;
        }
        public override PluginsControl GetUserControl(string controlName)
        {
            if (controlName == "SDKPreview")
            {
                return new FormPreview();
            }
            return null;
        }
    }
}
