using System;
using System.Collections.Generic;
using System.IO;
using ERY.EMath;

namespace CoeffBrowser
{
	public class CoeffBrowser
	{
		List<string> States = new List<string>();
		List<KPoint> Kpoints = new List<KPoint>();
		
		public CoeffBrowser ()
		{
		}
		
		public void Run(string file)
		{
			KPoint kpt = null;
			
			using (	StreamReader r = new StreamReader(file))
			{
				r.ReadLine();
				ParseStateInfo(r.ReadLine());
				
				int lineNumber = 3;
				Console.WriteLine("Reading " + file + "...");
				Console.WriteLine();
				
				while (r.EndOfStream == false)
				{
					string line = r.ReadLine();	
					
					if (line.StartsWith("# spin"))
					{
						if (kpt != null)
							Kpoints.Add(kpt);
						
						kpt = ParseKpoint(line);
						continue;
					}
					else
					{
						kpt.Wfk.Add(ParseCoeffs(line));
					}
					
					if (lineNumber % 1000 == 0)
					{
						Console.CursorTop --;
						Console.WriteLine("Reading line {0}...", lineNumber);
					}
					
					lineNumber++;
				}
				
				Kpoints.Add(kpt);
			}
			
			RunMenu();
		}

		Wavefunction ParseCoeffs (string line)
		{
			string[] vals = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			
			Wavefunction retval = new Wavefunction();
			
			retval.Energy = double.Parse(vals[1]);
			
			for (int i = 2; i < vals.Length; i += 2)
			{
				retval.Coeffs.Add(new Complex(double.Parse(vals[i]),
				                              double.Parse(vals[i+1])));
			}
			
			return retval;
		}


		KPoint ParseKpoint (string line)
		{
			KPoint retval = new KPoint();
			
			retval.Spin = int.Parse(line.Substring(8,3));
			retval.K = new ERY.EMath.Vector3(double.Parse(line.Substring(20, 14)),
			                                 double.Parse(line.Substring(37, 14)),			
			                                 double.Parse(line.Substring(54, 13)));
			
			retval.dk = double.Parse(line.Substring(77));
			
			return retval;
		}

		void ParseStateInfo (string text)
		{
			string lineStart = "# band index         e(k,n)       ";
			
			if (text.StartsWith(lineStart) == false)
				throw new Exception("Could not understand line 2.");

			text = text.Substring(lineStart.Length, text.Length - lineStart.Length);
			
			const int itemSize = 34;
			
			for (int index = 0; index < text.Length; index += itemSize)
			{
				States.Add(text.Substring(index, Math.Min(itemSize, text.Length - index)).Trim());
			}
		}

		class Contrib : IComparable<Contrib>
		{
			public string Text { get; set; }
			public Complex Value { get; set; }
			
			#region IComparable<Contrib> implementation
			
			int IComparable<Contrib>.CompareTo (Contrib other)
			{
				return -Value.MagnitudeSquared.CompareTo(other.Value.MagnitudeSquared);
			}
			
			#endregion
		}
		
		void RunMenu ()
		{
			int kptIndex = 0;
			int wfkIndex = 0;
			
			bool quit = false;
			
			double tolerance = 1e-4;
			
			while (quit == false)
			{			
				KPoint kpt = Kpoints[kptIndex];
				Wavefunction wfk = kpt.Wfk[wfkIndex];
				
				Console.Clear();
				
				Console.WriteLine("KPoint Index: {0}", kptIndex);
				Console.WriteLine("Spin: {0}    K: {1}  {2}  {3}    dk: {4}", 
				                  kpt.Spin, kpt.K[0], kpt.K[1], kpt.K[2], kpt.dk);

				Console.WriteLine();
				
				Console.WriteLine("Wavefunction energy: {0}", wfk.Energy);
				Console.WriteLine("Contributions:");
				
				List<Contrib> contribs = new List<Contrib>();
				
				for (int i = 0; i < wfk.Coeffs.Count; i++)
				{
					if (wfk.Coeffs[i].Magnitude < tolerance)
						continue;
				
					contribs.Add(new Contrib 
					             { 
						Text = States[i], 
						Value = wfk.Coeffs[i]
					});
				}
				
				if (contribs.Count > 0)
				{
					contribs.Sort();
					Complex divisor = contribs[0].Value;
					divisor /= divisor.Magnitude;
					
					foreach(var c in contribs)
					{
						Console.WriteLine("    {0}: {1}", c.Text, c.Value / divisor);
					}
				}
				
				Console.WriteLine();
				Console.WriteLine("Up/Down to change band index.  Left/Right to change kpt index.");
				Console.WriteLine("Plus/Minus to change coefficient tolerance.");
				Console.WriteLine("q to quit.");
				
				var key = Console.ReadKey(true);
				
				if (key.Key == ConsoleKey.UpArrow)
				{
					wfkIndex++;
					if (wfkIndex >= kpt.Wfk.Count)
						wfkIndex = kpt.Wfk.Count-1;
				}
				else if (key.Key == ConsoleKey.DownArrow)
				{
					wfkIndex--;
					if (wfkIndex < 0)
						wfkIndex = 0;
				}
				else if (key.Key == ConsoleKey.RightArrow)
				{
					kptIndex++;
					if (kptIndex >= Kpoints.Count )
						kptIndex = Kpoints.Count-1;
				}
				else if (key.Key == ConsoleKey.LeftArrow)
				{
					kptIndex--;
					if (kptIndex < 0)
						kptIndex = 0;
				}
				else if (key.Key == ConsoleKey.Q)
				{
					quit = true;
				}
				else if (key.Key == ConsoleKey.Add)
				{
					tolerance /= 2;	
				}
				else if (key.Key == ConsoleKey.Subtract)
				{
					tolerance *= 2;	
				}
			}				
		}

	}
}
