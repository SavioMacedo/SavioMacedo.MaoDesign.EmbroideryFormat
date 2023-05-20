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
                Write(" ");
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

        private void PecEncode(BinaryWriter writer)
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
                            writer.Write((byte)0x00);
                            writer.Write((byte)0x00);
                            jumping = false;
                        }
                        deltaX = (int)Math.Round(stitch.X);
                        deltaY = (int)Math.Round(stitch.Y);
                        if (deltaX < 63 && deltaX > -64 && deltaY < 63 && deltaY > -64)
                        {
                            writer.Write(deltaX & MASK_07_BIT);
                            writer.Write(deltaY & MASK_07_BIT);
                        }
                        else
                        {
                            deltaX = EncodeLongForm(deltaX);
                            writer.Write((deltaX >> 8) & 0xFF);
                            writer.Write(deltaX & 0xFF);

                            deltaY = EncodeLongForm(deltaY);
                            writer.Write((deltaY >> 8) & 0xFF);
                            writer.Write(deltaY & 0xFF);
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

                        writer.Write((deltaX >> 8) & 0xFF);
                        writer.Write(deltaX & 0xFF);

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

                        writer.Write((deltaY >> 8) & 0xFF);
                        writer.Write(deltaY & 0xFF);
                        colorchangeJump = false;
                        //}
                        break;
                    case Command.ColorChange: //prejump
                        if (jumping)
                        {
                            writer.Write((byte)0x00);
                            writer.Write((byte)0x00);
                            jumping = false;
                        }
                        //if (previousColor != 0) {
                        writer.Write(0xfe);
                        writer.Write(0xb0);
                        writer.Write((colorTwo) ? 2 : 1);
                        colorTwo = !colorTwo;
                        colorchangeJump = true;
                        //}
                        break;
                    case Command.Stop:
                        if (jumping)
                        {
                            writer.Write((byte)0x00);
                            writer.Write((byte)0x00);
                            jumping = false;
                        }
                        writer.Write((byte)0x80);
                        writer.Write((byte)0x1);
                        writer.Write((byte)0x00);
                        writer.Write((byte)0x00);
                        break;
                    case Command.End:
                        if (jumping)
                        {
                            writer.Write((byte)0x00);
                            writer.Write((byte)0x00);
                            jumping = false;
                        }
                        writer.Write(0xff);
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
