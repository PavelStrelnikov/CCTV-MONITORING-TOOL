namespace SDKAlarm
{
    partial class FormSDKIOAlarm
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
            this.m_textBoxIPAlarmIn = new System.Windows.Forms.TextBox();
            this.groupBoxChannelList = new System.Windows.Forms.GroupBox();
            this.m_listViewChannel = new System.Windows.Forms.ListView();
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxAlarmIn = new System.Windows.Forms.GroupBox();
            this.m_listViewAlarmIn = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxAlarmInfo = new System.Windows.Forms.GroupBox();
            this.m_listViewAlarmInfo = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonRemoveAlarm = new System.Windows.Forms.Button();
            this.buttonAlarm = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBoxChannelList.SuspendLayout();
            this.groupBoxAlarmIn.SuspendLayout();
            this.groupBoxAlarmInfo.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_textBoxIPAlarmIn
            // 
            this.m_textBoxIPAlarmIn.Location = new System.Drawing.Point(115, 18);
            this.m_textBoxIPAlarmIn.Name = "m_textBoxIPAlarmIn";
            this.m_textBoxIPAlarmIn.Size = new System.Drawing.Size(155, 21);
            this.m_textBoxIPAlarmIn.TabIndex = 30;
            // 
            // groupBoxChannelList
            // 
            this.groupBoxChannelList.Controls.Add(this.m_listViewChannel);
            this.groupBoxChannelList.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxChannelList.Location = new System.Drawing.Point(384, 321);
            this.groupBoxChannelList.Name = "groupBoxChannelList";
            this.groupBoxChannelList.Size = new System.Drawing.Size(392, 289);
            this.groupBoxChannelList.TabIndex = 28;
            this.groupBoxChannelList.TabStop = false;
            this.groupBoxChannelList.Text = "IPChannelList";
            // 
            // m_listViewChannel
            // 
            this.m_listViewChannel.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9});
            this.m_listViewChannel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_listViewChannel.Location = new System.Drawing.Point(3, 19);
            this.m_listViewChannel.Name = "m_listViewChannel";
            this.m_listViewChannel.Size = new System.Drawing.Size(386, 267);
            this.m_listViewChannel.TabIndex = 2;
            this.m_listViewChannel.UseCompatibleStateImageBehavior = false;
            this.m_listViewChannel.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "索引";
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "IP设备ID";
            this.columnHeader8.Width = 160;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "通道号";
            this.columnHeader9.Width = 160;
            // 
            // groupBoxAlarmIn
            // 
            this.groupBoxAlarmIn.Controls.Add(this.m_listViewAlarmIn);
            this.groupBoxAlarmIn.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxAlarmIn.Location = new System.Drawing.Point(13, 321);
            this.groupBoxAlarmIn.Name = "groupBoxAlarmIn";
            this.groupBoxAlarmIn.Size = new System.Drawing.Size(365, 289);
            this.groupBoxAlarmIn.TabIndex = 27;
            this.groupBoxAlarmIn.TabStop = false;
            this.groupBoxAlarmIn.Text = "IPAlarmInList";
            // 
            // m_listViewAlarmIn
            // 
            this.m_listViewAlarmIn.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.m_listViewAlarmIn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_listViewAlarmIn.Location = new System.Drawing.Point(3, 19);
            this.m_listViewAlarmIn.Name = "m_listViewAlarmIn";
            this.m_listViewAlarmIn.Size = new System.Drawing.Size(359, 267);
            this.m_listViewAlarmIn.TabIndex = 2;
            this.m_listViewAlarmIn.UseCompatibleStateImageBehavior = false;
            this.m_listViewAlarmIn.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "索引";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "IP设备ID";
            this.columnHeader5.Width = 150;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "报警输入口 ";
            this.columnHeader6.Width = 150;
            // 
            // groupBoxAlarmInfo
            // 
            this.groupBoxAlarmInfo.Controls.Add(this.m_listViewAlarmInfo);
            this.groupBoxAlarmInfo.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxAlarmInfo.Location = new System.Drawing.Point(13, 12);
            this.groupBoxAlarmInfo.Name = "groupBoxAlarmInfo";
            this.groupBoxAlarmInfo.Size = new System.Drawing.Size(757, 204);
            this.groupBoxAlarmInfo.TabIndex = 26;
            this.groupBoxAlarmInfo.TabStop = false;
            this.groupBoxAlarmInfo.Text = "AlarmInfo";
            // 
            // m_listViewAlarmInfo
            // 
            this.m_listViewAlarmInfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.m_listViewAlarmInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_listViewAlarmInfo.Location = new System.Drawing.Point(3, 19);
            this.m_listViewAlarmInfo.Name = "m_listViewAlarmInfo";
            this.m_listViewAlarmInfo.Size = new System.Drawing.Size(751, 182);
            this.m_listViewAlarmInfo.TabIndex = 2;
            this.m_listViewAlarmInfo.UseCompatibleStateImageBehavior = false;
            this.m_listViewAlarmInfo.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "报警时间";
            this.columnHeader1.Width = 160;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "报警设备";
            this.columnHeader2.Width = 200;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "报警信息";
            this.columnHeader3.Width = 370;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 29;
            this.label1.Text = "IP通道起始IO口号";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonRemoveAlarm);
            this.groupBox2.Controls.Add(this.buttonAlarm);
            this.groupBox2.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox2.Location = new System.Drawing.Point(508, 237);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(260, 65);
            this.groupBox2.TabIndex = 25;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Alarm";
            // 
            // buttonRemoveAlarm
            // 
            this.buttonRemoveAlarm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.buttonRemoveAlarm.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonRemoveAlarm.Location = new System.Drawing.Point(134, 25);
            this.buttonRemoveAlarm.Name = "buttonRemoveAlarm";
            this.buttonRemoveAlarm.Size = new System.Drawing.Size(90, 23);
            this.buttonRemoveAlarm.TabIndex = 9;
            this.buttonRemoveAlarm.Text = "Remove Alarm";
            this.buttonRemoveAlarm.UseVisualStyleBackColor = false;
            this.buttonRemoveAlarm.Click += new System.EventHandler(this.buttonRemoveAlarm_Click);
            // 
            // buttonAlarm
            // 
            this.buttonAlarm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.buttonAlarm.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonAlarm.Location = new System.Drawing.Point(28, 25);
            this.buttonAlarm.Name = "buttonAlarm";
            this.buttonAlarm.Size = new System.Drawing.Size(88, 23);
            this.buttonAlarm.TabIndex = 8;
            this.buttonAlarm.Text = "SetUp Alarm";
            this.buttonAlarm.UseVisualStyleBackColor = false;
            this.buttonAlarm.Click += new System.EventHandler(this.buttonAlarm_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.m_textBoxIPAlarmIn);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(19, 240);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(280, 48);
            this.groupBox1.TabIndex = 31;
            this.groupBox1.TabStop = false;
            // 
            // FormSDKIOAlarm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(782, 659);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxChannelList);
            this.Controls.Add(this.groupBoxAlarmIn);
            this.Controls.Add(this.groupBoxAlarmInfo);
            this.Controls.Add(this.groupBox2);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "FormSDKIOAlarm";
            this.Text = "FormSDKIOAlarm";
            this.groupBoxChannelList.ResumeLayout(false);
            this.groupBoxAlarmIn.ResumeLayout(false);
            this.groupBoxAlarmInfo.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox m_textBoxIPAlarmIn;
        private System.Windows.Forms.GroupBox groupBoxChannelList;
        private System.Windows.Forms.ListView m_listViewChannel;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.GroupBox groupBoxAlarmIn;
        private System.Windows.Forms.ListView m_listViewAlarmIn;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.GroupBox groupBoxAlarmInfo;
        private System.Windows.Forms.ListView m_listViewAlarmInfo;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonRemoveAlarm;
        private System.Windows.Forms.Button buttonAlarm;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}