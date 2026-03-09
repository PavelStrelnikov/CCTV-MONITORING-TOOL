using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Net;
using System.Runtime.InteropServices;

namespace SDKDebugTool
{
    public partial class FormSDKDebugTool : PluginsControl
    {
        //成员变量
        IDeviceTree m_deviceTree = PluginsFactory.GetDeviceTreeInstance();

        public FormSDKDebugTool()
        {
            InitializeComponent();
        }

        private void FormSDKDebugTool_Load(object sender, EventArgs e)
        {
            this.InboundDatarTBox.AutoWordSelection = false;
            this.OutboundDataTBox.AutoWordSelection = false;
            MethodcBox.SelectedIndex = 0;
            //BindComboBox(); //不注释掉会崩溃
            this.UrltBox.SelectedIndexChanged += new System.EventHandler(this.UrltBox_SelectedIndexChanged);
        }

        public int BindComboBox()
        {
            OleDbDataReader dr = null;
            OleDbConnection connect = new OleDbConnection();
            try
            {
                connect.ConnectionString = @"Provider=MicroSoft.JET.Oledb.4.0; Data Source=" + Application.StartupPath + "\\Report.mdb" + "; Persist Security Info=False";
                connect.Open();

                string strTableName = "t_ISAPIReport";
                string sCmdText = "select ID,Url from " + strTableName + " order by Url";

                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = connect;
                cmd.CommandText = sCmdText;

                dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {
                    DataTable dt = new DataTable();//创建数据集对象
                    dt.Load(dr);//填充数据集
                    UrltBox.DataSource = dt;//绑定到数据表
                    UrltBox.ValueMember = "ID";//实际值   
                    UrltBox.DisplayMember = "Url"; //显示值
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                dr.Close();
                if (dr != null)
                {
                    dr.Dispose();
                }

                connect.Close();
                if (connect != null)
                {
                    connect.Dispose();
                }
            }
            return 0;
        }

        private void UrltBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (UrltBox.SelectedItem == null || UrltBox.SelectedIndex < 0)
            {
                return;
            }
            String sCmdText = "";
            OleDbConnection connect = new OleDbConnection();
            OleDbDataReader dr = null;
            try
            {
                string strTableName = "";
                //                 if (STDXMLrBtn.Checked == true)
                //                 {
                //                     strTableName = "t_SDKReport";
                //                 }
                //                 else
                //                 {
                strTableName = "t_ISAPIReport";
                //                }
                sCmdText = "select * from " + strTableName + " where ID=" + UrltBox.SelectedValue.ToString();

                connect.ConnectionString = @"Provider=MicroSoft.JET.Oledb.4.0; Data Source=" + Application.StartupPath + "\\Report.mdb" + "; Persist Security Info=False";
                connect.Open();

                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = connect;
                cmd.CommandText = sCmdText;

                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        MethodcBox.Text = dr["Method"].ToString().Trim();
                        InboundDatarTBox.Text = dr["InboundData"].ToString().Trim();
                        OutboundDataTBox.Text = dr["Response"].ToString().Trim();
                    }
                }
                else
                    MessageBox.Show("No record!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            catch (Exception ex)
            {
                string err = ex.Message;
                MessageBox.Show(err, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            finally
            {
                dr.Close();
                if (dr != null)
                {
                    dr.Dispose();
                }

                connect.Close();
                if (connect != null)
                {
                    connect.Dispose();
                }
            }
        }

        private void ExecuteBtn_Click(object sender, EventArgs e)
        {

            string strMethod = MethodcBox.Text.ToString();
            string strUrl = UrltBox.Text.ToString();

            if (strMethod == "" || strUrl == "")
            {
                MessageBox.Show("[URL] or [Method] can not be empty!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (strMethod != "GET" && strMethod != "PUT" && strMethod != "POST" && strMethod != "DELETE")
            {
                MessageBox.Show("Unknown method, please check!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strInbound = InboundDatarTBox.Text.ToString();
            string strOutbound = "";

            OutboundDataTBox.Clear();
            OutboundDataTBox.ForeColor = Color.Black;

            if (!this.DoRequest(strMethod, strUrl, strInbound, out strOutbound))
            {
                OutboundDataTBox.ForeColor = Color.Red;
                OutboundDataTBox.Text = "Opps......\nErrorMsg: ";
                OutboundDataTBox.Text += strOutbound;
                return;
            }
            OutboundDataTBox.Text = strOutbound;

        }

        // //与设备交互
        private bool DoRequest(string strMethod, string strUri, string strInput, out string strOutput)
        {
            strOutput = string.Empty;

            IDeviceTree.DeviceInfo deviceInfo = m_deviceTree.GetSelectedDeviceInfo();
            if (deviceInfo == null)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKDebugTool", "have not selected a device");
                return false;
            }
            //通过透传接口发送请求
            //组装输入
            CHCNetSDK.NET_DVR_XML_CONFIG_INPUT struInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
            struInput.dwSize = (uint)Marshal.SizeOf(struInput);
            string strRequestUrl = strMethod + " " + strUri;
            IntPtr ptrUrl = Marshal.StringToCoTaskMemAnsi(strRequestUrl);
            struInput.lpRequestUrl = ptrUrl;
            struInput.dwRequestUrlLen = (uint)strRequestUrl.Length;
            struInput.dwRecvTimeOut = 3000;
            if (strMethod == "PUT" || strMethod == "POST")
            {
                struInput.lpInBuffer = Marshal.StringToCoTaskMemAnsi(strInput);
                struInput.dwInBufferSize = (uint)strInput.Length;
            }
            else
            {
                struInput.lpInBuffer = IntPtr.Zero;
                struInput.dwInBufferSize = 0;
            }
            IntPtr ptrInput = Marshal.AllocHGlobal(Marshal.SizeOf(struInput));
            Marshal.StructureToPtr(struInput, ptrInput, false);

            //组装输出
            CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT struOutput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();
            struOutput.dwSize = (uint)Marshal.SizeOf(struOutput);
            const int ciOutSize = 1024 * 1024; //预留1M接收数据
            IntPtr ptrOut = Marshal.AllocHGlobal(ciOutSize);
            struOutput.lpOutBuffer = ptrOut;
            struOutput.dwOutBufferSize = ciOutSize;
            struOutput.lpStatusBuffer = ptrOut;
            struOutput.dwStatusSize = ciOutSize;
            IntPtr ptrOutput = Marshal.AllocHGlobal(Marshal.SizeOf(struOutput));
            Marshal.StructureToPtr(struOutput, ptrOutput, false);
            bool bRet = CHCNetSDK.NET_DVR_STDXMLConfig((int)deviceInfo.lLoginID, ptrInput, ptrOutput);
            if (!bRet)
            {
                MainFormHandler.SetStatusString(MainFormHandler.Level.Error, "SDKDebugTool", "NET_DVR_STDXMLConfig failed[" + CHCNetSDK.NET_DVR_GetLastError().ToString() + "]");
                Marshal.FreeHGlobal(ptrInput);
                Marshal.FreeHGlobal(ptrOut);
                Marshal.FreeHGlobal(ptrOutput);
                return false;
            }
            MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "SDKDebugTool", "NET_DVR_STDXMLConfig succeed");
            strOutput = Marshal.PtrToStringAnsi(ptrOut);
            Marshal.FreeHGlobal(ptrInput);
            Marshal.FreeHGlobal(ptrOut);
            Marshal.FreeHGlobal(ptrOutput);                                    

            return true;
        }
    }

}
