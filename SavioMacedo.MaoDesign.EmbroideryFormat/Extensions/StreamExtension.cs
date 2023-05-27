using System;
using System.IO;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Extensions
{
    public static class StreamExtension
    {
        public static byte[] ReadFully(this Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using MemoryStream ms = new();
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            return ms.ToArray();
        }

        public static void WriteString(this BinaryWriter bw, string value) 
        {
            bw.Write(value.ToCharArray());
        }

        public static void FPad(this BinaryWriter file, byte c, int n)
        {
            for (int i = 0; i < n; i++)
            {
                file.Write(c);
            }
        }

        public static void WriteInt32Le(this BinaryWriter binaryWriter, int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            binaryWriter.Write(buffer);
        }

        public static void WriteInt16LE(this BinaryWriter writer, short value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            writer.Write(buffer);
        }

        public static void WriteInt16BE(this BinaryWriter writer, ushort value)
        {
            byte[] buffer = new byte[2];
            buffer[0] = (byte)((value >> 8) & 0xFF);
            buffer[1] = (byte)(value & 0xFF);
            writer.Write(buffer);
        }
    }
}
