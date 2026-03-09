namespace SDKANPR
{
    partial class ManualSnapForm
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.SnapPlateInfoListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label6 = new System.Windows.Forms.Label();
            this.m_textBoxLaneNo = new System.Windows.Forms.TextBox();
            this.OSDEnableCheckBox = new System.Windows.Forms.CheckBox();
            this.SnapBtn = new System.Windows.Forms.Button();
            this.m_panel = new System.Windows.Forms.Panel();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.platePictureBox = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.CloseUppictureBox = new System.Windows.Forms.PictureBox();
            this.groupBox2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.platePictureBox)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CloseUppictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.SnapPlateInfoListView);
            this.groupBox2.Location = new System.Drawing.Point(2, 361);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1115, 253);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "PlateInfo";
            // 
            // SnapPlateInfoListView
            // 
            this.SnapPlateInfoListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader11,
            this.columnHeader2,
            this.columnHeader7,
            this.columnHeader3,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader12,
            this.columnHeader8,
            this.columnHeader9});
            this.SnapPlateInfoListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SnapPlateInfoListView.FullRowSelect = true;
            this.SnapPlateInfoListView.GridLines = true;
            this.SnapPlateInfoListView.Location = new System.Drawing.Point(3, 17);
            this.SnapPlateInfoListView.Margin = new System.Windows.Forms.Padding(0);
            this.SnapPlateInfoListView.Name = "SnapPlateInfoListView";
            this.SnapPlateInfoListView.Size = new System.Drawing.Size(1109, 233);
            this.SnapPlateInfoListView.TabIndex = 7;
            this.SnapPlateInfoListView.UseCompatibleStateImageBehavior = false;
            this.SnapPlateInfoListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Index";
            this.columnHeader1.Width = 50;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Time";
            this.columnHeader11.Width = 100;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "PlateColor";
            this.columnHeader2.Width = 90;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "PlateType";
            this.columnHeader7.Width = 100;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "PlateLicense";
            this.columnHeader3.Width = 99;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "VehicleType";
            this.columnHeader5.Width = 96;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "VehicleColor";
            this.columnHeader6.Width = 115;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "ChanIndex";
            this.columnHeader12.Width = 82;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Speed";
            this.columnHeader8.Width = 125;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "PicPath";
            this.columnHeader9.Width = 303;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(577, 30);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 12);
            this.label6.TabIndex = 10;
            this.label6.Text = "Lane No：";
            // 
            // m_textBoxLaneNo
            // 
            this.m_textBoxLaneNo.Location = new System.Drawing.Point(634, 27);
            this.m_textBoxLaneNo.Name = "m_textBoxLaneNo";
            this.m_textBoxLaneNo.Size = new System.Drawing.Size(87, 21);
            this.m_textBoxLaneNo.TabIndex = 9;
            this.m_textBoxLaneNo.Text = "1";
            // 
            // OSDEnableCheckBox
            // 
            this.OSDEnableCheckBox.AutoSize = true;
            this.OSDEnableCheckBox.Location = new System.Drawing.Point(761, 29);
            this.OSDEnableCheckBox.Name = "OSDEnableCheckBox";
            this.OSDEnableCheckBox.Size = new System.Drawing.Size(90, 16);
            this.OSDEnableCheckBox.TabIndex = 8;
            this.OSDEnableCheckBox.Text = "OSD Enable ";
            this.OSDEnableCheckBox.UseVisualStyleBackColor = true;
            // 
            // SnapBtn
            // 
            this.SnapBtn.Location = new System.Drawing.Point(898, 29);
            this.SnapBtn.Name = "SnapBtn";
            this.SnapBtn.Size = new System.Drawing.Size(87, 23);
            this.SnapBtn.TabIndex = 11;
            this.SnapBtn.Text = "ManualSnap";
            this.SnapBtn.UseVisualStyleBackColor = true;
            this.SnapBtn.Click += new System.EventHandler(this.SnapBtn_Click);
            // 
            // m_panel
            // 
            this.m_panel.BackColor = System.Drawing.Color.White;
            this.m_panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_panel.Location = new System.Drawing.Point(5, 6);
            this.m_panel.Name = "m_panel";
            this.m_panel.Size = new System.Drawing.Size(464, 349);
            this.m_panel.TabIndex = 15;
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox5.Controls.Add(this.platePictureBox);
            this.groupBox5.Location = new System.Drawing.Point(813, 94);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(293, 220);
            this.groupBox5.TabIndex = 18;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Plate Pic";
            // 
            // platePictureBox
            // 
            this.platePictureBox.Location = new System.Drawing.Point(10, 20);
            this.platePictureBox.Name = "platePictureBox";
            this.platePictureBox.Size = new System.Drawing.Size(274, 176);
            this.platePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.platePictureBox.TabIndex = 0;
            this.platePictureBox.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.CloseUppictureBox);
            this.groupBox1.Location = new System.Drawing.Point(489, 74);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(318, 240);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Background Pic";
            // 
            // CloseUppictureBox
            // 
            this.CloseUppictureBox.Location = new System.Drawing.Point(6, 20);
            this.CloseUppictureBox.Name = "CloseUppictureBox";
            this.CloseUppictureBox.Size = new System.Drawing.Size(307, 220);
            this.CloseUppictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.CloseUppictureBox.TabIndex = 0;
            this.CloseUppictureBox.TabStop = false;
            // 
            // ManualSnapForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1118, 626);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.m_panel);
            this.Controls.Add(this.SnapBtn);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.m_textBoxLaneNo);
            this.Controls.Add(this.OSDEnableCheckBox);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ManualSnapForm";
            this.Text = "ManualSnapForm";
            this.groupBox2.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.platePictureBox)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CloseUppictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListView SnapPlateInfoListView;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox m_textBoxLaneNo;
        private System.Windows.Forms.CheckBox OSDEnableCheckBox;
        private System.Windows.Forms.Button SnapBtn;
        private System.Windows.Forms.Panel m_panel;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.PictureBox platePictureBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.PictureBox CloseUppictureBox;
    }
}