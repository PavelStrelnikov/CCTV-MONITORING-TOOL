namespace SDKAIOpenPlatform
{
    partial class FormAIOpenPlatform
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAIOpenPlatform));
            this.panelBase = new System.Windows.Forms.Panel();
            this.MainF_TabBOX = new System.Windows.Forms.TabControl();
            this.m_statusStrip = new System.Windows.Forms.StatusStrip();
            this.AIInfoStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.AImenuStrip = new System.Windows.Forms.MenuStrip();
            this.modelManageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AIAlarmMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelBase.SuspendLayout();
            this.m_statusStrip.SuspendLayout();
            this.AImenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBase
            // 
            this.panelBase.Controls.Add(this.MainF_TabBOX);
            this.panelBase.Controls.Add(this.m_statusStrip);
            this.panelBase.Controls.Add(this.AImenuStrip);
            this.panelBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBase.Location = new System.Drawing.Point(0, 0);
            this.panelBase.Name = "panelBase";
            this.panelBase.Size = new System.Drawing.Size(984, 649);
            this.panelBase.TabIndex = 0;
            // 
            // MainF_TabBOX
            // 
            this.MainF_TabBOX.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainF_TabBOX.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.MainF_TabBOX.Location = new System.Drawing.Point(31, 0);
            this.MainF_TabBOX.Name = "MainF_TabBOX";
            this.MainF_TabBOX.SelectedIndex = 0;
            this.MainF_TabBOX.Size = new System.Drawing.Size(953, 626);
            this.MainF_TabBOX.TabIndex = 7;
            // 
            // m_statusStrip
            // 
            this.m_statusStrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_statusStrip.AutoSize = false;
            this.m_statusStrip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("m_statusStrip.BackgroundImage")));
            this.m_statusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.m_statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AIInfoStatusLabel});
            this.m_statusStrip.Location = new System.Drawing.Point(0, 626);
            this.m_statusStrip.Name = "m_statusStrip";
            this.m_statusStrip.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.m_statusStrip.Size = new System.Drawing.Size(984, 22);
            this.m_statusStrip.TabIndex = 1;
            this.m_statusStrip.Text = "statusStrip1";
            // 
            // AIInfoStatusLabel
            // 
            this.AIInfoStatusLabel.BackColor = System.Drawing.Color.Transparent;
            this.AIInfoStatusLabel.ForeColor = System.Drawing.Color.DimGray;
            this.AIInfoStatusLabel.Name = "AIInfoStatusLabel";
            this.AIInfoStatusLabel.Size = new System.Drawing.Size(54, 17);
            this.AIInfoStatusLabel.Text = "AI INFO";
            // 
            // AImenuStrip
            // 
            this.AImenuStrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.AImenuStrip.AutoSize = false;
            this.AImenuStrip.BackColor = System.Drawing.Color.Transparent;
            this.AImenuStrip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("AImenuStrip.BackgroundImage")));
            this.AImenuStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.AImenuStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.AImenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modelManageMenuItem,
            this.AIAlarmMenuItem});
            this.AImenuStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.AImenuStrip.Location = new System.Drawing.Point(0, 0);
            this.AImenuStrip.Margin = new System.Windows.Forms.Padding(2);
            this.AImenuStrip.Name = "AImenuStrip";
            this.AImenuStrip.Padding = new System.Windows.Forms.Padding(2, 6, 0, 6);
            this.AImenuStrip.ShowItemToolTips = true;
            this.AImenuStrip.Size = new System.Drawing.Size(33, 626);
            this.AImenuStrip.TabIndex = 0;
            this.AImenuStrip.Text = "AI模型管理服务";
            // 
            // modelManageMenuItem
            // 
            this.modelManageMenuItem.AutoSize = false;
            this.modelManageMenuItem.BackColor = System.Drawing.Color.Transparent;
            this.modelManageMenuItem.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("modelManageMenuItem.BackgroundImage")));
            this.modelManageMenuItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.modelManageMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.modelManageMenuItem.ForeColor = System.Drawing.Color.Black;
            this.modelManageMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.modelManageMenuItem.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.modelManageMenuItem.Name = "modelManageMenuItem";
            this.modelManageMenuItem.Size = new System.Drawing.Size(23, 23);
            this.modelManageMenuItem.ToolTipText = "算法模型管理";
            this.modelManageMenuItem.Click += new System.EventHandler(this.modelManageMenuItem_Click);
            // 
            // AIAlarmMenuItem
            // 
            this.AIAlarmMenuItem.AutoSize = false;
            this.AIAlarmMenuItem.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("AIAlarmMenuItem.BackgroundImage")));
            this.AIAlarmMenuItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.AIAlarmMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.AIAlarmMenuItem.Name = "AIAlarmMenuItem";
            this.AIAlarmMenuItem.Size = new System.Drawing.Size(23, 23);
            this.AIAlarmMenuItem.Text = "开放平台设备检测数据上传布防";
            this.AIAlarmMenuItem.ToolTipText = "开放平台设备检测数据上传布防";
            this.AIAlarmMenuItem.Click += new System.EventHandler(this.AIAlarmMenuItem_Click);
            // 
            // FormAIOpenPlatform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(984, 649);
            this.Controls.Add(this.panelBase);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.AImenuStrip;
            this.Name = "FormAIOpenPlatform";
            this.Text = "AI开放平台";
            this.Load += new System.EventHandler(this.FormAIOpenPlatform_Load);
            this.panelBase.ResumeLayout(false);
            this.m_statusStrip.ResumeLayout(false);
            this.m_statusStrip.PerformLayout();
            this.AImenuStrip.ResumeLayout(false);
            this.AImenuStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBase;
        private System.Windows.Forms.MenuStrip AImenuStrip;
        private System.Windows.Forms.ToolStripMenuItem modelManageMenuItem;
        private System.Windows.Forms.StatusStrip m_statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel AIInfoStatusLabel;
        private System.Windows.Forms.TabControl MainF_TabBOX;
        private System.Windows.Forms.ToolStripMenuItem AIAlarmMenuItem;
    }
}