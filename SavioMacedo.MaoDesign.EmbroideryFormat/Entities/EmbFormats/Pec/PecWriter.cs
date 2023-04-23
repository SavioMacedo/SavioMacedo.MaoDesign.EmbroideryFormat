using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Pec
{
    public class PecWriter : EmbBasicWriter
    {
        static readonly int MASK_07_BIT = 0b01111111;
        static readonly int JUMP_CODE = 0b00010000;
        static readonly int TRIM_CODE = 0b00100000;
        static readonly int FLAG_LONG = 0b10000000;

        static readonly int PEC_ICON_WIDTH = 48;
        static readonly int PEC_ICON_HEIGHT = 38;

        public override void PreWrite(EmbroideryBasic embroidery)
        {
            TransCode t = TransCode.GetTransCode();
            float maxX;
            float minX;
            float maxY;
            float minY;
            (minX, minY, maxX, maxY) = embroidery.Extents();

            t.SetInitialPosition((int)((minX + maxX) / 2), (int)((minY + maxY) / 2));
            t.require_jumps = true;
            t.SplitLongJumps = true;
            t.MaxJumpLength = 2047;
            t.MaxStitchLength = 2047;
            t.Snap = true;
            t.FixColorCount = true;

            if (IsLockStitches())
            {
                t.Tie_on = true;
                t.Tie_off = true;
            }
            t.Transcode(embroidery);
        }

        public override void Write(EmbroideryBasic embroideryBasic)
        {
            stream = new BinaryWriter(new MemoryStream());
            Write("#PEC0001");
            WritePecStitches(embroideryBasic.FileName);
        }

        public void WritePecStitches(string fileName)
        {
            Write("LA:");
            if (fileName.Length > 16)
            {
                fileName = fileName[..8];
            }

            Write(fileName);
            for (int i = 0; i < (16 - fileName.Length); i++)
            {
                WriteInt8(0x20);
            }

            WriteInt8(0x0D);
            for (int i = 0; i < 12; i++)
            {
                WriteInt8(0x20);
            }

            WriteInt8(0xFF);
            WriteInt8(0x00);
            WriteInt8(0x06);
            WriteInt8(0x26);
        }

        public void WritePecStitchess(string fileName)
        {
            float maxX;
            float minX;
            float maxY;
            float minY;
            (minX, minY, maxX, maxY) = embroideryBasic.Extents();

            float width = maxX - minX;
            float height = maxY - minY;

            fileName ??= "untitled";

            Write("LA:");
            if (fileName.Length > 16)
            {
                fileName = fileName.Substring(0, 8);
            }

            Write(fileName);
            for (int i = 0; i < (16 - fileName.Length); i++)
            {
                WriteInt8(0x20);
            }

            WriteInt8(0x0D);
            for (int i = 0; i < 12; i++)
            {
                WriteInt8(0x20);
            }

            WriteInt8(0xFF);
            WriteInt8(0x00);
            WriteInt8(0x06);
            WriteInt8(0x26);

            PecThread[] threadSet = PecThread.GetThreadSet();
            EmbThread[] chart = new EmbThread[threadSet.Length];

            List<EmbThread> threads = embroideryBasic.GetUniqueThreadList();
            foreach (EmbThread thread in threads)
            {
                int index = EmbThread.FindNearestIndex((int)(uint)thread.Color, threadSet);
                threadSet[index] = null;
                chart[index] = thread;
            }

            BinaryWriter colorTempArray = new(new MemoryStream());
            Push(colorTempArray);


            foreach (var embObject in embroideryBasic.GetAsStitchBlock())
            {
                WriteInt8(EmbThread.FindNearestIndex((int)(uint)embObject.Item2.Color, chart));
            }

            Pop();
            int currentThreadCount = (int)colorTempArray.BaseStream.Length;
            if (currentThreadCount != 0)
            {
                for (int i = 0; i < 12; i++)
                {
                    WriteInt8(0x20);
                }
                //56

                WriteInt8(currentThreadCount - 1);
                var memoryTemp = new MemoryStream();
                colorTempArray.BaseStream.CopyTo(memoryTemp);
                Write(memoryTemp.ToArray());
            }
            else
            {
                WriteInt8(0x20);
                WriteInt8(0x20);
                WriteInt8(0x20);
                WriteInt8(0x20);
                WriteInt8(0x64);
                WriteInt8(0x20);
                WriteInt8(0x00);
                WriteInt8(0x20);
                WriteInt8(0x00);
                WriteInt8(0x20);
                WriteInt8(0x20);
                WriteInt8(0x20);
                WriteInt8(0xFF);
            }

            for (int i = 0; i < (463 - currentThreadCount); i++)
            {
                WriteInt8(0x20);
            } //520

            WriteInt8(0x00);
            WriteInt8(0x00);

            MemoryStream tempMemoryStream = new();
            BinaryWriter tempArray = new(new MemoryStream());
            Push(tempArray);
            PecEncode();
            Pop();

            int graphicsOffsetValue = (int)tempArray.BaseStream.Length + 20; //10 //15 //17
            WriteInt24LE(graphicsOffsetValue);

            WriteInt8(0x31);
            WriteInt8(0xFF);
            WriteInt8(0xF0);

            /* write 2 byte x size */
            WriteInt16LE((short)Math.Round(width));
            /* write 2 byte y size */
            WriteInt16LE((short)Math.Round(height));

            /* Write 4 miscellaneous int16's */
            WriteInt16LE((short)0x1E0);
            WriteInt16LE((short)0x1B0);

            WriteInt16BE((0x9000 | (int)-Math.Round(minX)));
            WriteInt16BE((0x9000 | (int)-Math.Round(minY)));
            tempArray.BaseStream.CopyTo(tempMemoryStream);
            stream.Write(tempMemoryStream.ToArray());

            PecGraphics graphics = new(minX, minY, maxX, maxY, PEC_ICON_WIDTH, PEC_ICON_HEIGHT);

            foreach(var embObject in embroideryBasic.GetAsStitchBlock())
            {
                graphics.Draw(embObject.Item1);
            }

            Write(graphics.GetGraphics());
            graphics.Clear();

            int lastcolor = 0;
            foreach(var embLayer in embroideryBasic.GetAsStitchBlock())
            {
                int currentcolor = (int)(uint)embLayer.Item2.Color;
                if ((lastcolor != 0) && (currentcolor != lastcolor))
                {
                    Write(graphics.GetGraphics());
                    graphics.Clear();
                }
                graphics.Draw(embLayer.Item1);
                lastcolor = currentcolor;
            }
            Write(graphics.GetGraphics());
        }

        private void PecEncode()
        {
            bool colorchangeJump = false;
            bool colorTwo = true;
            IEnumerable<Stitch> stitches = embroideryBasic.Stitches;
            int deltaX, deltaY;
            bool jumping = false;
            for (int i = 0, ie = stitches.Count(); i < ie; i++)
            {
                var stitch = stitches.ElementAtOrDefault(i);
                switch (stitch.Command)
                {
                    case Command.Stitch:
                        if (jumping)
                        {
                            stream.Write((byte)0x00);
                            stream.Write((byte)0x00);
                            jumping = false;
                        }
                        deltaX = (int)Math.Round(stitch.X);
                        deltaY = (int)Math.Round(stitch.Y);
                        if (deltaX < 63 && deltaX > -64 && deltaY < 63 && deltaY > -64)
                        {
                            stream.Write(deltaX & MASK_07_BIT);
                            stream.Write(deltaY & MASK_07_BIT);
                        }
                        else
                        {
                            deltaX = EncodeLongForm(deltaX);
                            stream.Write((deltaX >> 8) & 0xFF);
                            stream.Write(deltaX & 0xFF);

                            deltaY = EncodeLongForm(deltaY);
                            stream.Write((deltaY >> 8) & 0xFF);
                            stream.Write(deltaY & 0xFF);
                        }
                        break;
                    case Command.Jump:
                        jumping = true;
                        //if (index != 0) {
                        deltaX = (int)Math.Round(stitch.X);
                        deltaX = EncodeLongForm(deltaX);
                        if (colorchangeJump)
                        {
                            deltaX = FlagJump(deltaX);
                        }
                        else
                        {
                            deltaX = FlagTrim(deltaX);
                        }

                        stream.Write((deltaX >> 8) & 0xFF);
                        stream.Write(deltaX & 0xFF);

                        deltaY = (int)Math.Round(stitch.Y);
                        deltaY = EncodeLongForm(deltaY);
                        if (colorchangeJump)
                        {
                            deltaY = FlagJump(deltaY);
                        }
                        else
                        {
                            deltaY = FlagTrim(deltaY);
                        }

                        stream.Write((deltaY >> 8) & 0xFF);
                        stream.Write(deltaY & 0xFF);
                        colorchangeJump = false;
                        //}
                        break;
                    case Command.ColorChange: //prejump
                        if (jumping)
                        {
                            stream.Write((byte)0x00);
                            stream.Write((byte)0x00);
                            jumping = false;
                        }
                        //if (previousColor != 0) {
                        stream.Write(0xfe);
                        stream.Write(0xb0);
                        stream.Write((colorTwo) ? 2 : 1);
                        colorTwo = !colorTwo;
                        colorchangeJump = true;
                        //}
                        break;
                    case Command.Stop:
                        if (jumping)
                        {
                            stream.Write((byte)0x00);
                            stream.Write((byte)0x00);
                            jumping = false;
                        }
                        stream.Write((byte)0x80);
                        stream.Write((byte)0x1);
                        stream.Write((byte)0x00);
                        stream.Write((byte)0x00);
                        break;
                    case Command.End:
                        if (jumping)
                        {
                            stream.Write((byte)0x00);
                            stream.Write((byte)0x00);
                            jumping = false;
                        }
                        stream.Write(0xff);
                        break;
                }
            }
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
    }
}
