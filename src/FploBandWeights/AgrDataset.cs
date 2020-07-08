using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERY.EMath;

namespace FploBandWeights
{
	class AgrDataset
	{
		public AgrDataset()
		{
			Data = new List<AgrDataPoint>();
			LineStyle = LineStyle.Solid;
			
			LineColor = GraceColor.Black;
			SymbolColor = GraceColor.Black;
			SymbolFillColor = GraceColor.Black;

			Symbol = Symbol.None;
			DatasetType = DatasetType.xy;

		}

		public List<AgrDataPoint> Data { get; set; }
		public DatasetType DatasetType { get; set; }

		public LineStyle LineStyle { get; set; }
		public double LineWidth { get; set; }
		public string Legend { get; set; }
		public Symbol Symbol { get; set; }

		public GraceColor LineColor { get; set; }
		public GraceColor SymbolColor { get; set; }
		public GraceColor SymbolFillColor { get; set; }

		public SymbolFill SymbolFill { get; set; }
	}

	public enum DatasetType
	{
		xy,
		xysize,
	}
	public enum LineStyle
	{
		None,
		Solid, 
		Dashed,
	}

	public enum Symbol
	{
		None,
		Circle,
		Square,
	}

	public enum GraceColor
	{
		White,
		Black,
		Red,
		Green,
		Blue,
	}

	public enum SymbolFill
	{
		None,
		Solid,
	}
}
