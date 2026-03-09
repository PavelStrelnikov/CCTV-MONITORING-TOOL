namespace SDKANPR
{
    partial class ANPRAlarmForm
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
            this.UNGuardBtn = new System.Windows.Forms.Button();
            this.GurdBtn = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ANPRAlarmInfoListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ANPRPicBox = new System.Windows.Forms.PictureBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.ITSPlateInfoTextBox = new System.Windows.Forms.TextBox();
            this.ANPRDPicBox = new System.Windows.Forms.PictureBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ANPRPicBox)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ANPRDPicBox)).BeginInit();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.UNGuardBtn);
            this.groupBox1.Controls.Add(this.GurdBtn);
            this.groupBox1.Location = new System.Drawing.Point(2, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(949, 75);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "报警";
            // 
            // UNGuardBtn
            // 
            this.UNGuardBtn.Location = new System.Drawing.Point(202, 33);
            this.UNGuardBtn.Name = "UNGuardBtn";
            this.UNGuardBtn.Size = new System.Drawing.Size(75, 23);
            this.UNGuardBtn.TabIndex = 1;
            this.UNGuardBtn.Text = "撤防";
            this.UNGuardBtn.UseVisualStyleBackColor = true;
            this.UNGuardBtn.Click += new System.EventHandler(this.UNGuardBtn_Click);
            // 
            // GurdBtn
            // 
            this.GurdBtn.Location = new System.Drawing.Point(100, 33);
            this.GurdBtn.Name = "GurdBtn";
            this.GurdBtn.Size = new System.Drawing.Size(75, 23);
            this.GurdBtn.TabIndex = 0;
            this.GurdBtn.Text = "布防";
            this.GurdBtn.UseVisualStyleBackColor = true;
            this.GurdBtn.Click += new System.EventHandler(this.GurdBtn_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.ANPRAlarmInfoListView);
            this.groupBox2.Location = new System.Drawing.Point(0, 406);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(952, 209);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "ANPR报警信息列表";
            // 
            // ANPRAlarmInfoListView
            // 
            this.ANPRAlarmInfoListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader7,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader12,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10});
            this.ANPRAlarmInfoListView.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ANPRAlarmInfoListView.FullRowSelect = true;
            this.ANPRAlarmInfoListView.GridLines = true;
            this.ANPRAlarmInfoListView.Location = new System.Drawing.Point(3, 18);
            this.ANPRAlarmInfoListView.Margin = new System.Windows.Forms.Padding(0);
            this.ANPRAlarmInfoListView.Name = "ANPRAlarmInfoListView";
            this.ANPRAlarmInfoListView.Size = new System.Drawing.Size(946, 188);
            this.ANPRAlarmInfoListView.TabIndex = 7;
            this.ANPRAlarmInfoListView.UseCompatibleStateImageBehavior = false;
            this.ANPRAlarmInfoListView.View = System.Windows.Forms.View.Details;
            this.ANPRAlarmInfoListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ANPRAlarmInfoListView_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "报警IP";
            this.columnHeader1.Width = 100;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "报警触发时间";
            this.columnHeader2.Width = 112;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "车辆类型";
            this.columnHeader7.Width = 100;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "地区";
            this.columnHeader3.Width = 59;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "检测场景号";
            this.columnHeader4.Width = 76;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "车牌号码";
            this.columnHeader5.Width = 96;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "识别车道号";
            this.columnHeader6.Width = 84;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "检测方向";
            this.columnHeader12.Width = 115;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "详细信息";
            this.columnHeader8.Width = 66;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "车牌图片路径";
            this.columnHeader9.Width = 132;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "检测图片路径";
            this.columnHeader10.Width = 132;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox3.Controls.Add(this.ANPRPicBox);
            this.groupBox3.Location = new System.Drawing.Point(0, 86);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(479, 97);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "ANPR车牌报警图片";
            // 
            // ANPRPicBox
            // 
            this.ANPRPicBox.Location = new System.Drawing.Point(12, 20);
            this.ANPRPicBox.Name = "ANPRPicBox";
            this.ANPRPicBox.Size = new System.Drawing.Size(461, 67);
            this.ANPRPicBox.TabIndex = 0;
            this.ANPRPicBox.TabStop = false;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.ITSPlateInfoTextBox);
            this.groupBox4.Location = new System.Drawing.Point(506, 86);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(435, 314);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "ANPR报警详细信息";
            // 
            // ITSPlateInfoTextBox
            // 
            this.ITSPlateInfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ITSPlateInfoTextBox.Location = new System.Drawing.Point(6, 20);
            this.ITSPlateInfoTextBox.Multiline = true;
            this.ITSPlateInfoTextBox.Name = "ITSPlateInfoTextBox";
            this.ITSPlateInfoTextBox.Size = new System.Drawing.Size(423, 288);
            this.ITSPlateInfoTextBox.TabIndex = 0;
            // 
            // ANPRDPicBox
            // 
            this.ANPRDPicBox.Location = new System.Drawing.Point(12, 20);
            this.ANPRDPicBox.Name = "ANPRDPicBox";
            this.ANPRDPicBox.Size = new System.Drawing.Size(461, 179);
            this.ANPRDPicBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ANPRDPicBox.TabIndex = 0;
            this.ANPRDPicBox.TabStop = false;
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox5.Controls.Add(this.ANPRDPicBox);
            this.groupBox5.Location = new System.Drawing.Point(6, 189);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(479, 205);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "ANPR车辆检测报警图片";
            // 
            // ANPRAlarmForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(953, 626);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ANPRAlarmForm";
            this.Text = "LicensePlateAuditForm";
            this.Load += new System.EventHandler(this.ANPRAlarmForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ANPRPicBox)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ANPRDPicBox)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button UNGuardBtn;
        private System.Windows.Forms.Button GurdBtn;
        private System.Windows.Forms.ListView ANPRAlarmInfoListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.TextBox ITSPlateInfoTextBox;
        private System.Windows.Forms.PictureBox ANPRPicBox;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.PictureBox ANPRDPicBox;
        private System.Windows.Forms.GroupBox groupBox5;
    }
}