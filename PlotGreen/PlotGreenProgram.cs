using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PlotGreen
{
	class PlotGreenProgram
	{
		static void Main(string[] args)
		{
			PlotGreenProgram inst = new PlotGreenProgram();
			inst.Run(args);
		}
		void Run(string[] args)
		{
			string inputfile = TightBinding.MainClass.GetInputFile(args);

			TightBinding.TbInputFile tb = new TightBinding.TbInputFile(inputfile);

			tb.ReadFile();

			if (File.Exists("green.dat") == false)
				throw new FileNotFoundException("The file green.dat is not present.", "green.dat");

			using (StreamReader r = new StreamReader("green.dat"))
			{

			}
		}
	}
}
