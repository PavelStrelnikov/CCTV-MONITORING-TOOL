namespace SDKAlarm
{
    partial class FormSDKAlarm
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
            this.labelListenIP = new System.Windows.Forms.Label();
            this.textBoxListenPort = new System.Windows.Forms.TextBox();
            this.textBoxListenIP = new System.Windows.Forms.TextBox();
            this.listViewAlarmInfo = new System.Windows.Forms.ListView();
            this.buttonListen = new System.Windows.Forms.Button();
            this.buttonRemoveAlarm = new System.Windows.Forms.Button();
            this.buttonAlarm = new System.Windows.Forms.Button();
            this.groupBoxAlarmInfo = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBoxAlarmInfo.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(210, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "Listen Port:";
            // 
            // labelListenIP
            // 
            this.labelListenIP.AutoSize = true;
            this.labelListenIP.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelListenIP.Location = new System.Drawing.Point(12, 79);
            this.labelListenIP.Name = "labelListenIP";
            this.labelListenIP.Size = new System.Drawing.Size(89, 12);
            this.labelListenIP.TabIndex = 6;
            this.labelListenIP.Text = "Listen IPAddr:";
            // 
            // textBoxListenPort
            // 
            this.textBoxListenPort.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxListenPort.Location = new System.Drawing.Point(289, 73);
            this.textBoxListenPort.Name = "textBoxListenPort";
            this.textBoxListenPort.Size = new System.Drawing.Size(100, 21);
            this.textBoxListenPort.TabIndex = 5;
            this.textBoxListenPort.Text = "7200";
            // 
            // textBoxListenIP
            // 
            this.textBoxListenIP.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxListenIP.Location = new System.Drawing.Point(104, 74);
            this.textBoxListenIP.Name = "textBoxListenIP";
            this.textBoxListenIP.Size = new System.Drawing.Size(100, 21);
            this.textBoxListenIP.TabIndex = 4;
            this.textBoxListenIP.Text = "10.8.98.11";
            // 
            // listViewAlarmInfo
            // 
            this.listViewAlarmInfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewAlarmInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewAlarmInfo.Location = new System.Drawing.Point(3, 19);
            this.listViewAlarmInfo.Name = "listViewAlarmInfo";
            this.listViewAlarmInfo.Size = new System.Drawing.Size(1019, 438);
            this.listViewAlarmInfo.TabIndex = 2;
            this.listViewAlarmInfo.UseCompatibleStateImageBehavior = false;
            this.listViewAlarmInfo.View = System.Windows.Forms.View.Details;
            // 
            // buttonListen
            // 
            this.buttonListen.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonListen.Location = new System.Drawing.Point(405, 72);
            this.buttonListen.Name = "buttonListen";
            this.buttonListen.Size = new System.Drawing.Size(85, 23);
            this.buttonListen.TabIndex = 0;
            this.buttonListen.Text = "Start Listen";
            this.buttonListen.UseVisualStyleBackColor = true;
            this.buttonListen.Click += new System.EventHandler(this.buttonListen_Click);
            // 
            // buttonRemoveAlarm
            // 
            this.buttonRemoveAlarm.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonRemoveAlarm.Location = new System.Drawing.Point(155, 79);
            this.buttonRemoveAlarm.Name = "buttonRemoveAlarm";
            this.buttonRemoveAlarm.Size = new System.Drawing.Size(90, 23);
            this.buttonRemoveAlarm.TabIndex = 9;
            this.buttonRemoveAlarm.Text = "Remove Alarm";
            this.buttonRemoveAlarm.UseVisualStyleBackColor = true;
            this.buttonRemoveAlarm.Click += new System.EventHandler(this.buttonRemoveAlarm_Click);
            // 
            // buttonAlarm
            // 
            this.buttonAlarm.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonAlarm.Location = new System.Drawing.Point(49, 79);
            this.buttonAlarm.Name = "buttonAlarm";
            this.buttonAlarm.Size = new System.Drawing.Size(88, 23);
            this.buttonAlarm.TabIndex = 8;
            this.buttonAlarm.Text = "SetUp Alarm";
            this.buttonAlarm.UseVisualStyleBackColor = true;
            this.buttonAlarm.Click += new System.EventHandler(this.buttonAlarm_Click);
            // 
            // groupBoxAlarmInfo
            // 
            this.groupBoxAlarmInfo.Controls.Add(this.listViewAlarmInfo);
            this.groupBoxAlarmInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxAlarmInfo.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxAlarmInfo.Location = new System.Drawing.Point(0, 0);
            this.groupBoxAlarmInfo.Name = "groupBoxAlarmInfo";
            this.groupBoxAlarmInfo.Size = new System.Drawing.Size(1025, 460);
            this.groupBoxAlarmInfo.TabIndex = 10;
            this.groupBoxAlarmInfo.TabStop = false;
            this.groupBoxAlarmInfo.Text = "AlarmInfo";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxListenPort);
            this.groupBox1.Controls.Add(this.textBoxListenIP);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.buttonListen);
            this.groupBox1.Controls.Add(this.labelListenIP);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.groupBox1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(524, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(501, 144);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Listen";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonRemoveAlarm);
            this.groupBox2.Controls.Add(this.buttonAlarm);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox2.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(518, 144);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Alarm";
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "报警时间";
            this.columnHeader1.Width = 181;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "报警设备";
            this.columnHeader2.Width = 246;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "报警信息";
            this.columnHeader3.Width = 524;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 460);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1025, 144);
            this.panel1.TabIndex = 13;
            // 
            // FormSDKAlarm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1025, 604);
            this.Controls.Add(this.groupBoxAlarmInfo);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "FormSDKAlarm";
            this.Text = "SDKAlarm";
            //this.Load += new System.EventHandler(this.FormSDKAlarm_Load);
            this.Shown += new System.EventHandler(this.FormSDKAlarm_Shown);
            this.groupBoxAlarmInfo.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonListen;
        private System.Windows.Forms.ListView listViewAlarmInfo;
        private System.Windows.Forms.TextBox textBoxListenIP;
        private System.Windows.Forms.TextBox textBoxListenPort;
        private System.Windows.Forms.Label labelListenIP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonRemoveAlarm;
        private System.Windows.Forms.Button buttonAlarm;
        private System.Windows.Forms.GroupBox groupBoxAlarmInfo;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Panel panel1;
    }
}