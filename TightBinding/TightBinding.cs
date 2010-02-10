using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace TightBindingSuite
{
	public partial class TightBinding
	{
		Lattice lattice;
		OrbitalList orbitals;
		HoppingPairList hoppings;
		KptList kpath, mAllKmesh;
		KptList mKmesh;
		KptPlane mAllQplane;
		KptPlane mQplane;
		SpaceGroup mSpaceGroup;
		List<int> poles = new List<int>();

		public Lattice Lattice { get { return lattice; } }
		public OrbitalList Orbitals { get { return orbitals; } }
		public HoppingPairList Hoppings { get { return hoppings; } }
		public KptList KPath { get { return kpath; } }
		public KptList AllKMesh { get { return mAllKmesh; } }
		public KptList KMesh { get { return mKmesh; } }
		public KptPlane AllQPlane { get { return mAllQplane; } }
		public KptPlane QPlane { get { return mQplane; } }
		public bool SkipQPlaneLines { get; set; }
		public double[] FrequencyMesh { get; private set; }
		public double[] TemperatureMesh { get; private set; }
		public double[] MuMesh { get; private set; }
		public List<int> PoleStates { get { return poles; } }
		public SpaceGroup SpaceGroup { get { return mSpaceGroup; } private set { mSpaceGroup = value; } }
		public double[] Nelec { get; private set; }

		public InteractionList Interactions { get; private set; }

		int[] kgrid = new int[3];
		int[] shift = new int[] { 1, 1, 1 };

		int[] qgrid = new int[3];
		Vector3[] qplaneDef = new Vector3[3];
		bool setQplane = false;
		bool specifiedNelec = false;
		bool disableSymmetries = false;

		string outputfile;


		public TightBinding Clone()
		{
			TightBinding retval = new TightBinding();

			retval.lattice = lattice.Clone();
			retval.orbitals = orbitals.Clone();
			retval.hoppings = hoppings.Clone();
			retval.kpath = kpath.Clone();
			retval.mAllKmesh = mAllKmesh.Clone();
			retval.mKmesh = mKmesh.Clone();
			retval.mAllQplane = mAllQplane.Clone();
			retval.mQplane = mQplane.Clone();
			retval.poles.AddRange(poles);
			retval.FrequencyMesh = (double[])FrequencyMesh.Clone();
			retval.TemperatureMesh = (double[])TemperatureMesh.Clone();
			retval.MuMesh = (double[])MuMesh.Clone();
			retval.Nelec = (double[])Nelec.Clone();

			retval.Interactions = Interactions.Clone();
			retval.kgrid = (int[])kgrid.Clone();
			retval.shift = (int[])shift.Clone();
			retval.SpaceGroup = SpaceGroup.Clone();
			retval.qgrid = (int[])qgrid.Clone();
			retval.qplaneDef = (Vector3[])qplaneDef.Clone();
			retval.setQplane = setQplane;
			retval.specifiedNelec = specifiedNelec;

			retval.outputfile = outputfile;

			return retval;
		}

		public TightBinding() { }
		public TightBinding(string filename)
		{
			LoadTB(filename);
		}
		public void LoadTB(string filename)
		{
			string outputPrefix = Path.GetFileNameWithoutExtension(filename);
			this.outputfile = outputPrefix;

			TightBinding.TbInputFileReader r = new TightBinding.TbInputFileReader(filename, this);
			r.ReadFile();

			Output.WriteLine("Successfully parsed input file.");

			CalcWavefunctions();
			CalcNelec();
		}

		public void RunTB()
		{
			DoBandStructure();
			DoDensityOfStates();
		}


		private void CalcWavefunctions()
		{
			for (int i = 0; i < KMesh.Kpts.Count; i++)
			{
				Matrix m = CalcHamiltonian(KMesh.Kpts[i]);
				Matrix vals, vecs;
				m.EigenValsVecs(out vals, out vecs);

				KMesh.Kpts[i].SetWavefunctions(vals, vecs);
			}
		}
		double FermiFunction(double omega, double mu, double beta)
		{
			return 1.0 / (Math.Exp(beta * (omega - mu)) + 1);
		}

		private void CalcNelec()
		{
			KptList ks = KMesh;
			Matrix[] eigenvals = new Matrix[ks.Kpts.Count];

			for (int i = 0; i < KMesh.Kpts.Count; i++)
			{
				Matrix vals = new Matrix(Orbitals.Count, Orbitals.Count);

				for (int j = 0; j < Orbitals.Count; j++)
				{
					vals[j, 0] = ks.Kpts[i].Wavefunctions[j].Energy;
				}

				eigenvals[i] = vals;
			}

			double beta = 1 / TemperatureMesh[0];

			double N = FindNelec(ks, eigenvals, MuMesh[0], beta);

			if (specifiedNelec)
			{
				MuMesh = new double[Nelec.Length];

				for (int i = 0; i < MuMesh.Length; i++)
				{
					MuMesh[i] = FindMu(ks, eigenvals, Nelec[i], beta);
				}
			}
			else
			{
				Nelec = new double[MuMesh.Length];

				for (int i = 0; i < MuMesh.Length; i++)
				{
					Nelec[i] = FindNelec(ks, eigenvals, MuMesh[i], beta);
				}
			}

			Output.WriteLine("           mu         Nelec");

			for (int i = 0; i < MuMesh.Length; i++)
			{
				MuMesh[i] = FindMu(ks, eigenvals, Nelec[i], beta);

				Output.WriteLine("     {0:0.000000}      {1:0.000000}", MuMesh[i], Nelec[i]);
			}
		}

		private double FindMu(KptList ks, Matrix[] eigenvals, double Ntarget, double beta)
		{
			double N;
			double mu_lower, mu_upper;
			double N_lower, N_upper;
			double mu;

			mu_lower = mu_upper = mu = 0;
			
			// first bracket
			N = FindNelec(ks, eigenvals, mu, beta);

			if (N > Ntarget)
			{
				mu_upper = mu;

				while (mu_lower >= mu_upper)
				{
					mu -= 1;
					N = FindNelec(ks, eigenvals, mu, beta);

					if (N > Ntarget)
						mu_upper = mu;
					else
						mu_lower = mu;
				}
			}
			else
			{
				mu_lower = mu;

				while (mu_lower >= mu_upper)
				{
					mu += 1;
					N = FindNelec(ks, eigenvals, mu, beta);

					if (N > Ntarget)
						mu_upper = mu;
					else
						mu_lower = mu;
				}
			}

			mu = 0.5 * (mu_upper + mu_lower);

			N_lower = FindNelec(ks, eigenvals, mu_lower, beta);
			N_upper = FindNelec(ks, eigenvals, mu_upper, beta);

			// do linear extrapolation
			int iter = 0;
			while (Math.Abs(N - Ntarget) > 1e-11 && iter < 300)
			{
				double slope = (N_upper - N_lower) / (mu_upper - mu_lower);

				if ((iter / 3) % 5 < 2)
				{
					// bisection in case system is gapped at target number
					mu = 0.5 * (mu_upper + mu_lower);
				}
				else
				{
					// linear extrapoliation
					mu = (Ntarget - N_lower) / slope + mu_lower;
				}

				N = FindNelec(ks, eigenvals, mu, beta);

				if (N < Ntarget)
				{
					mu_lower = mu;
					N_lower = N;
				}
				else if (N > Ntarget)
				{
					mu_upper = mu;
					N_upper = N;
				}

				iter++;
			}

			if (iter >= 300)
			{
				Output.WriteLine("Failed to find chemical potential.  Check the number of electrons.");
				throw new Exception("Failed to find chemical potential.");
			}

			return mu;
		}
		private double FindNelec(KptList ks, Matrix[] eigenvals, double mu, double beta)
		{
			double N = 0;

			for (int i = 0; i < ks.Kpts.Count; i++)
			{
				double weight = ks.Kpts[i].Weight;

				for (int j = 0; j < eigenvals[i].Rows; j++)
				{
					double energy = eigenvals[i][j, 0].RealPart;
					double npt = 2 * FermiFunction(energy, mu, beta);

					N += npt * weight;
				}
			}
			return N;
		}

		void DoDensityOfStates()
		{
			KptList ks = KMesh;
			using (StreamWriter outf = new StreamWriter(outputfile + ".dos"))
			{
				outf.WriteLine("# Density of states");
				outf.WriteLine("# From {0} k-points.", ks.Kpts.Count);

				double smearing = TemperatureMesh[0];
				double effBeta = 1 / smearing;
				
				double emin = double.MaxValue, emax = double.MinValue;

				for (int i = 0; i < ks.Kpts.Count; i++)
				{
					KPoint kpt = ks.Kpts[i];

					emin = Math.Min(emin, kpt.Wavefunctions.Min(x => x.Energy));
					emax = Math.Max(emax, kpt.Wavefunctions.Max(x => x.Energy));
				}

				emin -= smearing * 10;
				emax += smearing * 10;

				emin -= MuMesh[0];
				emax -= MuMesh[0];

				int epts = 3000;

				double[] energyGrid = new double[epts];
				double[,] dos = new double[epts, Orbitals.Count + 1];
				int zeroIndex = 0;

				Output.WriteLine(
					"Calculating DOS from {0} to {1} with finite temperature smearing {2}.",
					emin, emax, smearing);

				Output.WriteLine("Using {0} kpts.", ks.Kpts.Count);

				for (int i = 0; i < epts; i++)
				{
					energyGrid[i] = emin + (emax - emin) * i / (double)(epts - 1);

					if (energyGrid[i] < 0)
						zeroIndex = i;
				}
				
				for (int i = 0; i < ks.Kpts.Count; i++)
				{
					KPoint kpt = ks.Kpts[i];

					for (int j = 0; j < kpt.Wavefunctions.Count; j++)
					{
						Wavefunction wfk = kpt.Wavefunctions[j];

						double energy = wfk.Energy - MuMesh[0];

						int startIndex = FindIndex(energyGrid, energy - smearing * 10);
						int endIndex = FindIndex(energyGrid, energy + smearing * 10);

						for (int k = startIndex; k <= endIndex; k++)
						{
							double ferm = FermiFunction(energyGrid[k], energy, effBeta);
							double smearWeight = ferm * (1 - ferm) * effBeta;
							smearWeight *= ks.Kpts[i].Weight;

							double weight = 0;
							for (int l = 0; l < wfk.Coeffs.Length; l++)
							{
								if (PoleStates.Contains(l))
									continue;

								double stateval = wfk.Coeffs[l].MagnitudeSquared;
								weight += stateval;
							}

							if (PoleStates.Count == 0 && Math.Abs(weight - 1) > 1e-8)
								throw new Exception("Eigenvector not normalized!");

							dos[k, 0] += smearWeight * weight;
							
							for (int state = 0; state < wfk.Coeffs.Length ; state++)
							{
								if (PoleStates.Contains(state))
									continue;

								double wtk = wfk.Coeffs[state].MagnitudeSquared;

								dos[k, state + 1] += smearWeight * wtk;
							}
						}

					}
				}

				// symmetrize DOS for equivalent orbitals.
				for (int k = 0; k < epts; k++)
				{
					for (int i = 0; i < Orbitals.Count; i++)
					{
						double wtk = dos[k, i + 1];
						int count = 1;

						foreach (int equiv in Orbitals[i].Equivalent)
						{
							wtk += dos[k, equiv + 1];
							count++;
						}

						dos[k, i + 1] = wtk / count;

						foreach (int equiv in Orbitals[i].Equivalent)
						{
							dos[k, equiv + 1] = wtk / count;
						}
					}
				}

				if (emin < 0 && emax > 0)
				{
					if (zeroIndex + 1 >= energyGrid.Length)
						zeroIndex = energyGrid.Length - 2;

					double slope = (dos[zeroIndex, 0] - dos[zeroIndex + 1, 0]) / (energyGrid[zeroIndex] - energyGrid[zeroIndex + 1]);
					double dosEF = slope * (-energyGrid[zeroIndex]) + dos[zeroIndex, 0];

					Output.WriteLine("Density of states at chemical potential: {0}", dosEF);

					for (int i = 0; i < epts; i++)
					{
						outf.Write("{0}     ", energyGrid[i]);

						for (int j = 0; j < Orbitals.Count + 1; j++)
						{
							outf.Write("{0}  ", dos[i, j]);
						}

						outf.WriteLine();
					}
				}

			}
		}

		private static double GetWeight(KPoint kpt, Matrix vecs, int j, int state)
		{
			int count = 1;
			double wtk = vecs[state, j].MagnitudeSquared;

			foreach (int orb in kpt.GetEquivalentOrbitals(state))
			{
				wtk += vecs[orb, j].MagnitudeSquared;
				count++;
			}

			return wtk / count;
		}
		int FindIndex(double[] grid, double value)
		{
			double min = grid[0];
			double max = grid[1];
			
			int trial = (int)((value - min) / (max - min));
			
			if (trial < 0) return 0;
			if (trial >= grid.Length) return grid.Length - 1;
			
			return trial;
		}

		void DoBandStructure()
		{
			KptList kpath = KPath;

			Output.WriteLine("Computing band structure with {0} k-points.",
							  kpath.Kpts.Count);

			List<Matrix> eigenvals = new List<Matrix>();
			List<Matrix> eigenvecs = new List<Matrix>();

			for (int i = 0; i < kpath.Kpts.Count; i++)
			{
				Matrix m = CalcHamiltonian(kpath.Kpts[i]);
				Matrix vals, vecs;
				m.EigenValsVecs(out vals, out vecs);
				eigenvals.Add(vals);
				eigenvecs.Add(vecs);

				for (int j = 0; j < vals.Rows; j++)
				{
					if (double.IsNaN(vals[j,0].RealPart))
						throw new Exception("NaN found while diagonalizing tight binding at kpt " + i.ToString()+ ".");
				}
			}
			int datasets = eigenvals[0].Rows;

			using (AgrWriter writer = new AgrWriter(outputfile + ".band.agr"))
			{
				int[] colors = new int[datasets];
				for(int i = 0; i < colors.Length; i++)
					colors[i] = 1;

				writer.WriteGraceHeader(kpath);
				writer.WriteGraceSetLineStyle(0, 2);
				writer.WriteGraceSetLineColor(0);
				writer.WriteGraceSetLineColor(1, colors);
				writer.WriteGraceBaseline(kpath.Kpts.Count);

				for (int i = 0; i < datasets; i++)
				{
					writer.WriteGraceDataset(kpath.Kpts.Count,
						x => new Pair<double,double>(x, eigenvals[x][i, 0].RealPart - MuMesh[0]));
				}
			}
			// Do fat bands plot
			using (AgrWriter writer = new AgrWriter(outputfile + ".bweights.agr"))
			{
				// set all band lines to black
				int[] colors = new int[datasets];
				for (int i = 0; i < colors.Length; i++)
					colors[i] = 1;

				writer.WriteGraceHeader(kpath);
				writer.WriteGraceSetLineStyle(0, 2);
				
				writer.WriteGraceSetLineColor(0);
				writer.WriteGraceSetLineColor(1, colors);

				int set = datasets + 1;

				for (int j = 0; j < Orbitals.Count; j++)
				{
					int color = j + 1;
					if (color > 15)
						color -= 15;

					writer.WriteGraceSetLineColor(set, color);
					writer.WriteGraceSetSymbol(set, 1);
					writer.WriteGraceSetSymbolColor(set, color);
					writer.WriteGraceSetSymbolFill(set, 1);

					set++;
				}

				set = datasets + 1;
				for (int j = 0; j < Orbitals.Count; j++)
				{
					writer.WriteGraceLegend(set, Orbitals[j].Name);
					set += 1;
				}

				writer.WriteGraceBaseline(kpath.Kpts.Count);

				for (int i = 0; i < datasets * Orbitals.Count; i++)
				{
					writer.WriteGraceSetLineStyle(i + datasets + 1, 0);
				}

				for (int i = 0; i < datasets; i++)
				{
					writer.WriteGraceDataset(kpath.Kpts.Count,
						x => new Pair<double, double>(x, eigenvals[x][i, 0].RealPart - MuMesh[0]));
				}

				for (int j = 0; j < Orbitals.Count; j++)
				{
					writer.WriteGraceDataset("xysize", kpath.Kpts.Count * datasets,
						x =>
						{
							int k = x % kpath.Kpts.Count;
							int i = x / kpath.Kpts.Count;
							double mag = eigenvecs[k][j, i].MagnitudeSquared;
							if (mag < 0.0001)
								return null;

							return new Triplet<double,double, double>(
								k,
								eigenvals[k][i, 0].RealPart - MuMesh[0],
								mag);
						});
				}
			}
		}

		public void WriteEigvals(StreamWriter w, int index, Matrix eigenvals)
		{
			w.Write(index);
			w.Write("    ");
				
			for (int i = 0 ; i < eigenvals.Rows; i++)
			{
				w.Write(eigenvals[i,0].RealPart);	
				w.Write("  ");
			}
			
			w.WriteLine();
		}
		
		public Matrix CalcHamiltonian(Vector3 kpt)
		{
			Matrix m = new Matrix(Orbitals.Count, Orbitals.Count);

			kpt *= 2 * Math.PI;

			for (int i = 0; i < Orbitals.Count; i++)
			{
				for (int j = 0; j < Orbitals.Count; j++)
				{
					HoppingPair p = Hoppings.FindOrThrow(i, j);
					
					Complex val = new Complex();
					 
					for (int k = 0; k < p.Hoppings.Count; k++)
					{
						HoppingValue hop = p.Hoppings[k];
						Vector3 R = hop.R;
						
						Complex newval = hop.Value * 
							Complex.Exp(new Complex(0, kpt.DotProduct(R)));

						val += newval;
					}
					
					m[i,j] = val;
				}
			}
			
			if (m.IsHermitian == false)
				throw new Exception("Hamiltonian at k = " + kpt.ToString() + " is not Hermitian.");
			
			return m;
		}


	}
}