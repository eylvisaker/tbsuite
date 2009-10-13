using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

			List<RpaParams> rpa = new List<RpaParams>();

			for (int tempIndex = 0; tempIndex < TemperatureMesh.Length; tempIndex++)
			{
				for (int muIndex = 0; muIndex < input.MuMesh.Length; muIndex++)
				{
					for (int qIndex = 0; qIndex < QMesh.Count; qIndex++)
					{
						for (int freqIndex = 0; freqIndex < FrequencyMesh.Length; freqIndex++)
						{
							rpa.Add(new RpaParams(
								qIndex,
								TemperatureMesh[tempIndex],
								FrequencyMesh[freqIndex],
								input.MuMesh[muIndex]));
						}
					}
				}
			}

			CalcX0(input, qpts, rpa);

			SaveMatricesQPlane(input, QMesh, rpa, x => x.X0, "chi_0");
			SaveMatricesQPlane(input, QMesh, rpa, x => x.Xs, "chi_s");
			SaveMatricesQPlane(input, QMesh, rpa, x => x.Xc, "chi_c");

		}

		private void CalcX0(TbInputFile input, KptList qpts, List<RpaParams> rpa)
		{
			Matrix ident = Matrix.Identity(input.Sites.Count * input.Sites.Count);

			Matrix S, C;
			CalcSpinChargeMatrices(input, out S, out C);

			Analyze("S", S);
			Analyze("C", C);

			Output.WriteLine("Calculating X0...");

			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			for (int i = 0; i < rpa.Count; i++)
			{
				SetTemperature(rpa[i].Temperature, rpa[i].ChemicalPotential);

				rpa[i].X0 = CalcX0(input, rpa[i].Frequency, qpts.Kpts[rpa[i].Qindex]);

				if (i == 0)
				{
					long time = watch.ElapsedTicks * rpa.Count;
					TimeSpan s = new TimeSpan(time);

					Output.WriteLine("Estimated total time {0:+hh.mm.ss}", s);
				}
				Complex val = new Complex();
				for (int j = 0; j < rpa[i].X0.Rows; j++)
				{
					val += rpa[i].X0[j, j];
				}

				Output.Write("q = {0}, T = {1:0.000}, mu = {2:0.000}, omega = {3:0.0000}",
					rpa[i].Qindex+1, rpa[i].Temperature, rpa[i].ChemicalPotential, rpa[i].Frequency);
				Output.WriteLine(", Tr(X_0) = {0}", val.ToString("0.0000"));
			}
			Output.WriteLine();

			double factor = InteractionAdjustment(rpa, S, C);

			S /= factor;
			C /= factor;

			Output.WriteLine("Calculating dressed susceptibilities.");

			for (int i = 0; i < rpa.Count; i++)
			{
				Matrix s_denom = (ident - S * rpa[i].X0);
				Matrix c_denom = (ident + C * rpa[i].X0);

				rpa[i].Xs = s_denom.Invert() * rpa[i].X0;
				rpa[i].Xc = c_denom.Invert() * rpa[i].X0;
			}
		}

		double InteractionAdjustment(List<RpaParams> rpa, Matrix S, Matrix C)
		{
			double largest = 0;

			foreach (Matrix x in rpa.Select(x => x.X0))
			{
				Matrix Bs = S * x;
				Matrix Bc = C * x;
				Matrix As = Bs * Bs.HermitianConjugate();
				Matrix Ac = Bc * Bc.HermitianConjugate();

				Matrix eigenvals, eigenvecs;

				As.EigenValsVecs(out eigenvals, out eigenvecs);
				double lv = eigenvals[eigenvals.Rows - 1, 0].RealPart;

				if (lv > largest)
					largest = lv;

				Ac.EigenValsVecs(out eigenvals, out eigenvecs);
				lv = eigenvals[eigenvals.Rows - 1, 0].RealPart;

				if (lv > largest)
					largest = lv;
			}

			largest = Math.Sqrt(largest);
			largest *= 1.005;

			Output.WriteLine("Adjusted interaction by dividing by {0}.", largest);

			return 1 / largest;
		}

		delegate Matrix MatrixGetter(RpaParams p);

		private void SaveByTemperature(TbInputFile input, List<KPoint> QMesh, List<RpaParams> rpa, MatrixGetter g, string name)
		{
			rpa.Sort(RpaParams.TemperatureComparison);

			Complex[] chisum = new Complex[rpa.Count];
			double[] chimag = new double[rpa.Count];
			double[] chimagsqr = new double[rpa.Count];

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

							// organize by temperature
							string filename = string.Format(
								"temperature/{0}.{1}{2}{3}{4}.T", name, l1, l2, l3, l4);

							double lastFreq = double.MinValue;
							double lastMu = double.MinValue;
							double lastq = int.MinValue;

							using (StreamWriter w = new StreamWriter(filename))
							{
								for (int index = 0; index < rpa.Count; index++)
								{
									bool newline = false;

									newline |= ChangeValue(ref lastFreq, rpa[index].Frequency);
									newline |= ChangeValue(ref lastMu, rpa[index].ChemicalPotential);
									newline |= ChangeValue(ref lastq, rpa[index].Qindex);

									if (newline)
									{
										w.WriteLine();
										w.WriteLine("# Frequency: {0}", rpa[index].Frequency);
										w.WriteLine("# Chemical Potential: {0}", rpa[index].ChemicalPotential);
										w.WriteLine("# Q: {0}", QMesh[rpa[index].Qindex]);
										w.WriteLine("#");
										w.WriteLine("# Temperature\tRe(Chi)\tIm(Chi)");
									}

									Complex val = g(rpa[index])[i, j];

									chisum[index] += val;
									chimag[index] += val.Magnitude;
									chimagsqr[index] += val.MagnitudeSquared;

									w.WriteLine("\t{0:0.000000}\t{1:0.0000000}\t{2:0.0000000}",
										rpa[index].Temperature, val.RealPart, val.ImagPart);
								}
							}
						}
					}
				}
			}
		}
		private void SaveByQPlane(TbInputFile input, List<KPoint> QMesh, List<RpaParams> rpa, MatrixGetter g, string name)
		{
			rpa.Sort(RpaParams.QIndexComparison);

			Complex[] chisum = new Complex[rpa.Count];
			double[] chimag = new double[rpa.Count];
			double[] chimagsqr = new double[rpa.Count];

			for (int l1 = 0; l1 < input.Sites.Count; l1++)
			{
				for (int l2 = 0; l2 < input.Sites.Count; l2++)
				{
					for (int l3 = 0; l3 < input.Sites.Count; l3++)
					{
						for (int l4 = 0; l4 < input.Sites.Count; l4++)
						{
							double lastFreq = double.MinValue;
							double lastMu = double.MinValue;
							double lastq = int.MinValue;

							int baseIndex = 0;

							for (int ti = 0; ti < input.TemperatureMesh.Length; ti++)
							{
								for (int wi = 0; wi < input.FrequencyMesh.Length; wi++)
								{
									string filename = string.Format("{0}.{1}{2}{3}{4}.w{5}.T{6}.qm",
														   name, l1, l2, l3, l4, wi, ti);

									Complex maxvalue = new Complex(double.MinValue, double.MinValue);
									Complex minvalue = new Complex(double.MaxValue, double.MaxValue);

									using (StreamWriter w = new StreamWriter(filename))
									{
										double last_t;
										double last_s;

										input.QPlane.GetPlaneST(input.QPlane.AllKpts[0], out last_s, out last_t);

										for (int qi = 0; qi < input.QPlane.AllKpts.Count; qi++)
										{
											Vector3 qpt = input.QPlane.AllKpts[qi];
											List<int> orbitalMap;

											double s, t;
											input.QPlane.GetPlaneST(input.QPlane.AllKpts[qi], out s, out t);

											if (Math.Abs(t - last_t) > 1e-6)
												w.WriteLine();

											int index =
												input.QPlane.GetKindex(input.Lattice, qpt, out orbitalMap, input.Symmetries);

											index += baseIndex;

											int newL1 = TransformOrbital(orbitalMap, l1);
											int newL2 = TransformOrbital(orbitalMap, l2);
											int newL3 = TransformOrbital(orbitalMap, l3);
											int newL4 = TransformOrbital(orbitalMap, l4);

											int newii = GetIndex(input, newL1, newL2);
											int newjj = GetIndex(input, newL3, newL4);

											Complex val = g(rpa[index])[newii, newjj];

											w.WriteLine(" {0}       {1}       {2}",
												s, t, val.RealPart);

											if (val.RealPart > maxvalue.RealPart) maxvalue.RealPart = val.RealPart;
											if (val.ImagPart > maxvalue.ImagPart) maxvalue.ImagPart = val.ImagPart;
											if (val.RealPart < minvalue.RealPart) minvalue.RealPart = val.RealPart;
											if (val.ImagPart < minvalue.ImagPart) minvalue.ImagPart = val.ImagPart;

											last_t = t;
											last_s = s;
										}
									}

									string gpfilename = "gnuplot." + filename;

									//minvalue.RealPart = Math.Floor(minvalue.RealPart);
									//maxvalue.RealPart = Math.Ceiling(maxvalue.RealPart);

									using (StreamWriter w = new StreamWriter(gpfilename))
									{
										w.WriteLine("#!/usr/bin/gnuplot");
										//w.WriteLine("set pm3d at bs flush center ftriangles scansbackward interpolate 1,1");
										w.WriteLine("set pm3d map flush center ftriangles scansbackward interpolate 5,5");
										w.WriteLine("set palette rgbformula 28,9,32");
										//w.WriteLine("set border 895");
										w.WriteLine("set key off");
										//w.WriteLine("set zrange [{0}:{1}]", minvalue.RealPart, maxvalue.RealPart);
										// label z = minvalue - 0.5 * (maxvalue - minvalue)
										//  set label 1 "G" at 0,0,1 font "Symbol" center front
										w.WriteLine("splot '{0}' with pm3d", filename);
									}

								}

								baseIndex += QMesh.Count;
							}
						}
					}
				}
			}
		}
		private static bool ChangeValue(ref double value, double newValue)
		{
			 if (value != newValue)
			 {
				 value = newValue;
				 return true;
			 }
			 else
				 return false;
		}

		private void SaveMatricesQPlane(TbInputFile input, List<KPoint> QMesh, List<RpaParams> chi, MatrixGetter g, string name)
		{
			if (input.TemperatureMesh.Length > 1)
			{
				Directory.CreateDirectory("temperature");
				SaveByTemperature(input, QMesh, chi, g, name);
			}

			SaveByQPlane(input, QMesh, chi, g, name);
		}
		private void Analyze(string name, Matrix S)
		{
			Output.WriteLine("Analysis of matrix {0}", name);

			Matrix evals, evecs;

			S.EigenValsVecs(out evals, out evecs);

			Complex lastEigenvalue = evals[0, 0];
			int multiplicity = 1;

			Output.WriteLine("Eigenvalues:");

			for (int i = 1; i < evals.Rows; i++)
			{
				Complex c = evals[i, 0];

				if (c == lastEigenvalue)
				{
					multiplicity++;
					continue;
				}
				Output.WriteLine("{0}      multiplicity: {1}", lastEigenvalue, multiplicity);

				lastEigenvalue = c;
				multiplicity = 1;
			}
			Output.WriteLine("{0}      multiplicity: {1}", lastEigenvalue, multiplicity);

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
							else if (l1 == l4 && l4 != l2 && l2 == l3)
							{
								S[i, j] = input.HubUp;
								C[i, j] = -input.HubUp + input.HubJ;
							}
							else if (l1 == l2 && l2 != l4 && l4 == l3)
							{
								S[i, j] = input.HubJ;
								C[i, j] = 2 * input.HubUp - input.HubJ;
							}
							else if (l1 == l3 && l3 != l2 && l2 == l4)
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

			Complex denom_factor = new Complex(0, 1e-4);

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
							
							Complex total = 0;

							for (int allkindex = 0; allkindex < input.KMesh.AllKpts.Count; allkindex++)
							{
								Complex val = 0;
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
											wfq.Coeffs[newL1] * wfq.Coeffs[newL4].Conjugate() *
											wfk.Coeffs[newL3] * wfk.Coeffs[newL2].Conjugate();

										if (coeff == 0) continue;
										if (f1 < 1e-15 && f2 < 1e-15) continue;

										Complex denom_p = (e2 - e1 + freq + denom_factor);
										//Complex denom_n = (e2 - e1 - freq - denom_factor);
										//Complex lindhard = (f1 - f2) * (1.0 / denom_p + 1.0 / denom_n);
										Complex lindhard = (f1 - f2) * (1.0 / denom_p);
										Complex contrib = coeff * lindhard;

										if (f1 == f2 && freq == 0.0)
										{
											contrib = coeff * f1 * (1 - f1) * Beta;
										}

										if (double.IsNaN(contrib.RealPart) || double.IsNaN(contrib.ImagPart))
										{
											throw new Exception("Found NaN when evaluating X0");
										}

										val += contrib;

									}
								}

								//Output.WriteLine(input.KMesh.AllKpts[kindex].Weight.ToString());
								val *= input.KMesh.AllKpts[kindex].Weight;
								total += val;
							}

							// get rid of small imaginary parts
							total.ImagPart = Math.Round(total.ImagPart, 7);

							x[i, j] = total;
							x[j, i] = total.Conjugate();
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