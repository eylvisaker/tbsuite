using System;
using System.Collections.Generic;
using System.IO;
using ERY.EMath;

namespace RPA
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			new MainClass().Run(args);

			Console.ReadKey(false);

		}
		
		void Run(string[] args)
		{
			string filename = "";
			bool createMesh = false;
			
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].StartsWith("-") == false)
				{
					filename = args[i];
					continue;	
				}
				
				switch(args[i])
				{
				case "-sample":
					SampleInput();
					return;
				case "-mesh":
					createMesh = true;
					break;
				}
			}
			
			if (filename == "")
			{
				Console.WriteLine("No input file specified.");
				Console.WriteLine();

				Usage();
				return;
			}
		
			InputFile input = ReadInput(filename);
			if (input == null)
				return;
			
			ReadReciprocalVecs(input);
			
			if (createMesh)
			{
				Console.WriteLine("Generating k-point mesh.");
				CreateMesh(input);
				return;
			}
			
			if (File.Exists("+coeff") == false)
			{
				Console.WriteLine("No +coeff file found.  Have you run fplo after generating the mesh?");
				return;
			}
			
			Console.WriteLine();
			using (StreamReader r = new StreamReader("+coeff"))
			{
				input.ReadWavefunctions(r);
			}
			
			Console.WriteLine("Read band structure.");

			Console.WriteLine();
			Console.WriteLine("Running RPA.");

			RunRpa(input);
		}
		
		void CreateMesh(InputFile input)
		{
			Console.WriteLine("Mesh: {0} {1} {2}", input.MeshSize[0], input.MeshSize[1], input.MeshSize[2]);
			
			Console.WriteLine("Scale: {0}", input.KptScale);
			
			Vector3[] kpts = input.KptMesh;
			
			using (StreamWriter w = new StreamWriter("=.kp"))
			{
				w.WriteLine("{0} f 0 0", kpts.Length);
				
				for (int i = 0; i < kpts.Length; i++)
				{
					Vector3 red = kpts[i];
					Vector3 k = red[0] * input.ReciprocalVecs[0] + red[1] * input.ReciprocalVecs[1] + red[2] * input.ReciprocalVecs[2];
					
					k /= input.KptScale;
					
					w.WriteLine("{0}               {1}             {2}", 
					            k.X, k.Y, k.Z);
				}
			}
			using (StreamWriter w = new StreamWriter("=.coeff"))
			{
			}

			Console.WriteLine("Make sure that the =.kp and =.coeff files");
			Console.WriteLine("are in the directory where you will run FPLO.");
		}
		void SampleInput()
		{
			Console.WriteLine("# Grid for the k-point mesh.  Should be even numbers.");
			Console.WriteLine("# The example value here is probably not sufficient for converged results.");
			Console.WriteLine("Mesh 12 12 12");
			Console.WriteLine();

			Console.WriteLine("# Grid for the q-point mesh.  These should divide evenly");
			Console.WriteLine("# into the k-mesh.");
			Console.WriteLine("Mesh 4 4 2");
			Console.WriteLine();

			Console.WriteLine("# Orbital indices which should be used.  Check the header of the");
			Console.WriteLine("# +coeff file.");
			Console.WriteLine("Orbitals 1 2 3 4 5");
			Console.WriteLine();

			Console.WriteLine("# Frequency range to be considered.");
			Console.WriteLine("Freq 0 5");
			Console.WriteLine("FreqSteps 10");
			Console.WriteLine();

			Console.WriteLine("# Hubbard U, U', J, J'");
			Console.WriteLine("U 4 4 1 1");
			Console.WriteLine();

			Console.WriteLine("# Temperature range to use.");
			Console.WriteLine("Temperature 0.01  0.1");
			Console.WriteLine("TemperatureSteps 5");
			
		}
		void Usage()
		{
			Console.WriteLine("Usage: rpa [options] input_file");
			Console.WriteLine();
			Console.WriteLine("The input file name must be the last argument.");
			Console.WriteLine();
			Console.WriteLine("Options:");
			Console.WriteLine("    -sample    Prints a sample input file to stdout.");
			Console.WriteLine("    -mesh      Create the =.kp file for fplo.");
		}
		
		InputFile ReadInput(string filename)
		{
			InputFile retval = new InputFile();
			
			using (StreamReader r = new StreamReader(filename))
			{
				while (r.EndOfStream == false)
				{
					string line = r.ReadLine().Trim();
					string var, args;
					
					if (line.StartsWith("#"))
						continue;
					
					if (line.Contains(" "))
					{
						var = line.Substring(0, line.IndexOf(" "));
						args = line.Substring(line.IndexOf(" ")+1);
					}
					else
					{
						var = line;
						args = "";
					}
					
					if (string.IsNullOrEmpty(var))
						continue;
					
					try
					{
						retval.SetVariable(var, args);
					}
					catch(Exception e)
					{
						Console.WriteLine("Error reading: " + var + " " + args);
						Console.WriteLine(e.Message);
						System.Environment.Exit(1);
					}
				}
			}

			retval.Validate();

			// force creation of the mesh
			var x = retval.FrequencyMesh;
			
			return retval;
		}
		
		void ReadReciprocalVecs(InputFile input)
		{
			if (File.Exists("out") == false)
			{
				Console.WriteLine("**********************************************");
				Console.WriteLine("You must run FPLO and redirect its output");
				Console.WriteLine("to the \"out\" file.");
				Console.WriteLine("**********************************************");
				Console.WriteLine();
				
				throw new Exception("No out file found.");
			}
			
			using (StreamReader r = new StreamReader("out"))	
			{
				while (r.EndOfStream == false)
				{
					string line = r.ReadLine().Trim();
					
					if (line.StartsWith("lattice constants:"))
					{
						// pull out the first number
						string text = line.Substring(19).Trim().Split(' ')[0];
						double val = double.Parse(text);
						
						input.KptScale = 2 * Math.PI / val;
					}
					
					// search for reciprocal lattice vectors.
					if (line.StartsWith("a1  :") == false)
						continue;

					input.DirectVecs[0] = ReadFploVec(line);
					input.DirectVecs[1] = ReadFploVec(r.ReadLine().Trim());
					input.DirectVecs[2] = ReadFploVec(r.ReadLine().Trim());

					r.ReadLine();

					input.ReciprocalVecs[0] = ReadFploVec(r.ReadLine().Trim());
					input.ReciprocalVecs[1] = ReadFploVec(r.ReadLine().Trim());
					input.ReciprocalVecs[2] = ReadFploVec(r.ReadLine().Trim());
					
					for (int i = 0; i < 2; i++)
						input.ReciprocalVecs[i] *= 2 * Math.PI;
					
					break;
				}
			}
			Console.WriteLine("Reciprocal lattice vecs: ");
			Console.WriteLine(input.ReciprocalVecs[0].ToString());
			Console.WriteLine(input.ReciprocalVecs[1].ToString());
			Console.WriteLine(input.ReciprocalVecs[2].ToString());
		}
		Vector3 ReadFploVec(string line)
		{
			var v = line.Substring(5).Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
			
			return new Vector3(double.Parse(v[0]), double.Parse(v[1]), double.Parse(v[2]));
		}


		void RunRpa(InputFile input)
		{
			Matrix[,,] x0 = new Matrix[input.QptMesh.Length, input.FrequencyMesh.Length, input.TemperatureMesh.Length];

			Console.WriteLine("Calculating X0...");

			for (int tempIndex = 0; tempIndex < input.TemperatureMesh.Length; tempIndex++)
			{
				input.SetTemperature(input.TemperatureMesh[tempIndex]);
				Console.WriteLine("Temperature: {1}    Beta: {0}", input.Beta, input.Temperature);

				for (int qIndex = 0; qIndex < input.QptMesh.Length; qIndex++)
				{
					input.CreateKQbands(input.QptMesh[qIndex]);
					Console.WriteLine("q = {0}", input.QptMesh[qIndex]);

					for (int freqIndex = 0; freqIndex < input.FrequencyMesh.Length; freqIndex++)
					{
						x0[qIndex, freqIndex, tempIndex] =
							CalcX0(input, input.FrequencyMesh[freqIndex]);
					}
				}
			}
			Console.WriteLine();

			Matrix S, C;
			CalcSpinChargeMatrices(input, out S, out C);

			Analyze("S", S);
			Analyze("C", C);

			Matrix[,,] xs = new Matrix[input.QptMesh.Length, input.FrequencyMesh.Length, input.TemperatureMesh.Length];
			Matrix[,,] xc = new Matrix[input.QptMesh.Length, input.FrequencyMesh.Length, input.TemperatureMesh.Length];
			Matrix ident = Matrix.Identity(input.OrbitalCount * input.OrbitalCount);

			for (int tempIndex = 0; tempIndex < input.TemperatureMesh.Length; tempIndex++)
			{
				for (int qIndex = 0; qIndex < input.QptMesh.Length; qIndex++)
				{
					for (int freqIndex = 0; freqIndex < input.FrequencyMesh.Length; freqIndex++)
					{
						Matrix s_denom = (ident - S * x0[qIndex, freqIndex, tempIndex]);
						Matrix c_denom = (ident + C * x0[qIndex, freqIndex, tempIndex]);

						xs[qIndex, freqIndex, tempIndex] = s_denom.Invert() * x0[qIndex, freqIndex, tempIndex];
						xc[qIndex, freqIndex, tempIndex] = c_denom.Invert() * x0[qIndex, freqIndex, tempIndex];
					}
				}
			}

			SaveMatrices(input, x0, "chi_0");
			SaveMatrices(input, xs, "chi_s");
			SaveMatrices(input, xc, "chi_c");

		}

		private void SaveMatrices(InputFile input, Matrix[,,] chi, string name)
		{
			for (int l1 = 0; l1 < input.OrbitalCount; l1++)
			{
				for (int l2 = 0; l2 < input.OrbitalCount; l2++)
				{
					for (int l3 = 0; l3 < input.OrbitalCount; l3++)
					{
						for (int l4 = 0; l4 < input.OrbitalCount; l4++)
						{
							if (l1 != l2 || l2 != l3 || l3 != l4)
								continue;

							int i = GetIndex(input, l1, l2);
							int j = GetIndex(input, l3, l4);

							string filename = string.Format(
								"{0}.{1}{2}{3}{4}.T", name, l1, l2, l3, l4);

							using (StreamWriter w = new StreamWriter(filename))
							{
								for (int wi = 0; wi < input.FrequencyMesh.Length; wi++)
								{
									w.WriteLine("# Frequency: {0}", input.FrequencyMesh[wi]);

									for (int qi = 0; qi < input.QptMesh.Length; qi++)
									{
										w.WriteLine("#{0}\tTemp\tRe(Chi)\tIm(Chi)", input.QptMesh[qi]);

										for (int ti = 0; ti < input.TemperatureMesh.Length; ti++)
										{
											Complex val = chi[qi, wi, ti][i, j];

											w.WriteLine("\t{0:0.000000}\t{1:0.000000}\t{2:0.000000}", 
												input.TemperatureMesh[ti],
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

		private void CalcSpinChargeMatrices(InputFile input, out Matrix S, out Matrix C)
		{
			int size = input.OrbitalCount * input.OrbitalCount;

			S = new Matrix(size, size);
			C = new Matrix(size, size);

			for (int l1 = 0; l1 < input.OrbitalCount; l1++)
			{
				for (int l2 = 0; l2 < input.OrbitalCount; l2++)
				{
					for (int l3 = 0; l3 < input.OrbitalCount; l3++)
					{
						for (int l4 = 0; l4 < input.OrbitalCount; l4++)
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

		Matrix CalcX0(InputFile input, double freq)
		{
			int size = input.OrbitalCount * input.OrbitalCount;
			Matrix x = new Matrix(size, size);

			double en_min = input.EnergyWindowMin;
			double en_max = input.EnergyWindowMax;
			Complex denom_factor = new Complex(0, input.KSmear);

			for (int l1 = 0; l1 < input.OrbitalCount; l1++)
			{
				for (int l4 = 0; l4 < input.OrbitalCount; l4++)
				{
					for (int l3 = l1; l3 < input.OrbitalCount; l3++)
					{
						for (int l2 = l4; l2 < input.OrbitalCount; l2++)
						{
							int i = GetIndex(input, l1, l2);
							int j = GetIndex(input, l3, l4);

							Complex val = 0;

							for (int kindex = 0; kindex < input.KptMesh.Length; kindex++)
							{
								for (int n1 = 0; n1 < input.BandCount; n1++)
								{
									Wavefunction wfk = input.Bands(kindex, n1);
									double e1 = wfk.Energy;
									double f1 = wfk.FermiFunction;

									if (e1 < en_min) continue;
									if (e1 > en_max) break;

									for (int n2 = 0; n2 < input.BandCount; n2++)
									{
										Wavefunction wfq = input.BandsKQ(kindex, n2);
										double e2 = wfq.Energy;
										double f2 = wfq.FermiFunction;

										if (e2 < en_min) continue;
										if (e2 > en_max) break;

										if (f1 == f2) continue;

										Complex coeff =
											wfq.Coeffs[l1] * wfq.Coeffs[l3].Conjugate() *
											wfk.Coeffs[l4] * wfk.Coeffs[l2].Conjugate();

										if (coeff == 0) continue;

										Complex denom = (e2 - e1 - freq - denom_factor);
										Complex lindhard = (f1 - f2) / denom;
										Complex contrib = coeff * lindhard;

										val += contrib;
									}
								}
							}

							val /= input.KptMesh.Length;

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
			
	
		int GetIndex(InputFile input, int l1, int l2)
		{
			// if this changes, be sure to correct the way 
			// x[i,j] and x[j,i] are set in CalcX0.
			return l1 * input.OrbitalCount + l2;
		}
		
	}
}