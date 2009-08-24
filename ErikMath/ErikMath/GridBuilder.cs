using System;
using System.Collections.Generic;
using System.Text;

namespace ERY.EMath
{
    public class GridBuilder
    {
        /// <summary>
        /// Creates a grid which is very dense near the lowest end, but sparse near the higher ends.
        /// Good for functions shich should be calculated on a very large range, but things vary slowly
        /// at large values of the independent coordinate.
        /// </summary>
        /// <param name="npoints">The number of points on the grid.  Must be greater than 1.</param>
        /// <param name="lowValue">The lowest value on the grid.</param>
        /// <param name="highValue">The highest value on the grid.  Must be higher than lowValue.</param>
        /// <returns>An array of the grid points.</returns>
        public static double[] CreateLogarithmicGrid(int npoints, double lowValue, double highValue)
        {
            CheckGridArgs(npoints, lowValue, highValue);

            // create a variable step grid 
            List<double> points = new List<double>();

            // there's some tricky stuff in here to make sure we are always taking logarithms of positive numbers
            double factor = Math.Log(highValue - lowValue + 1) / (npoints - 1);

            for (int i = 0; i < npoints; i++)
            {
                double gridPoint = Math.Exp((double)i * factor);

                gridPoint += lowValue - 1;

                points.Add(gridPoint);
            }

            return points.ToArray();

        }
        /// <summary>
        /// Creates a grid that is evenly divided between the two end points.  Both end points will
        /// be included on the grid.
        /// </summary>
        /// <param name="npoints">The number of points on the grid.  Must be greater than 1.</param>
        /// <param name="lowValue">The lowest value on the grid.</param>
        /// <param name="highValue">The highest value on the grid.  Must be higher than lowValue.</param>     
        /// <returns>An array of the grid points.</returns>
        public static double[] CreateLinearGrid(int npoints, double lowValue, double highValue)
        {
            CheckGridArgs(npoints, lowValue, highValue);
            
            List<double> points = new List<double>();

            double step = (highValue - lowValue) / (npoints - 1);

            for (int i = 0; i < npoints; i++)
            {
                double value = i * step + lowValue;

                points.Add(value);
            }

            return points.ToArray();
        }

        /// <summary>
        /// Creates a grid that is evenly divided between the two end points, with the specified space
        /// between grid points.
        /// </summary>
        /// <param name="lowValue">The lowest point on the grid.  This will always be included.</param>
        /// <param name="stepSize">The amount of spacing between each grid point.</param>
        /// <param name="highValue">The highest point on the grid.  May or may not be included, depending
        /// on whether (highValue - lowValue) is a multiple of stepSize.  If this value is not included,
        /// a single point above it will be included.</param>
        /// <returns>An array of the grid points.</returns>
        public static double[] CreateLinearGridWithStepSize(double lowValue, double stepSize, double highValue)
        {
            CheckGridArgsStepSize(lowValue, stepSize, highValue);

            List<double> points = new List<double>();
            double current = lowValue;
            int steps = 0;
            
            while (current < highValue)
            {
                current = steps * stepSize + lowValue;

                points.Add(current);

                steps++;

            }

            return points.ToArray();
        }

        private static void CheckGridArgsStepSize(double lowValue, double stepSize, double highValue)
        {
            if (highValue <= lowValue)
                throw new ArgumentException("Error: highValue must be greater than lowValue.");
            if (stepSize <= 0)
                throw new ArgumentException("Error: stepSize must be greater than zero.");
        }


        private static void CheckGridArgs(int npoints, double lowValue, double highValue)
        {
            if (highValue <= lowValue)
                throw new ArgumentException("Error: highValue must be greater than lowValue.");
            else if (npoints <= 1)
                throw new ArgumentException("Error: The number of points specified must be greater than 1.");
        }
    }
}
