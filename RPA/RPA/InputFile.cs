
using System;
using System.Collections.Generic;
using System.Linq;
using ERY.EMath;

namespace RPA
{
	public class InputFile
	{
		int[] mesh = new int[] { 10, 10, 10 };
		int[] qmesh = new int[] { 2, 2, 2 };
		double[] freq = new double[2] { 0, 0 };
		double[] temperature = new double[2] { 0.026, 0.026 };

		int temperatureSteps = 0;
		int freqSteps = 0;
		double[] energyWindow = new double[2] { -7, 7 };
		double[] frequencyMesh;
		Vector3[] reciprocalVecs = new Vector3[3];
		Vector3[] directVecs = new Vector3[3];
		double kptscale;
		Vector3[] kptMesh;
		Vector3[] qptMesh;
		int[] orbitals = new int[1];
		string[] orbitalNames;
		Wavefunction[][] bands;
		Wavefunction[][] kqbands;
		int bandCount;
		double[] U = new double[] { 4, 4, 1, 1 };
		double beta;
		double currentTemperature;
		double[] temperatureMesh;

		public InputFile()
		{
		}

		public int[] MeshSize { get { return mesh; } }
		public int OrbitalCount { get { return orbitals.Length; } }
		public int BandCount { get { return bandCount; } }
		public double EnergyWindowMin { get { return energyWindow[0]; } }
		public double EnergyWindowMax { get { return energyWindow[1]; } }
		public double HubU { get { return U[0]; } }
		public double HubUp { get { return U[1]; } }
		public double HubJ { get { return U[2]; } }
		public double HubJp { get { return U[3]; } }
		public double Temperature { get { return currentTemperature; } }
		public double Beta { get { return beta; } }
		public double KSmear { get; private set; }

		public void SetTemperature(double value)
		{
			currentTemperature = value;
			beta = 1 / Temperature;

			foreach (var k in bands)
			{
				foreach (Wavefunction wfk in k)
				{
					wfk.FermiFunction = FermiFunction(wfk.Energy);
				}
			}
		}
		public double FermiFunction(double energy)
		{
			return 1.0 / (Math.Exp(beta * energy) + 1);
		}

		internal void CreateKQbands(Vector3 q)
		{
			for (int i = 0; i < kptMesh.Length; i++)
			{
				Vector3 kq = KptMesh[i] + q;

				int index = GetKindex(kq);

				kqbands[index] = bands[i];
			}
		}
		public int GetKindex(Vector3 kpt)
		{
			for (int i = 0; i < 3; i++)
				kpt[i] = Math.Round(kpt[i], 9) % 1;

			for (int i = 0; i < kptMesh.Length; i++)
			{
				double dist = GetDist(kptMesh[i], kpt);

				if (dist < 1e-5)
					return i;
			}

			throw new Exception(string.Format("Could not find k-point {0}", kpt));
		}

		private double GetDist(Vector3 a, Vector3 b)
		{
			return (a - b).MagnitudeSquared;
		}
		
		public Wavefunction Bands(int kpt, int band)
		{
			return bands[kpt][band];
		}
		public Wavefunction BandsKQ(int kqpt, int band)
		{
			return kqbands[kqpt][band];
		}

		public void ReadWavefunctions(System.IO.StreamReader r)
		{
			int index = 0;
			char[] splitChars = new char[] { ' ', '\t' };

			// ignore first line
			r.ReadLine();

			// read basis comment
			string labels = r.ReadLine();
			if (labels.StartsWith("# band index         e(k,n)") == false)
				throw new Exception("Could not understand +coeff file.  Error reading line 2.");

			string[] tempSplit = labels.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
			string[] totalBasis = new string[tempSplit.Length - 4];
			for (int i = 0; i < totalBasis.Length; i++)
				totalBasis[i] = tempSplit[i + 4];

			bandCount = totalBasis.Length;

			orbitalNames = new string[orbitals.Length];

			for (int i = 0; i < orbitals.Length; i++)
			{
				if (orbitals[i] >= totalBasis.Length)
				{
					Console.WriteLine("Specified orbital {0} was not in the basis." + orbitals[i]);
					Environment.Exit(13);
				}

				orbitalNames[i] = totalBasis[orbitals[i]];
			}

			bands = new Wavefunction[KptMesh.Length][];

			for (int i = 0; i < bands.Length; i++)
				bands[i] = new Wavefunction[totalBasis.Length];

			// next line is comment for first k-point, so skip it.
			r.ReadLine();

			while (r.EndOfStream == false)
			{
				string line = r.ReadLine();

				if (line.StartsWith("#"))
				{
					// every k point is separated by a comment line.
					index++;
					continue;
				}

				// skip lines if we have used up allocated memory, but
				// keep reading the file to make sure it has the right amount
				// of data.
				if (index >= bands.Length)
					continue;

				string[] vals = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);

				// first value is the band index.
				// second value is the eigenvalue.
				int bandIndex = int.Parse(vals[0]) - 1;
				Wavefunction wfk = new Wavefunction(orbitals.Length);
				wfk.Energy = double.Parse(vals[1]);
				wfk.FermiFunction = FermiFunction(wfk.Energy);

				for (int i = 0; i < orbitals.Length; i++)
				{
					int basisIndex = 2 + 2 * orbitals[i];
					string coeffstr_real = vals[basisIndex];
					string coeffstr_imag = vals[basisIndex+1];

					wfk.Coeffs[i] = new Complex(double.Parse(coeffstr_real), double.Parse(coeffstr_imag));
				}

				bands[index][bandIndex] = wfk;
			}

			if (index != kptMesh.Length - 1)
			{
				Console.WriteLine("Number of kpoints in +bands file ({0}) does not match mesh value {1}.",
								  index, kptMesh.Length);
				System.Environment.Exit(1);
			}

			Console.WriteLine("Read {0} bands.", bandCount);
			Console.WriteLine();

			Console.Write("Band: ");

			for (int i = 0; i < OrbitalCount; i++)
			{
				Console.Write("   {0}", orbitalNames[i]);
			}
			Console.WriteLine();

			for (int n = 0; n < bandCount; n++)
			{
				Console.Write("{0:000}  ", n);

				for (int i = 0; i < OrbitalCount; i++)
				{
					double amount = 0;
					
					for (int k = 0; k < kptMesh.Length; k++)
					{
						amount += bands[k][n].Coeffs[i].MagnitudeSquared;
					}
					amount /= kptMesh.Length;

					Console.Write("       {0:0.00000}", amount);
				}

				Console.WriteLine();
			}
			Console.WriteLine();

			kqbands = new Wavefunction[kptMesh.Length][];
		}
		public Vector3[] QptMesh
		{
			get
			{
				if (qptMesh != null)
					return qptMesh;

				qptMesh = GenerateMesh(qmesh, 0.5, true);
				return qptMesh;
			}
		}
		public Vector3[] KptMesh
		{
			get
			{
				if (kptMesh != null)
					return kptMesh;

				kptMesh = GenerateMesh(mesh, 1.0, false);
				return kptMesh;

			}
		}

		private Vector3[] GenerateMesh(int[] mesh, double norm, bool includeEnds)
		{
			int index = 0;

			Vector3[] retval;
			
			if (includeEnds)
			{
				retval = new Vector3[(mesh[2]+1) * (mesh[1]+1) * (mesh[0]+1)];
			}
			else
				retval = new Vector3[mesh[2] * mesh[1] * mesh[0]];

			for (int z = 0; z <= mesh[2]; z++)
			{
				if (includeEnds == false && z == mesh[2])
					continue;
				
				double dz = z / (double)mesh[2] * norm;
				if (mesh[2] == 0) 
					dz = 0;

				for (int y = 0; y <= mesh[1]; y++)
				{
					if (includeEnds == false && y == mesh[1])
						continue;
				
					double dy = y / (double)mesh[1] * norm;
					if (mesh[1] == 0)
						dy = 0;

					for (int x = 0; x <= mesh[0]; x++)
					{
						if (includeEnds == false && x == mesh[0])
							continue;
				
						double dx = x / (double)mesh[0] * norm;
						if (mesh[0] == 0)
							dx = 0;

						Vector3 v = new Vector3(dx, dy, dz);

						retval[index] = v;

						index++;
					}
				}
			}

			return retval;
		}
		public Vector3[] DirectVecs
		{
			get { return directVecs; }
		}
		public Vector3[] ReciprocalVecs
		{
			get { return reciprocalVecs; }
		}
		public double KptScale
		{
			get { return kptscale; }
			set { kptscale = value; }
		}
		public double[] TemperatureMesh
		{
			get
			{
				if (temperatureMesh != null)
					return temperatureMesh;

				if (temperature[0] == temperature[1])
				{
					temperatureMesh = new double[] { temperature[0] };
				}
				else
				{
					if (temperatureSteps < 2) throw new Exception("Not enough temperature steps.");

					temperatureMesh = new double[temperatureSteps];
					double delta = temperature[1] - temperature[0];
					delta /= temperatureSteps - 1;

					for (int i = 0; i < temperatureSteps; i++)
					{
						temperatureMesh[i] = i * delta + temperature[0];
					}
				}

				return temperatureMesh;
			}
		}

		public double[] FrequencyMesh
		{
			get
			{
				if (frequencyMesh != null)
					return frequencyMesh;

				if (freq[0] == freq[1])
				{
					frequencyMesh = new double[] { freq[0] };
				}
				else
				{
					if (freqSteps < 2) throw new Exception("Not enough frequency steps.");

					frequencyMesh = new double[freqSteps];
					double delta = freq[1] - freq[0];
					delta /= freqSteps - 1;

					for (int i = 0; i < freqSteps; i++)
					{
						frequencyMesh[i] = i * delta + freq[0];
					}
				}

				return frequencyMesh;
			}
		}
		public void SetVariable(string name, string args)
		{
			Console.Write("{0} ", name);

			switch (name.ToLowerInvariant())
			{
				case "mesh":
					mesh = ReadArray<int>(args, int.Parse, 3);
					break;
				case "orbitals":
					orbitals = ReadArray<int>(args, int.Parse);
					
					// shift from 1 based to zero based.
					for (int i = 0; i < orbitals.Length; i++)
						orbitals[i]--;

					break;
				case "energywindow":
					energyWindow = ReadArray<double>(args, double.Parse, 2);
					break;
				case "ksmear":
					KSmear = double.Parse(args);
					break;
				case "freq":
					try
					{
						freq = ReadArray<double>(args, double.Parse, 2);
					}
					catch (Exception)
					{
						freq[0] = double.Parse(args);
						freq[1] = freq[0];
					}

					break;
				case "freqsteps":
					freqSteps = int.Parse(args);
					break;
				case "u":
					U = ReadArray<double>(args, double.Parse, 4);
					break;
				case "temperature":
					try
					{
						temperature = ReadArray<double>(args, double.Parse, 2);
					}
					catch (Exception)
					{
						temperature[0] = double.Parse(args);
						temperature[1] = temperature[0];
					} break;
				case "temperaturesteps":
					temperatureSteps = int.Parse(args);

					break;
				case "qmesh":
					qmesh = ReadArray<int>(args, int.Parse, 3);
					break;

				default:
					Console.WriteLine("Could not understand " + name + " " + args);
					throw new Exception();
					break;
			}
		}

		static char[] sep = new char[] { ' ', '\t' };

		T[] ReadArray<T>(string val, Func<string, T> converter)
		{
			string[] v = val.Split(sep, StringSplitOptions.RemoveEmptyEntries);
			T[] retval = new T[v.Length];
			
			for (int i = 0; i < retval.Length; i++)
			{
				Console.Write(v[i] + "  ");
				retval[i] = converter(v[i]);
			}
			Console.WriteLine();

			return retval;
		}
		T[] ReadArray<T>(string val, Func<string, T> converter, int length)
		{
			T[] retval = new T[length];
			string[] v = val.Split(sep, StringSplitOptions.RemoveEmptyEntries);

			if (v.Length < length)
				throw new Exception("Not enough inputs.");

			for (int i = 0; i < length; i++)
			{
				Console.Write(v[i] + "  ");
				retval[i] = converter(v[i]);
			}
			Console.WriteLine();

			return retval;
		}


		internal void Validate()
		{
			// check that k and q are commensurate
			for (int i = 0; i < 3; i++)
			{
				if (qmesh[i] == 0) 
					continue;

				double d = mesh[i] / (double)qmesh[i];
				int di = mesh[i] / qmesh[i];

				if (Math.Abs(d - di) > 1e-8)
				{
					Console.WriteLine("ERROR: K-mesh and Q-mesh are not commensurate.");
					Console.WriteLine("Make sure k-mesh is integer multiple of q mesh.");
					Environment.Exit(12);
				}
			}

		}
	}
}
