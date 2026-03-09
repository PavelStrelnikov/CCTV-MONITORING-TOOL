namespace SDKFaceLib
{
    partial class Face1vN
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
            this.label1 = new System.Windows.Forms.Label();
            this.m_textBoxChannelNo = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.m_textBoxPicturePath = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.buttonComparison = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.m_textBoxStatus = new System.Windows.Forms.TextBox();
            this.m_textBoxSimilarity = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "channelNo";
            // 
            // m_textBoxChannelNo
            // 
            this.m_textBoxChannelNo.Location = new System.Drawing.Point(88, 17);
            this.m_textBoxChannelNo.Name = "m_textBoxChannelNo";
            this.m_textBoxChannelNo.Size = new System.Drawing.Size(100, 21);
            this.m_textBoxChannelNo.TabIndex = 1;
            this.m_textBoxChannelNo.Text = "1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "picture path";
            // 
            // m_textBoxPicturePath
            // 
            this.m_textBoxPicturePath.Location = new System.Drawing.Point(10, 82);
            this.m_textBoxPicturePath.Name = "m_textBoxPicturePath";
            this.m_textBoxPicturePath.Size = new System.Drawing.Size(217, 21);
            this.m_textBoxPicturePath.TabIndex = 3;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(92, 53);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowse.TabIndex = 4;
            this.buttonBrowse.Text = "Browse";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // buttonComparison
            // 
            this.buttonComparison.Location = new System.Drawing.Point(152, 109);
            this.buttonComparison.Name = "buttonComparison";
            this.buttonComparison.Size = new System.Drawing.Size(75, 23);
            this.buttonComparison.TabIndex = 5;
            this.buttonComparison.Text = "Comparison";
            this.buttonComparison.UseVisualStyleBackColor = true;
            this.buttonComparison.Click += new System.EventHandler(this.buttonComparison_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.buttonComparison);
            this.groupBox1.Controls.Add(this.m_textBoxChannelNo);
            this.groupBox1.Controls.Add(this.buttonBrowse);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.m_textBoxPicturePath);
            this.groupBox1.Location = new System.Drawing.Point(12, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(265, 140);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Condition";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.m_textBoxSimilarity);
            this.groupBox2.Controls.Add(this.m_textBoxStatus);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(12, 157);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(265, 106);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Result";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "status";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 1;
            this.label4.Text = "similarity";
            // 
            // m_textBoxStatus
            // 
            this.m_textBoxStatus.Location = new System.Drawing.Point(88, 21);
            this.m_textBoxStatus.Name = "m_textBoxStatus";
            this.m_textBoxStatus.Size = new System.Drawing.Size(100, 21);
            this.m_textBoxStatus.TabIndex = 2;
            // 
            // m_textBoxSimilarity
            // 
            this.m_textBoxSimilarity.Location = new System.Drawing.Point(88, 53);
            this.m_textBoxSimilarity.Name = "m_textBoxSimilarity";
            this.m_textBoxSimilarity.Size = new System.Drawing.Size(100, 21);
            this.m_textBoxSimilarity.TabIndex = 3;
            // 
            // Face1vN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(289, 275);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Face1vN";
            this.Text = "Face1vN";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox m_textBoxChannelNo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox m_textBoxPicturePath;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Button buttonComparison;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox m_textBoxStatus;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox m_textBoxSimilarity;
    }
}