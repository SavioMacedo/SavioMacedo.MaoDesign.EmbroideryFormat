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

        public override void BinaryWrite()
        {
            MemoryStream memoryStream = new();
            BinaryWriter streamWriter = new(memoryStream, Encoding.UTF8);
            string version = GetMetadata("version");
            bool isTruncated = bool.Parse(GetMetadata("truncated"));

            if(isTruncated)
            {
                if(version == "1")
                {
                    WriteVersion1(streamWriter);
                }
            }


        }

        private void WriteVersion1(BinaryWriter streamWriter)
        {
            var threadSet = PecThread.GetThreadSet();
            
            streamWriter.Write("#PES0001");
            streamWriter.Flush();

            var extends = Extents();
            var cx = (extends.Item3 + extends.Item1) / 2;
            var cy = (extends.Item4+ extends.Item2) / 2;

            var left = extends.Item1 - cx;
            var top = extends.Item2 - cy;
            var right = extends.Item3 - cx;
            var bottom = extends.Item4 - cy;
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

        private void FillThread(int colorIndex, List<FancyLine> tempStitches)
        {
            PecThread thread = (PecThread)Threads[colorIndex];
            thread.FancyLines.AddRange(tempStitches);
            tempStitches.Clear();
        }
    }
}
