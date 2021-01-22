using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Core.Test
{
    [TestClass]
    public class ClassInspectorTest
    {
        private Mock<IUserInputRepository> _userInputRepoMock;
        private ClassInspector _inspector;

        [TestInitialize()]
        public void InitializeTests()
        {
            _userInputRepoMock = new Mock<IUserInputRepository>();
            _userInputRepoMock
                .Setup(x => x.GetUserInput(It.IsAny<string>()))
                .Returns("");
            _inspector = new ClassInspector(_userInputRepoMock.Object);
        }


        [TestMethod]
        public void GetFieldInfoFromType_ShouldPropertyMapCSharpName()
        {
            //Assemble
            var expected = "PublicTestString";

            //Act
            var actual =
                _inspector.GetFieldInfoFromType(typeof(SimpleTestClass));

            //Assert
            actual.Properties
                .First(x => x.ValidType == ValidType.String).CSharpName
                .Should()
                .Be(expected);
        }

        [DataTestMethod]
        [ValidTypeDataSource]
        public void GetFieldInfoFromType_ShouldPropertyMapCSharpType(
            ValidType expected
        )
        {
            //Act
            var actual = _inspector.GetFieldInfoFromType(
                typeof(AllValidValuesTestClass)
            );

            //Assert
            actual.Properties
                .Select(x => x.ValidType)
                .Should()
                .Contain(expected);
        }

        [TestMethod]
        public void GetFieldInfoFromType_IncludePrivatePropertiesIsFalse_ShouldExcludePrivateProperties()
        {
            //Assemble
            var privateProperty = new PropertyInfo
            {
                CSharpName = "PrivateTestString",
                ValidType = ValidType.String
            };

            //Act
            var actual =
                _inspector.GetFieldInfoFromType(typeof(SimpleTestClass));

            //Assert
            actual.Properties.Should().NotContain(privateProperty);
        }

        [TestMethod]
        public void GetFieldInfoFromType_IncludePrivatePropertiesIsTrue_ShouldIncludePrivateProperties()
        {
            //Assemble
            var expected = "PrivateTestString";

            //Act
            var actual = _inspector.GetFieldInfoFromType(
                typeof(SimpleTestClass),
                includePrivateProperties: true
            );

            //Assert
            actual
                .Properties
                .Select(x => x.CSharpName)
                .Should()
                .ContainEquivalentOf(expected);
        }

        [TestMethod]
        public void GetFieldInfoFromType_ShouldSetClassSqlName()
        {
            //Assemble
            var expected = "simple_test_class";

            //Act
            var actual =
                _inspector.GetFieldInfoFromType(typeof(SimpleTestClass));

            //Assert
            actual.SqlClassName.Should().Be(expected);
        }

        [TestMethod]
        public void GetFieldInfoFromType_ShouldSetClassCSharpName()
        {
            //Assemble
            var expected = "SimpleTestClass";

            //Act
            var actual =
                _inspector.GetFieldInfoFromType(typeof(SimpleTestClass));

            //Assert
            actual.CSharpClassName.Should().Be(expected);
        }

        [TestMethod]
        public void GetFieldInfoFromType_ShouldSetFlagOnIdProperty()
        {
            //Act
            var actual =
                _inspector.GetFieldInfoFromType(typeof(SimpleTestClass));

            //Assert
            actual.Properties
                .Should()
                .Contain(x => x.IsIdProperty && x.CSharpName.Equals("Id"));
        }

        [TestMethod]
        public void GetFieldInfoFromType_NoIdPropertyPresent_ShouldAskForIdProperty()
        {
            //Assemble
            _userInputRepoMock
                .Setup(x => x.GetUserInput(ClassInspector.NoIdPropertyMessage))
                .Returns("StringProperty");

            //Act
            var info =
                _inspector.GetFieldInfoFromType(typeof(MissingIdTestClass));

            //Assert
            _userInputRepoMock.Verify(
                x => x.GetUserInput(ClassInspector.NoIdPropertyMessage),
                Times.Once
            );
            info.Properties
                .First(x => x.CSharpName.Equals("StringProperty"))
                .IsIdProperty
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void GetFieldInfoFromType_UserProvidesInvalidIdPropertyName_ShouldThrowException()
        {
            //Assemble
            var invalidProperty = "not a real property";
            _userInputRepoMock
                .Setup(x => x.GetUserInput(ClassInspector.NoIdPropertyMessage))
                .Returns(invalidProperty);

            //Act
            Action act = () =>
                _inspector.GetFieldInfoFromType(typeof(MissingIdTestClass));

            //Assert
            act.
                Should()
                .ThrowExactly<InvalidInputException>()
                .WithMessage(ClassInspector.InvalidInputExceptionIdProperty(
                    invalidProperty
                ));
        }

        [TestMethod]
        public void GetFieldInfoFromType_NoIdPropertyPresent_ShouldAllowCompoundKey()
        {
            //Assemble
            _userInputRepoMock
                .Setup(x => x.GetUserInput(ClassInspector.NoIdPropertyMessage))
                .Returns("StringProperty,IntProperty");

            //Act
            var info =
                _inspector.GetFieldInfoFromType(typeof(MissingIdTestClass));

            //Assert
            _userInputRepoMock.Verify(
                x => x.GetUserInput(ClassInspector.NoIdPropertyMessage),
                Times.Once
            );
            info.Properties
                .First(x => x.CSharpName.Equals("StringProperty"))
                .IsIdProperty
                .Should()
                .BeTrue();
            info.Properties
                .First(x => x.CSharpName.Equals("IntProperty"))
                .IsIdProperty
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void GetFieldInfoFromType_ShouldNotIncludeClassProperties()
        {
            //Assemble
            var notExpectedName = "SimpleTestClass";

            //Act
            var actual = _inspector.GetFieldInfoFromType(
                typeof(TestClassWithComplexProperty)
            );

            //Assert
            actual.Properties
                .Select(x => x.CSharpName)
                .Should()
                .NotContain(notExpectedName);
        }
    }

    internal class TestClassWithComplexProperty
    {
        public int Id { get; set; }
        public SimpleTestClass SimpleTestClass { get; set; }
    }

    internal class SimpleTestClass
    {
        public int Id { get; set; }
        public string PublicTestString { get; set; }
        private string PrivateTestString { get; set; }
    }

    internal class InvalidIdTestClass
    {
        public string Id { get; set; }
    }

    internal class MissingIdTestClass
    {
        public string StringProperty { get; set; }
        public int IntProperty { get; set; }
    }

    internal class AllValidValuesTestClass
    {
        public int Id { get; set; }
        public int Int { get; set; }
        public bool Bool { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Decimal { get; set; }
        public ValidType Enum { get; set; }
        public double Double { get; set; }
        public string String { get; set; }
        public List<int> List { get; set; }
    }

    internal class ValidTypeDataSourceAttribute : Attribute, ITestDataSource
    {
        public IEnumerable<object[]> GetData(MethodInfo methodInfo) => Enum
            .GetValues<ValidType>()
            .Where(x => x != ValidType.InvalidType)
            .Select(x => new object[] { x });

        public string GetDisplayName(MethodInfo methodInfo, object[] data) =>
            $"{Enum.GetName<ValidType>((ValidType)data[0])}";
    }
}
