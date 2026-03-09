namespace SDKFaceContrast
{
    partial class FormFaceContrast
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFaceContrast));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.menuStripFaceContrast = new System.Windows.Forms.MenuStrip();
            this.MenuItemFDLib = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemAlarm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemUpload = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemRule = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemModeling = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemFDSearch = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStripFaceContrast = new System.Windows.Forms.StatusStrip();
            this.TimeStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.timerStatus = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStripFaceContrast.SuspendLayout();
            this.statusStripFaceContrast.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.menuStripFaceContrast);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.statusStripFaceContrast);
            this.splitContainer1.Panel2.Controls.Add(this.MainTabControl);
            this.splitContainer1.Size = new System.Drawing.Size(924, 658);
            this.splitContainer1.SplitterDistance = 62;
            this.splitContainer1.TabIndex = 0;
            // 
            // menuStripFaceContrast
            // 
            this.menuStripFaceContrast.AutoSize = false;
            this.menuStripFaceContrast.BackColor = System.Drawing.Color.AliceBlue;
            this.menuStripFaceContrast.Dock = System.Windows.Forms.DockStyle.Fill;
            this.menuStripFaceContrast.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.menuStripFaceContrast.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItemFDLib,
            this.MenuItemAlarm,
            this.MenuItemUpload,
            this.MenuItemRule,
            this.MenuItemModeling,
            this.MenuItemFDSearch});
            this.menuStripFaceContrast.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.menuStripFaceContrast.Location = new System.Drawing.Point(0, 0);
            this.menuStripFaceContrast.Name = "menuStripFaceContrast";
            this.menuStripFaceContrast.ShowItemToolTips = true;
            this.menuStripFaceContrast.Size = new System.Drawing.Size(62, 658);
            this.menuStripFaceContrast.TabIndex = 15;
            this.menuStripFaceContrast.Text = "FaceContrast";
            // 
            // MenuItemFDLib
            // 
            this.MenuItemFDLib.Name = "MenuItemFDLib";
            this.MenuItemFDLib.Size = new System.Drawing.Size(55, 4);
            // 
            // MenuItemAlarm
            // 
            this.MenuItemAlarm.AutoSize = false;
            this.MenuItemAlarm.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("MenuItemAlarm.BackgroundImage")));
            this.MenuItemAlarm.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.MenuItemAlarm.Name = "MenuItemAlarm";
            this.MenuItemAlarm.Size = new System.Drawing.Size(30, 30);
            this.MenuItemAlarm.ToolTipText = "Alarm";
            this.MenuItemAlarm.Click += new System.EventHandler(this.MenuItemAlarm_Click);
            // 
            // MenuItemUpload
            // 
            this.MenuItemUpload.AutoSize = false;
            this.MenuItemUpload.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("MenuItemUpload.BackgroundImage")));
            this.MenuItemUpload.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.MenuItemUpload.Name = "MenuItemUpload";
            this.MenuItemUpload.Size = new System.Drawing.Size(30, 30);
            this.MenuItemUpload.ToolTipText = "Upload";
            this.MenuItemUpload.Click += new System.EventHandler(this.MenuItemUpload_Click);
            // 
            // MenuItemRule
            // 
            this.MenuItemRule.Name = "MenuItemRule";
            this.MenuItemRule.Size = new System.Drawing.Size(55, 4);
            // 
            // MenuItemModeling
            // 
            this.MenuItemModeling.Name = "MenuItemModeling";
            this.MenuItemModeling.Size = new System.Drawing.Size(55, 4);
            // 
            // MenuItemFDSearch
            // 
            this.MenuItemFDSearch.Name = "MenuItemFDSearch";
            this.MenuItemFDSearch.Size = new System.Drawing.Size(55, 4);
            // 
            // statusStripFaceContrast
            // 
            this.statusStripFaceContrast.BackColor = System.Drawing.Color.AliceBlue;
            this.statusStripFaceContrast.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TimeStatus,
            this.toolStripStatusLabel1,
            this.toolStripStatusTime});
            this.statusStripFaceContrast.Location = new System.Drawing.Point(0, 636);
            this.statusStripFaceContrast.Name = "statusStripFaceContrast";
            this.statusStripFaceContrast.Size = new System.Drawing.Size(858, 22);
            this.statusStripFaceContrast.TabIndex = 16;
            this.statusStripFaceContrast.Text = "statusStripFaceContrast";
            // 
            // TimeStatus
            // 
            this.TimeStatus.Name = "TimeStatus";
            this.TimeStatus.Size = new System.Drawing.Size(71, 17);
            this.TimeStatus.Text = "TimeStatus";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripStatusTime
            // 
            this.toolStripStatusTime.Name = "toolStripStatusTime";
            this.toolStripStatusTime.Size = new System.Drawing.Size(131, 17);
            this.toolStripStatusTime.Text = "toolStripStatusLabel2";
            // 
            // MainTabControl
            // 
            this.MainTabControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.MainTabControl.Location = new System.Drawing.Point(0, 0);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(858, 658);
            this.MainTabControl.TabIndex = 0;
            // 
            // timerStatus
            // 
            this.timerStatus.Enabled = true;
            this.timerStatus.Interval = 1000;
            this.timerStatus.Tick += new System.EventHandler(this.timerStatus_Tick);
            // 
            // FormFaceContrast
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 658);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormFaceContrast";
            this.Text = "FormFaceContrast";
            this.Load += new System.EventHandler(this.FormFaceContrast_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.menuStripFaceContrast.ResumeLayout(false);
            this.menuStripFaceContrast.PerformLayout();
            this.statusStripFaceContrast.ResumeLayout(false);
            this.statusStripFaceContrast.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.MenuStrip menuStripFaceContrast;
        private System.Windows.Forms.ToolStripMenuItem MenuItemFDLib;
        private System.Windows.Forms.ToolStripMenuItem MenuItemAlarm;
        private System.Windows.Forms.ToolStripMenuItem MenuItemUpload;
        private System.Windows.Forms.ToolStripMenuItem MenuItemRule;
        private System.Windows.Forms.ToolStripMenuItem MenuItemModeling;
        private System.Windows.Forms.ToolStripMenuItem MenuItemFDSearch;
        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.Timer timerStatus;
        private System.Windows.Forms.StatusStrip statusStripFaceContrast;
        private System.Windows.Forms.ToolStripStatusLabel TimeStatus;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusTime;

    }
}

