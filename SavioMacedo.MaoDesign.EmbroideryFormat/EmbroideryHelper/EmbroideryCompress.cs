using System;
using System.Collections.Generic;
using System.Linq;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.EmbroideryHelper
{
    public class EmbroideryCompress
    {
        private byte[] _data;
        private int _blockelements;
        private int _bitPosition;
        private Huffman _characterHuffman;
        private Huffman _distanceHuffman;

        public List<int> Decompress(byte[] data, int? uncompressedSize = null)
        {
            _data = data;
            List<int> outputData = new List<int>();
            _blockelements = -1;
            int bitsTotal = _data.Length * 8;
            while (bitsTotal > _bitPosition && (uncompressedSize == null || outputData.Count <= uncompressedSize))
            {
                int character = GetToken();
                if (character <= 255)
                {
                    outputData.Add(character);
                }
                else if (character == 510)
                {
                    break;
                }
                else
                {
                    int lenght = character - 253;
                    int back = GetPosition() + 1;
                    int posititon = outputData.Count - back;
                    if (back > lenght)
                    {
                        outputData.AddRange(outputData.ToArray()[posititon..(posititon + lenght)]);
                    }
                    else
                    {
                        for (var i = posititon; i < posititon + lenght; i++)
                        {
                            outputData.Add(outputData[i]);
                        }
                    }
                }
            }
            return outputData;
        }

        private int GetPosition()
        {
            (int, int) h = _distanceHuffman.Lookup(Peek(16));
            Slide(h.Item2);
            if (h.Item1 == 0)
            {
                return 0;
            }

            int v = h.Item1 - 1;
            v = (1 << v) + Pop(v);
            return v;
        }

        public int GetToken()
        {
            if (_blockelements <= 0)
            {
                LoadBlock();
            }

            _blockelements -= 1;
            (int, int) h = _characterHuffman.Lookup(Peek(16));
            Slide(h.Item2);
            return h.Item1;
        }

        private void LoadBlock()
        {
            _blockelements = Pop(16);
            Huffman characterLenghtHuffman = LoadCharacterLenghtHuffman();
            _characterHuffman = LoadCharacterHuffman(characterLenghtHuffman);
            _distanceHuffman = LoadDistanceHuffman();
        }

        private Huffman LoadDistanceHuffman()
        {
            int count = Pop(5);
            Huffman huffman;
            if (count == 0)
            {
                int v = Pop(5);
                huffman = new Huffman(value: v);
                return huffman;
            }

            int index = 0;
            int[] lenghts = Enumerable.Range(0, count).ToArray();

            for (var i = 0; i < count; i++)
            {
                lenghts[i] = ReadVariableLenght();
                index += 1;
            }

            huffman = new Huffman(lenghts);
            huffman.BuildTable();
            return huffman;
        }

        private Huffman LoadCharacterHuffman(Huffman characterLenghtHuffman)
        {
            int count = Pop(9);
            Huffman huffman;
            if (count == 0)
            {
                int v = Pop(9);
                huffman = new Huffman(value: v);
                return huffman;
            }

            int[] huffmanCodeLenghts = Enumerable.Repeat(0, count).ToArray();
            int index = 0;
            while (index < count)
            {
                (int, int) h = characterLenghtHuffman.Lookup(Peek(16));
                int c = h.Item1;
                Slide(h.Item2);

                if (c == 0)
                {
                    c = 1;
                    index += c;
                }
                else if (c == 1)
                {
                    c = 3 + Pop(4);
                    index += c;
                }
                else if (c == 2)
                {
                    c = 20 + Pop(9);
                    index += c;
                }
                else
                {
                    c -= 2;
                    huffmanCodeLenghts[index] = c;
                    index += 1;
                }
            }
            huffman = new Huffman(huffmanCodeLenghts);
            huffman.BuildTable();
            return huffman;
        }

        private Huffman LoadCharacterLenghtHuffman()
        {
            int count = Pop(5);
            Huffman huffman;
            if (count == 0)
            {
                int v = Pop(5);
                huffman = new Huffman(value: v);
                return huffman;
            }

            int[] huffmanCodeLenghts = Enumerable.Repeat(0, count).ToArray();
            int index = 0;
            while (index < count)
            {
                if (index == 3)
                {
                    index += Pop(2);
                }

                huffmanCodeLenghts[index] = ReadVariableLenght();
                index++;
            }

            huffman = new Huffman(huffmanCodeLenghts, 8);
            huffman.BuildTable();
            return huffman;
        }

        private int ReadVariableLenght()
        {
            int m = Pop(3);
            if (m != 7)
            {
                return m;
            }

            for (var i = 0; i < 13; i++)
            {
                int s = Pop(1);
                if (s == 1)
                {
                    m++;
                    continue;
                }

                break;
            }

            return m;
        }

        private int Pop(int v)
        {
            int value = Peek(v);
            Slide(v);
            return value;
        }

        private void Slide(int v)
        {
            _bitPosition += v;
        }

        private int Peek(int v)
        {
            return GetBits(_bitPosition, v);
        }

        private int GetBits(int bitPosition, int v)
        {
            int endPosInBits = bitPosition + v - 1;
            int startPosInBytes = bitPosition / 8;
            int endPosInBytes = endPosInBits / 8;
            int value = 0;

            for (var i = startPosInBytes; i < endPosInBytes + 1; i++)
            {
                value <<= 8;
                try
                {
                    value |= _data[i] & 0xFF;
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }
            }

            int unusedBitsRightOfSample = (8 - (endPosInBits + 1) % 8) % 8;
            int maskSamplebits = (1 << v) - 1;
            int original = (value >> unusedBitsRightOfSample) & maskSamplebits;
            return original;
        }

        public static List<int> Expand(byte[] data, int uncompressedSize)
        {
            EmbroideryCompress embCompress = new();
            return embCompress.Decompress(data, uncompressedSize);
        }
    }
}
