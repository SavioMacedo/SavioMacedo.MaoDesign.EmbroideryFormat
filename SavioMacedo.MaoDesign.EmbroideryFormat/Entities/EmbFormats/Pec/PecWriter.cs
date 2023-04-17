using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic;
using System.Collections.Generic;

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
            if (IsLockStitches())
            {
                t.Tie_on = true;
                t.Tie_off = true;
            }
            t.transcode(input);
        }

        public override void Write()
        {
            Write("#PEC0001");
            WritePecStitches(embroideryBasic.FileName);
        }

        public void WritePecStitches(string fileName)
        {
            float maxX;
            float minX;
            float maxY;
            float minY;
            (minX, minY, maxX, maxY) = embroideryBasic.Extents();

            float width = maxX - minX;
            float height = maxY - minY;

            if (fileName == null)
            {
                fileName = "untitled";
            }
            
            Write("LA:");
            if (fileName.length() > 16)
            {
                fileName = fileName.substring(0, 8);
            }

            Write(fileName);
            for (int i = 0; i < (16 - fileName.length()); i++)
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
            EmbThread[] chart = new EmbThread[threadSet.length];

            List<EmbThread> threads = embroideryBasic.GetUniqueThreadList();
            for (EmbThread thread : threads)
            {
                int index = EmbThreadPec.findNearestIndex(thread.getColor(), threadSet);
                threadSet[index] = null;
                chart[index] = thread;
            }

            ByteArrayOutputStream colorTempArray = new ByteArrayOutputStream();
            push(colorTempArray);
            for (EmbObject object : pattern.asColorEmbObjects())
            {
                writeInt8(EmbThread.findNearestIndex(object.getThread().getColor(), chart));
            }
            pop();
            int currentThreadCount = colorTempArray.size();
            if (currentThreadCount != 0)
            {
                for (int i = 0; i < 12; i++)
                {
                    writeInt8(0x20);
                }
                //56

                writeInt8(currentThreadCount - 1);
                write(colorTempArray.toByteArray());
            }
            else
            {
                writeInt8(0x20);
                writeInt8(0x20);
                writeInt8(0x20);
                writeInt8(0x20);
                writeInt8(0x64);
                writeInt8(0x20);
                writeInt8(0x00);
                writeInt8(0x20);
                writeInt8(0x00);
                writeInt8(0x20);
                writeInt8(0x20);
                writeInt8(0x20);
                writeInt8(0xFF);
            }
            for (int i = 0; i < (463 - currentThreadCount); i++)
            {
                writeInt8(0x20);
            } //520
            writeInt8(0x00);
            writeInt8(0x00);

            ByteArrayOutputStream tempArray = new ByteArrayOutputStream();
            push(tempArray);
            pecEncode();
            pop();

            int graphicsOffsetValue = tempArray.size() + 20; //10 //15 //17
            writeInt24LE(graphicsOffsetValue);

            writeInt8(0x31);
            writeInt8(0xFF);
            writeInt8(0xF0);

            /* write 2 byte x size */
            writeInt16LE((short)Math.round(width));
            /* write 2 byte y size */
            writeInt16LE((short)Math.round(height));

            /* Write 4 miscellaneous int16's */
            writeInt16LE((short)0x1E0);
            writeInt16LE((short)0x1B0);

            writeInt16BE((0x9000 | -Math.round(minX)));
            writeInt16BE((0x9000 | -Math.round(minY)));
            stream.write(tempArray.toByteArray());

            PecGraphics graphics = new PecGraphics(minX, minY, maxX, maxY, PEC_ICON_WIDTH, PEC_ICON_HEIGHT);

            for (EmbObject object : pattern.asStitchEmbObjects())
            {
                graphics.draw(object.getPoints());
            }
            write(graphics.getGraphics());
            graphics.clear();

            int lastcolor = 0;
            for (EmbObject layer : pattern.asStitchEmbObjects())
            {
                int currentcolor = layer.getThread().getColor();
                if ((lastcolor != 0) && (currentcolor != lastcolor))
                {
                    write(graphics.getGraphics());
                    graphics.clear();
                }
                graphics.draw(layer.getPoints());
                lastcolor = currentcolor;
            }
            write(graphics.getGraphics());
        }
    }
}
