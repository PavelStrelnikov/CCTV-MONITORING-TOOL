namespace SDKDeviceTree
{
    partial class CtrlDeviceTree
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Device Tree(Right Click to Add Device)");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CtrlDeviceTree));
            this.treeViewDevice = new System.Windows.Forms.TreeView();
            this.DeviceTreeimageList = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuStripChan = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolStripMenuItemChannelAttribute = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripDevice = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolStripMenuItemLog = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemLogOff = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripChan.SuspendLayout();
            this.contextMenuStripDevice.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeViewDevice
            // 
            this.treeViewDevice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewDevice.ImageIndex = 14;
            this.treeViewDevice.ImageList = this.DeviceTreeimageList;
            this.treeViewDevice.Location = new System.Drawing.Point(0, 0);
            this.treeViewDevice.Margin = new System.Windows.Forms.Padding(0);
            this.treeViewDevice.Name = "treeViewDevice";
            treeNode1.Name = "TreeBoot";
            treeNode1.Text = "Device Tree(Right Click to Add Device)";
            this.treeViewDevice.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.treeViewDevice.SelectedImageIndex = 14;
            this.treeViewDevice.Size = new System.Drawing.Size(206, 642);
            this.treeViewDevice.TabIndex = 1;
            this.treeViewDevice.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewDevice_BeforeSelect);
            this.treeViewDevice.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewDevice_NodeMouseClick);
            this.treeViewDevice.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewDevice_NodeMouseDoubleClick);
            this.treeViewDevice.Leave += new System.EventHandler(this.treeViewDevice_Leave);
            // 
            // DeviceTreeimageList
            // 
            this.DeviceTreeimageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("DeviceTreeimageList.ImageStream")));
            this.DeviceTreeimageList.TransparentColor = System.Drawing.Color.Transparent;
            this.DeviceTreeimageList.Images.SetKeyName(0, "Alarm.bmp");
            this.DeviceTreeimageList.Images.SetKeyName(1, "camera.bmp");
            this.DeviceTreeimageList.Images.SetKeyName(2, "dev_alarm.bmp");
            this.DeviceTreeimageList.Images.SetKeyName(3, "fortify.bmp");
            this.DeviceTreeimageList.Images.SetKeyName(4, "fortify_alarm.bmp");
            this.DeviceTreeimageList.Images.SetKeyName(5, "IPChan.bmp");
            this.DeviceTreeimageList.Images.SetKeyName(6, "login.bmp");
            this.DeviceTreeimageList.Images.SetKeyName(7, "logout.bmp");
            this.DeviceTreeimageList.Images.SetKeyName(8, "p_r_a.bmp");
            this.DeviceTreeimageList.Images.SetKeyName(9, "play.bmp");
            this.DeviceTreeimageList.Images.SetKeyName(10, "play_alarm.bmp");
            this.DeviceTreeimageList.Images.SetKeyName(11, "playAndAlarm.bmp");
            this.DeviceTreeimageList.Images.SetKeyName(12, "rec.bmp");
            this.DeviceTreeimageList.Images.SetKeyName(13, "rec_play.bmp");
            this.DeviceTreeimageList.Images.SetKeyName(14, "tree.bmp");
            // 
            // contextMenuStripChan
            // 
            this.contextMenuStripChan.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemChannelAttribute});
            this.contextMenuStripChan.Name = "contextMenuStripChan";
            this.contextMenuStripChan.Size = new System.Drawing.Size(177, 26);
            // 
            // ToolStripMenuItemChannelAttribute
            // 
            this.ToolStripMenuItemChannelAttribute.Name = "ToolStripMenuItemChannelAttribute";
            this.ToolStripMenuItemChannelAttribute.Size = new System.Drawing.Size(176, 22);
            this.ToolStripMenuItemChannelAttribute.Text = "Channel Attribute";
            // 
            // contextMenuStripDevice
            // 
            this.contextMenuStripDevice.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemLog,
            this.ToolStripMenuItemLogOff});
            this.contextMenuStripDevice.Name = "contextMenuStripDevice";
            this.contextMenuStripDevice.Size = new System.Drawing.Size(119, 48);
            // 
            // ToolStripMenuItemLog
            // 
            this.ToolStripMenuItemLog.Name = "ToolStripMenuItemLog";
            this.ToolStripMenuItemLog.Size = new System.Drawing.Size(118, 22);
            this.ToolStripMenuItemLog.Text = "Log";
            this.ToolStripMenuItemLog.Click += new System.EventHandler(this.ToolStripMenuItemLog_Click);
            // 
            // ToolStripMenuItemLogOff
            // 
            this.ToolStripMenuItemLogOff.Name = "ToolStripMenuItemLogOff";
            this.ToolStripMenuItemLogOff.Size = new System.Drawing.Size(118, 22);
            this.ToolStripMenuItemLogOff.Text = "Log off";
            this.ToolStripMenuItemLogOff.Click += new System.EventHandler(this.ToolStripMenuItemLogOff_Click);
            // 
            // CtrlDeviceTree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.treeViewDevice);
            this.Name = "CtrlDeviceTree";
            this.Size = new System.Drawing.Size(206, 642);
            this.contextMenuStripChan.ResumeLayout(false);
            this.contextMenuStripDevice.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeViewDevice;
        private System.Windows.Forms.ImageList DeviceTreeimageList;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripChan;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemChannelAttribute;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripDevice;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemLog;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemLogOff;

    }
}
