using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
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
                ValidType = ValidType.Int
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
                ValidType = type
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
            string length,
            string expected
        )
        {
            //Assemble
            var property = new PropertyInfo
            {
                ValidType = type,
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
                ValidType = type,
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
        private Dictionary<ValidType, string> _sqlTypes = new Dictionary<ValidType, string>
        {
            { ValidType.Bool, "BIT" } ,
            { ValidType.DateTime, "DATETIME" },
            { ValidType.Decimal, "DECIMAL" },
            { ValidType.Double, "DECIMAL" },
            { ValidType.Enum, "INT" },
            { ValidType.Int, "INT" },
            { ValidType.List, "VARCHAR" },
            { ValidType.String, "VARCHAR" },
        };
        public override List<object[]> SqlTypes
        {
            get
            {
                return Enum
                    .GetValues<ValidType>()
                    .Where(x => x != ValidType.InvalidType)
                    .Select(x => new object[]
                    {
                        x,
                        _sqlTypes.GetValueOrDefault(x) ??
                            throw new InvalidOperationException(
                                $"The type {x} is missing from the test dictionary"
                            )
                    })
                    .ToList();
            }
        }
    }

    internal class TypeWithLengthDataSourceAttribute : BaseTypeDataSourceAttribute
    {
        private List<object[]> _sqlTypes = new List<object[]>()
        {
            new object[] { ValidType.String, "5", $"VARCHAR(5)" },
            new object[] { ValidType.String, "MAX", $"VARCHAR(MAX)" },
            new object[] { ValidType.List, "5", $"VARCHAR(5)" },
            new object[] { ValidType.List, "MAX", $"VARCHAR(MAX)" }
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
                $"DECIMAL({_precision}, {_scale})"
            },
            new object[]
            {
                ValidType.Double,
                _precision,
                _scale,
                $"DECIMAL({_precision}, {_scale})"
            },
        };
        public override List<object[]> SqlTypes { get { return _sqlTypes; } }
    }
}