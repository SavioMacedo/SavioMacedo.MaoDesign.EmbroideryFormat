using System.Collections.Generic;
using System.IO;
using System.Linq;
using SavioMacedo.MaoDesign.EmbroideryFormat.EmbroideryHelper;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic;
using SavioMacedo.MaoDesign.EmbroideryFormat.Extensions;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.Enums;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Hus
{
    public class HusFile : EmbroideryBasic
    {
        public static HusFile Read(Stream stream, bool allowTransparency, bool hideMachinePath, float threadThickness)
        {
            return Read(stream.ReadFully(), allowTransparency, hideMachinePath, threadThickness);
        }

        public static HusFile Read(byte[] bytes, bool allowTransparency, bool hideMachinePath, float threadThickness)
        {
            HusFile file = new()
            {
                Data = bytes,
                FileFormat = FileFormat.Hus
            };
            BinaryReader reader = new(new MemoryStream(bytes));

            reader.ReadInt32();
            int numberOfStitches = reader.ReadInt32();
            int numberOfColors = reader.ReadInt32();

            file.Signed16(reader.ReadInt16());
            file.Signed16(reader.ReadInt16());
            file.Signed16(reader.ReadInt16());
            file.Signed16(reader.ReadInt16());
            int commandOffset = reader.ReadInt32();
            int xOffset = reader.ReadInt32();
            int yOffset = reader.ReadInt32();

            file.FileName = reader.ReadString8(8);
            reader.ReadInt16();

            List<HusThread> threads = HusThread.GetHusThreads();

            for (var i = 0; i < numberOfColors; i++)
            {
                int index = reader.ReadInt16();
                file.AddThread(threads[index]);
            }

            reader.BaseStream.Seek(commandOffset, SeekOrigin.Begin);
            byte[] commandCompressed = reader.ReadBytes(xOffset - commandOffset);
            reader.BaseStream.Seek(xOffset, SeekOrigin.Begin);
            byte[] xCompressed = reader.ReadBytes(yOffset - xOffset);
            reader.BaseStream.Seek(yOffset, SeekOrigin.Begin);
            byte[] yCompressed = reader.BaseStream.ReadFully();
            List<int> commandDecompressed = EmbroideryCompress.Expand(commandCompressed, numberOfStitches);
            List<int> xDecompressed = EmbroideryCompress.Expand(xCompressed, numberOfStitches);
            List<int> yDecompressed = EmbroideryCompress.Expand(yCompressed, numberOfStitches);
            int stitchCount = (new int[] { commandDecompressed.Count, xDecompressed.Count, yDecompressed.Count }).Min();

            for (var i = 0; i < stitchCount; i++)
            {
                int cmd = commandDecompressed[i];
                int x = file.Signed8(xDecompressed[i]);
                int y = -file.Signed8(yDecompressed[i]);

                if (cmd == 0x80)
                {
                    file.Stitch(x, y);
                }
                else if (cmd == 0x81)
                {
                    file.Move(x, y);
                }
                else if (cmd == 0x84)
                {
                    file.ColorChange(x, y);
                }
                else if (cmd == 0x88)
                {
                    if (x != 0 || y != 0)
                    {
                        file.Move(x, y);
                    }
                    file.Trim();
                }
                else if (cmd == 0x90)
                {
                    break;
                }
                else
                {
                    file.Stitch(x, y);
                }
            }

            file.End();

            file.CreateStitchBlocks();
            file.UpdateSkBitMap(allowTransparency, hideMachinePath, threadThickness);

            return file;
        }
    }
}
