namespace SDKFaceSnap
{
    partial class FormFaceSnap
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFaceSnap));
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.menuStripFaceContrast = new System.Windows.Forms.MenuStrip();
            this.MenuItemRule = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemAlarm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemAlgParam = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStripFaceContrast = new System.Windows.Forms.StatusStrip();
            this.TimeStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.timerStatus = new System.Windows.Forms.Timer(this.components);
            this.menuStripFaceContrast.SuspendLayout();
            this.statusStripFaceContrast.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.MainTabControl.Location = new System.Drawing.Point(0, 0);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(783, 721);
            this.MainTabControl.TabIndex = 0;
            this.MainTabControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainTabControl_MouseDown);
            // 
            // menuStripFaceContrast
            // 
            this.menuStripFaceContrast.AutoSize = false;
            this.menuStripFaceContrast.BackColor = System.Drawing.Color.AliceBlue;
            this.menuStripFaceContrast.Dock = System.Windows.Forms.DockStyle.Fill;
            this.menuStripFaceContrast.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.menuStripFaceContrast.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItemRule,
            this.MenuItemAlarm,
            this.MenuItemAlgParam});
            this.menuStripFaceContrast.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.menuStripFaceContrast.Location = new System.Drawing.Point(0, 0);
            this.menuStripFaceContrast.Name = "menuStripFaceContrast";
            this.menuStripFaceContrast.ShowItemToolTips = true;
            this.menuStripFaceContrast.Size = new System.Drawing.Size(56, 721);
            this.menuStripFaceContrast.TabIndex = 15;
            this.menuStripFaceContrast.Text = "FaceContrast";
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
            // statusStripFaceContrast
            // 
            this.statusStripFaceContrast.BackColor = System.Drawing.Color.AliceBlue;
            this.statusStripFaceContrast.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TimeStatus,
            this.toolStripStatusLabel1,
            this.toolStripStatusTime});
            this.statusStripFaceContrast.Location = new System.Drawing.Point(0, 699);
            this.statusStripFaceContrast.Name = "statusStripFaceContrast";
            this.statusStripFaceContrast.Size = new System.Drawing.Size(783, 22);
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
            this.splitContainer1.Size = new System.Drawing.Size(843, 721);
            this.splitContainer1.SplitterDistance = 56;
            this.splitContainer1.TabIndex = 1;
            // 
            // timerStatus
            // 
            this.timerStatus.Enabled = true;
            this.timerStatus.Interval = 1000;
            this.timerStatus.Tick += new System.EventHandler(this.timerStatus_Tick);
            // 
            // FormFaceSnap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(843, 721);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "FormFaceSnap";
            this.Text = "FormFaceSnap";
            this.Load += new System.EventHandler(this.FormFaceSnap_Load);
            this.menuStripFaceContrast.ResumeLayout(false);
            this.menuStripFaceContrast.PerformLayout();
            this.statusStripFaceContrast.ResumeLayout(false);
            this.statusStripFaceContrast.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.MenuStrip menuStripFaceContrast;
        private System.Windows.Forms.ToolStripMenuItem MenuItemRule;
        private System.Windows.Forms.ToolStripMenuItem MenuItemAlarm;
        private System.Windows.Forms.ToolStripMenuItem MenuItemAlgParam;
        private System.Windows.Forms.StatusStrip statusStripFaceContrast;
        private System.Windows.Forms.ToolStripStatusLabel TimeStatus;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusTime;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Timer timerStatus;



    }
}