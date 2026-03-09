namespace SDKThermometry
{
    partial class FormThermalStream
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
            this.textBoxIntervalTime = new System.Windows.Forms.TextBox();
            this.btnSetBaredataOverlay = new System.Windows.Forms.Button();
            this.btnGetBaredataOverlay = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxBaredataOverlay = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.textBoxGrayscale = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.textBoxTemperature = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.btnThermalDataConv = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.comboBoxThermalDataConv = new System.Windows.Forms.ComboBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnSetStreamParam = new System.Windows.Forms.Button();
            this.btnGetStreamParam = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.comboBoxVideoCodingType = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnSerPixelToPixelParam = new System.Windows.Forms.Button();
            this.btnGetPixelToPixelParam = new System.Windows.Forms.Button();
            this.comboBoxDistanceUnit = new System.Windows.Forms.ComboBox();
            this.checkBoxReflectiveEnable = new System.Windows.Forms.CheckBox();
            this.textBoxRefreshInterval = new System.Windows.Forms.TextBox();
            this.textBoxDistance = new System.Windows.Forms.TextBox();
            this.textBoxEmissivity = new System.Windows.Forms.TextBox();
            this.textBoxReflectiveTemperature = new System.Windows.Forms.TextBox();
            this.textBoxMaxFrameRate = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnSetPalettes = new System.Windows.Forms.Button();
            this.btnGetPalettes = new System.Windows.Forms.Button();
            this.comboBoxColorateTargetMode = new System.Windows.Forms.ComboBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.textBoxB = new System.Windows.Forms.TextBox();
            this.textBoxG = new System.Windows.Forms.TextBox();
            this.textBoxR = new System.Windows.Forms.TextBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.textBoxMinTemperature = new System.Windows.Forms.TextBox();
            this.textBoxMaxTemperature = new System.Windows.Forms.TextBox();
            this.checkBoxColorateTargetMode = new System.Windows.Forms.CheckBox();
            this.checkBoxWhiteHot = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox1.Controls.Add(this.textBoxIntervalTime);
            this.groupBox1.Controls.Add(this.btnSetBaredataOverlay);
            this.groupBox1.Controls.Add(this.btnGetBaredataOverlay);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.checkBoxBaredataOverlay);
            this.groupBox1.Location = new System.Drawing.Point(31, 33);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(285, 166);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "码流叠加原始数据";
            // 
            // textBoxIntervalTime
            // 
            this.textBoxIntervalTime.Location = new System.Drawing.Point(155, 84);
            this.textBoxIntervalTime.Name = "textBoxIntervalTime";
            this.textBoxIntervalTime.Size = new System.Drawing.Size(100, 21);
            this.textBoxIntervalTime.TabIndex = 4;
            // 
            // btnSetBaredataOverlay
            // 
            this.btnSetBaredataOverlay.Location = new System.Drawing.Point(180, 126);
            this.btnSetBaredataOverlay.Name = "btnSetBaredataOverlay";
            this.btnSetBaredataOverlay.Size = new System.Drawing.Size(75, 23);
            this.btnSetBaredataOverlay.TabIndex = 3;
            this.btnSetBaredataOverlay.Text = "设置";
            this.btnSetBaredataOverlay.UseVisualStyleBackColor = true;
            this.btnSetBaredataOverlay.Click += new System.EventHandler(this.btnSetBaredataOverlay_Click);
            // 
            // btnGetBaredataOverlay
            // 
            this.btnGetBaredataOverlay.Location = new System.Drawing.Point(22, 126);
            this.btnGetBaredataOverlay.Name = "btnGetBaredataOverlay";
            this.btnGetBaredataOverlay.Size = new System.Drawing.Size(75, 23);
            this.btnGetBaredataOverlay.TabIndex = 2;
            this.btnGetBaredataOverlay.Text = "获取";
            this.btnGetBaredataOverlay.UseVisualStyleBackColor = true;
            this.btnGetBaredataOverlay.Click += new System.EventHandler(this.btnGetBaredataOverlay_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 87);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "刷新原始数据间隔(s)";
            // 
            // checkBoxBaredataOverlay
            // 
            this.checkBoxBaredataOverlay.AutoSize = true;
            this.checkBoxBaredataOverlay.Location = new System.Drawing.Point(22, 36);
            this.checkBoxBaredataOverlay.Name = "checkBoxBaredataOverlay";
            this.checkBoxBaredataOverlay.Size = new System.Drawing.Size(108, 16);
            this.checkBoxBaredataOverlay.TabIndex = 0;
            this.checkBoxBaredataOverlay.Text = "码流叠加裸数据";
            this.checkBoxBaredataOverlay.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox2.Controls.Add(this.groupBox8);
            this.groupBox2.Controls.Add(this.groupBox4);
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.groupBox1);
            this.groupBox2.Location = new System.Drawing.Point(22, 23);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(756, 562);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "热成像码流相关";
            // 
            // groupBox8
            // 
            this.groupBox8.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox8.Controls.Add(this.textBoxGrayscale);
            this.groupBox8.Controls.Add(this.label15);
            this.groupBox8.Controls.Add(this.textBoxTemperature);
            this.groupBox8.Controls.Add(this.label14);
            this.groupBox8.Controls.Add(this.btnThermalDataConv);
            this.groupBox8.Controls.Add(this.label13);
            this.groupBox8.Controls.Add(this.comboBoxThermalDataConv);
            this.groupBox8.Location = new System.Drawing.Point(31, 393);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(689, 122);
            this.groupBox8.TabIndex = 32;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "灰度温度转换";
            // 
            // textBoxGrayscale
            // 
            this.textBoxGrayscale.Location = new System.Drawing.Point(417, 63);
            this.textBoxGrayscale.Name = "textBoxGrayscale";
            this.textBoxGrayscale.Size = new System.Drawing.Size(100, 21);
            this.textBoxGrayscale.TabIndex = 6;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(358, 72);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(29, 12);
            this.label15.TabIndex = 5;
            this.label15.Text = "灰度";
            // 
            // textBoxTemperature
            // 
            this.textBoxTemperature.Location = new System.Drawing.Point(230, 63);
            this.textBoxTemperature.Name = "textBoxTemperature";
            this.textBoxTemperature.Size = new System.Drawing.Size(100, 21);
            this.textBoxTemperature.TabIndex = 4;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(178, 72);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(29, 12);
            this.label14.TabIndex = 3;
            this.label14.Text = "温度";
            // 
            // btnThermalDataConv
            // 
            this.btnThermalDataConv.Location = new System.Drawing.Point(552, 64);
            this.btnThermalDataConv.Name = "btnThermalDataConv";
            this.btnThermalDataConv.Size = new System.Drawing.Size(75, 23);
            this.btnThermalDataConv.TabIndex = 2;
            this.btnThermalDataConv.Text = "转换";
            this.btnThermalDataConv.UseVisualStyleBackColor = true;
            this.btnThermalDataConv.Click += new System.EventHandler(this.btnThermalDataConv_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(20, 40);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(53, 12);
            this.label13.TabIndex = 1;
            this.label13.Text = "转换类型";
            // 
            // comboBoxThermalDataConv
            // 
            this.comboBoxThermalDataConv.FormattingEnabled = true;
            this.comboBoxThermalDataConv.Items.AddRange(new object[] {
            "温度->灰度",
            "灰度->温度"});
            this.comboBoxThermalDataConv.Location = new System.Drawing.Point(22, 64);
            this.comboBoxThermalDataConv.Name = "comboBoxThermalDataConv";
            this.comboBoxThermalDataConv.Size = new System.Drawing.Size(121, 20);
            this.comboBoxThermalDataConv.TabIndex = 0;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnSetStreamParam);
            this.groupBox4.Controls.Add(this.btnGetStreamParam);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.comboBoxVideoCodingType);
            this.groupBox4.Location = new System.Drawing.Point(31, 259);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(285, 111);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "热成像码流参数配置";
            // 
            // btnSetStreamParam
            // 
            this.btnSetStreamParam.Location = new System.Drawing.Point(180, 75);
            this.btnSetStreamParam.Name = "btnSetStreamParam";
            this.btnSetStreamParam.Size = new System.Drawing.Size(75, 23);
            this.btnSetStreamParam.TabIndex = 2;
            this.btnSetStreamParam.Text = "设置";
            this.btnSetStreamParam.UseVisualStyleBackColor = true;
            this.btnSetStreamParam.Click += new System.EventHandler(this.btnSetStreamParam_Click);
            // 
            // btnGetStreamParam
            // 
            this.btnGetStreamParam.Location = new System.Drawing.Point(22, 75);
            this.btnGetStreamParam.Name = "btnGetStreamParam";
            this.btnGetStreamParam.Size = new System.Drawing.Size(75, 23);
            this.btnGetStreamParam.TabIndex = 2;
            this.btnGetStreamParam.Text = "获取";
            this.btnGetStreamParam.UseVisualStyleBackColor = true;
            this.btnGetStreamParam.Click += new System.EventHandler(this.btnGetStreamParam_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(20, 38);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(89, 12);
            this.label8.TabIndex = 1;
            this.label8.Text = "码流编解码类型";
            // 
            // comboBoxVideoCodingType
            // 
            this.comboBoxVideoCodingType.FormattingEnabled = true;
            this.comboBoxVideoCodingType.Items.AddRange(new object[] {
            "thermal_raw_data",
            "pixel-to-pixel_thermometry_data",
            "real-time_raw_data"});
            this.comboBoxVideoCodingType.Location = new System.Drawing.Point(134, 30);
            this.comboBoxVideoCodingType.Name = "comboBoxVideoCodingType";
            this.comboBoxVideoCodingType.Size = new System.Drawing.Size(121, 20);
            this.comboBoxVideoCodingType.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox3.Controls.Add(this.btnSerPixelToPixelParam);
            this.groupBox3.Controls.Add(this.btnGetPixelToPixelParam);
            this.groupBox3.Controls.Add(this.comboBoxDistanceUnit);
            this.groupBox3.Controls.Add(this.checkBoxReflectiveEnable);
            this.groupBox3.Controls.Add(this.textBoxRefreshInterval);
            this.groupBox3.Controls.Add(this.textBoxDistance);
            this.groupBox3.Controls.Add(this.textBoxEmissivity);
            this.groupBox3.Controls.Add(this.textBoxReflectiveTemperature);
            this.groupBox3.Controls.Add(this.textBoxMaxFrameRate);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Location = new System.Drawing.Point(363, 33);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(357, 337);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "全屏测温配置";
            // 
            // btnSerPixelToPixelParam
            // 
            this.btnSerPixelToPixelParam.Location = new System.Drawing.Point(195, 292);
            this.btnSerPixelToPixelParam.Name = "btnSerPixelToPixelParam";
            this.btnSerPixelToPixelParam.Size = new System.Drawing.Size(75, 23);
            this.btnSerPixelToPixelParam.TabIndex = 4;
            this.btnSerPixelToPixelParam.Text = "设置";
            this.btnSerPixelToPixelParam.UseVisualStyleBackColor = true;
            this.btnSerPixelToPixelParam.Click += new System.EventHandler(this.btnSerPixelToPixelParam_Click);
            // 
            // btnGetPixelToPixelParam
            // 
            this.btnGetPixelToPixelParam.Location = new System.Drawing.Point(39, 292);
            this.btnGetPixelToPixelParam.Name = "btnGetPixelToPixelParam";
            this.btnGetPixelToPixelParam.Size = new System.Drawing.Size(75, 23);
            this.btnGetPixelToPixelParam.TabIndex = 4;
            this.btnGetPixelToPixelParam.Text = "获取";
            this.btnGetPixelToPixelParam.UseVisualStyleBackColor = true;
            this.btnGetPixelToPixelParam.Click += new System.EventHandler(this.btnGetPixelToPixelParam_Click);
            // 
            // comboBoxDistanceUnit
            // 
            this.comboBoxDistanceUnit.FormattingEnabled = true;
            this.comboBoxDistanceUnit.Items.AddRange(new object[] {
            "meter-米",
            "feet-英尺",
            "centimeter-厘米"});
            this.comboBoxDistanceUnit.Location = new System.Drawing.Point(195, 249);
            this.comboBoxDistanceUnit.Name = "comboBoxDistanceUnit";
            this.comboBoxDistanceUnit.Size = new System.Drawing.Size(100, 20);
            this.comboBoxDistanceUnit.TabIndex = 3;
            // 
            // checkBoxReflectiveEnable
            // 
            this.checkBoxReflectiveEnable.AutoSize = true;
            this.checkBoxReflectiveEnable.Location = new System.Drawing.Point(39, 67);
            this.checkBoxReflectiveEnable.Name = "checkBoxReflectiveEnable";
            this.checkBoxReflectiveEnable.Size = new System.Drawing.Size(96, 16);
            this.checkBoxReflectiveEnable.TabIndex = 2;
            this.checkBoxReflectiveEnable.Text = "反射温度使能";
            this.checkBoxReflectiveEnable.UseVisualStyleBackColor = true;
            // 
            // textBoxRefreshInterval
            // 
            this.textBoxRefreshInterval.Location = new System.Drawing.Point(195, 210);
            this.textBoxRefreshInterval.Name = "textBoxRefreshInterval";
            this.textBoxRefreshInterval.Size = new System.Drawing.Size(100, 21);
            this.textBoxRefreshInterval.TabIndex = 1;
            this.textBoxRefreshInterval.Text = "0";
            // 
            // textBoxDistance
            // 
            this.textBoxDistance.Location = new System.Drawing.Point(195, 171);
            this.textBoxDistance.Name = "textBoxDistance";
            this.textBoxDistance.Size = new System.Drawing.Size(100, 21);
            this.textBoxDistance.TabIndex = 1;
            this.textBoxDistance.Text = "0";
            // 
            // textBoxEmissivity
            // 
            this.textBoxEmissivity.Location = new System.Drawing.Point(195, 137);
            this.textBoxEmissivity.Name = "textBoxEmissivity";
            this.textBoxEmissivity.Size = new System.Drawing.Size(100, 21);
            this.textBoxEmissivity.TabIndex = 1;
            this.textBoxEmissivity.Text = "0";
            // 
            // textBoxReflectiveTemperature
            // 
            this.textBoxReflectiveTemperature.Location = new System.Drawing.Point(195, 100);
            this.textBoxReflectiveTemperature.Name = "textBoxReflectiveTemperature";
            this.textBoxReflectiveTemperature.Size = new System.Drawing.Size(100, 21);
            this.textBoxReflectiveTemperature.TabIndex = 1;
            this.textBoxReflectiveTemperature.Text = "0";
            // 
            // textBoxMaxFrameRate
            // 
            this.textBoxMaxFrameRate.Location = new System.Drawing.Point(195, 37);
            this.textBoxMaxFrameRate.Name = "textBoxMaxFrameRate";
            this.textBoxMaxFrameRate.Size = new System.Drawing.Size(100, 21);
            this.textBoxMaxFrameRate.TabIndex = 1;
            this.textBoxMaxFrameRate.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(37, 249);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 0;
            this.label6.Text = "距离单位";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(37, 210);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(113, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "全屏测温映射表间隔";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(37, 171);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "距离";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(37, 137);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "发射率";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 103);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "反射温度";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(37, 37);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 0;
            this.label7.Text = "最大帧率";
            // 
            // btnSetPalettes
            // 
            this.btnSetPalettes.Location = new System.Drawing.Point(0, 0);
            this.btnSetPalettes.Name = "btnSetPalettes";
            this.btnSetPalettes.Size = new System.Drawing.Size(75, 23);
            this.btnSetPalettes.TabIndex = 0;
            // 
            // btnGetPalettes
            // 
            this.btnGetPalettes.Location = new System.Drawing.Point(0, 0);
            this.btnGetPalettes.Name = "btnGetPalettes";
            this.btnGetPalettes.Size = new System.Drawing.Size(75, 23);
            this.btnGetPalettes.TabIndex = 0;
            // 
            // comboBoxColorateTargetMode
            // 
            this.comboBoxColorateTargetMode.Location = new System.Drawing.Point(0, 0);
            this.comboBoxColorateTargetMode.Name = "comboBoxColorateTargetMode";
            this.comboBoxColorateTargetMode.Size = new System.Drawing.Size(121, 20);
            this.comboBoxColorateTargetMode.TabIndex = 0;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.label18);
            this.groupBox6.Controls.Add(this.label17);
            this.groupBox6.Controls.Add(this.label9);
            this.groupBox6.Controls.Add(this.textBoxB);
            this.groupBox6.Controls.Add(this.textBoxG);
            this.groupBox6.Controls.Add(this.textBoxR);
            this.groupBox6.Location = new System.Drawing.Point(277, 111);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(214, 138);
            this.groupBox6.TabIndex = 4;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "区域颜色";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(25, 97);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(11, 12);
            this.label18.TabIndex = 4;
            this.label18.Text = "B";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(25, 64);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(11, 12);
            this.label17.TabIndex = 4;
            this.label17.Text = "G";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(25, 27);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(11, 12);
            this.label9.TabIndex = 4;
            this.label9.Text = "R";
            // 
            // textBoxB
            // 
            this.textBoxB.Location = new System.Drawing.Point(81, 88);
            this.textBoxB.Name = "textBoxB";
            this.textBoxB.Size = new System.Drawing.Size(100, 21);
            this.textBoxB.TabIndex = 3;
            // 
            // textBoxG
            // 
            this.textBoxG.Location = new System.Drawing.Point(81, 55);
            this.textBoxG.Name = "textBoxG";
            this.textBoxG.Size = new System.Drawing.Size(100, 21);
            this.textBoxG.TabIndex = 3;
            // 
            // textBoxR
            // 
            this.textBoxR.Location = new System.Drawing.Point(81, 20);
            this.textBoxR.Name = "textBoxR";
            this.textBoxR.Size = new System.Drawing.Size(100, 21);
            this.textBoxR.TabIndex = 3;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.label10);
            this.groupBox7.Controls.Add(this.label11);
            this.groupBox7.Controls.Add(this.textBoxMinTemperature);
            this.groupBox7.Controls.Add(this.textBoxMaxTemperature);
            this.groupBox7.Location = new System.Drawing.Point(22, 111);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(230, 138);
            this.groupBox7.TabIndex = 4;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "边界温度";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(22, 94);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(41, 12);
            this.label10.TabIndex = 4;
            this.label10.Text = "最低温";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(22, 45);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(41, 12);
            this.label11.TabIndex = 4;
            this.label11.Text = "最高温";
            // 
            // textBoxMinTemperature
            // 
            this.textBoxMinTemperature.Location = new System.Drawing.Point(97, 85);
            this.textBoxMinTemperature.Name = "textBoxMinTemperature";
            this.textBoxMinTemperature.Size = new System.Drawing.Size(100, 21);
            this.textBoxMinTemperature.TabIndex = 3;
            // 
            // textBoxMaxTemperature
            // 
            this.textBoxMaxTemperature.Location = new System.Drawing.Point(97, 36);
            this.textBoxMaxTemperature.Name = "textBoxMaxTemperature";
            this.textBoxMaxTemperature.Size = new System.Drawing.Size(100, 21);
            this.textBoxMaxTemperature.TabIndex = 3;
            // 
            // checkBoxColorateTargetMode
            // 
            this.checkBoxColorateTargetMode.AutoSize = true;
            this.checkBoxColorateTargetMode.Location = new System.Drawing.Point(277, 72);
            this.checkBoxColorateTargetMode.Name = "checkBoxColorateTargetMode";
            this.checkBoxColorateTargetMode.Size = new System.Drawing.Size(48, 16);
            this.checkBoxColorateTargetMode.TabIndex = 3;
            this.checkBoxColorateTargetMode.Text = "使能";
            this.checkBoxColorateTargetMode.UseVisualStyleBackColor = true;
            // 
            // checkBoxWhiteHot
            // 
            this.checkBoxWhiteHot.AutoSize = true;
            this.checkBoxWhiteHot.Location = new System.Drawing.Point(31, 27);
            this.checkBoxWhiteHot.Name = "checkBoxWhiteHot";
            this.checkBoxWhiteHot.Size = new System.Drawing.Size(72, 16);
            this.checkBoxWhiteHot.TabIndex = 3;
            this.checkBoxWhiteHot.Text = "白热模式";
            this.checkBoxWhiteHot.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(29, 71);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(77, 12);
            this.label12.TabIndex = 2;
            this.label12.Text = "目标凸显模式";
            // 
            // FormThermalStream
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(803, 608);
            this.Controls.Add(this.groupBox2);
            this.Name = "FormThermalStream";
            this.Text = "FormThermalStream";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBoxIntervalTime;
        private System.Windows.Forms.Button btnSetBaredataOverlay;
        private System.Windows.Forms.Button btnGetBaredataOverlay;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxBaredataOverlay;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnSerPixelToPixelParam;
        private System.Windows.Forms.Button btnGetPixelToPixelParam;
        private System.Windows.Forms.ComboBox comboBoxDistanceUnit;
        private System.Windows.Forms.CheckBox checkBoxReflectiveEnable;
        private System.Windows.Forms.TextBox textBoxRefreshInterval;
        private System.Windows.Forms.TextBox textBoxDistance;
        private System.Windows.Forms.TextBox textBoxEmissivity;
        private System.Windows.Forms.TextBox textBoxReflectiveTemperature;
        private System.Windows.Forms.TextBox textBoxMaxFrameRate;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnSetStreamParam;
        private System.Windows.Forms.Button btnGetStreamParam;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboBoxVideoCodingType;
        private System.Windows.Forms.Button btnSetPalettes;
        private System.Windows.Forms.Button btnGetPalettes;
        private System.Windows.Forms.ComboBox comboBoxColorateTargetMode;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBoxB;
        private System.Windows.Forms.TextBox textBoxG;
        private System.Windows.Forms.TextBox textBoxR;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox textBoxMinTemperature;
        private System.Windows.Forms.TextBox textBoxMaxTemperature;
        private System.Windows.Forms.CheckBox checkBoxColorateTargetMode;
        private System.Windows.Forms.CheckBox checkBoxWhiteHot;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Button btnThermalDataConv;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox comboBoxThermalDataConv;
        private System.Windows.Forms.TextBox textBoxGrayscale;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox textBoxTemperature;
        private System.Windows.Forms.Label label14;
    }
}