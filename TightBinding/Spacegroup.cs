using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ERY.EMath;

namespace TightBindingSuite
{
	public class SpaceGroup
	{
		static Dictionary<int, SpaceGroup> mGroups = new Dictionary<int, SpaceGroup>();
		static bool init = false;
		static SymmetryList mPrimitiveSymmetries = new SymmetryList();

		static SpaceGroup()
		{
			Initialize();
		}

		static void Initialize()
		{
			if (init)
				return;
			init = true;

			InitializeRegex();
			string groups = Properties.Resources.spgroup;
			
			using (StringReader r = new StringReader(groups))
			{
				for (; ; )
				{
					string groupName = r.ReadLine().Trim();
					int number;

					if (groupName.EndsWith("&"))
						break;

					if (int.TryParse(groupName.Substring(1, groupName.Length - 2), out number) == false)
					{
						throw new Exception();
					}


					SpaceGroup sp = new SpaceGroup();
					sp.Number = number;

					for (; ; )
					{
						if (r.Peek() == '/')
							break;

						string text = r.ReadLine();
						int lastColon = text.IndexOf(":", 8);

						var sym = ParseSymmetry(text);
						sym.Name = text.Substring(lastColon + 1).Trim();

						if (sym.Value.IsIdentity)
							continue;

						if (mPrimitiveSymmetries.Contains(sym) == false)
							mPrimitiveSymmetries.Add(sym);

						sp.PrimitiveSymmetries.Add(sym);
					}

					mGroups.Add(number, sp);
				}
			}

			using (StringReader r = new StringReader(Properties.Resources.spnames))
			{
				for (; ; )
				{
					string text = r.ReadLine();

					if (text == "&")
						break;

					string[] vals = text.Split('-');

					int number = int.Parse(vals[0]);
					string name = vals[1].Trim();

					mGroups[number].Name = name;
				}
			}
		}

		static Regex rotation;
		static Regex rotationValues;
		static Regex translation;
		static Regex translationValues;

		private static void InitializeRegex()
		{
			rotation = new Regex(@"\([+\-XYZ]+ *,[+\-XYZ]+ *,[+\-XYZ]+ *\)", RegexOptions.Compiled);
			rotationValues = new Regex(@"[+\-XYZ]+", RegexOptions.Compiled);

			translation = new Regex(@"\([0-9/]+ *,[0-9/]+ *,[0-9/]+ *\)", RegexOptions.Compiled);
			translationValues = new Regex(@"[0-9/]+", RegexOptions.Compiled);
		}

		private static Symmetry ParseSymmetry(string text)
		{
			Match rotText = rotation.Match(text);
			MatchCollection values = rotationValues.Matches(rotText.ToString());

			Matrix matrix = new Matrix(3, 3);
			for (int i = 0; i < 3; i++)
			{
				Vector3 opVec = GetOperationVector(values[i].ToString());

				matrix.SetRow(i, opVec);
			}

			Match transText = translation.Match(text);
			MatchCollection transValues = translationValues.Matches(transText.ToString());

			Vector3 trans = new Vector3();
			for (int i = 0; i < 3; i++)
			{
				trans[i] = EvaluateFraction(transValues[i].ToString());
			}


			Symmetry sym = new Symmetry(matrix, trans);
			return sym;
		}

		private static double EvaluateFraction(string text)
		{
			if (text.Contains("/") == false)
			{
				return double.Parse(text);
			}

			int sindex = text.IndexOf("/");

			int numer = int.Parse(text.Substring(0, sindex));
			int denom = int.Parse(text.Substring(sindex + 1));

			return numer / (double)denom;
		}
		private static Vector3 GetOperationVector(string p)
		{
			p = p.ToUpperInvariant();

			Vector3 retval = new Vector3();

			int sign = 1;
			for (int i = 0; i < p.Length; i++)
			{
				switch (p[i])
				{
					case '-': sign = -1; break;
					case '+': sign = +1; break;

					case 'X': retval[0] = sign; break;
					case 'Y': retval[1] = sign; break;
					case 'Z': retval[2] = sign; break;

					default:
						throw new InvalidDataException();
				}
			}

			System.Diagnostics.Debug.Assert(retval.Magnitude > 0);

			return retval;
		}

		public static SymmetryList AllPrimitiveSymmetries { get { return mPrimitiveSymmetries; } }

		public static SpaceGroup IdentifyGroup(SymmetryList syms)
		{
			for (int i = 230; i >= 1; i--)
			{
				SpaceGroup test = mGroups[i];
				bool found = true;

				foreach (var sym in test.PrimitiveSymmetries)
				{
					if (syms.Contains(sym) == false)
						found = false;
				}

				if (found)
				{
					test.GenerateSymmetries();
					return test;
				}
			}

			// return lowest symmetry group.
			mGroups[1].GenerateSymmetries();
			return mGroups[1];
		}
		public static SpaceGroup LowestSymmetryGroup
		{
			get
			{
				mGroups[1].GenerateSymmetries();
				return mGroups[1];
			}
		}

		public SpaceGroup()
		{
			PrimitiveSymmetries = new SymmetryList();
		}

		public SpaceGroup Clone()
		{
			SpaceGroup retval = new SpaceGroup();

			retval.Number = Number;
			retval.Name = Name;
			retval.Symmetries = Symmetries.Clone();
			retval.PrimitiveSymmetries = PrimitiveSymmetries.Clone();

			return retval;
		}
		public int Number { get; private set; }
		public string Name { get; private set; }

		public SymmetryList PrimitiveSymmetries { get; private set; }
		public SymmetryList Symmetries { get; set; }

		private void GenerateSymmetries()
		{
			Symmetries = new SymmetryList();
			Symmetries.Add(new Symmetry(Matrix.Identity(3)));

			foreach (var sym in PrimitiveSymmetries)
			{
				Symmetries.Add(sym.Clone());

				GenerateSymmetries(sym);
			}
		}

		private void GenerateSymmetries(Symmetry sym)
		{
			foreach (var s in PrimitiveSymmetries)
			{
				Symmetry newSym = Symmetry.Compose(sym, s);

				if (DuplicateSymmetry(newSym))
					continue;

				Symmetries.Add(newSym);
				GenerateSymmetries(newSym);
			}
		}

		private bool DuplicateSymmetry(Symmetry newSym)
		{
			foreach (Symmetry sym in Symmetries)
			{
				if ((sym.Value - newSym.Value).IsZero)
					return true;
			}

			return false;
		}

		public override string ToString()
		{
			return Name + " : " + Number.ToString() + "; " + PrimitiveSymmetries.Count.ToString();
		}


	}
}
