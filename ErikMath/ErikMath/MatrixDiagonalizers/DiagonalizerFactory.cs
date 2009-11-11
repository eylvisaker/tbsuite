using System;
using System.Collections.Generic;
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

			Diagonalizers.Add(new LaPack());
			Diagonalizers.Add(new BuiltInDiagonalizer());

			initialized = true;
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

	}
}
