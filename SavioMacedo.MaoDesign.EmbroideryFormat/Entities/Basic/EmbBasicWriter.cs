using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic
{
    public abstract class EmbBasicWriter
    {
        public static int LOCK_STITCHES = 1;

        protected Stack<StreamWriter> streamStack;
        protected StreamWriter stream;
        protected EmbroideryBasic embroideryBasic;

        private int _settings;

        public void Write(EmbroideryBasic embroidery, StreamWriter stream) 
        {
            this.stream = stream;
            embroideryBasic = embroidery;
            PreWrite(embroidery);
            Write();
            PostWrite(embroidery);
        }

        public virtual void PostWrite(EmbroideryBasic embroidery)
        {}

        public abstract void Write();

        public virtual void PreWrite(EmbroideryBasic embroidery)
        {}

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
    }
}
