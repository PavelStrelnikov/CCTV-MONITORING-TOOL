namespace SDKThermometry
{
    partial class FormManualRuleThermmometry
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
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.textBoxMinThermX = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxMinThermY = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBoxMaxThermX = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxMaxThermY = new System.Windows.Forms.TextBox();
            this.btnGetRuleTherm = new System.Windows.Forms.Button();
            this.textBoxRuleID = new System.Windows.Forms.TextBox();
            this.textBoxAverTherm = new System.Windows.Forms.TextBox();
            this.textBoxMinTherm = new System.Windows.Forms.TextBox();
            this.textBoxMaxTherm = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.btnGetRuleTherm);
            this.groupBox1.Controls.Add(this.textBoxRuleID);
            this.groupBox1.Controls.Add(this.textBoxAverTherm);
            this.groupBox1.Controls.Add(this.textBoxMinTherm);
            this.groupBox1.Controls.Add(this.textBoxMaxTherm);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(1, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(562, 344);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "规则温度信息";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.textBoxMinThermX);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.textBoxMinThermY);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Location = new System.Drawing.Point(297, 160);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(200, 100);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "最低温坐标";
            // 
            // textBoxMinThermX
            // 
            this.textBoxMinThermX.Location = new System.Drawing.Point(51, 26);
            this.textBoxMinThermX.Name = "textBoxMinThermX";
            this.textBoxMinThermX.Size = new System.Drawing.Size(100, 21);
            this.textBoxMinThermX.TabIndex = 1;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 35);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(17, 12);
            this.label8.TabIndex = 0;
            this.label8.Text = "X:";
            // 
            // textBoxMinThermY
            // 
            this.textBoxMinThermY.Location = new System.Drawing.Point(51, 70);
            this.textBoxMinThermY.Name = "textBoxMinThermY";
            this.textBoxMinThermY.Size = new System.Drawing.Size(100, 21);
            this.textBoxMinThermY.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 73);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 12);
            this.label7.TabIndex = 0;
            this.label7.Text = "Y:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.textBoxMaxThermX);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.textBoxMaxThermY);
            this.groupBox2.Location = new System.Drawing.Point(50, 160);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 100);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "最高温坐标";
            // 
            // textBoxMaxThermX
            // 
            this.textBoxMaxThermX.Location = new System.Drawing.Point(43, 26);
            this.textBoxMaxThermX.Name = "textBoxMaxThermX";
            this.textBoxMaxThermX.Size = new System.Drawing.Size(100, 21);
            this.textBoxMaxThermX.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(20, 35);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "X:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 73);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(17, 12);
            this.label6.TabIndex = 0;
            this.label6.Text = "Y:";
            // 
            // textBoxMaxThermY
            // 
            this.textBoxMaxThermY.Location = new System.Drawing.Point(43, 70);
            this.textBoxMaxThermY.Name = "textBoxMaxThermY";
            this.textBoxMaxThermY.Size = new System.Drawing.Size(100, 21);
            this.textBoxMaxThermY.TabIndex = 1;
            // 
            // btnGetRuleTherm
            // 
            this.btnGetRuleTherm.Location = new System.Drawing.Point(315, 36);
            this.btnGetRuleTherm.Name = "btnGetRuleTherm";
            this.btnGetRuleTherm.Size = new System.Drawing.Size(75, 23);
            this.btnGetRuleTherm.TabIndex = 2;
            this.btnGetRuleTherm.Text = "获取";
            this.btnGetRuleTherm.UseVisualStyleBackColor = true;
            this.btnGetRuleTherm.Click += new System.EventHandler(this.btnGetRuleTherm_Click);
            // 
            // textBoxRuleID
            // 
            this.textBoxRuleID.Location = new System.Drawing.Point(90, 36);
            this.textBoxRuleID.Name = "textBoxRuleID";
            this.textBoxRuleID.Size = new System.Drawing.Size(100, 21);
            this.textBoxRuleID.TabIndex = 1;
            // 
            // textBoxAverTherm
            // 
            this.textBoxAverTherm.Location = new System.Drawing.Point(456, 88);
            this.textBoxAverTherm.Name = "textBoxAverTherm";
            this.textBoxAverTherm.Size = new System.Drawing.Size(100, 21);
            this.textBoxAverTherm.TabIndex = 1;
            // 
            // textBoxMinTherm
            // 
            this.textBoxMinTherm.Location = new System.Drawing.Point(256, 88);
            this.textBoxMinTherm.Name = "textBoxMinTherm";
            this.textBoxMinTherm.Size = new System.Drawing.Size(100, 21);
            this.textBoxMinTherm.TabIndex = 1;
            // 
            // textBoxMaxTherm
            // 
            this.textBoxMaxTherm.Location = new System.Drawing.Point(90, 88);
            this.textBoxMaxTherm.Name = "textBoxMaxTherm";
            this.textBoxMaxTherm.Size = new System.Drawing.Size(100, 21);
            this.textBoxMaxTherm.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(384, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "平均温度";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(209, 94);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "最低温";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 91);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "最高温";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "RuleID";
            // 
            // FormRuleThermmometry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(575, 359);
            this.Controls.Add(this.groupBox1);
            this.Name = "FormRuleThermmometry";
            this.Text = "手动获取测温规则温度信息";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox textBoxMinThermX;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBoxMinThermY;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBoxMaxThermX;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxMaxThermY;
        private System.Windows.Forms.Button btnGetRuleTherm;
        private System.Windows.Forms.TextBox textBoxRuleID;
        private System.Windows.Forms.TextBox textBoxAverTherm;
        private System.Windows.Forms.TextBox textBoxMinTherm;
        private System.Windows.Forms.TextBox textBoxMaxTherm;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}