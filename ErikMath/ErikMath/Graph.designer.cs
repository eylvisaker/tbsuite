namespace ERY.EMath
{
    public partial class Graph 
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Graph));
            this.txtMaxX = new System.Windows.Forms.TextBox();
            this.txtMinX = new System.Windows.Forms.TextBox();
            this.txtMaxY = new System.Windows.Forms.TextBox();
            this.txtMinY = new System.Windows.Forms.TextBox();
            this.menu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editDatasetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuShowTitle = new System.Windows.Forms.ToolStripMenuItem();
            this.titleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtTitle = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.horizontalAxisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verticalAxisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoScaleAxesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exportDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.graphToagrusedByXMGraceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.pageSetupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printDialog = new System.Windows.Forms.PrintDialog();
            this.printDocument = new System.Drawing.Printing.PrintDocument();
            this.printPreviewDialog = new System.Windows.Forms.PrintPreviewDialog();
            this.pageSetupDialog = new System.Windows.Forms.PageSetupDialog();
            this.imageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.encapsulatedPostScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFile = new System.Windows.Forms.SaveFileDialog();
            this.menu.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtMaxX
            // 
            this.txtMaxX.AcceptsReturn = true;
            this.txtMaxX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMaxX.Location = new System.Drawing.Point(217, 187);
            this.txtMaxX.Name = "txtMaxX";
            this.txtMaxX.Size = new System.Drawing.Size(40, 20);
            this.txtMaxX.TabIndex = 3;
            this.txtMaxX.Visible = false;
            this.txtMaxX.VisibleChanged += new System.EventHandler(this.txtBox_VisibleChanged);
            this.txtMaxX.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMaxX_KeyPress);
            this.txtMaxX.Validated += new System.EventHandler(this.txtMaxX_Validated);
            this.txtMaxX.Validating += new System.ComponentModel.CancelEventHandler(this.txtMaxX_Validating);
            // 
            // txtMinX
            // 
            this.txtMinX.AcceptsReturn = true;
            this.txtMinX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtMinX.Location = new System.Drawing.Point(34, 187);
            this.txtMinX.Name = "txtMinX";
            this.txtMinX.Size = new System.Drawing.Size(40, 20);
            this.txtMinX.TabIndex = 4;
            this.txtMinX.Visible = false;
            this.txtMinX.VisibleChanged += new System.EventHandler(this.txtBox_VisibleChanged);
            this.txtMinX.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMinX_KeyPress);
            this.txtMinX.Validated += new System.EventHandler(this.txtMinX_Validated);
            this.txtMinX.Validating += new System.ComponentModel.CancelEventHandler(this.txtMinX_Validating);
            // 
            // txtMaxY
            // 
            this.txtMaxY.AcceptsReturn = true;
            this.txtMaxY.Location = new System.Drawing.Point(0, 3);
            this.txtMaxY.Name = "txtMaxY";
            this.txtMaxY.Size = new System.Drawing.Size(40, 20);
            this.txtMaxY.TabIndex = 5;
            this.txtMaxY.Visible = false;
            this.txtMaxY.VisibleChanged += new System.EventHandler(this.txtBox_VisibleChanged);
            this.txtMaxY.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMaxY_KeyPress);
            this.txtMaxY.Validated += new System.EventHandler(this.txtMaxY_Validated);
            this.txtMaxY.Validating += new System.ComponentModel.CancelEventHandler(this.txtMaxY_Validating);
            // 
            // txtMinY
            // 
            this.txtMinY.AcceptsReturn = true;
            this.txtMinY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtMinY.Location = new System.Drawing.Point(0, 172);
            this.txtMinY.Name = "txtMinY";
            this.txtMinY.Size = new System.Drawing.Size(40, 20);
            this.txtMinY.TabIndex = 6;
            this.txtMinY.Visible = false;
            this.txtMinY.VisibleChanged += new System.EventHandler(this.txtBox_VisibleChanged);
            this.txtMinY.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMinY_KeyPress);
            this.txtMinY.Validated += new System.EventHandler(this.txtMinY_Validated);
            this.txtMinY.Validating += new System.ComponentModel.CancelEventHandler(this.txtMinY_Validating);
            // 
            // menu
            // 
            this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editDatasetsToolStripMenuItem,
            this.toolStripSeparator4,
            this.mnuShowTitle,
            this.titleToolStripMenuItem,
            this.toolStripSeparator2,
            this.horizontalAxisToolStripMenuItem,
            this.verticalAxisToolStripMenuItem,
            this.autoScaleAxesToolStripMenuItem,
            this.toolStripSeparator1,
            this.exportDataToolStripMenuItem,
            this.toolStripSeparator3,
            this.pageSetupToolStripMenuItem,
            this.printPreviewToolStripMenuItem,
            this.printToolStripMenuItem});
            this.menu.Name = "menu";
            this.menu.Size = new System.Drawing.Size(179, 248);
            // 
            // editDatasetsToolStripMenuItem
            // 
            this.editDatasetsToolStripMenuItem.Name = "editDatasetsToolStripMenuItem";
            this.editDatasetsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.editDatasetsToolStripMenuItem.Text = "Edit Datasets...";
            this.editDatasetsToolStripMenuItem.Click += new System.EventHandler(this.editDatasetsToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(175, 6);
            // 
            // mnuShowTitle
            // 
            this.mnuShowTitle.Name = "mnuShowTitle";
            this.mnuShowTitle.Size = new System.Drawing.Size(178, 22);
            this.mnuShowTitle.Text = "Show Title";
            this.mnuShowTitle.Click += new System.EventHandler(this.mnuShowTitle_Click);
            // 
            // titleToolStripMenuItem
            // 
            this.titleToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.txtTitle});
            this.titleToolStripMenuItem.Name = "titleToolStripMenuItem";
            this.titleToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.titleToolStripMenuItem.Text = "Title Text";
            // 
            // txtTitle
            // 
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(100, 21);
            this.txtTitle.Text = "Enter Title Text";
            this.txtTitle.TextChanged += new System.EventHandler(this.txtTitle_TextChanged);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(175, 6);
            // 
            // horizontalAxisToolStripMenuItem
            // 
            this.horizontalAxisToolStripMenuItem.Name = "horizontalAxisToolStripMenuItem";
            this.horizontalAxisToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.horizontalAxisToolStripMenuItem.Text = "Edit Horizontal Axis...";
            this.horizontalAxisToolStripMenuItem.Click += new System.EventHandler(this.horizontalAxisToolStripMenuItem_Click);
            // 
            // verticalAxisToolStripMenuItem
            // 
            this.verticalAxisToolStripMenuItem.Name = "verticalAxisToolStripMenuItem";
            this.verticalAxisToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.verticalAxisToolStripMenuItem.Text = "Edit Vertical Axis...";
            this.verticalAxisToolStripMenuItem.Click += new System.EventHandler(this.verticalAxisToolStripMenuItem_Click);
            // 
            // autoScaleAxesToolStripMenuItem
            // 
            this.autoScaleAxesToolStripMenuItem.Name = "autoScaleAxesToolStripMenuItem";
            this.autoScaleAxesToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.autoScaleAxesToolStripMenuItem.Text = "Auto Scale Axes";
            this.autoScaleAxesToolStripMenuItem.Click += new System.EventHandler(this.autoScaleAxesToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(175, 6);
            // 
            // exportDataToolStripMenuItem
            // 
            this.exportDataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.textToolStripMenuItem,
            this.graphToagrusedByXMGraceToolStripMenuItem,
            this.imageToolStripMenuItem,
            this.encapsulatedPostScriptToolStripMenuItem});
            this.exportDataToolStripMenuItem.Name = "exportDataToolStripMenuItem";
            this.exportDataToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.exportDataToolStripMenuItem.Text = "Export";
            // 
            // textToolStripMenuItem
            // 
            this.textToolStripMenuItem.Name = "textToolStripMenuItem";
            this.textToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.textToolStripMenuItem.Text = "Datasets to .txt...";
            this.textToolStripMenuItem.Click += new System.EventHandler(this.textToolStripMenuItem_Click);
            // 
            // graphToagrusedByXMGraceToolStripMenuItem
            // 
            this.graphToagrusedByXMGraceToolStripMenuItem.Name = "graphToagrusedByXMGraceToolStripMenuItem";
            this.graphToagrusedByXMGraceToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.graphToagrusedByXMGraceToolStripMenuItem.Text = "Graph to .agr (used by XM/Grace)...";
            this.graphToagrusedByXMGraceToolStripMenuItem.Click += new System.EventHandler(this.graphToagrusedByXMGraceToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(175, 6);
            // 
            // pageSetupToolStripMenuItem
            // 
            this.pageSetupToolStripMenuItem.Name = "pageSetupToolStripMenuItem";
            this.pageSetupToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.pageSetupToolStripMenuItem.Text = "Page Setup...";
            this.pageSetupToolStripMenuItem.Click += new System.EventHandler(this.pageSetupToolStripMenuItem_Click);
            // 
            // printPreviewToolStripMenuItem
            // 
            this.printPreviewToolStripMenuItem.Name = "printPreviewToolStripMenuItem";
            this.printPreviewToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.printPreviewToolStripMenuItem.Text = "Print Preview...";
            this.printPreviewToolStripMenuItem.Click += new System.EventHandler(this.printPreviewToolStripMenuItem_Click);
            // 
            // printToolStripMenuItem
            // 
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.printToolStripMenuItem.Text = "Print...";
            this.printToolStripMenuItem.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            // 
            // printDialog
            // 
            this.printDialog.Document = this.printDocument;
            this.printDialog.UseEXDialog = true;
            // 
            // printDocument
            // 
            this.printDocument.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.printDocument_PrintPage);
            // 
            // printPreviewDialog
            // 
            this.printPreviewDialog.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.printPreviewDialog.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.printPreviewDialog.ClientSize = new System.Drawing.Size(400, 300);
            this.printPreviewDialog.Document = this.printDocument;
            this.printPreviewDialog.Enabled = true;
            this.printPreviewDialog.Icon = ((System.Drawing.Icon)(resources.GetObject("printPreviewDialog.Icon")));
            this.printPreviewDialog.Name = "printPreviewDialog";
            this.printPreviewDialog.Visible = false;
            // 
            // pageSetupDialog
            // 
            this.pageSetupDialog.Document = this.printDocument;
            // 
            // imageToolStripMenuItem
            // 
            this.imageToolStripMenuItem.Name = "imageToolStripMenuItem";
            this.imageToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.imageToolStripMenuItem.Text = "Image...";
            this.imageToolStripMenuItem.Click += new System.EventHandler(this.imageToolStripMenuItem_Click);
            // 
            // encapsulatedPostScriptToolStripMenuItem
            // 
            this.encapsulatedPostScriptToolStripMenuItem.Name = "encapsulatedPostScriptToolStripMenuItem";
            this.encapsulatedPostScriptToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.encapsulatedPostScriptToolStripMenuItem.Text = "Encapsulated PostScript (EPS)...";
            this.encapsulatedPostScriptToolStripMenuItem.Click += new System.EventHandler(this.encapsulatedPostScriptToolStripMenuItem_Click);
            // 
            // Graph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ContextMenuStrip = this.menu;
            this.Controls.Add(this.txtMinY);
            this.Controls.Add(this.txtMaxY);
            this.Controls.Add(this.txtMinX);
            this.Controls.Add(this.txtMaxX);
            this.Name = "Graph";
            this.Size = new System.Drawing.Size(260, 229);
            this.Load += new System.EventHandler(this.Graph_Load);
            this.Click += new System.EventHandler(this.ClickOffTextBox);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Graph_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Graph_MouseMove);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Graph_MouseClick);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Graph_MouseDoubleClick);
            this.Resize += new System.EventHandler(this.Graph_Resize);
            this.MouseEnter += new System.EventHandler(this.Graph_MouseEnter);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Graph_Paint);
            this.MouseLeave += new System.EventHandler(this.Graph_MouseLeave);
            this.MouseHover += new System.EventHandler(this.Graph_MouseHover);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Graph_MouseUp);
            this.menu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtMaxX;
        private System.Windows.Forms.TextBox txtMinX;
        private System.Windows.Forms.TextBox txtMaxY;
        private System.Windows.Forms.TextBox txtMinY;
        private System.Windows.Forms.ContextMenuStrip menu;
        private System.Windows.Forms.ToolStripMenuItem mnuShowTitle;
        private System.Windows.Forms.ToolStripMenuItem titleToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox txtTitle;
        private System.Windows.Forms.ToolStripMenuItem autoScaleAxesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exportDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem graphToagrusedByXMGraceToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem horizontalAxisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem verticalAxisToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.PrintDialog printDialog;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private System.Windows.Forms.PrintPreviewDialog printPreviewDialog;
        private System.Drawing.Printing.PrintDocument printDocument;
        private System.Windows.Forms.PageSetupDialog pageSetupDialog;
        private System.Windows.Forms.ToolStripMenuItem pageSetupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editDatasetsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem imageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem encapsulatedPostScriptToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFile;
    }
}
