using SavioMacedo.MaoDeisgn.EmbroideryFormat.Tests.Fixtures;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Jef;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Pec;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Pes;
using System.IO;
using Xunit;

namespace SavioMacedo.MaoDeisgn.EmbroideryFormat.Tests.ReadFiles
{
    [Collection(nameof(FileFixtureCollection))]
    public class WriteFiles
    {
        private readonly FileFixture _fileFixture;

        public WriteFiles(FileFixture fileFixture) => _fileFixture = fileFixture;

        [Trait("Emb Formats", "Write PES files with success from Jef Format.")]
        [Fact]
        public void Pes_WriteFile_Success()
        {
            //Arrange
            Stream fileStream = _fileFixture.GetJefStream();

            //Act
            JefFile resultFile = JefFile.Read(fileStream, "smf_13.jef", false, false, 2.0f);
            PesFile pesFile = PesFile.Read(resultFile, false, false, 2.0f);

            //Assert
            Assert.Equal(5, pesFile.Threads.Count);
        }

        [Trait("Emb Formats", "Write PEC files with success from Jef Format.")]
        [Fact]
        public void Pec_WriteFile_Success()
        {
            //Arrange
            Stream fileStream = _fileFixture.GetJefStream();

            //Act
            JefFile resultFile = JefFile.Read(fileStream, "smf_13.jef", false, false, 2.0f);
            PecFile pecFile = PecFile.Read(resultFile, false, false, 2.0f);

            //Assert
            Assert.Equal(5, pecFile.Threads.Count);
        }

        [Trait("Emb Formats", "Write JEF files with success from Pes Format.")]
        [Fact]
        public void Jef_WriteFile_Success()
        {
            //Arrange
            Stream fileStream = _fileFixture.GetPesStream();

            //Act
            PesFile resultFile = PesFile.Read(fileStream, false, false, 2.0f);
            JefFile jefFile = JefFile.Read(resultFile, false, false, 2.0f);

            //Assert
            Assert.Equal(7, jefFile.Threads.Count);
        }
    }
}
