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
		unsafe static extern void zheev(ref char jobz, ref char uplo, ref int N, ref Complex A,
								   ref int lda, ref double w, ref Complex work,
								   ref int lwork, ref double rwork, out int info);

		public string Name { get { return "Lapack"; } }

		public LaPack()
		{
		}
		public void EigenValsVecs(Matrix m, out Matrix eigenvals, out Matrix eigenvecs)
		{
			if (m.IsHermitian)
			{
				CallZheev(m, out eigenvals, out eigenvecs);
				return;
			}

			throw new InvalidOperationException("Matrix can't be diagonalized.");
		}

		unsafe void CallZheev(Matrix m, out Matrix eigenvals, out Matrix eigenvecs)
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
				zheev(ref jobz, ref uplo, ref N, ref A[0], ref LDA, ref W[0], ref Work[0], ref lwork, ref rwork[0], out info);
			}

			if (info != 0)
				throw new Exception("Error calling Lapack.  Info: " + info.ToString());

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
					eigenvecs[i, j] = A_ar[i + j * N].Conjugate();
				}
			}
		}
	}
}
