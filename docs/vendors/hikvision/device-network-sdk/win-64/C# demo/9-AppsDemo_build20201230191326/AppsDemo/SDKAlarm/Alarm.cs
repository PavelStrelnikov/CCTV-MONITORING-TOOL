/*******************************************************
Copyright All Rights Reserved. (C) HangZhou Hikvision System Technology Co., Ltd. 
文  件：    CommandDef.h 
开发单位：    杭州海康威视
编  写：    qianshuo@hikvision.com
日  期：    2018-03-15
描  述：    SDKAlarm.cs
修  改：
********************************************************/

using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDKAlarm
{
    public class SDKAlarm : IPlugins
    {
        public SDKAlarm()
        {

        }
        public override string GetMenuName()
        {
            return "SDKAlarm";
        }
        public override List<string> GetMenuList()
        {
            List<string> menuList = new List<string>();
            menuList.Add("SDKAlarm");
            menuList.Add("SDKIOAlarm");
            return menuList;
        }
        public override PluginsControl GetUserControl(string controlName)
        {
             if (controlName == "SDKAlarm")
             {
                 return new FormSDKAlarm();
             }
             else if (controlName == "SDKIOAlarm")
             {
                 return new FormSDKIOAlarm();
             }
            return null;
        }
    }
}
