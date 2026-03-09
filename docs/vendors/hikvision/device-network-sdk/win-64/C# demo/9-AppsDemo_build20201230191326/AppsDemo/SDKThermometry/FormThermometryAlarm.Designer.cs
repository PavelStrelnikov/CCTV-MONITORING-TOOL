namespace SDKThermometry
{
    partial class FormThermometryAlarm
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
            this.groupBoxAlarmInfo = new System.Windows.Forms.GroupBox();
            this.listViewAlarmInfo = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBoxListenPort = new System.Windows.Forms.TextBox();
            this.textBoxListenIP = new System.Windows.Forms.TextBox();
            this.btnThermometryListen = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.labelListenIP = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnStopThermometryAlarm = new System.Windows.Forms.Button();
            this.btnThermometryAlarm = new System.Windows.Forms.Button();
            this.groupBoxAlarmInfo.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxAlarmInfo
            // 
            this.groupBoxAlarmInfo.Controls.Add(this.listViewAlarmInfo);
            this.groupBoxAlarmInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxAlarmInfo.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxAlarmInfo.Location = new System.Drawing.Point(0, 0);
            this.groupBoxAlarmInfo.Name = "groupBoxAlarmInfo";
            this.groupBoxAlarmInfo.Size = new System.Drawing.Size(1072, 502);
            this.groupBoxAlarmInfo.TabIndex = 14;
            this.groupBoxAlarmInfo.TabStop = false;
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
            this.listViewAlarmInfo.Size = new System.Drawing.Size(1066, 480);
            this.listViewAlarmInfo.TabIndex = 2;
            this.listViewAlarmInfo.UseCompatibleStateImageBehavior = false;
            this.listViewAlarmInfo.View = System.Windows.Forms.View.Details;
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
            this.panel1.Location = new System.Drawing.Point(0, 502);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1072, 144);
            this.panel1.TabIndex = 15;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox1.Controls.Add(this.textBoxListenPort);
            this.groupBox1.Controls.Add(this.textBoxListenIP);
            this.groupBox1.Controls.Add(this.btnThermometryListen);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.labelListenIP);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.groupBox1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(546, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(526, 144);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "监听";
            // 
            // textBoxListenPort
            // 
            this.textBoxListenPort.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxListenPort.Location = new System.Drawing.Point(258, 74);
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
            this.textBoxListenIP.Text = "10.8.98.28";
            // 
            // btnThermometryListen
            // 
            this.btnThermometryListen.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnThermometryListen.Location = new System.Drawing.Point(387, 74);
            this.btnThermometryListen.Name = "btnThermometryListen";
            this.btnThermometryListen.Size = new System.Drawing.Size(85, 23);
            this.btnThermometryListen.TabIndex = 0;
            this.btnThermometryListen.Text = "监听";
            this.btnThermometryListen.UseVisualStyleBackColor = true;
            this.btnThermometryListen.Click += new System.EventHandler(this.btnThermometryListen_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(210, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "端口";
            // 
            // labelListenIP
            // 
            this.labelListenIP.AutoSize = true;
            this.labelListenIP.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelListenIP.Location = new System.Drawing.Point(12, 79);
            this.labelListenIP.Name = "labelListenIP";
            this.labelListenIP.Size = new System.Drawing.Size(77, 12);
            this.labelListenIP.TabIndex = 6;
            this.labelListenIP.Text = "报警主机地址";
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox2.Controls.Add(this.btnStopThermometryAlarm);
            this.groupBox2.Controls.Add(this.btnThermometryAlarm);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox2.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(417, 144);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "报警";
            // 
            // btnStopThermometryAlarm
            // 
            this.btnStopThermometryAlarm.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStopThermometryAlarm.Location = new System.Drawing.Point(238, 65);
            this.btnStopThermometryAlarm.Name = "btnStopThermometryAlarm";
            this.btnStopThermometryAlarm.Size = new System.Drawing.Size(90, 23);
            this.btnStopThermometryAlarm.TabIndex = 9;
            this.btnStopThermometryAlarm.Text = "撤防";
            this.btnStopThermometryAlarm.UseVisualStyleBackColor = true;
            this.btnStopThermometryAlarm.Click += new System.EventHandler(this.btnStopThermometryAlarm_Click);
            // 
            // btnThermometryAlarm
            // 
            this.btnThermometryAlarm.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnThermometryAlarm.Location = new System.Drawing.Point(81, 65);
            this.btnThermometryAlarm.Name = "btnThermometryAlarm";
            this.btnThermometryAlarm.Size = new System.Drawing.Size(88, 23);
            this.btnThermometryAlarm.TabIndex = 8;
            this.btnThermometryAlarm.Text = "布防";
            this.btnThermometryAlarm.UseVisualStyleBackColor = true;
            this.btnThermometryAlarm.Click += new System.EventHandler(this.btnThermometryAlarm_Click);
            // 
            // FormThermometryAlarm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1072, 646);
            this.Controls.Add(this.groupBoxAlarmInfo);
            this.Controls.Add(this.panel1);
            this.Name = "FormThermometryAlarm";
            this.Text = "测温报警";
            this.groupBoxAlarmInfo.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxAlarmInfo;
        private System.Windows.Forms.ListView listViewAlarmInfo;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnThermometryListen;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnStopThermometryAlarm;
        private System.Windows.Forms.Button btnThermometryAlarm;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBoxListenPort;
        private System.Windows.Forms.TextBox textBoxListenIP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelListenIP;
    }
}