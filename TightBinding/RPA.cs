using System;
using System.Collections.Generic;
using System.IO;
using ERY.EMath;

namespace TightBinding
{
	class RPA
	{
		Wavefunction[][] bands;
		Wavefunction[][] kqbands;

		double Beta;

		public Wavefunction Bands(int kpt, int band)
		{
			return bands[kpt][band];
		}
		public Wavefunction BandsKQ(int kqpt, int band)
		{
			return kqbands[kqpt][band];
		}
		private void CalculateBands(TightBinding tb, TbInputFile input)
		{
			bands = new Wavefunction[input.KMesh.Kpts.Count][];
			kqbands = new Wavefunction[input.KMesh.Kpts.Count][];
			for (int i = 0; i < bands.Length; i++)
			{
				bands[i] = new Wavefunction[input.Sites.Count];
				kqbands[i] = new Wavefunction[input.Sites.Count];
			}

			for (int k = 0; k < input.KMesh.Kpts.Count; k++)
			{
				Matrix m = tb.CalcHamiltonian(input, input.KMesh.Kpts[k]);
				Matrix vals, vecs;

				m.EigenValsVecs(out vals, out vecs);

				for (int n = 0; n < vals.Rows; n++)
				{
					var wfk = new Wavefunction(input.Sites.Count);

					wfk.Energy = vals[n, 0].RealPart;

					for (int c = 0; c < vecs.Rows; c++)
					{
						wfk.Coeffs[c] = vecs[c, n];
					}

					bands[k][n] = wfk;
				}
			}
		}
		void SetTemperature(double temperature, double mu)
		{
			//currentTemperature = value;
			Beta = 1 / temperature;

			foreach (var k in bands)
			{
				foreach (Wavefunction wfk in k)
				{
					wfk.FermiFunction = FermiFunction(wfk.Energy - mu);
				}
			}
		}
		public double FermiFunction(double energy)
		{
			return 1.0 / (Math.Exp(Beta * energy) + 1);
		}

		internal void CreateKQbands(TightBinding tb, TbInputFile input, Vector3 q)
		{
			for (int i = 0; i < input.KMesh.Kpts.Count; i++)
			{
				Vector3 kq = input.KMesh.Kpts[i].Value + q;
				List<int> orbitalMap;

				int index = input.KMesh.GetKindex(
					input.Lattice, kq, out orbitalMap, input.Symmetries);

				for (int n = 0; n < bands[i].Length; n++)
				{
					kqbands[index][n] = bands[i][n].Clone(orbitalMap);
				}
			}
		}
		
		public void RunRpa(TightBinding tb, TbInputFile input, KptList qpts)
		{
			CalculateBands(tb, input);

			List<KPoint> QMesh = qpts.Kpts;
			double[] FrequencyMesh = input.FrequencyMesh;
			double[] TemperatureMesh = input.TemperatureMesh;

			Matrix[, ,] x0 = new Matrix[QMesh.Count, FrequencyMesh.Length, TemperatureMesh.Length];

			Matrix ident = Matrix.Identity(input.Sites.Count * input.Sites.Count);
			
			Matrix S, C;
			CalcSpinChargeMatrices(input, out S, out C);

			Analyze("S", S);
			Analyze("C", C);

			Console.WriteLine("Calculating X0...");

			for (int tempIndex = 0; tempIndex < TemperatureMesh.Length; tempIndex++)
			{
				SetTemperature(TemperatureMesh[tempIndex], input.ChemicalPotential);
				Console.WriteLine("Temperature: {1}    Beta: {0}",
					1 / TemperatureMesh[tempIndex], TemperatureMesh[tempIndex]);

				for (int qIndex = 0; qIndex < QMesh.Count; qIndex++)
				{
					Console.WriteLine("q = {0}", QMesh[qIndex].Value);
					//CreateKQbands(tb, input, QMesh[qIndex]);

					for (int freqIndex = 0; freqIndex < input.FrequencyMesh.Length; freqIndex++)
					{
						x0[qIndex, freqIndex, tempIndex] = CalcX0(input, input.FrequencyMesh[freqIndex], QMesh[qIndex].Value);

						Matrix s_denom = (ident - S * x0[qIndex, freqIndex, tempIndex]);
						Matrix c_denom = (ident + C * x0[qIndex, freqIndex, tempIndex]);
					}
				}
			}
			Console.WriteLine();


			Matrix[, ,] xs = new Matrix[QMesh.Count, input.FrequencyMesh.Length, input.TemperatureMesh.Length];
			Matrix[, ,] xc = new Matrix[QMesh.Count, input.FrequencyMesh.Length, input.TemperatureMesh.Length];
			List<Complex>[, ,] xs_evals = new List<Complex>[QMesh.Count, input.FrequencyMesh.Length, input.TemperatureMesh.Length];
			List<Complex>[, ,] xc_evals = new List<Complex>[QMesh.Count, input.FrequencyMesh.Length, input.TemperatureMesh.Length];

			Console.WriteLine("Calculating dressed susceptibilities.");

			for (int tempIndex = 0; tempIndex < input.TemperatureMesh.Length; tempIndex++)
			{
				Console.WriteLine("Temperature: {0}", input.TemperatureMesh[tempIndex]);

				for (int qIndex = 0; qIndex < QMesh.Count; qIndex++)
				{
					Console.WriteLine("q: {0}", QMesh[qIndex].Value);

					for (int freqIndex = 0; freqIndex < input.FrequencyMesh.Length; freqIndex++)
					{
						Matrix s_denom = (ident - S * x0[qIndex, freqIndex, tempIndex]);
						Matrix c_denom = (ident + C * x0[qIndex, freqIndex, tempIndex]);

						StoreEigenValues(s_denom, xs_evals, tempIndex, qIndex, freqIndex);
						StoreEigenValues(c_denom, xc_evals, tempIndex, qIndex, freqIndex);

						xs[qIndex, freqIndex, tempIndex] = s_denom.Invert() * x0[qIndex, freqIndex, tempIndex];
						xc[qIndex, freqIndex, tempIndex] = c_denom.Invert() * x0[qIndex, freqIndex, tempIndex];
					}
				}
			}

			SaveEigenValues(input, QMesh, xs_evals, "s", "1 - SX_0");
			SaveEigenValues(input, QMesh, xc_evals, "c", "1 + CX_0");

			SaveMatrices(input, QMesh, x0, "chi_0");
			SaveMatrices(input, QMesh, xs, "chi_s");
			SaveMatrices(input, QMesh, xc, "chi_c");

		}

		private static void StoreEigenValues(Matrix mat, List<Complex>[, ,] xs_evals, int tempIndex, int qIndex, int freqIndex)
		{
			Matrix evals, evecs;

			// currently we can't calculate eigenvalues/vectors for a regular matrix, 
			// so instead we construct a hermitian matrix where the eigenvalues are
			// real part of the desired eigenvalues.  All we care about here
			// is whether they are close to zero anyway.
			Matrix r = mat.HermitianConjugate() * mat;
			r.EigenValsVecs(out evals, out evecs);

			xs_evals[qIndex, freqIndex, tempIndex] = new List<Complex>();
			for (int i = 0; i < evals.Rows; i++)
				xs_evals[qIndex, freqIndex, tempIndex].Add(Complex.Sqrt(evals[i, 0]));
		}


		private void SaveEigenValues(TbInputFile input, List<KPoint> QMesh, List<Complex>[, ,] xs_evals, string name, string equation)
		{
			string filename = string.Format(
				"evals.{0}", name);


			using (StreamWriter w = new StreamWriter(filename))
			{
				w.WriteLine("# Eigenvalues for {0}", equation);

				for (int wi = 0; wi < input.FrequencyMesh.Length; wi++)
				{
					w.WriteLine("# Frequency: {0}", input.FrequencyMesh[wi]);

					for (int qi = 0; qi < QMesh.Count; qi++)
					{
						w.WriteLine("# Q: {0}", QMesh[qi]);

						for (int ti = 0; ti < input.TemperatureMesh.Length; ti++)
						{
							w.WriteLine("# Temperature: {0}", input.TemperatureMesh[ti]);

							var list = xs_evals[qi, wi, ti];

							for (int i = 0; i < list.Count; i++)
								w.Write("{0}   ", list[i]);

							w.WriteLine();
						}
					}
				}
			}
		}
		private void SaveMatrices(TbInputFile input, List<KPoint> QMesh, Matrix[, ,] chi, string name)
		{
			for (int l1 = 0; l1 < input.Sites.Count; l1++)
			{
				for (int l2 = 0; l2 < input.Sites.Count; l2++)
				{
					for (int l3 = 0; l3 < input.Sites.Count; l3++)
					{
						for (int l4 = 0; l4 < input.Sites.Count; l4++)
						{
							if (l1 != l2 || l2 != l3 || l3 != l4)
								continue;

							int i = GetIndex(input, l1, l2);
							int j = GetIndex(input, l3, l4);

							// organize by temperature
							string filename = string.Format(
								"{0}.{1}{2}{3}{4}.T", name, l1, l2, l3, l4);

							using (StreamWriter w = new StreamWriter(filename))
							{
								for (int wi = 0; wi < input.FrequencyMesh.Length; wi++)
								{
									w.WriteLine("# Frequency: {0}", input.FrequencyMesh[wi]);

									for (int qi = 0; qi < QMesh.Count; qi++)
									{
										w.WriteLine("#{0}\tTemp\tRe(Chi)\tIm(Chi)", QMesh[qi].Value);

										for (int ti = 0; ti < input.TemperatureMesh.Length; ti++)
										{
											Complex val = chi[qi, wi, ti][i, j];

											w.WriteLine("\t{0:0.000000}\t{1:0.0000000}\t{2:0.0000000}",
												input.TemperatureMesh[ti],
												val.RealPart, val.ImagPart);
										}
										w.WriteLine();
									}
								}
							}

							// organize by q
							filename = string.Format("{0}.{1}{2}{3}{4}.q", name, l1, l2, l3, l4);

							using (StreamWriter w = new StreamWriter(filename))
							{
								for (int wi = 0; wi < input.FrequencyMesh.Length; wi++)
								{
									w.WriteLine("# Frequency: {0}", input.FrequencyMesh[wi]);

									for (int ti = 0; ti < input.TemperatureMesh.Length; ti++)
									{
										w.WriteLine("# Temperature: {0}", input.TemperatureMesh[ti]);
										w.WriteLine("#\tQx, Qy, Qz\tRe(Chi)\tIm(Chi)");

										for (int qi = 0; qi < QMesh.Count; qi++)
										{
											Complex val = chi[qi, wi, ti][i, j];

											w.WriteLine("\t{0}\t{1:0.0000000}\t{2:0.0000000}",
												input.QMesh.Kpts[qi].Value,
												val.RealPart, val.ImagPart);
										}
										w.WriteLine();
									}
								}
							}


							// organize by w
							filename = string.Format(
								"{0}.{1}{2}{3}{4}.w", name, l1, l2, l3, l4);

							using (StreamWriter w = new StreamWriter(filename))
							{
								for (int qi = 0; qi < QMesh.Count; qi++)
								{
									w.WriteLine("#{0}", QMesh[qi].Value);

									for (int ti = 0; ti < input.TemperatureMesh.Length; ti++)
									{
										w.WriteLine("# Temperature: {0}", input.TemperatureMesh[ti]);
										w.WriteLine("#\tFrequency\tRe(Chi)\tIm(Chi)");

										for (int wi = 0; wi < input.FrequencyMesh.Length; wi++)
										{
											Complex val = chi[qi, wi, ti][i, j];

											w.WriteLine("\t{0:0.000000}\t{1:0.0000000}\t{2:0.0000000}",
												input.FrequencyMesh[wi],
												val.RealPart, val.ImagPart);
										}
										w.WriteLine();
									}
								}
							}

						}
					}
				}
			}
		}

		private void Analyze(string name, Matrix S)
		{
			Console.WriteLine("Analysis of matrix {0}", name);

			Matrix evals, evecs;

			S.EigenValsVecs(out evals, out evecs);

			Complex lastEigenvalue = evals[0, 0];
			int multiplicity = 1;

			Console.WriteLine("Eigenvalues:");

			for (int i = 1; i < evals.Rows; i++)
			{
				Complex c = evals[i, 0];

				if (c == lastEigenvalue)
				{
					multiplicity++;
					continue;
				}
				Console.WriteLine("{0}      multiplicity: {1}", lastEigenvalue, multiplicity);

				lastEigenvalue = c;
				multiplicity = 1;
			}
			Console.WriteLine("{0}      multiplicity: {1}", lastEigenvalue, multiplicity);

		}

		private void CalcSpinChargeMatrices(TbInputFile  input, out Matrix S, out Matrix C)
		{
			int size = input.Sites.Count * input.Sites.Count;

			S = new Matrix(size, size);
			C = new Matrix(size, size);

			for (int l1 = 0; l1 < input.Sites.Count; l1++)
			{
				for (int l2 = 0; l2 < input.Sites.Count; l2++)
				{
					for (int l3 = 0; l3 < input.Sites.Count; l3++)
					{
						for (int l4 = 0; l4 < input.Sites.Count; l4++)
						{
							int i = GetIndex(input, l1, l2);
							int j = GetIndex(input, l3, l4);

							if (l1 == l2 && l2 == l3 && l3 == l4)
							{
								S[i, j] = input.HubU;
								C[i, j] = input.HubU;
							}
							else if (l1 == l3 && l3 != l2 && l2 == l4)
							{
								S[i, j] = input.HubUp;
								C[i, j] = -input.HubUp + input.HubJ;
							}
							else if (l1 == l2 && l2 != l3 && l3 == l4)
							{
								S[i, j] = input.HubJ;
								C[i, j] = 2 * input.HubUp - input.HubJ;
							}
							else if (l1 == l4 && l4 != l2 && l2 == l3)
							{
								S[i, j] = input.HubJp;
								C[i, j] = input.HubJp;
							}
						}
					}
				}
			}
		}

		Matrix CalcX0(TbInputFile input, double freq, Vector3 q)
		{
			int orbitalCount = input.Sites.Count;
			int size = orbitalCount * orbitalCount;
			Matrix x = new Matrix(size, size);

			double en_min = -10;
			double en_max = 10;

			Complex denom_factor = new Complex(0, input.Smearing / 100);

			for (int l1 = 0; l1 < orbitalCount; l1++)
			{
				for (int l4 = 0; l4 < orbitalCount; l4++)
				{
					for (int l3 = l1; l3 < orbitalCount; l3++)
					{
						for (int l2 = l4; l2 < orbitalCount; l2++)
						{
							int i = GetIndex(input, l1, l2);
							int j = GetIndex(input, l3, l4);
							bool foundSymmetry = false;

							for (int s = 0; s < input.Symmetries.Count; s++)
							{
								Symmetry sym = input.Symmetries[s];
								
								if (sym.OrbitalTransform == null || sym.OrbitalTransform.Count == 0)
									continue;

								int newL1 = sym.OrbitalTransform[l1];
								int newL2 = sym.OrbitalTransform[l2];
								int newL3 = sym.OrbitalTransform[l3];
								int newL4 = sym.OrbitalTransform[l4];

								int newI = GetIndex(input, newL1, newL2);
								int newJ = GetIndex(input, newL3, newL4);

								if (newI == i && newJ == j)
									continue;

								foundSymmetry = true;

								if (newL1 > l1) foundSymmetry = false;
								if (newL2 > l2) foundSymmetry = false;
								if (newL3 > l3) foundSymmetry = false;
								if (newL4 > l4) foundSymmetry = false;

								if (foundSymmetry)
								{
									x[i, j] = x[newI, newJ];
									x[j, i] = x[i, j].Conjugate();

									break;
								}
							}

							if (foundSymmetry)
								continue;
							
							Complex val = 0;

							for (int allkindex = 0; allkindex < input.KMesh.AllKpts.Count; allkindex++)
							{
								Vector3 k = input.KMesh.AllKpts[allkindex];
								Vector3 kq = k + q;

								List<int> kOrbitalMap;
								List<int> kqOrbitalMap;

								int kindex = input.KMesh.GetKindex(
									input.Lattice, k, out kOrbitalMap, input.Symmetries);
								int kqindex = input.KMesh.GetKindex(
									input.Lattice, kq, out kqOrbitalMap, input.Symmetries);

								int newL1 = TransformOrbital(kqOrbitalMap, l1);
								int newL2 = TransformOrbital(kOrbitalMap, l2);
								int newL3 = TransformOrbital(kqOrbitalMap, l3);
								int newL4 = TransformOrbital(kOrbitalMap, l4);

								for (int n1 = 0; n1 < orbitalCount; n1++)
								{
									Wavefunction wfk = Bands(kindex, n1);
									double e1 = wfk.Energy;
									double f1 = wfk.FermiFunction;

									if (e1 < en_min) continue;
									if (e1 > en_max) break;

									for (int n2 = 0; n2 < orbitalCount; n2++)
									{
										Wavefunction wfq = Bands(kqindex, n2);//BandsKQ(kindex, n2);
										double e2 = wfq.Energy;
										double f2 = wfq.FermiFunction;

										if (e2 < en_min) continue;
										if (e2 > en_max) break;

										Complex coeff =
											wfq.Coeffs[newL1] * wfq.Coeffs[newL3].Conjugate() *
											wfk.Coeffs[newL4] * wfk.Coeffs[newL2].Conjugate();

										if (coeff == 0) continue;

										Complex denom_p = (e2 - e1 + freq + denom_factor);
										//Complex denom_n = (e2 - e1 - freq - denom_factor);
										//Complex lindhard = (f1 - f2) * (1.0 / denom_p + 1.0 / denom_n);
										Complex lindhard = (f1 - f2) * (1.0 / denom_p);
										Complex contrib = coeff * lindhard;

										if (f1 == f2 && freq == 0.0)
										{
											const double epsilon = 1e-5;
											denom_p += epsilon;
											lindhard = epsilon * (1.0 / denom_p);
											contrib = coeff * lindhard;

											contrib = coeff * f1 * (1 - f1) * Beta;
											Console.WriteLine(contrib.ToString());
										}

										val += contrib;
									}
								}

								//Console.WriteLine(input.KMesh.AllKpts[kindex].Weight.ToString());
								val *= input.KMesh.AllKpts[kindex].Weight;
							}

							// get rid of small imaginary parts
							val.ImagPart = Math.Round(val.ImagPart, 7);

							x[i, j] = val;
							x[j, i] = val.Conjugate();
						}
					}
				}
			}

			return x;
		}

		private int TransformOrbital(List<int> kqOrbitalMap, int l1)
		{
			if (kqOrbitalMap.Count > 0)
				return kqOrbitalMap[l1];
			else
				return l1;
		}


		int GetIndex(TbInputFile input, int l1, int l2)
		{
			// if this changes, be sure to correct the way 
			// x[i,j] and x[j,i] are set in CalcX0.
			return l1 * input.Sites.Count + l2;
		}

	}
}