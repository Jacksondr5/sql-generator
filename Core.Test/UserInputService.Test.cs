using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Core.Test
{
    [TestClass]
    public class UserInputServiceTest
    {
        private Mock<IUserInputRepository> _mockRepo;
        private UserInputService _service;
        private const string _mockRepoReturnValue = "5";
        private ClassInfo _classInfo;

        [TestInitialize()]
        public void InitializeTests()
        {
            _classInfo = new ClassInfo
            {
                ClassName = "TestClass",
                Properties = new List<PropertyInfo>
                {
                    new PropertyInfo
                    {
                        CSharpName = "Test Name",
                        CSharpType = ValidType.Bool
                    }
                }
            };
            _mockRepo = new Mock<IUserInputRepository>();
            _mockRepo
                .Setup(x => x.GetUserInput(It.IsAny<string>()))
                .Returns(_mockRepoReturnValue);
            _service = new UserInputService(_mockRepo.Object);
        }


        [TestMethod]
        public void GetUserInfo_ClassContainsString_ShouldAskUserForLength()
        {
            //Assemble
            _classInfo.Properties[0].CSharpType  = ValidType.String;

            //Act
            _service.GetUserInfo(_classInfo);

            //Assert
            _mockRepo.Verify(
                x => x.GetUserInput(UserInputService.GetLengthMessage(
                    _classInfo.Properties[0].CSharpName
                )),
                Times.Once
            );
        }

        [TestMethod]
        public void GetUserInfo_ClassContainsDecimal_ShouldAskUserForPrecisionAndScale()
        {
            //Assemble
            _classInfo.Properties[0].CSharpType  = ValidType.Decimal;

            //Act
            _service.GetUserInfo(_classInfo);

            //Assert
            _mockRepo.Verify(
                x => x.GetUserInput(UserInputService.GetPrecisionMessage(
                    _classInfo.Properties[0].CSharpName
                )),
                Times.Once
            );
            _mockRepo.Verify(
                x => x.GetUserInput(UserInputService.GetScaleMessage(
                    _classInfo.Properties[0].CSharpName
                )),
                Times.Once
            );
        }

        [TestMethod]
        public void GetUserInfo_ClassContainsDouble_ShouldAskUserForPrecisionAndScale()
        {
            //Assemble
            _classInfo.Properties[0].CSharpType  = ValidType.Double;

            //Act
            _service.GetUserInfo(_classInfo);

            //Assert
            _mockRepo.Verify(
                x => x.GetUserInput(UserInputService.GetPrecisionMessage(
                    _classInfo.Properties[0].CSharpName
                )),
                Times.Once
            );
            _mockRepo.Verify(
                x => x.GetUserInput(UserInputService.GetScaleMessage(
                    _classInfo.Properties[0].CSharpName
                )),
                Times.Once
            );
        }

        [TestMethod]
        public void GetUserInfo_ClassDoesntNeedExtraInfo_ShouldNotAskUserForAnything()
        {
            //Assemble
            _classInfo.Properties[0].CSharpType  = ValidType.Bool;

            //Act
            _service.GetUserInfo(_classInfo);

            //Assert
            _mockRepo.Verify(
                x => x.GetUserInput(It.IsAny<string>()),
                Times.Never
            );
        }

        [TestMethod]
        public void GetUserInfo_UserGivesInvalidValue_ShouldThrowException()
        {
            //Assemble
            var invalidInput = "invalid input";
            _classInfo.Properties[0].CSharpType = ValidType.String;
            _mockRepo
                .Setup(x => x.GetUserInput(It.IsAny<string>()))
                .Returns(invalidInput);
            
            //Act
            Action act = () => _service.GetUserInfo(_classInfo);

            //Assert
            act
                .Should()
                .ThrowExactly<InvalidInputException>()
                .WithMessage(
                    UserInputService.InvalidInputExceptionBadInput(invalidInput)
                );
        }
    }
}