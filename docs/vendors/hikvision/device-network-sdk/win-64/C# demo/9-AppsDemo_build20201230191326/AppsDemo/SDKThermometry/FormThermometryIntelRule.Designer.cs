namespace SDKThermometry
{
    partial class FormThermometryIntelRule
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
            this.btnSetThermIntelRule = new System.Windows.Forms.Button();
            this.btnGetThermIntelRule = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSaveLineRuleColor = new System.Windows.Forms.Button();
            this.textBoxLineB = new System.Windows.Forms.TextBox();
            this.textBoxLineG = new System.Windows.Forms.TextBox();
            this.textBoxLineR = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxRuleLineColor = new System.Windows.Forms.ComboBox();
            this.comboBoxFontSize = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox1.Controls.Add(this.btnSetThermIntelRule);
            this.groupBox1.Controls.Add(this.btnGetThermIntelRule);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.comboBoxRuleLineColor);
            this.groupBox1.Controls.Add(this.comboBoxFontSize);
            this.groupBox1.Location = new System.Drawing.Point(63, 46);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(329, 392);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "热成像智能规则";
            // 
            // btnSetThermIntelRule
            // 
            this.btnSetThermIntelRule.Location = new System.Drawing.Point(170, 353);
            this.btnSetThermIntelRule.Name = "btnSetThermIntelRule";
            this.btnSetThermIntelRule.Size = new System.Drawing.Size(75, 23);
            this.btnSetThermIntelRule.TabIndex = 3;
            this.btnSetThermIntelRule.Text = "设置";
            this.btnSetThermIntelRule.UseVisualStyleBackColor = true;
            this.btnSetThermIntelRule.Click += new System.EventHandler(this.btnSetThermIntelRule_Click);
            // 
            // btnGetThermIntelRule
            // 
            this.btnGetThermIntelRule.Location = new System.Drawing.Point(53, 353);
            this.btnGetThermIntelRule.Name = "btnGetThermIntelRule";
            this.btnGetThermIntelRule.Size = new System.Drawing.Size(75, 23);
            this.btnGetThermIntelRule.TabIndex = 3;
            this.btnGetThermIntelRule.Text = "获取";
            this.btnGetThermIntelRule.UseVisualStyleBackColor = true;
            this.btnGetThermIntelRule.Click += new System.EventHandler(this.btnGetThermIntelRule_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnSaveLineRuleColor);
            this.groupBox2.Controls.Add(this.textBoxLineB);
            this.groupBox2.Controls.Add(this.textBoxLineG);
            this.groupBox2.Controls.Add(this.textBoxLineR);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(38, 130);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(219, 197);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "规则配置";
            // 
            // btnSaveLineRuleColor
            // 
            this.btnSaveLineRuleColor.Location = new System.Drawing.Point(110, 163);
            this.btnSaveLineRuleColor.Name = "btnSaveLineRuleColor";
            this.btnSaveLineRuleColor.Size = new System.Drawing.Size(75, 23);
            this.btnSaveLineRuleColor.TabIndex = 2;
            this.btnSaveLineRuleColor.Text = "保存";
            this.btnSaveLineRuleColor.UseVisualStyleBackColor = true;
            this.btnSaveLineRuleColor.Click += new System.EventHandler(this.btnSaveLineRuleColor_Click);
            // 
            // textBoxLineB
            // 
            this.textBoxLineB.Location = new System.Drawing.Point(86, 118);
            this.textBoxLineB.Name = "textBoxLineB";
            this.textBoxLineB.Size = new System.Drawing.Size(100, 21);
            this.textBoxLineB.TabIndex = 1;
            this.textBoxLineB.Text = "0";
            // 
            // textBoxLineG
            // 
            this.textBoxLineG.Location = new System.Drawing.Point(86, 76);
            this.textBoxLineG.Name = "textBoxLineG";
            this.textBoxLineG.Size = new System.Drawing.Size(100, 21);
            this.textBoxLineG.TabIndex = 1;
            this.textBoxLineG.Text = "0";
            // 
            // textBoxLineR
            // 
            this.textBoxLineR.Location = new System.Drawing.Point(86, 26);
            this.textBoxLineR.Name = "textBoxLineR";
            this.textBoxLineR.Size = new System.Drawing.Size(100, 21);
            this.textBoxLineR.TabIndex = 1;
            this.textBoxLineR.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(30, 127);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(11, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "B";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 85);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(11, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "G";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(11, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "R";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(36, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "颜色规则";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "字体大小";
            // 
            // comboBoxRuleLineColor
            // 
            this.comboBoxRuleLineColor.FormattingEnabled = true;
            this.comboBoxRuleLineColor.Items.AddRange(new object[] {
            "正常规则",
            "预警规则",
            "报警规则"});
            this.comboBoxRuleLineColor.Location = new System.Drawing.Point(124, 82);
            this.comboBoxRuleLineColor.Name = "comboBoxRuleLineColor";
            this.comboBoxRuleLineColor.Size = new System.Drawing.Size(121, 20);
            this.comboBoxRuleLineColor.TabIndex = 0;
            this.comboBoxRuleLineColor.SelectedIndexChanged += new System.EventHandler(this.comboBoxRuleLineColor_SelectedIndexChanged);
            // 
            // comboBoxFontSize
            // 
            this.comboBoxFontSize.FormattingEnabled = true;
            this.comboBoxFontSize.Items.AddRange(new object[] {
            "8倍率",
            "12倍率",
            "16倍率",
            "24倍率"});
            this.comboBoxFontSize.Location = new System.Drawing.Point(124, 31);
            this.comboBoxFontSize.Name = "comboBoxFontSize";
            this.comboBoxFontSize.Size = new System.Drawing.Size(121, 20);
            this.comboBoxFontSize.TabIndex = 0;
            // 
            // FormThermometryIntelRule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(515, 491);
            this.Controls.Add(this.groupBox1);
            this.Name = "FormThermometryIntelRule";
            this.Text = "热成像智能规则";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBoxLineB;
        private System.Windows.Forms.TextBox textBoxLineG;
        private System.Windows.Forms.TextBox textBoxLineR;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxRuleLineColor;
        private System.Windows.Forms.ComboBox comboBoxFontSize;
        private System.Windows.Forms.Button btnSetThermIntelRule;
        private System.Windows.Forms.Button btnGetThermIntelRule;
        private System.Windows.Forms.Button btnSaveLineRuleColor;
    }
}