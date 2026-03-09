using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SDKFaceLib
{

    public partial class BlockFDForm : PluginsControl
    {
        private delegate void delegateGetFaceLibSearchResult(XmlDocument searchRetxml);

        private IDeviceTree g_deviceTree = PluginsFactory.GetDeviceTreeInstance();
        private string m_strCurDevIP = string.Empty;    //Use to save the selected device IP when switching devices

        public int m_iSelectedIndex;
        private int m_iSelectedFaceDataIndex;

        //private bool m_bSearch = false;

        public BlockFDForm()
        {
            m_strFDID = "";
            m_iSelectedIndex = 0;
            InitializeComponent();
            IDeviceTree.DeviceInfo struDeviceInfo = g_deviceTree.GetSelectedDeviceInfo();
            if (struDeviceInfo.sDeviceIP == null)
            {
                MessageBox.Show("Please login device first!");
                return;
            }
            else
            {
                m_strCurDevIP = struDeviceInfo.sDeviceIP;  //Initialize m_strCurDevIP
            }
            m_XmlFaceData = new xmlFaceData();
            m_XmlFaceDataList = new xmlFaceDataList();
            this.listView_FD.Columns.Add("name", 80, HorizontalAlignment.Center); //Step to add
            this.listView_FD.Columns.Add("count", 80, HorizontalAlignment.Center); //Step to add
            GetBlockFDList();
            bool res=GetSearchAbility();
            if (res)
            {
                if (m_xmlFDLibInfo != null && m_xmlFDLibInfo.FDLib.Count > 0)
                {
                    GetAllFaceData(m_xmlFDLibInfo.FDLib[0].FDID);
                    m_strFDID = m_xmlFDLibInfo.FDLib[0].FDID;
                }
            }
            //Timepicker 
            this.dateTimePickerStartTime.MinDate = DateTime.MinValue; ;
            this.dateTimePickerEndTime.MinDate = DateTime.MinValue;
        }


        private void BlockFDForm_Layout(object sender, LayoutEventArgs e)
        {
           
            
        }

           
           private void BlockFDForm_AddBlockFD(object sender, EventArgs e)
           {
               AddBlockFD addBlockFDForm = new AddBlockFD();
               if (addBlockFDForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
               {
                   this.GetBlockFDList();
               }
           
           }



           private void m_btnModifyFD_Click(object sender, EventArgs e)
           {
               int iCount = this.listView_FD.SelectedItems.Count;

               if (iCount > 0)
              {
                   //Gets the index value of the currently selected row
                  m_iSelectedIndex = this.listView_FD.SelectedItems[0].Index;
                  ModifyBlockFD modifyBlockFD = new ModifyBlockFD();
                   //Get FDID
                  modifyBlockFD.strFDID = m_xmlFDLibInfo.FDLib[m_iSelectedIndex].FDID;
            
                  if (modifyBlockFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                  {
                      this.GetBlockFDList();
                  }
              }
   
           }

 
           
           private void btn_delete_Click(object sender, EventArgs e)
           {
               int iCount = this.listView_FD.SelectedItems.Count;
               if (iCount > 0)
               {
                   //Gets the index value of the currently selected row
                   m_iSelectedIndex = this.listView_FD.SelectedItems[0].Index;
                   m_strFDID = m_xmlFDLibInfo.FDLib[m_iSelectedIndex].FDID;
                   DialogResult dr = MessageBox.Show("Delete this FDLib", "Delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                   if(dr == DialogResult.OK)
                   {
                       DeleteFD();
                       this.GetBlockFDList();
                   }
                   
               }
           }


           private void listView_FD_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
           {
               int iCount = this.listView_FD.SelectedItems.Count;
               if(iCount > 0)
               {
                   m_iSelectedIndex = this.listView_FD.SelectedItems[0].Index;
                   m_strFDID = m_xmlFDLibInfo.FDLib[m_iSelectedIndex].FDID;
                   this.m_flowLayoutPanelPictureData.Controls.Clear();
                   GetAllFaceData(m_strFDID);
               }
             
           }

           public void AddPicture(Image img)
           {
               PictureBox pic = new PictureBox();
               //pic.Width = img.Width;
               //pic.Height = img.Height;
               pic.Image = img;
               pic.SizeMode = PictureBoxSizeMode.StretchImage;
               pic.Margin = new Padding(0,0,0,0);
               pic.Padding = new Padding(0, 0, 0, 0);
               pic.Width = 190;
               pic.Height = 230;
               pic.SizeMode = PictureBoxSizeMode.Zoom;
               pic.BorderStyle = BorderStyle.FixedSingle;
               this.m_flowLayoutPanelPictureData.Controls.Add(pic);
               pic.Click += new EventHandler(pictureBoxClick);
           }

           private void m_buttonAddFace_Click(object sender, EventArgs e)
           {
               AddFaceDada addFaceDadaForm = new AddFaceDada();
               if (m_strFDID == string.Empty && m_xmlFDLibInfo != null && m_xmlFDLibInfo.FDLib.Count > 0)
               {
                   addFaceDadaForm.m_strFDID = m_xmlFDLibInfo.FDLib[0].FDID;
               }
               else if (m_strFDID != string.Empty)
               {
                   addFaceDadaForm.m_strFDID = m_strFDID;
               }
               else
               {
                   MessageBox.Show("Please add face lib first!");
                   return;
               }
               if (addFaceDadaForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
               {
                   Thread.Sleep(100);
                   GetAllFaceData(m_strFDID);
               }
           }

           private void m_buttonModifyFace_Click(object sender, EventArgs e)
           {
               if (m_xmlFDLibInfo != null && m_XmlFaceDataList.MatchList.Count < 0)
               {
                   MessageBox.Show("Please add face picture first!");
                   return;
               }
               ModifyFaceDada modifyFaceDadaForm = new ModifyFaceDada();
               modifyFaceDadaForm.m_strFDID = m_strFDID;
               if (m_iSelectedFaceDataIndex > 0)
               {
                   modifyFaceDadaForm.showFaceDetail(m_XmlFaceDataList.MatchList[m_iSelectedFaceDataIndex]);
               }
               else
               {
                   modifyFaceDadaForm.showFaceDetail(m_XmlFaceDataList.MatchList[0]);
               }
               modifyFaceDadaForm.ShowDialog();
               GetAllFaceData(m_strFDID);
           }

           private void m_buttonDeleteFace_Click(object sender, EventArgs e)
           {
               if (this.listView_FD.SelectedItems.Count > 0)
               {
                   m_iSelectedIndex = this.listView_FD.SelectedItems[0].Index;
               }
               else if (this.listView_FD.Items.Count > 0)
               {
                   m_iSelectedIndex = 0;
               }
               else
               {
                   return;
               }
               if (m_iSelectedFaceDataIndex >= 0)
               {
                   //Gets the index value of the currently selected row
                   
                   m_strFDID = m_xmlFDLibInfo.FDLib[m_iSelectedIndex].FDID;
                   DialogResult dr = MessageBox.Show("Delete this picture", "Delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                   if (dr == DialogResult.OK)
                   {
                       DeleteFacePicture();
                   }

               }
               GetAllFaceData(m_strFDID);
           }
           private void pictureBoxClick(object sender, EventArgs e)
           {
               PictureBox p = sender as PictureBox;
               Graphics pictureborder = p.CreateGraphics();
               Pen pen = new Pen(Color.Orange, 4);
               pictureborder.DrawRectangle(pen, p.ClientRectangle.X, p.ClientRectangle.Y, p.ClientRectangle.X + p.ClientRectangle.Width, p.ClientRectangle.Y + p.ClientRectangle.Height);

               foreach (Control c in this.m_flowLayoutPanelPictureData.Controls)
               {
                   PictureBox p1 = c as PictureBox;
                   if (p != p1)
                   {
                       p1.Invalidate();
                   }
                   else
                   {
                       m_iSelectedFaceDataIndex = this.m_flowLayoutPanelPictureData.Controls.IndexOf(c);
                   }
               }
           }


           //Find the image by properties
           private void buttonSearch_Click(object sender, EventArgs e)
           {
               if (this.g_deviceTree != null && this.g_deviceTree.GetDeviceTreeType() == IDeviceTree.EDeviceTreeType.SDKDeviceTree)
               {
                  // IDeviceTree.DeviceInfo struDeviceInfo = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo();
                   string strCurDevIP = PluginsFactory.GetDeviceTreeInstance().GetSelectedDeviceInfo().sDeviceIP;
                   if (strCurDevIP != m_strCurDevIP)
                   {
                       MessageBox.Show("you selected another device, reload picture firstly");
                       return;
                   }
                   FaceLibSearchByAttrCon searchCon = new FaceLibSearchByAttrCon();
                   try
                   {
                       searchCon.searchResultPosition = 0; //Convert.ToInt32(textBoxSearchResultPos.Text);
                       searchCon.maxResults = 100;    //Convert.ToInt32(textBoxMaxResults.Text); 100
                       searchCon.FPID = "";
                       if(listView_FD.SelectedItems.Count==0)
                       {
                           MessageBox.Show("Please select One FaceLib!");
                           return;
                       }
                       int iSelectedIndex = listView_FD.SelectedItems[0].Index;
                       searchCon.FDID = m_xmlFDLibInfo.FDLib[iSelectedIndex].FDID;


                       //Json has the biggest limitation on the number of face libraries
                       //for (int i = 1; i < listView_FD.SelectedItems.Count; i++)
                       //{
                       //    iSelectedIndex = listView_FD.SelectedItems[i].Index;
                       //    searchCon.FDID = searchCon.FDID + "," + m_jsonFDLibInfo.FDLib[iSelectedIndex].FDID;
                       //}
                       if (dateTimePickerStartTime.Value == DateTime.MinValue)
                       {
                           searchCon.startTime = "";
                       }
                       else
                       {
                           searchCon.startTime = dateTimePickerStartTime.Value.ToUniversalTime().ToString("yyyy-MM-dd");

                       }

                       if(dateTimePickerStartTime.Value == DateTime.MinValue)
                       {
                           searchCon.startTime = "";
                       }
                       else
                       {
                           searchCon.endTime = dateTimePickerEndTime.Value.ToUniversalTime().ToString("yyyy-MM-dd");
                       }
                       
                       searchCon.name = this.textBoxName.Text;
                       searchCon.gender = this.comboBoxGender.Text;//.SelectedText;
                       //switch(this.comboBoxGender.SelectedIndex)
                       //{
                       //    //case 1:
                       //    //    searchCon.gender ="unknown";
                       //    //    break;
                       //    case 2:
                       //        searchCon.gender ="male";
                       //        break;
                       //    case 3:
                       //        searchCon.gender ="female";
                       //        break;
                       //    default:
                       //        searchCon.gender ="";
                       //        break;
                       //}

                       searchCon.city = "";//this.textBoxCity.Text;
                       searchCon.certificateType = this.comboBoxCertificateType.Text;
                       //switch (this.comboBoxCertificateType.SelectedIndex)
                       //{
                       //    case 3:
                       //        searchCon.certificateType = "ID";
                       //        break;
                       //    case 4:
                       //        searchCon.certificateType = "officerID";
                       //        break;
                       //    default:
                       //        searchCon.certificateType = "";
                       //        break;
                       //}
                       searchCon.certificateNumber = this.textBoxCertificateID.Text;

                       FaceLibSearchByAttrRet searchByAttrRet = null;
                       string strOut=string.Empty;
                       if (FaceLibSearchByAttr(searchCon, out strOut))
                       {
                           //字符串转xml
                           XmlDocument resxml = new XmlDocument();//新建对象
                           resxml.LoadXml(strOut);
                           if (resxml.GetElementsByTagName("responseStatusStrg").Item(0).InnerText.ToUpper().Contains("OK"))
                           {
                               GetFaceLibSearchByAttrResult(resxml);
                               MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "BlockFDForm", "Search over: OK!");
                           }
                           else if (resxml.GetElementsByTagName("responseStatusStrg").Item(0).InnerText.ToUpper().Contains("NO MATCHES"))
                           {
                               //m_bSearch = false;
                               //GetFaceLibSearchByAttrResult(resxml);
                               MessageBox.Show("No match!");
                               MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "BlockFDForm", "Search over: NO MATCHES!");
                           }
                           else if (resxml.GetElementsByTagName("responseStatusStrg").Item(0).InnerText.ToUpper().Contains("MORE"))
                           {
                               searchCon.searchResultPosition += searchByAttrRet.numOfMatches;
                               GetFaceLibSearchByAttrResult(resxml);
                               if (searchCon.searchResultPosition == searchByAttrRet.totalMatches)
                               {
                                  // m_bSearch = false;
                                   MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "BlockFDForm", "Search over!");
                               }
                           }
                           else
                           {
                              // m_bSearch = false;
                               MainFormHandler.SetStatusString(MainFormHandler.Level.Info, "BlockFDForm", "Search Failed!");
                           }
                       }
                   }
                   catch (Exception exception)
                   {
                       MessageBox.Show(exception.Message);
                   }
                   
               }
               else
               {
                   MessageBox.Show("Please select ISAPIDeviceTree!");
               }
           }
           public class FaceLibSearchByAttrCon
           {
               public int searchResultPosition { get; set; }
               public int maxResults { get; set; }
               public string faceLibType { get; set; }
               public string FDID { get; set; }
               public string FPID;
               public string startTime { get; set; }
               public string endTime { get; set; }
               public string name { get; set; }
               public string gender { get; set; }
               public string city { get; set; }
               public string certificateType { get; set; }
               public string certificateNumber { get; set; }
           }
           public class FaceLibSearchByAttrRet
           {
               public string responseStatusStrg { get; set; }
               public int numOfMatches { get; set; }
               public int totalMatches { get; set; }
               public int errorCode { get; set; }
               public string errorMsg { get; set; }
               public class cMatchList
               {
                   public string FPID { get; set; }
                   public string faceURL { get; set; }
                   public string name { get; set; }
                   public string gender { get; set; }
                   public string bornTime { get; set; }
                   public string city { get; set; }
                   public string certificateType { get; set; }
                   public string certificateNumber { get; set; }
                   public string caseInfo { get; set; }
                   public string tag { get; set; }
                   public string address { get; set; }
                   public string customInfo { get; set; }
               }
               public List<cMatchList> MatchList { get; set; }
           }
           public bool FaceLibSearchByAttr(FaceLibSearchByAttrCon con, out string strOutput)
           {
               strOutput = string.Empty;
               if (null == con)
               {
                   return false;
               }
               IDeviceTree.DeviceInfo struDeviceInfo = g_deviceTree.GetSelectedDeviceInfo();
               this.m_flowLayoutPanelPictureData.Controls.Clear();
               string strSearchID = Guid.NewGuid().ToString();
               string strxmlSID = "<searchID>" + strSearchID + "</searchID>\r\n";
               string strxmlSRP = "<searchResultPosition>" + con.searchResultPosition + "</searchResultPosition>\r\n";
               string strxmlMR = "<maxResults>" + con.maxResults + "</maxResults>\r\n";
               string strxmlFDID = "<FDID>" + con.FDID + "</FDID>\r\n";
               string strxmlStartTime = "<startTime>" + con.startTime + "</startTime>\r\n";
               string strxmlEndTime = "<endTime>" + con.endTime + "</endTime>\r\n";
               string strxmlName = "<name>" + con.name + "</name>\r\n";
               string strxmlSex = "<sex>" + con.gender + "</sex>\r\n";
               string strxmlcertificateType = "<certificateType>" + con.certificateType + "</certificateType>\r\n";
               string strxmlcertificateNumber = "<certificateNumber>" + con.certificateNumber + "</certificateNumber>\r\n";
               StringBuilder strBuilder = new StringBuilder();
               strBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<FDSearchDescription version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n");
               strBuilder.Append(strxmlSID);
               strBuilder.Append(strxmlSRP);
               strBuilder.Append(strxmlMR);
               strBuilder.Append(strxmlFDID);
               strBuilder.Append(strxmlStartTime);
               strBuilder.Append(strxmlEndTime);
               strBuilder.Append(strxmlName);
               strBuilder.Append(strxmlSex);
               strBuilder.Append(strxmlcertificateType);
               strBuilder.Append(strxmlcertificateNumber);
               strBuilder.Append("</FDSearchDescription>\r\n");
               string strInput = strBuilder.ToString();
               //string strInput = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<FDSearchDescription version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\">\r\n" +
               //                strxmlSID + strxmlSRP + strxmlMR + strxmlFDID + strxmlStartTime + strxmlEndTime + strxmlName + "</FDSearchDescription>\r\n";
               string strUrl = "/ISAPI/Intelligent/FDLib/FDSearch";
               string strMethod = "POST";
               bool res = CommonMethod.DoRequest(struDeviceInfo, strMethod, strUrl, strInput, out strOutput);
               if (!res)
               {
                   iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                   string strErr = "Face image query failed ,Error number：" + iLastErr; //人脸照片删除失败，输出错误号
                   MessageBox.Show(strErr);
                   return false;
               }
               else
               {               
                   return true;
               }
           }


           private void GetFaceLibSearchByAttrResult(XmlDocument searchRetxml)
           {             
               if (this.m_flowLayoutPanelPictureData.InvokeRequired)
               {
                   delegateGetFaceLibSearchResult c = new delegateGetFaceLibSearchResult(GetFaceLibSearchByAttrResult);
                   this.Invoke(c, searchRetxml);  //null
               }
               else
               {
                   this.m_flowLayoutPanelPictureData.Controls.Clear();
                   try
                   {
                       if (searchRetxml != null && Convert.ToInt32(searchRetxml.GetElementsByTagName("totalMatches").Item(0).InnerText) != 0)
                       {
                           if (Convert.ToInt32(searchRetxml.GetElementsByTagName("totalMatches").Item(0).InnerText) == 0)
                           {
                               MessageBox.Show(searchRetxml.GetElementsByTagName("totalMatches").Item(0).InnerText);
                               return;
                           }
                           if (null != searchRetxml.GetElementsByTagName("totalMatches").Item(0).InnerText)
                           {
                               XmlNodeList fdlist = searchRetxml.GetElementsByTagName("MatchElement");
                               m_XmlFaceDataList = new xmlFaceDataList();
                               m_XmlFaceDataList.MatchList = new List<xmlFaceData>();
                               foreach (XmlNode fdnode in fdlist)
                               {
                                   int n = fdnode.ChildNodes.Count;
                                   xmlFaceData xmlfd = new xmlFaceData();
                                   for (int i = 0; i < n; i++)
                                   {
                                       if (fdnode.ChildNodes[i].Name == "name")
                                       {
                                           xmlfd.name = fdnode["name"].InnerText;
                                       }
                                       else if (fdnode.ChildNodes[i].Name == "picURL")
                                       {
                                           xmlfd.faceURL = fdnode["picURL"].InnerText;
                                       }
                                       else if (fdnode.ChildNodes[i].Name == "FDID")
                                       {
                                           xmlfd.FDID = fdnode["FDID"].InnerText;
                                       }
                                       else if (fdnode.ChildNodes[i].Name == "bornTime")
                                       {
                                           xmlfd.bornTime = fdnode["bornTime"].InnerText;
                                       }
                                       else if (fdnode.ChildNodes[i].Name == "city")
                                       {
                                           xmlfd.city = fdnode["city"].InnerText;
                                       }
                                       else if (fdnode.ChildNodes[i].Name == "sex")
                                       {
                                           xmlfd.gender = fdnode["sex"].InnerText;
                                       }
                                       else if (fdnode.ChildNodes[i].Name == "PID")
                                       {
                                           xmlfd.FPID = fdnode["PID"].InnerText;
                                       }
                                       else if (fdnode.ChildNodes[i].Name == "certificateType")
                                       {
                                           xmlfd.certificateType = fdnode["certificateType"].InnerText;
                                       }
                                       else if (fdnode.ChildNodes[i].Name == "certificateNumber")
                                       {
                                           xmlfd.certificateNumber = fdnode["certificateNumber"].InnerText;
                                       }
                                   }
                                   m_XmlFaceDataList.MatchList.Add(xmlfd);


                                   //FaceLibSearchByPicCtrl faceLibCaptureCtrl = new FaceLibSearchByPicCtrl();
                                   //faceLibCaptureCtrl.FaceInfo = targetItem;
                                   string face_picurl = fdnode["picURL"].InnerText; 
                                   
                                   WebRequest webreqface = WebRequest.Create(face_picurl);
                                   webreqface.Credentials = new NetworkCredential(this.g_deviceTree.GetSelectedDeviceInfo().sUsername, this.g_deviceTree.GetSelectedDeviceInfo().sPassword);
                                   WebResponse webresface = webreqface.GetResponse();
                                   using (Stream stream = webresface.GetResponseStream())
                                   {
                                       Image imgFace = null;
                                       try
                                       {
                                           imgFace = Image.FromStream(stream);
                                       }
                                       catch (Exception ex) { }
                                       stream.Close();
                                       stream.Dispose();
                                       System.GC.Collect();
                                       this.AddPicture(imgFace);
                                   }                                 
                               }
                           }                          
                       }
                       else
                       {
                           if (Convert.ToInt32(searchRetxml.GetElementsByTagName("numOfMatches").Item(0).InnerText) != 0) //searchRet != null
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
    }
}
