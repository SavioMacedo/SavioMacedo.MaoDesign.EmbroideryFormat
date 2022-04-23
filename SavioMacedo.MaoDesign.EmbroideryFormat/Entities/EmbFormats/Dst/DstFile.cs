using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.Enums;
using SavioMacedo.MaoDesign.EmbroideryFormat.Extensions;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Dst
{
    public class DstFile : EmbroideryBasic
    {
        public static DstFile Read(Stream stream, bool allowTransparency, bool hideMachinePath, float threadThickness)
        {
            return Read(stream.ReadFully(), allowTransparency, hideMachinePath, threadThickness);
        }

        public static DstFile Read(byte[] bytes, bool allowTransparency, bool hideMachinePath, float threadThickness)
        {
            DstFile file = new()
            {
                Data = bytes
            };
            BinaryReader reader = new(new MemoryStream(bytes));
            file.ReadHeader(reader);
            file.ReadStitches(reader);
            file.ConvertJumpsToTrim();
            file.FileFormat = FileFormat.Dst;
            file.CreateStitchBlocks();
            file.UpdateSkBitMap(allowTransparency, hideMachinePath, threadThickness);
            file.Threads.RemoveAll(a => !a.FancyLines.Any());
            return file;
        }

        public void ProcessHeaderInfo(string prefix, string value)
        {
            switch (prefix)
            {
                case "LA":
                    {
                        SetMetadata("name", value);
                        break;
                    }
                case "AU":
                    {
                        SetMetadata("author", value);
                        break;
                    }
                case "CP":
                    {
                        SetMetadata("copyright", value);
                        break;
                    }
                case "TC":
                    {
                        string[] values = value.Split(",").Select(a => a.Trim()).ToArray();
                        AddThread(new DstThread(values[0], values[1], values[2]));
                        break;
                    }
                default:
                    {
                        SetMetadata(prefix, value);
                        break;
                    }
            }
        }

        public void ReadHeader(BinaryReader reader)
        {
            byte[] header = reader.ReadBytes(512);
            string headerString = Encoding.UTF8.GetString(header);
            string[] headers = headerString.Split("\r").Select(a => a.Trim()).ToArray();

            foreach (string headerItem in headers)
            {
                if (headerItem.Length > 3)
                    ProcessHeaderInfo(headerItem[0..2].Trim(), headerItem[3..].Trim());
            }

            if (Threads.Count == 0)
            {
                Random rng = new Random();
                List<DstThread> threadList = DstThread.GetThreadSet().OrderBy(a => rng.Next()).ToList();
                Threads.AddRange(threadList);
            }
        }

        public void ReadStitches(BinaryReader reader)
        {
            bool sequinMode = false;
            while (true)
            {
                byte[] bytes = reader.ReadBytes(3);
                if (bytes.Length != 3)
                    break;

                int dx = DecodeDx(bytes[0], bytes[1], bytes[2]);
                int dy = DecodeDy(bytes[0], bytes[1], bytes[2]);

                if ((bytes[2] & 0b11110011) == 0b11110011)
                    Stop(dx, dy);
                else if ((bytes[2] & 0b11000011) == 0b11000011)
                    ColorChange(dx, dy);
                else if ((bytes[2] & 0b01000011) == 0b01000011)
                {
                    SequinMode(dx, dy);
                    sequinMode = !sequinMode;
                }
                else if ((bytes[2] & 0b10000011) == 0b10000011)
                {
                    if (sequinMode)
                        SequinEject(dx, dy);
                    else
                        Move(dx, dy);
                }
                else
                    Stitch(dx, dy);
            }
        }

        private int GetBit(int b, int pos)
        {
            return (b >> pos) & 1;
        }

        private int DecodeDx(byte b0, byte b1, byte b2)
        {
            var x = 0;
            x += GetBit(b2, 2) * (+81);
            x += GetBit(b2, 3) * (-81);
            x += GetBit(b1, 2) * (+27);
            x += GetBit(b1, 3) * (-27);
            x += GetBit(b0, 2) * (+9);
            x += GetBit(b0, 3) * (-9);
            x += GetBit(b1, 0) * (+3);
            x += GetBit(b1, 1) * (-3);
            x += GetBit(b0, 0) * (+1);
            x += GetBit(b0, 1) * (-1);

            return x;
        }

        private int DecodeDy(byte b0, byte b1, byte b2)
        {
            var y = 0;
            y += GetBit(b2, 5) * (+81);
            y += GetBit(b2, 4) * (-81);
            y += GetBit(b1, 5) * (+27);
            y += GetBit(b1, 4) * (-27);
            y += GetBit(b0, 5) * (+9);
            y += GetBit(b0, 4) * (-9);
            y += GetBit(b1, 7) * (+3);
            y += GetBit(b1, 6) * (-3);
            y += GetBit(b0, 7) * (+1);
            y += GetBit(b0, 6) * (-1);

            return -y;
        }
    }
}
