using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic
{
    public class EmbThread
    {
        public EmbThread(uint color = 0xFF000000, string description = "", string catalogNumber = "", string details = "", string brand = "", string chart = "", string weight = "")
        {
            Color = new SKColor(color);
            Description = description;
            CatalogNumber = catalogNumber;
            Details = details;
            Brand = brand;
            Chart = chart;
            Weight = weight;
            FancyLines = new List<FancyLine>();
        }

        public EmbThread()
        {
        }

        public SKColor Color { get; set; }
        public string Description { get; set; }
        public string CatalogNumber { get; set; }
        public string Details { get; set; }
        public string Brand { get; set; }
        public string Chart { get; set; }
        public string Weight { get; set; }
        public List<FancyLine> FancyLines { get; set; }

        public uint OpaqueColor
        {
            get
            {
                return 0xFF000000 | (uint)Color;
            }
        }

        public long Red => Color.Red;

        public long Green => Color.Green;

        public long Blue => Color.Blue;

        public string HexColor => Color.ToString();

        public void SetColor(uint red, uint green, uint blue)
        {
            Color = 0xFF000000 | (red & 255) << 16 | (green & 255) << 8 | blue & 255;
        }

        public void SetColor(string hexString)
        {
            try
            {
                string hex = hexString.Replace("#", "");
                if (hex.Length == 6 || hex.Length == 8)
                {
                    Color = SKColor.Parse(hex);
                }
                else if (hex.Length == 4 || hex.Length == 3)
                {
                    string convertColor = $"{hex[2]}{hex[2]}{hex[1]}{hex[1]}{hex[0]}{hex[0]}";
                    Color = SKColor.Parse(convertColor);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int FindNearestColorIndex(IEnumerable<EmbThread> embThreads)
        {
            long red = Red;
            long green = Green;
            long blue = Blue;

            int closestIndex = -1;
            int currentIndex = -1;
            float currentClosestValue = 0;

            foreach (EmbThread thread in embThreads)
            {
                currentIndex += 1;
                long dist = colorDistanceRedMean(red, green, blue, thread.Red, thread.Green, thread.Blue);
                if (dist < currentClosestValue)
                {
                    currentClosestValue = dist;
                    closestIndex = currentIndex;
                }
            }

            return currentIndex;
        }

        private long colorDistanceRedMean(long red1, long green1, long blue1, long red2, long green2, long blue2)
        {
            int redMean = int.Parse(Math.Round((decimal)((red1 + red2) / 2)).ToString());
            long red = red1 - red2;
            long green = green1 - green2;
            long blue = blue1 - blue2;

            return ((512 + redMean) * red * red >> 8) + 4 * green * green + ((767 - redMean) * blue * blue >> 8);
        }

        public SKColor AsSkColor()
        {
            return new SKColor((byte)Red, (byte)Green, (byte)Blue);
        }

        public static EmbThread GetThreadSet()
        {
            return new EmbThread();
        }

        //public static int findNearestColor<T>(int findColor, T[] values) where T: EmbThread
        //{
        //    return findNearestThread(findColor, values).getColor();
        //}

        public static int FindNearestIndex<T>(SKColor color, T[] values) where T: EmbThread
        {
            double currentClosestValue = double.PositiveInfinity;

            int closestIndex = -1;
            int currentIndex = -1;
            foreach (EmbThread thread in values)
            {
                currentIndex++;
                if (thread == null)
                {
                    continue;
                }
                double dist = DistanceRedMean(color.Red, color.Green, color.Blue, thread.Red, thread.Green, thread.Blue);
                if (dist <= currentClosestValue)
                {
                    currentClosestValue = dist;
                    closestIndex = currentIndex;
                }
            }
            return closestIndex;
        }

        public static double DistanceRedMean(int r1, int g1, int b1, long r2, long g2, long b2)
        {
            long rmean = (r1 + r2) / 2;
            long r = r1 - r2;
            long g = g1 - g2;
            long b = b1 - b2;
            return (((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8);
        }
    }
}
