using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ERY.EMath.MatrixDiagonalizers
{
	static class DiagonalizerFactory
	{
		static bool initialized;

		static List<IMatrixDiagonalizer> Diagonalizers = new List<IMatrixDiagonalizer>();

		static void Initialize()
		{
			if (initialized)
				return;

			AddDiagonalizer(new LaPack());
			AddDiagonalizer(new BuiltInDiagonalizer());

			initialized = true;
		}


		private static void AddDiagonalizer(IMatrixDiagonalizer diag)
		{
			Matrix x = Matrix.Identity(3);
			Matrix vals, vecs;

			try
			{
				diag.EigenValsVecs(x, out vals, out vecs);
				Diagonalizers.Add(diag);
			}
			catch (Exception e)
			{
				int j = 4;
				Console.WriteLine("Caught exception {0} while trying to initialize {1}",
					e.GetType().Name, diag.Name);
				Console.WriteLine(e.Message);
				Console.WriteLine();
			}
		}

		internal static void EigenValsVecs(Matrix matrix, out Matrix eigenvals, out Matrix eigenvecs)
		{
			Initialize();

			for (int i = 0; i < Diagonalizers.Count; i++)
			{
				//try
				//{
					Diagonalizers[i].EigenValsVecs(matrix, out eigenvals, out eigenvecs);
					return;
				//}
				//catch
				//{ }
			}

			throw new InvalidOperationException("Cannot diagonalize complex matrix which is not Hermitian.");
		}

		public static string PrimaryDiagonalizer
		{
			get
			{
				if (initialized == false)
					Initialize();

				return Diagonalizers[0].Name;
			}
		}


		public static bool CanDiagonalizeNonHermitian
		{
			get
			{
				Initialize();

				return Diagonalizers[0].CanDiagonalizeNonHermitian;
			}
		}
	}
}
