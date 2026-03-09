namespace SDKThermometry
{
    partial class FormCorrectThermometry
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
            this.btnStartCorrectTherm = new System.Windows.Forms.Button();
            this.btnGetCorrectTherm = new System.Windows.Forms.Button();
            this.btnPostCorrectTherm = new System.Windows.Forms.Button();
            this.btnGetFilePath = new System.Windows.Forms.Button();
            this.textBoxFilePath = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.textBoxTemperaturePointY = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textBoxTemperaturePointX = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnSetCorrectTempPointThird = new System.Windows.Forms.Button();
            this.textBoxTemperaturePointOne = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.btnSetCorrectTempPointFour = new System.Windows.Forms.Button();
            this.btnSetCorrectTempPointTwo = new System.Windows.Forms.Button();
            this.btnSetCorrectTempPointOne = new System.Windows.Forms.Button();
            this.checkBoxPointFour = new System.Windows.Forms.CheckBox();
            this.checkBoxPointThird = new System.Windows.Forms.CheckBox();
            this.checkBoxPointTwo = new System.Windows.Forms.CheckBox();
            this.checkBoxPointOne = new System.Windows.Forms.CheckBox();
            this.textBoxTemperaturePointFour = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxTemperaturePointThird = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxTemperaturePointTwo = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxEmissivity = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxEnviroTemperature = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxDistance = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox1.Controls.Add(this.btnStartCorrectTherm);
            this.groupBox1.Controls.Add(this.btnGetCorrectTherm);
            this.groupBox1.Controls.Add(this.btnPostCorrectTherm);
            this.groupBox1.Controls.Add(this.btnGetFilePath);
            this.groupBox1.Controls.Add(this.textBoxFilePath);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.textBoxEmissivity);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBoxEnviroTemperature);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBoxDistance);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(594, 547);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "测温矫正";
            // 
            // btnStartCorrectTherm
            // 
            this.btnStartCorrectTherm.Location = new System.Drawing.Point(484, 485);
            this.btnStartCorrectTherm.Name = "btnStartCorrectTherm";
            this.btnStartCorrectTherm.Size = new System.Drawing.Size(75, 23);
            this.btnStartCorrectTherm.TabIndex = 6;
            this.btnStartCorrectTherm.Text = "开始矫正";
            this.btnStartCorrectTherm.UseVisualStyleBackColor = true;
            this.btnStartCorrectTherm.Click += new System.EventHandler(this.btnStartCorrectTherm_Click);
            // 
            // btnGetCorrectTherm
            // 
            this.btnGetCorrectTherm.Location = new System.Drawing.Point(355, 485);
            this.btnGetCorrectTherm.Name = "btnGetCorrectTherm";
            this.btnGetCorrectTherm.Size = new System.Drawing.Size(75, 23);
            this.btnGetCorrectTherm.TabIndex = 6;
            this.btnGetCorrectTherm.Text = "获取";
            this.btnGetCorrectTherm.UseVisualStyleBackColor = true;
            this.btnGetCorrectTherm.Click += new System.EventHandler(this.btnGetCorrectTherm_Click);
            // 
            // btnPostCorrectTherm
            // 
            this.btnPostCorrectTherm.Location = new System.Drawing.Point(245, 487);
            this.btnPostCorrectTherm.Name = "btnPostCorrectTherm";
            this.btnPostCorrectTherm.Size = new System.Drawing.Size(75, 23);
            this.btnPostCorrectTherm.TabIndex = 6;
            this.btnPostCorrectTherm.Text = "参数导入";
            this.btnPostCorrectTherm.UseVisualStyleBackColor = true;
            this.btnPostCorrectTherm.Click += new System.EventHandler(this.btnPostCorrectTherm_Click);
            // 
            // btnGetFilePath
            // 
            this.btnGetFilePath.Location = new System.Drawing.Point(159, 487);
            this.btnGetFilePath.Name = "btnGetFilePath";
            this.btnGetFilePath.Size = new System.Drawing.Size(51, 23);
            this.btnGetFilePath.TabIndex = 5;
            this.btnGetFilePath.Text = "文件";
            this.btnGetFilePath.UseVisualStyleBackColor = true;
            this.btnGetFilePath.Click += new System.EventHandler(this.btnGetFilePath_Click);
            // 
            // textBoxFilePath
            // 
            this.textBoxFilePath.Location = new System.Drawing.Point(33, 487);
            this.textBoxFilePath.Name = "textBoxFilePath";
            this.textBoxFilePath.Size = new System.Drawing.Size(100, 21);
            this.textBoxFilePath.TabIndex = 4;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.btnSetCorrectTempPointThird);
            this.groupBox2.Controls.Add(this.textBoxTemperaturePointOne);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.btnSetCorrectTempPointFour);
            this.groupBox2.Controls.Add(this.btnSetCorrectTempPointTwo);
            this.groupBox2.Controls.Add(this.btnSetCorrectTempPointOne);
            this.groupBox2.Controls.Add(this.checkBoxPointFour);
            this.groupBox2.Controls.Add(this.checkBoxPointThird);
            this.groupBox2.Controls.Add(this.checkBoxPointTwo);
            this.groupBox2.Controls.Add(this.checkBoxPointOne);
            this.groupBox2.Controls.Add(this.textBoxTemperaturePointFour);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.textBoxTemperaturePointThird);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.textBoxTemperaturePointTwo);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Location = new System.Drawing.Point(33, 112);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(526, 337);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "预设温度";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.textBoxTemperaturePointY);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.textBoxTemperaturePointX);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(55, 240);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(423, 72);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "温度点坐标";
            // 
            // textBoxTemperaturePointY
            // 
            this.textBoxTemperaturePointY.Location = new System.Drawing.Point(254, 33);
            this.textBoxTemperaturePointY.Name = "textBoxTemperaturePointY";
            this.textBoxTemperaturePointY.Size = new System.Drawing.Size(100, 21);
            this.textBoxTemperaturePointY.TabIndex = 1;
            this.textBoxTemperaturePointY.Text = "0";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(223, 36);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(11, 12);
            this.label9.TabIndex = 0;
            this.label9.Text = "Y";
            // 
            // textBoxTemperaturePointX
            // 
            this.textBoxTemperaturePointX.Location = new System.Drawing.Point(51, 33);
            this.textBoxTemperaturePointX.Name = "textBoxTemperaturePointX";
            this.textBoxTemperaturePointX.Size = new System.Drawing.Size(100, 21);
            this.textBoxTemperaturePointX.TabIndex = 1;
            this.textBoxTemperaturePointX.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 36);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(11, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "X";
            // 
            // btnSetCorrectTempPointThird
            // 
            this.btnSetCorrectTempPointThird.Location = new System.Drawing.Point(280, 147);
            this.btnSetCorrectTempPointThird.Name = "btnSetCorrectTempPointThird";
            this.btnSetCorrectTempPointThird.Size = new System.Drawing.Size(75, 23);
            this.btnSetCorrectTempPointThird.TabIndex = 7;
            this.btnSetCorrectTempPointThird.Text = "设置";
            this.btnSetCorrectTempPointThird.UseVisualStyleBackColor = true;
            this.btnSetCorrectTempPointThird.Click += new System.EventHandler(this.btnSetCorrectTempPointThird_Click);
            // 
            // textBoxTemperaturePointOne
            // 
            this.textBoxTemperaturePointOne.Location = new System.Drawing.Point(144, 66);
            this.textBoxTemperaturePointOne.Name = "textBoxTemperaturePointOne";
            this.textBoxTemperaturePointOne.Size = new System.Drawing.Size(100, 21);
            this.textBoxTemperaturePointOne.TabIndex = 1;
            this.textBoxTemperaturePointOne.Text = "0";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(53, 71);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(47, 12);
            this.label10.TabIndex = 0;
            this.label10.Text = "温度点1";
            // 
            // btnSetCorrectTempPointFour
            // 
            this.btnSetCorrectTempPointFour.Location = new System.Drawing.Point(280, 183);
            this.btnSetCorrectTempPointFour.Name = "btnSetCorrectTempPointFour";
            this.btnSetCorrectTempPointFour.Size = new System.Drawing.Size(75, 23);
            this.btnSetCorrectTempPointFour.TabIndex = 6;
            this.btnSetCorrectTempPointFour.Text = "设置";
            this.btnSetCorrectTempPointFour.UseVisualStyleBackColor = true;
            this.btnSetCorrectTempPointFour.Click += new System.EventHandler(this.btnSetCorrectTempPointFour_Click);
            // 
            // btnSetCorrectTempPointTwo
            // 
            this.btnSetCorrectTempPointTwo.Location = new System.Drawing.Point(280, 106);
            this.btnSetCorrectTempPointTwo.Name = "btnSetCorrectTempPointTwo";
            this.btnSetCorrectTempPointTwo.Size = new System.Drawing.Size(75, 23);
            this.btnSetCorrectTempPointTwo.TabIndex = 5;
            this.btnSetCorrectTempPointTwo.Text = "设置";
            this.btnSetCorrectTempPointTwo.UseVisualStyleBackColor = true;
            this.btnSetCorrectTempPointTwo.Click += new System.EventHandler(this.btnSetCorrectTempPointTwo_Click);
            // 
            // btnSetCorrectTempPointOne
            // 
            this.btnSetCorrectTempPointOne.Location = new System.Drawing.Point(280, 66);
            this.btnSetCorrectTempPointOne.Name = "btnSetCorrectTempPointOne";
            this.btnSetCorrectTempPointOne.Size = new System.Drawing.Size(75, 23);
            this.btnSetCorrectTempPointOne.TabIndex = 3;
            this.btnSetCorrectTempPointOne.Text = "设置";
            this.btnSetCorrectTempPointOne.UseVisualStyleBackColor = true;
            this.btnSetCorrectTempPointOne.Click += new System.EventHandler(this.btnSetCorrectTempPointOne_Click);
            // 
            // checkBoxPointFour
            // 
            this.checkBoxPointFour.AutoSize = true;
            this.checkBoxPointFour.Location = new System.Drawing.Point(394, 188);
            this.checkBoxPointFour.Name = "checkBoxPointFour";
            this.checkBoxPointFour.Size = new System.Drawing.Size(15, 14);
            this.checkBoxPointFour.TabIndex = 2;
            this.checkBoxPointFour.UseVisualStyleBackColor = true;
            // 
            // checkBoxPointThird
            // 
            this.checkBoxPointThird.AutoSize = true;
            this.checkBoxPointThird.Location = new System.Drawing.Point(394, 147);
            this.checkBoxPointThird.Name = "checkBoxPointThird";
            this.checkBoxPointThird.Size = new System.Drawing.Size(15, 14);
            this.checkBoxPointThird.TabIndex = 2;
            this.checkBoxPointThird.UseVisualStyleBackColor = true;
            // 
            // checkBoxPointTwo
            // 
            this.checkBoxPointTwo.AutoSize = true;
            this.checkBoxPointTwo.Location = new System.Drawing.Point(394, 106);
            this.checkBoxPointTwo.Name = "checkBoxPointTwo";
            this.checkBoxPointTwo.Size = new System.Drawing.Size(15, 14);
            this.checkBoxPointTwo.TabIndex = 2;
            this.checkBoxPointTwo.UseVisualStyleBackColor = true;
            // 
            // checkBoxPointOne
            // 
            this.checkBoxPointOne.AutoSize = true;
            this.checkBoxPointOne.Location = new System.Drawing.Point(394, 66);
            this.checkBoxPointOne.Name = "checkBoxPointOne";
            this.checkBoxPointOne.Size = new System.Drawing.Size(15, 14);
            this.checkBoxPointOne.TabIndex = 2;
            this.checkBoxPointOne.UseVisualStyleBackColor = true;
            // 
            // textBoxTemperaturePointFour
            // 
            this.textBoxTemperaturePointFour.Location = new System.Drawing.Point(144, 189);
            this.textBoxTemperaturePointFour.Name = "textBoxTemperaturePointFour";
            this.textBoxTemperaturePointFour.Size = new System.Drawing.Size(100, 21);
            this.textBoxTemperaturePointFour.TabIndex = 1;
            this.textBoxTemperaturePointFour.Text = "0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(53, 192);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 12);
            this.label7.TabIndex = 0;
            this.label7.Text = "温度点4";
            // 
            // textBoxTemperaturePointThird
            // 
            this.textBoxTemperaturePointThird.Location = new System.Drawing.Point(144, 149);
            this.textBoxTemperaturePointThird.Name = "textBoxTemperaturePointThird";
            this.textBoxTemperaturePointThird.Size = new System.Drawing.Size(100, 21);
            this.textBoxTemperaturePointThird.TabIndex = 1;
            this.textBoxTemperaturePointThird.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(53, 152);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 12);
            this.label6.TabIndex = 0;
            this.label6.Text = "温度点3";
            // 
            // textBoxTemperaturePointTwo
            // 
            this.textBoxTemperaturePointTwo.Location = new System.Drawing.Point(144, 108);
            this.textBoxTemperaturePointTwo.Name = "textBoxTemperaturePointTwo";
            this.textBoxTemperaturePointTwo.Size = new System.Drawing.Size(100, 21);
            this.textBoxTemperaturePointTwo.TabIndex = 1;
            this.textBoxTemperaturePointTwo.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(53, 111);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "温度点2";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(384, 29);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 12);
            this.label8.TabIndex = 0;
            this.label8.Text = "状态返回";
            // 
            // textBoxEmissivity
            // 
            this.textBoxEmissivity.Location = new System.Drawing.Point(459, 53);
            this.textBoxEmissivity.Name = "textBoxEmissivity";
            this.textBoxEmissivity.Size = new System.Drawing.Size(100, 21);
            this.textBoxEmissivity.TabIndex = 1;
            this.textBoxEmissivity.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(401, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "发射率";
            // 
            // textBoxEnviroTemperature
            // 
            this.textBoxEnviroTemperature.Location = new System.Drawing.Point(270, 53);
            this.textBoxEnviroTemperature.Name = "textBoxEnviroTemperature";
            this.textBoxEnviroTemperature.Size = new System.Drawing.Size(100, 21);
            this.textBoxEnviroTemperature.TabIndex = 1;
            this.textBoxEnviroTemperature.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(197, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "环境温度";
            // 
            // textBoxDistance
            // 
            this.textBoxDistance.Location = new System.Drawing.Point(79, 53);
            this.textBoxDistance.Name = "textBoxDistance";
            this.textBoxDistance.Size = new System.Drawing.Size(100, 21);
            this.textBoxDistance.TabIndex = 1;
            this.textBoxDistance.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "距离";
            // 
            // FormCorrectThermometry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 597);
            this.Controls.Add(this.groupBox1);
            this.Name = "FormCorrectThermometry";
            this.Text = "测温矫正";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBoxPointFour;
        private System.Windows.Forms.CheckBox checkBoxPointThird;
        private System.Windows.Forms.CheckBox checkBoxPointTwo;
        private System.Windows.Forms.CheckBox checkBoxPointOne;
        private System.Windows.Forms.TextBox textBoxTemperaturePointFour;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxTemperaturePointThird;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxTemperaturePointTwo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxTemperaturePointX;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxEmissivity;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxEnviroTemperature;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxDistance;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnStartCorrectTherm;
        private System.Windows.Forms.Button btnGetCorrectTherm;
        private System.Windows.Forms.Button btnPostCorrectTherm;
        private System.Windows.Forms.Button btnGetFilePath;
        private System.Windows.Forms.TextBox textBoxFilePath;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnSetCorrectTempPointThird;
        private System.Windows.Forms.Button btnSetCorrectTempPointFour;
        private System.Windows.Forms.Button btnSetCorrectTempPointTwo;
        private System.Windows.Forms.Button btnSetCorrectTempPointOne;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox textBoxTemperaturePointY;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBoxTemperaturePointOne;
        private System.Windows.Forms.Label label10;
    }
}