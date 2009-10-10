using System;
using System.IO;
using ERY.EMath;

namespace TightBinding
{
	class MainClass
	{
		public static int Main(string[] args)
		{
			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Start();

			TightBinding c = new TightBinding();

			string filename;
			string outputfile;

			if (args.Length == 0)
			{
				filename = AskForFilename();
			}
			else
				filename = args[0];

			if (filename.EndsWith(".out"))
				throw new Exception("Invalid filename.  It must not have the extension '.out'");

			outputfile = Path.GetFileNameWithoutExtension(filename) + ".out";

			
			using (StreamWriter output = new StreamWriter(outputfile))
			{
				Output.SetFile(output);

				c.RunTB(filename, outputfile);
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

		private static string AskForFilename()
		{
			bool done = false;
			string name = "";

			while (done == false)
			{
				Console.WriteLine("Enter input filename.");//  Type 'list' for a list of valid files in the ");
				//Console.WriteLine("current directory.");
				Console.Write("> ");

				name = Console.ReadLine();

				if (name == "list")
				{
					string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.in");

					foreach (string file in files)
					{
						Console.WriteLine(Path.GetFileName(file));
					}
				}
				else if (File.Exists(name))
					done = true;
				else
				{
					Console.WriteLine("The file '{0}' does not exist.", name);
				}

				Console.WriteLine();
			}

			return name;
		}

		static void Usage()
		{
			Console.WriteLine("Usage: tightbinding inputfile");
		}
	}
}