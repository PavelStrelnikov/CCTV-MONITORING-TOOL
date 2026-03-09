using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Runtime.InteropServices;

namespace SDKFaceLib
{
    public partial class FaceLibSearchByPicForm : PluginsControl
    {
        private bool m_bSearch = false;
        private IntPtr m_ptrImage;
        private string m_strModeData = string.Empty;
        private string m_strTaskID = string.Empty;
        private string m_strFaceURL = string.Empty;
        private string m_strCurDevIP = string.Empty;    //Use to save the selected device IP when switching devices
        private delegate void delegateGetFaceLibSearchResult(FaceLibSearchByPicRet searchRet);
        public FaceLibSearchByPicForm()
        {
            InitializeComponent();
        }
        private string m_strPicPath = string.Empty;
        private void pictureBoxSearchPic_Click(object sender, EventArgs e)
        {
            IDeviceTree.DeviceInfo struDeviceInfo = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo();
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Image Formats(*.bmp;*.jpg;*.jpeg;*.gif;*.png;*.tif)|*.bmp;*.jpg;*.jpeg;*.gif;*.png;*.tif"
                        + "|Bitmaps (*.bmp)|*.bmp"
                        + "|GIFs (*.gif)|*.gif"
                        + "|JPEGs (*.jpg)|*.jpg;*.jpeg"
                        + "|PNGs (*.png)|*.png"
                        + "|TIFs (*.tif)|*.tif"
                        + "|All Files (*.*)|*.*";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_strPicPath = dlg.FileName;
                this.pictureBoxSearchPic.Image = Image.FromFile(m_strPicPath);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (m_bSearch)
            {
                return;
            }
            m_bSearch = true;
            this.panelPictures.Controls.Clear();
            if (this.pictureBoxSearchPic.Image == null)
            {
                MessageBox.Show("click picturebox to choose a picture firstly");
                return;
            }
            int iFilelength = 0;
            try
            {
                FileStream fs = File.OpenRead(m_strPicPath); // OpenRead
                iFilelength = (int)fs.Length;
                Byte[] byImage = new Byte[iFilelength];
                fs.Read(byImage, 0, iFilelength);
                m_ptrImage = Marshal.AllocHGlobal(iFilelength);
                Marshal.Copy(byImage, 0, m_ptrImage, iFilelength);
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);               
            }           
            IDeviceTree.DeviceInfo struDeviceInfo = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo();
            string strOutput = "";
            string strRequestUrl = "/ISAPI/Intelligent/analysisImage/face\r\n";
            string strMethod = "POST";
            bool res = CommonMethod.DoImageRequest(struDeviceInfo, strMethod, strRequestUrl, m_ptrImage, iFilelength, out strOutput);
            Marshal.FreeHGlobal(m_ptrImage);
            if (!res)
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "Face image analysis failed, Error number ：" + iLastErr; //人脸图片分析失败，输出错误号
                MessageBox.Show(strErr);
            }
            else
            {
                 XmlDocument outxml = new XmlDocument();//新建对象
                 outxml.LoadXml(strOutput);
                 m_strModeData = outxml.GetElementsByTagName("modeData").Item(0).InnerText;   
            }

 
            FaceLibSearchByPicCon searchCon = new FaceLibSearchByPicCon();
            try
            {
                searchCon.searchResultPosition = Convert.ToInt32(textBoxSearchResultPos.Text);
                searchCon.maxResults = Convert.ToInt32(textBoxMaxResults.Text);
                if (checkedListBoxFDlib.Items.Count > 0)
                {
                    searchCon.FDLib = new List<FaceLibSearchByPicCon.cFDID>();
                }
                for (int i = 0; i < checkedListBoxFDlib.Items.Count; i++)
                {
                    if (checkedListBoxFDlib.GetItemChecked(i))
                    {
                        FaceLibSearchByPicCon.cFDID fdlib = new FaceLibSearchByPicCon.cFDID();
                        //fdlib.FDID = checkedListBoxFDlib.Items[i].ToString();
                        fdlib.FDID = m_xmlFDLibInfo.FDLib[i].FDID.ToString();
                        searchCon.FDLib.Add(fdlib);
                    }
                }
                searchCon.dataType = "URL";
                searchCon.faceURL = m_strFaceURL;
                searchCon.startTime = dtStartTime.Value.ToUniversalTime().ToString("yyyy-MM-dd");
                searchCon.endTime = dtEndTime.Value.ToUniversalTime().ToString("yyyy-MM-dd");
                searchCon.name = this.textBoxName.Text;
                searchCon.gender = this.comboBoxGender.Text;
                searchCon.city = this.textBoxCity.Text;
                searchCon.certificateType = this.comboBoxCertificateType.Text;
                searchCon.certificateNumber = this.textBoxCertificateNumber.Text;
                searchCon.similarity = Convert.ToSingle(this.textBoxSimilarity.Text);

                FaceLibSearchByPicRet searchByPicRet = null;
                while (FaceLibSearchByPic(searchCon, out searchByPicRet))
                {
                    if (searchByPicRet.responseStatusStrg.ToUpper().Contains("OK"))
                    {
                        m_bSearch = false;
                        GetFaceLibSearchByPicResult(searchByPicRet);
                        MessageBox.Show("Search over!");
                        break;
                    }
                    else if (searchByPicRet.responseStatusStrg.ToUpper().Contains("NO MATCH"))
                    {
                        m_bSearch = false;
                        MessageBox.Show("No match!");
                        break;
                    }
                    else if (searchByPicRet.responseStatusStrg.ToUpper().Contains("MORE"))
                    {
                        if (searchByPicRet != null && !string.IsNullOrEmpty(searchByPicRet.taskID))
                        {
                            searchCon.taskID = searchByPicRet.taskID;
                        }
                        searchCon.searchResultPosition += searchByPicRet.numOfMatches;
                        GetFaceLibSearchByPicResult(searchByPicRet);
                        if (searchCon.searchResultPosition == searchByPicRet.totalMatches)
                        {
                            m_bSearch = false;
                            MessageBox.Show("Search over!");
                            break;
                        }                
                    }
                    else
                    {
                        m_bSearch = false;
                        MessageBox.Show("Search Failed!");
                    }
                }
            }
            catch (Exception exception)
            {
                m_bSearch = false;
                MessageBox.Show(exception.Message);
            }
            m_bSearch = false;
        }

        private void FaceLibSearchByPicForm_Load(object sender, EventArgs e)
        {

            FaceLibSearchByPicCaps caps = null;
            if (GetFaceLibSearchByPicCaps(out caps))
            {
                this.Enabled = true;
                GetBlockFDList();
                GetSearchAbility();
            }
            else
            {
                this.Enabled = false;
            }
            this.comboBoxGender.Enabled = false;
            this.textBoxName.Enabled = false;
            this.textBoxCity.Enabled = false;
            this.comboBoxCertificateType.Enabled = false;
            this.textBoxCertificateNumber.Enabled = false;
        }

        private void GetFaceLibSearchByPicResult(FaceLibSearchByPicRet searchRet)
        {
            IDeviceTree.DeviceInfo struDeviceInfo = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo();
            if (this.panelPictures.InvokeRequired)
            {
                delegateGetFaceLibSearchResult c = new delegateGetFaceLibSearchResult(GetFaceLibSearchByPicResult);
                this.Invoke(c, null);
            }
            else
            {
                try
                {
                    if (searchRet != null && searchRet.MatchList != null)
                    {
                        if (searchRet.totalMatches == 0)
                        {
                            MessageBox.Show(searchRet.responseStatusStrg);
                            return;
                        }
                        foreach (FaceLibSearchByPicRet.cMatchList targetItem in searchRet.MatchList)
                        {
                            FaceLibSearchByPicCtrl faceLibCaptureCtrl = new FaceLibSearchByPicCtrl();
                            faceLibCaptureCtrl.FaceInfo = targetItem;
                            faceLibCaptureCtrl.Similarity = targetItem.similarity.ToString();
                            string face_picurl = targetItem.faceURL;
                            WebRequest webreqface = WebRequest.Create(face_picurl);
                            webreqface.Credentials = new NetworkCredential(struDeviceInfo.sUsername, struDeviceInfo.sPassword);
                            WebResponse webresface = webreqface.GetResponse();
                            using (Stream stream = webresface.GetResponseStream())
                            {
                                faceLibCaptureCtrl.FacePicture = null;
                                try
                                {
                                    faceLibCaptureCtrl.FacePicture = Image.FromStream(stream, true, false);
                                }
                                catch (Exception ex) { }
                                stream.Close();
                                stream.Dispose();
                                System.GC.Collect();
                            }
                            this.AddPicture(faceLibCaptureCtrl);
                        }
                    }
                    else
                    {
                        if (searchRet != null)
                        {
                            MessageBox.Show("Show picture err");
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }
        private void AddPicture(UserControl imgCtrl)
        {
            this.panelPictures.Controls.Add(imgCtrl);
        }

        private void textBoxSimilarity_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
            if ((e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == (char)8)
            {
                e.Handled = false;
            }
        }

        private void textBoxSimilarity_TextChanged(object sender, EventArgs e)
        {
            if (int.Parse(this.textBoxSimilarity.Text) < 0 || int.Parse(this.textBoxSimilarity.Text) >100)
            {
                MessageBox.Show("the number out of range!");
                this.textBoxSimilarity.Text = "50";
            }
        }
    }
}
