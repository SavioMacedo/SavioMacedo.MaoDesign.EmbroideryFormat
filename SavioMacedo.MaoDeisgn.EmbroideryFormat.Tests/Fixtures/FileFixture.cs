using System.IO;
using SavioMacedo.MaoDesign.EmbroideryFormat.Extensions;
using Xunit;

namespace SavioMacedo.MaoDeisgn.EmbroideryFormat.Tests.Fixtures
{
    [CollectionDefinition(nameof(FileFixtureCollection))]
    public class FileFixtureCollection: ICollectionFixture<FileFixture>
    {}

    public class FileFixture
    {
        public Stream GetStream(string path)
        {
            FileStream fileStream = new(path, FileMode.Open, FileAccess.Read);
            return fileStream;
        }

        public Stream GetDstStream()
        {
            return GetStream("Fixtures/Files/sdas.DST");
        }

        public byte[] GetDstBytes()
        {
            Stream fileStream = GetDstStream();
            return fileStream.ReadFully();
        }

        public Stream GetHusStream()
        {
            return GetStream("Fixtures/Files/elephafr5a.hus");
        }

        public byte[] GetHusBytes()
        {
            Stream fileStream = GetHusStream();
            return fileStream.ReadFully();
        }

        public Stream GetJefStream()
        {
            return GetStream("Fixtures/Files/smf_13.jef");
        }

        public byte[] GetJefBytes()
        {
            Stream fileStream = GetJefStream();
            return fileStream.ReadFully();
        }

        public Stream GetPecStream()
        {
            return GetStream("Fixtures/Files/Hopea.pec");
        }

        public byte[] GetPecBytes()
        {
            Stream fileStream = GetPecStream();
            return fileStream.ReadFully();
        }

        public Stream GetPesStream()
        {
            return GetStream("Fixtures/Files/Golfcrest.pes");
        }

        public byte[] GetPesBytes()
        {
            Stream fileStream = GetPesStream();
            return fileStream.ReadFully();
        }

        public Stream GetXxxStream()
        {
            return GetStream("Fixtures/Files/zeikalesflow.xxx");
        }

        public byte[] GetXxxBytes()
        {
            Stream fileStream = GetXxxStream();
            return fileStream.ReadFully();
        }

        public Stream GetBinPesStream()
        {
            return GetStream("Fixtures/Files/pes.bin");
        }

        public byte[] GetBinPesBytes()
        {
            Stream fileStream = GetBinPesStream();
            return fileStream.ReadFully();
        }

        public Stream GetBinHusStream()
        {
            return GetStream("Fixtures/Files/hus.bin");
        }

        public Stream GetBinDstStream()
        {
            return GetStream("Fixtures/Files/dst.bin");
        }

        public Stream GetBinJefStream()
        {
            return GetStream("Fixtures/Files/jef.bin");
        }

        public Stream GetBinPecStream()
        {
            return GetStream("Fixtures/Files/pec.bin");
        }

        public Stream GetBinXxxStream()
        {
            return GetStream("Fixtures/Files/xxx.bin");
        }
    }
}
