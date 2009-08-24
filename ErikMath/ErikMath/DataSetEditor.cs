using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ERY.EMath
{
    public partial class frmDataSetEditor : Form
    {
        Graph mGraph;

        public frmDataSetEditor(Graph graph)
        {
            InitializeComponent();

            mGraph = graph;

            RefillListBox();

            grpDataSet.Enabled = false;

            graph.AddedDataset += new Graph.GraphDataSetEventHandler(graph_AddedDataset);
        }

        void graph_AddedDataset(object sender, Graph.GraphDataSetEventArgs e)
        {
            RefillListBox();
        }

        private void RefillListBox()
        {
            lstDataSets.Items.Clear();

            foreach (GraphDataSet data in mGraph.DataSets)
            {
                lstDataSets.Items.Add(data);
            }

            if (lstDataSets.Items.Count > 0)
                lstDataSets.SelectedIndex = 0;

            cboPointStyle.Items.Clear();

            foreach (GraphDataSet.SymbolType s in Enum.GetValues(typeof(GraphDataSet.SymbolType)))
            {
                cboPointStyle.Items.Add(s);
            }

            cboLineStyle.Items.Clear();

            foreach (GraphDataSet.LineType l in Enum.GetValues(typeof(GraphDataSet.LineType)))
            {
                cboLineStyle.Items.Add(l);
            }

            UpdateControls();
        }

        private void FillDataOptions()
        {
            GraphDataSet data = lstDataSets.SelectedItem as GraphDataSet;

            txtName.Text  = data.Name;

            lblColor.BackColor = data.Color;

            chkDrawDataset.Checked = data.DrawDataset;
            chkDrawLines.Checked = data.DrawLine;
            chkDrawPoints.Checked = data.DrawPoints;
            chkLegend.Checked = data.ShowInLegend;

            cboPointStyle.SelectedItem = data.PointType;
            cboLineStyle.SelectedItem = data.LineStyle;

            nudLineWeight.Value = (decimal)data.LineWeight;
        }
        private void UpdateControls()
        {
            Text = "Dataset Editor - " + mGraph.Title;

            if (lstDataSets.SelectedIndex != -1)
            {
                btnOK.Enabled = true;
                btnApply.Enabled = true;
            }
            else
            {
                btnOK.Enabled = false;
                btnApply.Enabled = false;
            }
        }

        private void ApplyChanges()
        {
            GraphDataSet data = lstDataSets.SelectedItem as GraphDataSet;

            data.Name = txtName.Text;

            data.Color = lblColor.BackColor;

            data.DrawDataset = chkDrawDataset.Checked;
            data.DrawLine = chkDrawLines.Checked;
            data.DrawPoints = chkDrawPoints.Checked;
            data.ShowInLegend = chkLegend.Checked;

            data.PointType = (GraphDataSet.SymbolType) cboPointStyle.SelectedItem;
            data.LineStyle = (GraphDataSet.LineType)cboLineStyle.SelectedItem;

            data.LineWeight = (float)nudLineWeight.Value;

            mGraph.Invalidate();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            ApplyChanges();

            Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void lstDataSets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstDataSets.SelectedItem != null)
            {
                grpDataSet.Enabled = true;

                FillDataOptions();
            }
            else
                grpDataSet.Enabled = false;


            UpdateControls();
        }

        private void lblColor_Click(object sender, EventArgs e)
        {
            colorPicker.Color = lblColor.BackColor;

            if (colorPicker.ShowDialog() == DialogResult.OK)
            {
                lblColor.BackColor = colorPicker.Color;
            }
        }

        
  

    }
}