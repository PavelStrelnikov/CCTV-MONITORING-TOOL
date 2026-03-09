namespace SDKPerimeterPrecaution
{
    partial class FormPerimeterPrecautionAlarm_SDK
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
            this.listViewAlarmInfoShow = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxAlarmInfo = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonRemoveAlarm = new System.Windows.Forms.Button();
            this.buttonAlarm = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonListen = new System.Windows.Forms.Button();
            this.textBoxListenPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxListenIP = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBoxAlarmInfo.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewAlarmInfoShow
            // 
            this.listViewAlarmInfoShow.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewAlarmInfoShow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewAlarmInfoShow.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listViewAlarmInfoShow.Location = new System.Drawing.Point(3, 17);
            this.listViewAlarmInfoShow.Name = "listViewAlarmInfoShow";
            this.listViewAlarmInfoShow.Size = new System.Drawing.Size(973, 433);
            this.listViewAlarmInfoShow.TabIndex = 0;
            this.listViewAlarmInfoShow.UseCompatibleStateImageBehavior = false;
            this.listViewAlarmInfoShow.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Time";
            this.columnHeader1.Width = 180;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Device";
            this.columnHeader2.Width = 250;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Info";
            this.columnHeader3.Width = 450;
            // 
            // groupBoxAlarmInfo
            // 
            this.groupBoxAlarmInfo.Controls.Add(this.listViewAlarmInfoShow);
            this.groupBoxAlarmInfo.Location = new System.Drawing.Point(5, 1);
            this.groupBoxAlarmInfo.Name = "groupBoxAlarmInfo";
            this.groupBoxAlarmInfo.Size = new System.Drawing.Size(979, 453);
            this.groupBoxAlarmInfo.TabIndex = 1;
            this.groupBoxAlarmInfo.TabStop = false;
            this.groupBoxAlarmInfo.Text = "AlarmInfo";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonRemoveAlarm);
            this.groupBox1.Controls.Add(this.buttonAlarm);
            this.groupBox1.Location = new System.Drawing.Point(8, 460);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(463, 144);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Alarm";
            // 
            // buttonRemoveAlarm
            // 
            this.buttonRemoveAlarm.Location = new System.Drawing.Point(266, 68);
            this.buttonRemoveAlarm.Name = "buttonRemoveAlarm";
            this.buttonRemoveAlarm.Size = new System.Drawing.Size(107, 31);
            this.buttonRemoveAlarm.TabIndex = 1;
            this.buttonRemoveAlarm.Text = "Remove Alarm";
            this.buttonRemoveAlarm.UseVisualStyleBackColor = true;
            this.buttonRemoveAlarm.Click += new System.EventHandler(this.buttonRemoveAlarm_Click);
            // 
            // buttonAlarm
            // 
            this.buttonAlarm.Location = new System.Drawing.Point(72, 68);
            this.buttonAlarm.Name = "buttonAlarm";
            this.buttonAlarm.Size = new System.Drawing.Size(99, 31);
            this.buttonAlarm.TabIndex = 0;
            this.buttonAlarm.Text = "SetUp Alarm";
            this.buttonAlarm.UseVisualStyleBackColor = true;
            this.buttonAlarm.Click += new System.EventHandler(this.buttonAlarm_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonListen);
            this.groupBox2.Controls.Add(this.textBoxListenPort);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.textBoxListenIP);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(477, 460);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(504, 144);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Listen";
            // 
            // buttonListen
            // 
            this.buttonListen.Location = new System.Drawing.Point(303, 70);
            this.buttonListen.Name = "buttonListen";
            this.buttonListen.Size = new System.Drawing.Size(107, 31);
            this.buttonListen.TabIndex = 4;
            this.buttonListen.Text = "Start Listen";
            this.buttonListen.UseVisualStyleBackColor = true;
            this.buttonListen.Click += new System.EventHandler(this.buttonListen_Click);
            // 
            // textBoxListenPort
            // 
            this.textBoxListenPort.Location = new System.Drawing.Point(147, 76);
            this.textBoxListenPort.Name = "textBoxListenPort";
            this.textBoxListenPort.Size = new System.Drawing.Size(68, 21);
            this.textBoxListenPort.TabIndex = 3;
            this.textBoxListenPort.Text = "7200";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(63, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "Listen Port:";
            // 
            // textBoxListenIP
            // 
            this.textBoxListenIP.Location = new System.Drawing.Point(147, 32);
            this.textBoxListenIP.Name = "textBoxListenIP";
            this.textBoxListenIP.Size = new System.Drawing.Size(100, 21);
            this.textBoxListenIP.TabIndex = 1;
            this.textBoxListenIP.Text = "10.8.98.123";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(63, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Listen IP:";
            // 
            // FormPerimeterPrecautionAlarm_SDK
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(990, 616);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxAlarmInfo);
            this.Name = "FormPerimeterPrecautionAlarm_SDK";
            this.Text = "FormPerimeterPrecautionAlarm_SDK";
            this.groupBoxAlarmInfo.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewAlarmInfoShow;
        private System.Windows.Forms.GroupBox groupBoxAlarmInfo;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonRemoveAlarm;
        private System.Windows.Forms.Button buttonAlarm;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxListenPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxListenIP;
        private System.Windows.Forms.Button buttonListen;
    }
}