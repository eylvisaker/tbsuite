using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ERY.EMath.MatrixDiagonalizers
{
	class LaPack : IMatrixDiagonalizer 
	{
		//      SUBROUTINE ZHEEV( JOBZ, UPLO, N, A, LDA, W, WORK, LWORK, RWORK,
		//$                  INFO )

		const string lapack = "clapack.dll";

		[DllImport(lapack, EntryPoint = "_zheev_@40")]
		unsafe static extern void _zheev(ref char jobz, ref char uplo, ref int N, Complex* A,
								   ref int lda, double* w, Complex* work,
								   ref int lwork, double* rwork, out int info);

		public LaPack()
		{
		}
		public unsafe void EigenValsVecs(Matrix m, out Matrix eigenvals, out Matrix eigenvecs)
		{
			char jobz = 'V';
			char uplo = 'U';
			int N = m.Rows;
			int LDA = N;
			Complex[] A_ar = m.GetData();
			double* W = stackalloc double[N];
			int lwork = 2 * N;
			Complex* Work = stackalloc Complex[lwork];
			double* rwork = stackalloc double[3 * N];
			int info;


			fixed (Complex* A = A_ar)
			{
				_zheev(ref jobz, ref uplo, ref N, A, ref LDA, W, Work, ref lwork, rwork, out info);
			}

			eigenvals = new Matrix(m.Rows, 1);
			eigenvecs = new Matrix(m.Rows, m.Columns);

			for (int i = 0; i < N; i++)
			{
				eigenvals[i, 0] = W[i];
			}

			for (int i = 0; i < N; i++)
			{
				for (int j = 0; j < N; j++)
				{
					eigenvecs[i, j] = A_ar[i + j * N];
				}
			}
		}
	}
}
