namespace SDKFaceLib
{
    partial class AddFaceDada
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
            this.chkCover = new System.Windows.Forms.CheckBox();
            this.txIdentityKey = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.chkConcurrent = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.m_textBoxUrl = new System.Windows.Forms.TextBox();
            this.m_Type = new System.Windows.Forms.ComboBox();
            this.m_Cancel = new System.Windows.Forms.Button();
            this.m_Sure = new System.Windows.Forms.Button();
            this.m_ChoosePic = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.m_Gender = new System.Windows.Forms.ComboBox();
            this.m_CeriNum = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.m_BornTime = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.m_City = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.m_Name = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkCover);
            this.groupBox1.Controls.Add(this.txIdentityKey);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.chkConcurrent);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.m_textBoxUrl);
            this.groupBox1.Controls.Add(this.m_Type);
            this.groupBox1.Controls.Add(this.m_Cancel);
            this.groupBox1.Controls.Add(this.m_Sure);
            this.groupBox1.Controls.Add(this.m_ChoosePic);
            this.groupBox1.Controls.Add(this.pictureBox1);
            this.groupBox1.Controls.Add(this.m_Gender);
            this.groupBox1.Controls.Add(this.m_CeriNum);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.m_BornTime);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.m_City);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.m_Name);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(541, 389);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "FaceData";
            // 
            // chkCover
            // 
            this.chkCover.AutoSize = true;
            this.chkCover.Location = new System.Drawing.Point(139, 325);
            this.chkCover.Name = "chkCover";
            this.chkCover.Size = new System.Drawing.Size(54, 16);
            this.chkCover.TabIndex = 77;
            this.chkCover.Text = "Cover";
            this.chkCover.UseVisualStyleBackColor = true;
            // 
            // txIdentityKey
            // 
            this.txIdentityKey.Location = new System.Drawing.Point(101, 277);
            this.txIdentityKey.Name = "txIdentityKey";
            this.txIdentityKey.Size = new System.Drawing.Size(111, 21);
            this.txIdentityKey.TabIndex = 76;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 281);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(71, 12);
            this.label9.TabIndex = 75;
            this.label9.Text = "IdentityKey";
            // 
            // chkConcurrent
            // 
            this.chkConcurrent.AutoSize = true;
            this.chkConcurrent.Location = new System.Drawing.Point(11, 325);
            this.chkConcurrent.Name = "chkConcurrent";
            this.chkConcurrent.Size = new System.Drawing.Size(84, 16);
            this.chkConcurrent.TabIndex = 74;
            this.chkConcurrent.Text = "Concurrent";
            this.chkConcurrent.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(245, 238);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(23, 12);
            this.label8.TabIndex = 73;
            this.label8.Text = "PID";
            // 
            // m_textBoxUrl
            // 
            this.m_textBoxUrl.Location = new System.Drawing.Point(275, 235);
            this.m_textBoxUrl.Name = "m_textBoxUrl";
            this.m_textBoxUrl.Size = new System.Drawing.Size(245, 21);
            this.m_textBoxUrl.TabIndex = 72;
            // 
            // m_Type
            // 
            this.m_Type.FormattingEnabled = true;
            this.m_Type.Items.AddRange(new object[] {
            "officerID",
            "ID",
            "passportID",
            "other"});
            this.m_Type.Location = new System.Drawing.Point(101, 196);
            this.m_Type.Name = "m_Type";
            this.m_Type.Size = new System.Drawing.Size(111, 20);
            this.m_Type.TabIndex = 71;
            // 
            // m_Cancel
            // 
            this.m_Cancel.BackColor = System.Drawing.Color.SkyBlue;
            this.m_Cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_Cancel.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold);
            this.m_Cancel.ForeColor = System.Drawing.Color.White;
            this.m_Cancel.Location = new System.Drawing.Point(340, 279);
            this.m_Cancel.Name = "m_Cancel";
            this.m_Cancel.Size = new System.Drawing.Size(75, 23);
            this.m_Cancel.TabIndex = 70;
            this.m_Cancel.Text = "Cancel";
            this.m_Cancel.UseVisualStyleBackColor = false;
            this.m_Cancel.Click += new System.EventHandler(this.m_Cancel_Click);
            // 
            // m_Sure
            // 
            this.m_Sure.BackColor = System.Drawing.Color.SkyBlue;
            this.m_Sure.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_Sure.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold);
            this.m_Sure.ForeColor = System.Drawing.Color.White;
            this.m_Sure.Location = new System.Drawing.Point(245, 279);
            this.m_Sure.Name = "m_Sure";
            this.m_Sure.Size = new System.Drawing.Size(75, 23);
            this.m_Sure.TabIndex = 69;
            this.m_Sure.Text = "Sure";
            this.m_Sure.UseVisualStyleBackColor = false;
            this.m_Sure.Click += new System.EventHandler(this.m_Sure_Click);
            // 
            // m_ChoosePic
            // 
            this.m_ChoosePic.BackColor = System.Drawing.Color.SkyBlue;
            this.m_ChoosePic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_ChoosePic.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold);
            this.m_ChoosePic.ForeColor = System.Drawing.Color.White;
            this.m_ChoosePic.Location = new System.Drawing.Point(259, 183);
            this.m_ChoosePic.Name = "m_ChoosePic";
            this.m_ChoosePic.Size = new System.Drawing.Size(113, 23);
            this.m_ChoosePic.TabIndex = 68;
            this.m_ChoosePic.Text = "UploadPic";
            this.m_ChoosePic.UseVisualStyleBackColor = false;
            this.m_ChoosePic.Click += new System.EventHandler(this.m_UploadPic_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox1.Location = new System.Drawing.Point(261, 26);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(153, 137);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 67;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // m_Gender
            // 
            this.m_Gender.FormattingEnabled = true;
            this.m_Gender.Items.AddRange(new object[] {
            "male",
            "female"});
            this.m_Gender.Location = new System.Drawing.Point(101, 72);
            this.m_Gender.Name = "m_Gender";
            this.m_Gender.Size = new System.Drawing.Size(111, 20);
            this.m_Gender.TabIndex = 66;
            // 
            // m_CeriNum
            // 
            this.m_CeriNum.Location = new System.Drawing.Point(101, 236);
            this.m_CeriNum.Name = "m_CeriNum";
            this.m_CeriNum.Size = new System.Drawing.Size(111, 21);
            this.m_CeriNum.TabIndex = 63;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 239);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 62;
            this.label3.Text = "certificateNo.";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 196);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 12);
            this.label5.TabIndex = 60;
            this.label5.Text = "certificateType ";
            // 
            // m_BornTime
            // 
            this.m_BornTime.Location = new System.Drawing.Point(101, 116);
            this.m_BornTime.Name = "m_BornTime";
            this.m_BornTime.Size = new System.Drawing.Size(111, 21);
            this.m_BornTime.TabIndex = 59;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 116);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 58;
            this.label4.Text = "BornTime";
            // 
            // m_City
            // 
            this.m_City.Location = new System.Drawing.Point(101, 156);
            this.m_City.Name = "m_City";
            this.m_City.Size = new System.Drawing.Size(111, 21);
            this.m_City.TabIndex = 55;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 159);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 54;
            this.label2.Text = "City";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 52;
            this.label1.Text = "gender";
            // 
            // m_Name
            // 
            this.m_Name.Location = new System.Drawing.Point(101, 26);
            this.m_Name.Name = "m_Name";
            this.m_Name.Size = new System.Drawing.Size(111, 21);
            this.m_Name.TabIndex = 51;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 29);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 12);
            this.label6.TabIndex = 50;
            this.label6.Text = "Name";
            // 
            // AddFaceDada
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(541, 389);
            this.Controls.Add(this.groupBox1);
            this.Name = "AddFaceDada";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AddFaceDada";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox m_Gender;
        private System.Windows.Forms.TextBox m_CeriNum;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DateTimePicker m_BornTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox m_City;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox m_Name;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button m_Cancel;
        private System.Windows.Forms.Button m_Sure;
        private System.Windows.Forms.Button m_ChoosePic;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ComboBox m_Type;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox m_textBoxUrl;
        private System.Windows.Forms.CheckBox chkConcurrent;
        private System.Windows.Forms.TextBox txIdentityKey;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox chkCover;
    }
}