using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Core.Test
{
    [TestClass]
    public class ClassInspectorTest
    {
        [TestMethod]
        public void GetFieldInfoFromType_ShouldPropertyMapCSharpName()
        {
            //Assemble
            var expected = "PublicTestString";

            //Act
            var actual = ClassInspector
                .GetFieldInfoFromType(typeof(SimpleTestClass));

            //Assert
            actual.Properties
                .First(x => x.CSharpType == ValidType.String).CSharpName
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
            var actual = ClassInspector
                .GetFieldInfoFromType(typeof(AllValidValuesTestClass));

            //Assert
            actual.Properties
                .Select(x => x.CSharpType)
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
                CSharpType = ValidType.String
            };

            //Act
            var actual =
                ClassInspector.GetFieldInfoFromType(typeof(SimpleTestClass));

            //Assert
            actual.Properties.Should().NotContain(privateProperty);
        }

        [TestMethod]
        public void GetFieldInfoFromType_IncludePrivatePropertiesIsTrue_ShouldIncludePrivateProperties()
        {
            //Assemble
            var expected = "PrivateTestString";

            //Act
            var actual = ClassInspector.GetFieldInfoFromType(
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
            var actual = ClassInspector.GetFieldInfoFromType(
                typeof(SimpleTestClass)
            );

            //Assert
            actual.SqlClassName.Should().Be(expected);
        }

        [TestMethod]
        public void GetFieldInfoFromType_ShouldSetClassCSharpName()
        {
            //Assemble
            var expected = "SimpleTestClass";

            //Act
            var actual = ClassInspector.GetFieldInfoFromType(
                typeof(SimpleTestClass)
            );

            //Assert
            actual.CSharpClassName.Should().Be(expected);
        }

        [TestMethod]
        public void GetFieldInfoFromType_ShouldSetFlagOnIdProperty()
        {
            //Act
            var actual = ClassInspector.GetFieldInfoFromType(
                typeof(SimpleTestClass)
            );

            //Assert
            actual.Properties
                .Should()
                .Contain(x => x.IsIdProperty && x.CSharpName.Equals("Id"));
        }

        [TestMethod]
        public void GetFieldInfoFromType_NoIdPropertyPresent_ShouldThrowException()
        {
            //Act
            Action act = () => ClassInspector.GetFieldInfoFromType(
                typeof(MissingIdTestClass)
            );

            //Assert
            act
                .Should()
                .ThrowExactly<InvalidInputException>()
                .WithMessage(ClassInspector.InvalidInputExceptionNoIdProperty(
                    "MissingIdTestClass"
                ));
        }

        [TestMethod]
        public void GetFieldInfoFromType_IdPropertyIsNotInt_ShouldThrowException()
        {
            //Act
            Action act = () => ClassInspector.GetFieldInfoFromType(
                typeof(InvalidIdTestClass)
            );

            //Assert
            act
                .Should()
                .ThrowExactly<InvalidInputException>()
                .WithMessage(
                    ClassInspector.InvalidInputExceptionIdPropertyNotInt(
                        "InvalidIdTestClass"
                    )
                );
        }

        [TestMethod]
        public void GetFieldInfoFromType_ShouldNotIncludeClassProperties()
        {
            //Assemble
            var notExpectedName = "SimpleTestClass";

            //Act
            var actual = ClassInspector.GetFieldInfoFromType(
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

    internal class MissingIdTestClass { }

    internal class AllValidValuesTestClass
    {
        public int Id { get; set; }
        public int Int { get; set; }
        public bool Bool { get; set; }
        public decimal Decimal { get; set; }
        public double Double { get; set; }
        public string String { get; set; }
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
