using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace ERY.EMath
{

    [DefaultEvent("MouseGraphEventHandler")]
    public partial class Graph : UserControl
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class Axis
        {
            #region --- Private fields ---

            private bool mDrawAxis = true;

            private double mMin = -10;
            private double mMax = 10;

            private bool mAutoSetMin = true;
            private bool mAutoSetMax = true;

            private double mMajorSpacing = 2;
            private int mMinorTicks = 1;

            private bool mLogarithmic = false;

            private Font mAxisFont = new Font("Microsoft San Serif", 10f);
            private Font mLabelFont = new Font("Microsoft San Serif", 12f);

            private string mAxisLabel;
            private bool mDisplayAxisLabel = true;

            #endregion

            #region --- Constructor ---

            internal Axis(string label)
            {
                mAxisLabel = label;
            }

            #endregion

            #region --- Public Designer Properties ---

            public Font AxisFont
            {
                get { return mAxisFont; }
                set
                {
                    if (value != null)
                        mAxisFont = value;

                    OnAxisUpdated();
                }
            }
            public Font LabelFont
            {
                get { return mLabelFont; }
                set
                {
                    mLabelFont = value;

                    OnAxisUpdated();
                }
            }
            [DefaultValue(true)]
            public bool DrawOrigin
            {
                get
                {
                    return mDrawAxis;
                }
                set
                {
                    mDrawAxis = value;
                    OnAxisUpdated();

                }
            }

            public string Text
            {
                get
                {
                    return mAxisLabel;
                }
                set
                {
                    mAxisLabel = value;

                    OnAxisUpdated();
                }
            }
            [DefaultValue(true)]
            public bool DisplayAxisLabel
            {
                get { return mDisplayAxisLabel; }
                set
                {
                    mDisplayAxisLabel = value;

                    OnAxisUpdated();
                }
            }

            public double MajorSpacing
            {
                get
                {
                    return mMajorSpacing;
                }
                set
                {
                    if (value > 0)
                        mMajorSpacing = value;

                    OnAxisUpdated();
                }
            }

            [DefaultValue(2)]
            public int MinorTicks
            {
                get
                {
                    return mMinorTicks;
                }
                set
                {
                    if (value > 0 && !mLogarithmic && value < 100)
                        mMinorTicks = value;

                    OnAxisUpdated();
                }
            }

            [DefaultValue(-10)]
            public double Min
            {
                get
                {
                    return mMin;
                }
                set
                {
                    if (value < mMax)
                    {
                        if (Logarithmic && value <= 0)
                        {

                        }
                        else
                            mMin = value;
                    }

                    OnAxisUpdated();
                }
            }
            [DefaultValue(10)]
            public double Max
            {
                get
                {
                    return mMax;
                }
                set
                {
                    if (value > mMin)
                    {
                        mMax = value;
                    }

                    OnAxisUpdated();
                }
            }
            [DefaultValue(true)]
            public bool AutoSetMin
            {
                get { return mAutoSetMin; }
                set { mAutoSetMin = value; }
            }
            [DefaultValue(true)]
            public bool AutoSetMax
            {
                get { return mAutoSetMax; }
                set { mAutoSetMax = value; }
            }

            [DefaultValue(false)]
            public bool Logarithmic
            {
                get { return mLogarithmic; }
                set
                {
                    if (mLogarithmic != value)
                    {
                        mLogarithmic = value;

                        if (value == true)
                        {
                            if (mMax < 0)
                            {
                                throw new System.Exception("Cannot display negative values on logarithmic axis.  Set max axis value to positive number first.");
                            }
                            else if (mMin < 0)
                                mMin = mMax / 1000.0;

                            mMajorSpacing = 10;
                            mMinorTicks = 8;
                            mDrawAxis = false;
                        }

                        OnAxisUpdated();
                    }
                }
            }

            #endregion
            #region --- Public Non-Designer Properties ---

            /// <summary>
            /// Returns the amount of graph space covered by the axis (ie. Max - Min)
            /// </summary>
            [Browsable(false)]
            public double Span
            {
                get { return Max - Min; }
            }

            #endregion

            #region --- Events ---

            internal void DoAutosetMin(double value)
            {
                if (mAutoSetMin)
                    Min = value;
            }
            internal void DoAutosetMax(double value)
            {
                if (mAutoSetMax)
                    Max = value;
            }


            public event EventHandler AxisUpdated;


            private void OnAxisUpdated()
            {
                if (AxisUpdated != null)
                    AxisUpdated(this, EventArgs.Empty);
            }

            #endregion


        }

        private bool mThrowError = true;
        private bool mErrorCondition = false;
        private string mErrorMessage = "";

        private bool mUserEditable = true;

        private int mMarginLeft = 50;
        private int mMarginRight = 10;
        private int mMarginTop = 10;
        private int mMarginBottom = 40;

        private Pen mGraphPen;
        private int mMinorTickSize = 2;
        private int mMajorTickSize = 5;

        private int mMouseRadius = 4;
        private Rectangle mBoxRect = new Rectangle();

        private Axis mXAxis = new Axis("X");
        private Axis mYAxis = new Axis("Y");

        private Font mTitleFont = new Font("Microsoft San Serif", 12f);
        private Font mSubTitleFont = new Font("Microsoft San Serif", 10f);

        private string mTitleText = "Graph";
        private bool mShowTitle = true;
        private Rectangle mTitleRect;

        private string mSubTitleText = "";
        private bool mShowSubTitle = false;
        private Rectangle mSubTitleRect;

        private List<GraphDataSet> mDataSets = new List<GraphDataSet>();
        private frmDataSetEditor mDataSetEditor;

        private Pen mAxisPen;

        private int mDataSetCreatedCount = 0;

        private static Color[] mColors = {
            Color.Blue,
            Color.Red,
            Color.DarkGreen,
            Color.Gold,
            Color.Fuchsia,
            Color.Purple,
            Color.Pink
        };

        private const int textBoxOverlap = 8;
        private bool mAllowInvalidate = true;

        public Graph()
        {
            InitializeComponent();

            mXAxis.AxisUpdated += new EventHandler(mAxis_AxisUpdated);
            mYAxis.AxisUpdated += new EventHandler(mAxis_AxisUpdated);

            /*
            mBoxRect.X = mMarginLeft;
            mBoxRect.Y = mMarginTop + lblTitle.Height;
            mBoxRect.Width = Width - mMarginLeft - mMarginRight;
            mBoxRect.Height = Height - mMarginTop - mMarginBottom;
            */

            SetDefaultPrintDocumentSettings();

            mGraphPen = Pens.Black;

            mAxisPen = new Pen(Color.Black, 1);
            mAxisPen.DashPattern = new float[] { 2, 2 };
        }

        public GraphDataSet CreateNewDataSet()
        {
            GraphDataSet retval = new GraphDataSet();
            mDataSetCreatedCount++;

            retval.Name = "Dataset " + mDataSetCreatedCount.ToString();

            retval.Color = mColors[mDataSets.Count % mColors.Length];

            mDataSets.Add(retval);

            OnCreateDataSet(retval);

            return retval;
        }
        public GraphDataSet GetOrCreateDataSet(int index)
        {
            if (index < mDataSets.Count)
                return mDataSets[index];

            while (index >= mDataSets.Count)
                CreateNewDataSet();

            return mDataSets[index];
        }
        public GraphDataSet GetOrCreateBlankDataSet(int index)
        {
            GraphDataSet retval = GetOrCreateDataSet(index);

            retval.Clear();

            return retval;
        }
        public void SetDataSetCount(int count)
        {
            while (mDataSets.Count > count)
                mDataSets.RemoveAt(mDataSets.Count - 1);

            while (mDataSets.Count < count)
                CreateNewDataSet();
        }

        private void OnCreateDataSet(GraphDataSet dataSet)
        {
            if (AddedDataset != null)
            {
                GraphDataSetEventArgs e = new GraphDataSetEventArgs();

                e.Graph = this;
                e.GraphDataSet = dataSet;

                AddedDataset(this, e);
            }
        }

        void mAxis_AxisUpdated(object sender, EventArgs e)
        {
            UpdateControls();
            UpdateGraph();
        }



        [DefaultValue(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public bool ShowTitle
        {
            get { return mShowTitle; }
            set
            {
                mShowTitle = value;

                if (mShowTitle == false)
                    mShowSubTitle = false;

                UpdateControls();
            }
        }
        public string Title
        {
            get { return mTitleText; }
            set
            {
                mTitleText = value;

                UpdateGraph();
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Font TitleFont
        {
            get { return mTitleFont; }
            set
            {
                mTitleFont = value;

                UpdateGraph();
            }
        }

        [DefaultValue(false)]
        public bool ShowSubTitle
        {
            get { return mShowSubTitle; }
            set
            {
                if (mShowTitle == false)
                    mShowSubTitle = false;
                else
                    mShowSubTitle = value;


                UpdateGraph();
            }
        }
        public string SubTitle
        {
            get { return mSubTitleText; }
            set
            {
                mSubTitleText = value;

                UpdateGraph();
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Font SubTitleFont
        {
            get { return mSubTitleFont; }
            set
            {
                mSubTitleFont = value;

                UpdateGraph();
            }
        }

        public List<GraphDataSet> DataSets
        {
            get { return mDataSets; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Axis HorizontalAxis
        {
            get { return mXAxis; }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Axis VerticalAxis
        {
            get { return mYAxis; }
        }


        [DefaultValue(true)]
        public bool UserEditable
        {
            get { return mUserEditable; }
            set
            {
                mUserEditable = value;

                if (value)
                {
                    this.ContextMenuStrip = menu;
                }
                else
                    this.ContextMenuStrip = null;
            }
        }
        public int MinorTickSize
        {
            get
            {
                return mMinorTickSize;
            }
            set
            {
                if (value >= 0)
                    mMinorTickSize = value;

                UpdateGraph();
            }
        }
        public int MajorTickSize
        {
            get
            {
                return mMajorTickSize;
            }
            set
            {
                if (value >= 0)
                    mMajorTickSize = value;

                UpdateGraph();
            }
        }

        //private int mMarginLeft = 50;
        //private int mMarginRight = 10;
        //private int mMarginTop = 10;
        //private int mMarginBottom = 40;

        [DefaultValue(50)]
        public int Margin_Left
        {
            get
            {
                return mMarginLeft;
            }
            set
            {
                mMarginLeft = value;

                UpdateControls();
            }
        }
        [DefaultValue(10)]
        public int Margin_Right
        {
            get
            {
                return mMarginRight;
            }
            set
            {
                mMarginRight = value;

                UpdateControls();
            }
        }
        [DefaultValue(10)]
        public int Margin_Top
        {
            get
            {
                return mMarginTop;
            }
            set
            {
                mMarginTop = value;

                UpdateControls();
            }
        }
        [DefaultValue(40)]
        public int Margin_Bottom
        {
            get
            {
                return mMarginBottom;
            }
            set
            {
                mMarginBottom = value;

                UpdateControls();
            }
        }

        public bool ThrowError
        {
            get
            {
                return mThrowError;
            }
            set
            {
                mThrowError = value;
            }
        }
        [Browsable(false)]
        public bool ErrorOccured
        {
            get { return mErrorCondition; }
        }
        [Browsable(false)]
        public string ErrorMessage
        {
            get { return mErrorMessage; }
        }

        public void ClearError()
        {
            mErrorCondition = false;
            mErrorMessage = "";
        }

        /// <summary>
        /// Size of area around mouse pointer that is included in search for datasets.
        /// </summary>
        [DefaultValue(4)]
        public int MouseRadius
        {
            get { return mMouseRadius; }
            set
            {
                if (value > 0)
                    mMouseRadius = value;
            }
        }

        public void AutoSetAxisLimits()
        {
            if (mDataSets.Count == 0 ||
                mDataSets[0].Count == 0)
            {
                HorizontalAxis.DoAutosetMin(-10);
                HorizontalAxis.DoAutosetMax(10);

                VerticalAxis.DoAutosetMin(-10);
                VerticalAxis.DoAutosetMax(10);

                return;
            }


            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;


            foreach (GraphDataSet d in mDataSets)
            {
                foreach (PointF p in d)
                {
                    if (HorizontalAxis.AutoSetMin == false && p.X < HorizontalAxis.Min)
                        continue;
                    if (HorizontalAxis.AutoSetMax == false && p.X > HorizontalAxis.Max)
                        continue;

                    if ((minX > p.X && float.IsInfinity(p.X) == false) || float.IsInfinity(minX) || float.IsNaN(minX))
                    {
                        if (HorizontalAxis.Logarithmic == false || p.X > 0)
                            minX = p.X;

                    }
                    if ((maxX < p.X && float.IsInfinity(p.X) == false) || float.IsInfinity(maxX) || float.IsNaN(maxX))
                        maxX = p.X;

                    if ((minY > p.Y && float.IsInfinity(p.Y) == false) || float.IsInfinity(minY) || float.IsNaN(minY))
                    {
                        if (VerticalAxis.Logarithmic == false || p.Y > 0)
                            minY = p.Y;

                    }
                    if ((maxY < p.Y && float.IsInfinity(p.Y) == false) || float.IsInfinity(maxY) || float.IsNaN(maxY))
                        maxY = p.Y;

                }
            }

            // make adjustments so it looks good
            if (minX == maxX)
            {
                maxX += 0.5f;
                minX -= 0.5f;
            }
            else
            {
                float diff = maxX - minX;

                maxX += 0.05f * diff;
                minX -= 0.05f * diff;
            }

            if (minY == maxY)
            {
                maxY += 0.5f;
                minY -= 0.5f;
            }
            else
            {
                float diff = maxY - minY;

                maxY += 0.05f * diff;
                minY -= 0.05f * diff;
            }

            // check and make sure it's not hopeless
            if (minX < maxX)
            {
                HorizontalAxis.DoAutosetMin(minX);
                HorizontalAxis.DoAutosetMax(maxX);
            }

            if (minY < maxY)
            {
                VerticalAxis.DoAutosetMin(minY);

                VerticalAxis.DoAutosetMax(maxY);
            }

            // ok now adjust ticks stuff.
            AutoSetTicks();


        }

        public void AutoSetTicks()
        {
            double spanX = mXAxis.Span;
            double spanY = mYAxis.Span;

            // figure out the first power of 10 lower than them
            double tickX_10 = Math.Pow(10, Math.Floor(Math.Log10(spanX)));
            double tickY_10 = Math.Pow(10, Math.Floor(Math.Log10(spanY)));


            // now count how many major ticks would be
            int majorCount_X = (int)(spanX / tickX_10);
            int majorCount_Y = (int)(spanY / tickY_10);

            if (majorCount_X < 2) tickX_10 /= 10;
            if (majorCount_Y < 2) tickY_10 /= 10;

            mXAxis.MajorSpacing = tickX_10;
            mYAxis.MajorSpacing = tickY_10;

            // recalculate how many ticks there are.
            majorCount_X = (int)(spanX / tickX_10);
            majorCount_Y = (int)(spanY / tickY_10);

            mXAxis.MinorTicks = Math.Max(15 / majorCount_X, 1);
            mYAxis.MinorTicks = Math.Max(15 / majorCount_Y, 1);

            UpdateGraph();
        }

        #region --- Private Coordinate Transform functions ---

        bool mMouseInBox = false;


        private void box_MouseEnter(object sender, EventArgs e)
        {
            if (GraphMouseEnter != null) GraphMouseEnter(this, e);

        }
        private void box_MouseLeave(object sender, EventArgs e)
        {
            if (GraphMouseLeave != null) GraphMouseLeave(this, e);

        }


        private void Graph_MouseClick(object sender, MouseEventArgs e)
        {
            if (CheckUserEditControls(e.Location))
                return;

            if (GraphMouseClick != null)
                GraphMouseClick(this, getMouseEventArgs(e));
        }
        private void Graph_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (GraphMouseDoubleClick != null) GraphMouseDoubleClick(this, getMouseEventArgs(e));

        }
        private void Graph_MouseDown(object sender, MouseEventArgs e)
        {
            if (GraphMouseDown != null) GraphMouseDown(this, getMouseEventArgs(e));

        }
        private void Graph_MouseEnter(object sender, EventArgs e)
        {

        }
        private void Graph_MouseHover(object sender, EventArgs e)
        {
            if (GraphMouseHover != null) GraphMouseHover(this, e);

        }
        private void Graph_MouseLeave(object sender, EventArgs e)
        {

        }
        private void Graph_MouseMove(object sender, MouseEventArgs e)
        {

            if (mBoxRect.Contains(e.Location))
            {
                if (mMouseInBox == false)
                {
                    mMouseInBox = true;

                    if (GraphMouseEnter != null)
                        GraphMouseEnter(this, getMouseEventArgs(e));
                }
            }
            else
            {
                if (mMouseInBox == true)
                {
                    mMouseInBox = false;


                    if (GraphMouseLeave != null)
                        GraphMouseLeave(this, getMouseEventArgs(e));
                }
            }

            if (GraphMouseMove != null)
                GraphMouseMove(this, getMouseEventArgs(e));


        }
        private void Graph_MouseUp(object sender, MouseEventArgs e)
        {
            if (GraphMouseUp != null) GraphMouseUp(this, getMouseEventArgs(e));

        }

        private void Graph_Load(object sender, EventArgs e)
        {

            UpdateControls();
            UpdateGraph();

        }

        private double graphToBox_X(double graph_x)
        {
            double box_x;

            if (!mXAxis.Logarithmic)
                box_x = (graph_x - mXAxis.Min) / (mXAxis.Max - mXAxis.Min) * mBoxRect.Width;
            else
            {
                double A = mBoxRect.Width / (Math.Log10(mXAxis.Max / mXAxis.Min));

                box_x = A * Math.Log10(graph_x / mXAxis.Min);
            }

            return box_x + mBoxRect.Left;
        }
        private double graphToBox_Y(double graph_y)
        {
            double box_y;

            if (!mYAxis.Logarithmic)
                box_y = mBoxRect.Height - (float)(graph_y - mYAxis.Min) / (float)(mYAxis.Max - mYAxis.Min) * mBoxRect.Height;
            else
            {
                double A = mBoxRect.Height / (Math.Log10(mYAxis.Max / mYAxis.Min));

                box_y = mBoxRect.Height - A * Math.Log10(graph_y / mYAxis.Min);
            }

            return box_y + mBoxRect.Top;
        }
        private float graphToBox_X(float graph_x)
        {
            return (float)graphToBox_X((double)graph_x);
        }
        private float graphToBox_Y(float graph_y)
        {
            return (float)graphToBox_Y((double)graph_y);
        }
        private PointF graphToBox(PointF graphPt)
        {
            float x, y;

            x = graphToBox_X(graphPt.X);
            y = graphToBox_Y(graphPt.Y);

            return new PointF(x, y);
        }

        // converts a point from the box to a point in the local graph coordinate system.
        private double boxToGraph_X(double box_x)
        {
            double graph_x;

            box_x -= mBoxRect.Left;

            if (!mXAxis.Logarithmic)
                graph_x = mXAxis.Min + (box_x) / (mBoxRect.Width) * (mXAxis.Max - mXAxis.Min);
            else
            {
                double lambda = Math.Log10(mXAxis.Max / mXAxis.Min) / mBoxRect.Width;

                graph_x = mXAxis.Min * Math.Pow(10, lambda * box_x);
            }

            return graph_x;
        }
        private float boxToGraph_X(float box_x)
        {
            return (float)boxToGraph_X((double)box_x);
        }
        private double boxToGraph_Y(double box_y)
        {
            double graph_y;
            box_y -= mBoxRect.Top;

            if (!mYAxis.Logarithmic)
                graph_y = mYAxis.Max - (box_y) / (mBoxRect.Height) * (mYAxis.Max - mYAxis.Min);
            else
            {
                double lambda = Math.Log10(mYAxis.Max / mYAxis.Min) / mBoxRect.Height;

                graph_y = mYAxis.Max * Math.Pow(10, -lambda * box_y);
            }

            return graph_y;
        }
        private float boxToGraph_Y(float box_y)
        {
            return (float)boxToGraph_Y((double)box_y);
        }
        private PointF boxToGraph(PointF boxPt)
        {
            return new PointF(boxToGraph_X(boxPt.X), boxToGraph_Y(boxPt.Y));

        }

        private PointF localToGraph(PointF localPt)
        {
            PointF boxPt = new PointF(localPt.X - mBoxRect.Left, localPt.Y - mBoxRect.Top);

            return boxToGraph(boxPt);
        }

        #endregion


        void UpdateControls()
        {

            txtMaxY.Location = new Point(mBoxRect.Location.X - txtMaxY.Width + textBoxOverlap,
                                         mBoxRect.Location.Y - txtMaxY.Height / 2);

            txtMinY.Location = new Point(mBoxRect.Location.X - txtMaxY.Width + textBoxOverlap,
                                         mBoxRect.Location.Y + mBoxRect.Height - txtMaxY.Height + textBoxOverlap);

            txtMinX.Location = new Point(mBoxRect.Location.X - textBoxOverlap,
                                         mBoxRect.Location.Y + mBoxRect.Height - textBoxOverlap);

            txtMaxX.Location = new Point(mBoxRect.Location.X + mBoxRect.Width - txtMaxX.Width + textBoxOverlap,
                                         mBoxRect.Location.Y + mBoxRect.Height - textBoxOverlap);


            UpdateMenuItems();
        }


        void UpdateGraph()
        {
            if (mAllowInvalidate)
                Invalidate();
        }
        void UpdateMenuItems()
        {
            mnuShowTitle.Checked = ShowTitle;
            txtTitle.Text = Title;

        }

        private void Graph_Resize(object sender, EventArgs e)
        {
            Refresh();
            UpdateControls();

        }
        private void Graph_Paint(object sender, PaintEventArgs e)
        {
            Graphics gr = e.Graphics;

            mAllowInvalidate = false;

            try
            {
                PaintToGraphics(gr, Width, Height);
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine("Error in Graph_Paint: " + error.Message);
            }
            mAllowInvalidate = true;
        }

        public void PaintToGraphics(Graphics gr, int width, int height)
        {
            UpdateBoxRect(gr, width, height);

            Brush backgroundBrush = Brushes.White;
            gr.FillRectangle(backgroundBrush, mBoxRect);
            
            PaintGraph(gr);
            PaintDatasets(gr, width, height);
            PaintAxisLabels(gr, width, height);
            PaintLegend(gr, width, height);
        }

        private void PaintLegend(Graphics gr, int width, int height)
        {

        }


        private void PaintAxisLabels(Graphics gr, int width, int height)
        {
            Brush fontBrush = Brushes.Black;


            // ok, now draw axis labels
            if (ShowTitle)
            {
                StringFormat format = new StringFormat();

                format.Alignment = StringAlignment.Center;

                gr.DrawString(mTitleText, mTitleFont, fontBrush, mTitleRect, format);


                if (ShowSubTitle)
                {

                    gr.DrawString(mSubTitleText, mSubTitleFont, fontBrush, mSubTitleRect, format);

                }
            }

            if (mXAxis.DisplayAxisLabel)
            {
                float stringHeight = gr.MeasureString(mXAxis.Text, mXAxis.LabelFont).Height;

                RectangleF dest = new RectangleF(mBoxRect.Left, height - stringHeight,
                                                 mBoxRect.Width, stringHeight);

                StringFormat format = new StringFormat();

                format.Alignment = StringAlignment.Center;

                gr.DrawString(mXAxis.Text, mXAxis.LabelFont, fontBrush, dest, format);
            }

            if (mYAxis.DisplayAxisLabel)
            {
                float stringHeight = gr.MeasureString(mYAxis.Text, mYAxis.LabelFont).Height;

                // string height is width, because the font is drawn vertically.
                RectangleF dest = new RectangleF(0, 0, stringHeight, mBoxRect.Height);

                StringFormat format = new StringFormat();

                format.Alignment = StringAlignment.Center;
                format.FormatFlags |= StringFormatFlags.DirectionVertical;

                gr.TranslateTransform(dest.Width, mBoxRect.Top + mBoxRect.Height);
                gr.RotateTransform(180.0f);

                gr.DrawString(mYAxis.Text, mYAxis.LabelFont, fontBrush, dest, format);

                //gr.DrawRectangle(Pens.Red, dest.X, dest.Y, dest.Width, dest.Height);
            }
        }
        private void PaintGraph(Graphics gr)
        {
            Brush fontBrush = Brushes.Black;
            
            PointF drawOrigin = graphToBox(new PointF(0, 0));
            // draw border

            gr.DrawRectangle(mGraphPen, mBoxRect);

            //boxGraphics.Clear(Color.White);


            // draw axes
            if (mYAxis.DrawOrigin && mYAxis.Logarithmic == false &&
                drawOrigin.Y > mBoxRect.Top && drawOrigin.Y < mBoxRect.Bottom)
            {

                try
                {
                    gr.DrawLine(mAxisPen,
                        new PointF(mBoxRect.Left, drawOrigin.Y),
                        new PointF(mBoxRect.Right, drawOrigin.Y));
                }
                catch (OverflowException overflow)
                {
                    gr.DrawString("An overflow error has occured when drawing axes.\r\n" +
                                "It is likely that the axis limits are too large.\r\n" + overflow.Message,
                        this.Font, fontBrush, new Point(0, 0));

                }

            }

            if (mXAxis.DrawOrigin && mXAxis.Logarithmic == false &&
                drawOrigin.X > mBoxRect.Left && drawOrigin.X < mBoxRect.Right)
            {
                try
                {
                    gr.DrawLine(mAxisPen,
                       new PointF(drawOrigin.X, mBoxRect.Top),
                       new PointF(drawOrigin.X, mBoxRect.Bottom));
                }
                catch (OverflowException overflow)
                {
                    gr.DrawString("An overflow error has occured when drawing axes.\r\n" +
                                "It is likely that the axis limits are too large.\r\n" + overflow.Message,
                        this.Font, fontBrush, new Point(0, 0));

                }

            }

            double startTick;

            // do x ticks
            if (!mXAxis.Logarithmic)
                startTick = mXAxis.MajorSpacing * ((int)(mXAxis.Min / mXAxis.MajorSpacing));
            else
            {
                startTick = Math.Pow(10, Math.Floor(Math.Log10(mXAxis.Min)));
                mXAxis.MinorTicks = 8;
            }

            // draws X ticks!
            for (double currentTick = startTick; currentTick <= mXAxis.Max; )
            {

                PointF drawPt = new PointF((float)graphToBox_X(currentTick), 0.0f);
                PointF textPt = new PointF(drawPt.X, 0.0f);
                string tickLabel = Math.Round(currentTick, 8).ToString();

                textPt.Y = mBoxRect.Bottom;
                textPt.X -= gr.MeasureString(tickLabel, mXAxis.AxisFont).Width / 2;

                try
                {
                    // draws major ticks
                    if (drawPt.X >= mBoxRect.Left && drawPt.X <= mBoxRect.Right)
                    {
                        gr.DrawLine(mGraphPen, new PointF(drawPt.X, mBoxRect.Bottom),
                                               new PointF(drawPt.X, mBoxRect.Bottom - mMajorTickSize - 2));
                    }
                }
                catch (Exception ex)
                {
                    if (mThrowError)
                        throw ex;
                    else
                    {
                        mErrorCondition = true;
                        mErrorMessage = "When drawing major X tick at " + currentTick + ":\n" + ex.Message;
                    }
                }
                for (int i = 1; i <= mXAxis.MinorTicks; i++)
                {
                    if (!mXAxis.Logarithmic)
                        drawPt.X = graphToBox_X((float)currentTick + i * (float)mXAxis.MajorSpacing / (mXAxis.MinorTicks + 1));
                    else
                        drawPt.X = graphToBox_X((float)currentTick * (i + 1));


                    try
                    {
                        // draws minor ticks
                        if (drawPt.X >= mBoxRect.Left && drawPt.X <= mBoxRect.Right)
                        {
                            gr.DrawLine(mGraphPen, new PointF(drawPt.X, mBoxRect.Bottom),
                                                   new PointF(drawPt.X, mBoxRect.Bottom - mMinorTickSize - 2));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (mThrowError)
                            throw ex;
                        else
                        {
                            mErrorCondition = true;
                            mErrorMessage = "When drawing minor X tick at " + currentTick + " at graphics context coordinate " + drawPt.X + ":\n" + ex.Message;
                        }
                    }
                }

                if (currentTick >= mXAxis.Min)
                    gr.DrawString(tickLabel, mXAxis.AxisFont, fontBrush, textPt);


                if (!mXAxis.Logarithmic)
                    currentTick += mXAxis.MajorSpacing;
                else
                    currentTick *= 10;
            }


            // do y ticks
            if (!mYAxis.Logarithmic)
                startTick = mYAxis.MajorSpacing * ((int)(mYAxis.Min / mYAxis.MajorSpacing));
            else
            {
                startTick = Math.Pow(10, Math.Floor(Math.Log10(mYAxis.Min)));
                mYAxis.MinorTicks = 8;
            }

            // draws Y ticks!
            for (double currentTick = startTick; currentTick <= mYAxis.Max; )
            {
                //        PointF drawPt = graphToBox(new PointF(0.0f, (float)currentTick));
                //        PointF textPt = new PointF(0.0f, drawPt.Y);
                //        string tickLabel = "" + currentTick;

                //        textPt.Y += box.Top - form.MeasureString(tickLabel, AxisFont).Height / 2;
                //        textPt.X += box.Left - form.MeasureString(tickLabel, AxisFont).Width - 2;

                PointF drawPt = new PointF(0.0f, (float)graphToBox_Y(currentTick));
                PointF textPt = new PointF(0.0f, drawPt.Y);
                string tickLabel = Math.Round(currentTick, 8).ToString();
                SizeF labelSize = gr.MeasureString(tickLabel, mYAxis.AxisFont);

                textPt.Y -= labelSize.Height / 2;
                textPt.X += mBoxRect.Left - labelSize.Width - 2;

                try
                {
                    // draws major ticks
                    if (drawPt.Y >= mBoxRect.Top && drawPt.Y <= mBoxRect.Bottom)
                    {
                        gr.DrawLine(mGraphPen, new PointF(mBoxRect.Left, drawPt.Y),
                                          new PointF(mBoxRect.Left + mMajorTickSize, drawPt.Y));
                    }
                }
                catch (Exception ex)
                {
                    if (mThrowError)
                        throw ex;
                    else
                    {
                        mErrorCondition = true;
                        mErrorMessage = "When drawing major Y tick at " + currentTick + ":\n" + ex.Message;
                    }
                }
                for (int i = 1; i <= mYAxis.MinorTicks; i++)
                {
                    if (!mYAxis.Logarithmic)
                        drawPt.Y = graphToBox_Y((float)currentTick + i * (float)mYAxis.MajorSpacing / (mYAxis.MinorTicks + 1));
                    else
                        drawPt.Y = graphToBox_Y((float)currentTick * (i + 1));


                    try
                    {
                        // draws minor ticks
                        if (drawPt.Y >= mBoxRect.Top && drawPt.Y <= mBoxRect.Bottom)
                        {
                            gr.DrawLine(mGraphPen, new PointF(mBoxRect.Left, drawPt.Y),
                                                   new PointF(mBoxRect.Left + mMinorTickSize, drawPt.Y));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (mThrowError)
                            throw ex;
                        else
                        {
                            mErrorCondition = true;
                            mErrorMessage = "When drawing minor Y tick at " + currentTick + " at graphics context coordinate " + drawPt.Y + ":\n" + ex.Message;
                        }
                    }
                }

                if (currentTick >= mYAxis.Min)
                    gr.DrawString(tickLabel, mYAxis.AxisFont, fontBrush, textPt);


                if (!mYAxis.Logarithmic)
                    currentTick += mYAxis.MajorSpacing;
                else
                    currentTick *= 10;
            }
        }
        private void PaintDatasets(Graphics boxGraphics, int width, int height)
        {
            boxGraphics.SetClip(mBoxRect);

            foreach (GraphDataSet current in DataSets)
            {
                if (current.DrawDataset == false)
                    continue;

                try
                {
                    Pen pen = new Pen(current.Color, current.LineWeight);
                    Brush brush = new SolidBrush(current.Color);

                    // lines can be dashed, so make sure to do that.
                    switch (current.LineStyle)
                    {
                        case GraphDataSet.LineType.Dashed:
                            pen.DashPattern = new float[] { 2, 2 };
                            break;

                        case GraphDataSet.LineType.Dotted:
                            pen.DashPattern = new float[] { 1, 1, };
                            break;

                        default:
                            break;
                    }

                    // draw the curve first.
                    if (current.DrawLine)
                    {
                        List<PointF> points = new List<PointF>();

                        for (int i = 0; i < current.Points.Count; i++)
                        {
                            PointF curvePt = current.Points[i];
                            PointF drawPt = graphToBox(curvePt);

                            if (float.IsNaN(drawPt.X) || float.IsNaN(drawPt.Y) ||
                                float.IsInfinity(drawPt.X) || float.IsInfinity(drawPt.Y))
                            {
                                if (points.Count > 0)
                                    boxGraphics.DrawLines(pen, points.ToArray());

                                points.Clear();
                            }
                            else
                                points.Add(drawPt);

                        }

                        if (points.Count > 0)
                            boxGraphics.DrawLines(pen, points.ToArray());
                    }


                    // now draw points

                    if (current.DrawPoints)
                    {
                        // point symbols shouldn't be dashed or anything, so recreate the pen.
                        pen = new Pen(current.Color, current.LineWeight);

                        for (int i = 0; i < current.Points.Count; i++)
                        {
                            PointF curvePt = current.Points[i];
                            PointF drawPt = graphToBox(curvePt);

                            Rectangle box = GetPointBox(drawPt, current);

                            if (float.IsNaN(drawPt.X) || float.IsNaN(drawPt.Y) ||
                                float.IsInfinity(drawPt.X) || float.IsInfinity(drawPt.Y))

                                continue;

                            switch (current.PointType)
                            {
                                case GraphDataSet.SymbolType.Square:

                                    boxGraphics.DrawRectangle(pen, box);

                                    break;

                                case GraphDataSet.SymbolType.Circle:

                                    boxGraphics.DrawEllipse(pen, box);

                                    break;

                                case GraphDataSet.SymbolType.Point:

                                    boxGraphics.FillEllipse(brush, box);

                                    break;
                            }
                        }
                    }

                }
                catch (OverflowException ex)
                {
                    // don't do shit here.
                    System.Diagnostics.Debug.WriteLine(ex.Message);

                }
                catch (Exception ex)
                {
                    if (mThrowError)
                        throw ex;
                    else
                    {
                        mErrorCondition = true;
                        mErrorMessage = ex.Message;
                    }
                }
            }

            boxGraphics.SetClip(new Rectangle(0, 0, width, height));

        }

        private void UpdateBoxRect(Graphics gr, int width, int height)
        {

            if (ShowTitle)
            {
                int titleHeight = (int)(gr.MeasureString(mTitleText, mTitleFont).Height + 0.9);
                int subTitleHeight = (int)(gr.MeasureString(mSubTitleText, mSubTitleFont).Height + 0.9);

                mTitleRect = new Rectangle(
                        mBoxRect.Left,
                        0,
                        mBoxRect.Width,
                        titleHeight);

                if (ShowSubTitle)
                {
                    mSubTitleRect = new Rectangle(
                        mBoxRect.Left,
                        mTitleRect.Bottom,
                        mBoxRect.Width,
                        subTitleHeight);


                }
                else
                {
                    mSubTitleRect = new Rectangle(
                        mBoxRect.Left,
                        mTitleRect.Bottom,
                        mBoxRect.Width,
                        0);
                }

                mBoxRect.Location = new Point(mMarginLeft, mMarginTop + mSubTitleRect.Bottom);

                mBoxRect.Size = new Size(width - mMarginRight - mMarginLeft,
                                         height - mSubTitleRect.Bottom - mMarginBottom - mMarginTop);

                mTitleRect = new Rectangle(
                        mBoxRect.Left,
                        0,
                        mBoxRect.Width,
                        titleHeight);

                mSubTitleRect = new Rectangle(
                        mBoxRect.Left,
                        mTitleRect.Bottom,
                        mBoxRect.Width,
                        subTitleHeight);
            }
            else
            {
                mBoxRect.Location = new Point(mMarginLeft, mMarginTop);

                mBoxRect.Size = new Size(Width - mMarginRight - mMarginLeft,
                                    Height - mMarginBottom - mMarginTop);
            }

            UpdateControls();
        }

        private Rectangle GetPointBox(PointF pt, GraphDataSet current)
        {
            float size = current.SymbolSize;

            Rectangle retval = new Rectangle(
                (int)(pt.X - size / 2), (int)(pt.Y - size / 2),
                (int)size, (int)size);

            return retval;
        }

        private bool CheckUserEditControls(Point point)
        {
            if (!UserEditable)
                return false;

            Control[] userEditControls = new Control[] { txtMaxX, txtMaxY, txtMinX, txtMinY };

            foreach (Control txt in userEditControls)
            {
                if (FocusControl(txt, point))
                    return true;
            }

            return false;
        }
        private bool FocusControl(Control box, Point point)
        {
            Rectangle boxRect = new Rectangle(box.Location, box.Size);

            if (boxRect.Contains(point))
            {
                box.Visible = true;
                box.Focus();

                return true;
            }
            else
                return false;

        }

        private void txtBox_VisibleChanged(object sender, EventArgs e)
        {
            /*
            TextBox t = sender as TextBox;


            if (t.Visible)
            {
                if (sender == txtMaxX)                    t.Text = mXAxis.Max.ToString();
                else if (sender == txtMinX)               t.Text = mXAxis.Min.ToString();
                else if (sender == txtMaxY)               t.Text = mYAxis.Max.ToString();
                else if (sender == txtMinY)               t.Text = mYAxis.Min.ToString();

            }
             * */
        }

        private void txtMaxX_Validating(object sender, CancelEventArgs e)
        {
            double val;


            if (double.TryParse(txtMaxX.Text, out val))
            {
                mXAxis.Max = val;
                mXAxis.AutoSetMax = false;

                if (mXAxis.Max != val)
                {
                    System.Media.SystemSounds.Beep.Play();
                    e.Cancel = true;
                }
            }
            else if (txtMaxX.Text == "")
            {
                mXAxis.AutoSetMax = true;

                AutoSetAxisLimits();
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
                e.Cancel = true;
            }
        }
        private void txtMinX_Validating(object sender, CancelEventArgs e)
        {
            double val;

            if (double.TryParse(txtMinX.Text, out val))
            {
                mXAxis.Min = val;
                mXAxis.AutoSetMin = false;

                if (mXAxis.Min != val)
                {
                    System.Media.SystemSounds.Beep.Play();
                    e.Cancel = true;
                }

                AutoSetAxisLimits();

            }
            else if (txtMinX.Text == "")
            {
                mXAxis.AutoSetMin = true;
                AutoSetAxisLimits();
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
                e.Cancel = true;
            }
        }
        private void txtMinY_Validating(object sender, CancelEventArgs e)
        {
            double val;

            if (double.TryParse(txtMinY.Text, out val))
            {
                mYAxis.Min = val;
                mYAxis.AutoSetMin = false;

                if (mYAxis.Min != val)
                {
                    System.Media.SystemSounds.Beep.Play();
                    e.Cancel = true;
                }
            }
            else if (txtMinY.Text == "")
            {
                mYAxis.AutoSetMin = true;
                AutoSetAxisLimits();
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
                e.Cancel = true;
            }
        }
        private void txtMaxY_Validating(object sender, CancelEventArgs e)
        {
            double val;

            if (double.TryParse(txtMaxY.Text, out val))
            {
                mYAxis.Max = val;
                mYAxis.AutoSetMax = false;

                if (mYAxis.Max != val)
                {
                    System.Media.SystemSounds.Beep.Play();
                    e.Cancel = true;
                }
            }
            else if (txtMaxY.Text == "")
            {
                mYAxis.AutoSetMax = true;
                AutoSetAxisLimits();
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
                e.Cancel = true;
            }
        }
        private void txtMinY_Validated(object sender, EventArgs e)
        {
            txtMinY.Visible = false;

            AutoSetTicks();
        }
        private void txtMaxY_Validated(object sender, EventArgs e)
        {
            txtMaxY.Visible = false;

            AutoSetTicks();
        }
        private void txtMaxX_Validated(object sender, EventArgs e)
        {
            txtMaxX.Visible = false;

            AutoSetTicks();
        }
        private void txtMinX_Validated(object sender, EventArgs e)
        {
            txtMinX.Visible = false;

            AutoSetTicks();
        }

        private void txtMaxY_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                CancelEventArgs c = new CancelEventArgs();

                txtMaxY_Validating(sender, c);

                if (c.Cancel == false)
                {
                    txtMaxY_Validated(sender, EventArgs.Empty);

                    e.Handled = true;
                }
            }
        }
        private void txtMinY_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                CancelEventArgs c = new CancelEventArgs();

                txtMinY_Validating(sender, c);

                if (c.Cancel == false)
                {
                    txtMinY_Validated(sender, EventArgs.Empty);

                    e.Handled = true;
                }
            }
        }
        private void txtMinX_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                CancelEventArgs c = new CancelEventArgs();

                txtMinX_Validating(sender, c);

                if (c.Cancel == false)
                {
                    txtMinX_Validated(sender, EventArgs.Empty);

                    e.Handled = true;
                }
            }
        }
        private void txtMaxX_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                CancelEventArgs c = new CancelEventArgs();

                txtMaxX_Validating(sender, c);

                if (c.Cancel == false)
                {
                    txtMaxX_Validated(sender, EventArgs.Empty);

                    e.Handled = true;
                }
            }
        }

        private void ClickOffTextBox(object sender, EventArgs e)
        {
            // fake out the event handler into thinking that enter was pressed in the text box.
            KeyPressEventArgs eve = new KeyPressEventArgs('\r');

            if (txtMinX.Visible == true) txtMinX_KeyPress(txtMinX, eve);
            if (txtMaxX.Visible == true) txtMaxX_KeyPress(txtMaxX, eve);
            if (txtMinY.Visible == true) txtMinY_KeyPress(txtMinY, eve);
            if (txtMaxY.Visible == true) txtMaxY_KeyPress(txtMaxY, eve);

        }


        private void mnuShowTitle_Click(object sender, EventArgs e)
        {
            ShowTitle = !ShowTitle;
        }
        private void txtTitle_TextChanged(object sender, EventArgs e)
        {
            Title = txtTitle.Text;
        }


        public class GraphMouseEventArgs : EventArgs
        {
            public MouseButtons Button;
            public int Clicks;
            public int Delta;

            public Point ScreenLocation;       // location of the mouse on the screen
            public PointF GraphLocation;       // location of the mouse within the coordinate system of the graph

            public int ScreenX
            {
                get { return ScreenLocation.X; }
            }
            public int ScreenY
            {
                get { return ScreenLocation.Y; }
            }

            public float GraphX
            {
                get { return GraphLocation.X; }
            }
            public float GraphY
            {
                get { return GraphLocation.Y; }
            }

            public class GraphDataSetHover
            {
                public GraphDataSet dataSet;
                public int pointIndex;
                public PointF point;
            }

            /// <summary>
            /// List of datasets and points the mouse is hovering over.
            /// </summary>
            public List<GraphDataSetHover> DataSetIndices = new List<GraphDataSetHover>();
        }
        public class GraphDataSetEventArgs : EventArgs
        {
            public Graph Graph;
            public GraphDataSet GraphDataSet;
        }

        public delegate void MouseGraphEventHandler(object sender, GraphMouseEventArgs e);
        public delegate void GraphDataSetEventHandler(object sender, GraphDataSetEventArgs e);

        public event MouseGraphEventHandler GraphMouseClick;
        public event MouseGraphEventHandler GraphMouseDoubleClick;
        public event MouseGraphEventHandler GraphMouseDown;
        public event EventHandler GraphMouseEnter;
        public event EventHandler GraphMouseHover;
        public event EventHandler GraphMouseLeave;
        public event MouseGraphEventHandler GraphMouseMove;
        public event MouseGraphEventHandler GraphMouseUp;
        public event GraphDataSetEventHandler AddedDataset;


        private GraphMouseEventArgs getMouseEventArgs(MouseEventArgs e)
        {
            GraphMouseEventArgs ret = new GraphMouseEventArgs();

            ret.Button = e.Button;
            ret.Clicks = e.Clicks;
            ret.Delta = e.Delta;
            ret.ScreenLocation = new Point(e.Location.X, e.Location.Y);

            ret.GraphLocation = boxToGraph(e.Location);

            PointF origin = boxToGraph(new PointF(0, 0));
            PointF one = boxToGraph(new PointF(1, 1));

            PointF pixelSize = new PointF(Math.Abs(one.X - origin.X), Math.Abs(one.Y - origin.Y));

            for (int index = 0; index < DataSets.Count; index++)
            {
                GraphDataSet d = DataSets[index];
                PointF diff = new PointF();

                for (int j = 0; j < d.Points.Count; j++)
                {
                    diff.X = ret.GraphLocation.X - d.Points[j].X;
                    diff.Y = ret.GraphLocation.Y - d.Points[j].Y;

                    if (Math.Abs(diff.X) < pixelSize.X * mMouseRadius &&
                        Math.Abs(diff.Y) < pixelSize.Y * mMouseRadius)
                    {
                        GraphMouseEventArgs.GraphDataSetHover val = new GraphMouseEventArgs.GraphDataSetHover();

                        val.dataSet = d;
                        val.pointIndex = j;
                        val.point = d.Points[j];

                        ret.DataSetIndices.Add(val);

                        break;
                    }
                }
            }

            return ret;
        }

        private void autoScaleAxesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mXAxis.AutoSetMax = true;
            mXAxis.AutoSetMin = true;

            mYAxis.AutoSetMax = true;
            mYAxis.AutoSetMin = true;

            UpdateGraph();
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                }
            }
            catch (Exception error)
            {
                MessageBox.Show("An error has occurred when trying to print: \r\n" + error.Message, "Printing Error");
            }
        }
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (printPreviewDialog.ShowDialog() == DialogResult.OK)
            {

            }
        }

        private void printDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {


            //e.Graphics.ScaleTransform(e.PageSettings.Bounds.Width / (float)Width,
            //                          e.PageSettings.Bounds.Height / (float)Height);

            // scale to get fonts to look a decent size
            e.Graphics.TranslateTransform(e.MarginBounds.Left, e.MarginBounds.Top);
            e.Graphics.ScaleTransform(2.0f, 2.0f);

            PaintToGraphics(e.Graphics, e.MarginBounds.Width / 2, e.MarginBounds.Height / 2);

        }

        private void SetDefaultPrintDocumentSettings()
        {
			try
			{
	            pageSetupDialog.PageSettings.Landscape = true;
	
	            pageSetupDialog.PageSettings.Margins.Left = 50;
	            pageSetupDialog.PageSettings.Margins.Right = 50;
	            pageSetupDialog.PageSettings.Margins.Bottom = 50;
	            pageSetupDialog.PageSettings.Margins.Top = 50;
			}
			catch(System.Drawing.Printing.InvalidPrinterException)
			{
				
			}
        }

        private void pageSetupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pageSetupDialog.ShowDialog();
        }

        private void editDatasetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mDataSetEditor == null || mDataSetEditor.IsDisposed)
            {
                mDataSetEditor = new frmDataSetEditor(this);
            }

            mDataSetEditor.Show();
        }

        private void verticalAxisToolStripMenuItem_Click(object sender, EventArgs e)
        {

            frmAxisEditor frm = new frmAxisEditor(this, VerticalAxis, true);

            frm.ShowDialog();

        }

        private void horizontalAxisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAxisEditor frm = new frmAxisEditor(this, HorizontalAxis, false);

            frm.ShowDialog();

        }

        #region --- Exporting ---

        private void textToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFile.Title = "Export Datasets to text file...";

            saveFile.OverwritePrompt = true;
            saveFile.Filter = "Text File (*.txt)|*.txt|Comma separated values (*.csv)|*.csv|All Files (*.*)|*.*";

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                // export data to text.
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(saveFile.FileName))
                {
                    writer.WriteLine("# Datasets exported on " + DateTime.Now.ToString());

                    foreach (GraphDataSet data in mDataSets)
                    {
                        if (data.DrawDataset == false)
                            continue;

                        writer.WriteLine("# " + data.Name);

                        for (int i = 0; i < data.Points.Count; i++)
                        {
                            writer.WriteLine(data.Points[i].X.ToString() + "\t" + data.Points[i].Y.ToString());
                        }

                        writer.WriteLine();
                    }
                }
            }
        }
        private void graphToagrusedByXMGraceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFile.Title = "Export graph to Grace file...";

            saveFile.OverwritePrompt = true;
            saveFile.Filter = "XM/Grace File (*.agr)|*.agr|All Files (*.*)|*.*";

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                // export to agr.
                using (AGRWriter writer = new AGRWriter())
                {
                    writer.Open(saveFile.FileName);

                    writer.mDataSets = mDataSets;

                    writer.xmin = HorizontalAxis.Min;
                    writer.xmax = HorizontalAxis.Max;

                    writer.ymin = VerticalAxis.Min;
                    writer.ymax = VerticalAxis.Max;

                    if (ShowTitle)
                        writer.title = Title;
                    if (ShowSubTitle)
                        writer.subtitle = SubTitle;

                    writer.xAxisLabel = HorizontalAxis.Text;
                    writer.yAxisLabel = VerticalAxis.Text;

                    writer.xMajorTick = HorizontalAxis.MajorSpacing;
                    writer.xMinorTicks = HorizontalAxis.MinorTicks;

                    writer.yMajorTick = VerticalAxis.MajorSpacing;
                    writer.yMinorTicks = VerticalAxis.MinorTicks;

                    writer.Write();
                }
            }
        }
        private void imageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFile.Title = "Export graph to image...";

            saveFile.OverwritePrompt = true;
            saveFile.Filter = "PNG image (*.png)|*.png" +
                              "|GIF image (*.gif)|*.gif|JPEG image (*.jpg)|*.jpg,*.jpeg" +
                              "|24-bit Bitmap (*.bmp)|*.bmp|TIFF image (*.tiff)|*.tiff";
            //"Enhanced Metafile (*.emf)|*.emf|Windows Metafile (*.wmf)|*.wmf";

            saveFile.DefaultExt = "png";
            saveFile.AddExtension = true;

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                System.Drawing.Imaging.ImageFormat format = null;
                //bool metaFile = false;

                switch (saveFile.FilterIndex)
                {
                    case 1:
                        format = System.Drawing.Imaging.ImageFormat.Png;
                        break;

                    case 2:
                        format = System.Drawing.Imaging.ImageFormat.Gif;
                        break;

                    case 3:
                        format = System.Drawing.Imaging.ImageFormat.Jpeg;
                        break;

                    case 4:
                        format = System.Drawing.Imaging.ImageFormat.Bmp;
                        break;

                    case 5:
                        format = System.Drawing.Imaging.ImageFormat.Tiff;
                        break;

                    //case 6:
                    //    format = System.Drawing.Imaging.ImageFormat.Emf;
                    //    metaFile = true;
                    //    break;

                    //case 7:
                    //    format = System.Drawing.Imaging.ImageFormat.Wmf;
                    //    metaFile = true;
                    //    break;
                }


                // export to bitmap.
                Image im = new Bitmap(450, 450);

                Graphics gr = Graphics.FromImage(im);
                gr.Clear(Color.White);

                PaintToGraphics(gr, im.Width, im.Height);

                gr.Dispose();


                im.Save(saveFile.FileName, format);
            }
        }
        private void encapsulatedPostScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFile.Title = "Export graph to Encapsulated PostScript file...";

            saveFile.OverwritePrompt = true;
            saveFile.Filter = "Encapsulated Postscript File (*.eps)|*.eps";

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                // export graph to eps.
                ExportToEPS(saveFile.FileName);
            }
        }

        private void ExportToEPS(string filename)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(filename);


        }


        #endregion

    }
}