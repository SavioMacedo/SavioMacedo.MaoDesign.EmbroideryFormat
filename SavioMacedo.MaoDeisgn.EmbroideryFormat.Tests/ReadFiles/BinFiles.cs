using SavioMacedo.MaoDeisgn.EmbroideryFormat.Tests.Fixtures;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Bin;
using System.IO;
using Xunit;

namespace SavioMacedo.MaoDeisgn.EmbroideryFormat.Tests.ReadFiles
{
    [Collection(nameof(FileFixtureCollection))]
    public class BinFiles
    {
        private readonly FileFixture _fileFixture;

        public BinFiles(FileFixture fileFixture) => _fileFixture = fileFixture;

        [Trait("Emb Formats", "Read bin files and discover the type pes.")]
        [Fact]
        public void Pes_Read_Success()
        {
            //Arrange
            byte[] data = _fileFixture.GetBinPesBytes();

            //Act
            EmbroideryBasic embroideryBasic = BinFile.Read(data, "pes.bin", false, false, 2.0f);

            //Assert
            Assert.Equal(7, embroideryBasic.Threads.Count);
            Assert.Equal(15111, embroideryBasic.Stitches.Count);
        }

        [Trait("Emb Formats", "Read bin files and discover the type hus.")]
        [Fact]
        public void Hus_Read_SuccessPesFile()
        {
            //Arrange
            Stream stream = _fileFixture.GetBinHusStream();

            //Act
            EmbroideryBasic resultFile = BinFile.Read(stream, "hus.bin", false, false, 2.0f);

            //Assert
            Assert.Equal(38155, resultFile.Data.Length);
            Assert.Equal(23819, resultFile.Stitches.Count);
        }

        [Trait("Emb Formats", "Read bin files and discover the type dst.")]
        [Fact]
        public void Dst_Read_Success()
        {
            //Arrange
            Stream stream = _fileFixture.GetBinDstStream();

            //Act
            EmbroideryBasic resultFile = BinFile.Read(stream, "dst.bin", false, false, 2.0f);

            //Assert
            Assert.Equal(4059, resultFile.Stitches.Count);
            Assert.Equal(12, resultFile.Metadata.Count);
            Assert.Equal(2, resultFile.Threads.Count);
        }

        [Trait("Emb Formats", "Read bin files and discover the type jef.")]
        [Fact]
        public void Jef_Read_Success()
        {
            //Arrange
            Stream stream = _fileFixture.GetBinJefStream();

            //Act
            EmbroideryBasic resultFile = BinFile.Read(stream, "dst.bin", false, false, 2.0f);

            //Assert
            Assert.Equal(1871, resultFile.Stitches.Count);
            Assert.Equal(5, resultFile.Threads.Count);
        }

        [Trait("Emb Formats", "Read bin files and discover the type pec.")]
        [Fact]
        public void Pec_Read_Success()
        {
            //Arrange
            Stream stream = _fileFixture.GetBinPecStream();

            //Act
            EmbroideryBasic resultFile = BinFile.Read(stream, "pec.bin", false, false, 2.0f);

            //Assert
            Assert.Equal(4045, resultFile.Stitches.Count);
            Assert.Equal(2, resultFile.Threads.Count);
        }

        [Trait("Emb Formats", "Read bin files and discover the type xxx.")]
        [Fact]
        public void Xxx_Read_Success()
        {
            //Arrange
            Stream stream = _fileFixture.GetBinXxxStream();

            //Act
            EmbroideryBasic resultFile = BinFile.Read(stream, "xxx.bin", false, false, 2.0f);

            //Assert
            Assert.Equal(7297, resultFile.Stitches.Count);
            Assert.Equal(7, resultFile.Threads.Count);
        }
    }
}
