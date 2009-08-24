using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace TestMath
{
	class Program
	{
		static void Main(string[] args)
		{
			const int size = 3;
			Random r = new Random(1234);
			Matrix H = new Matrix(size, size);

			//H[1, 2] = new Complex(0, 0.1);
			for (int i = 0; i < size; i++)
			{
				for (int j = i; j < size; j++)
				{
					H[i, j] += j;// Math.Cos(j + i / 2.0);
					H[j, i] += H[i,j].Conjugate();
				}
			}
			
			Console.WriteLine(H);

			Matrix evals, evecs;

			H.EigenValsVecs(out evals, out evecs);
			H.EigenValsVecsQR(out evals, out evecs);
			H.EigenValsVecsIQL(out evals, out evecs);
			
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine(evals.ToString("0.00"));
			Console.WriteLine(evecs.ToString("0.00"));
			Console.WriteLine();

			H.EigenValsVecsQR(out evals, out evecs);
			Console.WriteLine(evals.ToString("0.00"));
			Console.WriteLine(evecs.ToString("0.00"));
			Console.WriteLine();

			H.EigenValsVecs(out evals, out evecs);
			H.EigenValsVecs(out evals, out evecs);
			H.EigenValsVecsQR(out evals, out evecs);
			H.EigenValsVecsQR(out evals, out evecs);
			
			double time_1 = 0, time_2 = 0;
			const int count = 20;
			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

			watch.Start();
			for (int i = 0; i < count; i++)
			{
				H.EigenValsVecsQR(out evals, out evecs);
			}
			watch.Stop();
			time_1 = watch.ElapsedTicks /(double)System.Diagnostics.Stopwatch.Frequency /  count;


			watch.Reset();
			watch.Start();
			for (int i = 0; i < count; i++)
			{
				H.EigenValsVecs(out evals, out evecs);
			}
			watch.Stop();
			time_2 = watch.ElapsedTicks / (double)System.Diagnostics.Stopwatch.Frequency / count;

			Console.WriteLine("Time 1: {0}  2: {1}   ratio: {2}", time_1, time_2, time_1 / time_2);
			Console.ReadKey();

		}
	}
}
