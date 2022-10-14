using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Dst;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Hus;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Jef;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Pec;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Pes;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Xxx;
using SavioMacedo.MaoDesign.EmbroideryFormat.Exceptions;
using SavioMacedo.MaoDesign.EmbroideryFormat.Extensions;
using System.IO;
using System.Text;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Bin
{
    public class BinFile
    {
        public static EmbroideryBasic Read(Stream stream, string fileName, bool allowTransparency, bool hideMachinePaths, float threadThinckness)
        {
            return Read(stream.ReadFully(), fileName, allowTransparency, hideMachinePaths, threadThinckness);
        }

        public static EmbroideryBasic Read(byte[] bytes, string fileName, bool allowTransparency, bool hideMachinePaths, float threadThinckness)
        {
            if (IsDst(bytes))
            {
                return DstFile.Read(bytes, allowTransparency, hideMachinePaths, threadThinckness);
            }
            else if (IsHus(bytes))
            {
                return HusFile.Read(bytes, allowTransparency, hideMachinePaths, threadThinckness);
            }
            else if (IsJef(bytes))
            {
                return JefFile.Read(bytes, fileName, allowTransparency, hideMachinePaths, threadThinckness);
            }
            else if (IsPec(bytes))
            {
                return PecFile.Read(bytes, allowTransparency, hideMachinePaths, threadThinckness);
            }
            else if (IsPes(bytes))
            {
                return PesFile.Read(bytes, allowTransparency, hideMachinePaths, threadThinckness);
            }
            else if (IsXxx(bytes))
            {
                return XxxFile.Read(bytes, fileName, allowTransparency, hideMachinePaths, threadThinckness);
            }

            throw new UnknowFormatException("Formato bin não localizado.");
        }

        private static bool IsDst(byte[] bytes)
        {
            try
            {
                BinaryReader reader = new(new MemoryStream(bytes));
                byte[] header = reader.ReadBytes(512);
                string headerString = Encoding.UTF8.GetString(header);
                return headerString.Contains("LA") && headerString.Contains("ST");
            }
            catch
            {
                return false;
            }
        }

        private static bool IsHus(byte[] bytes)
        {
            try
            {
                BinaryReader reader = new(new MemoryStream(bytes));
                int magicNumber = reader.ReadInt32();
                return magicNumber == 13152091;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsJef(byte[] bytes)
        {
            try
            {
                BinaryReader reader = new(new MemoryStream(bytes));
                int stitchOffset = reader.ReadInt32();
                int unknownValidator = reader.ReadInt32();
                return unknownValidator == 1 || unknownValidator == 10 || unknownValidator == 20;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsPec(byte[] bytes)
        {
            try
            {
                BinaryReader reader = new(new MemoryStream(bytes));
                string pecString = reader.ReadString8(8);
                return pecString.Equals("#PEC0001");
            }
            catch
            {
                return false;
            }
        }

        private static bool IsPes(byte[] bytes)
        {
            try
            {
                BinaryReader reader = new(new MemoryStream(bytes));
                string pecString = reader.ReadString8(8);
                return pecString.Contains("PES");
            }
            catch
            {
                return false;
            }
        }

        private static bool IsXxx(byte[] bytes)
        {
            try
            {
                BinaryReader reader = new(new MemoryStream(bytes));
                int index1 = reader.ReadInt32();
                int index2 = reader.ReadInt32();
                int index3 = reader.ReadInt32();
                return index1 == 0  && index2 == 0 && index3 == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
