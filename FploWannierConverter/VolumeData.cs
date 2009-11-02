using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FploWannierConverter
{
	class VolumeData
	{
		public int Width { get; private set;  }
		public int Height { get; private set; }
		public int Depth { get; private set; }

		double[] data;

		public VolumeData(int width, int height, int depth)
		{
			this.Width = width;
			this.Height = height;
			this.Depth = depth;

			data = new double[width * height * depth];
		}
		public double this[int x, int y, int z]
		{
			get
			{
				AdjustPoint(ref x, ref y, ref z);

				return data[x + y * Width + z * Width * Height];
			}
			set
			{
				AdjustPoint(ref x, ref y, ref z);

				data[x + y * Width + z * Width * Height] = value;
			}
		}

		private void AdjustPoint(ref int x, ref int y, ref int z)
		{
			while (x < 0) x += Width;
			while (x >= Width) x -= Width;
			while (y < 0) y += Height;
			while (y >= Height) y -= Height;
			while (z < 0) z += Depth;
			while (z >= Depth) z -= Depth;
		}
	}
}
