using System;
using System.Collections.Generic;
using System.Text;

namespace ERY.EMath
{
    [Serializable]
    public class FunctionProcessor
    {
        private double mTolerance = 1e-10;
        private int mMaxIterations = 100;

        public delegate double FunctionOneVar(double x);
        public delegate Complex FunctionComplexOneVar(double x);

        public double Tolerance
        {
            get { return mTolerance; }
            set { mTolerance = Math.Abs(value); }
        }
        public int MaxIterations
        {
            get { return mMaxIterations; }
            set
            {
                if (value < 1)
                    throw new Exception("Error: MaxIterations must be positive!");

                mMaxIterations = value;
            }
        }

        #region --- Finding the point with a specified value ---

        /// <summary>
        /// Finds the root of the given function using the bisection algorithm.
        /// </summary>
        /// <param name="f">The function whose root we are supposed to find.</param>
        /// <param name="low">The lower end of the range within to look for a root.</param>
        /// <param name="high">The upper end of the range within to look for a root.</param>
        /// <returns></returns>
        public double RootByBisection(FunctionOneVar f, double low, double high)
        {
            return s_RootByBisection(f, low, high, mTolerance, mMaxIterations);
        }
        /// <summary>
        /// Finds the point at which given function is at the given value, using the bisection algorithm.
        /// </summary>
        /// <param name="f">The function whose root we are supposed to find.</param>
        /// <param name="low">The lower end of the range within to look for a root.</param>
        /// <param name="high">The upper end of the range within to look for a root.</param>
        /// <param name="value">The value to look for.</param>
        /// <returns></returns>
        public double ValueByBisection(FunctionOneVar f, double low, double high, double value)
        {
            mFunctionToCall = f;
            mFunctionValue = value;

            double retval = s_RootByBisection(pValueFunction, low, high, mTolerance, mMaxIterations);

            mFunctionToCall = null;
            mFunctionValue = 0;

            return retval;
        }

        #region --- Temporary variables for ValueByBisection call ---

        [NonSerialized] private FunctionOneVar mFunctionToCall;
        [NonSerialized] private double mFunctionValue;

        private double pValueFunction(double x)
        {
            return mFunctionToCall(x) - mFunctionValue;
        }

        #endregion

        /// <summary>
        /// Finds the root of a passed function.  Returns the value of x for which f(x) = 0.
        /// </summary>
        /// <param name="f">The function whose root we are supposed to find.</param>
        /// <param name="low">The lower end of the range within to look for a root.</param>
        /// <param name="high">The upper end of the range within to look for a root.</param>
        /// <param name="maxIterations">The maximum number of iterations in which to look.</param>
        /// <param name="tolerance">The level of tolerance at which the result should differ from the 
        /// correct value.  The actual value used is (tolerance * (f(high) - f(low) / 2)). </param>
        /// <returns></returns>
        public static double s_RootByBisection(FunctionOneVar f, double low, double high, double tolerance, int maxIterations)
        {
            double x = (low + high) / 2.0;
            double f_val = f(x);
            double f_low = f(low);

            while (double.IsNaN(f_low))
            {
                low += (high - low) / 1000.0;

                f_low = f(low);
            }

            double f_high = f(high);

            while (double.IsNaN(f_high))
            {
                high -= (high - low) / 1000.0;

                f_high = f(high);
            }

            double tol = Math.Abs(tolerance * (f_high - f_low) / 2);

            if (f_low * f_high >= 0)
                throw new Exception("Error: Range given is not guaranteed to bracket a zero.");

            int nIterations = 0;

            do
            {
                f_val = f(x);

                if (f_val * f_low < 0)
                {
                    high = x;
                    f_high = f_val;
                }
                else if (f_val * f_high < 0)
                {
                    low = x;
                    f_low = f_val;
                }

                x = (low + high) / 2.0;

                nIterations++;

            } while (nIterations < maxIterations && Math.Abs(f_val) > tol) ;

            return x;
        }

        #endregion

        #region --- Finding the maximum or minimum ---

        public double MinimumByBisection(FunctionOneVar f, double low, double high)
        {
            return s_MinimumByBisection(f, low, high, Tolerance, MaxIterations);
        }

        public static double s_MinimumByBisection(FunctionOneVar f, double x_low, double x_high, double tolerance, int maxIterations)
        {
            double x_center = (x_low + x_high) / 2.0;
            double f_center = f(x_center);
            double f_low = f(x_low);
            double f_high = f(x_high);
            double tol = tolerance * (f_high - f_low) / 2;

            int nIterations = 0;

            // check to see if we've failed to bracket a minimum.
            while (nIterations < maxIterations && f_center > f_low)
            {
                x_high = x_center;
                f_high = f_center;

                x_center = (x_low + x_high) / 2.0;
                f_center = f(x_center);

                nIterations++;
            }
            while (nIterations < maxIterations && f_center > f_high)
            {
                x_low = x_center;
                f_low = f_center;

                x_center = (x_low + x_high) / 2.0;
                f_center = f(x_center);

                nIterations++;
            }

            do
            {
                // examine right bracket
                double x = (x_high + x_center) / 2;
                double f_val = f(x);

                if (f_center < f_val)
                {
                    x_high = x;
                    f_high = f_val;
                }
                else
                {
                    x_low = x_center;
                    x_center = x;

                    f_low = f_center;
                    f_center = f_val;
                }

                // examine left bracket
                x = (x_low + x_center) / 2;
                f_val = f(x);

                if (f_center < f_val)
                {
                    x_low = x;
                    f_low = f_val;
                }
                else
                {
                    x_high = x_center;
                    x_center = x;

                    f_high = f_center;
                    f_center = f_val;
                }

                nIterations++;

            } while (nIterations < maxIterations && Math.Abs(f_high - f_low) > tol);

            return x_center;
        }

        #endregion

        #region --- Interpolation ---

        public double Interpolate(double[] x, double[] y, double newx)
        {
            return s_Interpolate(x, y, newx);
        }

        public static double s_Interpolate(double[] x, double[] y, double newx)
        {
            if (x.Length != y.Length)
                throw new ArgumentException("Error: The two arrays must be the same length!");
            if (x.Length == 0)
                throw new ArgumentException("Error: The arrays must have something in them!");

            // make sure x values are monotonic
            int sign = 0;

            for (int i = 0; i < x.Length - 1; i++)
            {
                if (x[i] < x[i + 1])
                {
                    if (sign == 0)
                        sign = 1;
                    else if (sign == -1)
                        throw new ArgumentException("Error: The x values must be monotonic!");
                }
                else if (x[i] > x[i + 1])
                {
                    if (sign == 0)
                        sign = -1;
                    else if (sign == 1)
                        throw new ArgumentException("Error: The x values must be monotonic!");
                }
            }

            if (sign == -1)
            {
                List<double> newxList = new List<double>();
                List<double> newyList = new List<double>();

                for (int i = x.Length - 1; i >= 0; i--)
                {
                    newxList.Add(x[i]);
                    newyList.Add(y[i]);
                }

                return s_Interpolate(newxList.ToArray(), newyList.ToArray(), newx);

            }

            // bracket the value in n, using bisection.
            int left = 0;
            int right = x.Length;
            int current = x.Length / 2;

            do
            {
                double current_x = x[current];

                if (current_x > newx)
                {
                    right = current;
                }
                else if (current_x < newx)
                {
                    left = current;
                }
                else
                    break;

                current = (left + right) / 2;

                if (current == 0)
                    break;
                else if (current == x.Length - 1)
                    break;

                // this condition should read
                // until current_n is below n and current+1_n is above n.
            } while (!(x[current] <= newx && x[current + 1] >= newx));

            if (x[current] == newx)
            {
                return y[current];
            }
            else if (x[current + 1] == newx)
                return y[current + 1];

            // now we need to linearly interpolate, because neither one is the exact value we want.
            double left_x = x[current];
            double right_x = x[current + 1];
            double left_y = y[current];
            double right_y = y[current + 1];

            if (double.IsNaN(left_y) || double.IsInfinity(left_y))
                return right_y;
            else if (double.IsNaN(right_y) || double.IsInfinity(right_y))
                return left_y;

            double result = s_TwoPointInterpolate(newx, left_x, left_y, right_x, right_y);

            if (newx > left_x && newx < right_x)
            {
                // make sure the result is between the two endpoints.
                System.Diagnostics.Debug.Assert(Math.Min(left_y, right_y) <= result && result <= Math.Max(left_y, right_y));
            }

            return result;

        }

        public static double s_TwoPointInterpolate(double newx, double left_x, double left_y, double right_x, double right_y)
        {
            double rightFactor = (newx - left_x) / (right_x - left_x);

            return left_y + rightFactor * (right_y - left_y);
        }
       

        #endregion 

        #region --- Calculus ---
        
        public static double Derivative(FunctionOneVar f, double x, double step)
        {
            double val = f(x);
            
            if (double.IsNaN(val) || double.IsInfinity(val))
                return double.NaN;

            double valRight = f(x + step);
            double valLeft = f(x - step);

            // if they're both NaN, we can't do anything.
            if (double.IsNaN(valRight) && double.IsNaN(valLeft))
                return double.NaN;
                
            // if only one is NaN, we can still do a first order derivative.
            else if (double.IsNaN(valLeft) || double.IsInfinity(valLeft))
            {
                valLeft = val;

                return (valRight - valLeft) / step;
            }
            else if (double.IsNaN(valRight) || double.IsInfinity(valRight))
            {
                valRight = val;

                return (valRight - valLeft) / step;
            }
            // wel, everything seems good here:
            else
                return (valRight - valLeft) / (2 * step);
        }

        public static double Integrate(FunctionOneVar f, double low, double high, int steps)
        {
            return IntegrateSimpsonsRule(f, low, high, steps);
            //return IntegrateTrapezoidRule(f, low, high, steps);
       
        }
        
        private static double IntegrateSimpsonsRule(FunctionOneVar f, double low, double high, int steps)
        {
            double retval = 0;
            double stepSize = (high - low) / (double)steps;
            double leftVal = 0;
            double midVal = 0;
            double rightVal = 0;

            leftVal = f(low);

            if (steps % 2 == 1)
                steps++;

            for (int i = 2; i <= steps; i += 2)
            {
                double xmid = low + (i - 1) * stepSize;
                double xright = low + i * stepSize;

                midVal = f(xmid);
                rightVal = f(xright);

                retval += (leftVal + 4 * midVal + rightVal) * stepSize / 3.0;

                leftVal = rightVal;

            }

            return retval;
        }

        private static double IntegrateTrapezoidRule(FunctionOneVar f, double low, double high, int steps)
        {
            double retval = 0;
            double stepSize = (high - low) / (double)steps;
            double leftVal = 0;
            double rightVal = 0;

            leftVal = f(low);

            for (int i = 1; i <= steps; i++)
            {
                double x = low + i * stepSize;

                rightVal = f(x);

                retval += 0.5 * (rightVal + leftVal) * stepSize;

                leftVal = rightVal;

            }

            return retval;
        }

        public static Complex IntegrateComplex(FunctionComplexOneVar f, double low, double high, int steps)
        {
            return IntegrateSimpsonsRule(f, low, high, steps);
            //return IntegrateTrapezoidRule(f, low, high, steps);
            
        }
        private static Complex IntegrateSimpsonsRule(FunctionComplexOneVar f, double low, double high, int steps)
        {
            Complex retval = 0;
            double stepSize = (high - low) / (double)steps;
            Complex leftVal = 0;
            Complex midVal = 0;
            Complex rightVal = 0;

            leftVal = f(low);

            if (steps % 2 == 1)
                steps++;

            for (int i = 2; i <= steps; i += 2)
            {
                double xmid = low + (i - 1) * stepSize;
                double xright = low + i * stepSize;

                midVal = f(xmid);
                rightVal = f(xright);

                retval += (leftVal + 4 * midVal + rightVal) * stepSize / 3.0;

                leftVal = rightVal;

            }

            return retval;
        }

        #endregion

    }
}
