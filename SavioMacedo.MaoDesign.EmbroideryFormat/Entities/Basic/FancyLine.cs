using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic
{
    public class FancyLine
    {
        public FancyLine(SKPoint a, SKPoint b)
        {
            A = a;
            B = b;
        }

        public SKPoint A { get; set; }
        public SKPoint B { get; set; }

        public double CalcLength()
        {
            double diffx = Math.Abs(A.X - B.X);
            double diffy = Math.Abs(A.Y - B.Y);
            return Math.Sqrt(Math.Pow(diffx, 2.0) + Math.Pow(diffy, 2.0));
        }
    }
}
