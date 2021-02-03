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
            var expectedSqlName = "public_test_string";

            //Act
            var actual = SqlMapper.MapPropertyToSql(property, "");

            //Assert
            actual.SqlName.Should().Be(expectedSqlName);
        }

        [DataTestMethod]
        [BuiltInTypeDataSource]
        public void MapPropertyToSql_ShouldMapSqlType(
            ValidType type,
            string expectedSqlType
        )
        {
            //Assemble
            var property = new PropertyInfo
            {
                ValidType = type
            };

            //Act
            var actual = SqlMapper.MapPropertyToSql(property, "");

            //Assert
            actual.SqlType.Should().StartWith(expectedSqlType);
        }

        [DataTestMethod]
        [TypeWithLengthDataSource]
        public void MapPropertyToSql_TypeHasLength_ShouldAddLengthToType(
            ValidType type,
            string length,
            string expectedSqlType
        )
        {
            //Assemble
            var property = new PropertyInfo
            {
                ValidType = type,
                Length = length
            };

            //Act
            var actual = SqlMapper.MapPropertyToSql(property, "");

            //Assert
            actual.SqlType.Should().StartWith(expectedSqlType);
        }

        [DataTestMethod]
        [TypeWithPrecisionAndScaleDataSource]
        public void MapPropertyToSql_TypeHasPrecisionAndScale_ShouldAddPrecisionAndScaleToType(
            ValidType type,
            int precision,
            int scale,
            string expectedSqlType
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
            var actual = SqlMapper.MapPropertyToSql(property, "");

            //Assert
            actual.SqlType.Should().StartWith(expectedSqlType);
        }

        [TestMethod]
        public void MapPropertyToSql_TypeIsNullable_ShouldIncludeNull()
        {
            //Assemble
            var property = new PropertyInfo
            {
                CSharpName = "PublicTestString",
                IsNullable = true,
                ValidType = ValidType.Int,
            };
            var expectedSqlType = "INT NULL";

            //Act
            var actual = SqlMapper.MapPropertyToSql(property, "");

            //Assert
            actual.SqlType.Should().Be(expectedSqlType);
        }

        [TestMethod]
        public void MapPropertyToSql_TypeIsNotNullable_ShouldIncludeNotNull()
        {
            //Assemble
            var property = new PropertyInfo
            {
                CSharpName = "PublicTestString",
                IsNullable = false,
                ValidType = ValidType.Int,
            };
            var expectedSqlType = "INT NOT NULL";

            //Act
            var actual = SqlMapper.MapPropertyToSql(property, "");

            //Assert
            actual.SqlType.Should().Be(expectedSqlType);
        }

        [TestMethod]
        public void MapPropertyToSql_PropertyIsNamedId_ShouldPrependTableNameToSqlName()
        {
            //Assemble
            var property = new PropertyInfo
            {
                CSharpName = "Id",
                IsIdProperty = true,
                ValidType = ValidType.Int
            };
            var tableName = "test";
            var expectedSqlName = $"{tableName}_id";

            //Act
            var actual = SqlMapper.MapPropertyToSql(property, tableName);

            //Assert
            actual.SqlName.Should().Be(expectedSqlName);
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
                        _sqlTypes.GetValueOrDefault(x) ?? Throw(x)
                    })
                    .ToList();
            }
        }

        //VS Code didn't like this being inline, text coloring got messed up
        private static string Throw(ValidType x) =>
            throw new InvalidOperationException(
                $"The type {x} is missing from the test dictionary"
            );
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