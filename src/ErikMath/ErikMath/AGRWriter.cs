using System;
using System.Collections.Generic;
using System.Text;

namespace ERY.EMath
{

    internal class AGRWriter : IDisposable
    {
        private System.IO.TextWriter mFile = null;
        private string mFilename;

        private struct ColorStruct
        {
            public System.Drawing.Color Color;
            public string Name;

            public ColorStruct(System.Drawing.Color clr, string colorName)
            {
                Color = clr;
                Name = colorName;
            }

        }

        private List<ColorStruct> mColors = new List<ColorStruct>();

        public List<GraphDataSet> mDataSets = new List<GraphDataSet>();
        public double xmin = 0;
        public double xmax = 1;
        public double ymin = 0;
        public double ymax = 1;

        public double xMajorTick = 0.1;
        public double yMajorTick = 0.1;
        public int xMinorTicks = 1;
        public int yMinorTicks = 1;

        public string yAxisLabel = "Y";
        public string xAxisLabel = "X";

        public string title = "";
        public string subtitle = "";

        bool mUseSpecialXTicks = false;
        Dictionary<int, string> mSpecialXTicks = new Dictionary<int, string>();

        public AGRWriter()
        {
            SetColors();
        }
        public AGRWriter(string filename)
        {
            Open(filename);

            SetColors();
        }

        private void SetColors()
        {
            //"@map color 0 to (255, 255, 255), \"white\"\n" +
            //"@map color 1 to (0, 0, 0), \"black\"\n" +
            //"@map color 2 to (255, 0, 0), \"red\"\n" +
            //"@map color 3 to (0, 255, 0), \"green\"\n" +
            //"@map color 4 to (0, 0, 255), \"blue\"\n" +
            //"@map color 5 to (255, 255, 0), \"yellow\"\n" +
            //"@map color 6 to (188, 143, 143), \"brown\"\n" +
            //"@map color 7 to (220, 220, 220), \"grey\"\n" +
            //"@map color 8 to (148, 0, 211), \"violet\"\n" +
            //"@map color 9 to (0, 255, 255), \"cyan\"\n" +
            //"@map color 10 to (255, 0, 255), \"magenta\"\n" +
            //"@map color 11 to (255, 165, 0), \"orange\"\n" +
            //"@map color 12 to (114, 33, 188), \"indigo\"\n" +
            //"@map color 13 to (103, 7, 72), \"maroon\"\n" +
            //"@map color 14 to (64, 224, 208), \"turquoise\"\n" +
            //"@map color 15 to (0, 139, 0), \"green4\"\n" +
            mColors.Add(new ColorStruct(System.Drawing.Color.FromArgb(255, 255, 255), "white"));
            mColors.Add(new ColorStruct(System.Drawing.Color.FromArgb(0, 0, 0), "black"));
            mColors.Add(new ColorStruct(System.Drawing.Color.FromArgb(255, 0, 0), "red"));
            mColors.Add(new ColorStruct(System.Drawing.Color.FromArgb(0, 255, 0), "green"));
            mColors.Add(new ColorStruct(System.Drawing.Color.FromArgb(0, 0, 255), "blue"));
            mColors.Add(new ColorStruct(System.Drawing.Color.FromArgb(255, 255, 0), "yellow"));
            mColors.Add(new ColorStruct(System.Drawing.Color.FromArgb(188, 143, 143), "brown"));
            mColors.Add(new ColorStruct(System.Drawing.Color.FromArgb(220, 220, 220), "grey"));
            mColors.Add(new ColorStruct(System.Drawing.Color.FromArgb(148, 0, 211), "violet"));
            mColors.Add(new ColorStruct(System.Drawing.Color.FromArgb(0, 255, 255), "cyan"));
            mColors.Add(new ColorStruct(System.Drawing.Color.FromArgb(255, 0, 255), "magenta"));
            mColors.Add(new ColorStruct(System.Drawing.Color.FromArgb(255, 165, 0), "orange"));
            mColors.Add(new ColorStruct(System.Drawing.Color.FromArgb(114, 33, 188), "indigo"));
            mColors.Add(new ColorStruct(System.Drawing.Color.FromArgb(103, 7, 72), "maroon"));
            mColors.Add(new ColorStruct(System.Drawing.Color.FromArgb(64, 224, 208), "turquoise"));
            mColors.Add(new ColorStruct(System.Drawing.Color.FromArgb(0, 139, 0), "green4"));
        }

        ~AGRWriter()
        {
        }

        public bool Open(string filename)
        {
            mFilename = filename;
            mFile = new System.IO.StreamWriter(mFilename);

            if (mFile != null)
                return true;
            else
                return false;
        }
        public void Write()
        {
            if (mFile == null)
                throw new System.NullReferenceException();

            string header =
                    "# Grace project file \n" +
                    "#\n" +
                    "@version 50114\n" +
                    "@page size 504, 576\n" +
                    "@page scroll 5%\n" +
                    "@page inout 5%\n" +
                    "@link page off\n" +
                    "@map font 0 to \"Times-Roman\", \"Times-Roman\"\n" +
                    "@map font 1 to \"Times-Italic\", \"Times-Italic\"\n" +
                    "@map font 2 to \"Times-Bold\", \"Times-Bold\"\n" +
                    "@map font 3 to \"Times-BoldItalic\", \"Times-BoldItalic\"\n" +
                    "@map font 4 to \"Helvetica\", \"Helvetica\"\n" +
                    "@map font 5 to \"Helvetica-Oblique\", \"Helvetica-Oblique\"\n" +
                    "@map font 6 to \"Helvetica-Bold\", \"Helvetica-Bold\"\n" +
                    "@map font 7 to \"Helvetica-BoldOblique\", \"Helvetica-BoldOblique\"\n" +
                    "@map font 8 to \"Courier\", \"Courier\"\n" +
                    "@map font 9 to \"Courier-Oblique\", \"Courier-Oblique\"\n" +
                    "@map font 10 to \"Courier-Bold\", \"Courier-Bold\"\n" +
                    "@map font 11 to \"Courier-BoldOblique\", \"Courier-BoldOblique\"\n" +
                    "@map font 12 to \"Symbol\", \"Symbol\"\n" +
                    "@map font 13 to \"ZapfDingbats\", \"ZapfDingbats\"\n";

            for (int i = 0; i < mColors.Count; i++)
            {
                ColorStruct clr = mColors[i];

                header += string.Format("@map color {0} to ({1}, {2}, {3}), \"{4}\"\n",
                    i, clr.Color.R, clr.Color.G, clr.Color.B, clr.Name);

            }
             /*       "@map color 0 to (255, 255, 255), \"white\"\n" +
                    "@map color 1 to (0, 0, 0), \"black\"\n" +
                    "@map color 2 to (255, 0, 0), \"red\"\n" +
                    "@map color 3 to (0, 255, 0), \"green\"\n" +
                    "@map color 4 to (0, 0, 255), \"blue\"\n" +
                    "@map color 5 to (255, 255, 0), \"yellow\"\n" +
                    "@map color 6 to (188, 143, 143), \"brown\"\n" +
                    "@map color 7 to (220, 220, 220), \"grey\"\n" +
                    "@map color 8 to (148, 0, 211), \"violet\"\n" +
                    "@map color 9 to (0, 255, 255), \"cyan\"\n" +
                    "@map color 10 to (255, 0, 255), \"magenta\"\n" +
                    "@map color 11 to (255, 165, 0), \"orange\"\n" +
                    "@map color 12 to (114, 33, 188), \"indigo\"\n" +
                    "@map color 13 to (103, 7, 72), \"maroon\"\n" +
                    "@map color 14 to (64, 224, 208), \"turquoise\"\n" +
                    "@map color 15 to (0, 139, 0), \"green4\"\n" +
              * */

            header += 
                    "@reference date 0\n" +
                    "@date wrap off\n" +
                    "@date wrap year 1950\n" +
                    "@default linewidth 1.0\n" +
                    "@default linestyle 1\n" +
                    "@default color 1\n" +
                    "@default pattern 1\n" +
                    "@default font 0\n" +
                    "@default char size 1.000000\n" +
                    "@default symbol size 1.000000\n" +
                    "@default sformat \"%.8g\"\n" +
                    "@background color 0\n" +
                    "@page background fill on\n" +
                    "@timestamp off\n" +
                    "@timestamp 0.03, 0.03\n" +
                    "@timestamp color 1\n" +
                    "@timestamp rot 0\n" +
                    "@timestamp font 0\n" +
                    "@timestamp char size 1.000000\n" +
                    "@timestamp def \"Thu Jan 12 16:44:25 2006\"\n" +
                    "@r0 off\n" +
                    "@link r0 to g0\n" +
                    "@r0 type above\n" +
                    "@r0 linestyle 1\n" +
                    "@r0 linewidth 1.0\n" +
                    "@r0 color 1\n" +
                    "@r0 line 0, 0, 0, 0\n" +
                    "@r1 off\n" +
                    "@link r1 to g0\n" +
                    "@r1 type above\n" +
                    "@r1 linestyle 1\n" +
                    "@r1 linewidth 1.0\n" +
                    "@r1 color 1\n" +
                    "@r1 line 0, 0, 0, 0\n" +
                    "@r2 off\n" +
                    "@link r2 to g0\n" +
                    "@r2 type above\n" +
                    "@r2 linestyle 1\n" +
                    "@r2 linewidth 1.0\n" +
                    "@r2 color 1\n" +
                    "@r2 line 0, 0, 0, 0\n" +
                    "@r3 off\n" +
                    "@link r3 to g0\n" +
                    "@r3 type above\n" +
                    "@r3 linestyle 1\n" +
                    "@r3 linewidth 1.0\n" +
                    "@r3 color 1\n" +
                    "@r3 line 0, 0, 0, 0\n" +
                    "@r4 off\n" +
                    "@link r4 to g0\n" +
                    "@r4 type above\n" +
                    "@r4 linestyle 1\n" +
                    "@r4 linewidth 1.0\n" +
                    "@r4 color 1\n" +
                    "@r4 line 0, 0, 0, 0\n" +
                    "@g0 on\n" +
                    "@g0 hidden false\n" +
                    "@g0 type XY\n" +
                    "@g0 stacked false\n" +
                    "@g0 bar hgap 0.000000\n" +
                    "@g0 fixedpoint off\n" +
                    "@g0 fixedpoint type 0\n" +
                    "@g0 fixedpoint xy 0.000000, 0.000000\n" +
                    "@g0 fixedpoint format general general\n" +
                    "@g0 fixedpoint prec 6, 6\n"
                            ;

            mFile.Write(header);

            // now do the first graph
            mFile.Write("@with g0\n");

            string graphinfo =
                    "@    world xmin " + xmin + "\n" +
                    "@    world xmax " + xmax + "\n" +
                    "@    world ymin " + ymin + "\n" +
                    "@    world ymax " + ymax + "\n";


            graphinfo +=
                    "@    stack world 0, 0, 0, 0\n" +
                    "@    znorm 1\n" +
                    "@    view xmin 0.120000\n" +
                    "@    view xmax 0.950000\n" +
                    "@    view ymin 0.100000\n" +
                    "@    view ymax 1.030000\n" +
                    "@    title \"" + title + "\"\n" +
                    "@    title font 0\n" +
                    "@    title size 1.500000\n" +
                    "@    title color 1\n" +
                    "@    subtitle \"" + subtitle + "\"\n" +
                    "@    subtitle font 0\n" +
                    "@    subtitle size 1.000000\n" +
                    "@    subtitle color 1\n" +
                    "@    xaxes scale Normal\n" +
                    "@    yaxes scale Normal\n" +
                    "@    xaxes invert off\n" +
                    "@    yaxes invert off\n" +
                    "@    xaxis  on\n" +
                    "@    xaxis  type zero false\n" +
                    "@    xaxis  offset 0.000000 , 0.000000\n" +
                    "@    xaxis  bar on\n" +
                    "@    xaxis  bar color 1\n" +
                    "@    xaxis  bar linestyle 1\n" +
                    "@    xaxis  bar linewidth 1.0\n" +
                    "@    xaxis  label \"" + xAxisLabel + "\"\n" +
                    "@    xaxis  label layout para\n" +
                    "@    xaxis  label place auto\n" +
                    "@    xaxis  label char size 1.000000\n" +
                    "@    xaxis  label font 0\n" +
                    "@    xaxis  label color 1\n" +
                    "@    xaxis  label place normal\n" +
                    "@    xaxis  tick on\n" +
                    "@    xaxis  tick major " + xMajorTick + "\n" +
                    "@    xaxis  tick minor ticks " + xMinorTicks + "\n" +
                    "@    xaxis  tick default 6\n" +
                    "@    xaxis  tick place rounded true\n" +
                    "@    xaxis  tick in\n" +
                    "@    xaxis  tick major size 1.000000\n" +
                    "@    xaxis  tick major color 1\n" +
                    "@    xaxis  tick major linewidth 1.0\n" +
                    "@    xaxis  tick major linestyle 1\n" +
                    "@    xaxis  tick major grid off\n" +
                    "@    xaxis  tick minor color 1\n" +
                    "@    xaxis  tick minor linewidth 1.0\n" +
                    "@    xaxis  tick minor linestyle 1\n" +
                    "@    xaxis  tick minor grid off\n" +
                    "@    xaxis  tick minor size 0.500000\n" +
                    "@    xaxis  ticklabel on\n" +
                    "@    xaxis  ticklabel format general\n" +
                    "@    xaxis  ticklabel prec 5\n" +
                    "@    xaxis  ticklabel formula \"\"\n" +
                    "@    xaxis  ticklabel append \"\"\n" +
                    "@    xaxis  ticklabel prepend \"\"\n" +
                    "@    xaxis  ticklabel angle 0\n" +
                    "@    xaxis  ticklabel skip 0\n" +
                    "@    xaxis  ticklabel stagger 0\n" +
                    "@    xaxis  ticklabel place normal\n" +
                    "@    xaxis  ticklabel offset auto\n" +
                    "@    xaxis  ticklabel offset 0.000000 , 0.010000\n" +
                    "@    xaxis  ticklabel start type auto\n" +
                    "@    xaxis  ticklabel start 0.000000\n" +
                    "@    xaxis  ticklabel stop type auto\n" +
                    "@    xaxis  ticklabel stop 0.000000\n" +
                    "@    xaxis  ticklabel char size 1.000000\n" +
                    "@    xaxis  ticklabel font 0\n" +
                    "@    xaxis  ticklabel color 1\n" +
                    "@    xaxis  tick place both\n";

            if (!mUseSpecialXTicks)
            {
                graphinfo +=
                    "@    xaxis  tick spec type none\n";
            }
            else
            {
                graphinfo +=
                    "@    xaxis  tick spec type both\n" +
                    "@    xaxis  tick spec " + mSpecialXTicks.Count + "\n";

                int j = 0;

                foreach (KeyValuePair<int, string> kvp in mSpecialXTicks)
                {
                    graphinfo +=
                        "@    xaxis  tick major " + j.ToString() + ", " + kvp.Key.ToString() + "\n" +
                        "@    xaxis  ticklabel " + j.ToString() + ", \"" + kvp.Value + "\"\n";

                    j++;
                }
            }

            graphinfo +=
                    "@    yaxis  on\n" +
                    "@    yaxis  type zero false\n" +
                    "@    yaxis  offset 0.000000 , 0.000000\n" +
                    "@    yaxis  bar on\n" +
                    "@    yaxis  bar color 1\n" +
                    "@    yaxis  bar linestyle 1\n" +
                    "@    yaxis  bar linewidth 1.0\n" +
                    "@    yaxis  label \"" + yAxisLabel + "\"\n" +
                    "@    yaxis  label layout para\n" +
                    "@    yaxis  label place auto\n" +
                    "@    yaxis  label char size 1.000000\n" +
                    "@    yaxis  label font 0\n" +
                    "@    yaxis  label color 1\n" +
                    "@    yaxis  label place normal\n" +
                    "@    yaxis  tick on\n" +
                    "@    yaxis  tick major " + yMajorTick + "\n" +
                    "@    yaxis  tick minor ticks " + yMinorTicks + "\n" +
                    "@    yaxis  tick default 6\n" +
                    "@    yaxis  tick place rounded true\n" +
                    "@    yaxis  tick in\n" +
                    "@    yaxis  tick major size 1.000000\n" +
                    "@    yaxis  tick major color 1\n" +
                    "@    yaxis  tick major linewidth 1.0\n" +
                    "@    yaxis  tick major linestyle 1\n" +
                    "@    yaxis  tick major grid off\n" +
                    "@    yaxis  tick minor color 1\n" +
                    "@    yaxis  tick minor linewidth 1.0\n" +
                    "@    yaxis  tick minor linestyle 1\n" +
                    "@    yaxis  tick minor grid off\n" +
                    "@    yaxis  tick minor size 0.500000\n" +
                    "@    yaxis  ticklabel on\n" +
                    "@    yaxis  ticklabel format general\n" +
                    "@    yaxis  ticklabel prec 5\n" +
                    "@    yaxis  ticklabel formula \"\"\n" +
                    "@    yaxis  ticklabel append \"\"\n" +
                    "@    yaxis  ticklabel prepend \"\"\n" +
                    "@    yaxis  ticklabel angle 0\n" +
                    "@    yaxis  ticklabel skip 0\n" +
                    "@    yaxis  ticklabel stagger 0\n" +
                    "@    yaxis  ticklabel place normal\n" +
                    "@    yaxis  ticklabel offset auto\n" +
                    "@    yaxis  ticklabel offset 0.000000 , 0.010000\n" +
                    "@    yaxis  ticklabel start type auto\n" +
                    "@    yaxis  ticklabel start 0.000000\n" +
                    "@    yaxis  ticklabel stop type auto\n" +
                    "@    yaxis  ticklabel stop 0.000000\n" +
                    "@    yaxis  ticklabel char size 1.000000\n" +
                    "@    yaxis  ticklabel font 0\n" +
                    "@    yaxis  ticklabel color 1\n" +
                    "@    yaxis  tick place both\n" +
                    "@    yaxis  tick spec type none\n" +
                    "@    altxaxis  off\n" +
                    "@    altyaxis  off\n" +
                    "@    legend on\n" +
                    "@    legend loctype view\n" +
                    "@    legend 0.85, 0.8\n" +
                    "@    legend box color 1\n" +
                    "@    legend box pattern 1\n" +
                    "@    legend box linewidth 1.0\n" +
                    "@    legend box linestyle 1\n" +
                    "@    legend box fill color 0\n" +
                    "@    legend box fill pattern 1\n" +
                    "@    legend font 0\n" +
                    "@    legend char size 1.000000\n" +
                    "@    legend color 1\n" +
                    "@    legend length 4\n" +
                    "@    legend vgap 1\n" +
                    "@    legend hgap 1\n" +
                    "@    legend invert false\n" +
                    "@    frame type 0\n" +
                    "@    frame linestyle 1\n" +
                    "@    frame linewidth 1.0\n" +
                    "@    frame color 1\n" +
                    "@    frame pattern 1\n" +
                    "@    frame background color 0\n" +
                    "@    frame background pattern 0\n"
                ;

            mFile.Write(graphinfo);

            for (int i = 0; i < mDataSets.Count; i++)
            {
                WriteDataSetHeader(i, mDataSets[i]);
            }

            for (int i = 0; i < mDataSets.Count; i++)
            {
                WriteDataSet(i, mDataSets[i]);
            }
        }

        void WriteDataSetHeader(int index, GraphDataSet set)
        {

            string indexString = "s" + index.ToString();
            string colorString = ColorIndex(set.Color).ToString();

            int symbol = 0;
            
            if (set.DrawPoints)
            {
                switch (set.PointType)
                {
                    case GraphDataSet.SymbolType.Square: symbol = 2; break;
                    case GraphDataSet.SymbolType.Circle: symbol = 1; break;
                    case GraphDataSet.SymbolType.Point: symbol = 3; break;
                }
            }

            int lineType = 0;

            if (set.DrawLine)
            {
                switch (set.LineStyle)
                {
                    case GraphDataSet.LineType.Solid: lineType = 1; break;
                    case GraphDataSet.LineType.Dashed: lineType = 2; break;
                    case GraphDataSet.LineType.Dotted: lineType = 3; break;
                }
            }
            
            string header =
                    "@    " + indexString + " hidden false\n" +
                    "@    " + indexString + " type xy\n" +
                    "@    " + indexString + " symbol " + symbol + "\n" +
                    "@    " + indexString + " symbol size 1.000000\n" +
                    "@    " + indexString + " symbol color " + colorString + "\n" +
                    "@    " + indexString + " symbol pattern 1\n" +
                    "@    " + indexString + " symbol fill color 1\n" +
                    "@    " + indexString + " symbol fill pattern 0\n" +
                    "@    " + indexString + " symbol linewidth " + set.LineWeight + "\n" +
                    "@    " + indexString + " symbol linestyle 1\n" +
                    "@    " + indexString + " symbol char 65\n" +
                    "@    " + indexString + " symbol char font 0\n" +
                    "@    " + indexString + " symbol skip 0\n" +
                    "@    " + indexString + " line type " + lineType + "\n" +
                    "@    " + indexString + " line linestyle 1\n" +
                    "@    " + indexString + " line linewidth " + set.LineWeight + "\n" +
                    "@    " + indexString + " line color " + colorString + "\n" +
                    "@    " + indexString + " line pattern 1\n" +
                    "@    " + indexString + " baseline type 0\n" +
                    "@    " + indexString + " baseline off\n" +
                    "@    " + indexString + " dropline off\n" +
                    "@    " + indexString + " fill type 0\n" +
                    "@    " + indexString + " fill rule 0\n" +
                    "@    " + indexString + " fill color " + colorString + "\n" +
                    "@    " + indexString + " fill pattern 1\n" +
                    "@    " + indexString + " avalue off\n" +
                    "@    " + indexString + " avalue type 2\n" +
                    "@    " + indexString + " avalue char size 1.000000\n" +
                    "@    " + indexString + " avalue font 0\n" +
                    "@    " + indexString + " avalue color " + colorString + "\n" +
                    "@    " + indexString + " avalue rot 0\n" +
                    "@    " + indexString + " avalue format general\n" +
                    "@    " + indexString + " avalue prec 3\n" +
                    "@    " + indexString + " avalue prepend \"\"\n" +
                    "@    " + indexString + " avalue append \"\"\n" +
                    "@    " + indexString + " avalue offset 0.000000 , 0.000000\n" +
                    "@    " + indexString + " errorbar on\n" +
                    "@    " + indexString + " errorbar place both\n" +
                    "@    " + indexString + " errorbar color " + colorString + "\n" +
                    "@    " + indexString + " errorbar pattern 1\n" +
                    "@    " + indexString + " errorbar size 1.000000\n" +
                    "@    " + indexString + " errorbar linewidth 1.0\n" +
                    "@    " + indexString + " errorbar linestyle 1\n" +
                    "@    " + indexString + " errorbar riser linewidth 1.0\n" +
                    "@    " + indexString + " errorbar riser linestyle 1\n" +
                    "@    " + indexString + " errorbar riser clip off\n" +
                    "@    " + indexString + " errorbar riser clip length 0.100000\n" +
                    "@    " + indexString + " comment \"\"\n" +
                    "@    " + indexString + " legend  \"\"\n"
                ;


            mFile.Write(header);
        }

        private int ColorIndex(System.Drawing.Color color)
        {
            // first see if there is an exact match
            for (int i = 0; i < mColors.Count; i++)
            {
                ColorStruct clr = mColors[i];

                if (color.Equals(clr.Color))
                {
                    return i;
                }
            }

            // now find best match
            int index = 1;
            double distance = ColorDistance(mColors[1].Color, color);

            for (int i = 2; i < mColors.Count; i++)
            {
                ColorStruct clr = mColors[i];

                if (ColorDistance(color, clr.Color) < distance)
                {
                    distance = ColorDistance(color, clr.Color);
                    index = i;
                }
            }

            return index;
        }

        private double ColorDistance(System.Drawing.Color color, System.Drawing.Color color_2)
        {
            int r = Math.Abs(color.R - color_2.R);
            int g = Math.Abs(color.G - color_2.G);
            int b = Math.Abs(color.B - color_2.B);

            // weight green differences heavier, because of the human eye's sensitivity towards it.
            return Math.Sqrt(r * r + 1.5 * g * g + b * b);
        }

        void WriteDataSet(int index, GraphDataSet set)
        {
            string indexString = "S" + index.ToString();


            mFile.Write("@target G0.{0}\n@type xy\n", indexString);

            for (int i = 0; i < set.Count; i++)
            {
                if (float.IsNaN(set[i].X) || float.IsNaN(set[i].Y))
                    continue;
                if (float.IsInfinity(set[i].X) || float.IsInfinity(set[i].Y))
                    continue;

                mFile.Write("{0} {1}\n", set[i].X, set[i].Y);
            }

            mFile.Write("&\n");

        }

        public void AddSpecialXTick(int value, string label)
        {
            mSpecialXTicks[value] = label;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (mFile != null)
                mFile.Close();

            mFile = null;
        }

        #endregion
    }
}