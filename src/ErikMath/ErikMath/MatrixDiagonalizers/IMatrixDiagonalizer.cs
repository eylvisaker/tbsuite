using System;
using System.Collections.Generic;
using System.Text;

namespace ERY.EMath.MatrixDiagonalizers
{
	interface IMatrixDiagonalizer
	{
		string Name { get; }
		void EigenValsVecs(Matrix matrix, out Matrix eigenvals, out Matrix eigenvecs);
		bool CanDiagonalizeNonHermitian { get; }
	}
}
