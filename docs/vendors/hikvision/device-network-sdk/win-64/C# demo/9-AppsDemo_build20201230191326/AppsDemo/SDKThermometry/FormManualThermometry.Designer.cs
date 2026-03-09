namespace SDKThermometry
{
    partial class FormManualThermometry
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
            this.btnSetManuakThermBasic = new System.Windows.Forms.Button();
            this.btnGetManuakThermBasic = new System.Windows.Forms.Button();
            this.textBoxManualEmissivity = new System.Windows.Forms.TextBox();
            this.textBoxManualDistance = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.listViewManualTherm = new System.Windows.Forms.ListView();
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader15 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSetManualThermometry = new System.Windows.Forms.Button();
            this.btnDelconlumn = new System.Windows.Forms.Button();
            this.btnAddConlum = new System.Windows.Forms.Button();
            this.btnDelRule = new System.Windows.Forms.Button();
            this.comboBoxRemoteMode = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnGetManualThermometry = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.comboBoxManualRuleCalibType = new System.Windows.Forms.ComboBox();
            this.checkBoxManualEnable = new System.Windows.Forms.CheckBox();
            this.textBoxManualRuleName = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxManualRuleID = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.pictureBoxPlay = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPlay)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox1.Controls.Add(this.btnSetManuakThermBasic);
            this.groupBox1.Controls.Add(this.btnGetManuakThermBasic);
            this.groupBox1.Controls.Add(this.textBoxManualEmissivity);
            this.groupBox1.Controls.Add(this.textBoxManualDistance);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(39, 25);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(313, 172);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "手动测温基本参数配置";
            // 
            // btnSetManuakThermBasic
            // 
            this.btnSetManuakThermBasic.Location = new System.Drawing.Point(165, 134);
            this.btnSetManuakThermBasic.Name = "btnSetManuakThermBasic";
            this.btnSetManuakThermBasic.Size = new System.Drawing.Size(75, 23);
            this.btnSetManuakThermBasic.TabIndex = 2;
            this.btnSetManuakThermBasic.Text = "设置";
            this.btnSetManuakThermBasic.UseVisualStyleBackColor = true;
            this.btnSetManuakThermBasic.Click += new System.EventHandler(this.btnSetManuakThermBasic_Click);
            // 
            // btnGetManuakThermBasic
            // 
            this.btnGetManuakThermBasic.Location = new System.Drawing.Point(28, 134);
            this.btnGetManuakThermBasic.Name = "btnGetManuakThermBasic";
            this.btnGetManuakThermBasic.Size = new System.Drawing.Size(75, 23);
            this.btnGetManuakThermBasic.TabIndex = 2;
            this.btnGetManuakThermBasic.Text = "获取";
            this.btnGetManuakThermBasic.UseVisualStyleBackColor = true;
            this.btnGetManuakThermBasic.Click += new System.EventHandler(this.btnGetManuakThermBasic_Click);
            // 
            // textBoxManualEmissivity
            // 
            this.textBoxManualEmissivity.Location = new System.Drawing.Point(140, 86);
            this.textBoxManualEmissivity.Name = "textBoxManualEmissivity";
            this.textBoxManualEmissivity.Size = new System.Drawing.Size(100, 21);
            this.textBoxManualEmissivity.TabIndex = 1;
            this.textBoxManualEmissivity.Text = "0";
            // 
            // textBoxManualDistance
            // 
            this.textBoxManualDistance.Location = new System.Drawing.Point(140, 38);
            this.textBoxManualDistance.Name = "textBoxManualDistance";
            this.textBoxManualDistance.Size = new System.Drawing.Size(100, 21);
            this.textBoxManualDistance.TabIndex = 1;
            this.textBoxManualDistance.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "辐射率";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "距离";
            // 
            // listViewManualTherm
            // 
            this.listViewManualTherm.BackColor = System.Drawing.SystemColors.ControlLight;
            this.listViewManualTherm.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader16,
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader14,
            this.columnHeader15,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader13});
            this.listViewManualTherm.FullRowSelect = true;
            this.listViewManualTherm.Location = new System.Drawing.Point(17, 37);
            this.listViewManualTherm.Name = "listViewManualTherm";
            this.listViewManualTherm.Size = new System.Drawing.Size(821, 339);
            this.listViewManualTherm.TabIndex = 5;
            this.listViewManualTherm.UseCompatibleStateImageBehavior = false;
            this.listViewManualTherm.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader16
            // 
            this.columnHeader16.Text = "序号";
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
            // columnHeader14
            // 
            this.columnHeader14.Text = "测温单位";
            // 
            // columnHeader15
            // 
            this.columnHeader15.Text = "数据状态类型";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "规则标定类型";
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "使能";
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
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox2.Controls.Add(this.listViewManualTherm);
            this.groupBox2.Location = new System.Drawing.Point(400, 252);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(844, 382);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "手动测温实时数据";
            // 
            // btnSetManualThermometry
            // 
            this.btnSetManualThermometry.Location = new System.Drawing.Point(139, 134);
            this.btnSetManualThermometry.Name = "btnSetManualThermometry";
            this.btnSetManualThermometry.Size = new System.Drawing.Size(55, 23);
            this.btnSetManualThermometry.TabIndex = 11;
            this.btnSetManualThermometry.Text = "设置";
            this.btnSetManualThermometry.UseVisualStyleBackColor = true;
            this.btnSetManualThermometry.Click += new System.EventHandler(this.btnSetManualThermometry_Click);
            // 
            // btnDelconlumn
            // 
            this.btnDelconlumn.Location = new System.Drawing.Point(466, 134);
            this.btnDelconlumn.Name = "btnDelconlumn";
            this.btnDelconlumn.Size = new System.Drawing.Size(55, 23);
            this.btnDelconlumn.TabIndex = 11;
            this.btnDelconlumn.Text = "删除列";
            this.btnDelconlumn.UseVisualStyleBackColor = true;
            this.btnDelconlumn.Click += new System.EventHandler(this.btnDelconlumn_Click);
            // 
            // btnAddConlum
            // 
            this.btnAddConlum.Location = new System.Drawing.Point(354, 134);
            this.btnAddConlum.Name = "btnAddConlum";
            this.btnAddConlum.Size = new System.Drawing.Size(59, 23);
            this.btnAddConlum.TabIndex = 11;
            this.btnAddConlum.Text = "添加列";
            this.btnAddConlum.UseVisualStyleBackColor = true;
            this.btnAddConlum.Click += new System.EventHandler(this.btnAddConlum_Click);
            // 
            // btnDelRule
            // 
            this.btnDelRule.Location = new System.Drawing.Point(243, 134);
            this.btnDelRule.Name = "btnDelRule";
            this.btnDelRule.Size = new System.Drawing.Size(61, 23);
            this.btnDelRule.TabIndex = 11;
            this.btnDelRule.Text = "删除规则";
            this.btnDelRule.UseVisualStyleBackColor = true;
            this.btnDelRule.Click += new System.EventHandler(this.btnDelRule_Click);
            // 
            // comboBoxRemoteMode
            // 
            this.comboBoxRemoteMode.FormattingEnabled = true;
            this.comboBoxRemoteMode.Items.AddRange(new object[] {
            "保留",
            "定时模式",
            "温差模式"});
            this.comboBoxRemoteMode.Location = new System.Drawing.Point(421, 78);
            this.comboBoxRemoteMode.Name = "comboBoxRemoteMode";
            this.comboBoxRemoteMode.Size = new System.Drawing.Size(100, 20);
            this.comboBoxRemoteMode.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(302, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "长连接模式";
            // 
            // btnGetManualThermometry
            // 
            this.btnGetManualThermometry.Location = new System.Drawing.Point(24, 134);
            this.btnGetManualThermometry.Name = "btnGetManualThermometry";
            this.btnGetManualThermometry.Size = new System.Drawing.Size(61, 23);
            this.btnGetManualThermometry.TabIndex = 6;
            this.btnGetManualThermometry.Text = "获取";
            this.btnGetManualThermometry.UseVisualStyleBackColor = true;
            this.btnGetManualThermometry.Click += new System.EventHandler(this.btnGetManualThermometry_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox3.Controls.Add(this.btnSetManualThermometry);
            this.groupBox3.Controls.Add(this.btnDelconlumn);
            this.groupBox3.Controls.Add(this.comboBoxManualRuleCalibType);
            this.groupBox3.Controls.Add(this.btnAddConlum);
            this.groupBox3.Controls.Add(this.checkBoxManualEnable);
            this.groupBox3.Controls.Add(this.btnDelRule);
            this.groupBox3.Controls.Add(this.textBoxManualRuleName);
            this.groupBox3.Controls.Add(this.btnGetManualThermometry);
            this.groupBox3.Controls.Add(this.comboBoxRemoteMode);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.textBoxManualRuleID);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Location = new System.Drawing.Point(417, 25);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(555, 172);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "手动测温数据设置";
            // 
            // comboBoxManualRuleCalibType
            // 
            this.comboBoxManualRuleCalibType.FormattingEnabled = true;
            this.comboBoxManualRuleCalibType.Items.AddRange(new object[] {
            "点",
            "线",
            "框"});
            this.comboBoxManualRuleCalibType.Location = new System.Drawing.Point(139, 78);
            this.comboBoxManualRuleCalibType.Name = "comboBoxManualRuleCalibType";
            this.comboBoxManualRuleCalibType.Size = new System.Drawing.Size(84, 20);
            this.comboBoxManualRuleCalibType.TabIndex = 3;
            this.comboBoxManualRuleCalibType.SelectedIndexChanged += new System.EventHandler(this.comboBoxManualRuleCalibType_SelectedIndexChanged);
            // 
            // checkBoxManualEnable
            // 
            this.checkBoxManualEnable.AutoSize = true;
            this.checkBoxManualEnable.Location = new System.Drawing.Point(232, 34);
            this.checkBoxManualEnable.Name = "checkBoxManualEnable";
            this.checkBoxManualEnable.Size = new System.Drawing.Size(48, 16);
            this.checkBoxManualEnable.TabIndex = 2;
            this.checkBoxManualEnable.Text = "启用";
            this.checkBoxManualEnable.UseVisualStyleBackColor = true;
            // 
            // textBoxManualRuleName
            // 
            this.textBoxManualRuleName.Location = new System.Drawing.Point(421, 29);
            this.textBoxManualRuleName.Name = "textBoxManualRuleName";
            this.textBoxManualRuleName.Size = new System.Drawing.Size(100, 21);
            this.textBoxManualRuleName.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(22, 86);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 12);
            this.label7.TabIndex = 0;
            this.label7.Text = "规则标定类型";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(314, 34);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 0;
            this.label6.Text = "规则名称";
            // 
            // textBoxManualRuleID
            // 
            this.textBoxManualRuleID.Location = new System.Drawing.Point(99, 29);
            this.textBoxManualRuleID.Name = "textBoxManualRuleID";
            this.textBoxManualRuleID.Size = new System.Drawing.Size(100, 21);
            this.textBoxManualRuleID.TabIndex = 1;
            this.textBoxManualRuleID.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 38);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "规则ID";
            // 
            // pictureBoxPlay
            // 
            this.pictureBoxPlay.BackColor = System.Drawing.SystemColors.ControlLight;
            this.pictureBoxPlay.Location = new System.Drawing.Point(39, 252);
            this.pictureBoxPlay.Name = "pictureBoxPlay";
            this.pictureBoxPlay.Size = new System.Drawing.Size(313, 376);
            this.pictureBoxPlay.TabIndex = 8;
            this.pictureBoxPlay.TabStop = false;
            this.pictureBoxPlay.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBoxPlay_MouseDown);
            // 
            // FormManualThermometry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1301, 646);
            this.Controls.Add(this.pictureBoxPlay);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "FormManualThermometry";
            this.Text = "手动测温";
            this.Load += new System.EventHandler(this.FormManualThermometry_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPlay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSetManuakThermBasic;
        private System.Windows.Forms.Button btnGetManuakThermBasic;
        private System.Windows.Forms.TextBox textBoxManualEmissivity;
        private System.Windows.Forms.TextBox textBoxManualDistance;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listViewManualTherm;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.ColumnHeader columnHeader14;
        private System.Windows.Forms.ColumnHeader columnHeader15;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnGetManualThermometry;
        private System.Windows.Forms.ComboBox comboBoxRemoteMode;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSetManualThermometry;
        private System.Windows.Forms.Button btnDelconlumn;
        private System.Windows.Forms.Button btnAddConlum;
        private System.Windows.Forms.Button btnDelRule;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboBoxManualRuleCalibType;
        private System.Windows.Forms.CheckBox checkBoxManualEnable;
        private System.Windows.Forms.TextBox textBoxManualRuleName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxManualRuleID;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ColumnHeader columnHeader16;
        private System.Windows.Forms.PictureBox pictureBoxPlay;
    }
}