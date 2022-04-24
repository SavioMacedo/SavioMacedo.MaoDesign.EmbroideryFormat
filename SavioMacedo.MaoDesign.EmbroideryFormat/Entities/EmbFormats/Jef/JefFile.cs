using System;
using System.IO;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.Enums;
using SavioMacedo.MaoDesign.EmbroideryFormat.Extensions;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Jef
{
    public class JefFile : EmbroideryBasic
    {
        public static JefFile Read(Stream stream, string fileName, bool allowTransparency, bool hideMachinePath, float threadThickness)
        {
            return Read(stream.ReadFully(), fileName, allowTransparency, hideMachinePath, threadThickness);
        }

        public static JefFile Read(byte[] bytes, string fileName, bool allowTransparency, bool hideMachinePath, float threadThickness)
        {
            JefFile file = new()
            {
                Data = bytes,
                FileName = fileName
            };
            BinaryReader reader = new BinaryReader(new MemoryStream(file.Data));
            JefThread[] jefThreads = JefThread.GetThreadSet();
            int stitchOffset = reader.ReadInt32();
            reader.BaseStream.Seek(20, SeekOrigin.Current);
            int countColors = reader.ReadInt32();
            reader.BaseStream.Seek(88, SeekOrigin.Current);

            for (var i = 0; i < countColors; i++)
            {
                int index = Math.Abs(reader.ReadInt32());
                file.AddThread(jefThreads[index % jefThreads.Length]);
            }

            reader.BaseStream.Seek(stitchOffset, SeekOrigin.Begin);
            file.ReadJefStitches(reader);
            file.ConvertJumpsToTrim();
            file.FileFormat = FileFormat.Jef;
            file.ToSkiaFormat();
            file.UpdateSkBitMap(allowTransparency, hideMachinePath, threadThickness);
            return file;
        }

        public void ReadJefStitches(BinaryReader reader)
        {
            int count = 0;
            while (true)
            {
                count++;
                byte[] b = reader.ReadBytes(2);
                int x, y;

                if (b.Length != 2)
                {
                    break;
                }

                if (b[0] != 0x80)
                {
                    x = Signed8(b[0]);
                    y = -Signed8(b[1]);
                    Stitch(x, y);
                    continue;
                }

                int ctrl = b[1];
                b = reader.ReadBytes(2);
                if (b.Length != 2)
                {
                    break;
                }

                x = Signed8(b[0]);
                y = -Signed8(b[1]);

                if (ctrl == 0x02)
                {
                    if (x == 0 && y == 0)
                    {
                        Stitch(x, y);
                    }
                    else
                    {
                        Stitch(x, y);
                    }

                    continue;
                }

                if (ctrl == 0x01)
                {
                    ColorChange(0, 0);
                    continue;
                }

                if (ctrl == 0x10)
                {
                    break;
                }
            }
            End(0, 0);
        }
    }
}
