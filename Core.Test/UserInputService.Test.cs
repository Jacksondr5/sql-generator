using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Core.Test
{
    [TestClass]
    public class UserInputServiceTest
    {
        private Mock<IUserInputRepository> _mockRepo;
        private const string _mockRepoReturnValue = "5";
        private ClassInfo _classInfo;

        [TestInitialize()]
        public void InitializeTests()
        {
            _classInfo = new ClassInfo
            {
                CSharpClassName = "TestClass",
                Properties = new List<PropertyInfo>
                {
                    new PropertyInfo
                    {
                        CSharpName = "Test Name",
                        ValidType = ValidType.Bool
                    }
                }
            };
            _mockRepo = new Mock<IUserInputRepository>();
            _mockRepo
                .Setup(x => x.GetUserInput(It.IsAny<string>()))
                .Returns(_mockRepoReturnValue);
        }


        [TestMethod]
        public void GetUserInfo_ClassContainsString_ShouldAskUserForLength()
        {
            //Assemble
            _classInfo.Properties[0].ValidType = ValidType.String;

            //Act
            UserInputService.GetUserInfo(_classInfo, _mockRepo.Object);

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
            _classInfo.Properties[0].ValidType = ValidType.Decimal;

            //Act
            UserInputService.GetUserInfo(_classInfo, _mockRepo.Object);

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
            _classInfo.Properties[0].ValidType = ValidType.Double;

            //Act
            UserInputService.GetUserInfo(_classInfo, _mockRepo.Object);

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
            _classInfo.Properties[0].ValidType = ValidType.Bool;

            //Act
            UserInputService.GetUserInfo(_classInfo, _mockRepo.Object);

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
            _classInfo.Properties[0].ValidType = ValidType.Decimal;
            _mockRepo
                .Setup(x => x.GetUserInput(It.IsAny<string>()))
                .Returns(invalidInput);

            //Act
            Action act = () => UserInputService.GetUserInfo(_classInfo, _mockRepo.Object);

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