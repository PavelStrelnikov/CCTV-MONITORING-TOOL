namespace SDKThermometry
{
    partial class FormThermAlg
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
            this.textBoxFireAlg = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxThermometryAlg = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxShipsAlg = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnGetThermalAlg = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox1.Controls.Add(this.textBoxFireAlg);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBoxThermometryAlg);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBoxShipsAlg);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnGetThermalAlg);
            this.groupBox1.Location = new System.Drawing.Point(12, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(366, 254);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "热成像算法版本";
            // 
            // textBoxFireAlg
            // 
            this.textBoxFireAlg.Location = new System.Drawing.Point(141, 153);
            this.textBoxFireAlg.Name = "textBoxFireAlg";
            this.textBoxFireAlg.Size = new System.Drawing.Size(169, 21);
            this.textBoxFireAlg.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 162);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "火点算法版本";
            // 
            // textBoxThermometryAlg
            // 
            this.textBoxThermometryAlg.Location = new System.Drawing.Point(141, 97);
            this.textBoxThermometryAlg.Name = "textBoxThermometryAlg";
            this.textBoxThermometryAlg.Size = new System.Drawing.Size(169, 21);
            this.textBoxThermometryAlg.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 106);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "测温算法版本";
            // 
            // textBoxShipsAlg
            // 
            this.textBoxShipsAlg.Location = new System.Drawing.Point(141, 48);
            this.textBoxShipsAlg.Name = "textBoxShipsAlg";
            this.textBoxShipsAlg.Size = new System.Drawing.Size(169, 21);
            this.textBoxShipsAlg.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "船只算法版本";
            // 
            // btnGetThermalAlg
            // 
            this.btnGetThermalAlg.Location = new System.Drawing.Point(235, 200);
            this.btnGetThermalAlg.Name = "btnGetThermalAlg";
            this.btnGetThermalAlg.Size = new System.Drawing.Size(75, 23);
            this.btnGetThermalAlg.TabIndex = 0;
            this.btnGetThermalAlg.Text = "获取";
            this.btnGetThermalAlg.UseVisualStyleBackColor = true;
            this.btnGetThermalAlg.Click += new System.EventHandler(this.btnGetThermalAlg_Click);
            // 
            // FormThermAlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 339);
            this.Controls.Add(this.groupBox1);
            this.Name = "FormThermAlg";
            this.Text = "FormThermAlg";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBoxFireAlg;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxThermometryAlg;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxShipsAlg;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnGetThermalAlg;
    }
}