using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDKThermometry
{
    public partial class FormManualThermometry : Form
    {
        public int m_iChannel = -1;
        public int m_lUserID = -1;
        private int m_iRemoteHandle = -1;
        private bool m_bGetManualTherm = false;
        private int m_iConlumn = -1;
        private int m_lRealHandle = -1;
        private ConcurrentQueue<Point> m_points = new ConcurrentQueue<Point>();
        private CHCNetSDK.NET_DVR_PREVIEWINFO m_struPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
        private CHCNetSDK.NET_DVR_STD_CONFIG m_struSTDConfig = new CHCNetSDK.NET_DVR_STD_CONFIG();
        private CHCNetSDK.NET_SDK_MANUALTHERM_BASICPARAM m_struManualThermBasicParam = new CHCNetSDK.NET_SDK_MANUALTHERM_BASICPARAM();
        private CHCNetSDK.NET_DVR_REALTIME_THERMOMETRY_COND m_struRealTimeThermCond = new CHCNetSDK.NET_DVR_REALTIME_THERMOMETRY_COND();
        private CHCNetSDK.NET_SDK_MANUAL_THERMOMETRY m_struManualTherm = new CHCNetSDK.NET_SDK_MANUAL_THERMOMETRY();
        private CHCNetSDK.REMOTECONFIGCALLBACK m_fManualThermData;
        public delegate void UpdateListBoxCallback(IntPtr lpBuffer);
        private int m_iPointNum = 0;
        private CHCNetSDK.DRAWFUN fDrawFun = null;
        private CHCNetSDK.NET_VCA_POINT m_struPoint = new CHCNetSDK.NET_VCA_POINT();
        private CHCNetSDK.NET_VCA_POLYGON m_struPolygon = new CHCNetSDK.NET_VCA_POLYGON();

        public FormManualThermometry()
        {
            InitializeComponent();

            m_struPolygon.struPos = new CHCNetSDK.NET_VCA_POINT[CHCNetSDK.VCA_MAX_POLYGON_POINT_NUM];
            if (m_fManualThermData == null)
            {
                m_fManualThermData = new CHCNetSDK.REMOTECONFIGCALLBACK(GetRealtimeThermometryCallback);
            }

            comboBoxManualRuleCalibType.SelectedIndex = 0;
            comboBoxRemoteMode.SelectedIndex = 0;

        }

        private void btnGetManuakThermBasic_Click(object sender, EventArgs e)
        {
            int iCondSize = sizeof(int);
            m_struSTDConfig.dwCondSize = (uint)iCondSize;
            m_struSTDConfig.lpCondBuffer = Marshal.AllocHGlobal(iCondSize);
            Marshal.WriteInt32(m_struSTDConfig.lpCondBuffer, m_iChannel);

            int iOutSize = Marshal.SizeOf(m_struManualThermBasicParam);
            m_struManualThermBasicParam.dwSize = (uint)iOutSize;
            IntPtr ptrOutBuffer = Marshal.AllocHGlobal(iOutSize);
            Marshal.StructureToPtr(m_struManualThermBasicParam, ptrOutBuffer, false);
            m_struSTDConfig.dwOutSize = (uint)iOutSize;
            m_struSTDConfig.lpOutBuffer = ptrOutBuffer;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, false);

            if (!CHCNetSDK.NET_DVR_GetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_MANUALTHERM_BASICPARAM, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：获取手动测温基本参数，错误码：" + iLastErr ;
                MessageBox.Show(strErr);
            }
            else
            {
                m_struSTDConfig = (CHCNetSDK.NET_DVR_STD_CONFIG)Marshal.PtrToStructure(ptr, typeof(CHCNetSDK.NET_DVR_STD_CONFIG));
                m_struManualThermBasicParam = (CHCNetSDK.NET_SDK_MANUALTHERM_BASICPARAM)Marshal.PtrToStructure(m_struSTDConfig.lpOutBuffer, typeof(CHCNetSDK.NET_SDK_MANUALTHERM_BASICPARAM));
                textBoxManualDistance.Text = m_struManualThermBasicParam.wDistance.ToString();
                textBoxManualEmissivity.Text = m_struManualThermBasicParam.fEmissivity.ToString();
            }

            Marshal.FreeHGlobal(ptrOutBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        private void btnSetManuakThermBasic_Click(object sender, EventArgs e)
        {
            m_struSTDConfig.dwCondSize = sizeof(int);
            m_struSTDConfig.lpCondBuffer = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(m_struSTDConfig.lpCondBuffer, m_iChannel);

            int iInSize = Marshal.SizeOf(m_struManualThermBasicParam);
            m_struManualThermBasicParam.dwSize = (uint)iInSize;
            m_struManualThermBasicParam.wDistance = Convert.ToUInt16(textBoxManualDistance.Text);
            m_struManualThermBasicParam.fEmissivity = Convert.ToSingle(textBoxManualEmissivity.Text);
            IntPtr ptrInBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(m_struManualThermBasicParam));
            Marshal.StructureToPtr(m_struManualThermBasicParam, ptrInBuffer, false);
            m_struSTDConfig.dwInSize = (uint)iInSize;
            m_struSTDConfig.lpInBuffer = ptrInBuffer;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, true);

            if (!CHCNetSDK.NET_DVR_SetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_MANUALTHERM_BASICPARAM, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：设置手动测温基本参数，错误码：" + iLastErr ;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("设置手动测温基本参数，成功！");
            }

            Marshal.FreeHGlobal(ptrInBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        private void btnGetManualThermometry_Click(object sender, EventArgs e)
        {
            if (m_bGetManualTherm)
            {
                if (!CHCNetSDK.NET_DVR_StopRemoteConfig(m_iRemoteHandle))
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "Thermal：停止获取手动测温数据置失败，错误码：" + iLastErr ;
                    MessageBox.Show(strErr);
                }
                else
                {
                    btnGetManualThermometry.Text = "获取";
                    m_bGetManualTherm = false;
                }
            }
            else
            {
            int iInSize = Marshal.SizeOf(m_struRealTimeThermCond);
            m_struRealTimeThermCond.dwSize = (uint)iInSize;
            m_struRealTimeThermCond.dwChan = (uint)m_iChannel;
            m_struRealTimeThermCond.byRuleID = byte.Parse(textBoxManualRuleID.Text);
            IntPtr lpInBuffer = Marshal.AllocHGlobal(iInSize);
            Marshal.StructureToPtr(m_struRealTimeThermCond, lpInBuffer, false);

            m_iRemoteHandle = CHCNetSDK.NET_DVR_StartRemoteConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_MANUALTHERM_INFO, lpInBuffer, iInSize, m_fManualThermData, IntPtr.Zero);
            if (m_iRemoteHandle < 0)
                {
                    int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                    string strErr = "Thermal：获取手动测温数据置失败，错误码：" + iLastErr ;
                    MessageBox.Show(strErr);
                }
                else
                {
                    btnGetManualThermometry.Text = "停止获取";
                    m_bGetManualTherm = true;
                }
            }
        }

        public void GetRealtimeThermometryCallback(uint dwType, IntPtr lpBuffer, uint dwBufLen, IntPtr pUserData)
        {
            if (dwType == (uint)CHCNetSDK.NET_SDK_CALLBACK_TYPE.NET_SDK_CALLBACK_TYPE_DATA)
            {
                //创建该控件的主线程直接更新信息列表 
                UpdateClientList(lpBuffer);
            }
        }

        public void UpdateClientList(IntPtr lpBuffer)
        {
            m_struManualTherm = (CHCNetSDK.NET_SDK_MANUAL_THERMOMETRY)Marshal.PtrToStructure(lpBuffer, typeof(CHCNetSDK.NET_SDK_MANUAL_THERMOMETRY));
            uint dwRelativeTime = m_struManualTherm.dwRelativeTime;
            uint dwAbsTime = m_struManualTherm.dwAbsTime;
            byte[] szRuleName = m_struManualTherm.struRuleInfo.szRuleName;
            string strRuleName = System.Text.Encoding.Default.GetString(szRuleName);
            uint byRuleID = m_struManualTherm.struRuleInfo.byRuleID;

            string strEnable = "";
            if (0 == m_struManualTherm.struRuleInfo.byEnable)
            {
                strEnable = "不启用";
            }
            if (1 == m_struManualTherm.struRuleInfo.byEnable)
            {
                strEnable = "启用";
            }

            byte byRuleCalibType = m_struManualTherm.struRuleInfo.byRuleCalibType;

            Single fTemperature = 0.0F;
            string strPoint = "";
            if (0 == byRuleCalibType)
            {
                fTemperature = m_struManualTherm.struRuleInfo.struPointTherm.fPointTemperature;
                strPoint = "" + byRuleCalibType + "X:" + m_struManualTherm.struRuleInfo.struPointTherm.struPoint.fX + "Y:" + m_struManualTherm.struRuleInfo.struPointTherm.struPoint.fY;
            }
            string strLinePolygon = "";
            Single fMaxTemperature = 0.0F;
            Single fMinTemperature = 0.0F;
            Single fAverageTemperature = 0.0F;
            Single fTemperatureDiff = 0.0F;
            if (1 == byRuleCalibType || 2 == byRuleCalibType)
            {
                fMaxTemperature = m_struManualTherm.struRuleInfo.struRegionTherm.fMaxTemperature;
                fMinTemperature = m_struManualTherm.struRuleInfo.struRegionTherm.fMinTemperature;
                fAverageTemperature = m_struManualTherm.struRuleInfo.struRegionTherm.fAverageTemperature;
                fTemperatureDiff = m_struManualTherm.struRuleInfo.struRegionTherm.fTemperatureDiff;

                int iPointNum = (int)m_struManualTherm.struRuleInfo.struRegionTherm.struRegion.dwPointNum;
                for (int i = 0; i < iPointNum; i++)
                {
                    float fX = m_struManualTherm.struRuleInfo.struRegionTherm.struRegion.struPos[i].fX;
                    float fY = m_struManualTherm.struRuleInfo.struRegionTherm.struRegion.struPos[i].fY;
                    strLinePolygon = strLinePolygon + "X" + i + 1 + ":" + fX + "Y" + i + 1 + ":" + fY;
                }
                strLinePolygon = "" + byRuleCalibType + strLinePolygon;
            }

            string strThermometryUnit = null;
            if (0 == m_struManualTherm.byThermometryUnit)
            {
                strThermometryUnit = "摄氏度";
            }
            if (1 == m_struManualTherm.byThermometryUnit)
            {
                strThermometryUnit = "华氏度";
            }
            if (2 == m_struManualTherm.byThermometryUnit)
            {
                strThermometryUnit = "开尔文";
            }

            string strDataType = null;
            if (0 == m_struManualTherm.byDataType)
            {
                strDataType = "检测中";
            }
            if (1 == m_struManualTherm.byDataType)
            {
                strDataType = "开始";
            }
            if (2 == m_struManualTherm.byDataType)
            {
                strDataType = "结束";
            }

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new UpdateListBoxCallback(UpdateClientList), lpBuffer);
            }
            else
            {
                if (listViewManualTherm.Items.Count > 200)
                {
                    listViewManualTherm.Items.RemoveAt(0);
                }

                m_iConlumn = m_iConlumn = listViewManualTherm.Items.Count - 1;
                
                listViewManualTherm.Items.Add(new ListViewItem(new string[] { ("" + (++m_iConlumn)), ("" + dwRelativeTime), ("" + dwAbsTime),(strRuleName),("" + byRuleID),
                                (strThermometryUnit), (strDataType),("" + byRuleCalibType),(strEnable),(strPoint),("" + fTemperature),(strLinePolygon),("" + fMaxTemperature),
                                ("" + fMinTemperature),("" + fAverageTemperature),("" + fTemperatureDiff)}));
            }
        }

        private void btnSetManualThermometry_Click(object sender, EventArgs e)
        {
            m_struSTDConfig.dwCondSize = sizeof(int);
            m_struSTDConfig.lpCondBuffer = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(m_struSTDConfig.lpCondBuffer, m_iChannel);

            int iInSize = Marshal.SizeOf(m_struManualTherm);
            m_struManualTherm.dwSize = (uint)iInSize;
            m_struManualTherm.dwChannel = (uint)m_iChannel;
            m_struManualTherm.struRuleInfo.byRuleID = byte.Parse(textBoxManualRuleID.Text);
            m_struManualTherm.struRuleInfo.byEnable = Convert.ToByte(checkBoxManualEnable.Checked);
            byte[] byName = System.Text.Encoding.Default.GetBytes(textBoxManualRuleName.Text);
            m_struManualTherm.struRuleInfo.szRuleName = new byte[32];
            byName.CopyTo(m_struManualTherm.struRuleInfo.szRuleName, 0);
            m_struManualTherm.struRuleInfo.struPointTherm.struPoint = m_struPoint;
            m_struManualTherm.struRuleInfo.struRegionTherm.struRegion = m_struPolygon;
            IntPtr ptrInBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(m_struManualTherm));
            Marshal.StructureToPtr(m_struManualTherm, ptrInBuffer, false);
            m_struSTDConfig.dwInSize = (uint)iInSize;
            m_struSTDConfig.lpInBuffer = ptrInBuffer;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(m_struSTDConfig));
            Marshal.StructureToPtr(m_struSTDConfig, ptr, true);

            if (!CHCNetSDK.NET_DVR_SetSTDConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_MANUALTHERM, ptr))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：设置手动测温数据，错误码：" + iLastErr ;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("设置手动测温数据，成功！");
            }

            Marshal.FreeHGlobal(ptrInBuffer);
            Marshal.FreeHGlobal(ptr);
        }

        private void btnDelRule_Click(object sender, EventArgs e)
        {
            int iCondSize = Marshal.SizeOf(m_struRealTimeThermCond);
            m_struRealTimeThermCond.dwSize = (uint)iCondSize;
            m_struRealTimeThermCond.dwChan = (uint)m_iChannel;
            m_struRealTimeThermCond.byRuleID = byte.Parse(textBoxManualRuleID.Text);
            m_struRealTimeThermCond.byMode = (byte)comboBoxRemoteMode.SelectedIndex;
            IntPtr lpCondBuffer = Marshal.AllocHGlobal(iCondSize);
            Marshal.StructureToPtr(m_struRealTimeThermCond, lpCondBuffer, false);

            CHCNetSDK.NET_DVR_STD_CONTROL m_struSTDControl = new CHCNetSDK.NET_DVR_STD_CONTROL();
            m_struSTDControl.dwCondSize = (uint)iCondSize;
            m_struSTDControl.lpCondBuffer = lpCondBuffer;

            m_struSTDControl.lpStatusBuffer = Marshal.AllocHGlobal(CHCNetSDK.ISAPI_STATUS_LEN);
            m_struSTDControl.dwStatusSize = CHCNetSDK.ISAPI_STATUS_LEN;

            if (!CHCNetSDK.NET_DVR_STDControl(m_lUserID, CHCNetSDK.NET_DVR_DEL_MANUALTHERM_RULE, ref m_struSTDControl))
            {
                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                string strErr = "Thermal：删除手动测温规则，错误码：" + iLastErr ;
                MessageBox.Show(strErr);
            }
            else
            {
                MessageBox.Show("删除手动测温规则，成功！");
            }

            Marshal.FreeHGlobal(lpCondBuffer);
        }

        private void btnDelconlumn_Click(object sender, EventArgs e)
        {
            int Index = 0;
            if (this.listViewManualTherm.SelectedItems.Count > 0)//判断listview有被选中项
            {
                Index = this.listViewManualTherm.SelectedItems[0].Index;//取当前选中项的index,SelectedItems[0]这必须为0
                listViewManualTherm.Items[Index].Remove();
            }
        }

        private void btnAddConlum_Click(object sender, EventArgs e)
        {
            m_iConlumn = listViewManualTherm.Items.Count - 1;
            listViewManualTherm.Items.Add(new ListViewItem("" + (++m_iConlumn)));
        }



        // 回调函数叠加图像字符
        private void cbDrawFun(int port, IntPtr hDc, uint nUser)
        {
            Graphics g = Graphics.FromHdc(hDc);
            Pen pen = new Pen(Color.Red, 2);
            Brush brush = new SolidBrush(Color.FromArgb(50, 255, 255, 255));
            if (g == null)
            {
                return;
            }
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Point[] pPolygon = m_points.ToArray();
            int pointsCount = pPolygon.Length;
            if (pointsCount > 0)
            {
                if (1 == pointsCount)
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(255, 0, 0)), pPolygon[0].X, pPolygon[0].Y, 5, 5);
                }

                for (int index = 0; index < pointsCount - 1; ++index)
                {
                    g.DrawLine(new Pen(Color.Red, 2), new Point(pPolygon[index].X, pPolygon[index].Y),
                        new Point(pPolygon[index + 1].X, pPolygon[index + 1].Y));
                }
                g.DrawLine(pen, new Point(pPolygon[pointsCount - 1].X, pPolygon[pointsCount - 1].Y),
                        new Point(pPolygon[0].X, pPolygon[0].Y));

                g.FillPolygon(brush, pPolygon);
            }
        }

        private void FormManualThermometry_Load(object sender, EventArgs e)
        {
            
            
        }

        private void comboBoxManualRuleCalibType_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_points = new ConcurrentQueue<Point>();
        }

        private void pictureBoxPlay_MouseDown(object sender, MouseEventArgs e)
        {
            m_struPreviewInfo.lChannel = m_iChannel;
            m_struPreviewInfo.dwStreamType = 0;
            m_struPreviewInfo.bBlocked = true;
            m_struPreviewInfo.DisplayBufNum = 0;
            m_struPreviewInfo.hPlayWnd = pictureBoxPlay.Handle;

            m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref m_struPreviewInfo, null, IntPtr.Zero);

            if (e.Button == MouseButtons.Left)
            {
                Point point = pictureBoxPlay.PointToClient(Control.MousePosition);

                if (0 == comboBoxManualRuleCalibType.SelectedIndex)
                {
                    if (m_points.Count >= 1)
                    {
                        m_points = new ConcurrentQueue<Point>();
                    }

                    m_struPoint.fX = (float)point.X / pictureBoxPlay.Width;
                    m_struPoint.fY = (float)point.Y / pictureBoxPlay.Height;

                    m_points.Enqueue(point);

                    if (m_points.Count == 1 && fDrawFun == null)
                    {
                        fDrawFun = new CHCNetSDK.DRAWFUN(cbDrawFun);
                        if (!CHCNetSDK.NET_DVR_RigisterDrawFun(m_lRealHandle, fDrawFun, 0))
                        {
                            int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                            string strErr = "Thermal：预览画面叠加配置，错误码：" + iLastErr;
                            MessageBox.Show(strErr);
                        }
                    }
                }

                if (2 == comboBoxManualRuleCalibType.SelectedIndex)  // 画线
                {
                    if (m_iPointNum >= 2)
                    {
                        m_iPointNum = 0;
                        m_points = new ConcurrentQueue<Point>();
                    }

                    if (m_points.Count >= 0 && m_points.Count < 2)
                    {
                        m_struPolygon.struPos[m_iPointNum].fX = (float)point.X / pictureBoxPlay.Width;
                        m_struPolygon.struPos[m_iPointNum].fY = (float)point.Y / pictureBoxPlay.Height;
                        ++m_iPointNum;
                        m_struPolygon.dwPointNum = (uint)m_iPointNum;
                        m_points.Enqueue(point);

                        if (m_points.Count == 2 && fDrawFun == null)
                        {
                            fDrawFun = new CHCNetSDK.DRAWFUN(cbDrawFun);
                            if (!CHCNetSDK.NET_DVR_RigisterDrawFun(m_lRealHandle, fDrawFun, 0))
                            {
                                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                                string strErr = "Thermal：预览画面叠加配置，错误码：" + iLastErr;
                                MessageBox.Show(strErr);
                            }
                        }
                    }

                }

                if (1 == comboBoxManualRuleCalibType.SelectedIndex)  // 画框
                {
                    if (m_iPointNum >= CHCNetSDK.VCA_MAX_POLYGON_POINT_NUM)
                    {
                        m_struPolygon.struPos = new CHCNetSDK.NET_VCA_POINT[CHCNetSDK.VCA_MAX_POLYGON_POINT_NUM];
                        m_iPointNum = 0;
                        m_points = new ConcurrentQueue<Point>();
                    }

                    if (m_points.Count < CHCNetSDK.VCA_MAX_POLYGON_POINT_NUM)
                    {
                        m_struPolygon.struPos[m_iPointNum].fX = (float)point.X / pictureBoxPlay.Width;
                        m_struPolygon.struPos[m_iPointNum].fY = (float)point.Y / pictureBoxPlay.Height;
                        ++m_iPointNum;
                        m_struPolygon.dwPointNum = (uint)m_iPointNum;
                        m_points.Enqueue(point);

                        if (fDrawFun == null)
                        {
                            fDrawFun = new CHCNetSDK.DRAWFUN(cbDrawFun);
                            if (!CHCNetSDK.NET_DVR_RigisterDrawFun(m_lRealHandle, fDrawFun, 0))
                            {
                                int iLastErr = Convert.ToInt32(CHCNetSDK.NET_DVR_GetLastError());
                                string strErr = "Thermal：预览画面叠加配置，错误码：" + iLastErr;
                                MessageBox.Show(strErr);
                            }
                        }
                    }
                }
            }
        }


    }
}
