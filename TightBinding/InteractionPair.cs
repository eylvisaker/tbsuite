using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace TightBindingSuite
{
	public class InteractionPair
	{
		List<int> mOrbitalsLeft = new List<int>();
		List<int> mOrbitalsRight = new List<int>();
		List<Vector3> mVectors = new List<Vector3>();

		string mLeftGroup, mRightGroup;

		public double HubbardU, InterorbitalU, Exchange, PairHopping;

		InteractionPair() { }
		public InteractionPair(OrbitalList orbitals, string leftGroup, string rightGroup)
		{
			mLeftGroup = leftGroup;
			mRightGroup = rightGroup;

			mOrbitalsLeft.AddRange(orbitals.OrbitalsInInteractionGroup(leftGroup));
			mOrbitalsRight.AddRange(orbitals.OrbitalsInInteractionGroup(rightGroup));
		}
		internal InteractionPair Clone()
		{
			InteractionPair p = new InteractionPair();

			p.mOrbitalsLeft.AddRange(mOrbitalsLeft);
			p.mOrbitalsRight.AddRange(mOrbitalsRight);
			p.mVectors.AddRange(mVectors);

			p.mLeftGroup = mLeftGroup;
			p.mRightGroup = mRightGroup;

			p.HubbardU = HubbardU;
			p.InterorbitalU = InterorbitalU;
			p.Exchange = Exchange;
			p.PairHopping = PairHopping;

			return p;
		}

		public List<int> OrbitalsLeft { get { return mOrbitalsLeft;} }
		public List<int> OrbitalsRight { get { return mOrbitalsRight; } }
		public List<Vector3> Vectors { get { return mVectors; } }

		public bool OnSite
		{
			get { return mVectors.Count == 0; }
		}

		
		public double StructureFactor(Vector3 q)
		{
			if (OnSite)
				return 1;

			double retval = 0;

			for (int i = 0; i < Vectors.Count; i++)
			{
				retval += Math.Cos(2 * Math.PI * q.DotProduct(Vectors[i]));
			}

			return retval;
		}
		public override string ToString()
		{
			return string.Format(
				"Interaction: {1}*'{0}' and {3}*'{2}', U={4},Up={5},J={6},Jp={7}",
				mLeftGroup, mOrbitalsLeft.Count, mRightGroup, mOrbitalsRight.Count,
				HubbardU, InterorbitalU, Exchange, PairHopping);
		}

	}

	public class InteractionList : List<InteractionPair>
	{
		public InteractionList()
		{
			MaxEigenvalue = 0.999;
		}
		public bool AdjustInteractions { get; set; }
		/// <summary>
		/// Maximum eigenvalue for the denominator in the RPA formula x/(1 - S*x)
		/// </summary>
		public double MaxEigenvalue { get; set; }

		internal InteractionList Clone()
		{
			InteractionList L = new InteractionList();

			L.AddRange(this.Select(x => x.Clone()));

			return L;
		}
	}


}
