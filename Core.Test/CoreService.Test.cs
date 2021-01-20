using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Core.Test
{
    [TestClass]
    public class CoreServiceTest
    {
        private Mock<IClassObtainer> _classObtainerMock;
        private Mock<IFileWriter> _fileWriterMock;
        private Mock<IUserInputRepository> _userInputRepoMock;
        private CoreService _service;

        [TestInitialize()]
        public void InitializeTests()
        {
            _classObtainerMock = new Mock<IClassObtainer>();
            _classObtainerMock
                .Setup(x => x.GetTypeFromAssembly(
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ))
                .Returns(typeof(SimpleTestClass));

            _fileWriterMock = new Mock<IFileWriter>();
            _fileWriterMock.Setup(x => x.WriteFile(
                It.IsAny<string>(),
                It.IsAny<string>()
            ));

            _userInputRepoMock = new Mock<IUserInputRepository>();
            _userInputRepoMock
                .Setup(x => x.GetUserInput(CoreService.GetSchemaMessage))
                .Returns("dbo");
            _userInputRepoMock
                .Setup(x => x.GetUserInput(It.IsAny<string>()))
                .Returns("5");

            _service = new CoreService(
                _classObtainerMock.Object,
                _fileWriterMock.Object,
                _userInputRepoMock.Object
            );
        }

        [TestMethod]
        public async Task GenerateSqlForType_TypeNeedsUserInfo_ShouldCallUserInputRepo()
        {
            //Act
            await _service.GenerateSqlForType("path", "type", "output", false);

            //Assert
            _userInputRepoMock.Verify(x =>
                x.GetUserInput(
                    UserInputService.GetLengthMessage("PublicTestString")
                ),
                Times.Once
            );
        }

        [TestMethod]
        public async Task GenerateSqlForType_ShouldAskForSchemaName()
        {
            //Act
            await _service.GenerateSqlForType("path", "type", "output", false);

            //Assert
            _userInputRepoMock.Verify(
                x => x.GetUserInput(CoreService.GetSchemaMessage),
                Times.Once
            );
        }

        [TestMethod]
        public async Task GenerateSqlForType_ShouldWriteFSqlFiles()
        {
            //Act
            await _service.GenerateSqlForType("path", "type", "output", false);

            //Assert
            _fileWriterMock.Verify(
                x => x.WriteFile(It.IsRegex("output.*"), It.IsAny<string>()),
                Times.Exactly(4)
            );
        }
    }
}