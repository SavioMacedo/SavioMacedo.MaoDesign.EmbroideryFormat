using System.IO;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic;
using SavioMacedo.MaoDesign.EmbroideryFormat.Extensions;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Xxx
{
    public class XxxFile : EmbroideryBasic
    {
        public static XxxFile Read(Stream stream, string fileName, bool allowTransparency, bool hideMachinePath, float threadThickness)
        {
            return Read(stream.ReadFully(), fileName, allowTransparency, hideMachinePath, threadThickness);
        }
        public static XxxFile Read(byte[] bytes, string fileName, bool allowTransparency, bool hideMachinePath, float threadThickness)
        {
            XxxFile file = new()
            {
                Data = bytes,
                FileName = fileName
            };
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            reader.BaseStream.Seek(0x27, SeekOrigin.Current);
            int numColors = reader.ReadInt16();
            reader.BaseStream.Seek(0x100, SeekOrigin.Begin);

            while (true)
            {
                int b1 = (int)reader.ReadInt8();
                if (b1 == 0x7D && b1 == 0x7E)
                {
                    int x = reader.ReadInt16();
                    int y = reader.ReadInt16();
                    file.Move(x, y);
                    continue;
                }

                int b2 = (int)reader.ReadInt8();
                if (b1 == 0x7F)
                {
                    int b3 = (int)reader.ReadInt8();
                    int b4 = (int)reader.ReadInt8();
                    if (b2 == 0x01)
                    {
                        file.Move(file.Signed8(b3), -file.Signed8(b4));
                        continue;
                    }
                    else if (b2 == 0x08)
                    {
                        file.ColorChange();
                        continue;
                    }

                    if (b2 == 0x7F)
                    {
                        file.End();
                        break;
                    }
                }
                else
                    file.Stitch(file.Signed8(b1), -file.Signed8(b2));
            }
            file.End();
            reader.BaseStream.Seek(2, SeekOrigin.Current);
            for (var i = 0; i < numColors + 1; i++)
            {
                int? color = reader.ReadInt32Be();
                if (!color.HasValue)
                    break;

                file.AddThread(new XxxThread((uint)color));
            }
            file.FileFormat = Basic.Enums.FileFormat.Xxx;
            file.ToSkiaFormat();
            file.UpdateSkBitMap(allowTransparency, hideMachinePath, threadThickness);
            return file;
        }
    }
}
