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
		private void CalculateBands(TightBinding tb)
		{
			bands = new Wavefunction[tb.KMesh.Kpts.Count][];
			kqbands = new Wavefunction[tb.KMesh.Kpts.Count][];
			for (int i = 0; i < bands.Length; i++)
			{
				bands[i] = new Wavefunction[tb.Sites.Count];
				kqbands[i] = new Wavefunction[tb.Sites.Count];
			}

			for (int k = 0; k < tb.KMesh.Kpts.Count; k++)
			{
				Matrix m = tb.CalcHamiltonian(tb.KMesh.Kpts[k]);
				Matrix vals, vecs;

				m.EigenValsVecs(out vals, out vecs);

				StreamWriter w = new StreamWriter("matrix");

				for (int i = 0; i < m.Rows; i++)
				{
					for (int j = 0; j < m.Columns; j++)
					{
						w.Write("{0}  {1}   ", m[i, j].RealPart, m[i, j].ImagPart);
					}
					w.WriteLine();
				}
				for (int i = 0; i < m.Rows; i++)
				{
					for (int j = 0; j < m.Columns; j++)
					{
						w.Write("{0}  {1}  ", vecs[i, j].RealPart, vecs[i, j].ImagPart);
					}
					w.WriteLine();
				}

				w.Close();

				for (int n = 0; n < vals.Rows; n++)
				{
					var wfk = new Wavefunction(tb.Sites.Count);

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

		internal void CreateKQbands(TightBinding tb, Vector3 q)
		{
			for (int i = 0; i < tb.KMesh.Kpts.Count; i++)
			{
				Vector3 kq = tb.KMesh.Kpts[i].Value + q;
				List<int> orbitalMap;

				int index = tb.KMesh.GetKindex(
					tb.Lattice, kq, out orbitalMap, tb.Symmetries);

				for (int n = 0; n < bands[i].Length; n++)
				{
					kqbands[index][n] = bands[i][n].Clone(orbitalMap);
				}
			}
		}
		
		public void RunRpa(TightBinding tb, KptList qpts)
		{
			CalculateBands(tb);

			List<KPoint> QMesh = qpts.Kpts;
			double[] FrequencyMesh = tb.FrequencyMesh;
			double[] TemperatureMesh = tb.TemperatureMesh;

			List<RpaParams> rpa = new List<RpaParams>();

			for (int tempIndex = 0; tempIndex < TemperatureMesh.Length; tempIndex++)
			{
				for (int muIndex = 0; muIndex < tb.MuMesh.Length; muIndex++)
				{
					for (int qIndex = 0; qIndex < QMesh.Count; qIndex++)
					{
						for (int freqIndex = 0; freqIndex < FrequencyMesh.Length; freqIndex++)
						{
							rpa.Add(new RpaParams(
								qIndex,
								QMesh[qIndex].Value,
								TemperatureMesh[tempIndex],
								FrequencyMesh[freqIndex],
								tb.MuMesh[muIndex]));
						}
					}
				}
			}

			CalcX0(tb, qpts, rpa);

			SaveMatricesQPlane(tb, QMesh, rpa, x => x.X0, "chi_0");
			SaveMatricesQPlane(tb, QMesh, rpa, x => x.Xs, "chi_s");
			SaveMatricesQPlane(tb, QMesh, rpa, x => x.Xc, "chi_c");

		}

		private void CalcX0(TightBinding tb, KptList qpts, List<RpaParams> rpa)
		{
			Matrix ident = Matrix.Identity(tb.Sites.Count * tb.Sites.Count);

			Matrix[] S, C;
			CalcSpinChargeMatrices(tb, rpa, out S, out C);

			Output.WriteLine("Calculating X0...");

			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			for (int i = 0; i < rpa.Count; i++)
			{
				SetTemperature(rpa[i].Temperature, rpa[i].ChemicalPotential);

				rpa[i].X0 = CalcX0(tb, rpa[i].Frequency, qpts.Kpts[rpa[i].Qindex]);

				if (i == 0)
				{
					long time = watch.ElapsedTicks * rpa.Count;
					TimeSpan s = new TimeSpan(time);

					Output.WriteLine("Estimated total time {0:+hh.mm.ss}", s);
				}
				Complex val = rpa[i].X0.Trace();

				Output.Write("q = {0}, T = {1:0.000}, mu = {2:0.000}, omega = {3:0.0000}",
					rpa[i].Qindex+1, rpa[i].Temperature, rpa[i].ChemicalPotential, rpa[i].Frequency);
				Output.WriteLine(", Tr(X_0) = {0}", val.ToString("0.0000"));
			}
			Output.WriteLine();

			double factor = InteractionAdjustment(rpa, S, C);

			if (tb.Interactions.AdjustInteractions)
			{
				for (int i = 0; i < rpa.Count; i++)
				{
					S[i] *= factor;
					C[i] *= factor;
				}
			}
			else if (factor < 1)
			{
				Output.WriteLine("WARNING:  There will be divergent geometric series.");
				Output.WriteLine("          Interpret results with care!");
			}

			Output.WriteLine("Calculating dressed susceptibilities.");
			Output.WriteLine();

			RpaParams largestParams = null;
			double largest = 0;
			string indices = "";
			bool charge = false;

			for (int i = 0; i < rpa.Count; i++)
			{
				Matrix s_denom = (ident - S[i] * rpa[i].X0);
				Matrix c_denom = (ident + C[i] * rpa[i].X0);

				Matrix s_inv = s_denom.Invert();
				Matrix c_inv = c_denom.Invert();

				rpa[i].Xs = rpa[i].X0 * s_inv;
				rpa[i].Xc = rpa[i].X0 * c_inv;

				for (int l1 = 0; l1 < tb.Sites.Count; l1++)
				{
					for (int l2 = 0; l2 < tb.Sites.Count; l2++)
					{
						for (int l3 = 0; l3 < tb.Sites.Count; l3++)
						{
							for (int l4 = 0; l4 < tb.Sites.Count; l4++)
							{
								int a = GetIndex(tb, l1, l2);
								int b = GetIndex(tb, l3, l4);

								bool found = false;

								if (rpa[i].Xs[a,b].MagnitudeSquared > largest)
								{
									largest = rpa[i].Xs[a,b].MagnitudeSquared;
									charge = false;
									found = true;
								}
								if (rpa[i].Xc[a, b].MagnitudeSquared > largest)
								{
									largest = rpa[i].Xc[a, b].MagnitudeSquared;
									charge = true;
									found = true;
								}
								if (found == false)
									continue;

								indices = string.Format("{0}{1}{2}{3}", l1, l2, l3, l4);
								largestParams = rpa[i];
							}
						}
					}
				}
			}

			Output.WriteLine("Largest susceptibility found at:");
			Output.WriteLine("    {0} susceptibility: {1}", charge ? "Charge" : "Spin", Math.Sqrt(largest));
			Output.WriteLine("    Indices: {0}", indices);
			Output.WriteLine("    Temperature: {0}", largestParams.Temperature);
			Output.WriteLine("    Frequency: {0}", largestParams.Frequency);
			Output.WriteLine("    Chemical Potential: {0}", largestParams.ChemicalPotential);
			Output.WriteLine("    Q: {0}", largestParams.QptValue);

		}

		double InteractionAdjustment(List<RpaParams> rpa, Matrix[] S, Matrix[] C)
		{
			double largest = 0;
			RpaParams largestParams = null;
			bool Cdiv = false;

			for (int i = 0; i < rpa.Count; i++)
			{
				RpaParams p = rpa[i];
				Matrix x0 = p.X0;

				double lv = LargestEigenvalue(x0 * S[i]);

				if (lv > largest)
				{
					largest = lv;
					largestParams = p;
					Cdiv = false;
				}

				lv = LargestEigenvalue(x0 * C[i]);

				if (lv > largest)
				{
					largest = lv;
					largestParams = p;
					Cdiv = true;
				}
			}

			largest = Math.Sqrt(largest);

			if (largest >= 1)
			{
				Output.WriteLine("Interaction should be divided by {0} to avoid divergence.", largest);
			}
			Output.WriteLine("Largest eigenvalue found at:");
			Output.WriteLine("    q = {0}", largestParams.QptValue);
			Output.WriteLine("    T = {0}", largestParams.Temperature);
			Output.WriteLine("    u = {0}", largestParams.ChemicalPotential);
			Output.WriteLine("    w = {0}", largestParams.Frequency);
			Output.WriteLine("    {0} susceptibility", Cdiv ? "Charge" : "Spin");

			largest *= 1.001;

			Output.WriteLine();
			return 1 / largest;
		}

		private static double LargestEigenvalue(Matrix x)
		{
			Matrix As = x * x.HermitianConjugate();
			Matrix eigenvals, eigenvecs;

			As.EigenValsVecs(out eigenvals, out eigenvecs);
			double lv = eigenvals[eigenvals.Rows - 1, 0].RealPart;
			return lv;
		}

		delegate Matrix MatrixGetter(RpaParams p);

		private void SaveByTemperature(TightBinding tb, List<KPoint> QMesh, List<RpaParams> rpa, MatrixGetter g, string name)
		{
			rpa.Sort(RpaParams.TemperatureComparison);

			Complex[] chisum = new Complex[rpa.Count];
			double[] chimag = new double[rpa.Count];
			double[] chimagsqr = new double[rpa.Count];

			for (int l1 = 0; l1 < tb.Sites.Count; l1++)
			{
				for (int l2 = 0; l2 < tb.Sites.Count; l2++)
				{
					for (int l3 = 0; l3 < tb.Sites.Count; l3++)
					{
						for (int l4 = 0; l4 < tb.Sites.Count; l4++)
						{
							int i = GetIndex(tb, l1, l2);
							int j = GetIndex(tb, l3, l4);

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
		private void SaveByQPlane(TightBinding tb, List<KPoint> QMesh, List<RpaParams> rpa, MatrixGetter g, string name)
		{
			rpa.Sort(RpaParams.QIndexComparison);

			Complex[] chisum = new Complex[rpa.Count];
			double[] chimag = new double[rpa.Count];
			double[] chimagsqr = new double[rpa.Count];

			for (int l1 = 0; l1 < tb.Sites.Count; l1++)
			{
				for (int l2 = 0; l2 < tb.Sites.Count; l2++)
				{
					for (int l3 = 0; l3 < tb.Sites.Count; l3++)
					{
						for (int l4 = 0; l4 < tb.Sites.Count; l4++)
						{
							double lastFreq = double.MinValue;
							double lastMu = double.MinValue;
							double lastq = int.MinValue;

							int baseIndex = 0;

							for (int ti = 0; ti < tb.TemperatureMesh.Length; ti++)
							{
								for (int wi = 0; wi < tb.FrequencyMesh.Length; wi++)
								{
									string filename_re = string.Format("{0}.re.{1}{2}{3}{4}.w{5}.T{6}.qm",
														   name, l1, l2, l3, l4, wi, ti);
									string filename_im = string.Format("{0}.im.{1}{2}{3}{4}.w{5}.T{6}.qm",
														   name, l1, l2, l3, l4, wi, ti);
									string filename_mag = string.Format("{0}.mag.{1}{2}{3}{4}.w{5}.T{6}.qm",
														   name, l1, l2, l3, l4, wi, ti);

									Complex maxvalue = new Complex(double.MinValue, double.MinValue);
									Complex minvalue = new Complex(double.MaxValue, double.MaxValue);

									using (StreamWriter w_re = new StreamWriter(filename_re))
									using (StreamWriter w_im = new StreamWriter(filename_im))
									using (StreamWriter w_mag = new StreamWriter(filename_mag))
									{
										double last_t;
										double last_s;

										tb.QPlane.GetPlaneST(tb.QPlane.AllKpts[0], out last_s, out last_t);

										for (int qi = 0; qi < tb.QPlane.AllKpts.Count; qi++)
										{
											Vector3 qpt = tb.QPlane.AllKpts[qi];
											List<int> orbitalMap;

											double s, t;
											tb.QPlane.GetPlaneST(tb.QPlane.AllKpts[qi], out s, out t);

											if (Math.Abs(t - last_t) > 1e-6)
											{
												w_re.WriteLine();
												w_im.WriteLine();
												w_mag.WriteLine();
											}

											int index =
												tb.QPlane.GetKindex(tb.Lattice, qpt, out orbitalMap, tb.Symmetries);

											index += baseIndex;

											int newL1 = TransformOrbital(orbitalMap, l1);
											int newL2 = TransformOrbital(orbitalMap, l2);
											int newL3 = TransformOrbital(orbitalMap, l3);
											int newL4 = TransformOrbital(orbitalMap, l4);

											int newii = GetIndex(tb, newL1, newL2);
											int newjj = GetIndex(tb, newL3, newL4);

											Complex val = g(rpa[index])[newii, newjj];

											w_re.WriteLine(" {0}       {1}       {2}", s, t, val.RealPart);
											w_im.WriteLine(" {0}       {1}       {2}", s, t, val.ImagPart);
											w_mag.WriteLine(" {0}       {1}       {2}", s, t, val.Magnitude);

											if (val.RealPart > maxvalue.RealPart) maxvalue.RealPart = val.RealPart;
											if (val.ImagPart > maxvalue.ImagPart) maxvalue.ImagPart = val.ImagPart;
											if (val.RealPart < minvalue.RealPart) minvalue.RealPart = val.RealPart;
											if (val.ImagPart < minvalue.ImagPart) minvalue.ImagPart = val.ImagPart;

											last_t = t;
											last_s = s;
										}
									}

									for (int i = 0; i < 3; i++)
									{
										string filename;
										switch (i)
										{
											case 0: filename = filename_re; break;
											case 1: filename = filename_im; break;
											case 2: filename = filename_mag; break;
											default:
												continue;
										}

										string gpfilename = "gnuplot." + filename;

										//minvalue.RealPart = Math.Floor(minvalue.RealPart);
										//maxvalue.RealPart = Math.Ceiling(maxvalue.RealPart);

										using (StreamWriter w = new StreamWriter(gpfilename))
										{
											w.WriteLine("#!/usr/bin/gnuplot");
											//w.WriteLine("set pm3d at bs flush center ftriangles scansbackward interpolate 1,1");
											w.WriteLine("set pm3d map flush center ftriangles scansbackward interpolate 5,5");
											w.WriteLine("set palette rgbformula 23,9,-36");
											//w.WriteLine("set border 895");
											w.WriteLine("set key off");
											//w.WriteLine("set zrange [{0}:{1}]", minvalue.RealPart, maxvalue.RealPart);
											// label z = minvalue - 0.5 * (maxvalue - minvalue)
											//  set label 1 "G" at 0,0,1 font "Symbol" center front
											w.WriteLine("splot '{0}' with pm3d", filename);
										}
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

		private void SaveMatricesQPlane(TightBinding tb, List<KPoint> QMesh, List<RpaParams> chi, MatrixGetter g, string name)
		{
			if (tb.TemperatureMesh.Length > 1)
			{
				Directory.CreateDirectory("temperature");
				SaveByTemperature(tb, QMesh, chi, g, name);
			}

			SaveByQPlane(tb, QMesh, chi, g, name);
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

		private void CalcSpinChargeMatrices(TightBinding tb, List<RpaParams> rpa, out Matrix[] S, out Matrix[] C)
		{
			S = new Matrix[rpa.Count];
			C = new Matrix[rpa.Count];

			for (int rpa_index = 0; rpa_index < rpa.Count; rpa_index++)
			{
				Vector3 q = rpa[rpa_index].QptValue;

				int size = tb.Sites.Count * tb.Sites.Count;

				Matrix _S = new Matrix(size, size);
				Matrix _C = new Matrix(size, size);

				foreach (var interaction in tb.Interactions)
				{
					double structureFactor = interaction.StructureFactor(q);

					foreach (int l1 in interaction.SitesLeft)
					{
						foreach (int l2 in interaction.SitesLeft)
						{
							foreach (int l3 in interaction.SitesRight)
							{
								foreach (int l4 in interaction.SitesRight)
								{
									int i = GetIndex(tb, l1, l2);
									int j = GetIndex(tb, l3, l4);

									if (l1 == l2 && l2 == l3 && l3 == l4)
									{
										_S[i, j] += interaction.HubbardU * structureFactor;
										_C[i, j] += interaction.HubbardU * structureFactor;
									}
									else if (l1 == l4 && l4 != l2 && l2 == l3)
									{
										_S[i, j] += interaction.InterorbitalU * structureFactor;
										_C[i, j] += (-interaction.InterorbitalU + interaction.Exchange) * structureFactor;
									}
									else if (l1 == l2 && l2 != l3 && l3 == l4)
									{
										_S[i, j] += interaction.Exchange * structureFactor;
										_C[i, j] += (2 * interaction.InterorbitalU - interaction.Exchange) * structureFactor;
									}
									else if (l1 == l3 && l3 != l2 && l2 == l4)
									{
										_S[i, j] += interaction.PairHopping * structureFactor;
										_C[i, j] += interaction.PairHopping * structureFactor;
									}
								}
							}
						}
					}
				}

				S[rpa_index] = _S;
				C[rpa_index] = _C;
			}
		}

		Matrix CalcX0(TightBinding tb, double freq, Vector3 q)
		{
			int orbitalCount = tb.Sites.Count;
			int size = orbitalCount * orbitalCount;
			Matrix x = new Matrix(size, size);

			double en_min = -10;
			double en_max = 10;

			Complex denom_factor = new Complex(0, 1e-4);

			//using (StreamWriter ww = new StreamWriter("hamilt"))
			//{
			//    for (int i = 0; i < tb.KMesh.Kpts.Count; i++)
			//    {

			//        for (int n = 0; n < orbitalCount; n++)
			//        {
			//            var wfk = Bands(i, n);

			//            ww.Write("{0}   {1}   {2}          ", i, n, wfk.Energy);

			//            for (int c = 0; c < orbitalCount; c++)
			//            {
			//                ww.Write("     {0}", wfk.Coeffs[c]);
			//            }

			//            ww.WriteLine();
			//        }
			//    }
			//}
			for (int l1 = 0; l1 < orbitalCount; l1++)
			{
				for (int l4 = 0; l4 < orbitalCount; l4++)
				{
					for (int l3 = l1; l3 < orbitalCount; l3++)
					{
						for (int l2 = l4; l2 < orbitalCount; l2++)
						{
							int i = GetIndex(tb, l1, l2);
							int j = GetIndex(tb, l3, l4);
							bool foundSymmetry = false;

							

							for (int s = 0; s < tb.Symmetries.Count; s++)
							{
								Symmetry sym = tb.Symmetries[s];
								
								if (sym.OrbitalTransform == null || sym.OrbitalTransform.Count == 0)
									continue;

								int newL1 = sym.OrbitalTransform[l1];
								int newL2 = sym.OrbitalTransform[l2];
								int newL3 = sym.OrbitalTransform[l3];
								int newL4 = sym.OrbitalTransform[l4];

								int newI = GetIndex(tb, newL1, newL2);
								int newJ = GetIndex(tb, newL3, newL4);

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

							//string filename = string.Format("vars.{0}{1}{2}{3}", l1, l2, l3, l4);
							//StreamWriter w = new StreamWriter(File.Open(filename, FileMode.Append));
							//w.WriteLine("total for q = {0}", q);

							Complex total = 0;

							for (int allkindex = 0; allkindex < tb.KMesh.AllKpts.Count; allkindex++)
							{
								Complex val = 0;
								Vector3 k = tb.KMesh.AllKpts[allkindex];
								Vector3 kq = k + q;

								List<int> kOrbitalMap;
								List<int> kqOrbitalMap;

								int kindex = tb.KMesh.GetKindex(
									tb.Lattice, k, out kOrbitalMap, tb.Symmetries);
								int kqindex = tb.KMesh.GetKindex(
									tb.Lattice, kq, out kqOrbitalMap, tb.Symmetries);

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

										//coeff = 1;

										if (coeff == 0) continue;
										if (f1 < 1e-15 && f2 < 1e-15) continue;

										Complex denom_p = (e2 - e1 + freq + denom_factor);
										//Complex denom_n = (e2 - e1 - freq - denom_factor);
										//Complex lindhard = (f1 - f2) * (1.0 / denom_p + 1.0 / denom_n);
										Complex lindhard = (f1 - f2) * (1.0 / denom_p);
										Complex contrib = coeff * lindhard;

										if (Math.Abs(f1 - f2) < 1e-11 && freq == 0.0)
										{
											contrib = coeff * f1 * (1 - f1) * Beta;
										}

										//w.Write("{0}  {1}   {2}   {3}           ", kindex, kqindex, n1, n2);
										//w.WriteLine("{0}   {1}   {2}   {3}   {4}", coeff, e1, e2, f1, f2);

										if (double.IsNaN(contrib.RealPart) || double.IsNaN(contrib.ImagPart))
										{
											throw new Exception("Found NaN when evaluating X0");
										}

										val += contrib;
									}
								}

								//w.WriteLine("{0}  {1}   total           {2} + {3}i", kindex, kqindex,
								//    Math.Round(val.RealPart, 4), Math.Round(val.ImagPart, 4));

								//Output.WriteLine(tb.KMesh.AllKpts[kindex].Weight.ToString());
								val *= tb.KMesh.AllKpts[kindex].Weight;
								total += val;
							}

							// get rid of small imaginary parts
							total.ImagPart = Math.Round(total.ImagPart, 7);

							x[i, j] = total;
							x[j, i] = total.Conjugate();

							//w.WriteLine("total is : {0}", total);
							//w.Close();
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


		int GetIndex(TightBinding tb, int l1, int l2)
		{
			// if this changes, be sure to correct the way 
			// x[i,j] and x[j,i] are set in CalcX0.
			return l1 * tb.Sites.Count + l2;
		}

	}
}