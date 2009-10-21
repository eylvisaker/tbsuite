
using System;
using System.Collections.Generic;
using System.Linq;
using ERY.EMath;

namespace TightBindingSuite
{
	public class HoppingPair
	{
		int left, right;
		List<HoppingValue> hoppings = new List<HoppingValue>();
		
		public HoppingPair(int left, int right)
		{
			this.left = left;
			this.right = right;
		}
	
		public int Left { get { return left; } }
		public int Right { get { return right; } }
		
		public List<HoppingValue> Hoppings { get { return hoppings; } }
		
		public double GetHopping(Vector3 R)
		{
			foreach(var hop in hoppings)
			{
				if ((hop.R - R).MagnitudeSquared < 1e-12)
					return hop.Value;
			}
			
			return 0;
		}
		
		public override bool Equals (object obj)
		{
			if (obj is HoppingPair)
				return Equals((HoppingPair)obj);
			else
				return false;
		}
		public override int GetHashCode ()
		{
			return Left + Right + Hoppings.Count;
		}
 
 
		public bool Equals(HoppingPair a)
		{
			if (a == null)
				return false;
			
			if (a.Left != Left || a.Right != Right)
				return false;
			
			if (hoppings.Count != a.hoppings.Count)
			{
				Console.WriteLine("Wrong number of hoppings.");
				return false;
			}
			
			foreach(HoppingValue v in Hoppings)
			{
				if (a.Contains(v) == false)
				{
					return false;
				}
			}
			
			return true;
		}
		
		public bool Contains(HoppingValue val)
		{
			foreach (HoppingValue a in Hoppings)
			{
				if (a.Value != val.Value)
					continue;
				if ((a.R - val.R).Magnitude > 1e-6)
					continue;
				
				//Console.WriteLine("hop: {0} {1} {2} {3}",
				//                  a.Value, a.BravaisVector,
				//                  val.Value, val.BravaisVector);
				return true;
			}
			
			return false;
		}
	}
	
	public class HoppingPairList : List<HoppingPair>
	{
		public override bool Equals (object obj)
		{
			if (obj is HoppingPairList)
				return Equals((HoppingPairList)obj);
			else
				return false;
		}
		public override int GetHashCode ()
		{
			return 0;
		}
 
 
		public bool Equals(HoppingPairList a)
		{
			if (a.Count != Count)
			{
				Console.WriteLine("Failed count test.");
				return false;
			}
			
			foreach(var pair in this)
			{
				HoppingPair other = a.Find(pair.Left, pair.Right);	
				
				if (other == null)
				{
					Console.WriteLine("Failed to find match.");
					return false;
				}
				if (pair.Equals(other) == false)
				{
					return false;
				}
			}
			
			return true;
		}
		
		public HoppingPair FindOrThrow(int left, int right)
		{			
			HoppingPair retval = Find(left, right);
			
			if (retval == null)
				throw new Exception("The pair " + 
				                    (left+1).ToString() + " " + 
				                    (right+1).ToString() + " was not found.");
			
			return retval;
		}
		public HoppingPair Find(int left, int right)
		{
			HoppingPair retval = this.Find(x => x.Left == left && x.Right == right);
			
			if (retval == null)
			{
				HoppingPair p = this.Find(x => x.Right == left && x.Left == right);
				
				if (p == null)
					return null;
				
				// generate symmetric hopping if it doesn't exist.
				retval = new HoppingPair(left, right);
				
				for (int i = 0 ;i < p.Hoppings.Count; i++)
				{
					HoppingValue v = new HoppingValue();
					v.Value = p.Hoppings[i].Value;
					v.R = -p.Hoppings[i].R;
					
					retval.Hoppings.Add(v);
				}
				
				this.Add(retval);
			}
			
			return retval;
		}
		
		public void EnergyScale(out double min, out double max)
		{
			double emin = double.MaxValue, emax = double.MinValue;
			double tsum = 0;
				
			for (int i = 0; i < Count; i++)
			{
				HoppingPair p = this[i];
				
				if (p.Left == p.Right)
				{
					double site = p.GetHopping(Vector3.Zero);
					
					if (emin > site)
						emin = site;
					if (emax < site)
					    emax = site;
				}
				
				foreach(var hop in p.Hoppings)
				{
					if (p.Left == p.Right && hop.R == Vector3.Zero)
						continue;
					
					tsum += Math.Abs(hop.Value);
				}
			}
			
			min = emin - tsum;
			max = emax + tsum;
		}
	}
	
	public class HoppingValue
	{
		public double Value;
		public Vector3 R;
		
		public HoppingValue Clone()
		{
			return new HoppingValue { Value = Value, R = R };	
		}
		
		public override bool Equals (object obj)
		{
			if (obj is HoppingValue)
				return Equals((HoppingValue)obj);
			else
				return base.Equals(obj);
		}
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
 
		public bool Equals(HoppingValue v)
		{
			return Math.Abs(Value - v.Value) < 1e-6 && 
				(R - v.R).Magnitude < 1e-6;
		}
		public override string ToString ()
		{
			return string.Format("[HoppingValue {0} {1}]", Value, R);
		}
 
	}
}
