namespace ERY.EMath
{
    partial class frmAxisEditor
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtLabel = new System.Windows.Forms.TextBox();
            this.chkDrawAxis = new System.Windows.Forms.CheckBox();
            this.chkShowLabel = new System.Windows.Forms.CheckBox();
            this.txtMajorTick = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.nudMinorTicks = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.chkLog = new System.Windows.Forms.CheckBox();
            this.chkAutoMinimum = new System.Windows.Forms.CheckBox();
            this.chkAutoMaximum = new System.Windows.Forms.CheckBox();
            this.txtMinimum = new System.Windows.Forms.TextBox();
            this.txtMaximum = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinorTicks)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Axis Label";
            // 
            // txtLabel
            // 
            this.txtLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLabel.Location = new System.Drawing.Point(93, 12);
            this.txtLabel.Name = "txtLabel";
            this.txtLabel.Size = new System.Drawing.Size(195, 20);
            this.txtLabel.TabIndex = 0;
            this.txtLabel.Enter += new System.EventHandler(this.textBox_Enter);
            // 
            // chkDrawAxis
            // 
            this.chkDrawAxis.AutoSize = true;
            this.chkDrawAxis.Location = new System.Drawing.Point(93, 206);
            this.chkDrawAxis.Name = "chkDrawAxis";
            this.chkDrawAxis.Size = new System.Drawing.Size(81, 17);
            this.chkDrawAxis.TabIndex = 1;
            this.chkDrawAxis.Text = "Draw Origin";
            this.chkDrawAxis.UseVisualStyleBackColor = true;
            // 
            // chkShowLabel
            // 
            this.chkShowLabel.AutoSize = true;
            this.chkShowLabel.Location = new System.Drawing.Point(93, 38);
            this.chkShowLabel.Name = "chkShowLabel";
            this.chkShowLabel.Size = new System.Drawing.Size(82, 17);
            this.chkShowLabel.TabIndex = 2;
            this.chkShowLabel.Text = "Show Label";
            this.chkShowLabel.UseVisualStyleBackColor = true;
            // 
            // txtMajorTick
            // 
            this.txtMajorTick.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMajorTick.Location = new System.Drawing.Point(93, 122);
            this.txtMajorTick.Name = "txtMajorTick";
            this.txtMajorTick.Size = new System.Drawing.Size(105, 20);
            this.txtMajorTick.TabIndex = 7;
            this.txtMajorTick.Enter += new System.EventHandler(this.textBox_Enter);
            this.txtMajorTick.TextChanged += new System.EventHandler(this.txtMajorTick_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 26);
            this.label2.TabIndex = 5;
            this.label2.Text = "Major Tick\r\nSpacing\r\n";
            // 
            // nudMinorTicks
            // 
            this.nudMinorTicks.Location = new System.Drawing.Point(93, 157);
            this.nudMinorTicks.Name = "nudMinorTicks";
            this.nudMinorTicks.Size = new System.Drawing.Size(73, 20);
            this.nudMinorTicks.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 159);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Minor Ticks";
            // 
            // chkLog
            // 
            this.chkLog.AutoSize = true;
            this.chkLog.Location = new System.Drawing.Point(93, 183);
            this.chkLog.Name = "chkLog";
            this.chkLog.Size = new System.Drawing.Size(80, 17);
            this.chkLog.TabIndex = 9;
            this.chkLog.Text = "Logarithmic";
            this.chkLog.UseVisualStyleBackColor = true;
            // 
            // chkAutoMinimum
            // 
            this.chkAutoMinimum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAutoMinimum.AutoSize = true;
            this.chkAutoMinimum.Location = new System.Drawing.Point(211, 63);
            this.chkAutoMinimum.Name = "chkAutoMinimum";
            this.chkAutoMinimum.Size = new System.Drawing.Size(48, 17);
            this.chkAutoMinimum.TabIndex = 4;
            this.chkAutoMinimum.Text = "Auto";
            this.chkAutoMinimum.UseVisualStyleBackColor = true;
            // 
            // chkAutoMaximum
            // 
            this.chkAutoMaximum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAutoMaximum.AutoSize = true;
            this.chkAutoMaximum.Location = new System.Drawing.Point(211, 89);
            this.chkAutoMaximum.Name = "chkAutoMaximum";
            this.chkAutoMaximum.Size = new System.Drawing.Size(48, 17);
            this.chkAutoMaximum.TabIndex = 6;
            this.chkAutoMaximum.Text = "Auto";
            this.chkAutoMaximum.UseVisualStyleBackColor = true;
            // 
            // txtMinimum
            // 
            this.txtMinimum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMinimum.Location = new System.Drawing.Point(93, 61);
            this.txtMinimum.Name = "txtMinimum";
            this.txtMinimum.Size = new System.Drawing.Size(105, 20);
            this.txtMinimum.TabIndex = 3;
            this.txtMinimum.Enter += new System.EventHandler(this.textBox_Enter);
            this.txtMinimum.TextChanged += new System.EventHandler(this.txtMinimum_TextChanged);
            // 
            // txtMaximum
            // 
            this.txtMaximum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMaximum.Location = new System.Drawing.Point(93, 87);
            this.txtMaximum.Name = "txtMaximum";
            this.txtMaximum.Size = new System.Drawing.Size(105, 20);
            this.txtMaximum.TabIndex = 5;
            this.txtMaximum.Enter += new System.EventHandler(this.textBox_Enter);
            this.txtMaximum.TextChanged += new System.EventHandler(this.txtMaximum_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 64);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Minimum";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 90);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Maximum";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(204, 231);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(55, 23);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(143, 231);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(55, 23);
            this.btnOK.TabIndex = 10;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Location = new System.Drawing.Point(265, 231);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(55, 23);
            this.btnApply.TabIndex = 12;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // frmAxisEditor
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(332, 266);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtMaximum);
            this.Controls.Add(this.txtMinimum);
            this.Controls.Add(this.chkAutoMaximum);
            this.Controls.Add(this.chkAutoMinimum);
            this.Controls.Add(this.chkLog);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.nudMinorTicks);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtMajorTick);
            this.Controls.Add(this.chkShowLabel);
            this.Controls.Add(this.chkDrawAxis);
            this.Controls.Add(this.txtLabel);
            this.Controls.Add(this.label1);
            this.Name = "frmAxisEditor";
            this.Text = "Axis Editor";
            ((System.ComponentModel.ISupportInitialize)(this.nudMinorTicks)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtLabel;
        private System.Windows.Forms.CheckBox chkDrawAxis;
        private System.Windows.Forms.CheckBox chkShowLabel;
        private System.Windows.Forms.TextBox txtMajorTick;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudMinorTicks;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkLog;
        private System.Windows.Forms.CheckBox chkAutoMinimum;
        private System.Windows.Forms.CheckBox chkAutoMaximum;
        private System.Windows.Forms.TextBox txtMinimum;
        private System.Windows.Forms.TextBox txtMaximum;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnApply;
    }
}