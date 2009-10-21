using System;
using System.Collections.Generic;
using System.Text;
using ERY.EMath;

namespace TightBinding
{
	public class InteractionPair
	{
		List<int> mSitesLeft = new List<int>();
		List<int> mSitesRight = new List<int>();
		List<Vector3> mVectors = new List<Vector3>();

		string mLeftSite, mRightSite;

		public double HubbardU, InterorbitalU, Exchange, PairHopping;

		public InteractionPair(SiteList sites, string leftSite, string rightSite)
		{
			mLeftSite = leftSite;
			mRightSite = rightSite;

			mSitesLeft.AddRange(sites.SitesAt(leftSite));
			mSitesRight.AddRange(sites.SitesAt(rightSite));
		}

		public List<int> SitesLeft { get { return mSitesLeft;} }
		public List<int> SitesRight { get { return mSitesRight; } }

		public bool OnSite
		{
			get { return mVectors.Count == 0; }
		}

		public List<Vector3> Vectors { get { return mVectors; } }

		public double StructureFactor(Vector3 q)
		{
			if (OnSite)
				return 1;

			double retval = 0;

			for (int i = 0; i < Vectors.Count; i++)
			{
				retval += Math.Cos(q.DotProduct(Vectors[i]));
			}

			return retval;
		}
		public override string ToString()
		{
			return string.Format(
				"Interaction: {1}*'{0}' and {3}*'{2}', U={4},Up={5},J={6},Jp={7}",
				mLeftSite, mSitesLeft.Count, mRightSite, mSitesRight.Count,
				HubbardU, InterorbitalU, Exchange, PairHopping);
		}
	}

	public class InteractionList : List<InteractionPair>
	{
		public bool AdjustInteractions { get; set; }
	}


}
