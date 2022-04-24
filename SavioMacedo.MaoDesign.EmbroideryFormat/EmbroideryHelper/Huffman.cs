using System.Collections.Generic;
using System.Linq;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.EmbroideryHelper
{
    public class Huffman
    {
        private int _defaultValue;
        private int[] _lenghts;
        private List<int> _table;
        private int _tableWidth;

        public Huffman(int[] lenghts = null, int value = 0)
        {
            _lenghts = lenghts;
            _table = null;
            _defaultValue = value;
            _tableWidth = 0;
        }

        internal void BuildTable()
        {
            _tableWidth = _lenghts.Max();
            _table = new List<int>();
            int size = 1 << _tableWidth;

            for (var bitLength = 1; bitLength < _tableWidth + 1; bitLength++)
            {
                size /= 2;
                for (var lexIndex = 0; lexIndex < _lenghts.Length; lexIndex++)
                {
                    int length = _lenghts[lexIndex];
                    if (length == bitLength)
                    {
                        _table.AddRange(Enumerable.Repeat(lexIndex, size));
                    }
                }
            }
        }

        public (int, int) Lookup(int byteLookup)
        {
            if (_table == null)
            {
                return (_defaultValue, 0);
            }

            int v = _table[byteLookup >> (16 - _tableWidth)];
            return (v, _lenghts[v]);
        }
    }
}
