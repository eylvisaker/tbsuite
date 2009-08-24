
using System;
using System.IO;

namespace TightBinding
{
	public abstract class InputReader
	{
		StreamReader reader;
		string line;
		int lineIndex;
		
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
		}
		protected abstract void Validate();
	
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
			//catch(Exception ex)
			//{
			//	ThrowEx(ex);	
			//}
		}
		protected abstract void ReadSection(string sectionName);
		
		protected void ReadNextLine()
		{
			if (reader.EndOfStream)
				throw new EndOfInputException();
			
			lineIndex++;			        
			line = reader.ReadLine().Trim();
						
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
		protected int LineIndex
		{
			get { return lineIndex; }	
		}
		
		protected bool EOF
		{
			get 
			{
				return reader.EndOfStream;	
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
