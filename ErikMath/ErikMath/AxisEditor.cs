using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ERY.EMath
{
    public partial class frmAxisEditor : Form
    {
        Graph mGraph;
        Graph.Axis mAxis;
        bool mIsVertical;

        public frmAxisEditor(Graph graph, Graph.Axis axis, bool isVertical)
        {
            InitializeComponent();

            mGraph = graph;
            mAxis = axis;
            mIsVertical = isVertical;

            FillDataFromAxis();

        }

        private void FillDataFromAxis()
        {
            txtLabel.Text = mAxis.Text;
            
            chkShowLabel.Checked = mAxis.DisplayAxisLabel;
            chkDrawAxis.Checked = mAxis.DrawOrigin;
            chkLog.Checked = mAxis.Logarithmic;

            txtMajorTick.Text = mAxis.MajorSpacing.ToString();
            nudMinorTicks.Value = (decimal)mAxis.MinorTicks;

            txtMaximum.Text = mAxis.Max.ToString();
            txtMinimum.Text = mAxis.Min.ToString();

            chkAutoMaximum.Checked = mAxis.AutoSetMax;
            chkAutoMinimum.Checked = mAxis.AutoSetMin;

        }

        private void ApplyChanges()
        {
            mAxis.Text = txtLabel.Text;

            mAxis.DisplayAxisLabel = chkShowLabel.Checked;
            mAxis.DrawOrigin = chkDrawAxis.Checked;
            mAxis.Logarithmic = chkLog.Checked;

            mAxis.MajorSpacing = double.Parse(txtMajorTick.Text);
            mAxis.MinorTicks = (int)nudMinorTicks.Value;

            mAxis.Max = double.Parse(txtMaximum.Text);
            mAxis.Min = double.Parse(txtMinimum.Text);

            mAxis.AutoSetMax = chkAutoMaximum.Checked;
            mAxis.AutoSetMin = chkAutoMinimum.Checked;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            ApplyChanges();
            FillDataFromAxis();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            ApplyChanges();
            FillDataFromAxis();
        }

        private void txtMajorTick_TextChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            double val;
            bool allowApply = true;

            if (double.TryParse(txtMajorTick.Text, out val) == false) allowApply = false;
            if (double.TryParse(txtMaximum.Text, out val) == false) allowApply = false;
            if (double.TryParse(txtMinimum.Text, out val) == false) allowApply = false;



            btnApply.Enabled = allowApply;
            btnOK.Enabled = allowApply;
        }

        private void txtMinimum_TextChanged(object sender, EventArgs e)
        {
            chkAutoMinimum.Checked = false; 
            
            UpdateControls();
        }

        private void txtMaximum_TextChanged(object sender, EventArgs e)
        {
            chkAutoMaximum.Checked = false;

            UpdateControls();
        }

        private void textBox_Enter(object sender, EventArgs e)
        {
            TextBox box = sender as TextBox;

            box.SelectionStart = 0;
            box.SelectionLength = box.Text.Length;

        }
    }
}