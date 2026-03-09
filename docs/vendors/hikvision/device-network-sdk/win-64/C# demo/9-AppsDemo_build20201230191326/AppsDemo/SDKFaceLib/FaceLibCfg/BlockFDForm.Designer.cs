namespace SDKFaceLib
{
    partial class BlockFDForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BlockFDForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.m_btnModifyFD = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.m_btnAddFD = new System.Windows.Forms.Button();
            this.btn_delete = new System.Windows.Forms.Button();
            this.listView_FD = new System.Windows.Forms.ListView();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.m_buttonDeleteFace = new System.Windows.Forms.Button();
            this.m_buttonModifyFace = new System.Windows.Forms.Button();
            this.m_buttonAddFace = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dateTimePickerEndTime = new System.Windows.Forms.DateTimePicker();
            this.dateTimePickerStartTime = new System.Windows.Forms.DateTimePicker();
            this.labelEndBirth = new System.Windows.Forms.Label();
            this.labelStartBirth = new System.Windows.Forms.Label();
            this.comboBoxCity = new System.Windows.Forms.ComboBox();
            this.labelCity = new System.Windows.Forms.Label();
            this.comboBoxProvince = new System.Windows.Forms.ComboBox();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.labelProvince = new System.Windows.Forms.Label();
            this.textBoxCertificateID = new System.Windows.Forms.TextBox();
            this.comboBoxCertificateType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.labelCertificateType = new System.Windows.Forms.Label();
            this.comboBoxGender = new System.Windows.Forms.ComboBox();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.labelGender = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.m_flowLayoutPanelPictureData = new System.Windows.Forms.FlowLayoutPanel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.m_btnModifyFD);
            this.groupBox1.Controls.Add(this.m_btnAddFD);
            this.groupBox1.Controls.Add(this.btn_delete);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(243, 57);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // m_btnModifyFD
            // 
            this.m_btnModifyFD.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.m_btnModifyFD.FlatAppearance.BorderSize = 0;
            this.m_btnModifyFD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_btnModifyFD.ImageIndex = 2;
            this.m_btnModifyFD.ImageList = this.imageList1;
            this.m_btnModifyFD.Location = new System.Drawing.Point(76, 20);
            this.m_btnModifyFD.Name = "m_btnModifyFD";
            this.m_btnModifyFD.Size = new System.Drawing.Size(38, 23);
            this.m_btnModifyFD.TabIndex = 1;
            this.toolTip1.SetToolTip(this.m_btnModifyFD, "编辑库");
            this.m_btnModifyFD.UseVisualStyleBackColor = true;
            this.m_btnModifyFD.Click += new System.EventHandler(this.m_btnModifyFD_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "special.png");
            this.imageList1.Images.SetKeyName(1, "icon_delete.png");
            this.imageList1.Images.SetKeyName(2, "icon_edit.png");
            this.imageList1.Images.SetKeyName(3, "icon_video.png");
            this.imageList1.Images.SetKeyName(4, "untitled.png");
            this.imageList1.Images.SetKeyName(5, "untitled.png");
            // 
            // m_btnAddFD
            // 
            this.m_btnAddFD.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.m_btnAddFD.FlatAppearance.BorderSize = 0;
            this.m_btnAddFD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_btnAddFD.ImageIndex = 5;
            this.m_btnAddFD.ImageList = this.imageList1;
            this.m_btnAddFD.Location = new System.Drawing.Point(6, 20);
            this.m_btnAddFD.Name = "m_btnAddFD";
            this.m_btnAddFD.Size = new System.Drawing.Size(38, 23);
            this.m_btnAddFD.TabIndex = 1;
            this.toolTip1.SetToolTip(this.m_btnAddFD, "添加库");
            this.m_btnAddFD.UseVisualStyleBackColor = true;
            this.m_btnAddFD.Click += new System.EventHandler(this.BlockFDForm_AddBlockFD);
            // 
            // btn_delete
            // 
            this.btn_delete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_delete.FlatAppearance.BorderSize = 0;
            this.btn_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_delete.ImageIndex = 1;
            this.btn_delete.ImageList = this.imageList1;
            this.btn_delete.Location = new System.Drawing.Point(146, 20);
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.Size = new System.Drawing.Size(38, 23);
            this.btn_delete.TabIndex = 1;
            this.toolTip1.SetToolTip(this.btn_delete, "删除库");
            this.btn_delete.UseVisualStyleBackColor = true;
            this.btn_delete.Click += new System.EventHandler(this.btn_delete_Click);
            // 
            // listView_FD
            // 
            this.listView_FD.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listView_FD.LargeImageList = this.imageList1;
            this.listView_FD.Location = new System.Drawing.Point(0, 0);
            this.listView_FD.MultiSelect = false;
            this.listView_FD.Name = "listView_FD";
            this.listView_FD.Size = new System.Drawing.Size(243, 249);
            this.listView_FD.SmallImageList = this.imageList1;
            this.listView_FD.TabIndex = 2;
            this.listView_FD.UseCompatibleStateImageBehavior = false;
            this.listView_FD.View = System.Windows.Forms.View.List;
            this.listView_FD.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listView_FD_ItemSelectionChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.m_buttonDeleteFace);
            this.groupBox2.Controls.Add(this.m_buttonModifyFace);
            this.groupBox2.Controls.Add(this.m_buttonAddFace);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(799, 55);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            // 
            // m_buttonDeleteFace
            // 
            this.m_buttonDeleteFace.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.m_buttonDeleteFace.FlatAppearance.BorderSize = 0;
            this.m_buttonDeleteFace.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_buttonDeleteFace.ImageIndex = 1;
            this.m_buttonDeleteFace.ImageList = this.imageList1;
            this.m_buttonDeleteFace.Location = new System.Drawing.Point(144, 20);
            this.m_buttonDeleteFace.Name = "m_buttonDeleteFace";
            this.m_buttonDeleteFace.Size = new System.Drawing.Size(38, 23);
            this.m_buttonDeleteFace.TabIndex = 5;
            this.toolTip1.SetToolTip(this.m_buttonDeleteFace, "删除图片");
            this.m_buttonDeleteFace.UseVisualStyleBackColor = true;
            this.m_buttonDeleteFace.Click += new System.EventHandler(this.m_buttonDeleteFace_Click);
            // 
            // m_buttonModifyFace
            // 
            this.m_buttonModifyFace.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.m_buttonModifyFace.FlatAppearance.BorderSize = 0;
            this.m_buttonModifyFace.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_buttonModifyFace.ImageIndex = 2;
            this.m_buttonModifyFace.ImageList = this.imageList1;
            this.m_buttonModifyFace.Location = new System.Drawing.Point(75, 20);
            this.m_buttonModifyFace.Name = "m_buttonModifyFace";
            this.m_buttonModifyFace.Size = new System.Drawing.Size(38, 23);
            this.m_buttonModifyFace.TabIndex = 4;
            this.toolTip1.SetToolTip(this.m_buttonModifyFace, "编辑图片");
            this.m_buttonModifyFace.UseVisualStyleBackColor = true;
            this.m_buttonModifyFace.Click += new System.EventHandler(this.m_buttonModifyFace_Click);
            // 
            // m_buttonAddFace
            // 
            this.m_buttonAddFace.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.m_buttonAddFace.FlatAppearance.BorderSize = 0;
            this.m_buttonAddFace.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_buttonAddFace.ImageIndex = 5;
            this.m_buttonAddFace.ImageList = this.imageList1;
            this.m_buttonAddFace.Location = new System.Drawing.Point(6, 20);
            this.m_buttonAddFace.Name = "m_buttonAddFace";
            this.m_buttonAddFace.Size = new System.Drawing.Size(38, 23);
            this.m_buttonAddFace.TabIndex = 2;
            this.toolTip1.SetToolTip(this.m_buttonAddFace, "添加图片");
            this.m_buttonAddFace.UseVisualStyleBackColor = true;
            this.m_buttonAddFace.Click += new System.EventHandler(this.m_buttonAddFace_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(1046, 578);
            this.splitContainer1.SplitterDistance = 243;
            this.splitContainer1.TabIndex = 6;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.panel3);
            this.splitContainer2.Panel2.Controls.Add(this.panel2);
            this.splitContainer2.Size = new System.Drawing.Size(243, 578);
            this.splitContainer2.SplitterDistance = 57;
            this.splitContainer2.TabIndex = 3;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.panel1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 255);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(243, 262);
            this.panel3.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.dateTimePickerEndTime);
            this.panel1.Controls.Add(this.dateTimePickerStartTime);
            this.panel1.Controls.Add(this.labelEndBirth);
            this.panel1.Controls.Add(this.labelStartBirth);
            this.panel1.Controls.Add(this.comboBoxCity);
            this.panel1.Controls.Add(this.labelCity);
            this.panel1.Controls.Add(this.comboBoxProvince);
            this.panel1.Controls.Add(this.buttonSearch);
            this.panel1.Controls.Add(this.labelProvince);
            this.panel1.Controls.Add(this.textBoxCertificateID);
            this.panel1.Controls.Add(this.comboBoxCertificateType);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.labelCertificateType);
            this.panel1.Controls.Add(this.comboBoxGender);
            this.panel1.Controls.Add(this.textBoxName);
            this.panel1.Controls.Add(this.labelGender);
            this.panel1.Controls.Add(this.labelName);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(243, 262);
            this.panel1.TabIndex = 3;
            // 
            // dateTimePickerEndTime
            // 
            this.dateTimePickerEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerEndTime.Location = new System.Drawing.Point(109, 145);
            this.dateTimePickerEndTime.Name = "dateTimePickerEndTime";
            this.dateTimePickerEndTime.Size = new System.Drawing.Size(121, 21);
            this.dateTimePickerEndTime.TabIndex = 16;
            // 
            // dateTimePickerStartTime
            // 
            this.dateTimePickerStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerStartTime.Location = new System.Drawing.Point(109, 118);
            this.dateTimePickerStartTime.Name = "dateTimePickerStartTime";
            this.dateTimePickerStartTime.Size = new System.Drawing.Size(121, 21);
            this.dateTimePickerStartTime.TabIndex = 15;
            this.dateTimePickerStartTime.Value = new System.DateTime(1900, 8, 23, 15, 5, 0, 0);
            // 
            // labelEndBirth
            // 
            this.labelEndBirth.AutoSize = true;
            this.labelEndBirth.Location = new System.Drawing.Point(0, 151);
            this.labelEndBirth.Name = "labelEndBirth";
            this.labelEndBirth.Size = new System.Drawing.Size(71, 12);
            this.labelEndBirth.TabIndex = 14;
            this.labelEndBirth.Text = "endBornTime";
            // 
            // labelStartBirth
            // 
            this.labelStartBirth.AutoSize = true;
            this.labelStartBirth.Location = new System.Drawing.Point(2, 124);
            this.labelStartBirth.Name = "labelStartBirth";
            this.labelStartBirth.Size = new System.Drawing.Size(83, 12);
            this.labelStartBirth.TabIndex = 13;
            this.labelStartBirth.Text = "startBornTime";
            // 
            // comboBoxCity
            // 
            this.comboBoxCity.FormattingEnabled = true;
            this.comboBoxCity.Items.AddRange(new object[] {
            "不限",
            "北京市",
            "天津市",
            "河北省",
            "山西省",
            "内蒙古自治区",
            "辽宁省",
            "吉林省"});
            this.comboBoxCity.Location = new System.Drawing.Point(111, 201);
            this.comboBoxCity.Name = "comboBoxCity";
            this.comboBoxCity.Size = new System.Drawing.Size(121, 20);
            this.comboBoxCity.TabIndex = 12;
            this.comboBoxCity.Text = "不限";
            this.comboBoxCity.Visible = false;
            // 
            // labelCity
            // 
            this.labelCity.AutoSize = true;
            this.labelCity.Location = new System.Drawing.Point(4, 204);
            this.labelCity.Name = "labelCity";
            this.labelCity.Size = new System.Drawing.Size(29, 12);
            this.labelCity.TabIndex = 11;
            this.labelCity.Text = "city";
            this.labelCity.Visible = false;
            // 
            // comboBoxProvince
            // 
            this.comboBoxProvince.FormattingEnabled = true;
            this.comboBoxProvince.Items.AddRange(new object[] {
            "不限",
            "北京市",
            "天津市",
            "河北省",
            "山西省",
            "内蒙古自治区",
            "辽宁省",
            "吉林省"});
            this.comboBoxProvince.Location = new System.Drawing.Point(111, 175);
            this.comboBoxProvince.Name = "comboBoxProvince";
            this.comboBoxProvince.Size = new System.Drawing.Size(121, 20);
            this.comboBoxProvince.TabIndex = 10;
            this.comboBoxProvince.Text = "不限";
            this.comboBoxProvince.Visible = false;
            // 
            // buttonSearch
            // 
            this.buttonSearch.Location = new System.Drawing.Point(70, 227);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(42, 23);
            this.buttonSearch.TabIndex = 2;
            this.buttonSearch.Text = "查询";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // labelProvince
            // 
            this.labelProvince.AutoSize = true;
            this.labelProvince.Location = new System.Drawing.Point(4, 178);
            this.labelProvince.Name = "labelProvince";
            this.labelProvince.Size = new System.Drawing.Size(53, 12);
            this.labelProvince.TabIndex = 9;
            this.labelProvince.Text = "province";
            this.labelProvince.Visible = false;
            // 
            // textBoxCertificateID
            // 
            this.textBoxCertificateID.Location = new System.Drawing.Point(109, 91);
            this.textBoxCertificateID.Name = "textBoxCertificateID";
            this.textBoxCertificateID.Size = new System.Drawing.Size(121, 21);
            this.textBoxCertificateID.TabIndex = 8;
            // 
            // comboBoxCertificateType
            // 
            this.comboBoxCertificateType.FormattingEnabled = true;
            this.comboBoxCertificateType.Items.AddRange(new object[] {
            "",
            "ID",
            "officerID",
            "passportID",
            "other"});
            this.comboBoxCertificateType.Location = new System.Drawing.Point(109, 65);
            this.comboBoxCertificateType.Name = "comboBoxCertificateType";
            this.comboBoxCertificateType.Size = new System.Drawing.Size(121, 20);
            this.comboBoxCertificateType.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "certificateNumber";
            // 
            // labelCertificateType
            // 
            this.labelCertificateType.AutoSize = true;
            this.labelCertificateType.Location = new System.Drawing.Point(2, 68);
            this.labelCertificateType.Name = "labelCertificateType";
            this.labelCertificateType.Size = new System.Drawing.Size(95, 12);
            this.labelCertificateType.TabIndex = 5;
            this.labelCertificateType.Text = "certificateType";
            // 
            // comboBoxGender
            // 
            this.comboBoxGender.FormattingEnabled = true;
            this.comboBoxGender.Items.AddRange(new object[] {
            "",
            "male",
            "female"});
            this.comboBoxGender.Location = new System.Drawing.Point(109, 39);
            this.comboBoxGender.Name = "comboBoxGender";
            this.comboBoxGender.Size = new System.Drawing.Size(121, 20);
            this.comboBoxGender.TabIndex = 4;
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(109, 12);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(121, 21);
            this.textBoxName.TabIndex = 3;
            // 
            // labelGender
            // 
            this.labelGender.AutoSize = true;
            this.labelGender.Location = new System.Drawing.Point(1, 42);
            this.labelGender.Name = "labelGender";
            this.labelGender.Size = new System.Drawing.Size(23, 12);
            this.labelGender.TabIndex = 1;
            this.labelGender.Text = "sex";
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(2, 15);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(29, 12);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "name";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.listView_FD);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(243, 249);
            this.panel2.TabIndex = 4;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.groupBox2);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer4);
            this.splitContainer3.Size = new System.Drawing.Size(799, 578);
            this.splitContainer3.SplitterDistance = 55;
            this.splitContainer3.TabIndex = 6;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.m_flowLayoutPanelPictureData);
            this.splitContainer4.Size = new System.Drawing.Size(799, 519);
            this.splitContainer4.SplitterDistance = 468;
            this.splitContainer4.TabIndex = 6;
            // 
            // m_flowLayoutPanelPictureData
            // 
            this.m_flowLayoutPanelPictureData.AutoScroll = true;
            this.m_flowLayoutPanelPictureData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_flowLayoutPanelPictureData.Location = new System.Drawing.Point(0, 0);
            this.m_flowLayoutPanelPictureData.Name = "m_flowLayoutPanelPictureData";
            this.m_flowLayoutPanelPictureData.Size = new System.Drawing.Size(799, 468);
            this.m_flowLayoutPanelPictureData.TabIndex = 0;
            // 
            // BlockFDForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1046, 578);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "BlockFDForm";
            this.Text = "BlockFDForm";
            this.Layout += new System.Windows.Forms.LayoutEventHandler(this.BlockFDForm_Layout);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btn_delete;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button m_btnAddFD;
        private System.Windows.Forms.Button m_btnModifyFD;
        private System.Windows.Forms.ListView listView_FD;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button m_buttonDeleteFace;
        private System.Windows.Forms.Button m_buttonModifyFace;
        private System.Windows.Forms.Button m_buttonAddFace;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.FlowLayoutPanel m_flowLayoutPanelPictureData;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DateTimePicker dateTimePickerEndTime;
        private System.Windows.Forms.DateTimePicker dateTimePickerStartTime;
        private System.Windows.Forms.Label labelEndBirth;
        private System.Windows.Forms.Label labelStartBirth;
        private System.Windows.Forms.ComboBox comboBoxCity;
        private System.Windows.Forms.Label labelCity;
        private System.Windows.Forms.ComboBox comboBoxProvince;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.Label labelProvince;
        private System.Windows.Forms.TextBox textBoxCertificateID;
        private System.Windows.Forms.ComboBox comboBoxCertificateType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelCertificateType;
        private System.Windows.Forms.ComboBox comboBoxGender;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label labelGender;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
    }
}