using System;
using System.IO;
using ERY.EMath;

namespace TightBindingSuite
{
	public class MainClass
	{
		public static int Main(string[] args)
		{
			using (BootStrap b = new BootStrap())
			{
				System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
				watch.Start();


				string filename = b.GetInputFile("Tight Binding code", "tb", args);

				TightBinding c = new TightBinding();

				c.LoadTB(filename);
				c.RunTB();

				watch.Stop();
				Output.WriteLine("Total time: {0} s", watch.ElapsedMilliseconds / 1000.0);

				long milliseconds = watch.ElapsedMilliseconds;
				int seconds = (int)(milliseconds / 1000L);
				int minutes = seconds / 60;
				int hours = minutes / 60;
				minutes %= 60;
				seconds %= 60;
				milliseconds %= 1000;

				Output.WriteLine("            {0}:{1:00}:{2:00}.{3:000}", hours, minutes, seconds, milliseconds);
				return 0;
			}
		}

	}
}