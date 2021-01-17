using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.Test
{
    [TestClass]
    public class ClassInspectorTest
    {
        [TestMethod]
        public void GetFieldInfoFromType_ShouldMapCSharpName()
        {
            //Assemble
            var expected = "PublicTestString";

            //Act
            var actual = ClassInspector
                .GetFieldInfoFromType(typeof(SimpleTestType));

            //Assert
            actual.Properties.First().CSharpName.Should().Be(expected);
        }

        [DataTestMethod]
        [ValidTypeDataSource]
        public void GetFieldInfoFromType_ShouldMapCSharpType(ValidType expected)
        {
            //Act
            var actual = ClassInspector
                .GetFieldInfoFromType(typeof(AllValidValuesTestType));

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
                ClassInspector.GetFieldInfoFromType(typeof(SimpleTestType));

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
                typeof(SimpleTestType),
                includePrivateProperties: true
            );

            //Assert
            actual
                .Properties
                .Select(x => x.CSharpName)
                .Should()
                .ContainEquivalentOf(expected);
        }
    }

    internal class SimpleTestType
    {
        public string PublicTestString { get; set; }
        private string PrivateTestString { get; set; }
    }

    internal class AllValidValuesTestType
    {
        public int Int { get; set; }
        public bool Bool { get; set; }
        public decimal Decimal { get; set; }
        public double Double { get; set; }
        public string String { get; set; }
    }

    internal class ValidTypeDataSourceAttribute : Attribute, ITestDataSource
    {
        public IEnumerable<object[]> GetData(MethodInfo methodInfo) =>
            Enum.GetValues<ValidType>().Select(x => new object[] { x });

        public string GetDisplayName(MethodInfo methodInfo, object[] data) =>
            $"{Enum.GetName<ValidType>((ValidType)data[0])}";
    }
}
