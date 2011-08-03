using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ERY.EMath;

namespace TightBindingSuite
{
	public class RPA
	{
		int threads;

		static void Main(string[] args)
		{
			using (BootStrap b = new BootStrap())
			{
				string inputfile = b.GetInputFile("Random Phase Approximation code", "rpa", args);

				RPA inst = new RPA();
				inst.Run(inputfile);
			}
		}

		double Beta;


		void Run(string inputfile)
		{
			TightBinding tb = new TightBinding();
			tb.LoadTB(inputfile);
			tb.RunTB();

			bool ranRPA = false;

			SetCpus();

			if (tb.UseQPlane && tb.QPlane != null && tb.QPlane.Kpts.Count > 0)
			{
				RunRpa(tb, tb.QPlane, true);
				ranRPA = true;
			}
			if (tb.QMesh != null)
			{
				RunRpa(tb, tb.QMesh, false);
				ranRPA = true;
			}

			if (!ranRPA)
				Output.WriteLine("No q-points defined, so we will not run the RPA.");

		}

		private void SetCpus()
		{
			const string threadsFile = "rpa_threads";

			if (File.Exists(threadsFile))
			{
				string text = File.ReadAllText(threadsFile);

				if (int.TryParse(text, out threads))
					return;
			}

			threads = Environment.ProcessorCount - 1;

			if (threads < 1)
				threads = 1;

			using (var w = new StreamWriter(threadsFile))
			{
				w.WriteLine(threads.ToString());
			}
		}

		public void RunRpa(TightBinding tb, KptList qpts, bool plane)
		{
			List<KPoint> QMesh = qpts.Kpts;
			List<RpaParams> rpa = CreateRpaParameterList(tb, QMesh);

			Output.WriteLine("Calculating susceptibility for {0} q-points.", QMesh.Count);
			
			CalcSusceptibility(tb, qpts, rpa);

			if (plane)
			{
				SaveMatricesQPlane(tb, QMesh, rpa, x => x.X0, "chi_0");
				SaveMatricesQPlane(tb, QMesh, rpa, x => x.Xs, "chi_s");
				SaveMatricesQPlane(tb, QMesh, rpa, x => x.Xc, "chi_c");
			}
			else
			{
				OutputBands(tb, qpts, rpa, CalcX0 => CalcX0.X0, "chi_0");
			}
		}
		
		void OutputBands(TightBinding tb, KptList ks, 
		                 List<RpaParams> rpa, MatrixGetter g, string name)
		{
			
			using (StreamWriter w = new StreamWriter("eigenvalues." + name + ".q"))
			{
				w.WriteLine("# Grid");
				w.WriteLine("{0}  {1}  {2}  {3}  {4}  {5}", ks.Mesh[0], ks.Mesh[1], ks.Mesh[2],
				            ks.Shift[0], ks.Shift[1], ks.Shift[2]);
				
				w.WriteLine("# Eigenvalues");
				
				foreach(var rpa_i in rpa)
				{
					var qpt = rpa_i.QptValue;
					
					w.Write("{0}    {1}    {2}             ",
					        qpt.X, qpt.Y, qpt.Z);
					
					Matrix chi = g(rpa_i);
					Matrix evalues = chi.EigenValues();
					
					for (int j = 0; j < evalues.Rows; j++)
					{
						w.Write("{0}   ", evalues[j, 0].RealPart);
					}
					
					w.WriteLine();
				}
			}
		}		
		
		public List<RpaParams> CreateRpaParameterList(TightBinding tb, List<KPoint> QMesh)
		{
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
			return rpa;
		}

		public Wavefunction Bands(TightBinding tb, int kpt, int band)
		{
			return tb.KMesh.AllKpts[kpt].Wavefunctions[band];
		}
		void SetTemperature(TightBinding tb, double temperature, double mu)
		{
			//currentTemperature = value;
			Beta = 1 / temperature;

			tb.KMesh.SetTemperature(temperature, mu);

		}

		private void CalcSusceptibility(TightBinding tb, KptList qpts, List<RpaParams> rpa)
		{
			Matrix ident = Matrix.Identity(tb.Orbitals.Count * tb.Orbitals.Count);

			Matrix[] S, C;
			CalcSpinChargeMatrices(tb, rpa, out S, out C);

			Output.WriteLine("Calculating X0...");

			
			RpaThreadInfo[] threadInfos = CreateThreadInfos(tb, rpa, qpts);

			Output.WriteLine("Using {0} threads.", threads);

			for (int i = 0; i < threadInfos.Length; i++)
			{
				RunRpaThread(threadInfos[i]);

				if (i == 0)
					Thread.Sleep(20);
			}
			
			bool threadsRunning;

			do
			{
				threadsRunning = false;

				for (int i = 0; i < threadInfos.Length; i++)
				{
					if (threadInfos[i].Thread.ThreadState == ThreadState.Running)
						threadsRunning = true;
				}

				Thread.Sleep(10);

			} while (threadsRunning);

			Output.WriteLine();
			Output.WriteLine("Bare susceptibility calculation completed.");
			Output.WriteLine();

			double factor = InteractionAdjustment(rpa, S, C, tb);

			if (tb.Interactions.AdjustInteractions)
			{
				Output.WriteLine("Multiplying interactions by {0}.", factor);

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

			Output.WriteLine();
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

				System.Diagnostics.Debug.Assert((s_denom * s_inv).IsIdentity);

				rpa[i].Xs = rpa[i].X0 * s_inv;
				rpa[i].Xc = rpa[i].X0 * c_inv;

				for (int l1 = 0; l1 < tb.Orbitals.Count; l1++)
				{
					for (int l2 = 0; l2 < tb.Orbitals.Count; l2++)
					{
						for (int l3 = 0; l3 < tb.Orbitals.Count; l3++)
						{
							for (int l4 = 0; l4 < tb.Orbitals.Count; l4++)
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

		private void VerifySymmetry(TightBinding tb, Matrix S, int a, int b)
		{
			for (int m1 = 0; m1 < 2; m1++)
			{
				for (int m2 = 0; m2 < 2; m2++)
				{
					for (int m3 = 0; m3 < 2; m3++)
					{
						for (int m4 = 0; m4 < 2; m4++)
						{
							int l1 = Select(m1, a, b);
							int l2 = Select(m2, a, b);
							int l3 = Select(m3, a, b);
							int l4 = Select(m4, a, b);

							int a1 = Select(m1, b, a);
							int a2 = Select(m2, b, a);
							int a3 = Select(m3, b, a);
							int a4 = Select(m4, b, a);

							int i = GetIndex(tb, l1, l2);
							int j = GetIndex(tb, l3, l4);

							int ii = GetIndex(tb, a1, a2);
							int jj = GetIndex(tb, a3, a4);

							var diff = S[i, j] - S[ii, jj];

							if (diff.Magnitude > 1e-8)
							{
								Output.WriteLine("ERROR: Failed to verify symmetry.");
								Output.WriteLine("  a = {0}   b = {1}", a, b);
								Output.WriteLine("  L = {0}{1}{2}{3}", l1, l2, l3, l4);
								Output.WriteLine("  A = {0}{1}{2}{3}", a1, a2, a3, a4);
								Output.WriteLine("  ij = {0},{1}      {2},{3}", i, j, ii, jj);
								
								Output.WriteLine("  M[i,j] = {0}        {1}", S[i,j], S[ii,jj]);
								Output.WriteLine("  diff = {0}", diff.Magnitude);
								
								try
								{
									throw new Exception("blah");	
								}
								catch(Exception e)
								{
									Output.WriteLine(e.StackTrace);	
								}
								
								Environment.Exit(4);
							}
						}
					}
				}
			}
		}

		private int Select(int m1, int a, int b)
		{
			if (m1 == 0)
				return a;
			else
				return b;
		}

		private void RunRpaThread(RpaThreadInfo info)
		{
			Thread t = new Thread(RpaChi0Thread);
			info.Thread = t;
			info.Thread.Start(info);
		}

		private void RpaChi0Thread(object obj)
		{
			RpaThreadInfo info = (RpaThreadInfo)obj;

			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			TightBinding tb = info.tb;
			List<RpaParams> rpa = info.RpaParams;
			KptList qpts = info.qpts;

			for (int i = 0; i < rpa.Count; i++)
			{
				SetTemperature(tb, rpa[i].Temperature, rpa[i].ChemicalPotential);

				rpa[i].X0 = CalcX0(tb, rpa[i].Frequency, qpts.Kpts[rpa[i].Qindex]);

				if (i == 0 && info.PrimaryThread)
				{
					long time = watch.ElapsedTicks * rpa.Count;
					TimeSpan s = new TimeSpan(time);

					Output.WriteLine("Estimated total time {0:+hh.mm}", s);
				}
				Complex val = rpa[i].X0.Trace();
				
				Output.Write("q = {0}, T = {1:0.000}, mu = {2:0.000}, omega = {3:0.0000}",
					rpa[i].Qindex + 1, rpa[i].Temperature, rpa[i].ChemicalPotential, rpa[i].Frequency);
				Output.WriteLine(", Tr(X_0) = {0}", val.ToString("0.0000"));
			}
		}

		private RpaThreadInfo[] CreateThreadInfos(TightBinding tb, List<RpaParams> rpa, KptList qpts)
		{
			RpaThreadInfo[] infos = new RpaThreadInfo[threads];

			for (int i = 0; i < infos.Length; i++)
			{
				infos[i] = new RpaThreadInfo();
				infos[i].tb = tb.Clone();
				infos[i].qpts = qpts;
			}

			infos[0].PrimaryThread = true;

			for (int i = 0; i < rpa.Count; i++)
			{
				infos[i % threads].RpaParams.Add(rpa[i]);
			}

			return infos;
		}

		double InteractionAdjustment(List<RpaParams> rpa, Matrix[] S, Matrix[] C, TightBinding tb)
		{
			double largest = double.MinValue;
			RpaParams largestParams = null;
			bool Cdiv = false;

			for (int i = 0; i < rpa.Count; i++)
			{
				RpaParams p = rpa[i];
				Matrix x0 = p.X0;

				double lv = LargestPositiveEigenvalue(x0 * S[i]);

				if (lv > largest)
				{
					largest = lv;
					largestParams = p;
					Cdiv = false;
				}

				lv = LargestPositiveEigenvalue(-x0 * C[i]);

				if (lv > largest)
				{
					largest = lv;
					largestParams = p;
					Cdiv = true;
				}
			}

			if (largest >= 1)
			{
				Output.WriteLine("Interaction should be reduced to avoid divergence.", largest);
			}

			Output.WriteLine("Largest eigenvalue of denominator found at:");
			Output.WriteLine("    Eigenvalue: {0}", largest);
			Output.WriteLine("    {0} susceptibility", Cdiv ? "Charge" : "Spin");
			Output.WriteLine("    q = {0}", largestParams.QptValue);
			Output.WriteLine("    Temperature = {0}", largestParams.Temperature);
			Output.WriteLine("    Chemical Potential = {0}", largestParams.ChemicalPotential);
			Output.WriteLine("    Frequency = {0}", largestParams.Frequency);

			largest /= tb.Interactions.MaxEigenvalue;

			Output.WriteLine();
			return 1 / largest;
		}

		private static double LargestPositiveEigenvalue(Matrix x)
		{
			Matrix eigenvals, eigenvecs;
			double shift = 5;
			double lastValue = 0;
			double thisValue = 0;
			int iter = 0;

			
			if (x.IsHermitian)
			{
				x.EigenValsVecs(out eigenvals, out eigenvecs);

				return eigenvals[eigenvals.Rows - 1, 0].Magnitude;
			}
			else if (Matrix.CanDiagonalizeNonHermitian)
			{
				x.EigenValsVecs(out eigenvals, out eigenvecs);

				double largest = double.MinValue;

				for (int i = 0; i < eigenvals.Rows; i++)
				{
					if (largest < eigenvals[i, 0].RealPart)
						largest = eigenvals[i, 0].RealPart;
				}

				return largest;
			}

			do
			{
				lastValue = thisValue;

				Matrix x1 = x + Matrix.Identity(x.Rows) * shift;
				Matrix As = x1 * x1.HermitianConjugate();
				As.EigenValsVecs(out eigenvals, out eigenvecs);

				thisValue = Math.Sqrt(eigenvals[eigenvals.Rows - 1, 0].RealPart);
				thisValue -= shift;

				shift += (thisValue - lastValue);
				iter++;

			} while (Math.Abs(thisValue - lastValue) > 1e-8 || iter < 2);

			return thisValue;

		}

		delegate Matrix MatrixGetter(RpaParams p);

		private void SaveByTemperature(TightBinding tb, List<KPoint> QMesh, List<RpaParams> rpa, MatrixGetter g, string name)
		{
			rpa.Sort(RpaParams.TemperatureComparison);

			Complex[] chisum = new Complex[rpa.Count];
			double[] chimag = new double[rpa.Count];
			double[] chimagsqr = new double[rpa.Count];

			for (int l1 = 0; l1 < tb.Orbitals.Count; l1++)
			{
				for (int l2 = 0; l2 < tb.Orbitals.Count; l2++)
				{
					for (int l3 = 0; l3 < tb.Orbitals.Count; l3++)
					{
						for (int l4 = 0; l4 < tb.Orbitals.Count; l4++)
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

			for (int l1 = 0; l1 < tb.Orbitals.Count; l1++)
			{
				for (int l2 = 0; l2 < tb.Orbitals.Count; l2++)
				{
					for (int l3 = 0; l3 < tb.Orbitals.Count; l3++)
					{
						for (int l4 = 0; l4 < tb.Orbitals.Count; l4++)
						{
							double lastFreq = double.MinValue;
							double lastMu = double.MinValue;
							double lastq = int.MinValue;

							int baseIndex = 0;

							for (int ti = 0; ti < tb.TemperatureMesh.Length; ti++)
							{
								for (int ui = 0; ui < tb.MuMesh.Length; ui++)
								{
									for (int wi = 0; wi < tb.FrequencyMesh.Length; wi++)
									{
										string filename_re = string.Format("{0}.re.{1}{2}{3}{4}.w{5}.T{6}.u{7}.qm",
															   name, l1, l2, l3, l4, wi, ti, ui);
										string filename_im = string.Format("{0}.im.{1}{2}{3}{4}.w{5}.T{6}.u{7}.qm",
															   name, l1, l2, l3, l4, wi, ti, ui);
										string filename_mag = string.Format("{0}.mag.{1}{2}{3}{4}.w{5}.T{6}.u{7}.qm",
															   name, l1, l2, l3, l4, wi, ti, ui);

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

												int kindex =
													tb.QPlane.IrreducibleIndex(qpt, tb.Lattice, tb.Symmetries, out orbitalMap);

												int index = GetRpaIndex(rpa, kindex, 
													tb.TemperatureMesh[ti], 
													tb.FrequencyMesh[wi], 
													tb.MuMesh[ui]);

												int newL1 = tb.Symmetries.TransformOrbital(orbitalMap, l1);
												int newL2 = tb.Symmetries.TransformOrbital(orbitalMap, l2);
												int newL3 = tb.Symmetries.TransformOrbital(orbitalMap, l3);
												int newL4 = tb.Symmetries.TransformOrbital(orbitalMap, l4);

												int newii = GetIndex(tb, newL1, newL2);
												int newjj = GetIndex(tb, newL3, newL4);

												Complex val = g(rpa[index])[newii, newjj];

												w_re.WriteLine(" {0}       {1}       {2:0.0000000}", s, t, val.RealPart);
												w_im.WriteLine(" {0}       {1}       {2:0.0000000}", s, t, val.ImagPart);
												w_mag.WriteLine(" {0}       {1}       {2:0.0000000}", s, t, val.Magnitude);

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
		}

		private int GetRpaIndex(List<RpaParams> rpa, int qindex, double temperature, double freq, double mu)
		{
			for (int i = 0; i < rpa.Count; i++)
			{
				if (rpa[i].Qindex != qindex) continue;

				if (Math.Abs(rpa[i].Temperature - temperature) > 1e-10) continue;
				if (Math.Abs(rpa[i].Frequency - freq) > 1e-10) continue;
				if (Math.Abs(rpa[i].ChemicalPotential - mu) > 1e-10) continue;

				return i;
			}

			throw new Exception("Could not find rpa index!");
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

				int size = tb.Orbitals.Count * tb.Orbitals.Count;

				Matrix _S = new Matrix(size, size);
				Matrix _C = new Matrix(size, size);

				foreach (var interaction in tb.Interactions)
				{
					double structureFactor = interaction.StructureFactor(q);

					if (interaction.OnSite)
						CalcOnSiteInteraction(tb, _S, _C, interaction);
					else
						CalcOffSiteInteraction(tb, _S, _C, interaction, structureFactor);
				}

				System.Diagnostics.Debug.Assert(_S.IsSymmetric);
				System.Diagnostics.Debug.Assert(_C.IsSymmetric);

				S[rpa_index] = _S;
				C[rpa_index] = _C;
			}
		}

		private void CalcOnSiteInteraction(TightBinding tb, Matrix _S, Matrix _C, InteractionPair interaction)
		{
			foreach (int l1 in interaction.OrbitalsLeft)
			{
				foreach (int l2 in interaction.OrbitalsLeft)
				{
					foreach (int l3 in interaction.OrbitalsRight)
					{
						foreach (int l4 in interaction.OrbitalsRight)
						{
							int i = GetIndex(tb, l1, l2);
							int j = GetIndex(tb, l3, l4);

							if (l1 == l2 && l2 == l3 && l3 == l4)
							{
								_S[i, j] += interaction.HubbardU;
								_C[i, j] += interaction.HubbardU;
							}
							else if (l1 == l4 && l4 != l2 && l2 == l3)
							{
								_S[i, j] += interaction.InterorbitalU;
								_C[i, j] += (-interaction.InterorbitalU + interaction.Exchange);
							}
							else if (l1 == l2 && l2 != l3 && l3 == l4)
							{
								_S[i, j] += interaction.Exchange;
								_C[i, j] += 2 * interaction.InterorbitalU - interaction.Exchange;
							}
							else if (l1 == l3 && l3 != l2 && l2 == l4)
							{
								_S[i, j] += interaction.PairHopping;
								_C[i, j] += interaction.PairHopping;
							}
						}
					}
				}
			}
		}
		private void CalcOffSiteInteraction(TightBinding tb, Matrix _S, Matrix _C, InteractionPair interaction, double structureFactor)
		{
			foreach (int l1 in interaction.OrbitalsLeft)
			{
				foreach (int l2 in interaction.OrbitalsLeft)
				{
					foreach (int l3 in interaction.OrbitalsRight)
					{
						foreach (int l4 in interaction.OrbitalsRight)
						{
							int i = GetIndex(tb, l1, l2);
							int j = GetIndex(tb, l3, l4);
							double Sval = 0, Cval = 0;

							if (l1 == l2 && l2 == l3 && l3 == l4)
							{
								Sval = -0.5 * interaction.Exchange * structureFactor;
								Cval = (2 * interaction.InterorbitalU - 0.5 * interaction.Exchange) * structureFactor;
							}
							else if (l1 == l2 && l2 != l3 && l3 == l4)
							{
								Sval = -0.25 * interaction.Exchange * structureFactor;
								Cval = (2 * interaction.InterorbitalU - 0.5 * interaction.Exchange) * structureFactor;
							}

							_S[i, j] += Sval;
							_S[j, i] += Sval;

							_C[i, j] += Cval;
							_C[j, i] += Cval;
						}
					}
				}
			}
		}
		
		Matrix CalcX0(TightBinding tb, double freq, Vector3 q)
		{
			int orbitalCount = tb.Orbitals.Count;
			int size = orbitalCount * orbitalCount;
			Matrix x = new Matrix(size, size);

			Complex denom_factor = new Complex(0, 1e-4);

			//StreamWriter w = new StreamWriter(string.Format("qcont.{0}", q.ToString("0.000")));
			//bool writeThis = false;

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

							//if (l1 == 0 && l2 == 0 && l3 == 0 && l4 == 0)
							//    writeThis = true;
							//else
								//writeThis = false;

							//if (writeThis)
							//    w.WriteLine("{0}{1}{2}{3}", l1, l2, l3, l4);

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

							Complex total = 0;

							for (int allkindex = 0; allkindex < tb.KMesh.AllKpts.Count; allkindex++)
							{
								Complex val = 0;
								Vector3 k = tb.KMesh.AllKpts[allkindex];
								Vector3 kq = k + q;

								//List<int> kOrbitalMap;
								//List<int> kqOrbitalMap;

								//int kindex = tb.KMesh.IrreducibleIndex(k, tb.Lattice, tb.Symmetries, out kOrbitalMap);
								//int kqindex = tb.KMesh.IrreducibleIndex(kq, tb.Lattice, tb.Symmetries, out kqOrbitalMap);
								int kindex = tb.KMesh.AllKindex(k, tb.Lattice);
								int kqindex = tb.KMesh.AllKindex(kq, tb.Lattice);

								System.Diagnostics.Debug.Assert(kindex == allkindex);

								//int newL1 = TransformOrbital(kqOrbitalMap, l1);
								//int newL2 = TransformOrbital(kOrbitalMap, l2);
								//int newL3 = TransformOrbital(kqOrbitalMap, l3);
								//int newL4 = TransformOrbital(kOrbitalMap, l4);

								for (int n1 = 0; n1 < orbitalCount; n1++)
								{
									Wavefunction wfk = Bands(tb, kindex, n1);
									double e1 = wfk.Energy;
									double f1 = wfk.FermiFunction;

									for (int n2 = 0; n2 < orbitalCount; n2++)
									{
										Wavefunction wfq = Bands(tb, kqindex, n2);
										double e2 = wfq.Energy;
										double f2 = wfq.FermiFunction;

										Complex coeff =
											wfq.Coeffs[l1] * wfq.Coeffs[l4].Conjugate() *
											wfk.Coeffs[l3] * wfk.Coeffs[l2].Conjugate();

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

								//if (writeThis)
								//    w.WriteLine("{0}        {1}              {2}", allkindex, total, val);
							}

							x[i, j] = total;
							x[j, i] = total.Conjugate();

							//if (writeThis)
							//{
							//    w.WriteLine("total for {0}{1}{2}{3}: {4}", l1, l2, l3, l4, total);
							//    w.WriteLine("---------------------");
							//}
						}
					}
				}
			}

			//w.Close();

			return x;
		}



		int GetIndex(TightBinding tb, int l1, int l2)
		{
			// if this changes, be sure to correct the way 
			// x[i,j] and x[j,i] are set in CalcX0.
			return l1 * tb.Orbitals.Count + l2;
		}

	}
}