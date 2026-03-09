namespace SDKConfiguration
{
    partial class FormUpgrade
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
            this.ComNetEnv = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ComUpgradeType = new System.Windows.Forms.ComboBox();
            this.labelAssist = new System.Windows.Forms.Label();
            this.ComAssistDev = new System.Windows.Forms.ComboBox();
            this.BtnConfigNetEnv = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.UpgradeFilePath = new System.Windows.Forms.TextBox();
            this.BtnScan = new System.Windows.Forms.Button();
            this.CheckUpgrade = new System.Windows.Forms.CheckBox();
            this.BtnObtainUpInfo = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.unitID = new System.Windows.Forms.TextBox();
            this.BtnUpgrade = new System.Windows.Forms.Button();
            this.BtnExit = new System.Windows.Forms.Button();
            this.BtnChoseDev = new System.Windows.Forms.Button();
            this.labelSequence = new System.Windows.Forms.Label();
            this.SequenceNum = new System.Windows.Forms.TextBox();
            this.labelChannel = new System.Windows.Forms.Label();
            this.ComChannel = new System.Windows.Forms.ComboBox();
            this.labelCardType = new System.Windows.Forms.Label();
            this.ComCardType = new System.Windows.Forms.ComboBox();
            this.BtnUpgradeCopy = new System.Windows.Forms.Button();
            this.BtnStopUpgrade = new System.Windows.Forms.Button();
            this.labelUpgradeState = new System.Windows.Forms.Label();
            this.UpgradeBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "NetEnvironment:";
            // 
            // ComNetEnv
            // 
            this.ComNetEnv.FormattingEnabled = true;
            this.ComNetEnv.Items.AddRange(new object[] {
            "LAN",
            "WAN"});
            this.ComNetEnv.Location = new System.Drawing.Point(113, 33);
            this.ComNetEnv.Name = "ComNetEnv";
            this.ComNetEnv.Size = new System.Drawing.Size(53, 20);
            this.ComNetEnv.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(203, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "UpgradeType:";
            // 
            // ComUpgradeType
            // 
            this.ComUpgradeType.Cursor = System.Windows.Forms.Cursors.Default;
            this.ComUpgradeType.FormattingEnabled = true;
            this.ComUpgradeType.Items.AddRange(new object[] {
            "DVR",
            "Adapter",
            "Vca lib",
            "ACS",
            "IDS",
            "LED",
            "Intelligent"});
            this.ComUpgradeType.Location = new System.Drawing.Point(286, 33);
            this.ComUpgradeType.Name = "ComUpgradeType";
            this.ComUpgradeType.Size = new System.Drawing.Size(59, 20);
            this.ComUpgradeType.TabIndex = 3;
            this.ComUpgradeType.SelectedIndexChanged += new System.EventHandler(this.ComUpgradeType_SelectedIndexChanged);
            // 
            // labelAssist
            // 
            this.labelAssist.AutoSize = true;
            this.labelAssist.Location = new System.Drawing.Point(360, 36);
            this.labelAssist.Name = "labelAssist";
            this.labelAssist.Size = new System.Drawing.Size(77, 12);
            this.labelAssist.TabIndex = 4;
            this.labelAssist.Text = "AssistDevice";
            // 
            // ComAssistDev
            // 
            this.ComAssistDev.FormattingEnabled = true;
            this.ComAssistDev.Items.AddRange(new object[] {
            "Keyboard",
            "Movement",
            "NetModule",
            "Router",
            "Zone",
            "RS485",
            "TempCtrl",
            "ElectricLock",
            "NetPortPowerSupply"});
            this.ComAssistDev.Location = new System.Drawing.Point(443, 33);
            this.ComAssistDev.Name = "ComAssistDev";
            this.ComAssistDev.Size = new System.Drawing.Size(55, 20);
            this.ComAssistDev.TabIndex = 5;
            // 
            // BtnConfigNetEnv
            // 
            this.BtnConfigNetEnv.Location = new System.Drawing.Point(514, 33);
            this.BtnConfigNetEnv.Name = "BtnConfigNetEnv";
            this.BtnConfigNetEnv.Size = new System.Drawing.Size(89, 22);
            this.BtnConfigNetEnv.TabIndex = 6;
            this.BtnConfigNetEnv.Text = "ConfigNetEnv";
            this.BtnConfigNetEnv.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 123);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "UpgradeFile";
            // 
            // UpgradeFilePath
            // 
            this.UpgradeFilePath.Location = new System.Drawing.Point(101, 120);
            this.UpgradeFilePath.Name = "UpgradeFilePath";
            this.UpgradeFilePath.Size = new System.Drawing.Size(221, 21);
            this.UpgradeFilePath.TabIndex = 8;
            // 
            // BtnScan
            // 
            this.BtnScan.Location = new System.Drawing.Point(347, 118);
            this.BtnScan.Name = "BtnScan";
            this.BtnScan.Size = new System.Drawing.Size(75, 23);
            this.BtnScan.TabIndex = 9;
            this.BtnScan.Text = "Scan";
            this.BtnScan.UseVisualStyleBackColor = true;
            this.BtnScan.Click += new System.EventHandler(this.BtnScan_Click);
            // 
            // CheckUpgrade
            // 
            this.CheckUpgrade.AutoSize = true;
            this.CheckUpgrade.Location = new System.Drawing.Point(12, 161);
            this.CheckUpgrade.Name = "CheckUpgrade";
            this.CheckUpgrade.Size = new System.Drawing.Size(114, 16);
            this.CheckUpgrade.TabIndex = 10;
            this.CheckUpgrade.Text = "ambigousUpgrade";
            this.CheckUpgrade.UseVisualStyleBackColor = true;
            // 
            // BtnObtainUpInfo
            // 
            this.BtnObtainUpInfo.Location = new System.Drawing.Point(154, 157);
            this.BtnObtainUpInfo.Name = "BtnObtainUpInfo";
            this.BtnObtainUpInfo.Size = new System.Drawing.Size(116, 23);
            this.BtnObtainUpInfo.TabIndex = 11;
            this.BtnObtainUpInfo.Text = "ObtainUpgradeInfo";
            this.BtnObtainUpInfo.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 201);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 12;
            this.label5.Text = "UintID";
            // 
            // unitID
            // 
            this.unitID.Location = new System.Drawing.Point(101, 198);
            this.unitID.Name = "unitID";
            this.unitID.Size = new System.Drawing.Size(221, 21);
            this.unitID.TabIndex = 13;
            // 
            // BtnUpgrade
            // 
            this.BtnUpgrade.Location = new System.Drawing.Point(30, 327);
            this.BtnUpgrade.Name = "BtnUpgrade";
            this.BtnUpgrade.Size = new System.Drawing.Size(72, 23);
            this.BtnUpgrade.TabIndex = 14;
            this.BtnUpgrade.Text = "Upgrade";
            this.BtnUpgrade.UseVisualStyleBackColor = true;
            this.BtnUpgrade.Click += new System.EventHandler(this.BtnUpgrade_Click);
            // 
            // BtnExit
            // 
            this.BtnExit.Location = new System.Drawing.Point(229, 327);
            this.BtnExit.Name = "BtnExit";
            this.BtnExit.Size = new System.Drawing.Size(72, 23);
            this.BtnExit.TabIndex = 15;
            this.BtnExit.Text = "Exit";
            this.BtnExit.UseVisualStyleBackColor = true;
            this.BtnExit.Click += new System.EventHandler(this.BtnExit_Click);
            // 
            // BtnChoseDev
            // 
            this.BtnChoseDev.Location = new System.Drawing.Point(14, 74);
            this.BtnChoseDev.Name = "BtnChoseDev";
            this.BtnChoseDev.Size = new System.Drawing.Size(81, 23);
            this.BtnChoseDev.TabIndex = 16;
            this.BtnChoseDev.Text = "ChoseDevice";
            this.BtnChoseDev.UseVisualStyleBackColor = true;
            // 
            // labelSequence
            // 
            this.labelSequence.AutoSize = true;
            this.labelSequence.Location = new System.Drawing.Point(111, 79);
            this.labelSequence.Name = "labelSequence";
            this.labelSequence.Size = new System.Drawing.Size(71, 12);
            this.labelSequence.TabIndex = 17;
            this.labelSequence.Text = "SequenceNum";
            // 
            // SequenceNum
            // 
            this.SequenceNum.Location = new System.Drawing.Point(188, 73);
            this.SequenceNum.Name = "SequenceNum";
            this.SequenceNum.Size = new System.Drawing.Size(59, 21);
            this.SequenceNum.TabIndex = 18;
            // 
            // labelChannel
            // 
            this.labelChannel.AutoSize = true;
            this.labelChannel.Location = new System.Drawing.Point(366, 79);
            this.labelChannel.Name = "labelChannel";
            this.labelChannel.Size = new System.Drawing.Size(47, 12);
            this.labelChannel.TabIndex = 19;
            this.labelChannel.Text = "Channel";
            // 
            // ComChannel
            // 
            this.ComChannel.FormattingEnabled = true;
            this.ComChannel.Location = new System.Drawing.Point(443, 74);
            this.ComChannel.Name = "ComChannel";
            this.ComChannel.Size = new System.Drawing.Size(55, 20);
            this.ComChannel.TabIndex = 20;
            // 
            // labelCardType
            // 
            this.labelCardType.AutoSize = true;
            this.labelCardType.Location = new System.Drawing.Point(253, 79);
            this.labelCardType.Name = "labelCardType";
            this.labelCardType.Size = new System.Drawing.Size(29, 12);
            this.labelCardType.TabIndex = 21;
            this.labelCardType.Text = "Type";
            // 
            // ComCardType
            // 
            this.ComCardType.FormattingEnabled = true;
            this.ComCardType.Items.AddRange(new object[] {
            "SendCard",
            "RecvCard"});
            this.ComCardType.Location = new System.Drawing.Point(288, 73);
            this.ComCardType.Name = "ComCardType";
            this.ComCardType.Size = new System.Drawing.Size(57, 20);
            this.ComCardType.TabIndex = 22;
            // 
            // BtnUpgradeCopy
            // 
            this.BtnUpgradeCopy.Location = new System.Drawing.Point(126, 327);
            this.BtnUpgradeCopy.Name = "BtnUpgradeCopy";
            this.BtnUpgradeCopy.Size = new System.Drawing.Size(78, 23);
            this.BtnUpgradeCopy.TabIndex = 23;
            this.BtnUpgradeCopy.Text = "UpgradeCopy";
            this.BtnUpgradeCopy.UseVisualStyleBackColor = true;
            //this.BtnUpgradeCopy.Click += new System.EventHandler(this.BtnUpgradeCopy_Click);
            // 
            // BtnStopUpgrade
            // 
            this.BtnStopUpgrade.Location = new System.Drawing.Point(326, 327);
            this.BtnStopUpgrade.Name = "BtnStopUpgrade";
            this.BtnStopUpgrade.Size = new System.Drawing.Size(82, 23);
            this.BtnStopUpgrade.TabIndex = 24;
            this.BtnStopUpgrade.Text = "StopUpgrade";
            this.BtnStopUpgrade.UseVisualStyleBackColor = true;
            this.BtnStopUpgrade.Click += new System.EventHandler(this.BtnStopUpgrade_Click);
            // 
            // labelUpgradeState
            // 
            this.labelUpgradeState.AutoSize = true;
            this.labelUpgradeState.Location = new System.Drawing.Point(41, 235);
            this.labelUpgradeState.Name = "labelUpgradeState";
            this.labelUpgradeState.Size = new System.Drawing.Size(0, 12);
            this.labelUpgradeState.TabIndex = 25;
            // 
            // UpgradeBar
            // 
            this.UpgradeBar.Location = new System.Drawing.Point(32, 279);
            this.UpgradeBar.Name = "UpgradeBar";
            this.UpgradeBar.Size = new System.Drawing.Size(381, 23);
            this.UpgradeBar.TabIndex = 26;
            // 
            // FormUpgrade
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(617, 362);
            this.Controls.Add(this.UpgradeBar);
            this.Controls.Add(this.labelUpgradeState);
            this.Controls.Add(this.BtnStopUpgrade);
            this.Controls.Add(this.BtnUpgradeCopy);
            this.Controls.Add(this.ComCardType);
            this.Controls.Add(this.labelCardType);
            this.Controls.Add(this.ComChannel);
            this.Controls.Add(this.labelChannel);
            this.Controls.Add(this.SequenceNum);
            this.Controls.Add(this.labelSequence);
            this.Controls.Add(this.BtnChoseDev);
            this.Controls.Add(this.BtnExit);
            this.Controls.Add(this.BtnUpgrade);
            this.Controls.Add(this.unitID);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.BtnObtainUpInfo);
            this.Controls.Add(this.CheckUpgrade);
            this.Controls.Add(this.BtnScan);
            this.Controls.Add(this.UpgradeFilePath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.BtnConfigNetEnv);
            this.Controls.Add(this.ComAssistDev);
            this.Controls.Add(this.labelAssist);
            this.Controls.Add(this.ComUpgradeType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ComNetEnv);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "FormUpgrade";
            this.Text = "Device Upgrade";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ComNetEnv;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox ComUpgradeType;
        private System.Windows.Forms.Label labelAssist;
        private System.Windows.Forms.ComboBox ComAssistDev;
        private System.Windows.Forms.Button BtnConfigNetEnv;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox UpgradeFilePath;
        private System.Windows.Forms.Button BtnScan;
        private System.Windows.Forms.CheckBox CheckUpgrade;
        private System.Windows.Forms.Button BtnObtainUpInfo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox unitID;
        private System.Windows.Forms.Button BtnUpgrade;
        private System.Windows.Forms.Button BtnExit;
        private System.Windows.Forms.Button BtnChoseDev;
        private System.Windows.Forms.Label labelSequence;
        private System.Windows.Forms.TextBox SequenceNum;
        private System.Windows.Forms.Label labelChannel;
        private System.Windows.Forms.ComboBox ComChannel;
        private System.Windows.Forms.Label labelCardType;
        private System.Windows.Forms.ComboBox ComCardType;
        private System.Windows.Forms.Button BtnUpgradeCopy;
        private System.Windows.Forms.Button BtnStopUpgrade;
        private System.Windows.Forms.Label labelUpgradeState;
        private System.Windows.Forms.ProgressBar UpgradeBar;


    }
}