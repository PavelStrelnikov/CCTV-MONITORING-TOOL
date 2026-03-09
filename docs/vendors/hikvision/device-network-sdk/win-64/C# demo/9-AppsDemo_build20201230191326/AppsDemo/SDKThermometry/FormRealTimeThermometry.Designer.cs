namespace SDKThermometry
{
    partial class FormRealTimeThermometry
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
            this.textBoxAlarmID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxRemoteMode = new System.Windows.Forms.ComboBox();
            this.btnGetRealTimeThermometry = new System.Windows.Forms.Button();
            this.listViewRemote = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader15 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader17 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader18 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(34, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "报警ID";
            // 
            // textBoxAlarmID
            // 
            this.textBoxAlarmID.Location = new System.Drawing.Point(97, 32);
            this.textBoxAlarmID.Name = "textBoxAlarmID";
            this.textBoxAlarmID.Size = new System.Drawing.Size(100, 21);
            this.textBoxAlarmID.TabIndex = 1;
            this.textBoxAlarmID.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(255, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "长连接模式";
            // 
            // comboBoxRemoteMode
            // 
            this.comboBoxRemoteMode.FormattingEnabled = true;
            this.comboBoxRemoteMode.Items.AddRange(new object[] {
            "保留",
            "定时模式",
            "温差模式"});
            this.comboBoxRemoteMode.Location = new System.Drawing.Point(340, 32);
            this.comboBoxRemoteMode.Name = "comboBoxRemoteMode";
            this.comboBoxRemoteMode.Size = new System.Drawing.Size(121, 20);
            this.comboBoxRemoteMode.TabIndex = 2;
            // 
            // btnGetRealTimeThermometry
            // 
            this.btnGetRealTimeThermometry.Location = new System.Drawing.Point(626, 32);
            this.btnGetRealTimeThermometry.Name = "btnGetRealTimeThermometry";
            this.btnGetRealTimeThermometry.Size = new System.Drawing.Size(75, 23);
            this.btnGetRealTimeThermometry.TabIndex = 3;
            this.btnGetRealTimeThermometry.Text = "获取";
            this.btnGetRealTimeThermometry.UseVisualStyleBackColor = true;
            this.btnGetRealTimeThermometry.Click += new System.EventHandler(this.btnGetRealTimeThermometry_Click);
            // 
            // listViewRemote
            // 
            this.listViewRemote.BackColor = System.Drawing.SystemColors.ControlLight;
            this.listViewRemote.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader13,
            this.columnHeader14,
            this.columnHeader15,
            this.columnHeader16,
            this.columnHeader17,
            this.columnHeader18});
            this.listViewRemote.Location = new System.Drawing.Point(12, 107);
            this.listViewRemote.Name = "listViewRemote";
            this.listViewRemote.Size = new System.Drawing.Size(771, 368);
            this.listViewRemote.TabIndex = 4;
            this.listViewRemote.UseCompatibleStateImageBehavior = false;
            this.listViewRemote.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "相对时标";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "绝对时标";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "规则名称";
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "规则ID号";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "规则标定类型";
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "预置点号";
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "点测温";
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "当前温度";
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "区域测温";
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "最高温度";
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "最低温度";
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "平均温度";
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "温差";
            // 
            // columnHeader14
            // 
            this.columnHeader14.Text = "测温单位";
            // 
            // columnHeader15
            // 
            this.columnHeader15.Text = "数据状态类型";
            // 
            // columnHeader16
            // 
            this.columnHeader16.Text = "中心点温度";
            // 
            // columnHeader17
            // 
            this.columnHeader17.Text = "最高点温度";
            // 
            // columnHeader18
            // 
            this.columnHeader18.Text = "最低点温度";
            // 
            // FormRealTimeThermometry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(795, 497);
            this.Controls.Add(this.listViewRemote);
            this.Controls.Add(this.btnGetRealTimeThermometry);
            this.Controls.Add(this.comboBoxRemoteMode);
            this.Controls.Add(this.textBoxAlarmID);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "FormRealTimeThermometry";
            this.Text = "测温实时数据";
            this.Load += new System.EventHandler(this.FormRealTimeThermometry_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxAlarmID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxRemoteMode;
        private System.Windows.Forms.Button btnGetRealTimeThermometry;
        private System.Windows.Forms.ListView listViewRemote;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.ColumnHeader columnHeader14;
        private System.Windows.Forms.ColumnHeader columnHeader15;
        private System.Windows.Forms.ColumnHeader columnHeader16;
        private System.Windows.Forms.ColumnHeader columnHeader17;
        private System.Windows.Forms.ColumnHeader columnHeader18;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}