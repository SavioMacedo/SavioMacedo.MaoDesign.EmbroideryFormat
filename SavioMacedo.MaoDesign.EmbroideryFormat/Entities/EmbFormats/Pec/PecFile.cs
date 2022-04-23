using System.Collections.Generic;
using System.IO;
using System.Linq;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic;
using SavioMacedo.MaoDesign.EmbroideryFormat.Extensions;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.Enums;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Pec
{
    public class PecFile : EmbroideryBasic
    {
        private const int JumpCode = 0x10;
        private const int TrimCode = 0x20;
        private const int FlagLong = 0x80;

        public static PecFile Read(Stream stream, bool allowTransparency, bool hideMachinePath, float threadThickness)
        {
            return Read(stream.ReadFully(), allowTransparency, hideMachinePath, threadThickness);
        }

        public static PecFile Read(byte[] bytes, bool allowTransparency, bool hideMachinePath, float threadThickness)
        {
            PecFile file = new()
            {
                Data = bytes
            };
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            string pecString = reader.ReadString8(8);

            if (!pecString.Equals("#PEC0001"))
            {
                return null;
            }

            file.ReadPec(reader);
            file.FileFormat = FileFormat.Pec;
            file.ToSkiaFormat();
            file.UpdateSkBitMap(allowTransparency, hideMachinePath, threadThickness);
            return file;
        }

        internal void ReadPec(BinaryReader reader, List<PecThread> pesCharts = null)
        {
            reader.BaseStream.Seek(3, SeekOrigin.Current);
            string label = reader.ReadString8(16).Trim();
            SetMetadata("Label", label);
            FileName = label;

            reader.BaseStream.Seek(0xF, SeekOrigin.Current);
            int pecGraphicByteStride = (int)reader.ReadInt8();
            int pecGraphicIconHeight = (int)reader.ReadInt8();
            reader.BaseStream.Seek(0xC, SeekOrigin.Current);
            int colorChanges = (int)reader.ReadInt8();
            int countColors = colorChanges + 1;
            byte[] colorBytes = reader.ReadBytes(countColors);
            List<PecThread> threads = new List<PecThread>();
            MapPecColors(colorBytes, pesCharts, threads);
            reader.BaseStream.Seek(0x1D0 - colorChanges, SeekOrigin.Current);
            long stitchBlockEnd = reader.ReadInt24Le() - 5 + reader.BaseStream.Position;
            reader.BaseStream.Seek(0x0F, SeekOrigin.Current);
            ReadPecStitches(reader);
            reader.BaseStream.Seek(stitchBlockEnd, SeekOrigin.Current);
            int byteSize = pecGraphicByteStride * pecGraphicIconHeight;
            ReadPecGraphics(reader, byteSize, pecGraphicByteStride, countColors + 1, threads);
        }

        private void ReadPecGraphics(BinaryReader reader, int byteSize, int pecGraphicByteStride, int count, List<PecThread> threads)
        {
            PecThread[] values = threads.ToArray();
            List<PecThread> valuesCopy = values.ToList();
            valuesCopy.Insert(0, null);
            for (int i = 0; i < count; i++)
            {
                _ = reader.ReadBytes(byteSize);
            }
        }

        private void ReadPecStitches(BinaryReader reader)
        {
            while (true)
            {
                int? val1 = reader.ReadInt8();
                int? val2 = reader.ReadInt8();

                if (val1 == 0xFF && val2 == 0x00 || val2 == null)
                    break;

                if (val1 == 0xFE && val2 == 0xB0)
                {
                    reader.BaseStream.Seek(1, SeekOrigin.Current);
                    ColorChange(0, 0);
                    continue;
                }

                bool jump = false;
                bool trim = false;
                int x;
                if ((val1 & FlagLong) != 0)
                {
                    if ((val1 & TrimCode) != 0)
                    {
                        trim = true;
                    }

                    if ((val1 & JumpCode) != 0)
                    {
                        jump = true;
                    }

                    int code = (int)val1 << 8 | (int)val2;
                    x = Signed12(code);
                    val2 = reader.ReadInt8();
                    if (val2 == null)
                        break;
                }
                else
                {
                    x = Signed7((int)val1);
                }

                int y = 0;

                if ((val2 & FlagLong) != 0)
                {
                    if ((val2 & TrimCode) != 0)
                    {
                        trim = true;
                    }

                    if ((val2 & JumpCode) != 0)
                    {
                        jump = true;
                    }

                    int? val3 = reader.ReadInt8();
                    if (val3 == null)
                        break;

                    int code = (int)val2 << 8 | (int)val3;
                    y = Signed12(code);
                }
                else
                    y = Signed7((int)val2);

                if (jump)
                {
                    Move(x, y);
                }
                else if (trim)
                {
                    Trim();
                    Move(x, y);
                }
                else
                {
                    Stitch(x, y);
                }
            }
            End();
        }

        private int Signed7(int b)
        {
            if (b > 63)
                return -128 + b;
            else
            {
                return b;
            }
        }

        private int Signed12(int b)
        {
            b &= 0xFFF;
            if (b > 0x7FF)
                return -0x1000 + b;
            else
                return b;
        }

        private void MapPecColors(byte[] colorBytes, List<PecThread> charts, List<PecThread> threads)
        {
            if (charts == null || charts.Count == 0)
            {
                ProcessPecColors(colorBytes, threads);
            }
            else if (charts.Count >= colorBytes.Length)
            {
                foreach (PecThread chart in charts)
                {
                    AddThread(chart);
                    threads.Add(chart);
                }
            }
            else
            {
                ProcessPecTable(colorBytes, charts, threads);
            }
        }

        private void ProcessPecTable(byte[] colorsBytes, List<PecThread> charts, List<PecThread> threads)
        {
            PecThread[] threadSet = PecThread.GetThreadSet();
            int maxValue = threadSet.Length;
            Dictionary<int, PecThread> threadMap = new Dictionary<int, PecThread>();

            for (int i = 0; i < colorsBytes.Length; i++)
            {
                int colorIndex = colorsBytes[i] % maxValue;
                PecThread threadValue = threadMap.GetValueOrDefault(colorIndex);
                if (threadValue == null)
                {
                    if (charts.Count >= 0)
                    {
                        threadValue = charts[0];
                        charts.RemoveAt(0);
                    }
                    else
                    {
                        threadValue = threadSet[colorIndex];
                    }

                    threadMap.Add(colorIndex, threadValue);
                }
                AddThread(threadValue);
                threads.Add(threadValue);
            }
        }

        private void ProcessPecColors(byte[] colorBytes, List<PecThread> threads)
        {
            PecThread[] threadSet = PecThread.GetThreadSet();
            int maxValue = threadSet.Length;

            foreach (byte color in colorBytes)
            {
                PecThread thread = threadSet[color % maxValue];
                AddThread(thread);
                threads.Add(thread);
            }
        }
    }
}
