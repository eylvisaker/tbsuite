
using System;
using System.Collections.Generic;
using System.Linq;
using ERY.EMath;

namespace TightBindingSuite
{
	public class KptList
	{
		List<KPoint> allKpts = new List<KPoint>();
		List<KPoint> kpts = new List<KPoint>();
		List<Tetrahedron> tets = new List<Tetrahedron>();
		int[] mesh;
		int[] shift;
		bool gammaCentered;
		Dictionary<int, int> Nvalues = new Dictionary<int, int>();
		Dictionary<int, int> AllNvalues = new Dictionary<int, int>();

		Vector3 sdir, tdir, origin;

		public KptList()
		{
		}

		public static KptList DefaultPath(Lattice l)
		{
			const int pts = 40;
			KptList retval = new KptList();

			retval.Kpts.Add(new KPoint(Vector3.Zero));
			retval.AddPts(Vector3.Zero, l.G1 * Math.PI, pts);
			retval.AddPts(l.G1 * Math.PI, (l.G1 + l.G2) * Math.PI, pts);
			retval.AddPts((l.G1 + l.G2) * Math.PI, l.G2 * Math.PI, pts);
			retval.AddPts(l.G2 * Math.PI, l.G3 * Math.PI, pts);
			retval.AddPts(l.G3 * Math.PI, Vector3.Zero, pts);

			return retval;
		}
		public static Vector3 CalcK(Lattice l, double dx, double dy, double dz)
		{
			return dx * l.G1 + dy * l.G2 + dz * l.G3;
		}

		public int[] Mesh { get { return mesh; } }

		public int IndexOf(Vector3 kpt, int start)
		{
			for (int i = start; i < kpts.Count; i++)
			{
				Vector3 diff = kpts[i].Value - kpt;

				if (diff.X > 0.01) continue;
				if (diff.Y > 0.01) continue;
				if (diff.Z > 0.01) continue;

				if (diff.MagnitudeSquared < 1e-6)
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Adds "count" points from the start to the end vectors.
		/// The start point is not added, and the end point is.
		/// </summary>
		/// <param name="start">
		/// A <see cref="Vector3"/>
		/// </param>
		/// <param name="end">
		/// A <see cref="Vector3"/>
		/// </param>
		/// <param name="count">
		/// A <see cref="System.Int32"/>
		/// </param>
		public void AddPts(Vector3 start, Vector3 end, int count)
		{
			if (count <= 0)
				throw new ArgumentException("Count must be positive.");

			Vector3 step = (end - start) / count;

			for (int i = 1; i < count; i++)
			{
				Kpts.Add(new KPoint(start + step * i));
			}

			Kpts.Add(new KPoint(end));
		}

		public List<KPoint> Kpts { get { return kpts; } }
		public List<KPoint> AllKpts { get { return allKpts; } }

		public List<Tetrahedron> Tetrahedrons { get { return tets; } }

		private static bool CenterOnGamma(Lattice lattice, ref KPoint qpt, KptList list)
		{
			double dist = qpt.Value.Magnitude;
			double olddist = dist;
			Vector3 newPt = qpt.Value;
			bool retval = true;
			double bs, bt;

			list.GetPlaneST(qpt, out bs, out bt);


			for (int k = -1; k <= 1; k++)
			{
				for (int j = -1; j <= 1; j++)
				{
					for (int i = -1; i <= 1; i++)
					{
						if (i == 0 && j == 0 && k == 0)
							continue;

						Vector3 pt = qpt.Value +
							i * lattice.G1 +
							j * lattice.G2 +
							k * lattice.G3;

						double s, t;
						bool valid = list.GetPlaneST(new KPoint(pt), out s, out t);

						if (!valid)
							continue;

						if (pt.Magnitude < dist - 1e-6)
						{
							if (list.allKpts.Any(x => Math.Abs((x.Value - pt).Magnitude) > 1e-6))
							{
								retval = false;
								continue;
							}

							dist = pt.Magnitude;
							newPt = pt;
						}
					}
				}
			}

			if (olddist != dist)
				retval = true;

			if (retval == false)
				return false;

			qpt.Value = newPt;

			return true;
		}

		public static KptList GeneratePlane(Lattice lattice, Vector3[] points, SymmetryList syms, int[] qgrid, int[] qshift)
		{
			KptList qmesh = GenerateMesh(lattice, qgrid, qshift, syms, true);
			return GeneratePlane(lattice, points, syms, qmesh);
		}
		public static KptList GeneratePlane(Lattice lattice, Vector3[] points, SymmetryList syms, KptList qmesh)
		{
			Vector3 diff_1 = points[1] - points[0];
			Vector3 diff_2 = points[2] - points[0];
			Vector3 norm = Vector3.CrossProduct(diff_1, diff_2);

			KptList retval = new KptList();

			retval.mesh = (int[])qmesh.mesh.Clone();
			retval.shift = new int[3];
			retval.gammaCentered = true;

			retval.origin = points[0];
			retval.sdir = diff_1;
			retval.tdir = Vector3.CrossProduct(norm, diff_1);

			NormalizeST(lattice, retval);

			int zmax = qmesh.mesh[2] * 2;
			int ymax = qmesh.mesh[1] * 2;
			int xmax = qmesh.mesh[0] * 2;

			int index = 0;

			List<KPoint> planePoints  = new List<KPoint>();
			for (int i = 0; i < qmesh.AllKpts.Count; i++)
			{
				var qpt = qmesh.AllKpts[i];

				Vector3 diff = qpt.Value - points[0];
				double dot = Math.Abs(diff.DotProduct(norm));

				if (dot < 1e-8)
				{
					double s, t;
				
					retval.GetPlaneST(qpt, out s, out t); 
					planePoints.Add(qpt);
				}
			}
			SortByDistanceFromGamma(planePoints);

			for (int i = 0; i <planePoints.Count; i++)
			{
				var qpt = planePoints[i];
				double s, t;
				retval.GetPlaneST(qpt, out s, out t);

				//if (CenterOnGamma(lattice, ref qpt, retval) == false)
				//    continue;

				//retval.GetPlaneST(qpt, out news, out newt);

				retval.allKpts.Add(qpt);
			}

			// now sort q-points to lay them in the s,t plane.
			Comparison<KPoint> sorter = (x, y) =>
			{
				double s_x, s_y, t_x, t_y;

				retval.GetPlaneST(x, out s_x, out t_x);
				retval.GetPlaneST(y, out s_y, out t_y);

				if (Math.Abs(t_x - t_y) > 1e-6)
					return t_x.CompareTo(t_y);
				else
					return s_x.CompareTo(s_y);
			};

			retval.allKpts.Sort(sorter);
			for (int i = 0; i < retval.allKpts.Count; i++)
			{
				var qpt = retval.AllKpts[i];

				int N = retval.KptToInteger(lattice, qpt);
				int symN = N;
				bool foundSym = false;
				List<int> orbitals = null;

				Vector3 pt = qpt.Value;

				foreach (var symmetry in syms)
				{
					Vector3 Tpt = symmetry.Value * pt;

					int newi, newj, newk;
					retval.ReduceKpt(lattice, Tpt, out newi, out newj, out newk);

					if (newi % 2 != 0 || newj % 2 != 0 || newk % 2 != 0)
						continue;

					symN = retval.KptToInteger(newi, newj, newk);

					if (retval.Nvalues.ContainsKey(symN))
					{
						foundSym = true;

						if (symmetry.OrbitalTransform.Count > 0)
						{
							orbitals = symmetry.OrbitalTransform;
						}
					}

					if (foundSym)
						break;
				}

				if (foundSym == false && retval.Nvalues.ContainsKey(N) == false)
				{
					retval.kpts.Add(qpt);
					retval.Nvalues.Add(N, index);
					index++;
				}
				else if (retval.Nvalues.ContainsKey(N) == false)
				{
					int newIndex = retval.Nvalues[symN];
					retval.kpts[newIndex].AddOrbitalSymmetry(orbitals);

					retval.Nvalues.Add(N, newIndex);
				}
				else
				{ }  // skip points which are already in there.  This should only happen for zone edge points
			}
			
			retval.kpts.Sort(sorter);

			Vector3 sd = retval.sdir / SmallestNonzero(retval.sdir);
			Vector3 td = retval.tdir / SmallestNonzero(retval.tdir);

			Output.WriteLine("Plane horizontal direction: {0}", sd);
			Output.WriteLine("Plane vertical direction: {0}", td);

			Output.WriteLine("Plane horizontal vector: {0}", retval.sdir);
			Output.WriteLine("Plane vertical vector: {0}", retval.tdir);

			return retval;
		}


		private static void SortByDistanceFromGamma(List<KPoint> planePoints)
		{
			planePoints.Sort(
				(x, y) =>
				{
					return x.Value.Magnitude.CompareTo(y.Value.Magnitude);
				});
		}

		
		private static void NormalizeST(Lattice lattice, KptList retval)
		{
			retval.sdir /= retval.sdir.Magnitude;
			retval.tdir /= retval.tdir.Magnitude;

			retval.sdir /= GammaInDirection(lattice, retval.sdir).Magnitude;
			retval.tdir /= GammaInDirection(lattice, retval.tdir).Magnitude;

			// double them to make s and t 1 at the zone boundary, instead of 0.5.
			retval.sdir *= 2;
			retval.tdir *= 2;
		}

		private static Vector3 GammaInDirection(Lattice lattice, Vector3 direction)
		{
			Vector3 sred = lattice.ReducedCoords(direction);
			sred /= SmallestNonzero(sred);

			Vector3 retval = sred.X * lattice.G1 + sred.Y * lattice.G2 + sred.Z * lattice.G3;

			return retval;
		}

		private static double SmallestNonzero(Vector3 vector3)
		{
			double smallest = double.MaxValue;
			for (int i = 0; i < 3; i++)
			{
				if (Math.Abs(vector3[i]) < smallest &&
					Math.Abs(vector3[i]) > 1e-6)
					smallest = Math.Abs(vector3[i]);
			}

			return smallest;
		}
		public bool GetPlaneST(KPoint kpt, out double s, out double t)
		{
			s = (kpt.Value - origin).DotProduct(sdir);
			t = (kpt.Value - origin).DotProduct(tdir);

			double norm = (kpt.Value - origin).DotProduct(sdir.CrossProduct(tdir));

			if (Math.Abs(norm) > 1e-7)
				return false;
			else
				return true;
		}
		
		public static KptList GenerateMesh(Lattice lattice, int[] kgrid, int[] shift, SymmetryList syms, bool includeEnds)
		{
			KptList retval = new KptList();

			if (shift == null)
				shift = new int[3];

			for (int i = 0; i < 3; i++)
				if (kgrid[i] == 1)
					shift[i] = 1;

			retval.mesh = (int[])kgrid.Clone();
			retval.shift = (int[])shift.Clone();
			retval.gammaCentered = true;

			SymmetryList compatSyms = FindCompatibleSymmetries(kgrid, syms);

			int index = 0;
			for (int k = -kgrid[2] + shift[2]; k <= kgrid[2]; k += 2)
			{
				for (int j = -kgrid[1] + shift[1]; j <= kgrid[1]; j += 2)
				{
					for (int i = -kgrid[0] + shift[0]; i <= kgrid[0]; i += 2)
					{
						if (includeEnds == false)
						{
							if (k == kgrid[2]) break;
							if (j == kgrid[1]) break;
							if (i == kgrid[0]) break;
						}

						double dx = i * 0.5 / kgrid[0];
						double dy = j * 0.5 / kgrid[1];
						double dz = k * 0.5 / kgrid[2];

						Vector3 kptValue = CalcK(lattice, dx, dy, dz);

						int N = retval.KptToInteger(i, j, k);
						int symN = N;

						System.Diagnostics.Debug.Assert(!(retval.Nvalues.ContainsKey(N) && includeEnds == false));
						
						List<int> orbitals = null;
						bool foundSym = false;

						foreach (var symmetry in compatSyms)
						{
							Vector3 Tpt = symmetry.Value * kptValue;

							if (Tpt == kptValue)
								continue;

							int newi, newj, newk;
							
							retval.ReduceKpt(lattice, Tpt, out newi, out newj, out newk);

							if (Math.Abs(newi) % 2 != shift[0] ||
								Math.Abs(newj) % 2 != shift[1] ||
								Math.Abs(newk) % 2 != shift[2])
								continue;

							symN = retval.KptToInteger(newi, newj, newk);

							if (symN < N)
							{
								foundSym = true;

								if (symmetry.OrbitalTransform.Count > 0)
								{
									orbitals = symmetry.OrbitalTransform;
								}

								break;
							}
						}

						retval.AllNvalues.Add(N, retval.allKpts.Count);
						retval.allKpts.Add(new KPoint(kptValue));
						
						if (foundSym == false)
						{
							retval.kpts.Add(new KPoint(kptValue));
							retval.Nvalues.Add(N, index);
							index++;
						}
						else
						{
							int newIndex = retval.Nvalues[symN];
							retval.kpts[newIndex].Weight++;
							retval.kpts[newIndex].AddOrbitalSymmetry(orbitals);

							retval.Nvalues.Add(N, newIndex);
						}
					}
				}
			}

			int count = kgrid[0] * kgrid[1] * kgrid[2];
			for (int i = 0; i < retval.kpts.Count; i++)
			{
				retval.kpts[i].Weight /= count;
			}
			for (int i = 0; i < retval.allKpts.Count; i++)
			{
				retval.allKpts[i].Weight /= count;
			}

			return retval;
		}
		
		private static void TranslateKptToNearGamma(Lattice lattice, ref Vector3 kptValue)
		{
			Vector3 newkpt = kptValue;
			double newmag = newkpt.MagnitudeSquared;

			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					for (int k = -1; k <= 1; k++)
					{
						if (i == j && j == k && k == 0) continue;

						Vector3 G = i * lattice.G1 + j * lattice.G2 + k * lattice.G3;
						Vector3 kpt = kptValue + G;
						double kmag = kpt.MagnitudeSquared;

						if (kmag < newmag - 1e-6)
						{
							newkpt = kpt;
							newmag = kmag;
						}
					}
				}
			}

			kptValue = newkpt;
		}

		private void ReduceKpt(Lattice lattice, Vector3 pt, out int newi, out int newj, out int newk)
		{
			Vector3 red = lattice.ReducedCoords(pt, true);

			newi = (int)Math.Round(mesh[0] * red.X * 2);
			newj = (int)Math.Round(mesh[1] * red.Y * 2);
			newk = (int)Math.Round(mesh[2] * red.Z * 2);
		}

		private int KptToInteger(int i, int j, int k)
		{
			i += mesh[0];
			j += mesh[1];
			k += mesh[2];

			return i + j * 1000 + k * 1000000;
		}
		private int KptToInteger(Lattice lattice, KPoint qpt)
		{
			return KptToInteger(lattice, qpt.Value);
		}
		private int KptToInteger(Lattice lattice, Vector3 qpt)
		{
			int i, j, k;
			ReduceKpt(lattice, qpt, out i, out j, out k);

			return KptToInteger(i, j, k);
		}

		private static SymmetryList FindCompatibleSymmetries(int[] kgrid, SymmetryList syms)
		{
			SymmetryList compatSyms;
			compatSyms = new SymmetryList();
			Vector3 gridVector = new Vector3(kgrid[0], kgrid[1], kgrid[2]);

			foreach (var symmetry in syms)
			{
				Vector3 grid2 = symmetry.Value * gridVector;
				for (int gi = 0; gi < 3; gi++)
					grid2[gi] = Math.Abs(grid2[gi]);

				if (grid2 == gridVector)
				{
					compatSyms.Add(symmetry);
				}
			}
			return compatSyms;
		}

		public static KptList oldGenerateMesh(Lattice lattice, int[] kgrid, int[] shift, SymmetryList syms, bool includeEnds)
		{
			bool centerGamma = false;
			KptList retval = new KptList();
			int zmax = kgrid[2] * 2;
			int ymax = kgrid[1] * 2;
			int xmax = kgrid[0] * 2;

			if (shift == null)
				shift = new int[3];

			retval.mesh = (int[])kgrid.Clone();
			retval.shift = (int[])shift.Clone();
			retval.gammaCentered = centerGamma;

			int index = 0;
			Vector3 gridVector = new Vector3(kgrid[0], kgrid[1], kgrid[2]);

			SymmetryList compatSyms = new SymmetryList();
			foreach (var symmetry in syms)
			{
				Vector3 grid2 = symmetry.Value * gridVector;
				for (int gi = 0; gi < 3; gi++)
					grid2[gi] = Math.Abs(grid2[gi]);

				if (grid2 == gridVector)
				{
					compatSyms.Add(symmetry);
				}
			}

			for (int k = 0; k <= zmax; k += 2)
			{
				for (int j = 0; j <= ymax; j += 2)
				{
					for (int i = 0; i <= xmax; i += 2)
					{
						if (includeEnds == false)
						{
							if (k == zmax) break;
							if (i == xmax) break;
							if (j == ymax) break;
						}

						int N = retval.KptToInteger(i, j, k);
						bool foundSym = false;

						double dx = (i + shift[0]) / (double)xmax;
						double dy = (j + shift[1]) / (double)ymax;
						double dz = (k + shift[2]) / (double)zmax;
						int symN = N;
						List<int> orbitals = null;

						if (centerGamma)
						{
							if (kgrid[0] > 1) dx -= 0.5;
							if (kgrid[1] > 1) dy -= 0.5;
							if (kgrid[2] > 1) dz -= 0.5;
						}

						Vector3 pt = CalcK(lattice, dx, dy, dz);
						
#if DEBUG
						int testi, testj, testk;
						retval.ReduceKpt(lattice, new Vector3(pt), out testi, out testj, out testk);

						//System.Diagnostics.Debug.Assert(i == testi);
						//System.Diagnostics.Debug.Assert(j == testj);
						//System.Diagnostics.Debug.Assert(k == testk);

#endif
						foreach (var symmetry in compatSyms)
						{
							Vector3 Tpt = symmetry.Value * pt;

							if (Tpt == pt)
								continue;

							Vector3 red = lattice.ReducedCoords(Tpt, true);

							int newi = (int)Math.Round(xmax * red.X - shift[0]);
							int newj = (int)Math.Round(ymax * red.Y - shift[1]);
							int newk = (int)Math.Round(zmax * red.Z - shift[2]);

							if (newi % 2 != 0 || newj % 2 != 0 || newk % 2 != 0)
								continue;

							symN = retval.KptToInteger(newi, newj, newk);

							if (symN < N)
							{
								foundSym = true;

								if (symmetry.OrbitalTransform.Count > 0)
								{
									orbitals = symmetry.OrbitalTransform;
								}
							}

							if (foundSym)
								break;
						}

						Vector3 kptValue = CalcK(lattice, dx, dy, dz);

						retval.allKpts.Add(new KPoint(kptValue));

						if (retval.Nvalues.ContainsKey(N))
						{
							
						}
						else if (foundSym == false)
						{
							retval.kpts.Add(new KPoint(kptValue));
							retval.Nvalues.Add(N, index);
							index++;
						}
						else
						{
							int newIndex = retval.Nvalues[symN];
							retval.kpts[newIndex].Weight++;
							retval.kpts[newIndex].AddOrbitalSymmetry(orbitals);

							retval.Nvalues.Add(N, newIndex);
						}
					}
				}
			}

			int count = kgrid[0] * kgrid[1] * kgrid[2];
			for (int i = 0; i < retval.kpts.Count; i++)
			{
				retval.kpts[i].Weight /= count;
				retval.allKpts[i].Weight /= count;
			}

			if (includeEnds)
			{
				retval.allKpts.Sort((x, y) => x.Value.Z.CompareTo(y.Value.Z));

				List<int> removeThese = new List<int>();
				List<int> equivKpt = new List<int>();

				// read this value first, because the size of the array will change.
				int kptCount = retval.AllKpts.Count;
				for (int kindex = 0; kindex < kptCount; kindex++)
				{
					if (removeThese.Contains(kindex))
						continue;

					Vector3 kpt = retval.allKpts[kindex].Value;
					double dist = kpt.Magnitude;

					equivKpt.Clear();

					for (int k = -1; k <= 1; k++)
					{
						for (int j = -1; j <= 1; j++)
						{
							for (int i = -1; i <= 1; i++)
							{
								if (i == 0 && j == 0 && k == 0)
									continue;

								Vector3 pt = kpt +
									i * lattice.G1 +
									j * lattice.G2 +
									k * lattice.G3;

								double newDist = pt.Magnitude;

								if (newDist < dist - 1e-6)
								{
									foreach (var value in equivKpt)
									{
										if (removeThese.Contains(value) == false)
											removeThese.Add(value);
									}

									equivKpt.Clear();

									int search = retval.AllKpts.FindIndex(x => (x.Value - pt).Magnitude < 1e-6);

									if (search != -1)
									{
										if (removeThese.Contains(kindex) == false)
											removeThese.Add(kindex);

										// break out of the loop
										k = 1; j = 1; i = 2;
									}
									else
									{
										retval.allKpts[kindex].Value = pt;
										kpt = pt;
										dist = newDist;

										// reset variables since we updated this kpoint value.
										k = -1;
										j = -1;
										i = -2;
									}
								}
								else if (Math.Abs(dist - newDist) < 1e-6)
								{
									int search = retval.AllKpts.FindIndex(x => (x.Value - pt).Magnitude < 1e-6);

									if (search != -1)
									{
										if (removeThese.Contains(search))
										{
											k = 1; j = 1; i = 2;
											removeThese.Add(kindex);
											break;
										}

										equivKpt.Add(search);
										continue;
									}

									equivKpt.Add(retval.allKpts.Count);
									retval.allKpts.Add(new KPoint(pt));
								}
							}
						}
					}
				}

				// sort in reverse order
				removeThese.Sort((x, y) => -x.CompareTo(y));

				for (int i = 0; i < removeThese.Count; i++)
				{
					retval.allKpts.RemoveAt(removeThese[i]);
				}

				retval.allKpts.Sort((x, y) => x.Value.Z.CompareTo(y.Value.Z));
			}
#if DEBUG
			if (!includeEnds)
			{
				double check = 0;
				for (int i = 0; i < retval.kpts.Count; i++)
					check += retval.kpts[i].Weight;

				System.Diagnostics.Debug.Assert(Math.Abs(check - 1) < 1e-8);
			}
#endif

			return retval;
		}



		public int IrreducibleIndex(Vector3 kpt, Lattice lattice, SymmetryList symmetries, out List<int> orbitalMap)
		{
			for (int s = 0; s < symmetries.Count; s++)
			{
				int newi, newj, newk;
				Vector3 newKpt = symmetries[s].Inverse * kpt;

				ReduceKpt(lattice, newKpt, out newi, out newj, out newk);

				int N = KptToInteger(newi, newj, newk);

				if (Nvalues.ContainsKey(N))
				{
					int index = Nvalues[N];

					orbitalMap = symmetries[s].OrbitalTransform;
					return index;
				}
			}

			throw new Exception(string.Format("Could not find k-point {0}", kpt));
		}

		public int AllKindex(Vector3 kpt, Lattice lattice)
		{
			int newi, newj, newk;
				
			ReduceKpt(lattice, kpt, out newi, out newj, out newk);
			
			int N = KptToInteger(newi, newj, newk);

			return AllNvalues[N];
		}

		public override string ToString()
		{
			return string.Format("K-points: {0}   Irreducible: {1}", allKpts.Count, kpts.Count);
		}

		public void SetTemperature(double temperature, double mu)
		{
			double beta = 1 / temperature;

			SetTemperature(Kpts, beta, mu);
			SetTemperature(AllKpts, beta, mu);
		}

		private void SetTemperature(List<KPoint> kpts, double beta, double mu)
		{
			foreach (var k in kpts)
			{
				foreach (Wavefunction wfk in k.Wavefunctions)
				{
					wfk.FermiFunction = FermiFunction(wfk.Energy - mu, beta);
				}
			}
		}


		private double FermiFunction(double energy, double beta)
		{
			return 1.0 / (Math.Exp(beta * energy) + 1);
		}
	}
}