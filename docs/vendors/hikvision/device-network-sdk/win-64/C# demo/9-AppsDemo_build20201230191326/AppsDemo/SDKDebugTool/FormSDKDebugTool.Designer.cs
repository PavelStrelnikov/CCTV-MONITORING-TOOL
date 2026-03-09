namespace SDKDebugTool
{
    partial class FormSDKDebugTool
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSDKDebugTool));
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.UrltBox = new System.Windows.Forms.ComboBox();
            this.ExecuteBtn = new System.Windows.Forms.Button();
            this.MethodcBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.nboundDataTBox = new System.Windows.Forms.GroupBox();
            this.InboundDatarTBox = new System.Windows.Forms.RichTextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.OutboundDataTBox = new System.Windows.Forms.RichTextBox();
            this.groupBoxWork = new System.Windows.Forms.GroupBox();
            this.panel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.nboundDataTBox.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBoxWork.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(3, 17);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1062, 67);
            this.panel2.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.groupBox1.Controls.Add(this.UrltBox);
            this.groupBox1.Controls.Add(this.ExecuteBtn);
            this.groupBox1.Controls.Add(this.MethodcBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1062, 67);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Please select the option you need";
            // 
            // UrltBox
            // 
            this.UrltBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UrltBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.UrltBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.UrltBox.FormattingEnabled = true;
            this.UrltBox.Location = new System.Drawing.Point(58, 27);
            this.UrltBox.Name = "UrltBox";
            this.UrltBox.Size = new System.Drawing.Size(619, 23);
            this.UrltBox.TabIndex = 11;
            // 
            // ExecuteBtn
            // 
            this.ExecuteBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ExecuteBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ExecuteBtn.BackgroundImage")));
            this.ExecuteBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ExecuteBtn.ForeColor = System.Drawing.Color.White;
            this.ExecuteBtn.Location = new System.Drawing.Point(951, 22);
            this.ExecuteBtn.Name = "ExecuteBtn";
            this.ExecuteBtn.Size = new System.Drawing.Size(102, 29);
            this.ExecuteBtn.TabIndex = 7;
            this.ExecuteBtn.Text = "Execute";
            this.ExecuteBtn.UseVisualStyleBackColor = true;
            this.ExecuteBtn.Click += new System.EventHandler(this.ExecuteBtn_Click);
            // 
            // MethodcBox
            // 
            this.MethodcBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MethodcBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.MethodcBox.FormattingEnabled = true;
            this.MethodcBox.Items.AddRange(new object[] {
            "GET",
            "PUT",
            "POST",
            "DELETE"});
            this.MethodcBox.Location = new System.Drawing.Point(759, 27);
            this.MethodcBox.Name = "MethodcBox";
            this.MethodcBox.Size = new System.Drawing.Size(121, 23);
            this.MethodcBox.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(740, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(12, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "*";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(687, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 15);
            this.label4.TabIndex = 3;
            this.label4.Text = "Method";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(40, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(12, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "*";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "URL";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 84);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.nboundDataTBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox3);
            this.splitContainer1.Size = new System.Drawing.Size(1062, 512);
            this.splitContainer1.SplitterDistance = 513;
            this.splitContainer1.TabIndex = 3;
            // 
            // nboundDataTBox
            // 
            this.nboundDataTBox.Controls.Add(this.InboundDatarTBox);
            this.nboundDataTBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nboundDataTBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nboundDataTBox.Location = new System.Drawing.Point(0, 0);
            this.nboundDataTBox.Name = "nboundDataTBox";
            this.nboundDataTBox.Size = new System.Drawing.Size(513, 512);
            this.nboundDataTBox.TabIndex = 0;
            this.nboundDataTBox.TabStop = false;
            this.nboundDataTBox.Text = "Inbound data";
            // 
            // InboundDatarTBox
            // 
            this.InboundDatarTBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.InboundDatarTBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InboundDatarTBox.Location = new System.Drawing.Point(3, 17);
            this.InboundDatarTBox.Name = "InboundDatarTBox";
            this.InboundDatarTBox.Size = new System.Drawing.Size(507, 492);
            this.InboundDatarTBox.TabIndex = 1;
            this.InboundDatarTBox.Text = "";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.OutboundDataTBox);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(545, 512);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Outbound data";
            // 
            // OutboundDataTBox
            // 
            this.OutboundDataTBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.OutboundDataTBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OutboundDataTBox.Location = new System.Drawing.Point(3, 17);
            this.OutboundDataTBox.Name = "OutboundDataTBox";
            this.OutboundDataTBox.Size = new System.Drawing.Size(539, 492);
            this.OutboundDataTBox.TabIndex = 1;
            this.OutboundDataTBox.Text = "";
            // 
            // groupBoxWork
            // 
            this.groupBoxWork.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.groupBoxWork.Controls.Add(this.splitContainer1);
            this.groupBoxWork.Controls.Add(this.panel2);
            this.groupBoxWork.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxWork.Location = new System.Drawing.Point(0, 0);
            this.groupBoxWork.Name = "groupBoxWork";
            this.groupBoxWork.Size = new System.Drawing.Size(1068, 599);
            this.groupBoxWork.TabIndex = 2;
            this.groupBoxWork.TabStop = false;
            // 
            // FormSDKDebugTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1068, 599);
            this.Controls.Add(this.groupBoxWork);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormSDKDebugTool";
            this.Text = "Debug Tool For ISAPI";
            this.Load += new System.EventHandler(this.FormSDKDebugTool_Load);
            this.panel2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.nboundDataTBox.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBoxWork.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBoxWork;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button ExecuteBtn;
        private System.Windows.Forms.ComboBox MethodcBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox nboundDataTBox;
        private System.Windows.Forms.RichTextBox InboundDatarTBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RichTextBox OutboundDataTBox;
        private System.Windows.Forms.ComboBox UrltBox;
    }
}