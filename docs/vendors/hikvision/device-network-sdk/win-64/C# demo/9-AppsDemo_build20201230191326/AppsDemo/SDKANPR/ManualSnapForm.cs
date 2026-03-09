using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDKANPR
{
    public partial class ManualSnapForm : Form
    {
        //设备树对象映射
        IDeviceTree m_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        Random rd = new Random();
        public int m_index = 0;
       

               

        public ManualSnapForm()
        {
            InitializeComponent();

            CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
            lpPreviewInfo.lChannel = m_deviceTree.GetSelectedDeviceInfo().iDeviceChanNum;
            lpPreviewInfo.hPlayWnd = m_panel.Handle;
            int iUserID = (int)m_deviceTree.GetSelectedDeviceInfo().lLoginID;
            int m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(iUserID, ref lpPreviewInfo, null, IntPtr.Zero);           
        }



        private void SnapBtn_Click(object sender, EventArgs e)
        {

            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();

            CHCNetSDK.NET_DVR_MANUALSNAP struManualSnap = new CHCNetSDK.NET_DVR_MANUALSNAP();
            CHCNetSDK.NET_DVR_PLATE_RESULT struPlateResult = new CHCNetSDK.NET_DVR_PLATE_RESULT();

            if(OSDEnableCheckBox.Checked)
            {
                struManualSnap.byOSDEnable = 1;
            }
            else
            {
                struManualSnap.byOSDEnable = 0;
            }

            struManualSnap.byLaneNo = byte.Parse(m_textBoxLaneNo.Text);

            struPlateResult.pBuffer1 = Marshal.AllocHGlobal(2 * 1024 * 1024);
            struPlateResult.pBuffer2 = Marshal.AllocHGlobal(2 * 1024 * 1024);

            if(!CHCNetSDK.NET_DVR_ManualSnap((int)deviceInfo.lLoginID, ref struManualSnap, ref struPlateResult))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "NET_DVR_ManualSnap fail, Err：" + iLastErr;
                MessageBox.Show(strErr);
            }
            else
            {
                ListViewItem lvi = new ListViewItem();

                m_index++;
                lvi.Text = m_index.ToString();

                lvi.SubItems.Add(System.Text.Encoding.Default.GetString(struPlateResult.byAbsTime));
                // plate color
                if (0 == struPlateResult.struPlateInfo.byColor)
                {
                    lvi.SubItems.Add("blue");
                }
                else if (1 == struPlateResult.struPlateInfo.byColor)
                {
                    lvi.SubItems.Add("yellow");
                }
                else if (2 == struPlateResult.struPlateInfo.byColor)
                {
                    lvi.SubItems.Add("white");
                }
                else if (3 == struPlateResult.struPlateInfo.byColor)
                {
                    lvi.SubItems.Add("black");
                }
                else if (4 == struPlateResult.struPlateInfo.byColor)
                {
                    lvi.SubItems.Add("green");
                }
                else if (5 == struPlateResult.struPlateInfo.byColor)
                {
                    lvi.SubItems.Add("bkair");
                }
                else
                {
                    lvi.SubItems.Add("Other");
                }

                //plate type
                if (0 == struPlateResult.struPlateInfo.byPlateType)
                {
                    lvi.SubItems.Add("civil and nonallow list");
                }
                else if(1 == struPlateResult.struPlateInfo.byPlateType)
                {
                    lvi.SubItems.Add("02 type of civil license");
                }
                else if(2 == struPlateResult.struPlateInfo.byPlateType)
                {
                    lvi.SubItems.Add("Police Car");
                }
                else if(3 == struPlateResult.struPlateInfo.byPlateType)
                {
                    lvi.SubItems.Add("Police Wagon");
                }
                else if(4 == struPlateResult.struPlateInfo.byPlateType)
                {
                    lvi.SubItems.Add("Double license");
                }
                else if(5 == struPlateResult.struPlateInfo.byPlateType)
                {
                    lvi.SubItems.Add("Embassy license");
                }
                else if(6 == struPlateResult.struPlateInfo.byPlateType)
                {
                    lvi.SubItems.Add("Agricultural license");
                }
                else if(7 == struPlateResult.struPlateInfo.byPlateType)
                {
                    lvi.SubItems.Add("Motorcycle license");
                }
                else if(8 == struPlateResult.struPlateInfo.byPlateType)
                {
                    lvi.SubItems.Add("new energy license");
                }
                else
                {
                    lvi.SubItems.Add("unknow");
                }

                //sLicense
                lvi.SubItems.Add(System.Text.Encoding.Default.GetString(struPlateResult.struPlateInfo.sLicense));
                
                // vehicle type
                if (0 == struPlateResult.struVehicleInfo.byVehicleType)
                {
                    lvi.SubItems.Add("Other Vehicles");
                }
                else if (1 == struPlateResult.struVehicleInfo.byVehicleType)
                {
                    lvi.SubItems.Add("Small Cars");
                }
                else if (2 == struPlateResult.struVehicleInfo.byVehicleType)
                {
                    lvi.SubItems.Add("Oversize Vehicle");
                }
                else if (3 == struPlateResult.struVehicleInfo.byVehicleType)
                {
                    lvi.SubItems.Add("Passer");
                }
                else if (4 == struPlateResult.struVehicleInfo.byVehicleType)
                {
                    lvi.SubItems.Add("Two-wheeler");
                }
                else if (5 == struPlateResult.struVehicleInfo.byVehicleType)
                {
                    lvi.SubItems.Add("Three-wheeler");
                }

                //vehicle color
                if (0 == struPlateResult.struVehicleInfo.byColor)
                {
                    lvi.SubItems.Add("Other");
                }
                else if (1 == struPlateResult.struVehicleInfo.byColor)
                {
                    lvi.SubItems.Add("White");
                }
                else if (2 == struPlateResult.struVehicleInfo.byColor)
                {
                    lvi.SubItems.Add("Silver");
                }
                else if (3 == struPlateResult.struVehicleInfo.byColor)
                {
                    lvi.SubItems.Add("Gray");
                }
                else if (4 == struPlateResult.struVehicleInfo.byColor)
                {
                    lvi.SubItems.Add("Black");
                }
                else if (5 == struPlateResult.struVehicleInfo.byColor)
                {
                    lvi.SubItems.Add("Red");
                }
                else if (6 == struPlateResult.struVehicleInfo.byColor)
                {
                    lvi.SubItems.Add("DarkBlue");
                }
                else if (7 == struPlateResult.struVehicleInfo.byColor)
                {
                    lvi.SubItems.Add("Blue");
                }
                else if (8 == struPlateResult.struVehicleInfo.byColor)
                {
                    lvi.SubItems.Add("Yellow");
                }
                else if (9 == struPlateResult.struVehicleInfo.byColor)
                {
                    lvi.SubItems.Add("Green");
                }
                else if (10 == struPlateResult.struVehicleInfo.byColor)
                {
                    lvi.SubItems.Add("Brown");
                }
                else if (11 == struPlateResult.struVehicleInfo.byColor)
                {
                    lvi.SubItems.Add("Pink");
                }
                else if (12 == struPlateResult.struVehicleInfo.byColor)
                {
                    lvi.SubItems.Add("Purple");
                }
                else if (13 == struPlateResult.struVehicleInfo.byColor)
                {
                    lvi.SubItems.Add("DarkGray");
                }
                else if (14 == struPlateResult.struVehicleInfo.byColor)
                {
                    lvi.SubItems.Add("Cyan");
                }

                //vehicle chan
                lvi.SubItems.Add(struPlateResult.byChanIndex.ToString());

                //speed
                lvi.SubItems.Add(struPlateResult.struVehicleInfo.wSpeed.ToString()); 
             
                string strFileSavePath = Application.StartupPath + "\\ANPRManualSnap\\" + DateTime.Now.ToString("yyyy-MM-dd");
                if (!Directory.Exists(strFileSavePath))
                Directory.CreateDirectory(strFileSavePath);

                if (struPlateResult.pBuffer1 != IntPtr.Zero && struPlateResult.dwPicLen != 0)
                {
                    string strCloseUpPicPath = strFileSavePath + "\\CloseUpPic_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + "_" + rd.Next().ToString() + ".jpg";
                    FileStream fs = new FileStream(strCloseUpPicPath, FileMode.Create);
                    int iLen = (int)struPlateResult.dwPicLen;
                    byte[] by = new byte[iLen];
                    Marshal.Copy(struPlateResult.pBuffer1, by, 0, iLen);
                    fs.Write(by, 0, iLen);
                    fs.Close();
                    this.CloseUppictureBox.Image = Image.FromFile(strCloseUpPicPath);

                    //picPath
                    lvi.SubItems.Add(strCloseUpPicPath);
                }

                if (struPlateResult.pBuffer2 != IntPtr.Zero && struPlateResult.dwPicPlateLen != 0)
                {
                    string strPlatePicPath = strFileSavePath + "\\PlatePic_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + "_" + rd.Next().ToString() + ".jpg"; ;
                    FileStream fs = new FileStream(strPlatePicPath, FileMode.Create);
                    int iLen = (int)struPlateResult.dwPicPlateLen;
                    byte[] by = new byte[iLen];
                    Marshal.Copy(struPlateResult.pBuffer2, by, 0, iLen);
                    fs.Write(by, 0, iLen);
                    fs.Close();

                    this.platePictureBox.Image = Image.FromFile(strPlatePicPath); 
                }

                
                SnapPlateInfoListView.Items.Insert(0, lvi);

                Marshal.FreeHGlobal(struPlateResult.pBuffer1);
                Marshal.FreeHGlobal(struPlateResult.pBuffer2);
            }
        }
    }
}
