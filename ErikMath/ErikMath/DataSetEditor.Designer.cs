namespace ERY.EMath
{
    partial class frmDataSetEditor
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
            this.lstDataSets = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.grpDataSet = new System.Windows.Forms.GroupBox();
            this.nudLineWeight = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.cboLineStyle = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cboPointStyle = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkDrawPoints = new System.Windows.Forms.CheckBox();
            this.chkDrawLines = new System.Windows.Forms.CheckBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkDrawDataset = new System.Windows.Forms.CheckBox();
            this.chkLegend = new System.Windows.Forms.CheckBox();
            this.colorPicker = new System.Windows.Forms.ColorDialog();
            this.label6 = new System.Windows.Forms.Label();
            this.lblColor = new System.Windows.Forms.Label();
            this.grpDataSet.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLineWeight)).BeginInit();
            this.SuspendLayout();
            // 
            // lstDataSets
            // 
            this.lstDataSets.DisplayMember = "Name";
            this.lstDataSets.FormattingEnabled = true;
            this.lstDataSets.Location = new System.Drawing.Point(12, 25);
            this.lstDataSets.Name = "lstDataSets";
            this.lstDataSets.Size = new System.Drawing.Size(87, 147);
            this.lstDataSets.TabIndex = 0;
            this.lstDataSets.SelectedIndexChanged += new System.EventHandler(this.lstDataSets_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Data Sets:";
            // 
            // grpDataSet
            // 
            this.grpDataSet.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpDataSet.Controls.Add(this.lblColor);
            this.grpDataSet.Controls.Add(this.label6);
            this.grpDataSet.Controls.Add(this.chkLegend);
            this.grpDataSet.Controls.Add(this.chkDrawDataset);
            this.grpDataSet.Controls.Add(this.nudLineWeight);
            this.grpDataSet.Controls.Add(this.label5);
            this.grpDataSet.Controls.Add(this.cboLineStyle);
            this.grpDataSet.Controls.Add(this.label4);
            this.grpDataSet.Controls.Add(this.cboPointStyle);
            this.grpDataSet.Controls.Add(this.label3);
            this.grpDataSet.Controls.Add(this.chkDrawPoints);
            this.grpDataSet.Controls.Add(this.chkDrawLines);
            this.grpDataSet.Controls.Add(this.txtName);
            this.grpDataSet.Controls.Add(this.label2);
            this.grpDataSet.Location = new System.Drawing.Point(105, 12);
            this.grpDataSet.Name = "grpDataSet";
            this.grpDataSet.Size = new System.Drawing.Size(208, 255);
            this.grpDataSet.TabIndex = 2;
            this.grpDataSet.TabStop = false;
            this.grpDataSet.Text = "Current DataSet";
            // 
            // nudLineWeight
            // 
            this.nudLineWeight.DecimalPlaces = 1;
            this.nudLineWeight.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.nudLineWeight.Location = new System.Drawing.Point(76, 168);
            this.nudLineWeight.Maximum = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.nudLineWeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudLineWeight.Name = "nudLineWeight";
            this.nudLineWeight.Size = new System.Drawing.Size(69, 20);
            this.nudLineWeight.TabIndex = 12;
            this.nudLineWeight.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(5, 170);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Line Weight";
            // 
            // cboLineStyle
            // 
            this.cboLineStyle.FormattingEnabled = true;
            this.cboLineStyle.Location = new System.Drawing.Point(77, 141);
            this.cboLineStyle.Name = "cboLineStyle";
            this.cboLineStyle.Size = new System.Drawing.Size(112, 21);
            this.cboLineStyle.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 145);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Line Style";
            // 
            // cboPointStyle
            // 
            this.cboPointStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPointStyle.FormattingEnabled = true;
            this.cboPointStyle.Location = new System.Drawing.Point(77, 114);
            this.cboPointStyle.Name = "cboPointStyle";
            this.cboPointStyle.Size = new System.Drawing.Size(112, 21);
            this.cboPointStyle.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Point Style";
            // 
            // chkDrawPoints
            // 
            this.chkDrawPoints.AutoSize = true;
            this.chkDrawPoints.Location = new System.Drawing.Point(77, 91);
            this.chkDrawPoints.Name = "chkDrawPoints";
            this.chkDrawPoints.Size = new System.Drawing.Size(83, 17);
            this.chkDrawPoints.TabIndex = 6;
            this.chkDrawPoints.Text = "Draw Points";
            this.chkDrawPoints.UseVisualStyleBackColor = true;
            // 
            // chkDrawLines
            // 
            this.chkDrawLines.AutoSize = true;
            this.chkDrawLines.Location = new System.Drawing.Point(77, 68);
            this.chkDrawLines.Name = "chkDrawLines";
            this.chkDrawLines.Size = new System.Drawing.Size(74, 17);
            this.chkDrawLines.TabIndex = 5;
            this.chkDrawLines.Text = "Draw Line";
            this.chkDrawLines.UseVisualStyleBackColor = true;
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(77, 42);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(112, 20);
            this.txtName.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Name";
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Location = new System.Drawing.Point(251, 288);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(62, 23);
            this.btnApply.TabIndex = 0;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(114, 288);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(62, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(182, 288);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(62, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // chkDrawDataset
            // 
            this.chkDrawDataset.AutoSize = true;
            this.chkDrawDataset.Location = new System.Drawing.Point(77, 19);
            this.chkDrawDataset.Name = "chkDrawDataset";
            this.chkDrawDataset.Size = new System.Drawing.Size(108, 17);
            this.chkDrawDataset.TabIndex = 13;
            this.chkDrawDataset.Text = "Draw this dataset";
            this.chkDrawDataset.UseVisualStyleBackColor = true;
            // 
            // chkLegend
            // 
            this.chkLegend.AutoSize = true;
            this.chkLegend.Location = new System.Drawing.Point(76, 194);
            this.chkLegend.Name = "chkLegend";
            this.chkLegend.Size = new System.Drawing.Size(104, 17);
            this.chkLegend.TabIndex = 14;
            this.chkLegend.Text = "Show In Legend";
            this.chkLegend.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 224);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Color";
            // 
            // lblColor
            // 
            this.lblColor.BackColor = System.Drawing.Color.Navy;
            this.lblColor.Location = new System.Drawing.Point(74, 214);
            this.lblColor.Name = "lblColor";
            this.lblColor.Size = new System.Drawing.Size(27, 23);
            this.lblColor.TabIndex = 16;
            this.lblColor.Text = "    ";
            this.lblColor.Click += new System.EventHandler(this.lblColor_Click);
            // 
            // frmDataSetEditor
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(325, 323);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.grpDataSet);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lstDataSets);
            this.Name = "frmDataSetEditor";
            this.Text = "Data Set Editor";
            this.grpDataSet.ResumeLayout(false);
            this.grpDataSet.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLineWeight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstDataSets;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox grpDataSet;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboPointStyle;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkDrawPoints;
        private System.Windows.Forms.CheckBox chkDrawLines;
        private System.Windows.Forms.NumericUpDown nudLineWeight;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cboLineStyle;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkDrawDataset;
        private System.Windows.Forms.CheckBox chkLegend;
        private System.Windows.Forms.Label lblColor;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ColorDialog colorPicker;
    }
}