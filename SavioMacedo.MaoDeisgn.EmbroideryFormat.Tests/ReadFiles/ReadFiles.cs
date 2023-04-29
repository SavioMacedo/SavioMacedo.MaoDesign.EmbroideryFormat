using System.IO;
using System.Linq;
using SavioMacedo.MaoDeisgn.EmbroideryFormat.Tests.Fixtures;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Dst;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Hus;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Jef;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Pec;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Pes;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Xxx;
using Xunit;

namespace SavioMacedo.MaoDeisgn.EmbroideryFormat.Tests.ReadFiles
{
    [Collection(nameof(FileFixtureCollection))]
    public class ReadFiles
    {
        private readonly FileFixture _fileFixture;

        public ReadFiles(FileFixture fileFixture) => _fileFixture = fileFixture;

        [Trait("Emb Formats", "Read DST files with success from stream.")]
        [Fact]
        public void Dst_Read_SuccessFromStream()
        {
            //Arrange
            Stream fileStream = _fileFixture.GetDstStream();

            //Act
            DstFile resultFile = DstFile.Read(fileStream, false, false, 2.0f);

            //Assert
            Assert.Equal(12690, resultFile.Data.Length);
            Assert.Equal(4059, resultFile.Stitches.Count);
            Assert.Equal(12, resultFile.Metadata.Count);
            Assert.Equal(2, resultFile.Threads.Count);
        }

        [Trait("Emb Formats", "Read DST files with success from bytes.")]
        [Fact]
        public void Dst_Read_SuccessFromBytes()
        {
            //Arrange
            byte[] fileBytes = _fileFixture.GetDstBytes();

            //Act
            DstFile resultFile = DstFile.Read(fileBytes, false, false, 2.0f);

            //Assert
            Assert.Equal(12690, resultFile.Data.Length);
            Assert.Equal(4059, resultFile.Stitches.Count);
            Assert.Equal(12, resultFile.Metadata.Count);
            Assert.Equal(2, resultFile.Threads.Count);
        }

        [Trait("Emb Formats", "Read HUS files with success from stream.")]
        [Fact]
        public void Hus_Read_SuccessFromStream()
        {
            //Arrange
            Stream fileStream = _fileFixture.GetHusStream();

            //Act
            HusFile resultFile = HusFile.Read(fileStream, false, false, 2.0f);

            //Assert
            Assert.Equal(38155, resultFile.Data.Length);
            Assert.Equal(23819, resultFile.Stitches.Count);
            Assert.Equal(3, resultFile.Threads.Count);
        }

        [Trait("Emb Formats", "Read HUS files with success from bytes.")]
        [Fact]
        public void Hus_Read_SuccessFromBytes()
        {
            //Arrange
            byte[] fileBytes = _fileFixture.GetHusBytes();

            //Act
            HusFile resultFile = HusFile.Read(fileBytes, false, false, 2.0f);

            //Assert
            Assert.Equal(38155, resultFile.Data.Length);
            Assert.Equal(23819, resultFile.Stitches.Count);
            Assert.Equal(3, resultFile.Threads.Count);
        }

        [Trait("Emb Formats", "Read JEF files with success from stream.")]
        [Fact]
        public void Jef_Read_SuccessFromStream()
        {
            //Arrange
            Stream fileStream = _fileFixture.GetJefStream();

            //Act
            JefFile resultFile = JefFile.Read(fileStream, "smf_13.jef", false, false, 2.0f);

            //Assert
            Assert.Equal(3928, resultFile.Data.Length);
            Assert.Equal(1871, resultFile.Stitches.Count);
            Assert.Equal(5, resultFile.Threads.Count);
        }

        [Trait("Emb Formats", "Read JEF files with success from bytes.")]
        [Fact]
        public void Jef_Read_SuccessFromBytes()
        {
            //Arrange
            byte[] fileBytes = _fileFixture.GetJefBytes();

            //Act
            JefFile resultFile = JefFile.Read(fileBytes, "smf_13.jef", false, false, 2.0f);

            //Assert
            Assert.Equal(3928, resultFile.Data.Length);
            Assert.Equal(1871, resultFile.Stitches.Count);
            Assert.Equal(5, resultFile.Threads.Count);
        }

        [Trait("Emb Formats", "Read PEC files with success from stream.")]
        [Fact]
        public void Pec_Read_SuccessFromStream()
        {
            //Arrange
            Stream fileStream = _fileFixture.GetPecStream();

            //Act
            PecFile resultFile = PecFile.Read(fileStream, false, false, 2.0f);

            //Assert
            Assert.Equal(8860, resultFile.Data.Length);
            Assert.Equal(4045, resultFile.Stitches.Count);
            Assert.Equal(2, resultFile.Threads.Count);
        }

        [Trait("Emb Formats", "Read PEC files with success from bytes.")]
        [Fact]
        public void Pec_Read_SuccessFromBytes()
        {
            //Arrange
            byte[] fileBytes = _fileFixture.GetPecBytes();

            //Act
            PecFile resultFile = PecFile.Read(fileBytes, false, false, 2.0f);

            //Assert
            Assert.Equal(8860, resultFile.Data.Length);
            Assert.Equal(4045, resultFile.Stitches.Count);
            Assert.Equal(2, resultFile.Threads.Count);
        }

        [Trait("Emb Formats", "Read PES files with success from stream.")]
        [Fact]
        public void Pes_Read_SuccessFromStream()
        {
            //Arrange
            Stream fileStream = _fileFixture.GetPesStream();

            //Act
            PesFile resultFile = PesFile.Read(fileStream, false, false, 2.0f);
            var data = PecFile.Write(resultFile);

            PecFile pecFile = PecFile.Read(data, false, false, 2.0f);

            //Assert
            Assert.Equal(93513, resultFile.Data.Length);
            Assert.Equal(15111, resultFile.Stitches.Count);
            Assert.Equal(7, resultFile.Threads.Count);
        }

        [Trait("Emb Formats", "Read PES files with success from bytes.")]
        [Fact]
        public void Pes_Read_SuccessFromBytes()
        {
            //Arrange
            byte[] fileBytes = _fileFixture.GetPesBytes();

            //Act
            PesFile resultFile = PesFile.Read(fileBytes, false, false, 2.0f);

            //Assert
            Assert.Equal(93513, resultFile.Data.Length);
            Assert.Equal(15111, resultFile.Stitches.Count);
            Assert.Equal(7, resultFile.Threads.Count);
        }

        [Trait("Emb Formats", "Read XXX files with success from stream.")]
        [Fact]
        public void Xxx_Read_SuccessFromStream()
        {
            //Arrange
            Stream fileStream = _fileFixture.GetXxxStream();

            //Act
            XxxFile resultFile = XxxFile.Read(fileStream, "zeikalesflow.xxx", false, false, 2.0f);

            //Assert
            Assert.Equal(15058, resultFile.Data.Length);
            Assert.Equal(7297, resultFile.Stitches.Count);
            Assert.Equal(7, resultFile.Threads.Count);
        }

        [Trait("Emb Formats", "Read XXX files with success from bytes.")]
        [Fact]
        public void Xxx_Read_SuccessFromBytes()
        {
            //Arrange
            byte[] fileBytes = _fileFixture.GetXxxBytes();

            //Act
            XxxFile resultFile = XxxFile.Read(fileBytes, "zeikalesflow.xxx", false, false, 2.0f);

            //Assert
            Assert.Equal(15058, resultFile.Data.Length);
            Assert.Equal(7297, resultFile.Stitches.Count);
            Assert.Equal(7, resultFile.Threads.Count);
        }
    }
}
