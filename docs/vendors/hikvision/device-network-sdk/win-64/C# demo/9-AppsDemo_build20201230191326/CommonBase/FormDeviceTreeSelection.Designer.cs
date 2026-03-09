namespace Common
{
    partial class FormDeviceTreeSelection
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
            this.listBoxDeviceTree = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // listBoxDeviceTree
            // 
            this.listBoxDeviceTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxDeviceTree.Font = new System.Drawing.Font("宋体", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listBoxDeviceTree.FormattingEnabled = true;
            this.listBoxDeviceTree.ItemHeight = 20;
            this.listBoxDeviceTree.Location = new System.Drawing.Point(0, 0);
            this.listBoxDeviceTree.Name = "listBoxDeviceTree";
            this.listBoxDeviceTree.Size = new System.Drawing.Size(284, 262);
            this.listBoxDeviceTree.TabIndex = 0;
            this.listBoxDeviceTree.SelectedIndexChanged += new System.EventHandler(this.listBoxDeviceTree_SelectedIndexChanged);
            // 
            // FormDeviceTreeSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.ControlBox = false;
            this.Controls.Add(this.listBoxDeviceTree);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormDeviceTreeSelection";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select a DeviceTree";
            this.Load += new System.EventHandler(this.FormDeviceTreeSelection_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxDeviceTree;
    }
}