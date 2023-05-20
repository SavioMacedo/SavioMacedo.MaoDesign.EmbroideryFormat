using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Extensions
{
    public static class ReadHelper
    {
        public static string ReadString8(this BinaryReader reader, int? lenght)
        {
            if (lenght == null)
                return string.Empty;

            char[] chars = reader.ReadChars((int)lenght);
            string str = new(chars);
            return str;
        }

        public static int? ReadInt8(this BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(1);
            if(bytes.Count() == 1)
                return bytes[0];

            return null;
        }

        public static int ReadInt24Le(this BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(3);
            if(bytes.Length == 3)
            {
                return (bytes[0] & 0xFF) + ((bytes[1] & 0xFF) << 8) + ((bytes[2] & 0xFF) << 16);
            }

            return 0;
        }

        public static string ReadPesString(this BinaryReader reader)
        {
            int? lenght = reader.ReadInt8();

            if (lenght == 0)
                return string.Empty;

            return reader.ReadString8(lenght);
        }

        public static int ReadInt24Be(this BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(3);
            if(bytes.Length == 3)
            {
                return (bytes[0] & 0xFF) + ((bytes[1] & 0xFF) << 8) + ((bytes[2] & 0xFF) << 16);
            }

            return 0;
        }

        public static int? ReadInt32Be(this BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(4);
            if( bytes.Length == 4)
            {
                return (bytes[3] & 0xFF) + ((bytes[2] & 0xFF) << 8) + ((bytes[1] & 0xFF) << 16) + ((bytes[0] & 0xFF) << 24);
            }
            return null;
        }

        public static void WriteInt8(this BinaryWriter writer, int value)
        {
            writer.Write((byte)value);
        }

        public static void WriteInt24LE(this BinaryWriter writer, int value)
        {
            writer.Write(value & 0xFF);
            writer.Write((value >> 8) & 0xFF);
            writer.Write((value >> 16) & 0xFF);
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
