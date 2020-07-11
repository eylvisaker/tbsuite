using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace TestMath
{
	class Program
	{
		public static void Main(string[] args)
		{
			new Program().Run(args);
		}

		Random rnd = new Random();

		void Run(string[] args)
		{
			if (args[0] == "--diagonalizer") 
			{
				Console.WriteLine(ERY.EMath.MatrixDiagonalizers.DiagonalizerFactory.PrimaryDiagonalizer);
				return;
			}

			bool done = false;
			Console.WriteLine("Math tester");
			Console.WriteLine();
			Console.WriteLine("Using {0} diagonalizer.", ERY.EMath.MatrixDiagonalizers.DiagonalizerFactory.PrimaryDiagonalizer);
			Console.WriteLine();

			while (!done)
			{
				Console.WriteLine("Main menu");
				Console.WriteLine("=========");
				Console.WriteLine();

				Console.WriteLine("1. Test eigenvalues");
				Console.WriteLine("q. Quit");

				bool valid = false;
				while (!valid)
				{
					var input = Console.ReadKey(true);
					valid = true;

					switch (input.KeyChar)
					{
						case '1':
							EigenValueMenu();
							break;

						case 'q':
							done = true;
							break;

						default:
							valid = false;
							break;
					}
				}
			}

		}

		private  void EigenValueMenu()
		{
			bool done = false;

			while (!done)
			{
				Console.WriteLine();
				Console.WriteLine("Test Eigenvalues");
				Console.WriteLine();
				Console.WriteLine("1. Test cases");
				Console.WriteLine("2. Random matrices");
				Console.WriteLine("x. Exit");

				bool valid = false;

				while (!valid)
				{
					var input = Console.ReadKey(true);
					valid = true;

					switch (input.KeyChar)
					{
						case '1':
							RunMatrixTestCases();
							break;
						case '2':
							RunRandomMatrices();
							break;

						case 'x':
							done = true;
							break;

						default:
							valid = false;
							break;
					}
				}
			}
		}

		private  void RunRandomMatrices()
		{
			const int maxSize = 200;
			const int maxCount = 5;

			int failcount = 0;

			using (StreamWriter w = new StreamWriter("random.txt"))
			{
				for (int size = 2; size < maxSize; size++)
				{
					Console.WriteLine("Testing size {0}...", size);

					for (int degeneracy = 1; degeneracy < size; degeneracy++)
					{
						Console.Write("Testing degeneracy {0}...", degeneracy);

						int totalIter = 0;

						for (int count = 0; count < maxCount; count++)
						{
							for (int near = 0; near <= 1; near++)
							{
								Matrix H, ev, vec;
								CreateRandomMatrix(out H, out ev, out vec, size, degeneracy, near);

								MatrixTestCase test = new MatrixTestCase();
								test.Eigenvalues = ev;
								test.Eigenvectors = vec;
								test.Matrix = H;

								try
								{
									RunMatrixTestCase(test, false);
									totalIter += ERY.EMath.MatrixDiagonalizers.BuiltInDiagonalizer.lastDiagonalIter;
								}
								catch
								{
									SaveFailedTestCase(w, test);
									failcount++;
								}
							}
						}

						double avg = totalIter / (double)maxCount;

						Console.WriteLine(" average {0} iterations.", avg);
					}
				}
			}

			Console.WriteLine();
			Console.WriteLine("Failed: {0}", failcount);
		}

		private void SaveFailedTestCase(StreamWriter w, MatrixTestCase test)
		{
			w.WriteLine("Matrix:");
			w.WriteLine("{0} {1}", test.Matrix.Rows, test.Matrix.Columns);

			WriteTestCaseMatrix(w, test.Matrix);

			w.WriteLine("Eigenvalues:");
			for (int i = 0; i < test.Eigenvalues.Rows; i++)
			{
				w.WriteLine("    {0}", test.Eigenvalues[i, 0].RealPart);
			}

			w.WriteLine("Eigenvectors:");
			WriteTestCaseMatrix(w, test.Eigenvectors);
		}

		private static void WriteTestCaseMatrix(StreamWriter w, Matrix m)
		{
			for (int i = 0; i < m.Rows; i++)
			{
				w.Write("    ");
				for (int j = 0; j < m.Columns; j++)
				{
					if (j > 0) w.Write(", ");
					Complex val = m[i, j];

					w.Write(val.ToString());
				}
				w.WriteLine();
			}
		}

		double[] tempEigenValues;
		private void CreateRandomMatrix(out Matrix H, out Matrix ev, out Matrix vec, int size, int degeneracy, int nearDegeneracy)
		{
			if (tempEigenValues == null || tempEigenValues.Length != size)
				tempEigenValues = new double[size];
			if (degeneracy < 1)
				degeneracy = 1;

			int index = 0;
			while (index < size)
			{
				double val = rnd.NextDouble() * 5 - 2;

				if (nearDegeneracy != 0 && index > 0 && rnd.NextDouble() < 0.5)
				{
					val = tempEigenValues[index - 1];
					val += (rnd.NextDouble() + 0.5) * 1e-6;
				}

				for (int i = 0; i < degeneracy; i++)
				{
					tempEigenValues[index] = val;

					index++;
					if (index == size)
						break;
				}
			}

			// randomize the eigenvalue order
			for (int i = 0; i < size * 2; i++)
			{
				int a = rnd.Next(size);
				int b = rnd.Next(size);

				double tmp = tempEigenValues[a];
				tempEigenValues[a] = tempEigenValues[b];
				tempEigenValues[b] = tmp;
			}
			
			// generate random eigenvectors;
			vec = new Matrix(size, size);

			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					vec[i, j] = new Complex(rnd.NextDouble(), rnd.NextDouble());
				}
			}

			// make first element of first vector real.
			vec[0, 0] = vec[0, 0].Magnitude;

			// now orthonormalize them
			GramSchmidtOrthogonalization(vec);
			
			// make vectors column vectors
			vec = vec.HermitianConjugate();

			AssertEigenvectorsOrthoNormal(vec);

			// make diagonal matrix with eigenvalues
			ev = new Matrix(size, size);
			for (int i = 0; i < size; i++)
				ev[i, i] = tempEigenValues[i];


			H = vec * ev * vec.HermitianConjugate();
			ev = new Matrix(size, 1, tempEigenValues);
		}

		private static void NormalizeRows(Matrix vec)
		{
			for (int i = 0; i < vec.Rows; i++)
				NormalizeRow(vec, i);
		}

		private void GramSchmidtOrthogonalization(Matrix vec)
		{
			int size = vec.Rows;

			// modify i-th vector
			for (int i = 0; i < size; i++)
			{
				// orthogonalize against j-th vector.
				for (int j = 0; j < i; j++)
				{
					Complex proj = RowProjection(vec, j, i);

					for (int k = 0; k < size; k++)
					{
						Complex val = proj * vec[j, k];

						vec[i, k] -= val;
					}

					Debug.Assert(RowProjection(vec, j, i).Magnitude < 1e-11);
				}

				NormalizeRow(vec, i);
			}

		}

		private static void NormalizeRow(Matrix vec, int row)
		{
			double norm = 0;

			for (int i = 0; i < vec.Rows; i++)
			{
				norm += vec[row, i].MagnitudeSquared;
			}
			norm = Math.Sqrt(norm);

			for (int i = 0; i < vec.Rows; i++)
			{
				vec[row, i] /= norm;
			}
		}

		private Complex RowProjection(Matrix vec, int a, int b)
		{
			Complex retval = new Complex();

			for (int i = 0; i < vec.Rows; i++)
			{
				retval += vec[a, i].Conjugate() * vec[b, i];
			}

			return retval;
		}

		private void RunMatrixTestCases()
		{
			foreach (MatrixTestCase m in GetMatrixTestCases())
			{
				RunMatrixTestCase(m, true);

			}
		}

		private void RunMatrixTestCase(MatrixTestCase m, bool verbose)
		{
			if (verbose) 
				Console.WriteLine("Matrix:\n{0}", m.Matrix.ToString("0.000"));

			Matrix eigenvals, eigenvecs;
			m.Matrix.EigenValsVecs(out eigenvals, out eigenvecs);

			CheckEigenvectorDifference(eigenvals, eigenvecs, m.Eigenvalues, m.Eigenvectors);
		}

		private  void CheckEigenvectorDifference(Matrix vals_test, Matrix vecs_test, Matrix vals_ref, Matrix vecs_ref)
		{
			Matrix dot = vecs_ref.HermitianConjugate() * vecs_test;
			Matrix evDiff = new Matrix(vecs_test.Rows, 1);
			bool badVector = false;
			bool badValue = false;

			AssertEigenvectorsOrthoNormal(vecs_test);
			AssertEigenvectorsOrthoNormal(vecs_ref);

			for (int i = 0; i < dot.Rows; i++)
			{
				bool found = false;

				for (int j = 0; j < dot.Columns; j++)
				{
					double mag = dot[i, j].Magnitude;
					double mag_one = Math.Abs(dot[i, j].Magnitude - 1);

					if (mag > 1e-4 && mag_one < 1e-4)
					{
						found = true;
						evDiff[i, 0] = vals_test[j, 0] - vals_ref[i, 0];

						if (evDiff[i, 0].Magnitude > 1e-4)
							badValue = true;
					}
					else if (mag > 1e-4 && mag_one > 1e-4)
					{
						evDiff[i, 0] = vals_test[j, 0] - vals_ref[i, 0];

						if (evDiff[i, 0].Magnitude > 1e-4)
							badValue = true;

						found = true;
					}
				}

				if (found == false)
					badVector = true;
			}

			if (badVector)
			{
				Console.WriteLine("Eigenvectors differ:");
				Console.WriteLine();
				Console.WriteLine("Eigenvalues:");
				Console.WriteLine(vals_test.ToString());
				Console.WriteLine("Eigenvectors:");
				Console.WriteLine(vecs_test.ToString("0.0000"));
				Console.WriteLine();
				Console.WriteLine("Reference:");
				Console.WriteLine(vecs_ref.ToString("0.0000"));
				Console.WriteLine();
				Console.WriteLine("Dot products:");
				Console.WriteLine(dot.ToString("0.0000"));
				throw new Exception();
			}
			if (badValue)
			{
				Console.WriteLine("Eigenvalues differ:");
				Console.WriteLine("Eigenvalues:");
				Console.WriteLine(vals_test.ToString());
				Console.WriteLine("Reference:");
				Console.WriteLine(vals_ref.ToString());
				Console.WriteLine("Difference:");
				Console.WriteLine(evDiff.ToString());
				throw new Exception();
			}
		}

		private  void AssertEigenvectorsOrthoNormal(Matrix eigenvecs_a)
		{
			Matrix ident = eigenvecs_a.HermitianConjugate() * eigenvecs_a;
			bool bad = false;

			for (int i = 0; i < ident.Rows; i++)
			{
				for (int j = 0; j < ident.Columns; j++)
				{
					if (i == j && (ident[i, j] - 1).Magnitude > 1e-5)
						bad = true;
					else if (i != j && ident[i, j].Magnitude > 1e-5)
						bad = true;
				}
			}

			if (bad == true)
			{
				Console.WriteLine("Eigenvectors not orthonormal.");
				Console.WriteLine(eigenvecs_a.ToString("0.0000"));
				Console.WriteLine(ident.ToString("0.0000"));
				throw new Exception("Eigenvectors not orthonormal.");

			}
		}

		private  IEnumerable<MatrixTestCase> GetMatrixTestCases()
		{
			string source = Resources.Matrices;

			string[] lines = source.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

			int index = 0;
			while (index < lines.Length)
			{
				if (lines[index] == "Matrix:")
				{
					index++;
					int[] size = SplitOnSpace(lines[index]).Select<string, int>(int.Parse).ToArray();

					Matrix retval = ReadMatrix(lines, ref index, size[0], size[1]);
					index++;
					Matrix eigenvalues = ReadMatrix(lines, ref index, size[0], 1);
					index++;
					Matrix eigenvectors = ReadMatrix(lines, ref index, size[0], size[1]);

					MatrixTestCase test = new MatrixTestCase();
					test.Matrix = retval;
					test.Eigenvalues = eigenvalues;
					test.Eigenvectors = eigenvectors;

					AssertEigenvectorsOrthoNormal(test.Eigenvectors);

					yield return test;
				}
				else if (string.IsNullOrEmpty(lines[index]) == false)
					throw new Exception();

				index++;
			}
		}
		private  string[] SplitOnSpace(string line)
		{
			return line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		}

		private  Matrix ReadMatrix(string[] lines, ref int index, int rows, int columns)
		{
			int startIndex = index;

			Matrix retval = new Matrix(rows, columns);

			for (int i = 0; i < retval.Rows; i++)
			{
				++startIndex;
				string[] values = SplitOnComma(lines[startIndex]);

				for (int j = 0; j < values.Length; j++)
				{
					retval[i, j] = Complex.Parse(values[j]);
				}
			}

			index = startIndex;

			return retval;
		}

		private  string[] SplitOnComma(string p)
		{
			var retval = p.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
			return retval.ToArray();
		}
	}
}
