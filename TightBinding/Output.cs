using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TightBinding
{
	public static class Output
	{
		static StreamWriter file;

		public static void SetFile(StreamWriter file)
		{
			Output.file = file;
		}

		public static void WriteLine()
		{
			file.WriteLine();
			Console.WriteLine();
		}
		public static void WriteLine(string text)
		{
			file.WriteLine(text);
			Console.WriteLine(text);
		}
		public static void WriteLine(string format, params object[] args)
		{
			file.WriteLine(format, args);
			Console.WriteLine(format, args);
		}
	}
}
