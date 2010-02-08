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

		[DllImport(lapack, EntryPoint = "_zgeev_@56")]
		unsafe static extern void zgeev(ref char jobvl, ref char jobvr, ref int N, ref Complex A,
			ref int lda, ref Complex W, ref Complex vl, ref int ldvl, ref Complex vr, ref int ldvr,
			ref Complex work, ref int lwork, ref double rwork, out int info);

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
			else
			{
				CallZgeev(m, out eigenvals, out eigenvecs);
				return;
			}

			throw new InvalidOperationException("Matrix can't be diagonalized.");
		}


		public bool CanDiagonalizeNonHermitian
		{
			get { return true; }
		}
		private unsafe void CallZgeev(Matrix m, out Matrix eigenvals, out Matrix eigenvecs)
		{
			char jobvl = 'N';
			char jobvr = 'V';

			int N = m.Rows;
			Complex[] A_array = m.GetData();
			int lda = N;
			Complex* W = stackalloc Complex[N];
			Complex* VL = stackalloc Complex[N];
			int ldvl = 1;
			Complex* VR = stackalloc Complex[N*N];
			int ldvr = N;
			int lwork = 4 * N;
			Complex* work = stackalloc Complex[lwork];
			double* rwork = stackalloc double[2*N];
			int info;

			fixed (Complex* A = A_array)
			{
				zgeev(ref jobvl, ref jobvr, ref N, ref A[0], ref lda, ref W[0],
					  ref VL[0], ref ldvl, ref VR[0], ref ldvr, ref work[0], ref lwork,
					  ref rwork[0], out info);
			}

			eigenvals = new Matrix(N, 1);
			for (int i = 0; i < N; i++)
			{
				eigenvals[i, 0] = W[i];
			}
			eigenvecs = new Matrix(N, N);

			for (int i = 0; i < N; i++)
			{
				for (int j = 0; j < N; j++)
				{
					eigenvecs[i, j] = VR[i + j * N].Conjugate();
				}
			}
		}

		unsafe void CallZheev(Matrix m, out Matrix eigenvals, out Matrix eigenvecs)
		{
			char jobz = 'V';
			char uplo = 'U';
			int N = m.Rows;
			int LDA = N;
			Complex[] A_array = m.GetData();
			double* W = stackalloc double[N];
			int lwork = 4 * N;
			Complex* Work = stackalloc Complex[lwork];
			double* rwork = stackalloc double[4 * N];
			int info;


			fixed (Complex* A = A_array)
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
					eigenvecs[i, j] = A_array[i + j * N].Conjugate();
				}
			}
		}
	}
}
