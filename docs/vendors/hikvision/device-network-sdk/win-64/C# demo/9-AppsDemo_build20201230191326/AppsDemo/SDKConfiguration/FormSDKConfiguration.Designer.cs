namespace SDKConfiguration
{
    partial class FormSDKConfiguration
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
            this.BtnReStartDev = new System.Windows.Forms.Button();
            this.BtnRestore = new System.Windows.Forms.Button();
            this.BtnUpgrade = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnReStartDev
            // 
            this.BtnReStartDev.Location = new System.Drawing.Point(16, 58);
            this.BtnReStartDev.Name = "BtnReStartDev";
            this.BtnReStartDev.Size = new System.Drawing.Size(111, 26);
            this.BtnReStartDev.TabIndex = 1;
            this.BtnReStartDev.Text = "Restart";
            this.BtnReStartDev.UseVisualStyleBackColor = true;
            this.BtnReStartDev.Click += new System.EventHandler(this.BtnReStartDev_Click);
            // 
            // BtnRestore
            // 
            this.BtnRestore.Location = new System.Drawing.Point(140, 58);
            this.BtnRestore.Name = "BtnRestore";
            this.BtnRestore.Size = new System.Drawing.Size(110, 26);
            this.BtnRestore.TabIndex = 2;
            this.BtnRestore.Text = "RestoreToDefault";
            this.BtnRestore.UseVisualStyleBackColor = true;
            this.BtnRestore.Click += new System.EventHandler(this.BtnRestore_Click);
            // 
            // BtnUpgrade
            // 
            this.BtnUpgrade.Location = new System.Drawing.Point(263, 58);
            this.BtnUpgrade.Name = "BtnUpgrade";
            this.BtnUpgrade.Size = new System.Drawing.Size(111, 26);
            this.BtnUpgrade.TabIndex = 3;
            this.BtnUpgrade.Text = "Device Upgrade";
            this.BtnUpgrade.UseVisualStyleBackColor = true;
            this.BtnUpgrade.Click += new System.EventHandler(this.BtnUpgrade_Click);
            // 
            // FormSDKConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(731, 490);
            this.Controls.Add(this.BtnUpgrade);
            this.Controls.Add(this.BtnRestore);
            this.Controls.Add(this.BtnReStartDev);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "FormSDKConfiguration";
            this.Text = "设备基本配置";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnReStartDev;
        private System.Windows.Forms.Button BtnRestore;
        private System.Windows.Forms.Button BtnUpgrade;
    }
}