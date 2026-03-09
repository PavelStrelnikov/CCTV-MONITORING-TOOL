namespace SDKANPR
{
    partial class licensePlateRecognitionForm
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
            this.DetectModeBox = new System.Windows.Forms.ComboBox();
            this.SET = new System.Windows.Forms.Button();
            this.Get = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.CommandBtn = new System.Windows.Forms.Button();
            this.CommandBox = new System.Windows.Forms.ComboBox();
            this.URLTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.ResultTextBox = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.RequestTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.DetectModeBox);
            this.groupBox1.Controls.Add(this.SET);
            this.groupBox1.Controls.Add(this.Get);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(789, 75);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "检测模式";
            // 
            // DetectModeBox
            // 
            this.DetectModeBox.FormattingEnabled = true;
            this.DetectModeBox.Items.AddRange(new object[] {
            "混行检测",
            "车辆检测"});
            this.DetectModeBox.Location = new System.Drawing.Point(51, 34);
            this.DetectModeBox.Name = "DetectModeBox";
            this.DetectModeBox.Size = new System.Drawing.Size(121, 20);
            this.DetectModeBox.TabIndex = 2;
            // 
            // SET
            // 
            this.SET.Location = new System.Drawing.Point(669, 31);
            this.SET.Name = "SET";
            this.SET.Size = new System.Drawing.Size(75, 23);
            this.SET.TabIndex = 1;
            this.SET.Text = "设置";
            this.SET.UseVisualStyleBackColor = true;
            this.SET.Click += new System.EventHandler(this.SET_Click);
            // 
            // Get
            // 
            this.Get.Location = new System.Drawing.Point(497, 32);
            this.Get.Name = "Get";
            this.Get.Size = new System.Drawing.Size(75, 23);
            this.Get.TabIndex = 0;
            this.Get.Text = "获取";
            this.Get.UseVisualStyleBackColor = true;
            this.Get.Click += new System.EventHandler(this.Get_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.AutoSize = true;
            this.groupBox2.Controls.Add(this.CommandBtn);
            this.groupBox2.Controls.Add(this.CommandBox);
            this.groupBox2.Controls.Add(this.URLTextBox);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.groupBox4);
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Location = new System.Drawing.Point(12, 107);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(789, 577);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "车辆检测配置";
            // 
            // CommandBtn
            // 
            this.CommandBtn.Location = new System.Drawing.Point(686, 41);
            this.CommandBtn.Name = "CommandBtn";
            this.CommandBtn.Size = new System.Drawing.Size(75, 23);
            this.CommandBtn.TabIndex = 5;
            this.CommandBtn.Text = "操作";
            this.CommandBtn.UseVisualStyleBackColor = true;
            this.CommandBtn.Click += new System.EventHandler(this.CommandBtn_Click);
            // 
            // CommandBox
            // 
            this.CommandBox.Cursor = System.Windows.Forms.Cursors.No;
            this.CommandBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CommandBox.FormattingEnabled = true;
            this.CommandBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CommandBox.Items.AddRange(new object[] {
            "GET",
            "PUT",
            "POST",
            "DELETE"});
            this.CommandBox.Location = new System.Drawing.Point(21, 41);
            this.CommandBox.Name = "CommandBox";
            this.CommandBox.Size = new System.Drawing.Size(121, 20);
            this.CommandBox.TabIndex = 4;
            // 
            // URLTextBox
            // 
            this.URLTextBox.Location = new System.Drawing.Point(148, 40);
            this.URLTextBox.Name = "URLTextBox";
            this.URLTextBox.Size = new System.Drawing.Size(509, 21);
            this.URLTextBox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(146, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "URI:";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.ResultTextBox);
            this.groupBox4.Location = new System.Drawing.Point(416, 107);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(345, 358);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "输出报文";
            // 
            // ResultTextBox
            // 
            this.ResultTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ResultTextBox.Location = new System.Drawing.Point(7, 13);
            this.ResultTextBox.Multiline = true;
            this.ResultTextBox.Name = "ResultTextBox";
            this.ResultTextBox.Size = new System.Drawing.Size(333, 340);
            this.ResultTextBox.TabIndex = 1;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox3.Controls.Add(this.RequestTextBox);
            this.groupBox3.Location = new System.Drawing.Point(6, 107);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(360, 353);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "输入报文";
            // 
            // RequestTextBox
            // 
            this.RequestTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RequestTextBox.Location = new System.Drawing.Point(6, 20);
            this.RequestTextBox.Multiline = true;
            this.RequestTextBox.Name = "RequestTextBox";
            this.RequestTextBox.Size = new System.Drawing.Size(348, 327);
            this.RequestTextBox.TabIndex = 0;
            // 
            // licensePlateRecognitionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(819, 696);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "licensePlateRecognitionForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button SET;
        private System.Windows.Forms.Button Get;
        private System.Windows.Forms.ComboBox DetectModeBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox URLTextBox;
        private System.Windows.Forms.ComboBox CommandBox;
        private System.Windows.Forms.Button CommandBtn;
        private System.Windows.Forms.TextBox RequestTextBox;
        private System.Windows.Forms.TextBox ResultTextBox;
    }
}