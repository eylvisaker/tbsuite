using System;
using System.Collections.Generic;
using System.Text;

namespace ERY.EMath.MatrixDiagonalizers
{
	interface IMatrixDiagonalizer
	{
		void EigenValsVecs(Matrix matrix, out Matrix eigenvals, out Matrix eigenvecs);
	}
}
