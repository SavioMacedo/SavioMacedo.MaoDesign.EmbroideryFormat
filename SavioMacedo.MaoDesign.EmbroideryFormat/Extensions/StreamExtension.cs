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
    }
}
