using System.Collections.Generic;
using System.IO;
using System.Linq;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic;
using SavioMacedo.MaoDesign.EmbroideryFormat.Extensions;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.Enums;
using SavioMacedo.MaoDesign.EmbroideryFormat.Exceptions;
using System;
using SkiaSharp;
using System.Text;
using EmbroideryHelper = SavioMacedo.MaoDesign.EmbroideryFormat.EmbroideryHelper.EmbroideryHelper;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Pec
{
    public class PecFile : EmbroideryBasic
    {
        private const int JumpCode = 0x10;
        private const int TrimCode = 0x20;
        private const int FlagLong = 0x80;

        static readonly int PEC_ICON_WIDTH = 48;
        static readonly int PEC_ICON_HEIGHT = 38;
        static readonly int MASK_07_BIT = 0b01111111;
        static readonly int JUMP_CODE = 0b00010000;
        static readonly int TRIM_CODE = 0b00100000;
        static readonly int FLAG_LONG = 0b10000000;

        public static PecFile Read(Stream stream, bool allowTransparency, bool hideMachinePath, float threadThickness)
        {
            return Read(stream.ReadFully(), allowTransparency, hideMachinePath, threadThickness);
        }

        public static byte[] Write(EmbroideryBasic embroidery)
        {
            using MemoryStream stream = new();
            using BinaryWriter writer = new(stream);
            embroidery.FixColorCount();
            writer.WriteString("#PEC0001");
            WritePecStitches(embroidery, writer);

            return stream.ToArray();
        }

        public static void WritePecStitches(EmbroideryBasic pattern, BinaryWriter file)
        {
            byte[,] image = new byte[38, 48];
            byte toWrite;
            int i, j, graphicsOffsetLocation;
            int graphicsOffsetValue;
            double xFactor, yFactor;
            string fileName = pattern.FileName;
            int width = (int)pattern.ImageWidth;
            int height = (int)pattern.ImageHeight;
            var (minX, minY, maxX, maxY) = pattern.Extents();

            file.WriteString("LA:");
            file.WriteString(fileName);
            for (int z = 0; z < (16 - fileName.Length); z++)
            {
                file.WriteString(" ");
            }
            file.Write((byte)0x0D);
            file.FPad(0x20, 12);
            file.Write(new byte[] { 0xFF, 0x00, 0x06, 0x26 });

            file.FPad(0x20, 12);
            toWrite = (byte)(pattern.Threads.Count - 1);
            file.Write(toWrite);

            foreach (EmbThread thread in pattern.Threads)
            {
                byte color = (byte)PecThread.FindNearestIndex(thread.Color, PecThread.GetThreadSet());
                file.Write(color);
            }
            file.FPad(0x20, 463 - pattern.Threads.Count);
            file.FPad(0x00, 2);

            graphicsOffsetLocation = (int)file.BaseStream.Position;
            // placeholder bytes to be overwritten
            file.FPad(0x00, 3);

            file.Write(new byte[] { 0x31, 0xFF, 0xF0 });

            // write 2 byte x size
            file.WriteInt16LE((short)width);
            // write 2 byte y size
            file.WriteInt16LE((short)height);

            // Write 4 miscellaneous int16's
            file.Write(new byte[] { 0x01, 0xE0, 0x01, 0xB0 });

            file.WriteInt16BE((ushort)(0x9000 | -EmbroideryHelper.EmbroideryHelper.EmbRound(minX)));
            file.WriteInt16BE((ushort)(0x9000 | -EmbroideryHelper.EmbroideryHelper.EmbRound(minY)));

            PecEncode(file, pattern);
            graphicsOffsetValue = (int)file.BaseStream.Position - graphicsOffsetLocation + 2;
            file.Seek(graphicsOffsetLocation, SeekOrigin.Begin);

            file.Write((byte)(graphicsOffsetValue & 0xFF));
            file.Write((byte)((graphicsOffsetValue >> 8) & 0xFF));
            file.Write((byte)((graphicsOffsetValue >> 16) & 0xFF));

            file.Seek(0x00, SeekOrigin.End);

            // Writing all colors
            Array.Copy(EmbroideryHelper.EmbroideryHelper.imageWithFrame, image, 48 * 38);

            yFactor = 32.0 / height;
            xFactor = 42.0 / width;
            for (i = 0; i < pattern.Stitches.Count; i++)
            {
                Stitch stitch = pattern.Stitches[i];
                int x = EmbroideryHelper.EmbroideryHelper.EmbRound((stitch.X - minX) * xFactor) + 3;
                int y = EmbroideryHelper.EmbroideryHelper.EmbRound((stitch.Y - minY) * yFactor) + 3;
                if (x <= 0 || x > 48) continue;
                if (y <= 0 || y > 38) continue;
                image[y, x] = 1;
            }
            WriteImage(file, image);

            // Writing each individual color
            j = 0;
            for (i = 0; i < pattern.Threads.Count; i++)
            {
                Array.Copy(EmbroideryHelper.EmbroideryHelper.imageWithFrame, image, 48 * 38);
                for (; j < pattern.Stitches.Count; j++)
                {
                    Stitch stitch = pattern.Stitches[j];
                    int x = EmbroideryHelper.EmbroideryHelper.EmbRound((stitch.X - minX) * xFactor) + 3;
                    int y = EmbroideryHelper.EmbroideryHelper.EmbRound((stitch.Y - minY) * yFactor) + 3;
                    if (x <= 0 || x > 48) continue;
                    if (y <= 0 || y > 38) continue;
                    if (stitch.Command == Command.ColorChange)
                    {
                        break;
                    }
                    image[y, x] = 1;
                }
                WriteImage(file, image);
            }
        }

        public static void WriteImage(BinaryWriter file, byte[,] image)
        {
            for (int i = 0; i < 38; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    int offset = j * 8;
                    byte output = 0;
                    output |= (byte)(image[i, offset] != 0 ? 1 : 0);
                    output |= (byte)((image[i, offset + 1] != 0 ? 1 : 0) << 1);
                    output |= (byte)((image[i, offset + 2] != 0 ? 1 : 0) << 2);
                    output |= (byte)((image[i, offset + 3] != 0 ? 1 : 0) << 3);
                    output |= (byte)((image[i, offset + 4] != 0 ? 1 : 0) << 4);
                    output |= (byte)((image[i, offset + 5] != 0 ? 1 : 0) << 5);
                    output |= (byte)((image[i, offset + 6] != 0 ? 1 : 0) << 6);
                    output |= (byte)((image[i, offset + 7] != 0 ? 1 : 0) << 7);
                    file.Write(output);
                }
            }
        }

        public static void PecEncode(BinaryWriter writer, EmbroideryBasic embroidery)
        {
            float thisX = 0;
            float thisY = 0;
            byte stopCode = 2;
            int i;

            for (i = 0; i < embroidery.Stitches.Count(); i++)
            {
                int deltaX, deltaY;
                Stitch stitch = embroidery.Stitches[i];

                deltaX = (int)Math.Round(stitch.X - thisX);
                deltaY = (int)Math.Round(stitch.Y - thisY);
                thisX += deltaX;
                thisY += deltaY;

                if (stitch.Command == Command.ColorChange)
                {
                    PecEncodeStop(writer, stopCode);
                    if (stopCode == 2)
                    {
                        stopCode = 1;
                    }
                    else
                    {
                        stopCode = 2;
                    }
                }
                else if (stitch.Command == Command.End)
                {
                    writer.Write((byte)0xFF);
                }
                else if (deltaX < 63 && deltaX > -64 && deltaY < 63 && deltaY > -64 && (!(stitch.Command == (Command.Jump | Command.Trim))))
                {
                    byte[] output = new byte[2];
                    if (deltaX < 0)
                    {
                        output[0] = (byte)(deltaX + 0x80);
                    }
                    else
                    {
                        output[0] = (byte)deltaX;
                    }
                    if (deltaY < 0)
                    {
                        output[1] = (byte)(deltaY + 0x80);
                    }
                    else
                    {
                        output[1] = (byte)deltaY;
                    }
                    writer.Write(output);
                }
                else
                {
                    PecEncodeJump(writer, deltaX, stitch.Command);
                    PecEncodeJump(writer, deltaY, stitch.Command);
                }
            }

        }

        static void PecEncodeJump(BinaryWriter file, int x, Command types)
        {
            int outputVal = Math.Abs(x) & 0x7FF;
            uint orPart = 0x80;
            byte toWrite;

            if (types == Command.Trim)
            {
                orPart |= 0x20;
            }

            if (types == Command.Jump)
            {
                orPart |= 0x10;
            }

            if (x < 0)
            {
                outputVal = (x + 0x1000) & 0x7FF;
                outputVal |= 0x800;
            }

            toWrite = (byte)(((outputVal >> 8) & 0x0F) | orPart);
            file.Write(toWrite);

            toWrite = (byte)(outputVal & 0xFF);
            file.Write(toWrite);
        }

        static void PecEncodeStop(BinaryWriter file, byte val)
        {
            file.Write((byte)0xFE);
            file.Write((byte)0xB0);
            file.Write(val);
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
                throw new UnknowFormatException($"PEC File with header not reconized: {pecString}, expected: #PEC0001");
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
