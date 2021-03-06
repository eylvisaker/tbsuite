using System;
using System.Collections.Generic;
using System.Text;
using DotNetMatrix;

namespace ERY.EMath
{
	[Serializable]
	public class Matrix : ICloneable
	{
		public static double tolerance = 1e-10;
		private static MatrixDiagonalizers.IMatrixDiagonalizer sDiagonalizer;

		private int mRows;
		private int mCols;

		private Complex[] mElements;

		private bool mValidDeterminant = false;
		private Complex mDeterminant;

		public static bool CanDiagonalizeNonHermitian
		{
			get { return MatrixDiagonalizers.DiagonalizerFactory.CanDiagonalizeNonHermitian; }
		}

		public Matrix()
		{		}

		public Matrix(int rows, int cols)
		{
			SetMatrixSize(rows, cols);
		}
		public Matrix(int rows, int cols, params double[] vals)
		{
			if (vals.Length != rows * cols)
				throw new ArgumentException("Not right number of values.");

			mElements = null;
			mRows = 0;
			mCols = 0;

			SetMatrixSize(rows, cols);

			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < cols; j++)
				{
					this[i, j] = vals[i * cols + j];
				}
			}

		}
		/// <summary>
		/// Creates a column vector Matrix from the input.
		/// A column vector is a Matrix with a single column.
		/// </summary>
		/// <param name="vector"></param>
		public Matrix(double[] vector)
			: this(vector, true)
		{
		}
		/// <summary>
		/// Creates a row or column vector from the input.
		/// A column vector is a Matrix with a single column.
		/// A row vector is a Matrix with a single row.
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="columnVector">True causes a column vector to be generated, false
		/// creates a row vector.</param>
		public Matrix(double[] vector, bool columnVector)
		{
			if (columnVector)
			{
				SetMatrixSize(vector.Length, 1);

				for (int i = 0; i < vector.Length; i++)
					this[i, 0] = vector[i];
			}
			else
			{
				SetMatrixSize(1, vector.Length);

				for (int i = 0; i < vector.Length; i++)
					this[0, i] = vector[i];
			}
		}
		/// <summary>
		/// Creates a real matrix by copying values from the array.
		/// </summary>
		/// <param name="realmatrix"></param>
		public Matrix(double[,] realmatrix)
		{
			SetMatrixSize(realmatrix.GetUpperBound(0) + 1, realmatrix.GetUpperBound(1) + 1);

			for (int i = 0; i < realmatrix.GetUpperBound(0) + 1; i++)
				for (int j = 0; j < realmatrix.GetUpperBound(1) + 1; j++)
				{
					this[i, j] = realmatrix[i, j];
				}
		}

		public Matrix(Matrix source)
		{
			mElements = null;
			mRows = 0;
			mCols = 0;

			CopyFrom(source);
		}

		~Matrix()
		{
			ClearMatrix();
		}

		public Complex[] GetData()
		{
			return (Complex[])mElements.Clone();
		}
		public Complex this[int row, int col]
		{
			get
			{
				if (row < 0 || row >= mRows || col < 0 || col >= mCols)
					throw new Exception("Invalid matrix element accessed.");

				return mElements[row * mCols + col];
			}
			set
			{
				if (row < 0 || row >= mRows || col < 0 || col >= mCols)
					throw new Exception("Invalid matrix element accessed.");

				mElements[row * mCols + col] = value;

				if (mValidDeterminant)
					mValidDeterminant = false;
			}

		}

		public int Rows
		{
			get { return mRows; }
		}
		public int Columns
		{
			get { return mCols; }
		}

		/// <summary>
		/// Calculate the sum of the squares of all the elements in a column.
		/// If you wish to normalize the column, this value must be square-rooted.
		/// </summary>
		/// <param name="colIndex"></param>
		/// <returns></returns>
		public double CalcColumnNorm(int colIndex)
		{
			double result = 0;

			for (int i = 0; i < Rows; i++)
			{
				result += this[i, colIndex].MagnitudeSquared;
			}

			return result;
		}
		/// <summary>
		/// Calculate the sum of the squares of all the elements in a row.
		/// If you wish to normalize the row, this value must be square-rooted.
		/// </summary>
		/// <param name="colIndex"></param>
		/// <returns></returns>
		public double CalcRowNorm(int rowIndex)
		{
			double result = 0;

			for (int i = 0; i < Columns; i++)
			{
				result += this[rowIndex, i].MagnitudeSquared;
			}

			return result;
		}
		public void ClearMatrix()
		{
			mElements = null;

			mRows = mCols = 0;
		}
		public void SetMatrixSize(int rows, int cols)
		{
			ClearMatrix();

			mElements = new Complex[rows * cols];

			mRows = rows;
			mCols = cols;
		}
		public void SetZero()
		{
			for (int i = 0; i < mRows; i++)
			{
				for (int j = 0; j < mCols; j++)
					this[i, j] = 0;
			}
		}
		public void CopyFrom(Matrix source)
		{
			ClearMatrix();

			SetMatrixSize(source.mRows, source.mCols);

			for (int i = 0; i < mRows; i++)
			{
				for (int j = 0; j < mCols; j++)
				{
					this[i, j] = source[i, j];
				}
			}
		}



		public bool IsSquare
		{
			get { return mRows == mCols; }
		}
		public bool IsDiagonal
		{
			get
			{
				// check for meaningless condition
				if (mRows != mCols)
					return false;

				// we want the tolerance to be for off-diagonals at a factor of tolerance less than the diagonals.
				// so first find the smallest value on the diagonal
				double smallest = this[0, 0].Magnitude;

				for (int i = 1; i < mRows; i++)
					if (this[i, i].Magnitude < smallest)
						smallest = this[i, i].Magnitude;

				Matrix test = Round(tolerance * smallest).Round(tolerance);


				for (int i = 0; i < mRows; i++)
				{
					for (int j = 0; j < mCols; j++)
					{
						// skip diagonal
						if (i == j) continue;

						Complex elem = test[i, j];


						// if off-diagonal element is non-zero, then this is not diagonal
						if (elem != 0)
							return false;
					}
				}

				// no non-diagonal element tested positive, so return true.
				return true;
			}
		}
		public bool IsUpperTriangular
		{
			get
			{
				if (IsSquare == false)
					return false;

				for (int i = 1; i < Rows; i++)
				{
					for (int j = 0; j < i; j++)
					{
						if (this[i, j].MagnitudeSquared > Complex.TOLERANCE)
							return false;
					}
				}

				return true;
			}
		}
		public bool IsIdentity
		{
			get
			{
				if (!IsSquare)
					return false;

				Matrix test = new Matrix(this - Identity(mRows));

				return test.IsZero;
			}
		}
		public bool IsZero
		{
			get
			{
				Matrix test = Round();

				for (int i = 0; i < mRows; i++)
				{
					for (int j = 0; j < mCols; j++)
					{
						if (test[i, j] != 0)
							return false;
					}
				}

				return true;
			}
		}

		public bool IsSymmetric
		{
			get
			{
				Matrix result = this - this.Transpose();

				return (result.IsZero);
			}
		}
		public bool IsHermitian
		{
			get
			{
				Matrix result = this - this.HermitianConjugate();

				return (result.IsZero);

			}
		}
		public bool AllElementsReal
		{
			get
			{
				for (int i = 0; i < Rows; i++)
				{
					for (int j = 0; j < Columns; j++)
					{
						Complex v = this[i, j].Round(tolerance);

						if (v.ImagPart != 0)
							return false;
					}
				}

				return true;

			}
		}
		// matrix diagonalization
		public static int technique = 0;

		/// <summary>
		/// returns a column vector with the eigenvalues
		/// </summary>
		/// <returns></returns>
		public Matrix EigenValues()
		{
			Matrix eigenvals, ev;

			EigenValsVecs(out eigenvals, out ev);

			return eigenvals;
		}
		/// <summary>
		/// returns the eigenvectors
		/// </summary>
		/// <returns></returns>
		public Matrix EigenVecs()
		{
			Matrix eigenvals, ev;

			EigenValsVecs(out eigenvals, out ev);

			return ev;
		}

		/// <summary>
		/// returns the eigenvalues and eigenvectors.
		/// </summary>
		/// <param name="eigenvals"></param>
		/// <param name="eigenvecs"></param>
		public void EigenValsVecs(out Matrix eigenvals, out Matrix eigenvecs)
		{
			if (Columns == 1 && Rows == 1)
			{
				eigenvals = this.Clone();
				eigenvecs = Matrix.Identity(1);
				return;
			}

			MatrixDiagonalizers.DiagonalizerFactory.EigenValsVecs(this, out eigenvals, out eigenvecs);

		}

		private void UseGeneralMatrix(out Matrix eigenvals, out Matrix eigenvecs)
		{
			if (AllElementsReal == false)
				throw new Exception("All matrix elements must be real to diagonalize.");

			double[][] eles = new double[Rows][];
			for (int i = 0; i < Rows; i++)
				eles[i] = new double[Columns];

			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Columns; j++)
				{
					eles[i][j] = this[i, j].RealPart;
				}
			}

			GeneralMatrix matrix = new GeneralMatrix(eles);

			EigenvalueDecomposition evs = new EigenvalueDecomposition(matrix);

			GeneralMatrix vectors = evs.GetV();

			eigenvecs = new Matrix(Rows, Columns);

			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Columns; j++)
				{
					eigenvecs[i, j] = vectors.GetElement(i, j);
				}
			}

			double[] values = evs.RealEigenvalues;
			double[] valimag = evs.ImagEigenvalues;

			eigenvals = new Matrix(Rows, 1);

			for (int i = 0; i < Rows; i++)
			{
				eigenvals[i, 0] = new Complex(values[i], valimag[i]);
			}
		}
		/*
		void jacobiRotation(Matrix eigenvals);
		void jacobiRotation(Matrix eigenvals, Matrix eigenvecs);
        
		Matrix squareRoot();   // returns a matrix A satisfying A*A = *this;
		*/

		// matrix basic routines
		// returns a matrix with each element's Complex conjugate
		public Matrix ComplexConjugate()
		{
			Matrix retval = new Matrix(mRows, mCols);

			for (int j = 0; j < mRows; j++)
			{
				for (int i = 0; i < mCols; i++)
					retval[j, i] = this[j, i].Conjugate();
			}

			return retval;
		}
		// returns the transpose of this matrix
		public Matrix Transpose()
		{
			Matrix retval = new Matrix(mCols, mRows);

			for (int j = 0; j < mCols; j++)
			{
				for (int i = 0; i < mRows; i++)
					retval[j, i] = this[i, j];
			}

			return retval;
		}
		// returns the hermitian conjugate of this matrix
		public Matrix HermitianConjugate()
		{
			Matrix retval = new Matrix(mCols, mRows);

			for (int j = 0; j < mCols; j++)
			{
				for (int i = 0; i < mRows; i++)
					retval[j, i] = this[i, j].Conjugate();
			}

			return retval;

		}

		// check to see if we can add these two matrices
		public static bool CanAdd(Matrix a, Matrix b)
		{
			if (a.mRows == b.mRows && a.mCols == b.mCols)
				return true;
			else
				return false;
		}
		// check to see if we can multiply together these two matrices.  This is the left matrix.
		public static bool CanMultiply(Matrix a, Matrix b)
		{
			if (a.mCols == b.mRows)
				return true;
			else
				return false;
		}

		public Complex Determinant()
		{
			if (mRows != mCols)
				throw new Exception("Can't take the determinant of a non-square matrix!");

			if (mRows == 1)
				return this[0, 0];

			if (!mValidDeterminant)
			{
				mDeterminant = CalcDeterminant();

				mValidDeterminant = true;
			}

			return mDeterminant;
		}

		private Complex CalcDeterminant()
		{
			if (mRows == 2)
			{
				return this[0, 0] * this[1, 1] -
					   this[1, 0] * this[0, 1];
			}
			else
			{
				Complex retval = new Complex();

				for (int i = 0; i < mCols; i++)
				{
					retval += this[0, i] * CofactorMatrix(0, i).Determinant()
								* ((i % 2 == 1) ? -1 : 1);
				}

				return retval;
			}
		}

		public Matrix Invert()
		{
			return InvertByRowOperations();
		}

		public Matrix SubMatrix(int startRow, int startCol, int rows, int cols)
		{
			Matrix retval = new Matrix(rows, cols);

			for (int i = 0; i < retval.mRows; i++)
			{
				for (int j = 0; j < retval.mCols; j++)
				{
					retval[i, j] = this[i + startRow, j + startCol];
				}
			}

			return retval;
		}
		public Matrix CofactorMatrix(int row, int col)
		{
			Matrix retval = new Matrix(mRows - 1, mCols - 1);

			for (int i = 0; i < retval.mRows; i++)
			{
				for (int j = 0; j < retval.mCols; j++)
				{
					int x = i, y = j;

					if (x >= row) x++;
					if (y >= col) y++;

					retval[i, j] = this[x, y];
				}
			}

			return retval;
		}

		// inversion by cofactor
		public Matrix InvertByCofactorMethod()
		{
			Complex det = Determinant();
			Matrix retval = new Matrix(mRows, mCols);

			if (det.RealPart == 0.0 && det.ImagPart == 0.0)
				throw new Exception("Singular matrix.");

			for (int i = 0; i < mRows; i++)
			{
				for (int j = 0; j < mCols; j++)
				{
					// transpose of cofactor matrix, so use j,i instead.
					retval[j, i] = CofactorMatrix(i, j).Determinant()
						* (((i + j) % 2 == 1) ? -1.0 : 1.0);
				}
			}

			retval /= det;

			return retval;
		}
		public Matrix InvertByRowOperations()
		{
			if (!IsSquare)
				throw new Exception("Matrix not square!");

			Matrix retval = new Matrix(Rows, Columns * 2);

			// build the inversion matrix
			for (int i = 0; i < Columns; i++)
				for (int j = 0; j < Rows; j++)
					retval[j, i] = this[j, i];

			for (int i = Columns; i < Columns * 2; i++)
				for (int j = 0; j < Rows; j++)
					retval[j, i] = ((i - Columns) == j) ? 1 : 0;

			// do the columns one by one
			for (int col = 0; col < Columns; col++)
			{
				if (retval[col, col] == new Complex(0, 0))
				{
					for (int row = col + 1; row < Rows; row++)
					{
						if (retval[row, col] != new Complex(0, 0))
						{
							retval.AddRow(col, row, 1);
						}
					}
				}

				if (retval[col, col] == new Complex(0, 0))
				{
					// apparently we are singular.
					throw new Exception("Singular matrix.");
				}

				retval.ScaleRow(col, 1 / retval[col, col]);

				for (int row = 0; row < Rows; row++)
				{
					if (row == col)
						continue;

					Complex scale = -retval[row, col];

					retval.AddRow(row, col, scale);
				}
			}

			// extract the actual inversion matrix
			return retval.SubMatrix(0, Columns, Rows, Columns);

		}

		public void ScaleRow(int row, Complex scale)
		{
			for (int i = 0; i < Columns; i++)
				this[row, i] *= scale;
		}
		public void AddRow(int rowdest, int rowsrc, Complex scale)
		{
			for (int i = 0; i < Columns; i++)
				this[rowdest, i] += this[rowsrc, i] * scale;
		}


		// operators
		public static Matrix operator -(Matrix t)
		{

			Matrix retval = new Matrix(t.Rows, t.Columns);

			for (int i = 0; i < t.Rows; i++)
			{
				for (int j = 0; j < t.Columns; j++)
					retval[i, j] = -t[i, j];
			}

			return retval;


		}

		public static Matrix operator +(Matrix a, Matrix b)
		{
			if (!CanAdd(a, b))
				throw new Exception("Can't add these matrices.");

			Matrix retval = new Matrix(a.Rows, a.Columns);

			for (int i = 0; i < a.Rows; i++)
			{
				for (int j = 0; j < a.Columns; j++)
				{
					retval[i, j] = a[i, j] + b[i, j];
				}
			}

			return retval;
		}
		public static Matrix operator -(Matrix a, Matrix b)
		{
			if (!CanAdd(a, b))
				throw new Exception("Can't add these matrices.");

			Matrix retval = new Matrix(a.Rows, a.Columns);

			for (int i = 0; i < a.Rows; i++)
			{
				for (int j = 0; j < a.Columns; j++)
				{
					retval[i, j] = a[i, j] - b[i, j];
				}
			}

			return retval;
		}
		public static Matrix operator *(Matrix a, Matrix b)
		{
			if (!CanMultiply(a, b))
				throw new Exception("Can't multiply these matrices in this order.");

			Matrix retval = new Matrix(a.mRows, b.mCols);

			//for (int i = 0; i < retval.mRows; i++)
			//{
			//    for (int j = 0; j < retval.mCols; j++)
			//    {
			//        retval[i, j] = 0;

			//        for (int k = 0; k < a.mCols; k++)
			//        {
			//            retval[i, j] += a[i, k] * b[k, j];
			//        }
			//    }

			//}

			Complex result = new Complex();

			for (int i = 0; i < retval.mRows; i++)
			{
				for (int j = 0; j < retval.mCols; j++)
				{
					int index = i * retval.mCols + j;
					
					int m = i * a.mCols;
					int n = j;

					result.x = 0;
					result.y = 0;

					for (int k = 0; k < a.mCols; k++)
					{
						Complex aval = a.mElements[m];
						Complex bval = b.mElements[n];

						double new_x = aval.x * bval.x - aval.y * bval.y;
						double new_y = aval.x * bval.y + aval.y * bval.x;

						result.x += new_x;
						result.y += new_y;

						m += 1;
						n += b.mCols;
					}

					retval.mElements[index] = result;
				}

			}
			return retval;

		}
		public static Vector3 operator *(Matrix a, Vector3 b)
		{
			Vector3 retval = new Vector3();

			retval.X = (a[0, 0] * b.X + a[0, 1] * b.Y + a[0, 2] * b.Z).RealPart;
			retval.Y = (a[1, 0] * b.X + a[1, 1] * b.Y + a[1, 2] * b.Z).RealPart;
			retval.Z = (a[2, 0] * b.X + a[2, 1] * b.Y + a[2, 2] * b.Z).RealPart;

			return retval;
		}

		public static Matrix operator *(Matrix a, Complex b)
		{
			Matrix retval = new Matrix(a);

			for (int i = 0; i < retval.Rows; i++)
			{
				for (int j = 0; j < retval.Columns; j++)
				{
					retval[i, j] *= b;
				}
			}

			return retval;
		}
		public static Matrix operator *(Complex a, Matrix b)
		{
			return b * a;
		}
		public static Matrix operator /(Matrix a, Complex b)
		{
			Matrix retval = new Matrix(a);

			for (int i = 0; i < retval.Rows; i++)
			{
				for (int j = 0; j < retval.Columns; j++)
				{
					retval[i, j] /= b;
				}
			}

			return retval;
		}


		//Complex  operator () (int row, int col) { return element(row, col); }

		public override string ToString()
		{
			return ToString("");
		}
		public string ToString(string formatString)
		{
			string retval = "[ ";

			for (int i = 0; i < mRows; i++)
			{
				if (i > 0)
					retval += "\r\n  ";

				retval += "[ ";


				for (int j = 0; j < mCols; j++)
				{
					retval += this[i, j].ToString(formatString);
					retval += ", ";
				}

				retval += "] ";


			}

			retval += "]";

			return retval;

		}

		/// <summary>
		/// Returns a size by size matrix with all elements zero.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Matrix Zero(int size) { return new Matrix(size, size); }
		/// <summary>
		/// Returns a size by size identity matrix
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Matrix Identity(int size)
		{
			Matrix retval = new Matrix(size, size);

			for (int i = 0; i < retval.mRows; i++)
			{
				for (int j = 0; j < retval.mCols; j++)
				{
					if (i == j)
						retval[i, j] = 1;
					else
						retval[i, j] = 0;
				}
			}

			return retval;
		}

		/// <summary>
		/// Returns a matrix with random values, from 0 to 1.
		/// </summary>
		/// <param name="rows">How many rows the matrix should contain.</param>
		/// <param name="cols">How many columns the matrix should contain.</param>
		/// <param name="real">True if all elements should be real, imaginary parts are zero.</param>
		/// <param name="hermitian">True if the matrix generated should be Hermitian.  If
		/// real and hermitian are true, then the generated matrix will be symmetric.</param>
		/// <returns></returns>
		public static Matrix RandomMatrix(int rows, int cols, bool real, bool hermitian, Random rnd)
		{
			Matrix retval = new Matrix(rows, cols);

			if (rnd == null)
				rnd = new Random();

			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < cols; j++)
				{
					if (real)
						retval[i, j] = rnd.NextDouble();
					else
						retval[i, j] = new Complex(rnd.NextDouble(), rnd.NextDouble());
				}
			}

			if (hermitian)
			{
				// copy upper triangular to lower triangular, and conjugate to make Hermitian
				// make sure diagonals are real.
				for (int i = 0; i < rows; i++)
				{
					for (int j = 0; j <= i; j++)
					{
						if (i == j)
						{
							retval[i, j] = new Complex(retval[i, j].RealPart, 0);
							continue;
						}

						retval[i, j] = retval[j, i].Conjugate();
					}
				}

				System.Diagnostics.Debug.Assert(retval.IsHermitian);
			}

			return retval;
		}
		/*
		static Matrix random(int rows, int cols);	// returns a matrix full of random integers, from zero to max-1
		// returns a matrix full of random integers, from zero to max-1
		static Matrix random(int rows, int cols, int max)
		{
	Matrix retval(rows, cols);
	
	for (int i = 0; i < retval.mRows; i++)
	{
		for (int j = 0; j < retval.mCols; j++)
		{
			retval.this[i, j] = Complex(int(rand() * double(max) / double(RAND_MAX)));
		}
	}

	return retval;
}
		// returns a random symmetric real matrix
		static Matrix randomSymmetric(int size){
	Matrix retval(size, size);
	
	for (int i = 0; i < retval.mRows; i++)
	{
		for (int j = i; j < retval.mCols; j++)
		{
			retval[j,i] = retval.this[i, j] = Complex(rand());
		}
	}

	return retval;
}
		*/
		// cuts everything smaller than TOLERANCE.  equivalent to *this = this.round();
		public void SetRound()
		{

			for (int i = 0; i < mRows; i++)
			{
				for (int j = 0; j < mCols; j++)
				{
					if (Math.Abs(this[i, j].RealPart) < tolerance)
						this[i, j] = new Complex(0, this[i, j].ImagPart);

					if (Math.Abs(this[i, j].ImagPart) < tolerance)
						this[i, j] = new Complex(this[i, j].RealPart, 0);
				}
			}


		}

		/// <summary>
		/// returns a matrix with everything smaller than TOLERANCE 
		/// set to zero.
		/// </summary>
		/// <returns></returns>
		public Matrix Round()
		{
			return Round(1);
		}
		/// <summary>
		/// Rounds the matrix trimming off any values of 
		/// size tolerance * relativeTo.
		/// </summary>
		/// <param name="relativeTo"></param>
		/// <returns></returns>
		public Matrix Round(double relativeTo)
		{
			Matrix retval = new Matrix(this);

			for (int i = 0; i < mRows; i++)
			{
				for (int j = 0; j < mCols; j++)
				{
					Complex value = retval[i, j];

					retval[i, j] = retval[i, j].Round(relativeTo * tolerance);

				}
			}

			return retval;
		}
		// forces this matrix to be unitary, by doing Graham-Schmidt orthogonalization of rows, and normalizing them.  Use with caution.
		public double ForceUnitary()
		{
			if (!IsSquare)
				return 0;

			Matrix save = new Matrix(this);

			// Do Graham-Schmidt orthogonalization
			for (int i = 0; i < mRows; i++)
			{
				// subtract out projections
				for (int j = 0; j < i; j++)
				{
					Complex proj = new Complex();

					for (int c = 0; c < mCols; c++)
						proj += this[i, c] * this[j, c].Conjugate();

					for (int c = 0; c < mCols; c++)
						this[i, c] -= proj * this[j, c];
				}

				// normalize the row
				double norm2 = 0;
				for (int c = 0; c < mCols; c++)
					norm2 += this[i, c].MagnitudeSquared;

				for (int c = 0; c < mCols; c++)
					this[i, c] /= Math.Sqrt(norm2);

			}

			return 0;

		}


		
		/// <summary>
		/// Returns a matrix containing only the real parts of the values in this matrix.
		/// </summary>
		/// <returns></returns>
		public Matrix GetRealPart()
		{
			Matrix retval = new Matrix(mRows, mCols);

			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Columns; j++)
				{
					retval[i, j] = this[i, j].RealPart;
				}
			}

			return retval;
		}
		/// <summary>
		/// Returns a matrix containing only the imaginary parts of the values in this matrix.
		/// All values in the returned matrix are real.
		/// </summary>
		/// <returns></returns>
		public Matrix GetImagPart()
		{
			Matrix retval = new Matrix(mRows, mCols);

			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Columns; j++)
				{
					retval[i, j] = this[i, j].ImagPart;
				}
			}

			return retval;
		}
		/// <summary>
		/// Calculates and returns the trace of the matrix.  Throw an exception if the matrix is
		/// not square.
		/// </summary>
		/// <returns></returns>
		public Complex Trace()
		{
			if (!IsSquare)
				throw new InvalidOperationException("Can only calculate trace of square matrix.");

			Complex retval = 0;

			for (int i = 0; i < Rows; i++)
				retval += this[i, i];

			return retval;
		}

		#region ICloneable Members

		public Matrix Clone()
		{
			Matrix retval = new Matrix(Rows, Columns);

			retval.mElements = new Complex[Rows * Columns];
			mElements.CopyTo(retval.mElements, 0);

			return retval;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="matrix"></param>
		/// <param name="destRow"></param>
		/// <param name="destCol"></param>
		/// <param name="row"></param>
		/// <param name="col"></param>
		/// <param name="rows"></param>
		/// <param name="cols"></param>
		public void CopySubmatrixFrom(Matrix matrix, int destRow, int destCol,
										int row, int col, int rows, int cols)
		{
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < cols; j++)
				{
					this[destRow + i, destCol + j] =
						matrix[row + i, col + j];
				}
			}
		}

		public void SetRow(int row, Vector3 v)
		{
			if (Columns != 3)
				throw new ArgumentException("Can only set row with a vector in a matrix with 3 columns.");

			this[row, 0] = v.X;
			this[row, 1] = v.Y;
			this[row, 2] = v.Z;
		}
		public void SetColumn(int col, Vector3 v)
		{
			if (Columns != 3)
				throw new ArgumentException("Can only set column with a vector in a matrix with 3 rows.");

			this[0, col] = v.X;
			this[1, col] = v.Y;
			this[2, col] = v.Z;
		}

		public void SetColumns(params Vector3[] v)
		{
			if (v.Length != Columns)
				throw new ArgumentException("Must supply the same amount of vectors as columns.");

			for (int i = 0; i < v.Length; i++)
				SetColumn(i, v[i]);
		}

		public void SetRows(params Vector3[] v)
		{
			if (v.Length != Rows)
				throw new ArgumentException("Must supply the same amount of vectors as rows.");

			for (int i = 0; i < v.Length; i++)
				SetRow(i, v[i]);
		}

		public Vector3 GetRowAsVector3(int row)
		{
			if (Columns != 3)
				throw new InvalidOperationException("Cannot get a Vector3.");

			return new Vector3(this[row, 0].RealPart, this[row, 1].RealPart, this[row, 2].RealPart);
		}
		public Vector3 GetColumnAsVector3(int col)
		{
			if (Rows != 3)
				throw new InvalidOperationException("Cannot get a Vector3.");

			return new Vector3(this[0, col].RealPart, this[1, col].RealPart, this[2, col].RealPart);
		}

		public Matrix ToTriDiagonal(out Matrix transform)
		{
			if (IsSquare == false)
				throw new InvalidOperationException("Only square matrices can be reduced to tridiagonal form.");

			Matrix retval = this.Clone();
			transform = Matrix.Identity(this.Rows);

			for (int block = 1; block < this.Rows - 1; block++)
			{
				Matrix x = retval.SubMatrix(block, block - 1, this.Rows - block, 1);

				const int sign = -1;
				double alpha = x[0, 0].Argument;
				Complex fact = Complex.Exp(new Complex(0, alpha));
				Matrix u = x.Clone();
				double xnorm = Math.Sqrt(x.CalcColumnNorm(0));
				u[0, 0] += sign * xnorm * fact;
				if (u.IsZero) continue;

				double H = 0.5 * (u.HermitianConjugate() * u)[0, 0].RealPart;
				double Halt = xnorm * xnorm + sign * xnorm * (x[0, 0] / fact).RealPart;

				Matrix sub = Matrix.Identity(this.Rows - block) - u * u.HermitianConjugate() / H;
				Matrix thisTrans = Matrix.Identity(this.Rows);
				thisTrans.SetSubMatrix(block, block, sub);

				transform = transform * thisTrans;

				retval = transform.HermitianConjugate() * this * transform;
			}

			return retval;
		}

		private void SetSubMatrix(int startRow, int startCol, Matrix newSub)
		{
			for (int i = 0; i < newSub.Rows; i++)
			{
				for (int j = 0; j < newSub.Columns; j++)
				{
					this[startRow + i, startCol + j] = newSub[i, j];
				}
			}
		}

	}
}