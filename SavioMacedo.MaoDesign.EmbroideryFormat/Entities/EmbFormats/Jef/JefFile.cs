using System;
using System.IO;
using System.Linq;
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

        public static JefFile Read(EmbroideryBasic embroideryBasic, bool allowTransparency, bool hideMachinePath, float threadThickness)
        {
            byte[] data = Write(embroideryBasic);
            return Read(data, embroideryBasic.FileName, allowTransparency, hideMachinePath, threadThickness);
        }

        public static JefFile Read(byte[] bytes, string fileName, bool allowTransparency, bool hideMachinePath, float threadThickness)
        {
            JefFile file = new()
            {
                Data = bytes,
                FileName = fileName
            };
            BinaryReader reader = new(new MemoryStream(file.Data));
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

        public static byte[] Write(EmbroideryBasic pattern)
        {
            MemoryStream memoryStream = new();
            BinaryWriter writer = new(memoryStream);

            int colorlistSize, minColors, designWidth, designHeight, i;
            double width, height;
            int data;
            double dx, dy;
            double xx = 0.0, yy = 0.0;
            byte[] b = new byte[4];

            pattern.FlipVertical();
            pattern.CorrectForMaxStitchLength((float)127, (float)127);

            colorlistSize = pattern.Threads.Count;
            minColors = Math.Max(colorlistSize, 6);
            writer.WriteInt32Le(0x74 + (colorlistSize * 8));
            writer.WriteInt32Le(0x0A);

            DateTime currentTime = DateTime.Now;
            int year = currentTime.Year;
            int month = currentTime.Month;
            int day = currentTime.Day;
            int hour = currentTime.Hour;
            int minute = currentTime.Minute;
            int second = currentTime.Second;

            string formattedTime = string.Format("{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}",
                                                 year, month, day, hour, minute, second);
            writer.WriteString(formattedTime);

            writer.FPad(0x00, 2);
            writer.WriteInt32Le(pattern.Threads.Count);

            data = pattern.Stitches.Count + pattern.Stitches.Count(a => a.Command == Command.Stop || a.Command == Command.Trim || a.Command == Command.Jump || a.Command == Command.ColorChange);
            writer.WriteInt32Le(data);

            var bounds = pattern.Extents();
            width = pattern.ImageWidth;
            height = pattern.ImageHeight;
            designWidth = (int)(width * 10.0);
            designHeight = (int)(height * 10.0);

            writer.WriteInt32Le(JefGetHoopSize(designWidth, designHeight));

            writer.WriteInt32Le(designWidth / 2);
            writer.WriteInt32Le(designHeight / 2);
            writer.WriteInt32Le(designWidth / 2);
            writer.WriteInt32Le(designHeight / 2);

            if (Math.Min(550 - designWidth / 2, 550 - designHeight / 2) >= 0)
            {
                writer.WriteInt32Le(Math.Max(-1, 550 - designWidth / 2));
                writer.WriteInt32Le(Math.Max(-1, 550 - designHeight / 2));
                writer.WriteInt32Le(Math.Max(-1, 550 - designWidth / 2));
                writer.WriteInt32Le(Math.Max(-1, 550 - designHeight / 2));
            }
            else
            {
                writer.WriteInt32Le(-1);
                writer.WriteInt32Le(-1);
                writer.WriteInt32Le(-1);
                writer.WriteInt32Le(-1);
            }

            if (Math.Min(250 - designWidth / 2, 250 - designHeight / 2) >= 0)
            {
                writer.WriteInt32Le(Math.Max(-1, 250 - designWidth / 2));
                writer.WriteInt32Le(Math.Max(-1, 250 - designHeight / 2));
                writer.WriteInt32Le(Math.Max(-1, 250 - designWidth / 2));
                writer.WriteInt32Le(Math.Max(-1, 250 - designHeight / 2));
            }
            else
            {
                writer.WriteInt32Le(-1);
                writer.WriteInt32Le(-1);
                writer.WriteInt32Le(-1);
                writer.WriteInt32Le(-1);
            }

            writer.WriteInt32Le((700 - designWidth / 2));
            writer.WriteInt32Le((1000 - designHeight / 2));
            writer.WriteInt32Le((700 - designWidth / 2));
            writer.WriteInt32Le((1000 - designHeight / 2));

            writer.WriteInt32Le((630 - designWidth / 2));
            writer.WriteInt32Le((550 - designHeight / 2));
            writer.WriteInt32Le((630 - designWidth / 2));
            writer.WriteInt32Le((550 - designHeight / 2));

            for (i = 0; i < pattern.Threads.Count; i++)
            {
                int color = EmbThread.FindNearestIndex(pattern.Threads[i].Color, JefThread.GetThreadSet());
                writer.WriteInt32Le(color);
            }

            for (i = 0; i < colorlistSize; i++)
            {
                writer.WriteInt32Le(0x0D);
            }

            foreach(Stitch stitch in pattern.Stitches)
            {
                float x = stitch.X;
                float y = stitch.Y;
                dx = EmbroideryHelper.EmbroideryHelper.EmbRound(x - xx);
                dy = EmbroideryHelper.EmbroideryHelper.EmbRound(y - yy);
                xx = x;
                yy = y;
                byte dxByte = (byte)(dx < 0 ? dx + 256 : dx);
                byte dyByte = (byte)(dy < 0 ? dy + 256 : dy);
                JefEncode(b, dxByte, dyByte, stitch.Command);

                writer.Write(b[0]);
                writer.Write(b[1]);
                if ((b[0] == 0x80) && ((b[1] == 1) || (b[1] == 2) || (b[1] == 4) || (b[1] == 0x10)))
                {
                    writer.Write(b[2]);
                    writer.Write(b[3]);
                }
            }

            return memoryStream.ToArray();
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

        static void JefEncode(byte[] b, byte dx, byte dy, Command command)
        {
            if (command == Command.ColorChange || command == Command.Stop)
            {
                b[0] = 0x80;
                b[1] = 1;
                b[2] = dx;
                b[3] = dy;
            }
            else if (command == Command.End)
            {
                b[0] = 0x80;
                b[1] = 0x10;
                b[2] = 0;
                b[3] = 0;
            }
            else if (command == Command.Trim || command == Command.Jump)
            {
                b[0] = 0x80;
                b[1] = 2;
                b[2] = dx;
                b[3] = dy;
            }
            else
            {
                b[0] = dx;
                b[1] = dy;
            }
        }


        static int JefGetHoopSize(int width, int height)
        {
            if (width < 50 && height < 50)
            {
                return 2;
            }
            if (width < 110 && height < 110)
            {
                return 1;
            }
            if (width < 140 && height < 200)
            {
                return 3;
            }
            return 1;
        }

    }
}
