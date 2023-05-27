using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.Enums;
using System.Collections;
using System.Collections.Generic;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic
{
    public class SectionEmbObjects : IEnumerator<(EmbThread, Stitch[], int)>
    {
        private readonly List<Stitch> _stitches;
        private readonly EmbroideryBasic _pattern;
        private int indexStart = -1;
        private int indexStop = 0;
        readonly int NOT_CALCULATED = 0;
        readonly int HAS_NEXT = 1;
        readonly int ENDED = 2;

        int mode = 0;
        int threadIndex = 0;
        int type;

        public (EmbThread, Stitch[], int) Current
        {
            get
            {
                mode = NOT_CALCULATED;
                return (_pattern.GetThreadOrFiller(threadIndex), _stitches.GetRange(indexStart, indexStop - indexStart).ToArray(), type);
            }
        }

        object IEnumerator.Current => Current;

        public int Item3 { get; internal set; }

        public SectionEmbObjects(EmbroideryBasic pattern)
        {
            _stitches = pattern.Stitches;
            _pattern = pattern;
        }

        void Calculate()
        {
            indexStart = indexStop;
            indexStop = -1;
            for (int i = indexStart, ie = _stitches.Count; i < ie; i++)
            {
                Command data = _stitches[i].Command;
                if (data == Command.ColorChange)
                {
                    threadIndex++;
                }
                if (data == Command.Stitch)
                {
                    type = 0;
                    indexStart = i;
                    break;
                }
                if (data == Command.Jump)
                {
                    type = 1;
                    indexStart = i;
                    break;
                }
            }
            for (int i = indexStart, ie = _stitches.Count; i < ie; i++)
            {
                int data = (int)_stitches[i].Command;
                if (data != type)
                {
                    indexStop = i;
                    break;
                }
            }
            mode = (indexStop == -1 || indexStart == indexStop) ? ENDED : HAS_NEXT;
        }

        public bool MoveNext()
        {
            if (mode == NOT_CALCULATED) 
                Calculate();
            return mode == HAS_NEXT;
        }

        public void Reset()
        {
            indexStart = -1;
            indexStop = 0;
            mode = 0;
            threadIndex = 0;
            type = 0;
        }

        public void Dispose()
        {
        }
    }

}
