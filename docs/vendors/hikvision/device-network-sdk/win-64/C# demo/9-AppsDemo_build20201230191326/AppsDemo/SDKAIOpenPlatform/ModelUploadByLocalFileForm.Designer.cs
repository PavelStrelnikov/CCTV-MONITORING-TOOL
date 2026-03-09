namespace SDKAIOpenPlatform
{
    partial class ModelUploadByLocalFileForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModelUploadByLocalFileForm));
            this.label6 = new System.Windows.Forms.Label();
            this.ParamFilePathtBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.UploadParamFileBtn = new System.Windows.Forms.Button();
            this.UploadModelFileBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ModelFilePathtBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ModelUploadBtn = new System.Windows.Forms.Button();
            this.paramOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.modelOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.uploadstateLab = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.MPID = new System.Windows.Forms.Label();
            this.MPIDtextBox = new System.Windows.Forms.TextBox();
            this.MPNametextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.Red;
            this.label6.Location = new System.Drawing.Point(66, 101);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(11, 12);
            this.label6.TabIndex = 16;
            this.label6.Text = "*";
            // 
            // ParamFilePathtBox
            // 
            this.ParamFilePathtBox.Location = new System.Drawing.Point(93, 98);
            this.ParamFilePathtBox.Name = "ParamFilePathtBox";
            this.ParamFilePathtBox.Size = new System.Drawing.Size(328, 21);
            this.ParamFilePathtBox.TabIndex = 15;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 14;
            this.label2.Text = "参数文件";
            // 
            // UploadParamFileBtn
            // 
            this.UploadParamFileBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("UploadParamFileBtn.BackgroundImage")));
            this.UploadParamFileBtn.ForeColor = System.Drawing.Color.White;
            this.UploadParamFileBtn.Location = new System.Drawing.Point(453, 96);
            this.UploadParamFileBtn.Name = "UploadParamFileBtn";
            this.UploadParamFileBtn.Size = new System.Drawing.Size(94, 23);
            this.UploadParamFileBtn.TabIndex = 26;
            this.UploadParamFileBtn.Text = "选择文件..";
            this.UploadParamFileBtn.UseVisualStyleBackColor = true;
            this.UploadParamFileBtn.Click += new System.EventHandler(this.UploadParamFileBtn_Click);
            // 
            // UploadModelFileBtn
            // 
            this.UploadModelFileBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("UploadModelFileBtn.BackgroundImage")));
            this.UploadModelFileBtn.ForeColor = System.Drawing.Color.White;
            this.UploadModelFileBtn.Location = new System.Drawing.Point(453, 134);
            this.UploadModelFileBtn.Name = "UploadModelFileBtn";
            this.UploadModelFileBtn.Size = new System.Drawing.Size(94, 23);
            this.UploadModelFileBtn.TabIndex = 30;
            this.UploadModelFileBtn.Text = "选择文件";
            this.UploadModelFileBtn.UseVisualStyleBackColor = true;
            this.UploadModelFileBtn.Click += new System.EventHandler(this.UploadModelFileBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(66, 139);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(11, 12);
            this.label1.TabIndex = 29;
            this.label1.Text = "*";
            // 
            // ModelFilePathtBox
            // 
            this.ModelFilePathtBox.Location = new System.Drawing.Point(93, 136);
            this.ModelFilePathtBox.Name = "ModelFilePathtBox";
            this.ModelFilePathtBox.Size = new System.Drawing.Size(328, 21);
            this.ModelFilePathtBox.TabIndex = 28;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 139);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 27;
            this.label3.Text = "模型文件";
            // 
            // ModelUploadBtn
            // 
            this.ModelUploadBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ModelUploadBtn.BackgroundImage")));
            this.ModelUploadBtn.ForeColor = System.Drawing.Color.White;
            this.ModelUploadBtn.Location = new System.Drawing.Point(256, 202);
            this.ModelUploadBtn.Name = "ModelUploadBtn";
            this.ModelUploadBtn.Size = new System.Drawing.Size(94, 23);
            this.ModelUploadBtn.TabIndex = 31;
            this.ModelUploadBtn.Text = "上传";
            this.ModelUploadBtn.UseVisualStyleBackColor = true;
            this.ModelUploadBtn.Click += new System.EventHandler(this.ModelUploadBtn_Click);
            // 
            // paramOpenFileDialog
            // 
            this.paramOpenFileDialog.FileName = "openFileDialog1";
            // 
            // modelOpenFileDialog
            // 
            this.modelOpenFileDialog.FileName = "openFileDialog2";
            // 
            // uploadstateLab
            // 
            this.uploadstateLab.AutoSize = true;
            this.uploadstateLab.Location = new System.Drawing.Point(91, 180);
            this.uploadstateLab.Name = "uploadstateLab";
            this.uploadstateLab.Size = new System.Drawing.Size(0, 12);
            this.uploadstateLab.TabIndex = 32;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 180);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 33;
            this.label4.Text = "上传进度：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 64);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 34;
            this.label5.Text = "MPName";
            // 
            // MPID
            // 
            this.MPID.AutoSize = true;
            this.MPID.Location = new System.Drawing.Point(12, 29);
            this.MPID.Name = "MPID";
            this.MPID.Size = new System.Drawing.Size(29, 12);
            this.MPID.TabIndex = 35;
            this.MPID.Text = "MPID";
            // 
            // MPIDtextBox
            // 
            this.MPIDtextBox.Location = new System.Drawing.Point(93, 26);
            this.MPIDtextBox.Name = "MPIDtextBox";
            this.MPIDtextBox.Size = new System.Drawing.Size(328, 21);
            this.MPIDtextBox.TabIndex = 36;
            // 
            // MPNametextBox
            // 
            this.MPNametextBox.Location = new System.Drawing.Point(93, 64);
            this.MPNametextBox.Name = "MPNametextBox";
            this.MPNametextBox.Size = new System.Drawing.Size(328, 21);
            this.MPNametextBox.TabIndex = 37;
            // 
            // ModelUploadByLocalFileForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(579, 255);
            this.Controls.Add(this.MPNametextBox);
            this.Controls.Add(this.MPIDtextBox);
            this.Controls.Add(this.MPID);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.uploadstateLab);
            this.Controls.Add(this.ModelUploadBtn);
            this.Controls.Add(this.UploadModelFileBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ModelFilePathtBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.UploadParamFileBtn);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.ParamFilePathtBox);
            this.Controls.Add(this.label2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ModelUploadByLocalFileForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "预设推送模式";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox ParamFilePathtBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button UploadParamFileBtn;
        private System.Windows.Forms.Button UploadModelFileBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ModelFilePathtBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button ModelUploadBtn;
        private System.Windows.Forms.OpenFileDialog paramOpenFileDialog;
        private System.Windows.Forms.OpenFileDialog modelOpenFileDialog;
        private System.Windows.Forms.Label uploadstateLab;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label MPID;
        private System.Windows.Forms.TextBox MPIDtextBox;
        private System.Windows.Forms.TextBox MPNametextBox;
    }
}