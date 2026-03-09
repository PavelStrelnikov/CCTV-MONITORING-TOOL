namespace SDKSystemManagement
{
    partial class FormPreview
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPreview));
            this.m_btnStop = new System.Windows.Forms.Button();
            this.m_btnPlay = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.m_panelOne = new System.Windows.Forms.Panel();
            this.m_panelThree = new System.Windows.Forms.Panel();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.m_panelTwo = new System.Windows.Forms.Panel();
            this.m_panelFour = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ConvertCallBackradioBtn = new System.Windows.Forms.RadioButton();
            this.StandardCallBackradioBtn = new System.Windows.Forms.RadioButton();
            this.DecodeInRealPlayrBtn = new System.Windows.Forms.RadioButton();
            this.splitContainerPreviewAndPTZ = new System.Windows.Forms.SplitContainer();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.m_comboBoxProtoType = new System.Windows.Forms.ComboBox();
            this.m_comboBoxLinkMode = new System.Windows.Forms.ComboBox();
            this.m_comboBoxStreamType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxShowPTZPanel = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.listViewPresets = new System.Windows.Forms.ListView();
            this.columnHeaderID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripPresets = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gotoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonFocus = new System.Windows.Forms.Button();
            this.textBoxFocus = new System.Windows.Forms.TextBox();
            this.buttonDown = new System.Windows.Forms.Button();
            this.buttonRight = new System.Windows.Forms.Button();
            this.buttonLeft = new System.Windows.Forms.Button();
            this.buttonUp = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPreviewAndPTZ)).BeginInit();
            this.splitContainerPreviewAndPTZ.Panel1.SuspendLayout();
            this.splitContainerPreviewAndPTZ.Panel2.SuspendLayout();
            this.splitContainerPreviewAndPTZ.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.contextMenuStripPresets.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_btnStop
            // 
            this.m_btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnStop.BackColor = System.Drawing.Color.SkyBlue;
            this.m_btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_btnStop.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.m_btnStop.ForeColor = System.Drawing.Color.White;
            this.m_btnStop.Location = new System.Drawing.Point(681, 540);
            this.m_btnStop.Name = "m_btnStop";
            this.m_btnStop.Size = new System.Drawing.Size(105, 27);
            this.m_btnStop.TabIndex = 11;
            this.m_btnStop.Text = "Stop";
            this.m_btnStop.UseVisualStyleBackColor = false;
            this.m_btnStop.Click += new System.EventHandler(this.m_btnStop_Click);
            // 
            // m_btnPlay
            // 
            this.m_btnPlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnPlay.BackColor = System.Drawing.Color.SkyBlue;
            this.m_btnPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_btnPlay.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.m_btnPlay.ForeColor = System.Drawing.Color.White;
            this.m_btnPlay.Location = new System.Drawing.Point(681, 507);
            this.m_btnPlay.Name = "m_btnPlay";
            this.m_btnPlay.Size = new System.Drawing.Size(105, 27);
            this.m_btnPlay.TabIndex = 10;
            this.m_btnPlay.Text = "Play";
            this.m_btnPlay.UseVisualStyleBackColor = false;
            this.m_btnPlay.Click += new System.EventHandler(this.m_btnPlay_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.splitContainer1);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(786, 472);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Select one channel on the DeviceTree and click the button Play to Preview";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 19);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(780, 450);
            this.splitContainer1.SplitterDistance = 389;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 0;
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
            this.splitContainer3.Panel1.Controls.Add(this.m_panelOne);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.m_panelThree);
            this.splitContainer3.Size = new System.Drawing.Size(389, 450);
            this.splitContainer3.SplitterDistance = 221;
            this.splitContainer3.SplitterWidth = 5;
            this.splitContainer3.TabIndex = 0;
            // 
            // m_panelOne
            // 
            this.m_panelOne.BackColor = System.Drawing.Color.White;
            this.m_panelOne.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_panelOne.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_panelOne.Location = new System.Drawing.Point(0, 0);
            this.m_panelOne.Name = "m_panelOne";
            this.m_panelOne.Size = new System.Drawing.Size(389, 221);
            this.m_panelOne.TabIndex = 14;
            this.m_panelOne.Paint += new System.Windows.Forms.PaintEventHandler(this.m_panelOne_Paint);
            this.m_panelOne.MouseDown += new System.Windows.Forms.MouseEventHandler(this.m_panelOne_MouseDown);
            // 
            // m_panelThree
            // 
            this.m_panelThree.BackColor = System.Drawing.Color.White;
            this.m_panelThree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_panelThree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_panelThree.Location = new System.Drawing.Point(0, 0);
            this.m_panelThree.Name = "m_panelThree";
            this.m_panelThree.Size = new System.Drawing.Size(389, 224);
            this.m_panelThree.TabIndex = 16;
            this.m_panelThree.Paint += new System.Windows.Forms.PaintEventHandler(this.m_panelThree_Paint);
            this.m_panelThree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.m_panelThree_MouseDown);
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
            this.splitContainer2.Panel1.Controls.Add(this.m_panelTwo);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.m_panelFour);
            this.splitContainer2.Size = new System.Drawing.Size(386, 450);
            this.splitContainer2.SplitterDistance = 221;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 0;
            // 
            // m_panelTwo
            // 
            this.m_panelTwo.BackColor = System.Drawing.Color.White;
            this.m_panelTwo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_panelTwo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_panelTwo.Location = new System.Drawing.Point(0, 0);
            this.m_panelTwo.Name = "m_panelTwo";
            this.m_panelTwo.Size = new System.Drawing.Size(386, 221);
            this.m_panelTwo.TabIndex = 15;
            this.m_panelTwo.Paint += new System.Windows.Forms.PaintEventHandler(this.m_panelTwo_Paint);
            this.m_panelTwo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.m_panelTwo_MouseDown);
            // 
            // m_panelFour
            // 
            this.m_panelFour.BackColor = System.Drawing.Color.White;
            this.m_panelFour.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_panelFour.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_panelFour.Location = new System.Drawing.Point(0, 0);
            this.m_panelFour.Name = "m_panelFour";
            this.m_panelFour.Size = new System.Drawing.Size(386, 224);
            this.m_panelFour.TabIndex = 17;
            this.m_panelFour.Paint += new System.Windows.Forms.PaintEventHandler(this.m_panelFour_Paint);
            this.m_panelFour.MouseDown += new System.Windows.Forms.MouseEventHandler(this.m_panelFour_MouseDown);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.ConvertCallBackradioBtn);
            this.groupBox2.Controls.Add(this.StandardCallBackradioBtn);
            this.groupBox2.Controls.Add(this.DecodeInRealPlayrBtn);
            this.groupBox2.Location = new System.Drawing.Point(6, 481);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(281, 92);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Please select one decode type";
            // 
            // ConvertCallBackradioBtn
            // 
            this.ConvertCallBackradioBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ConvertCallBackradioBtn.AutoSize = true;
            this.ConvertCallBackradioBtn.Location = new System.Drawing.Point(15, 68);
            this.ConvertCallBackradioBtn.Name = "ConvertCallBackradioBtn";
            this.ConvertCallBackradioBtn.Size = new System.Drawing.Size(249, 18);
            this.ConvertCallBackradioBtn.TabIndex = 2;
            this.ConvertCallBackradioBtn.Text = "DecodeInCallBack-ConvertCallBack";
            this.ConvertCallBackradioBtn.UseVisualStyleBackColor = true;
            // 
            // StandardCallBackradioBtn
            // 
            this.StandardCallBackradioBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.StandardCallBackradioBtn.AutoSize = true;
            this.StandardCallBackradioBtn.Location = new System.Drawing.Point(15, 46);
            this.StandardCallBackradioBtn.Name = "StandardCallBackradioBtn";
            this.StandardCallBackradioBtn.Size = new System.Drawing.Size(256, 18);
            this.StandardCallBackradioBtn.TabIndex = 1;
            this.StandardCallBackradioBtn.Text = "DecodeInCallBack-StandardCallBack";
            this.StandardCallBackradioBtn.UseVisualStyleBackColor = true;
            // 
            // DecodeInRealPlayrBtn
            // 
            this.DecodeInRealPlayrBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DecodeInRealPlayrBtn.AutoSize = true;
            this.DecodeInRealPlayrBtn.Checked = true;
            this.DecodeInRealPlayrBtn.Location = new System.Drawing.Point(15, 22);
            this.DecodeInRealPlayrBtn.Name = "DecodeInRealPlayrBtn";
            this.DecodeInRealPlayrBtn.Size = new System.Drawing.Size(137, 18);
            this.DecodeInRealPlayrBtn.TabIndex = 0;
            this.DecodeInRealPlayrBtn.TabStop = true;
            this.DecodeInRealPlayrBtn.Text = "DecodeInRealPlay";
            this.DecodeInRealPlayrBtn.UseVisualStyleBackColor = true;
            // 
            // splitContainerPreviewAndPTZ
            // 
            this.splitContainerPreviewAndPTZ.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerPreviewAndPTZ.Location = new System.Drawing.Point(0, 0);
            this.splitContainerPreviewAndPTZ.Name = "splitContainerPreviewAndPTZ";
            // 
            // splitContainerPreviewAndPTZ.Panel1
            // 
            this.splitContainerPreviewAndPTZ.Panel1.Controls.Add(this.groupBox5);
            this.splitContainerPreviewAndPTZ.Panel1.Controls.Add(this.checkBoxShowPTZPanel);
            this.splitContainerPreviewAndPTZ.Panel1.Controls.Add(this.groupBox1);
            this.splitContainerPreviewAndPTZ.Panel1.Controls.Add(this.m_btnStop);
            this.splitContainerPreviewAndPTZ.Panel1.Controls.Add(this.groupBox2);
            this.splitContainerPreviewAndPTZ.Panel1.Controls.Add(this.m_btnPlay);
            // 
            // splitContainerPreviewAndPTZ.Panel2
            // 
            this.splitContainerPreviewAndPTZ.Panel2.Controls.Add(this.groupBox3);
            this.splitContainerPreviewAndPTZ.Size = new System.Drawing.Size(1046, 576);
            this.splitContainerPreviewAndPTZ.SplitterDistance = 792;
            this.splitContainerPreviewAndPTZ.TabIndex = 14;
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox5.Controls.Add(this.m_comboBoxProtoType);
            this.groupBox5.Controls.Add(this.m_comboBoxLinkMode);
            this.groupBox5.Controls.Add(this.m_comboBoxStreamType);
            this.groupBox5.Controls.Add(this.label3);
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.label1);
            this.groupBox5.Location = new System.Drawing.Point(293, 481);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(260, 92);
            this.groupBox5.TabIndex = 15;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Channel Info";
            this.groupBox5.Visible = false;
            // 
            // m_comboBoxProtoType
            // 
            this.m_comboBoxProtoType.FormattingEnabled = true;
            this.m_comboBoxProtoType.Items.AddRange(new object[] {
            "Private",
            "RTSP"});
            this.m_comboBoxProtoType.Location = new System.Drawing.Point(89, 67);
            this.m_comboBoxProtoType.Name = "m_comboBoxProtoType";
            this.m_comboBoxProtoType.Size = new System.Drawing.Size(121, 22);
            this.m_comboBoxProtoType.TabIndex = 5;
            // 
            // m_comboBoxLinkMode
            // 
            this.m_comboBoxLinkMode.FormattingEnabled = true;
            this.m_comboBoxLinkMode.Items.AddRange(new object[] {
            "TCP",
            "UDP",
            "RTP/HTTPS"});
            this.m_comboBoxLinkMode.Location = new System.Drawing.Point(89, 45);
            this.m_comboBoxLinkMode.Name = "m_comboBoxLinkMode";
            this.m_comboBoxLinkMode.Size = new System.Drawing.Size(121, 22);
            this.m_comboBoxLinkMode.TabIndex = 4;
            // 
            // m_comboBoxStreamType
            // 
            this.m_comboBoxStreamType.FormattingEnabled = true;
            this.m_comboBoxStreamType.Items.AddRange(new object[] {
            "Main Stream",
            "Sub stream"});
            this.m_comboBoxStreamType.Location = new System.Drawing.Point(89, 21);
            this.m_comboBoxStreamType.Name = "m_comboBoxStreamType";
            this.m_comboBoxStreamType.Size = new System.Drawing.Size(121, 22);
            this.m_comboBoxStreamType.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 14);
            this.label3.TabIndex = 2;
            this.label3.Text = "Proto Type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 14);
            this.label2.TabIndex = 1;
            this.label2.Text = "Link Mode";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(48, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 14);
            this.label1.TabIndex = 0;
            this.label1.Text = "Type";
            // 
            // checkBoxShowPTZPanel
            // 
            this.checkBoxShowPTZPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxShowPTZPanel.AutoSize = true;
            this.checkBoxShowPTZPanel.Location = new System.Drawing.Point(662, 481);
            this.checkBoxShowPTZPanel.Name = "checkBoxShowPTZPanel";
            this.checkBoxShowPTZPanel.Size = new System.Drawing.Size(124, 18);
            this.checkBoxShowPTZPanel.TabIndex = 14;
            this.checkBoxShowPTZPanel.Text = "Show PTZ Panel";
            this.checkBoxShowPTZPanel.UseVisualStyleBackColor = true;
            this.checkBoxShowPTZPanel.CheckedChanged += new System.EventHandler(this.checkBoxShowPTZPanel_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Controls.Add(this.panel1);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(250, 576);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "PTZ";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.listViewPresets);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(3, 170);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(244, 403);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Presets";
            // 
            // listViewPresets
            // 
            this.listViewPresets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderID,
            this.columnHeaderName});
            this.listViewPresets.ContextMenuStrip = this.contextMenuStripPresets;
            this.listViewPresets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewPresets.FullRowSelect = true;
            this.listViewPresets.GridLines = true;
            this.listViewPresets.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewPresets.LabelWrap = false;
            this.listViewPresets.Location = new System.Drawing.Point(3, 19);
            this.listViewPresets.MultiSelect = false;
            this.listViewPresets.Name = "listViewPresets";
            this.listViewPresets.Size = new System.Drawing.Size(238, 381);
            this.listViewPresets.TabIndex = 0;
            this.listViewPresets.UseCompatibleStateImageBehavior = false;
            this.listViewPresets.View = System.Windows.Forms.View.Details;
            this.listViewPresets.SizeChanged += new System.EventHandler(this.listViewPresets_SizeChanged);
            this.listViewPresets.DoubleClick += new System.EventHandler(this.listViewPresets_DoubleClick);
            // 
            // columnHeaderID
            // 
            this.columnHeaderID.Text = "ID";
            this.columnHeaderID.Width = 40;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            // 
            // contextMenuStripPresets
            // 
            this.contextMenuStripPresets.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.removeToolStripMenuItem,
            this.gotoToolStripMenuItem});
            this.contextMenuStripPresets.Name = "contextMenuStripPresets";
            this.contextMenuStripPresets.Size = new System.Drawing.Size(124, 70);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // gotoToolStripMenuItem
            // 
            this.gotoToolStripMenuItem.Name = "gotoToolStripMenuItem";
            this.gotoToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.gotoToolStripMenuItem.Text = "Goto";
            this.gotoToolStripMenuItem.Click += new System.EventHandler(this.gotoToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonFocus);
            this.panel1.Controls.Add(this.textBoxFocus);
            this.panel1.Controls.Add(this.buttonDown);
            this.panel1.Controls.Add(this.buttonRight);
            this.panel1.Controls.Add(this.buttonLeft);
            this.panel1.Controls.Add(this.buttonUp);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 19);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(244, 151);
            this.panel1.TabIndex = 0;
            // 
            // buttonFocus
            // 
            this.buttonFocus.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonFocus.Location = new System.Drawing.Point(109, 112);
            this.buttonFocus.Name = "buttonFocus";
            this.buttonFocus.Size = new System.Drawing.Size(126, 23);
            this.buttonFocus.TabIndex = 11;
            this.buttonFocus.Text = "Focus(-100~100)";
            this.buttonFocus.UseVisualStyleBackColor = true;
            this.buttonFocus.Click += new System.EventHandler(this.buttonFocus_Click);
            // 
            // textBoxFocus
            // 
            this.textBoxFocus.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.textBoxFocus.Location = new System.Drawing.Point(12, 112);
            this.textBoxFocus.Name = "textBoxFocus";
            this.textBoxFocus.Size = new System.Drawing.Size(75, 23);
            this.textBoxFocus.TabIndex = 10;
            this.textBoxFocus.Text = "0";
            this.textBoxFocus.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // buttonDown
            // 
            this.buttonDown.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonDown.Location = new System.Drawing.Point(93, 78);
            this.buttonDown.Name = "buttonDown";
            this.buttonDown.Size = new System.Drawing.Size(54, 28);
            this.buttonDown.TabIndex = 8;
            this.buttonDown.Text = "Down";
            this.buttonDown.UseVisualStyleBackColor = true;
            this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
            // 
            // buttonRight
            // 
            this.buttonRight.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonRight.Location = new System.Drawing.Point(153, 44);
            this.buttonRight.Name = "buttonRight";
            this.buttonRight.Size = new System.Drawing.Size(54, 28);
            this.buttonRight.TabIndex = 7;
            this.buttonRight.Text = "Right";
            this.buttonRight.UseVisualStyleBackColor = true;
            this.buttonRight.Click += new System.EventHandler(this.buttonRight_Click);
            // 
            // buttonLeft
            // 
            this.buttonLeft.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonLeft.Location = new System.Drawing.Point(33, 44);
            this.buttonLeft.Name = "buttonLeft";
            this.buttonLeft.Size = new System.Drawing.Size(54, 28);
            this.buttonLeft.TabIndex = 6;
            this.buttonLeft.Text = "Left";
            this.buttonLeft.UseVisualStyleBackColor = true;
            this.buttonLeft.Click += new System.EventHandler(this.buttonLeft_Click);
            // 
            // buttonUp
            // 
            this.buttonUp.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonUp.Location = new System.Drawing.Point(93, 9);
            this.buttonUp.Name = "buttonUp";
            this.buttonUp.Size = new System.Drawing.Size(54, 28);
            this.buttonUp.TabIndex = 5;
            this.buttonUp.Text = "Up";
            this.buttonUp.UseVisualStyleBackColor = true;
            this.buttonUp.Click += new System.EventHandler(this.buttonUp_Click);
            // 
            // FormPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1046, 576);
            this.Controls.Add(this.splitContainerPreviewAndPTZ);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormPreview";
            this.Text = "Preview";
            this.Load += new System.EventHandler(this.FormPreview_Load);
            this.groupBox1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.splitContainerPreviewAndPTZ.Panel1.ResumeLayout(false);
            this.splitContainerPreviewAndPTZ.Panel1.PerformLayout();
            this.splitContainerPreviewAndPTZ.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPreviewAndPTZ)).EndInit();
            this.splitContainerPreviewAndPTZ.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.contextMenuStripPresets.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button m_btnStop;
        private System.Windows.Forms.Button m_btnPlay;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Panel m_panelOne;
        private System.Windows.Forms.Panel m_panelThree;
        private System.Windows.Forms.Panel m_panelTwo;
        private System.Windows.Forms.Panel m_panelFour;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton DecodeInRealPlayrBtn;
        private System.Windows.Forms.RadioButton ConvertCallBackradioBtn;
        private System.Windows.Forms.RadioButton StandardCallBackradioBtn;
        private System.Windows.Forms.SplitContainer splitContainerPreviewAndPTZ;
        private System.Windows.Forms.CheckBox checkBoxShowPTZPanel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView listViewPresets;
        private System.Windows.Forms.ColumnHeader columnHeaderID;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripPresets;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gotoToolStripMenuItem;
        private System.Windows.Forms.Button buttonDown;
        private System.Windows.Forms.Button buttonRight;
        private System.Windows.Forms.Button buttonLeft;
        private System.Windows.Forms.Button buttonUp;
        private System.Windows.Forms.Button buttonFocus;
        private System.Windows.Forms.TextBox textBoxFocus;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.ComboBox m_comboBoxProtoType;
        private System.Windows.Forms.ComboBox m_comboBoxLinkMode;
        private System.Windows.Forms.ComboBox m_comboBoxStreamType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}