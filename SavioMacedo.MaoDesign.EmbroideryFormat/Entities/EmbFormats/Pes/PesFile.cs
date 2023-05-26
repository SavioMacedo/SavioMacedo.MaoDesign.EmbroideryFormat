using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.Enums;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Pec;
using SavioMacedo.MaoDesign.EmbroideryFormat.Extensions;
using SkiaSharp;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Pes
{
    public class PesFile : PecFile
    {
        public static new PesFile Read(Stream stream, bool allowTransparency, bool hideMachinePath, float threadThickness)
        {
            return Read(stream.ReadFully(), allowTransparency, hideMachinePath, threadThickness);
        }

        public static new PesFile Read(byte[] bytes, bool allowTransparency, bool hideMachinePath, float threadThickness)
        {
            PesFile file = new()
            {
                Data = bytes
            };
            BinaryReader reader = new(new MemoryStream(file.Data));
            List<PecThread> pecThread = new();
            string pecString = reader.ReadString8(8);

            if (pecString.Equals("#PEC0001"))
            {
                file.ReadPec(reader, pecThread);
                file.ConvertDuplicateColorChangeToStop();
                return file;
            }

            int pecBlockPosition = reader.ReadInt32();

            switch (pecString)
            {
                case "#PES0060":
                    {
                        file.ReadPesHeaderVersion6(reader, pecThread);
                        file.SetMetadata("version", "6");
                        break;
                    }
                case "#PES0050":
                    {
                        file.ReadPesHeaderVersion5(reader, pecThread);
                        file.SetMetadata("version", "5");
                        break;
                    }
                case "#PES0055":
                    {
                        file.ReadPesHeaderVersion5(reader, pecThread);
                        file.SetMetadata("version", "5.5");
                        break;
                    }
                case "#PES0056":
                    {
                        file.ReadPesHeaderVersion5(reader, pecThread);
                        file.SetMetadata("version", "5.6");
                        break;
                    }
                case "#PES0040":
                    {
                        file.ReadPesHeaderVersion4(reader);
                        file.SetMetadata("version", "4");
                        break;
                    }
                case "#PES0001":
                    {
                        file.SetMetadata("version", "1");
                        break;
                    }
            }

            reader.BaseStream.Seek(pecBlockPosition, SeekOrigin.Begin);
            file.ReadPec(reader, pecThread);
            file.ConvertDuplicateColorChangeToStop();
            file.FileFormat = FileFormat.Pes;
            file.ToSkiaFormat();
            file.UpdateSkBitMap(allowTransparency, hideMachinePath, threadThickness);
            return file;
        }

        public static new PesFile Read(EmbroideryBasic embroidery, bool allowTransparency, bool hideMachinePath, float threadThickness)
        {
            return Read(Write(embroidery), allowTransparency, hideMachinePath, threadThickness);
        }

        public static new byte[] Write(EmbroideryBasic embroideryBasic)
        {
            var (minX, minY, maxX, maxY) = embroideryBasic.Extents();

            float pattern_left = minX;
            float pattern_top = minY;
            float pattern_right = maxX;
            float pattern_bottom = maxY;
            float cx = (pattern_left + pattern_right) / 2;
            float cy = (pattern_top + pattern_bottom) / 2;

            pattern_left -= cx;
            pattern_right -= cx;
            pattern_top -= cy;
            pattern_bottom -= cy;

            MemoryStream memoryStream = new();
            BinaryWriter writer = new(memoryStream);

            MemoryStream memoryStreamHeader = new();
            BinaryWriter binaryWriter = new(memoryStreamHeader);

            WritePesHeaderV1(1, binaryWriter);
            binaryWriter.WriteInt16BE(0xFFFF);
            binaryWriter.WriteInt16LE(0x0000);
            WritePesBlocks(binaryWriter, embroideryBasic, pattern_left, pattern_top, pattern_right, pattern_bottom);

            writer.WriteString("#PES0001");
            int pecLocation = (int)("#PES0001".Length + binaryWriter.BaseStream.Length + 4);
            writer.WriteInt32Le(pecLocation);
            writer.Write(memoryStreamHeader.ToArray());
            WritePecStitches(embroideryBasic, writer);

            return memoryStream.ToArray();
        }

        public void ConvertDuplicateColorChangeToStop()
        {
            PesFile embroideryBasic = new PesFile();
            embroideryBasic.AddThread(GetThreadOrFiller(0));
            int threadIndex = 0;
            foreach (Stitch stitch in Stitches)
            {
                if (stitch.Command == Command.ColorChange || stitch.Command == Command.ColorBreak)
                {
                    threadIndex += 1;
                    EmbThread thread = GetThreadOrFiller(threadIndex);
                    if (thread == embroideryBasic.Threads.LastOrDefault())
                    {
                        embroideryBasic.Stop();
                    }
                    else
                    {
                        embroideryBasic.ColorChange();
                        embroideryBasic.AddThread(thread);
                    }
                }
                else
                {
                    embroideryBasic.AddStitchAbsolute(stitch.Command, stitch.X, stitch.Y);
                }
            }

            Stitches = embroideryBasic.Stitches;
            Threads = embroideryBasic.Threads;
        }

        private void ReadPesThread(BinaryReader reader, List<PecThread> threads)
        {
            PecThread thread = new PecThread();
            thread.CatalogNumber = reader.ReadPesString();
            thread.Color = new SKColor((uint)(0xFF000000 | reader.ReadInt24Be()));
            reader.BaseStream.Seek(5, SeekOrigin.Current);
            thread.Description = reader.ReadPesString();
            thread.Brand = reader.ReadPesString();
            thread.Chart = reader.ReadPesString();
            threads.Add(thread);
        }

        private void ReadMetadata(BinaryReader reader)
        {
            string value = reader.ReadPesString();
            if (!string.IsNullOrEmpty(value))
                SetMetadata("name", value);
            value = reader.ReadPesString();
            if (!string.IsNullOrEmpty(value))
                SetMetadata("category", value);
            value = reader.ReadPesString();
            if (!string.IsNullOrEmpty(value))
                SetMetadata("author", value);
            value = reader.ReadPesString();
            if (!string.IsNullOrEmpty(value))
                SetMetadata("keywords", value);
            value = reader.ReadPesString();
            if (!string.IsNullOrEmpty(value))
                SetMetadata("comments", value);
        }

        private void ReadPesHeaderVersion6(BinaryReader reader, List<PecThread> threadList)
        {
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            ReadMetadata(reader);
            reader.BaseStream.Seek(36, SeekOrigin.Current);
            string imageFile = reader.ReadPesString();

            if (!string.IsNullOrEmpty(imageFile))
                SetMetadata("image_file", imageFile);

            reader.BaseStream.Seek(24, SeekOrigin.Current);
            int countProgrammableFills = reader.ReadInt16();
            if (countProgrammableFills != 0)
            {
                return;
            }

            int countMotifs = reader.ReadInt16();
            if (countMotifs != 0)
            {
                return;
            }

            int countFeatherPatterns = reader.ReadInt16();
            if (countFeatherPatterns != 0)
            {
                return;
            }

            int countThreads = reader.ReadInt16();
            for (var i = 0; i < countThreads; i++)
            {
                ReadPesThread(reader, threadList);
            }
        }

        private void ReadPesHeaderVersion5(BinaryReader reader, List<PecThread> threadList)
        {
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            ReadMetadata(reader);
            reader.BaseStream.Seek(24, SeekOrigin.Current);
            string image = reader.ReadPesString();
            if (!string.IsNullOrEmpty(image))
            {
                SetMetadata("image", image);
            }

            reader.BaseStream.Seek(24, SeekOrigin.Current);
            int countProgrammableFills = reader.ReadInt16();
            if (countProgrammableFills != 0)
            {
                return;
            }

            int countMotifs = reader.ReadInt16();
            if (countMotifs != 0)
            {
                return;
            }

            int countFeatherPatherns = reader.ReadInt16();
            if (countFeatherPatherns != 0)
            {
                return;
            }

            int countThreads = reader.ReadInt16();
            for (var i = 0; i < countThreads; i++)
            {
                ReadPesThread(reader, threadList);
            }
        }

        private void ReadPesHeaderVersion4(BinaryReader reader)
        {
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            ReadMetadata(reader);
        }

        public static void WritePesHeaderV1(int distinctBlockObjects, BinaryWriter writer)
        {
            writer.WriteInt16LE(0x01); // 1 is scale to fit
            writer.WriteInt16LE(0x01); // 0 = 100x100 else 130x180 or above
            writer.WriteInt16LE(value: (short)distinctBlockObjects); // number of distinct blocks
        }

        public static void WritePesBlocks(BinaryWriter writer, EmbroideryBasic pattern, float pattern_left, float pattern_top, float pattern_right, float pattern_bottom)
        {
            if (pattern.Stitches.Count > 0)
            {
                WritePesString16(writer, "CEmbOne");
                float height = pattern_bottom - pattern_top;
                float width = pattern_right - pattern_left;
                int hoopHeight = 1800, hoopWidth = 1300;
                writer.WriteInt16LE(0);
                writer.WriteInt16LE(0);
                writer.WriteInt16LE(0);
                writer.WriteInt16LE(0);
                writer.WriteInt16LE(0);
                writer.WriteInt16LE(0);
                writer.WriteInt16LE(0);
                writer.WriteInt16LE(0);
                float transX = 0;
                float transY = 0;
                transX += 350f;
                transY += 100f + height;
                transX += hoopWidth / 2;
                transY += hoopHeight / 2;
                transX += -width / 2;
                transY += -height / 2;
                writer.WriteInt32Le((int)1f);
                writer.WriteInt32Le((int)0f);
                writer.WriteInt32Le((int)0f);
                writer.WriteInt32Le((int)1f);
                writer.WriteInt32Le((int)transX);
                writer.WriteInt32Le((int)transY);
                writer.WriteInt16LE(1);
                writer.WriteInt16LE(0);
                writer.WriteInt16LE(0);
                writer.WriteInt16LE((short)width);
                writer.WriteInt16LE((short)height);
                writer.WriteInt32Le(0);
                writer.WriteInt32Le(0);
                writer.WriteInt16LE((short)(pattern.GetSegmentCount() + (pattern.CountColorChanges() * 2)));

                writer.WriteInt16BE(0xFFFF);
                writer.WriteInt16LE(0x0000);

                WritePesString16(writer, "CSewSeg");

                using MemoryStream colorlog = new();
                using BinaryWriter binaryWriter = new(colorlog);
                int section = 0;
                int colorCode = -1;
                EmbThread previousThread = null;
                var obj = new SectionEmbObjects(pattern);

                while(obj.MoveNext())
                {
                    var objStitch = obj.Current;
                    EmbThread currentThread = objStitch.Item1;
                    bool colorchange = (currentThread != previousThread);
                    Stitch[] stitches = objStitch.Item2;
                    if (objStitch.Item3 == 0)
                    {
                        colorCode = EmbThread.FindNearestIndex(currentThread.Color, PecThread.GetThreadSet());

                        if (previousThread != null)
                        {
                            int lastCC = EmbThread.FindNearestIndex(previousThread.Color, PecThread.GetThreadSet());

                            writer.WriteInt16LE(0x0);
                            writer.WriteInt16LE((short)lastCC);
                            writer.WriteInt16LE(0x1);
                            WritePosition(writer, pattern_left, pattern_top, pattern_right, pattern_bottom, stitches[0].X, stitches[0].Y);
                            writer.WriteInt16BE(0x8003);
                            section++;

                            binaryWriter.WriteInt16LE((short)section);
                            binaryWriter.WriteInt16LE((short)colorCode);

                            writer.WriteInt16LE(0x1);
                            writer.WriteInt16LE((short)colorCode);
                            writer.WriteInt16LE(0x2);
                            WritePosition(writer, pattern_left, pattern_top, pattern_right, pattern_bottom, stitches[0].X, stitches[0].Y);
                            WritePosition(writer, pattern_left, pattern_top, pattern_right, pattern_bottom, stitches[0].X, stitches[0].Y);
                            writer.WriteInt16BE(0x8003);
                            section++;
                        }
                        else
                        {
                            writer.WriteInt16LE(0x1);
                            writer.WriteInt16LE((short)colorCode);
                            writer.WriteInt16LE(0x2);
                            WritePosition(writer, pattern_left, pattern_top, pattern_right, pattern_bottom, stitches[0].X, stitches[0].Y);
                            WritePosition(writer, pattern_left, pattern_top, pattern_right, pattern_bottom, stitches[0].X, stitches[0].Y);
                            writer.WriteInt16BE(0x8003);
                            section++;
                            writer.WriteInt16LE(0x0);
                            writer.WriteInt16LE((short)colorCode);
                            writer.WriteInt16LE(0x2);
                            WritePosition(writer, pattern_left, pattern_top, pattern_right, pattern_bottom, stitches[0].X, stitches[0].Y);
                            WritePosition(writer, pattern_left, pattern_top, pattern_right, pattern_bottom, stitches[0].X, stitches[0].Y);
                            writer.WriteInt16BE(0x8003);
                            section++;
                        }
                        writer.WriteInt16LE(0);
                        writer.WriteInt16LE((short)colorCode);
                        writer.WriteInt16LE((short)stitches.Length);
                        for (int i = 0; i < stitches.Length; i++)
                        {
                            WritePosition(writer, pattern_left, pattern_top, pattern_right, pattern_bottom, stitches[i].X, stitches[i].Y);
                        }
                        writer.WriteInt16BE(0x8003);
                        section++;
                    }
                    if (objStitch.Item3 == 1)
                    {
                        if (colorchange && (previousThread == null))
                        {
                            colorCode = EmbThread.FindNearestIndex(currentThread.Color, PecThread.GetThreadSet());
                            
                            binaryWriter.WriteInt16LE((short)section);
                            binaryWriter.WriteInt16LE((short)colorCode);
                        }
                        writer.WriteInt16LE(1);
                        writer.WriteInt16LE((short)colorCode);
                        writer.WriteInt16LE((short)stitches.Length);
                        for (int i = 0; i < stitches.Length; i++)
                        {
                            WritePosition(writer, pattern_left, pattern_top, pattern_right, pattern_bottom, stitches[i].X, stitches[i].Y);
                        }
                        writer.WriteInt16BE(0x8003);
                        section++;
                    }
                    previousThread = currentThread;
                }
                int count = (int)(colorlog.Length / 4);
                writer.WriteInt16LE((short)count);

                writer.Write(colorlog.ToArray());
                writer.WriteInt16LE(0x0000);
                writer.WriteInt16LE(0x0000);
            }
        }

        private static void WritePesString16(BinaryWriter writer, string v)
        {
            writer.WriteInt16LE((short)v.Length);
            writer.Write(Encoding.ASCII.GetBytes(v));
        }

        private static void WritePosition(BinaryWriter writer, float left, float top, float right, float bottom, float x, float y)
        {
            writer.WriteInt16LE((short)(x - left));
            writer.WriteInt16LE((short)(y - bottom));
        }
    }
}
