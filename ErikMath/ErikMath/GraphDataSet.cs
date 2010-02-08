using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace ERY.EMath
{
    public class GraphDataSet : ICollection<PointF>, IList<PointF>
    {
        public enum SymbolType
        {
            Square,
            Circle,
            Point,
        }

        public enum LineType
        {
            Solid,
            Dashed,
            Dotted,
        }

        private List<PointF> mPoints = new List<PointF>();
        private Color mColor = Color.Blue;

        private SymbolType mSymbolType = SymbolType.Square;
        private LineType mLineStyle = LineType.Solid;

        private bool mDrawPoints = false;
        private bool mDrawLine = true;
        private bool mDrawDataset = true;
        private bool mShowInLegend = true;

        private float mLineWeight = 1.0f;
        private float mSymbolSize = 6.0f;

        private string mName = "DataSet";

        internal GraphDataSet()
        {

        }

        public delegate float  CallBackFunctionF(float x);
        public delegate double CallBackFunctionD(double x);
        public delegate double CallBackFunctionD_param<T>(double x, T param);

        public List<PointF> Points
        {
            get { return mPoints; }
            set { mPoints = value; }
        }
        public bool DrawPoints
        {
            get { return mDrawPoints; }
            set { mDrawPoints = value; }
        }
        public SymbolType PointType
        {
            get { return mSymbolType; }
            set { mSymbolType = value; }
        }
        public LineType LineStyle
        {
            get { return mLineStyle; }
            set { mLineStyle = value; }
        }
        /// <summary>
        /// Gets or sets the size of the displayed symbol.
        /// If displayed symbols are characters, it's the font size, otherwise
        /// it's the diameter for circles or width for squares, etc.
        /// </summary>
        public float SymbolSize
        {
            get { return mSymbolSize; }
            set
            {
                if (value > 0)
                    mSymbolSize = value;
            }
        }

        public bool DrawDataset
        {
            get { return mDrawDataset; }
            set { mDrawDataset = value; }
        }
        public bool ShowInLegend
        {
            get { return mShowInLegend; }
            set { mShowInLegend = value; }
        }

        public bool DrawLine
        {
            get { return mDrawLine; }
            set { mDrawLine = value; }
        }
        public Color Color
        {
            get { return mColor; }
            set { mColor = value; }
        }
        public float LineWeight
        {
            get { return mLineWeight; }
            set
            {
                if (value > 0)
                    mLineWeight = value;
                else
                    throw new ArgumentOutOfRangeException("LineWeight must be greater than zero!");
            }
        }

        public string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        public void TranslateData(double shift_x, double shift_y)
        {
            TranslateData((float)shift_x, (float)shift_y);
        }
        public void TranslateData(float shift_x, float shift_y)
        {
            for (int i = 0; i < mPoints.Count; i++)
            {
                PointF pt = mPoints[i];

                pt.X += shift_x;
                pt.Y += shift_y;

                mPoints[i] = pt;
            }
        
        }

        public void ScaleData(double scale_x, double scale_y)
        {
            ScaleData((float)scale_x, (float)scale_y);
        }
        public void ScaleData(float scale_x, float scale_y)
        {
            for (int i = 0; i < mPoints.Count; i++)
            {
                PointF pt = mPoints[i];

                pt.X *= scale_x;
                pt.Y *= scale_y;

                mPoints[i] = pt;
            }
        }

        public void RaiseDataToPower(double power_y)
        {
            for (int i = 0; i < mPoints.Count; i++)
            {
                PointF pt = mPoints[i];

                pt.Y = (float) Math.Pow((double)pt.Y, power_y);

                mPoints[i] = pt;
            }
        }
        public void RaiseDataToPower(float power_y)
        {
            RaiseDataToPower((double)power_y);
        }
        
        public void FillData(double minX, double maxX, int pointCount, CallBackFunctionD func)
        {
            for (int i = 0; i < pointCount; i++)
            {
                double currentX = minX + i / ((double)pointCount - 1) * (maxX - minX);
                double value = func(currentX);

                mPoints.Add(new PointF((float)currentX, (float)value));
            }

        }
        public void FillData(float  minX, float  maxX, int pointCount, CallBackFunctionF func)
        {
            for (int i = 0; i < pointCount; i++)
            {
                float currentX = minX + i / ((float)pointCount - 1) * (maxX - minX);
                float value = func(currentX);

                mPoints.Add(new PointF(currentX, value));
            }

        }

        public void FillData<T>(double minX, double maxX, int pointCount, CallBackFunctionD_param<T> func, T param)
        {
            for (int i = 0; i < pointCount; i++)
            {
                double currentX = minX + i / ((double)pointCount - 1) * (maxX - minX);
                double value = func(currentX, param);

                mPoints.Add(new PointF((float)currentX, (float)value));
            }
        }

        public void FillParametricData(double minT, double maxT, int pointCount, CallBackFunctionD func_x, CallBackFunctionD func_y)
        {
            for (int i = 0; i < pointCount; i++)
            {
                double currentT = minT + i / ((double)pointCount - 1) * (maxT - minT);

                double x = func_x(currentT);
                double y = func_y(currentT);

                mPoints.Add(new PointF((float)x, (float)y));
            }
        }
        public void FillParametricData(float  minT, float  maxT, int pointCount, CallBackFunctionF func_x, CallBackFunctionF func_y)
        {
            for (int i = 0; i < pointCount; i++)
            {
                float currentT = minT + i / ((float)pointCount - 1) * (maxT - minT);

                float x = func_x(currentT);
                float y = func_y(currentT);

                mPoints.Add(new PointF(x, y));
            }
        }

        public void Add(double x, double y)
        {
            PointF pt = new PointF((float)x, (float)y);

            mPoints.Add(pt);
        }
        public void Add(float x, float y)
        {
            PointF pt = new PointF(x, y);

            mPoints.Add(pt);
        }
        public void AddRange(IEnumerable<PointF> points)
        {
            mPoints.AddRange(points);
        }

        #region --- ICollection<PointF> Members ---

        public void Add(PointF item)
        {
            mPoints.Add(item);
        }

        public void Clear()
        {
            mPoints.Clear();
        }

        public bool Contains(PointF item)
        {
            return mPoints.Contains(item);
        }

        public void CopyTo(PointF[] array, int arrayIndex)
        {
            mPoints.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return mPoints.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(PointF item)
        {
            return mPoints.Remove(item);
        }

        #endregion
        #region --- IEnumerable<PointF> Members ---

        public IEnumerator<PointF> GetEnumerator()
        {
            return mPoints.GetEnumerator();
        }

        #endregion
        #region --- IEnumerable Members ---

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
        #region --- IList<PointF> Members ---

        public int IndexOf(PointF item)
        {
            return mPoints.IndexOf(item);
        }

        public void Insert(int index, PointF item)
        {
            mPoints.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            mPoints.RemoveAt(index);
        }

        public PointF this[int index]
        {
            get
            {
                return mPoints[index];
            }
            set
            {
                mPoints[index] = value;
            }
        }

        #endregion
    }
}
