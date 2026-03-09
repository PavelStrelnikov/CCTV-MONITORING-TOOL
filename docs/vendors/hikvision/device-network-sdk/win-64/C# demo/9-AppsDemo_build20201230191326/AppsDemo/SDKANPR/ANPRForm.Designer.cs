namespace SDKANPR
{
    partial class ANPRForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ANPRForm));
            this.panelBase = new System.Windows.Forms.Panel();
            this.MainF_TabBOX = new System.Windows.Forms.TabControl();
            this.m_statusStrip = new System.Windows.Forms.StatusStrip();
            this.ANPRmenuStrip = new System.Windows.Forms.MenuStrip();
            this.LicensePlateAuditMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.licensePlateRecognitionItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ANPRAlarmMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ANPRListenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ManualSnapMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelBase.SuspendLayout();
            this.ANPRmenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBase
            // 
            this.panelBase.Controls.Add(this.MainF_TabBOX);
            this.panelBase.Controls.Add(this.m_statusStrip);
            this.panelBase.Controls.Add(this.ANPRmenuStrip);
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
            this.m_statusStrip.Location = new System.Drawing.Point(0, 626);
            this.m_statusStrip.Name = "m_statusStrip";
            this.m_statusStrip.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.m_statusStrip.Size = new System.Drawing.Size(984, 22);
            this.m_statusStrip.TabIndex = 1;
            this.m_statusStrip.Text = "statusStrip1";
            // 
            // ANPRmenuStrip
            // 
            this.ANPRmenuStrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ANPRmenuStrip.AutoSize = false;
            this.ANPRmenuStrip.BackColor = System.Drawing.Color.Transparent;
            this.ANPRmenuStrip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ANPRmenuStrip.BackgroundImage")));
            this.ANPRmenuStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.ANPRmenuStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.ANPRmenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LicensePlateAuditMenuItem,
            this.licensePlateRecognitionItem,
            this.ANPRAlarmMenuItem,
            this.ANPRListenMenuItem,
            this.ManualSnapMenuItem});
            this.ANPRmenuStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.ANPRmenuStrip.Location = new System.Drawing.Point(0, 0);
            this.ANPRmenuStrip.Margin = new System.Windows.Forms.Padding(2);
            this.ANPRmenuStrip.Name = "ANPRmenuStrip";
            this.ANPRmenuStrip.Padding = new System.Windows.Forms.Padding(2, 6, 0, 6);
            this.ANPRmenuStrip.ShowItemToolTips = true;
            this.ANPRmenuStrip.Size = new System.Drawing.Size(33, 626);
            this.ANPRmenuStrip.TabIndex = 0;
            this.ANPRmenuStrip.Text = "ANPR模块";
            // 
            // LicensePlateAuditMenuItem
            // 
            this.LicensePlateAuditMenuItem.AutoSize = false;
            this.LicensePlateAuditMenuItem.BackColor = System.Drawing.Color.Transparent;
            this.LicensePlateAuditMenuItem.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("LicensePlateAuditMenuItem.BackgroundImage")));
            this.LicensePlateAuditMenuItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.LicensePlateAuditMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.LicensePlateAuditMenuItem.ForeColor = System.Drawing.Color.Black;
            this.LicensePlateAuditMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.LicensePlateAuditMenuItem.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.LicensePlateAuditMenuItem.Name = "LicensePlateAuditMenuItem";
            this.LicensePlateAuditMenuItem.Size = new System.Drawing.Size(23, 23);
            this.LicensePlateAuditMenuItem.ToolTipText = "block allow list config";
            this.LicensePlateAuditMenuItem.Click += new System.EventHandler(this.LicensePlateAuditMenuItem_Click);
            // 
            // licensePlateRecognitionItem
            // 
            this.licensePlateRecognitionItem.AutoSize = false;
            this.licensePlateRecognitionItem.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("licensePlateRecognitionItem.BackgroundImage")));
            this.licensePlateRecognitionItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.licensePlateRecognitionItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.licensePlateRecognitionItem.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.licensePlateRecognitionItem.Name = "licensePlateRecognitionItem";
            this.licensePlateRecognitionItem.Size = new System.Drawing.Size(23, 23);
            this.licensePlateRecognitionItem.Text = "licensePlateRecognitionItem";
            this.licensePlateRecognitionItem.ToolTipText = "车牌识别";
            this.licensePlateRecognitionItem.Click += new System.EventHandler(this.licensePlateRecognitionItem_Click);
            // 
            // ANPRAlarmMenuItem
            // 
            this.ANPRAlarmMenuItem.AutoSize = false;
            this.ANPRAlarmMenuItem.BackColor = System.Drawing.Color.Transparent;
            this.ANPRAlarmMenuItem.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ANPRAlarmMenuItem.BackgroundImage")));
            this.ANPRAlarmMenuItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ANPRAlarmMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ANPRAlarmMenuItem.ForeColor = System.Drawing.Color.Black;
            this.ANPRAlarmMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.ANPRAlarmMenuItem.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.ANPRAlarmMenuItem.Name = "ANPRAlarmMenuItem";
            this.ANPRAlarmMenuItem.Size = new System.Drawing.Size(23, 23);
            this.ANPRAlarmMenuItem.Text = "ANPRAlarmMenuItem";
            this.ANPRAlarmMenuItem.ToolTipText = "ANPR布防";
            this.ANPRAlarmMenuItem.Click += new System.EventHandler(this.ANPRAlarmMenuItem_Click);
            // 
            // ANPRListenMenuItem
            // 
            this.ANPRListenMenuItem.AutoSize = false;
            this.ANPRListenMenuItem.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ANPRListenMenuItem.BackgroundImage")));
            this.ANPRListenMenuItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ANPRListenMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ANPRListenMenuItem.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.ANPRListenMenuItem.Name = "ANPRListenMenuItem";
            this.ANPRListenMenuItem.Size = new System.Drawing.Size(23, 22);
            this.ANPRListenMenuItem.Text = "ANPRListenMenuItem";
            this.ANPRListenMenuItem.ToolTipText = "ANPR监听";
            this.ANPRListenMenuItem.Click += new System.EventHandler(this.ANPRListenMenuItem_Click);
            // 
            // ManualSnapMenuItem
            // 
            this.ManualSnapMenuItem.AutoSize = false;
            this.ManualSnapMenuItem.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ManualSnapMenuItem.BackgroundImage")));
            this.ManualSnapMenuItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ManualSnapMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ManualSnapMenuItem.Name = "ManualSnapMenuItem";
            this.ManualSnapMenuItem.Size = new System.Drawing.Size(23, 23);
            this.ManualSnapMenuItem.Text = "ManualSnapMenuItem";
            this.ManualSnapMenuItem.Click += new System.EventHandler(this.ManualSnapMenuItem_Click);
            // 
            // ANPRForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(984, 649);
            this.Controls.Add(this.panelBase);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.MainMenuStrip = this.ANPRmenuStrip;
            this.Name = "ANPRForm";
            this.Text = "ANPR模块";
            this.Load += new System.EventHandler(this.FormANPRform_Load);
            this.panelBase.ResumeLayout(false);
            this.ANPRmenuStrip.ResumeLayout(false);
            this.ANPRmenuStrip.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.Panel panelBase;
        private System.Windows.Forms.MenuStrip ANPRmenuStrip;
        private System.Windows.Forms.StatusStrip m_statusStrip;
        private System.Windows.Forms.TabControl MainF_TabBOX;
        private System.Windows.Forms.ToolStripMenuItem LicensePlateAuditMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ANPRAlarmMenuItem;
        private System.Windows.Forms.ToolStripMenuItem licensePlateRecognitionItem;
        private System.Windows.Forms.ToolStripMenuItem ANPRListenMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ManualSnapMenuItem;
        //private System.Windows.Forms.ToolStripStatusLabel AIInfoStatusLabel;


        
    }
}