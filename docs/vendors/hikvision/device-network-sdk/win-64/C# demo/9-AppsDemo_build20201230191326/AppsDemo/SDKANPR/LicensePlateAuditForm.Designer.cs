namespace SDKANPR
{
    partial class LicensePlateAuditForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.LPListAuditContrastSetButton = new System.Windows.Forms.Button();
            this.LPListAuditContrastGetButton = new System.Windows.Forms.Button();
            this.ContrastCountryAreaCheckBox = new System.Windows.Forms.CheckBox();
            this.ContrastPlateCategoryCheckBox = new System.Windows.Forms.CheckBox();
            this.ContrastLicensePlateCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.m_labelDownload = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.DownLoad = new System.Windows.Forms.Button();
            this.DownLoadBrowseButton = new System.Windows.Forms.Button();
            this.DownLoadFileText = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.m_labelUpload = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.UpLoadButton = new System.Windows.Forms.Button();
            this.UpLoadBrowseButton = new System.Windows.Forms.Button();
            this.UpLoadFileText = new System.Windows.Forms.TextBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.SearchLPListAudit_Button = new System.Windows.Forms.Button();
            this.maxResultsTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.searchResultPositionTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.searchIDTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.LicensePlateInfoListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.m_textBoxLaneNo = new System.Windows.Forms.TextBox();
            this.m_comBarrierGateCtrl = new System.Windows.Forms.ComboBox();
            this.SetBarrierGateCtrlButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.LPListAuditContrastSetButton);
            this.groupBox1.Controls.Add(this.LPListAuditContrastGetButton);
            this.groupBox1.Controls.Add(this.ContrastCountryAreaCheckBox);
            this.groupBox1.Controls.Add(this.ContrastPlateCategoryCheckBox);
            this.groupBox1.Controls.Add(this.ContrastLicensePlateCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(2, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(949, 55);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "block allow list compare";
            // 
            // LPListAuditContrastSetButton
            // 
            this.LPListAuditContrastSetButton.Location = new System.Drawing.Point(864, 24);
            this.LPListAuditContrastSetButton.Name = "LPListAuditContrastSetButton";
            this.LPListAuditContrastSetButton.Size = new System.Drawing.Size(75, 23);
            this.LPListAuditContrastSetButton.TabIndex = 4;
            this.LPListAuditContrastSetButton.Text = "设置";
            this.LPListAuditContrastSetButton.UseVisualStyleBackColor = true;
            this.LPListAuditContrastSetButton.Click += new System.EventHandler(this.LPListAuditContrastSetButton_Click);
            // 
            // LPListAuditContrastGetButton
            // 
            this.LPListAuditContrastGetButton.Location = new System.Drawing.Point(783, 24);
            this.LPListAuditContrastGetButton.Name = "LPListAuditContrastGetButton";
            this.LPListAuditContrastGetButton.Size = new System.Drawing.Size(75, 23);
            this.LPListAuditContrastGetButton.TabIndex = 3;
            this.LPListAuditContrastGetButton.Text = "获取";
            this.LPListAuditContrastGetButton.UseVisualStyleBackColor = true;
            this.LPListAuditContrastGetButton.Click += new System.EventHandler(this.LPListAuditContrastGetButton_Click);
            // 
            // ContrastCountryAreaCheckBox
            // 
            this.ContrastCountryAreaCheckBox.AutoSize = true;
            this.ContrastCountryAreaCheckBox.Location = new System.Drawing.Point(299, 28);
            this.ContrastCountryAreaCheckBox.Name = "ContrastCountryAreaCheckBox";
            this.ContrastCountryAreaCheckBox.Size = new System.Drawing.Size(96, 16);
            this.ContrastCountryAreaCheckBox.TabIndex = 2;
            this.ContrastCountryAreaCheckBox.Text = "国家地区比对";
            this.ContrastCountryAreaCheckBox.UseVisualStyleBackColor = true;
            // 
            // ContrastPlateCategoryCheckBox
            // 
            this.ContrastPlateCategoryCheckBox.AutoSize = true;
            this.ContrastPlateCategoryCheckBox.Location = new System.Drawing.Point(161, 28);
            this.ContrastPlateCategoryCheckBox.Name = "ContrastPlateCategoryCheckBox";
            this.ContrastPlateCategoryCheckBox.Size = new System.Drawing.Size(96, 16);
            this.ContrastPlateCategoryCheckBox.TabIndex = 1;
            this.ContrastPlateCategoryCheckBox.Text = "车牌类型比对";
            this.ContrastPlateCategoryCheckBox.UseVisualStyleBackColor = true;
            // 
            // ContrastLicensePlateCheckBox
            // 
            this.ContrastLicensePlateCheckBox.AutoSize = true;
            this.ContrastLicensePlateCheckBox.Location = new System.Drawing.Point(18, 28);
            this.ContrastLicensePlateCheckBox.Name = "ContrastLicensePlateCheckBox";
            this.ContrastLicensePlateCheckBox.Size = new System.Drawing.Size(96, 16);
            this.ContrastLicensePlateCheckBox.TabIndex = 0;
            this.ContrastLicensePlateCheckBox.Text = "启用车牌比对";
            this.ContrastLicensePlateCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.groupBox4);
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Location = new System.Drawing.Point(0, 336);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(952, 279);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "block allow list import";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.m_labelDownload);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.DownLoad);
            this.groupBox4.Controls.Add(this.DownLoadBrowseButton);
            this.groupBox4.Controls.Add(this.DownLoadFileText);
            this.groupBox4.Location = new System.Drawing.Point(20, 165);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(921, 113);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "block allow list export";
            // 
            // m_labelDownload
            // 
            this.m_labelDownload.AutoSize = true;
            this.m_labelDownload.Location = new System.Drawing.Point(81, 69);
            this.m_labelDownload.Name = "m_labelDownload";
            this.m_labelDownload.Size = new System.Drawing.Size(0, 12);
            this.m_labelDownload.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "下载进度:";
            // 
            // DownLoad
            // 
            this.DownLoad.Location = new System.Drawing.Point(726, 27);
            this.DownLoad.Name = "DownLoad";
            this.DownLoad.Size = new System.Drawing.Size(75, 23);
            this.DownLoad.TabIndex = 6;
            this.DownLoad.Text = "下载";
            this.DownLoad.UseVisualStyleBackColor = true;
            this.DownLoad.Click += new System.EventHandler(this.DownLoad_Click);
            // 
            // DownLoadBrowseButton
            // 
            this.DownLoadBrowseButton.Location = new System.Drawing.Point(633, 27);
            this.DownLoadBrowseButton.Name = "DownLoadBrowseButton";
            this.DownLoadBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.DownLoadBrowseButton.TabIndex = 5;
            this.DownLoadBrowseButton.Text = "浏览";
            this.DownLoadBrowseButton.UseVisualStyleBackColor = true;
            this.DownLoadBrowseButton.Click += new System.EventHandler(this.DownLoadBrowseButton_Click);
            // 
            // DownLoadFileText
            // 
            this.DownLoadFileText.Location = new System.Drawing.Point(17, 29);
            this.DownLoadFileText.Name = "DownLoadFileText";
            this.DownLoadFileText.Size = new System.Drawing.Size(591, 21);
            this.DownLoadFileText.TabIndex = 4;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.m_labelUpload);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.UpLoadButton);
            this.groupBox3.Controls.Add(this.UpLoadBrowseButton);
            this.groupBox3.Controls.Add(this.UpLoadFileText);
            this.groupBox3.Location = new System.Drawing.Point(20, 46);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(921, 113);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "import block allow list";
            // 
            // m_labelUpload
            // 
            this.m_labelUpload.AutoSize = true;
            this.m_labelUpload.Cursor = System.Windows.Forms.Cursors.Default;
            this.m_labelUpload.Location = new System.Drawing.Point(74, 74);
            this.m_labelUpload.Name = "m_labelUpload";
            this.m_labelUpload.Size = new System.Drawing.Size(0, 12);
            this.m_labelUpload.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "上传进度:";
            // 
            // UpLoadButton
            // 
            this.UpLoadButton.Location = new System.Drawing.Point(726, 34);
            this.UpLoadButton.Name = "UpLoadButton";
            this.UpLoadButton.Size = new System.Drawing.Size(75, 23);
            this.UpLoadButton.TabIndex = 2;
            this.UpLoadButton.Text = "上传";
            this.UpLoadButton.UseVisualStyleBackColor = true;
            this.UpLoadButton.Click += new System.EventHandler(this.UpLoadButton_Click);
            // 
            // UpLoadBrowseButton
            // 
            this.UpLoadBrowseButton.Location = new System.Drawing.Point(633, 34);
            this.UpLoadBrowseButton.Name = "UpLoadBrowseButton";
            this.UpLoadBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.UpLoadBrowseButton.TabIndex = 1;
            this.UpLoadBrowseButton.Text = "浏览";
            this.UpLoadBrowseButton.UseVisualStyleBackColor = true;
            this.UpLoadBrowseButton.Click += new System.EventHandler(this.UpLoadBrowseButton_Click);
            // 
            // UpLoadFileText
            // 
            this.UpLoadFileText.Location = new System.Drawing.Point(17, 36);
            this.UpLoadFileText.Name = "UpLoadFileText";
            this.UpLoadFileText.Size = new System.Drawing.Size(591, 21);
            this.UpLoadFileText.TabIndex = 0;
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.SearchLPListAudit_Button);
            this.groupBox5.Controls.Add(this.maxResultsTextBox);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.searchResultPositionTextBox);
            this.groupBox5.Controls.Add(this.label4);
            this.groupBox5.Controls.Add(this.searchIDTextBox);
            this.groupBox5.Controls.Add(this.label3);
            this.groupBox5.Controls.Add(this.LicensePlateInfoListView);
            this.groupBox5.Location = new System.Drawing.Point(2, 127);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(949, 203);
            this.groupBox5.TabIndex = 3;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "search block allow list";
            // 
            // SearchLPListAudit_Button
            // 
            this.SearchLPListAudit_Button.Location = new System.Drawing.Point(783, 25);
            this.SearchLPListAudit_Button.Name = "SearchLPListAudit_Button";
            this.SearchLPListAudit_Button.Size = new System.Drawing.Size(75, 23);
            this.SearchLPListAudit_Button.TabIndex = 13;
            this.SearchLPListAudit_Button.Text = "查询";
            this.SearchLPListAudit_Button.UseVisualStyleBackColor = true;
            this.SearchLPListAudit_Button.Click += new System.EventHandler(this.SearchLPListAudit_Button_Click);
            // 
            // maxResultsTextBox
            // 
            this.maxResultsTextBox.Location = new System.Drawing.Point(457, 27);
            this.maxResultsTextBox.Name = "maxResultsTextBox";
            this.maxResultsTextBox.Size = new System.Drawing.Size(79, 21);
            this.maxResultsTextBox.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(380, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 12);
            this.label5.TabIndex = 11;
            this.label5.Text = "最大记录数:";
            // 
            // searchResultPositionTextBox
            // 
            this.searchResultPositionTextBox.Location = new System.Drawing.Point(263, 25);
            this.searchResultPositionTextBox.Name = "searchResultPositionTextBox";
            this.searchResultPositionTextBox.Size = new System.Drawing.Size(79, 21);
            this.searchResultPositionTextBox.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(198, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "起始位置:";
            // 
            // searchIDTextBox
            // 
            this.searchIDTextBox.Location = new System.Drawing.Point(93, 25);
            this.searchIDTextBox.Name = "searchIDTextBox";
            this.searchIDTextBox.Size = new System.Drawing.Size(79, 21);
            this.searchIDTextBox.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "搜索标识ID:";
            // 
            // LicensePlateInfoListView
            // 
            this.LicensePlateInfoListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11});
            this.LicensePlateInfoListView.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.LicensePlateInfoListView.FullRowSelect = true;
            this.LicensePlateInfoListView.GridLines = true;
            this.LicensePlateInfoListView.Location = new System.Drawing.Point(3, 67);
            this.LicensePlateInfoListView.Margin = new System.Windows.Forms.Padding(0);
            this.LicensePlateInfoListView.Name = "LicensePlateInfoListView";
            this.LicensePlateInfoListView.Size = new System.Drawing.Size(943, 133);
            this.LicensePlateInfoListView.TabIndex = 6;
            this.LicensePlateInfoListView.UseCompatibleStateImageBehavior = false;
            this.LicensePlateInfoListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "ID";
            this.columnHeader1.Width = 40;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "车牌";
            this.columnHeader2.Width = 93;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "车牌类型";
            this.columnHeader3.Width = 75;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "createTime";
            this.columnHeader4.Width = 73;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "方向";
            this.columnHeader5.Width = 52;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "车道号";
            this.columnHeader6.Width = 66;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "车牌类型";
            this.columnHeader7.Width = 94;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "国家";
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "区域";
            this.columnHeader9.Width = 56;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "effectiveTime";
            this.columnHeader10.Width = 146;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "countryIndex";
            this.columnHeader11.Width = 132;
            // 
            // groupBox6
            // 
            this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox6.Controls.Add(this.label7);
            this.groupBox6.Controls.Add(this.label6);
            this.groupBox6.Controls.Add(this.m_textBoxLaneNo);
            this.groupBox6.Controls.Add(this.m_comBarrierGateCtrl);
            this.groupBox6.Controls.Add(this.SetBarrierGateCtrlButton);
            this.groupBox6.Location = new System.Drawing.Point(3, 66);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(949, 55);
            this.groupBox6.TabIndex = 4;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "道闸控制";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(203, 27);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 12);
            this.label7.TabIndex = 8;
            this.label7.Text = "道闸控制：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(33, 27);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 7;
            this.label6.Text = "道闸号：";
            // 
            // m_textBoxLaneNo
            // 
            this.m_textBoxLaneNo.Location = new System.Drawing.Point(92, 24);
            this.m_textBoxLaneNo.Name = "m_textBoxLaneNo";
            this.m_textBoxLaneNo.Size = new System.Drawing.Size(100, 21);
            this.m_textBoxLaneNo.TabIndex = 6;
            // 
            // m_comBarrierGateCtrl
            // 
            this.m_comBarrierGateCtrl.FormattingEnabled = true;
            this.m_comBarrierGateCtrl.Items.AddRange(new object[] {
            "关闭道闸",
            "开启道闸",
            "停止道闸",
            "锁定道闸",
            "解锁道闸"});
            this.m_comBarrierGateCtrl.Location = new System.Drawing.Point(262, 24);
            this.m_comBarrierGateCtrl.Name = "m_comBarrierGateCtrl";
            this.m_comBarrierGateCtrl.Size = new System.Drawing.Size(121, 20);
            this.m_comBarrierGateCtrl.TabIndex = 5;
            // 
            // SetBarrierGateCtrlButton
            // 
            this.SetBarrierGateCtrlButton.Location = new System.Drawing.Point(782, 18);
            this.SetBarrierGateCtrlButton.Name = "SetBarrierGateCtrlButton";
            this.SetBarrierGateCtrlButton.Size = new System.Drawing.Size(75, 23);
            this.SetBarrierGateCtrlButton.TabIndex = 4;
            this.SetBarrierGateCtrlButton.Text = "控制";
            this.SetBarrierGateCtrlButton.UseVisualStyleBackColor = true;
            this.SetBarrierGateCtrlButton.Click += new System.EventHandler(this.SetBarrierGateCtrlButton_Click);
            // 
            // LicensePlateAuditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(953, 626);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "LicensePlateAuditForm";
            this.Text = "LicensePlateAuditForm";
            this.Load += new System.EventHandler(this.LicensePlateAuditForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button UpLoadButton;
        private System.Windows.Forms.Button UpLoadBrowseButton;
        private System.Windows.Forms.TextBox UpLoadFileText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button DownLoad;
        private System.Windows.Forms.Button DownLoadBrowseButton;
        private System.Windows.Forms.TextBox DownLoadFileText;
        private System.Windows.Forms.Label m_labelUpload;
        private System.Windows.Forms.Label m_labelDownload;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.ListView LicensePlateInfoListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox searchIDTextBox;
        private System.Windows.Forms.Button SearchLPListAudit_Button;
        private System.Windows.Forms.TextBox maxResultsTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox searchResultPositionTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button LPListAuditContrastSetButton;
        private System.Windows.Forms.Button LPListAuditContrastGetButton;
        private System.Windows.Forms.CheckBox ContrastCountryAreaCheckBox;
        private System.Windows.Forms.CheckBox ContrastPlateCategoryCheckBox;
        private System.Windows.Forms.CheckBox ContrastLicensePlateCheckBox;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button SetBarrierGateCtrlButton;
        private System.Windows.Forms.ComboBox m_comBarrierGateCtrl;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox m_textBoxLaneNo;
        private System.Windows.Forms.Label label7;
    }
}