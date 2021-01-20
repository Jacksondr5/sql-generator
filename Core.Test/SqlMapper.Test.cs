using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Core.Test
{
    [TestClass]
    public class SqlMapperTest
    {
        [TestMethod]
        public void MapPropertyToSql_ShouldMapSqlName()
        {
            //Assemble
            var property = new PropertyInfo
            {
                CSharpName = "PublicTestString",
                CSharpType = ValidType.Int
            };
            var expected = "public_test_string";

            //Act
            var actual = SqlMapper.MapPropertyToSql(property);

            //Assert
            actual.SqlName.Should().Be(expected);
        }

        [DataTestMethod]
        [BuiltInTypeDataSource]
        public void MapPropertyToSql_ShouldMapSqlType(
            ValidType type,
            string expected
        )
        {
            //Assemble
            var property = new PropertyInfo
            {
                CSharpType = type
            };

            //Act
            var actual = SqlMapper.MapPropertyToSql(property);

            //Assert
            actual.SqlType.Should().StartWith(expected);
        }

        [DataTestMethod]
        [TypeWithLengthDataSource]
        public void MapPropertyToSql_TypeHasLength_ShouldAddLengthToType(
            ValidType type,
            int length,
            string expected
        )
        {
            //Assemble
            var property = new PropertyInfo
            {
                CSharpType = type,
                Length = length
            };

            //Act
            var actual = SqlMapper.MapPropertyToSql(property);

            //Assert
            actual.SqlType.Should().Be(expected);
        }

        [DataTestMethod]
        [TypeWithPrecisionAndScaleDataSource]
        public void MapPropertyToSql_TypeHasPrecisionAndScale_ShouldAddPrecisionAndScaleToType(
            ValidType type,
            int precision,
            int scale,
            string expected
        )
        {
            //Assemble
            var property = new PropertyInfo
            {
                CSharpType = type,
                Scale = scale,
                Precision = precision
            };

            //Act
            var actual = SqlMapper.MapPropertyToSql(property);

            //Assert
            actual.SqlType.Should().Be(expected);
        }
    }

    internal abstract class BaseTypeDataSourceAttribute : Attribute, ITestDataSource
    {
        public abstract List<object[]> SqlTypes { get; }
        public IEnumerable<object[]> GetData(MethodInfo methodInfo) =>
            SqlTypes;

        public string GetDisplayName(MethodInfo methodInfo, object[] data) =>
            $"{Enum.GetName<ValidType>((ValidType)data[0])}";
    }

    internal class BuiltInTypeDataSourceAttribute : BaseTypeDataSourceAttribute
    {
        private List<object[]> _sqlTypes = new List<object[]>()
        {
            new object[] { ValidType.Bool, "BIT", } ,
            new object[] { ValidType.Decimal, "DECIMAL", },
            new object[] { ValidType.Double, "DECIMAL", },
            new object[] { ValidType.Int, "INT", },
            new object[] { ValidType.String, "VARCHAR", },
        };
        public override List<object[]> SqlTypes { get { return _sqlTypes; } }
    }

    internal class TypeWithLengthDataSourceAttribute : BaseTypeDataSourceAttribute
    {
        private const int _length = 5;
        private List<object[]> _sqlTypes = new List<object[]>()
        {
            new object[] { ValidType.String, _length, $"VARCHAR({_length})", },
        };
        public override List<object[]> SqlTypes { get { return _sqlTypes; } }
    }

    internal class TypeWithPrecisionAndScaleDataSourceAttribute : BaseTypeDataSourceAttribute
    {
        private const int _scale = 5;
        private const int _precision = 7;
        private List<object[]> _sqlTypes = new List<object[]>()
        {
            new object[]
            {
                ValidType.Decimal,
                _precision,
                _scale,
                $"DECIMAL({_precision}, {_scale})",
            },
            new object[]
            {
                ValidType.Double,
                _precision,
                _scale,
                $"DECIMAL({_precision}, {_scale})",
            },
        };
        public override List<object[]> SqlTypes { get { return _sqlTypes; } }
    }
}