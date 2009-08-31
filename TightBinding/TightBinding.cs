using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ERY.EMath;

namespace TightBinding
{
	class TightBinding
	{
		public void RunTB(string filename, string outputfile)
		{
			TbInputFile r = new TbInputFile(filename);
			r.ReadFile();
			Console.WriteLine("Successfully parsed input file.");
			
			CalcValues(r, outputfile);

			bool ranRPA = false;

			if (r.QPath != null && r.QPath.Kpts.Count > 0)
			{
				RunRPA(r, r.QPath);
				ranRPA = true;
			}
			if (r.QMesh != null && r.QMesh.Kpts.Count > 0)
			{
				RunRPA(r, r.QMesh);
				ranRPA = true;
			}

			if (!ranRPA)
				Console.WriteLine("No q-points defined, so we will not run the RPA.");
		}

		private void RunRPA(TbInputFile r , KptList qptList)
		{
			RPA rpa = new RPA();

			rpa.RunRpa(this, r, qptList);
		}
		
		public void CalcValues(TbInputFile inp, string outputfile)
		{
			DoBandStructure(inp, outputfile);
			DoDensityOfStates(inp, outputfile);
		}
		void tet_DoDensityOfStates(TbInputFile inp, string outputfile)
		{
			KptList ks = inp.KMesh;
			StreamWriter outf = new StreamWriter(outputfile + ".dos");

			double smearing = inp.Smearing;
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

			Console.WriteLine("Calculating DOS from {0} to {1} with tetrahedron method.",
							  emin, emax, smearing);

			Console.WriteLine("Using {0} tetrahedrons.", ks.Tetrahedrons.Count);

			for (int tetindex = 0; tetindex < ks.Tetrahedrons.Count; tetindex++)
			{
				Tetrahedron tet = ks.Tetrahedrons[tetindex];
				if (tetindex % (ks.Tetrahedrons.Count / 10) == 0 && tetindex > 0)
					Console.WriteLine("At {0}...", tetindex);

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

			Console.WriteLine("Creating +coeff file.");
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
					outf.Write("{0}     {1}    ", i+1, vals[i,0].RealPart);

					for (int j = 0; j < vecs.Columns; j++)
					{
						outf.Write("{0}    {1}    ", vecs[i, j].RealPart, vecs[i, j].ImagPart);
					}
					outf.WriteLine();
				}
			}

		}

		void FermiFunction(double omega, double mu, double beta)
		{
			return 1.0 / Math.Exp(beta * omega + mu);
		}
		void DoDensityOfStates(TbInputFile inp, string outputfile)
		{
			KptList ks = inp.KMesh;
			using (StreamWriter outf = new StreamWriter(outputfile + ".dos"))
			{

				double smearing = inp.Smearing;
				double smearNorm = 1 / smearing * Math.Pow(Math.PI, -0.5);
				double oneOverSmearSquared = Math.Pow(smearing, -2);

				
				double emin, emax;
				inp.Hoppings.EnergyScale(out emin, out emax);

				emin -= smearing * 5;
				emax += smearing * 5;

				int epts = 2000;

				double[] energyGrid = new double[epts];
				double[,] dos = new double[epts, inp.Sites.Count + 1];
				int zeroIndex = 0;

				for (int i = 0; i < epts; i++)
				{
					energyGrid[i] = emin + (emax - emin) * i / (double)(epts - 1);

					if (energyGrid[i] < 0)
						zeroIndex = i;
				}
				Console.WriteLine("Calculating DOS from {0} to {1} with smearing {2}.",
								  emin, emax, smearing);

				Console.WriteLine("Using {0} kpts.", ks.Kpts.Count);

				/*
				if (inp.PoleStates.Count > 0)
					Console.WriteLine("Pole states present: {0}", inp.PoleStates.Count);
				*/


				for (int i = 0; i < ks.Kpts.Count; i++)
				{
					Matrix m = CalcHamiltonian(inp, ks.Kpts[i]);
					Matrix vals, vecs;
					m.EigenValsVecs(out vals, out vecs);

					for (int j = 0; j < vals.Rows; j++)
					{
						double energy = vals[j, 0].RealPart - inp.ChemicalPotential;

						int startIndex = FindIndex(energyGrid, energy - smearing * 10);
						int endIndex = FindIndex(energyGrid, energy + smearing * 10);

						for (int k = startIndex; k <= endIndex; k++)
						{
							double gaus = Math.Exp(
								-Math.Pow(energyGrid[k] - energy, 2) * oneOverSmearSquared);
							gaus *= smearNorm;
							gaus *= ks.Kpts[i].Weight;

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

							dos[k, 0] += gaus * weight;

							for (int state = 0; state < vecs.Rows; state++)
							{
								if (inp.PoleStates.Contains(state))
									continue;

								double wtk = vecs[state, j].MagnitudeSquared;//GetWeight(ks.Kpts[i], vecs, j, l);

								dos[k, state + 1] += gaus * wtk;
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

				Console.WriteLine("Density of states at chemical potential: {0}", dosEF);

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
		
		void DoBandStructure(TbInputFile inp, string outputfile)
		{
			KptList kpath = inp.KPath;
			
			StreamWriter band = new StreamWriter(outputfile + ".band");
			StreamWriter pts = new StreamWriter(outputfile + ".bandpts");
			
			Console.WriteLine("Computing band structure with {0} k-points.",
			                  kpath.Kpts.Count);
			
			try
			{
				for (int i = 0; i < kpath.Kpts.Count; i++)
				{
					Matrix m = CalcHamiltonian(inp, kpath.Kpts[i]);
					Matrix vals, vecs;
					m.EigenValsVecs(out vals, out vecs);
					
					WriteEigvals(band, i, vals);
					
					if (string.IsNullOrEmpty(kpath.Kpts[i].Name) == false)
					{
						pts.WriteLine("{0}   {1}", i, kpath.Kpts[i].Name);
					}
				}
				
			}
			finally
			{
				band.Dispose();	
				pts.Dispose();
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
						
						val += hop.Value * 
							Complex.Exp(new Complex(0, kpt.DotProduct(R)));
					}
					
					if (Math.Abs(val.ImagPart) > 1e-7)
					{
						//Console.WriteLine("Imaginary part detected.  Check translation vectors: ");
						
						//for (int k = 0; k < p.Hoppings.Count; k++)
						//{
						//    HoppingValue hop = p.Hoppings[k];
							
						//    Console.WriteLine(hop.R);
						//}
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