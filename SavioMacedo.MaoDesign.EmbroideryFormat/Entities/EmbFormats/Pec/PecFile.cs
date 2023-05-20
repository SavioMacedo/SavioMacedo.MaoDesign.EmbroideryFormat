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

        //The method that i need to create is Write and receives a EmbroideryBasic and returns a byte[], the byte[] is a .pec format file
        //I need to create this method because i need to create a .pec file format from a EmbroideryBasic
        //The EmbroideryBasic is a class that i created to represent the embroidery file, it has a list of stitchs and a list of threads
        //Take a look at the EmbroideryBasic.cs file to understand better
        //Take a look on internet to understand the .pec file format
        //Take a look at the repository https://github.com/Embroidermodder/libembroidery/blob/main/src/formats/format_pec.c to understand how to create the .pec file format
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

            file.Write(Encoding.ASCII.GetBytes("LA:"), 0, 3);
            file.WriteString(fileName);
            for (int z = 0; z < (16 - fileName.Length); z++)
            {
                file.WriteString(" ");
            }
            file.Write(0x0D);
            file.FPad(0x20, 12);
            file.Write(new byte[] { 0xFF, 0x00, 0x06, 0x26 }, 0, 4);

            file.FPad(0x20, 12);
            toWrite = (byte)(pattern.Threads.Count - 1);
            file.Write(toWrite);

            foreach (EmbThread thread in pattern.Threads)
            {
                byte color = (byte)PecThread.FindNearestIndex((int)(uint)thread.Color, PecThread.GetThreadSet());
                file.Write(color);
            }
            file.FPad(0x20, 0x1CF - pattern.Threads.Count);
            file.FPad(0x00, 2);

            graphicsOffsetLocation = (int)file.BaseStream.Position;
            // placeholder bytes to be overwritten
            file.FPad(0x00, 3);

            file.Write(new byte[] { 0x31, 0xFF, 0xF0 }, 0, 3);

            // write 2 byte x size
            file.WriteInt16LE((short)width);
            // write 2 byte y size
            file.WriteInt16LE((short)height);

            // Write 4 miscellaneous int16's
            file.Write(new byte[] { 0x01, 0xE0, 0x01, 0xB0 }, 0, 4);

            file.WriteInt16BE((ushort)(0x9000 | -EmbroideryHelper.EmbroideryHelper.EmbRound(minX)));
            file.WriteInt16BE((ushort)(0x9000 | -EmbroideryHelper.EmbroideryHelper.EmbRound(maxY)));

            PecEncode(file, pattern);
            graphicsOffsetValue = (int)file.BaseStream.Position - graphicsOffsetLocation + 2;
            file.Seek(graphicsOffsetLocation, SeekOrigin.Begin);

            file.Write((byte)(graphicsOffsetValue & 0xFF));
            file.Write((byte)((graphicsOffsetValue >> 8) & 0xFF));
            file.Write((byte)((graphicsOffsetValue >> 16) & 0xFF));

            file.Seek(0x00, SeekOrigin.End);

            // Writing all colors
            Buffer.BlockCopy(EmbroideryHelper.EmbroideryHelper.imageWithFrame, 0, image, 0, 48 * 38);

            yFactor = 32.0 / height;
            xFactor = 42.0 / width;
            for (i = 0; i < pattern.Stitches.Count; i++)
            {
                Stitch st = pattern.Stitches[i];
                int x = EmbroideryHelper.EmbroideryHelper.EmbRound((st.X - minX) * xFactor) + 3;
                int y = EmbroideryHelper.EmbroideryHelper.EmbRound((st.Y - maxY) * yFactor) + 3;
                if (x <= 0 || x > 48) continue;
                if (y <= 0 || y > 38) continue;
                image[y, x] = 1;
            }
            WriteImage(file, image);

            // Writing each individual color
            j = 0;
            for (i = 0; i < pattern.Threads.Count; i++)
            {
                Buffer.BlockCopy(EmbroideryHelper.EmbroideryHelper.imageWithFrame, 0, image, 0, 48 * 38);
                for (; j < pattern.Stitches.Count; j++)
                {
                    Stitch stitch = pattern.Stitches[j];
                    int x = EmbroideryHelper.EmbroideryHelper.EmbRound((stitch.X - minX) * xFactor) + 3;
                    int y = EmbroideryHelper.EmbroideryHelper.EmbRound((stitch.Y - maxY) * yFactor) + 3;
                    if (x <= 0 || x > 48) continue;
                    if (y <= 0 || y > 38) continue;
                    if (stitch.Command == Command.Stop)
                    {
                        break;
                    }
                    image[y, x] = 1;
                }
                WriteImage(file, image);
            }
        }

        //private static void WritePec(BinaryWriter writer, string fileName, EmbroideryBasic embroidery)
        //{
        //    var (minX, minY, maxX, maxY) = embroidery.Extents();
        //    byte[,] image = new byte[38, 48];
        //    byte toWrite;
        //    double xFactor, yFactor;

        //    float width = embroidery.ImageWidth;
        //    float height = embroidery.ImageHeight;

        //    writer.WriteString("LA:");
        //    writer.WriteString(fileName);
        //    for (int i = 0; i < (16 - fileName.Length); i++)
        //    {
        //        writer.WriteString(" ");
        //    }

        //    writer.WriteInt8(0x0D);
        //    for (int i = 0; i < 12; i++)
        //    {
        //        writer.WriteInt8(0x20);
        //    }

        //    writer.WriteInt8(0xFF);
        //    writer.WriteInt8(0x00);
        //    writer.WriteInt8(0x06);
        //    writer.WriteInt8(0x26);

        //    PecThread[] threadSet = PecThread.GetThreadSet();
        //    EmbThread[] chart = new EmbThread[threadSet.Count()];
        //    List<PecThread> threads = embroidery.Threads.Cast<PecThread>().ToList();

        //    foreach (var thread in threads)
        //    {
        //        int index = PecThread.FindNearestIndex((int)(uint)thread.Color, threadSet);
        //        threadSet[index] = null;
        //        chart[index] = thread;
        //    }

        //    BinaryWriter colorTemp = new(new MemoryStream());
        //    foreach (var embObject in embroidery.GetAsStitchBlock())
        //    {
        //        colorTemp.WriteInt8(EmbThread.FindNearestIndex((int)(uint)embObject.Item2.Color, threadSet));
        //    }

        //    int currentThreadCount = (int)colorTemp.BaseStream.Length;
        //    if (currentThreadCount != 0)
        //    {
        //        for (int i = 0; i < 12; i++)
        //        {
        //            writer.WriteInt8(0x20);
        //        }

        //        writer.WriteInt8(currentThreadCount - 1);
        //        colorTemp.BaseStream.Seek(0, SeekOrigin.Begin);
        //        writer.Write(colorTemp.BaseStream.ReadFully());
        //    }
        //    else
        //    {
        //        writer.WriteInt8(0x20);
        //        writer.WriteInt8(0x20);
        //        writer.WriteInt8(0x20);
        //        writer.WriteInt8(0x20);
        //        writer.WriteInt8(0x64);
        //        writer.WriteInt8(0x20);
        //        writer.WriteInt8(0x00);
        //        writer.WriteInt8(0x20);
        //        writer.WriteInt8(0x00);
        //        writer.WriteInt8(0x20);
        //        writer.WriteInt8(0x20);
        //        writer.WriteInt8(0x20);
        //        writer.WriteInt8(0xFF);
        //    }

        //    for (int i = 0; i < (463 - currentThreadCount); i++)
        //    {
        //        writer.WriteInt8(0x20);
        //    } //520

        //    writer.WriteInt8(0x00);
        //    writer.WriteInt8(0x00);

        //    int graphicsOffsetValueLocation = (int)writer.BaseStream.Position;

        //    writer.Write(0x00);
        //    writer.Write(0x00);
        //    writer.Write(0x00);

        //    writer.WriteInt8(0x31);
        //    writer.WriteInt8(0xFF);
        //    writer.WriteInt8(0xF0);

        //    /* write 2 byte x size */
        //    writer.WriteInt16LE((short)Math.Round(width));
        //    /* write 2 byte y size */
        //    writer.WriteInt16LE((short)Math.Round(height));

        //    /* Write 4 miscellaneous int16's */
        //    writer.WriteInt8(0x01);
        //    writer.WriteInt8(0xe0);
        //    writer.WriteInt8(0x01);
        //    writer.WriteInt8(0x00);

        //    writer.WriteInt16BE((0x9000 | (int)-Math.Round(minX)));
        //    writer.WriteInt16BE((0x9000 | (int)-Math.Round(minY)));

        //    PecEncode(writer, embroidery);

        //    long graphicsOffsetValue = writer.BaseStream.Position - graphicsOffsetValueLocation + 2;
        //    writer.Seek((int)graphicsOffsetValue, SeekOrigin.Begin);

        //    writer.Write((byte)(graphicsOffsetValueLocation & 0xFF));
        //    writer.Write((byte)((graphicsOffsetValueLocation >> 8) & 0xFF));
        //    writer.Write((byte)((graphicsOffsetValueLocation >> 16) & 0xFF));

        //    writer.Seek(0, SeekOrigin.End);

        //    Buffer.BlockCopy(EmbroideryHelper.EmbroideryHelper.imageWithFrame, 0, image, 0, 48 * 38);

        //    yFactor = 32.0 / height;
        //    xFactor = 42.0 / width;

        //    for (int i = 0; i < embroidery.Stitches.Count; i++)
        //    {
        //        Stitch stitch = embroidery.Stitches[i];
        //        int x = EmbroideryHelper.EmbroideryHelper.EmbRound(stitch.X * xFactor) + 3;
        //        int y = EmbroideryHelper.EmbroideryHelper.EmbRound(stitch.Y * yFactor) + 3;

        //        if (x <= 0 || x > 48) 
        //            continue;
        //        if (y <= 0 || y > 38) 
        //            continue;

        //        image[y, x] = 1;
        //    }

        //    WriteImage(writer, image);

        //    int j = 0;

        //    for(int i = 0; i < embroidery.Threads.Count; i++)
        //    {
        //        Buffer.BlockCopy(EmbroideryHelper.EmbroideryHelper.imageWithFrame, 0, image, 0, 48 * 38);
        //        for(; j < embroidery.Stitches.Count; j++)
        //        {
        //            Stitch stitch = embroidery.Stitches[j];
        //            int x = EmbroideryHelper.EmbroideryHelper.EmbRound(stitch.X * xFactor) + 3;
        //            int y = EmbroideryHelper.EmbroideryHelper.EmbRound(stitch.Y * yFactor) + 3;
        //            if (x <= 0 || x > 48)
        //                continue;
        //            if (y <= 0 || y > 38)
        //                continue;
        //            image[y, x] = 1;
        //            if (stitch.Color != embroidery.Threads[i].Color)
        //                break;
        //        }
        //    }
        //}

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

                if (stitch.Command == Command.Stop)
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
                    writer.Write(new byte[] { 0xFF }, 0, 1);
                }
                else if (deltaX < 63 && deltaX > -64 && deltaY < 63 && deltaY > -64 && (!(stitch.Command == (Command.Jump | Command.Trim))))
                {
                    byte[] saida = new byte[2];
                    if (deltaX < 0)
                    {
                        saida[0] = (byte)(deltaX + 0x80);
                    }
                    else
                    {
                        saida[0] = (byte)deltaX;
                    }
                    if (deltaY < 0)
                    {
                        saida[1] = (byte)(deltaY + 0x80);
                    }
                    else
                    {
                        saida[1] = (byte)deltaY;
                    }
                    writer.Write(saida, 0, 2);
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
            if (file == null)
            {
                Console.WriteLine("ERROR: format-pec.c pecEncodeStop(), file argument is null");
                return;
            }

            file.Write(new byte[] { 0xFE, 0xB0 }, 0, 2);
            file.Write(val);
        }

        public static int EncodeLongForm(int value)
        {
            value &= 0b00001111_11111111;
            value |= 0b10000000_00000000;
            return value;
        }

        public static int FlagJump(int longForm)
        {
            return longForm | (JUMP_CODE << 8);
        }

        public static int FlagTrim(int longForm)
        {
            return longForm | (TRIM_CODE << 8);
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
