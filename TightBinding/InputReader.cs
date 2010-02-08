using System;
using System.Collections.Generic;
using System.IO;

namespace TightBindingSuite
{
	public abstract class InputReader
	{
		StreamReader reader;
		string line;
		int lineIndex;
		SectionOptions options;

		public class SectionOptions : Dictionary<string,double?>
		{

		}

		public InputReader(string filename)
		{
			reader = new StreamReader(filename);
		}
		
		public void ReadFile()
		{
			reader.BaseStream.Position = 0;			
			
			ReadNextLine();	
			
			while (!EOF)
			{
				ReadSection();
				
				if (EOF == false && LineType != LineType.NewSection)
					ReadNextLine();
			}
			
			Validate();
			PostProcess();
		}
		protected abstract void Validate();
		protected abstract void PostProcess();

		protected void ThrowEx(string format, params object[] args)
		{
			string message = string.Format(format, args);
			ThrowEx(message);
		}
		protected void ThrowEx(string message)
		{
			throw new Exception(string.Format(
			    "Line {0}: {1}", lineIndex+1, message));
		}
			
		protected void ThrowEx(Exception inner)
		{
			throw new Exception(string.Format(
			    "Line {0}: {1}", lineIndex+1, inner.Message), inner);
		}
		void ReadSection()
		{
			if (LineType != LineType.NewSection)
				ThrowEx("Did not find new section.");
			
			string sectionName = line.Substring(1, line.Length - 2);
			
			try
			{
				ReadNextLine();
				ReadSection(sectionName);
			}
			catch(EndOfInputException)
			{
			}
#if !DEBUG
			catch (Exception ex)
			{
				ThrowEx(ex);
			}
#endif
		}
		protected abstract void ReadSection(string sectionName);
		
		protected void ReadNextLine()
		{
			if (reader.EndOfStream)
			{
				if (string.IsNullOrEmpty(line))
					throw new EndOfInputException();

				line = "";
				return;
			}
			
			lineIndex++;
			line = reader.ReadLine().Trim();
			line = line.Replace('\t', ' ');

			if (line.Contains("#"))
			{
				line = line.Substring(0, line.IndexOf("#"));
			}
			
			if (EOF == false && LineType == LineType.Empty)
			{
				ReadNextLine();
				return;
			}
		}
		protected string Line
		{
			get { return line; }
		}
		protected string[] LineWords
		{
			get { return Line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); }
		}
		protected int LineIndex
		{
			get { return lineIndex; }	
		}
		
		protected bool EOF
		{
			get 
			{
				if (string.IsNullOrEmpty(Line) && reader.EndOfStream)
					return true;
				else
					return false;
			}
		}
		
		protected LineType LineType
		{
			get
			{
				double dummy;
				
				if (string.IsNullOrEmpty(line))
					return LineType.Empty;
				if (line.StartsWith("[") && line.EndsWith("]"))
					return LineType.NewSection;
				if (line.StartsWith(":") && line.EndsWith(":") && line.Length > 1)
					return LineType.NewSubSection;
				if (line.StartsWith("T="))
					return LineType.Hopping;
				
				for (int i = 1; i < 5 && i < line.Length; i++)
				{
					if (double.TryParse(line.Substring(0, i), out dummy))
						return LineType.Numeric;
				}
				
				return LineType.Unknown;
			}
		}
		protected SectionOptions Options { get { return options; } }

		protected void ReadSectionOptions()
		{
			string[] vals = LineWords;

			options = new SectionOptions();
			double number;

			if (double.TryParse(vals[0], out number))
				return;

			for (int i = 0; i < vals.Length; i++)
			{
				string key = vals[i];

				if (i < vals.Length - 1)
				{
					if (double.TryParse(vals[i + 1], out number))
						options.Add(key, number);
					else
						options.Add(key, null);
				}
				else
				{
					options.Add(key, null);
				}
			}

			ReadNextLine();
		}
		protected string[] ReadSubSectionParameters()
		{
			if (LineType != LineType.NewSubSection)
				ThrowEx("Expected a : : delimited subsection.");

			string pair = Line.Substring(1, Line.Length - 2);
			string[] values = pair.Split(' ');

			//Output.WriteLine(Line);

			if (values.Length != 2)
				ThrowEx("Subsection does not contain a pair of values.");

			return values;
		}
			  
	}
	
	public enum LineType
	{
		Unknown,		
		Empty,

		NewSection,
		NewSubSection,
		Hopping,

		Numeric,
	}

	public class EndOfInputException : Exception
	{
	}
}
