namespace SDKThermometry
{
    partial class FormXML
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
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.label37 = new System.Windows.Forms.Label();
            this.textBoxOutXML = new System.Windows.Forms.TextBox();
            this.label36 = new System.Windows.Forms.Label();
            this.textBoxInXML = new System.Windows.Forms.TextBox();
            this.label35 = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.textBoxRequest = new System.Windows.Forms.TextBox();
            this.btnXML = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxOpera = new System.Windows.Forms.ComboBox();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox6
            // 
            this.groupBox6.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox6.Controls.Add(this.comboBoxOpera);
            this.groupBox6.Controls.Add(this.textBoxStatus);
            this.groupBox6.Controls.Add(this.label37);
            this.groupBox6.Controls.Add(this.textBoxOutXML);
            this.groupBox6.Controls.Add(this.label36);
            this.groupBox6.Controls.Add(this.textBoxInXML);
            this.groupBox6.Controls.Add(this.label35);
            this.groupBox6.Controls.Add(this.label1);
            this.groupBox6.Controls.Add(this.label34);
            this.groupBox6.Controls.Add(this.textBoxRequest);
            this.groupBox6.Controls.Add(this.btnXML);
            this.groupBox6.Location = new System.Drawing.Point(12, 12);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(656, 396);
            this.groupBox6.TabIndex = 55;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "XML透传";
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Location = new System.Drawing.Point(434, 184);
            this.textBoxStatus.Multiline = true;
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxStatus.Size = new System.Drawing.Size(211, 160);
            this.textBoxStatus.TabIndex = 59;
            this.textBoxStatus.WordWrap = false;
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(433, 164);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(53, 12);
            this.label37.TabIndex = 58;
            this.label37.Text = "状态XML:";
            // 
            // textBoxOutXML
            // 
            this.textBoxOutXML.Location = new System.Drawing.Point(217, 184);
            this.textBoxOutXML.Multiline = true;
            this.textBoxOutXML.Name = "textBoxOutXML";
            this.textBoxOutXML.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxOutXML.Size = new System.Drawing.Size(211, 160);
            this.textBoxOutXML.TabIndex = 57;
            this.textBoxOutXML.WordWrap = false;
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(212, 164);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(53, 12);
            this.label36.TabIndex = 56;
            this.label36.Text = "输出XML:";
            // 
            // textBoxInXML
            // 
            this.textBoxInXML.Location = new System.Drawing.Point(24, 184);
            this.textBoxInXML.Multiline = true;
            this.textBoxInXML.Name = "textBoxInXML";
            this.textBoxInXML.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxInXML.Size = new System.Drawing.Size(187, 160);
            this.textBoxInXML.TabIndex = 55;
            this.textBoxInXML.WordWrap = false;
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(20, 164);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(53, 12);
            this.label35.TabIndex = 54;
            this.label35.Text = "输入XML:";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(20, 93);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(35, 12);
            this.label34.TabIndex = 53;
            this.label34.Text = "命令:";
            // 
            // textBoxRequest
            // 
            this.textBoxRequest.Location = new System.Drawing.Point(89, 90);
            this.textBoxRequest.Name = "textBoxRequest";
            this.textBoxRequest.Size = new System.Drawing.Size(309, 21);
            this.textBoxRequest.TabIndex = 52;
            // 
            // btnXML
            // 
            this.btnXML.Location = new System.Drawing.Point(460, 90);
            this.btnXML.Name = "btnXML";
            this.btnXML.Size = new System.Drawing.Size(75, 23);
            this.btnXML.TabIndex = 51;
            this.btnXML.Text = "XML透传";
            this.btnXML.UseVisualStyleBackColor = true;
            this.btnXML.Click += new System.EventHandler(this.btnXML_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 53;
            this.label1.Text = "操作类型:";
            // 
            // comboBoxOpera
            // 
            this.comboBoxOpera.FormattingEnabled = true;
            this.comboBoxOpera.Items.AddRange(new object[] {
            "GET",
            "PUT",
            "POST",
            "DELETE"});
            this.comboBoxOpera.Location = new System.Drawing.Point(89, 37);
            this.comboBoxOpera.Name = "comboBoxOpera";
            this.comboBoxOpera.Size = new System.Drawing.Size(121, 20);
            this.comboBoxOpera.TabIndex = 60;
            // 
            // FormXML
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 420);
            this.Controls.Add(this.groupBox6);
            this.Name = "FormXML";
            this.Text = "FormXML";
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.TextBox textBoxOutXML;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.TextBox textBoxInXML;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.TextBox textBoxRequest;
        private System.Windows.Forms.Button btnXML;
        private System.Windows.Forms.ComboBox comboBoxOpera;
        private System.Windows.Forms.Label label1;
    }
}