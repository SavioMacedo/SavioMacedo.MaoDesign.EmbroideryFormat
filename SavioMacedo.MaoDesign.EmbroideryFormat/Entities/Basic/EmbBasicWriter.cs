using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic
{
    public abstract class EmbBasicWriter
    {
        public static int LOCK_STITCHES = 1;

        protected Stack<BinaryWriter> streamStack;
        public BinaryWriter stream;
        protected EmbroideryBasic embroideryBasic;

        private int _settings;

        public void Write(EmbroideryBasic embroidery, BinaryWriter stream)
        {
            this.stream = stream;
            embroideryBasic = embroidery;
            //PreWrite(embroidery);
            Write(embroidery);
            PostWrite(embroidery);
        }

        public virtual void PostWrite(EmbroideryBasic embroidery)
        { }

        public abstract void Write(EmbroideryBasic embroidery);

        public virtual void PreWrite(EmbroideryBasic embroidery)
        { }

        public bool IsLockStitches()
        {
            return (_settings & LOCK_STITCHES) != 0;
        }

        public void Write(string value)
        {
            stream.Write(value);
        }

        public void WriteInt8(int value)
        {
            stream.Write(value);
        }

        public void Write(byte[] bytes)
        {
            stream.Write(bytes);
        }

        public void WriteInt16LE(int value)
        {
            stream.Write(value & 0xFF);
            stream.Write((value >> 8) & 0xFF);
        }

        public void WriteInt16BE(int value)
        {
            stream.Write((value >> 8) & 0xFF);
            stream.Write(value & 0xFF);
        }

        public void WriteInt24LE(int value)
        {
            stream.Write(value & 0xFF);
            stream.Write((value >> 8) & 0xFF);
            stream.Write((value >> 16) & 0xFF);
        }

        public void Push(BinaryWriter push)
        {
            streamStack ??= new();
            streamStack.Push(stream);
            stream = push;
        }

        public BinaryWriter Pop()
        {
            if (streamStack == null)
            {
                return null;
            }
            if (streamStack.Any())
            {
                return null;
            }
            BinaryWriter pop = stream;
            stream = streamStack.Pop();
            return pop;
        }
    }
}
