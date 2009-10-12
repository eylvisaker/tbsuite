using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ERY.EMath;

namespace TightBinding
{
	class TightBinding
	{
		string outputfile;

		public void RunTB(string filename, string outputPrefix)
		{
			this.outputfile = outputPrefix;
			
			TbInputFile r = new TbInputFile(filename);
			r.ReadFile();

			Output.WriteLine("Successfully parsed input file.");

			CalcValues(r);

			bool ranRPA = false;

			if (r.QPlane != null && r.QPlane.Kpts.Count > 0)
			{
				RunRPA(r, r.QPlane);
				ranRPA = true;
			}

			if (!ranRPA)
				Output.WriteLine("No q-points defined, so we will not run the RPA.");

		}

		private void RunRPA(TbInputFile r , KptList qptList)
		{
			RPA rpa = new RPA();

			rpa.RunRpa(this, r, qptList);
		}
		
		public void CalcValues(TbInputFile inp)
		{
			CalcNelec(inp);
			DoBandStructure(inp);
			DoDensityOfStates(inp);
		}


		double FermiFunction(double omega, double mu, double beta)
		{
			return 1.0 / (Math.Exp(beta * (omega - mu)) + 1);
		}

		private void CalcNelec(TbInputFile inp)
		{
			KptList ks = inp.KMesh;
			Matrix[] eigenvals = new Matrix[ks.Kpts.Count];

			for (int i = 0; i < ks.Kpts.Count; i++)
			{
				Matrix m = CalcHamiltonian(inp, ks.Kpts[i]);
				Matrix vals, vecs;
				m.EigenValsVecs(out vals, out vecs);

				eigenvals[i] = vals;
			}

			double beta = 1 / inp.TemperatureMesh[0];

			double N = FindNelec(ks, eigenvals, inp.MuMesh[0], beta);

			if (inp.Nelec != 0)
			{
				inp.MuMesh[0] = FindMu(ks, eigenvals, inp.Nelec, beta);
				Output.WriteLine("Found chemical potential of {0}", inp.MuMesh[0]);
			}
			else
			{
				N = FindNelec(ks, eigenvals, inp.MuMesh[0], beta);

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
		void DoDensityOfStates(TbInputFile inp)
		{
			KptList ks = inp.KMesh;
			using (StreamWriter outf = new StreamWriter(outputfile + ".dos"))
			{

				double smearing = inp.TemperatureMesh[0];
				double effBeta = 1 / smearing;
				
				double emin = double.MaxValue, emax = double.MinValue;

				for (int i = 0; i < ks.Kpts.Count; i++)
				{
					Matrix m = CalcHamiltonian(inp, ks.Kpts[i]);
					Matrix vals, vecs;
					m.EigenValsVecs(out vals, out vecs);

					//ks.Kpts[i].SetStates(vals, vecs);

					for (int j = 0; j < vals.Rows; j++)
					{
						double ev = vals[j, 0].RealPart;

						if (emin > ev) emin = ev;
						if (emax < ev) emax = ev;
					}
				}

				emin -= smearing * 50;
				emax += smearing * 50;

				int epts = 3000;

				double[] energyGrid = new double[epts];
				double[,] dos = new double[epts, inp.Sites.Count + 1];
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
					Matrix m = CalcHamiltonian(inp, ks.Kpts[i]);
					Matrix vals, vecs;
					m.EigenValsVecs(out vals, out vecs);

					for (int j = 0; j < vals.Rows; j++)
					{
						double energy = vals[j, 0].RealPart - inp.MuMesh[0];

						int startIndex = FindIndex(energyGrid, energy - smearing * 10);
						int endIndex = FindIndex(energyGrid, energy + smearing * 10);

						for (int k = startIndex; k <= endIndex; k++)
						{
							double ferm = FermiFunction(energyGrid[k], energy, effBeta);
							double smearWeight = ferm * (1 - ferm) * effBeta;
							smearWeight *= ks.Kpts[i].Weight;

							double weight = 0;
							for (int l = 0; l < vecs.Rows; l++)
							{
								if (inp.PoleStates.Contains(l))
									continue;

								double stateval = vecs[l, j].MagnitudeSquared;
								weight += stateval;
							}
							if (inp.PoleStates.Count == 0 && Math.Abs(weight - 1) > 1e-8)
								throw new Exception("Eigenvector not normalized!");

							dos[k, 0] += smearWeight * weight;

							for (int state = 0; state < vecs.Rows; state++)
							{
								if (inp.PoleStates.Contains(state))
									continue;

								double wtk = vecs[state, j].MagnitudeSquared;//GetWeight(ks.Kpts[i], vecs, j, l);

								dos[k, state + 1] += smearWeight * wtk;
							}
						}

					}
				}

				// symmetrize DOS for equivalent orbitals.
				for (int k = 0; k < epts; k++)
				{
					for (int i = 0; i < inp.Sites.Count; i++)
					{
						double wtk = dos[k, i + 1];
						int count = 1;

						foreach (int equiv in inp.Sites[i].Equivalent)
						{
							wtk += dos[k, equiv + 1];
							count++;
						}

						dos[k, i + 1] = wtk / count;

						foreach (int equiv in inp.Sites[i].Equivalent)
						{
							dos[k, equiv + 1] = wtk / count;
						}
					}
				}

				double slope = (dos[zeroIndex, 0] - dos[zeroIndex + 1, 0]) / (energyGrid[zeroIndex] - energyGrid[zeroIndex + 1]);
				double dosEF = slope * (-energyGrid[zeroIndex]) + dos[zeroIndex, 0];

				Output.WriteLine("Density of states at chemical potential: {0}", dosEF);

				for (int i = 0; i < epts; i++)
				{
					outf.Write("{0}     ", energyGrid[i]);

					for (int j = 0; j < inp.Sites.Count + 1; j++)
					{
						outf.Write("{0}  ", dos[i, j]);
					}

					outf.WriteLine();
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

		void DoBandStructure(TbInputFile inp)
		{
			KptList kpath = inp.KPath;

			Output.WriteLine("Computing band structure with {0} k-points.",
							  kpath.Kpts.Count);

			List<Matrix> eigenvals = new List<Matrix>();

			for (int i = 0; i < kpath.Kpts.Count; i++)
			{
				Matrix m = CalcHamiltonian(inp, kpath.Kpts[i]);
				Matrix vals, vecs;
				m.EigenValsVecs(out vals, out vecs);
				eigenvals.Add(vals);

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
				writer.WriteGraceDottedSetStyle(0);
				writer.WriteGraceSetLineColor(0);
				writer.WriteGraceSetLineColor(1, colors);
				writer.WriteGraceBaseline(kpath.Kpts.Count);

				for (int i = 0; i < datasets; i++)
				{
					writer.WriteGraceDataset(kpath.Kpts.Count,
						x => eigenvals[x][i, 0].RealPart - inp.MuMesh[0]);
				}
			}
		}

		public void WriteEigvals(StreamWriter w, int index, Matrix eigenvals)
		{
			w.Write(index);
			w.Write("    ");
				
			for (int i = 0 ;i < eigenvals.Rows; i++)
			{
				w.Write(eigenvals[i,0].RealPart);	
				w.Write("  ");
			}
			
			w.WriteLine();
		}
		
		public Matrix CalcHamiltonian(TbInputFile inp, Vector3 kpt)
		{
			Matrix m = new Matrix(inp.Sites.Count, inp.Sites.Count);

			kpt *= 2 * Math.PI;

			for (int i = 0; i < inp.Sites.Count; i++)
			{
				for (int j = 0; j < inp.Sites.Count; j++)
				{
					HoppingPair p = inp.Hoppings.FindOrThrow(i, j);
					
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

		/// <summary>
		/// This function does not work with symmetries, so it is unused.
		/// </summary>
		/// <param name="inp"></param>
		/// <param name="outputfile"></param>
		void tet_DoDensityOfStates(TbInputFile inp)
		{
			KptList ks = inp.KMesh;
			StreamWriter outf = new StreamWriter(outputfile + ".dos");

			double smearing = inp.TemperatureMesh[0];
			double smearNorm = 1 / smearing * Math.Pow(Math.PI, -0.5);
			double oneOverSmearSquared = Math.Pow(smearing, -2);

			double emin, emax;
			inp.Hoppings.EnergyScale(out emin, out emax);

			emin -= smearing * 5;
			emax += smearing * 5;


			int epts = 2000;

			double[] energyGrid = new double[epts];
			double[,] dos = new double[epts, inp.Sites.Count + 1];

			smearNorm /= ks.Kpts.Count;

			for (int i = 0; i < epts; i++)
			{
				energyGrid[i] = emin + (emax - emin) * i / (double)(epts - 1);
			}

			Output.WriteLine("Calculating DOS from {0} to {1} with tetrahedron method.",
							  emin, emax, smearing);

			Output.WriteLine("Using {0} tetrahedrons.", ks.Tetrahedrons.Count);

			for (int tetindex = 0; tetindex < ks.Tetrahedrons.Count; tetindex++)
			{
				Tetrahedron tet = ks.Tetrahedrons[tetindex];
				if (tetindex % (ks.Tetrahedrons.Count / 10) == 0 && tetindex > 0)
					Output.WriteLine("At {0}...", tetindex);

				Matrix[] eigenvals = new Matrix[4];

				for (int i = 0; i < 4; i++)
				{
					Matrix m = CalcHamiltonian(inp, tet.Corners[i]);
					Matrix vals, vecs;
					m.EigenValsVecs(out vals, out vecs);

					eigenvals[i] = vals;
				}

				for (int nband = 0; nband < eigenvals[0].Rows; nband++)
				{
					for (int i = 0; i < 4; i++)
					{
						tet.Values[i] = eigenvals[i][nband, 0].RealPart;
					}

					tet.SortCorners();

					int estart = FindIndex(energyGrid, tet.Values[0]);
					int eend = FindIndex(energyGrid, tet.Values[3]);

					for (int ei = estart; ei < eend; ei++)
					{
						dos[ei, 0] += tet.IntegrateArea(energyGrid[ei]);
					}
				}
			}

			for (int i = 0; i < epts; i++)
			{
				dos[i, 0] /= ks.Tetrahedrons.Count;
			}

			for (int i = 0; i < epts; i++)
			{
				outf.Write("{0}     ", energyGrid[i]);

				for (int j = 0; j < inp.Sites.Count + 1; j++)
				{
					outf.Write("{0}  ", dos[i, j]);
				}

				outf.WriteLine();
			}

			outf.Close();

			Output.WriteLine("Creating +coeff file.");
			outf = new StreamWriter(Path.Combine(Path.GetDirectoryName(outputfile), "+coeff"));

			outf.WriteLine("#\t1\t0\t" + ks.Kpts.Count.ToString());
			outf.Write("# band index\te(k,n)\t");

			for (int i = 0; i < inp.Sites.Count; i++)
			{
				if (string.IsNullOrEmpty(inp.Sites[i].Name))
				{
					outf.Write("TB{0}\t", i);
				}
				else
					outf.Write("{0}\t", inp.Sites[i].Name);
			}
			outf.WriteLine();

			for (int kindex = 0; kindex < ks.Kpts.Count; kindex++)
			{
				Matrix m = CalcHamiltonian(inp, ks.Kpts[kindex]);
				Matrix vals, vecs;
				m.EigenValsVecs(out vals, out vecs);

				outf.WriteLine("# spin=    1 k={0}", ks.Kpts[kindex].Value);

				for (int i = 0; i < vals.Rows; i++)
				{
					outf.Write("{0}     {1}    ", i + 1, vals[i, 0].RealPart);

					for (int j = 0; j < vecs.Columns; j++)
					{
						outf.Write("{0}    {1}    ", vecs[i, j].RealPart, vecs[i, j].ImagPart);
					}
					outf.WriteLine();
				}
			}

		}

	}
}