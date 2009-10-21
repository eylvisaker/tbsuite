using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TightBindingSuite
{
	public static class InputHelper
	{
		public static string GetInputFile(string programName, string outSuffix, string[] args)
		{
			string text = new string(' ', (51 - programName.Length) / 2) +
							programName;
			text += new string(' ', 51 - text.Length);

			Console.WriteLine("-----------------------------------------------------");
			Console.WriteLine("|{0}|", text);
			Console.WriteLine("|             By Dr. Erik R. Ylvisaker              |");
			Console.WriteLine("-----------------------------------------------------");
			Console.WriteLine();

			string filename;

			if (args.Length == 0)
				filename = AskForFilename();
			else
				filename = args[0];

			if (filename.EndsWith(".out"))
				throw new Exception("Invalid filename.  It must not have the extension '.out'");

			string outputFile = Path.GetFileNameWithoutExtension(filename) + "." + outSuffix + ".out";

			StreamWriter output = new StreamWriter(outputFile);
			Output.SetFile(output);

			Output.WriteLine("-----------------------------------------------------");
			Output.WriteLine("|{0}|", text);
			Output.WriteLine("|             By Dr. Erik R. Ylvisaker              |");
			Output.WriteLine("-----------------------------------------------------");
			Output.WriteLine();

			return filename;
		}
		public static string GetOutputPrefix(string inputFile)
		{
			return Path.GetFileNameWithoutExtension(inputFile);
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
				else if (name.EndsWith(".in") == false && File.Exists(name + ".in"))
				{
					name += ".in";
					done = true;
				}
				else
				{
					Console.WriteLine("The file '{0}' does not exist.", name);
				}

				Console.WriteLine();
			}

			return name;
		}

	}
}
