using System;
using System.IO;
using ERY.EMath;

namespace TightBinding
{
	class MainClass
	{
		public static int Main(string[] args)
		{
			if (args.Length < 2)
			{
				Usage();
				return 1;
			}

			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Start();

			TightBinding c = new TightBinding();

			for (int i = 0; i < args.Length; i += 2)
			{
				c.RunTB(args[i], args[i + 1]);
			}

			watch.Stop();
			Console.WriteLine("Total time: {0} s", watch.ElapsedMilliseconds / 1000.0);

			long milliseconds = watch.ElapsedMilliseconds;
			int seconds = (int)(milliseconds / 1000L);
			int minutes = seconds / 60;
			int hours = minutes / 60;
			minutes %= 60;
			seconds %= 60;
			milliseconds %= 1000;

			Console.WriteLine("            {0}:{1:00}:{2:00}.{3:000}", hours, minutes, seconds, milliseconds);
			return 0;
		}

		static void Usage()
		{
			Console.WriteLine("Usage: tightbinding inputfile outputfile");
		}
	}
}