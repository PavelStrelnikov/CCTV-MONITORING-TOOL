namespace SDKThermometry
{
    partial class FormBasicParam
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
            this.groupBox_BasicParam = new System.Windows.Forms.GroupBox();
            this.textOpticalTemperature = new System.Windows.Forms.TextBox();
            this.textTransmissivity = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnThermBasicGet = new System.Windows.Forms.Button();
            this.btnThermBasicSet = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboRange = new System.Windows.Forms.ComboBox();
            this.comboUnit = new System.Windows.Forms.ComboBox();
            this.chkPicOverlapOriginal = new System.Windows.Forms.CheckBox();
            this.chkOverlapTemperature = new System.Windows.Forms.CheckBox();
            this.chkTemperatureStrip = new System.Windows.Forms.CheckBox();
            this.chkThemometry = new System.Windows.Forms.CheckBox();
            this.groupBox_BasicParam.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox_BasicParam
            // 
            this.groupBox_BasicParam.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox_BasicParam.Controls.Add(this.textOpticalTemperature);
            this.groupBox_BasicParam.Controls.Add(this.textTransmissivity);
            this.groupBox_BasicParam.Controls.Add(this.label5);
            this.groupBox_BasicParam.Controls.Add(this.btnThermBasicGet);
            this.groupBox_BasicParam.Controls.Add(this.btnThermBasicSet);
            this.groupBox_BasicParam.Controls.Add(this.label3);
            this.groupBox_BasicParam.Controls.Add(this.label4);
            this.groupBox_BasicParam.Controls.Add(this.label2);
            this.groupBox_BasicParam.Controls.Add(this.comboRange);
            this.groupBox_BasicParam.Controls.Add(this.comboUnit);
            this.groupBox_BasicParam.Controls.Add(this.chkPicOverlapOriginal);
            this.groupBox_BasicParam.Controls.Add(this.chkOverlapTemperature);
            this.groupBox_BasicParam.Controls.Add(this.chkTemperatureStrip);
            this.groupBox_BasicParam.Controls.Add(this.chkThemometry);
            this.groupBox_BasicParam.Location = new System.Drawing.Point(30, 23);
            this.groupBox_BasicParam.Name = "groupBox_BasicParam";
            this.groupBox_BasicParam.Size = new System.Drawing.Size(438, 291);
            this.groupBox_BasicParam.TabIndex = 3;
            this.groupBox_BasicParam.TabStop = false;
            this.groupBox_BasicParam.Text = "基本参数配置";
            // 
            // textOpticalTemperature
            // 
            this.textOpticalTemperature.Location = new System.Drawing.Point(157, 146);
            this.textOpticalTemperature.Name = "textOpticalTemperature";
            this.textOpticalTemperature.Size = new System.Drawing.Size(100, 21);
            this.textOpticalTemperature.TabIndex = 4;
            // 
            // textTransmissivity
            // 
            this.textTransmissivity.Location = new System.Drawing.Point(157, 104);
            this.textTransmissivity.Name = "textTransmissivity";
            this.textTransmissivity.Size = new System.Drawing.Size(100, 21);
            this.textTransmissivity.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(18, 147);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 3;
            this.label5.Text = "外部光学温度";
            // 
            // btnThermBasicGet
            // 
            this.btnThermBasicGet.Location = new System.Drawing.Point(20, 201);
            this.btnThermBasicGet.Name = "btnThermBasicGet";
            this.btnThermBasicGet.Size = new System.Drawing.Size(75, 23);
            this.btnThermBasicGet.TabIndex = 0;
            this.btnThermBasicGet.Text = "获取";
            this.btnThermBasicGet.UseVisualStyleBackColor = true;
            this.btnThermBasicGet.Click += new System.EventHandler(this.btnThermBasicGet_Click);
            // 
            // btnThermBasicSet
            // 
            this.btnThermBasicSet.Location = new System.Drawing.Point(182, 201);
            this.btnThermBasicSet.Name = "btnThermBasicSet";
            this.btnThermBasicSet.Size = new System.Drawing.Size(75, 23);
            this.btnThermBasicSet.TabIndex = 0;
            this.btnThermBasicSet.Text = "设置";
            this.btnThermBasicSet.UseVisualStyleBackColor = true;
            this.btnThermBasicSet.Click += new System.EventHandler(this.btnThermBasicSet_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(348, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "测温范围";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 107);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "外部光学透过率";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(348, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "测温单位";
            // 
            // comboRange
            // 
            this.comboRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboRange.FormattingEnabled = true;
            this.comboRange.Items.AddRange(new object[] {
            "默认值",
            "-20℃~150℃",
            "0℃~550℃"});
            this.comboRange.Location = new System.Drawing.Point(350, 147);
            this.comboRange.Name = "comboRange";
            this.comboRange.Size = new System.Drawing.Size(64, 20);
            this.comboRange.TabIndex = 2;
            // 
            // comboUnit
            // 
            this.comboUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboUnit.FormattingEnabled = true;
            this.comboUnit.Items.AddRange(new object[] {
            "摄氏度",
            "华氏度",
            "开尔文"});
            this.comboUnit.Location = new System.Drawing.Point(350, 65);
            this.comboUnit.Name = "comboUnit";
            this.comboUnit.Size = new System.Drawing.Size(64, 20);
            this.comboUnit.TabIndex = 2;
            // 
            // chkPicOverlapOriginal
            // 
            this.chkPicOverlapOriginal.AutoSize = true;
            this.chkPicOverlapOriginal.Location = new System.Drawing.Point(160, 69);
            this.chkPicOverlapOriginal.Name = "chkPicOverlapOriginal";
            this.chkPicOverlapOriginal.Size = new System.Drawing.Size(120, 16);
            this.chkPicOverlapOriginal.TabIndex = 0;
            this.chkPicOverlapOriginal.Text = "抓图叠加原始数据";
            this.chkPicOverlapOriginal.UseVisualStyleBackColor = true;
            // 
            // chkOverlapTemperature
            // 
            this.chkOverlapTemperature.AutoSize = true;
            this.chkOverlapTemperature.Location = new System.Drawing.Point(20, 69);
            this.chkOverlapTemperature.Name = "chkOverlapTemperature";
            this.chkOverlapTemperature.Size = new System.Drawing.Size(120, 16);
            this.chkOverlapTemperature.TabIndex = 0;
            this.chkOverlapTemperature.Text = "码流叠加温度信息";
            this.chkOverlapTemperature.UseVisualStyleBackColor = true;
            // 
            // chkTemperatureStrip
            // 
            this.chkTemperatureStrip.AutoSize = true;
            this.chkTemperatureStrip.Location = new System.Drawing.Point(160, 34);
            this.chkTemperatureStrip.Name = "chkTemperatureStrip";
            this.chkTemperatureStrip.Size = new System.Drawing.Size(84, 16);
            this.chkTemperatureStrip.TabIndex = 0;
            this.chkTemperatureStrip.Text = "显示温度条";
            this.chkTemperatureStrip.UseVisualStyleBackColor = true;
            // 
            // chkThemometry
            // 
            this.chkThemometry.AutoSize = true;
            this.chkThemometry.Location = new System.Drawing.Point(20, 34);
            this.chkThemometry.Name = "chkThemometry";
            this.chkThemometry.Size = new System.Drawing.Size(72, 16);
            this.chkThemometry.TabIndex = 0;
            this.chkThemometry.Text = "测温功能";
            this.chkThemometry.UseVisualStyleBackColor = true;
            // 
            // FormBasicParam
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(508, 359);
            this.Controls.Add(this.groupBox_BasicParam);
            this.Name = "FormBasicParam";
            this.Text = "测温基本参数配置";
            this.groupBox_BasicParam.ResumeLayout(false);
            this.groupBox_BasicParam.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox_BasicParam;
        private System.Windows.Forms.TextBox textOpticalTemperature;
        private System.Windows.Forms.TextBox textTransmissivity;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnThermBasicGet;
        private System.Windows.Forms.Button btnThermBasicSet;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboRange;
        private System.Windows.Forms.ComboBox comboUnit;
        private System.Windows.Forms.CheckBox chkPicOverlapOriginal;
        private System.Windows.Forms.CheckBox chkOverlapTemperature;
        private System.Windows.Forms.CheckBox chkTemperatureStrip;
        private System.Windows.Forms.CheckBox chkThemometry;
    }
}