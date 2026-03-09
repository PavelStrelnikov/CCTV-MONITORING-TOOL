namespace SDKAIOpenPlatform
{
    partial class ModelManageForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModelManageForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.surplusCapacityLab = new System.Windows.Forms.Label();
            this.totalCapacityLab = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.modelUploadbutton = new System.Windows.Forms.Button();
            this.modelUploadTypecomboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ModelInfoListView = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.surplusCapacityLab);
            this.groupBox1.Controls.Add(this.totalCapacityLab);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.modelUploadbutton);
            this.groupBox1.Controls.Add(this.modelUploadTypecomboBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(1, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(949, 75);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "模型库管理";
            // 
            // surplusCapacityLab
            // 
            this.surplusCapacityLab.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.surplusCapacityLab.AutoSize = true;
            this.surplusCapacityLab.Location = new System.Drawing.Point(824, 30);
            this.surplusCapacityLab.Name = "surplusCapacityLab";
            this.surplusCapacityLab.Size = new System.Drawing.Size(53, 12);
            this.surplusCapacityLab.TabIndex = 6;
            this.surplusCapacityLab.Text = "剩余容量";
            // 
            // totalCapacityLab
            // 
            this.totalCapacityLab.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.totalCapacityLab.AutoSize = true;
            this.totalCapacityLab.Location = new System.Drawing.Point(740, 30);
            this.totalCapacityLab.Name = "totalCapacityLab";
            this.totalCapacityLab.Size = new System.Drawing.Size(41, 12);
            this.totalCapacityLab.TabIndex = 5;
            this.totalCapacityLab.Text = "总容量";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(617, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "设备模型包存储空间：";
            // 
            // modelUploadbutton
            // 
            this.modelUploadbutton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("modelUploadbutton.BackgroundImage")));
            this.modelUploadbutton.ForeColor = System.Drawing.Color.White;
            this.modelUploadbutton.Location = new System.Drawing.Point(217, 24);
            this.modelUploadbutton.Name = "modelUploadbutton";
            this.modelUploadbutton.Size = new System.Drawing.Size(94, 23);
            this.modelUploadbutton.TabIndex = 2;
            this.modelUploadbutton.Text = "预设";
            this.modelUploadbutton.UseVisualStyleBackColor = true;
            this.modelUploadbutton.Click += new System.EventHandler(this.modelUploadbutton_Click);
            // 
            // modelUploadTypecomboBox
            // 
            this.modelUploadTypecomboBox.FormattingEnabled = true;
            this.modelUploadTypecomboBox.Items.AddRange(new object[] {
            "推送模式"});
            this.modelUploadTypecomboBox.Location = new System.Drawing.Point(79, 27);
            this.modelUploadTypecomboBox.Name = "modelUploadTypecomboBox";
            this.modelUploadTypecomboBox.Size = new System.Drawing.Size(121, 20);
            this.modelUploadTypecomboBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "预设模式";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.ModelInfoListView);
            this.groupBox2.Location = new System.Drawing.Point(1, 97);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(952, 527);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "设备模型信息列表";
            // 
            // ModelInfoListView
            // 
            this.ModelInfoListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader8,
            this.columnHeader1});
            this.ModelInfoListView.ContextMenuStrip = this.contextMenuStrip;
            this.ModelInfoListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ModelInfoListView.FullRowSelect = true;
            this.ModelInfoListView.GridLines = true;
            this.ModelInfoListView.Location = new System.Drawing.Point(3, 17);
            this.ModelInfoListView.Margin = new System.Windows.Forms.Padding(0);
            this.ModelInfoListView.Name = "ModelInfoListView";
            this.ModelInfoListView.Size = new System.Drawing.Size(946, 507);
            this.ModelInfoListView.TabIndex = 5;
            this.ModelInfoListView.UseCompatibleStateImageBehavior = false;
            this.ModelInfoListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "模型包ID";
            this.columnHeader2.Width = 86;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "模型包名称";
            this.columnHeader3.Width = 120;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "状态信息";
            this.columnHeader4.Width = 130;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "引擎ID";
            this.columnHeader5.Width = 73;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "详细信息";
            this.columnHeader8.Width = 377;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "加载进度";
            this.columnHeader1.Width = 120;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(101, 26);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(100, 22);
            this.toolStripMenuItem1.Text = "刷新";
            // 
            // ModelManageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(953, 626);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ModelManageForm";
            this.Text = "ModelManageForm";
            this.Load += new System.EventHandler(this.ModelManageForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListView ModelInfoListView;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox modelUploadTypecomboBox;
        private System.Windows.Forms.Button modelUploadbutton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label totalCapacityLab;
        private System.Windows.Forms.Label surplusCapacityLab;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
    }
}