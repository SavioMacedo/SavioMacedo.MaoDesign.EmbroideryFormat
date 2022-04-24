using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.BitMapHelpers
{
    public struct OptimizedBlockData
    {
        public SKColor color;
        public SKPoint[] points;

        public OptimizedBlockData(SKColor color, SKPoint[] points)
        {
            this.color = color;
            this.points = points;
        }
    }
}
