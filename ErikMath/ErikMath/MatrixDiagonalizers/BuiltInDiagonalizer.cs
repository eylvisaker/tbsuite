using System;
using System.Collections.Generic;
using System.Text;

namespace ERY.EMath.MatrixDiagonalizers
{
	class BuiltInDiagonalizer : IMatrixDiagonalizer 
	{
		internal static int lastDiagonalIter = 0;

		private bool OffDiagonalInColumn(Matrix input, int col)
		{
			bool foundNonzero = false;
			for (int i = col + 1; i < input.Rows; i++)
			{
				if (input[i, col].MagnitudeSquared > 1e-28)
				{
					foundNonzero = true;
					break;
				}
			}
			return foundNonzero;
		}

		private double CalcShift(Matrix input, ref int shiftStart, ref int shiftIter)
		{
			bool foundNonzero = OffDiagonalInColumn(input, shiftStart);

			shiftIter++;

			if (foundNonzero == false)
				shiftIter += 5;

			if (shiftIter > 10)
			{
				shiftIter = 0;

				int newCol = shiftStart;

				for (int i = 1; i <= input.Rows; i++)
				{
					newCol--;

					if (newCol < 0)
						newCol = input.Rows - 2;

					if (OffDiagonalInColumn(input, newCol))
						break;
				}
				shiftStart = newCol;
			}


			int a = shiftStart;
			int b = a + 1;

			Complex sum = input[a, a] + input[b, b];
			Complex diff = input[a, a] - input[b, b];
			Complex offdiag = input[b, a] * input[a, b];

			Complex ev1 = 0.5 * (sum - Complex.Sqrt(diff * diff + 4 * offdiag));
			Complex ev2 = 0.5 * (sum + Complex.Sqrt(diff * diff + 4 * offdiag));

			Complex diff_1 = ev1 - input[b, b];
			Complex diff_2 = ev2 - input[b, b];

			if (diff_1.MagnitudeSquared < diff_2.MagnitudeSquared)
				return ev1.RealPart;
			else
				return ev2.RealPart;

		}

		private void ShiftDiagonal(Matrix input, double shift)
		{
			for (int i = 0; i < input.Rows; i++)
				input[i, i] -= shift;
		}

		private double CalculateOffDiagonalNorm(Matrix input)
		{
			double matrixNorm = 0;

			// check to see if we've diagonalized the matrix
			for (int i = 0; i < input.Rows; i++)
			{
				for (int j = 0; j < input.Columns; j++)
				{
					if (i == j)
						continue;

					matrixNorm += input[i, j].MagnitudeSquared;
				}
			}
			return matrixNorm;
		}

		private void PerformGivensRotation(Matrix Q, Matrix R, int i, int j, ref Complex Gii, ref Complex Gij, ref Complex Gji, ref Complex Gjj)
		{
			// do matrix multiplication by hand for speed
			for (int k = 0; k < R.Columns; k++)
			{
				Complex Rik = R[i, k], Rjk = R[j, k];

				R[i, k] = Gii * Rik + Gij * Rjk;
				R[j, k] = Gji * Rik + Gjj * Rjk;
			}

			Complex GTii = Gii.Conjugate(),
				GTij = Gji.Conjugate(),
				GTji = Gij.Conjugate(),
				GTjj = Gjj.Conjugate();

			for (int k = 0; k < Q.Rows; k++)
			{
				Complex Qki = Q[k, i], Qkj = Q[k, j];

				Q[k, i] = Qki * GTii + Qkj * GTji;
				Q[k, j] = Qki * GTij + Qkj * GTjj;
			}
		}



		private void OrderEigenvectors(ref Matrix eigenvals, ref Matrix eigenvecs)
		{
			List<KeyValuePair<int, double>> vals = new List<KeyValuePair<int, double>>();

			for (int i = 0; i < eigenvals.Rows; i++)
			{
				vals.Add(new KeyValuePair<int, double>(i, eigenvals[i, i].RealPart));
			}
			vals.Sort((x, y) => x.Value.CompareTo(y.Value));

			Matrix newTransform = new Matrix(eigenvecs.Rows, eigenvecs.Columns);

			for (int i = 0; i < eigenvals.Rows; i++)
			{
				newTransform[vals[i].Key, i] = 1;
			}

			eigenvecs = eigenvecs * newTransform;
		}

		private bool GivensRotation(Matrix matrix, int rowa, int rowb, out Complex Gaa, out Complex Gab, out Complex Gba, out Complex Gbb)
		{
			if (matrix.IsSquare != true)
				throw new InvalidOperationException();

			Complex a = matrix[rowa, rowa];
			Complex b = matrix[rowb, rowa];

			if (a.MagnitudeSquared == 0 && b.MagnitudeSquared == 0)
			{
				Gaa = 1;
				Gab = 0;
				Gba = 0;
				Gbb = 1;
				return false;
			}
			double r = Math.Sqrt(a.MagnitudeSquared + b.MagnitudeSquared);

			Complex c = a.Conjugate() / r;
			Complex s = b.Conjugate() / r;

			Gaa = c;
			Gab = s;
			Gba = -s.Conjugate();
			Gbb = c.Conjugate();

			return true;
		}

		public void EigenValsVecs(Matrix matrix, out Matrix eigenvals, out Matrix eigenvecs)
		{
			if (matrix.IsHermitian == false)
				throw new InvalidOperationException("Can only diagonalize Hermitian matrices.");

			Matrix transform;
			Matrix tri = matrix.ToTriDiagonal(out transform);
			//Console.WriteLine();
			//Console.WriteLine(tri.ToString("0.00"));

			Matrix Q = Matrix.Identity(matrix.Rows);
			Matrix input = tri.Clone();

			double shift = 0;
			int shiftStart = matrix.Rows - 2;
			int shiftIter = 0;
			int iter;
			Matrix R;
			double shiftValTolerance = 1;
			bool doshift = false;
			double matrixNorm = double.MaxValue, smallestMatrixNorm = double.MaxValue;
			double matrixNormTolerance = 1e-21 *matrix. Rows * matrix.Rows;
			int maxIter = matrix.Rows * matrix.Columns * 5000;
			int rotationStart = 0;

			// construct an upper triangular matrix by doing a generalized Givens rotation
			// on the tridiagonal form of this matrix
			for (iter = 0; iter < maxIter; iter++)
			{
				if (doshift)
				{
					shift = CalcShift(input, ref shiftStart, ref shiftIter);

					ShiftDiagonal(input, shift);
				}

				R = input;
				Q = Matrix.Identity(input.Rows);

				for (int i = rotationStart; i < input.Rows - 1; i++)
				{
					Complex Gii, Gij, Gji, Gjj;
					int j = i + 1;
					bool nontrivial = GivensRotation(R, i, j, out Gii, out Gij, out Gji, out Gjj);

					//Matrix sampleInput = R * Q;

					//Console.WriteLine("Q:");
					//Console.WriteLine(Q.ToString("0.0"));
					//Console.WriteLine("R:");
					//Console.WriteLine(R.ToString("0.0"));

					//Console.WriteLine("SampleInput {0}:", i);
					//Console.WriteLine(sampleInput.ToString("0.00"));


					if (nontrivial == false)
						continue;

					PerformGivensRotation(Q, R, i, j, ref Gii, ref Gij, ref Gji, ref Gjj);


					//System.Diagnostics.Debug.Assert((Q * Q.HermitianConjugate()).IsIdentity);
					//System.Diagnostics.Debug.Assert((Q * R - input).IsZero);
				}

				//Console.WriteLine("R:");
				//Console.WriteLine(R.ToString("0.00"));
				//Console.WriteLine("Q:");
				//Console.WriteLine(Q.ToString("0.00"));
				//Console.WriteLine();

				input = R * Q;
				transform = transform * Q;

				if (doshift)
				{
					ShiftDiagonal(input, -shift);
				}


				//Console.WriteLine("Transform:");
				//Console.WriteLine(transform.ToString("0.000"));
				//Console.WriteLine("Input:");
				//Console.WriteLine(input.ToString("0.00"));
				//Console.WriteLine();
				System.Diagnostics.Debug.Assert((transform * transform.HermitianConjugate()).IsIdentity);

				//smallestMatrixNorm = Math.Min(smallestMatrixNorm, matrixNorm);
				matrixNorm = CalculateOffDiagonalNorm(input);
				if (matrixNorm < matrixNormTolerance)
					break;

				//if (doshift && matrixNorm > shiftValTolerance)
				//{
				//    shiftValTolerance /= 1.2;
				//}

				//doshift = matrixNorm < shiftValTolerance;

				//if (doshift && matrixNorm > smallestMatrixNorm * 10)
				//{
				//    doshift = false;
				//    shiftValTolerance /= 1.2;
				//    smallestMatrixNorm = matrixNorm;
				//}

				doshift = true;// iter % 3 > 0;
			}

			lastDiagonalIter = iter;
			//Console.WriteLine("Niter: {0}", iter);

			if (matrixNorm > matrixNormTolerance * 10)
			{
				Console.WriteLine("**************************************************");
				Console.WriteLine("Failed to diagonalize matrix.");
				Console.WriteLine("Norm: {0}", matrixNorm);
				Console.WriteLine(this.ToString());
				Console.WriteLine("**************************************************");

				throw new Exception("Failed to diagonalize matrix.");
			}


			eigenvals = transform.HermitianConjugate() * matrix * transform;
			eigenvecs = transform;
			OrderEigenvectors(ref eigenvals, ref eigenvecs);


			Matrix temp = eigenvecs.HermitianConjugate() * matrix * eigenvecs;
			//Console.WriteLine(temp.ToString("0.00"));

			eigenvals = new Matrix(matrix.Rows, 1);
			for (int i = 0; i < matrix.Rows; i++)
				eigenvals[i, 0] = temp[i, i].RealPart;  // take real part here because matrix was hermitian to begin with!
		}

	}
}
