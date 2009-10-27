using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace TestMath
{
	class Program
	{
		static void Main(string[] args)
		{
			foreach (MatrixTestCase m in GetMatrixTestCases())
			{
				Console.WriteLine("Matrix:\n{0}", m.Matrix.ToString());

				Matrix eigenvals, eigenvecs;
				m.Matrix.EigenValsVecs(out eigenvals, out eigenvecs);

				CheckEigenvectorDifference(eigenvals, eigenvecs, m.Eigenvalues, m.Eigenvectors);

			}
		}

		private static void CheckEigenvectorDifference(Matrix eigenvals_a, Matrix eigenvecs_a, Matrix eigenvals_b, Matrix eigenvecs_b)
		{
			Matrix dot = eigenvecs_a.HermitianConjugate() * eigenvecs_b;
			Matrix evDiff = new Matrix(eigenvecs_a.Rows, 1);
			bool badVector = false;
			bool badValue = false;

			AssertEigenvectorsNormalized(eigenvecs_a);

			for (int i = 0; i < dot.Rows; i++)
			{
				bool found = false;

				for (int j = 0; j < dot.Columns; j++)
				{
					double mag = dot[i, j].Magnitude;
					double mag_one = Math.Abs(dot[i, j].Magnitude - 1);

					if (mag > 1e-5 && mag_one < 1e-5)
					{
						if (found == false)
						{
							found = true;
							evDiff[i, 0] = eigenvals_a[i, 0] - eigenvals_b[j, 0];

							if (evDiff[i, 0].Magnitude > 1e-5)
								badValue = true;
						}
						else
							badVector = true;
					}
					else if (mag > 1e-5 && mag_one > 1e-5)
					{
						evDiff[i, 0] = eigenvals_a[i, 0] - eigenvals_b[j, 0];

						if (evDiff[i, 0].Magnitude > 1e-5)
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
				Console.WriteLine(eigenvals_a.ToString());
				Console.WriteLine("Eigenvectors:");
				Console.WriteLine(eigenvecs_a.ToString("0.0000"));
				Console.WriteLine();
				Console.WriteLine("Reference:");
				Console.WriteLine(eigenvecs_b.ToString("0.0000"));
				Console.WriteLine();
				Console.WriteLine("Dot products:");
				Console.WriteLine(dot.ToString("0.0000"));
				throw new Exception();
			}
			if (badValue)
			{
				Console.WriteLine("Eigenvalues differ:");
				Console.WriteLine("Eigenvalues:");
				Console.WriteLine(eigenvals_a.ToString());
				Console.WriteLine("Reference:");
				Console.WriteLine(eigenvals_b.ToString());
				Console.WriteLine("Difference:");
				Console.WriteLine(evDiff.ToString());
				throw new Exception();
			}
		}

		private static void AssertEigenvectorsNormalized(Matrix eigenvecs_a)
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
				throw new Exception();

			}
		}

		private static IEnumerable<MatrixTestCase> GetMatrixTestCases()
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

					AssertEigenvectorsNormalized(test.Eigenvectors);

					yield return test;
				}
				else if (string.IsNullOrEmpty(lines[index]) == false)
					throw new Exception();

				index++;
			}
		}
		private static string[] SplitOnSpace(string line)
		{
			return line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		}

		private static Matrix ReadMatrix(string[] lines, ref int index, int rows, int columns)
		{
			Matrix retval = new Matrix(rows, columns);

			for (int i = 0; i < retval.Rows; i++)
			{
				++index;
				string[] values = SplitOnComma(lines[index]);

				for (int j = 0; j < values.Length; j++)
				{
					retval[i, j] = Complex.Parse(values[j]);
				}
			}

			return retval;
		}

		private static string[] SplitOnComma(string p)
		{
			var retval = p.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
			return retval.ToArray();
		}
	}
}
