using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Test
{
    [TestClass]
    public class SqlGeneratorTest
    {
        private static readonly string _nl = Environment.NewLine;
        private static readonly ClassInfo _simpleInfo = new ClassInfo
        {
            CSharpClassName = "TestClass",
            Properties = new List<PropertyInfo>
            {
                new PropertyInfo
                {
                    CSharpName = "Id",
                    ValidType = ValidType.Int,
                    IsIdProperty = true,
                    SqlName = "test_class_id",
                    SqlType = "INT"
                },
                new PropertyInfo
                {
                    CSharpName = "AnotherTestProperty",
                    ValidType = ValidType.Int,
                    SqlName = "another_test_property",
                    SqlType = "INT"
                },
                new PropertyInfo
                {
                    CSharpName = "TestProperty",
                    ValidType = ValidType.String,
                    SqlName = "test_property",
                    SqlType = "VARCHAR(128)"
                },
            },
            SqlTableName = "test_class"
        };
        private const string _schema = "dbo";
        private static readonly SqlGenerator _generator = new SqlGenerator(
            _simpleInfo,
            _schema
        );

        [TestMethod]
        public void GetSql_ShouldGenerateGetByIdProc()
        {
            //Assemble
            var idProperty = _simpleInfo.Properties[0];
            var procName = $"{_schema}.{_simpleInfo.SqlTableName}_get_by_id";
            var builder = new StringBuilder();
            builder.Append(
                $"CREATE OR ALTER PROCEDURE {procName}{_nl}"
            );
            builder.Append($"\t@{idProperty.SqlName} {idProperty.SqlType}{_nl}");
            builder.Append($"AS{_nl}");
            builder.Append($"BEGIN{_nl}");
            builder.Append($"\tSELECT{_nl}");
            builder.Append(
                $"\t\t[{idProperty.CSharpName}] = [{idProperty.SqlName}],{_nl}"
            );
            builder.Append(
                $"\t\t[{_simpleInfo.Properties[1].CSharpName}] = [{_simpleInfo.Properties[1].SqlName}],{_nl}"
            );
            builder.Append(
                $"\t\t[{_simpleInfo.Properties[2].CSharpName}] = [{_simpleInfo.Properties[2].SqlName}]{_nl}"
            );
            builder.Append($"\tFROM {_schema}.{_simpleInfo.SqlTableName}{_nl}");
            builder.Append($"\tWHERE{_nl}");
            builder.Append(
                $"\t\t[{idProperty.SqlName}] = @{idProperty.SqlName}{_nl}"
            );
            builder.Append("END");
            var expected = new SqlFile
            {
                Content = builder.ToString(),
                Name = $"{procName}.sql"
            };

            //Act
            var actual = _generator.GetSql();

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Be(expected.Content);
        }

        [TestMethod]
        public void GetSql_ShouldGenerateCreateProc()
        {
            //Assemble
            var procName = $"{_schema}.{_simpleInfo.SqlTableName}_insert";
            var builder = new StringBuilder();
            builder.Append(
                $"CREATE OR ALTER PROCEDURE {procName}{_nl}"
            );
            builder.Append(
                $"\t@{_simpleInfo.Properties[1].SqlName} {_simpleInfo.Properties[1].SqlType},{_nl}"
            );
            builder.Append(
                $"\t@{_simpleInfo.Properties[2].SqlName} {_simpleInfo.Properties[2].SqlType}{_nl}"
            );
            builder.Append($"AS{_nl}");
            builder.Append($"BEGIN{_nl}");
            builder.Append(
                $"\tINSERT {_schema}.{_simpleInfo.SqlTableName} ({_nl}"
            );
            builder.Append($"\t\t[{_simpleInfo.Properties[1].SqlName}],{_nl}");
            builder.Append($"\t\t[{_simpleInfo.Properties[2].SqlName}]{_nl}");
            builder.Append($"\t){_nl}");
            builder.Append($"\tVALUES ({_nl}");
            builder.Append($"\t\t@{_simpleInfo.Properties[1].SqlName},{_nl}");
            builder.Append($"\t\t@{_simpleInfo.Properties[2].SqlName}{_nl}");
            builder.Append($"\t);{_nl}");
            builder.Append($"{_nl}");
            builder.Append($"\tSELECT SCOPE_IDENTITY(){_nl}");
            builder.Append("END");
            var expected = new SqlFile
            {
                Content = builder.ToString(),
                Name = $"{procName}.sql"
            };

            //Act
            var actual = _generator.GetSql();

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Be(expected.Content);
        }

        [TestMethod]
        public void GetSql_ShouldGenerateUpdateProc()
        {
            //Assemble
            var idProperty = _simpleInfo.Properties[0];
            var procName = $"{_schema}.{_simpleInfo.SqlTableName}_update";
            var builder = new StringBuilder();
            builder.Append(
                $"CREATE OR ALTER PROCEDURE {procName}{_nl}"
            );
            builder.Append($"\t@{idProperty.SqlName} {idProperty.SqlType},{_nl}");
            builder.Append(
                $"\t@{_simpleInfo.Properties[1].SqlName} {_simpleInfo.Properties[1].SqlType},{_nl}"
            );
            builder.Append(
                $"\t@{_simpleInfo.Properties[2].SqlName} {_simpleInfo.Properties[2].SqlType}{_nl}"
            );
            builder.Append($"AS{_nl}");
            builder.Append($"BEGIN{_nl}");
            builder.Append(
                $"\tUPDATE {_schema}.{_simpleInfo.SqlTableName}{_nl}"
            );
            builder.Append($"\tSET{_nl}");
            builder.Append(
                $"\t\t[{_simpleInfo.Properties[1].SqlName}] = @{_simpleInfo.Properties[1].SqlName},{_nl}"
            );
            builder.Append(
                $"\t\t[{_simpleInfo.Properties[2].SqlName}] = @{_simpleInfo.Properties[2].SqlName}{_nl}"
            );
            builder.Append($"\tWHERE{_nl}");
            builder.Append(
                $"\t\t[{idProperty.SqlName}] = @{idProperty.SqlName}{_nl}"
            );
            builder.Append("END");
            var expected = new SqlFile
            {
                Content = builder.ToString(),
                Name = $"{procName}.sql"
            };

            //Act
            var actual = _generator.GetSql();

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Be(expected.Content);
        }

        [TestMethod]
        public void GetSql_ShouldGenerateDeleteProc()
        {
            //Assemble
            var idProperty = _simpleInfo.Properties[0];
            var procName = $"{_schema}.{_simpleInfo.SqlTableName}_delete";
            var builder = new StringBuilder();
            builder.Append(
                $"CREATE OR ALTER PROCEDURE {procName}{_nl}"
            );
            builder.Append($"\t@{idProperty.SqlName} {idProperty.SqlType}{_nl}");
            builder.Append($"AS{_nl}");
            builder.Append($"BEGIN{_nl}");
            builder.Append($"\tDELETE{_nl}");
            builder.Append($"\tFROM {_schema}.{_simpleInfo.SqlTableName}{_nl}");
            builder.Append($"\tWHERE{_nl}");
            builder.Append(
                $"\t\t[{idProperty.SqlName}] = @{idProperty.SqlName}{_nl}"
            );
            builder.Append("END");
            var expected = new SqlFile
            {
                Content = builder.ToString(),
                Name = $"{procName}.sql"
            };

            //Act
            var actual = _generator.GetSql();

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Be(expected.Content);
        }

        [TestMethod]
        public void GetSql_ShouldGenerateGetAllProc()
        {
            var idProperty = _simpleInfo.Properties[0];
            var procName = $"{_schema}.{_simpleInfo.SqlTableName}_get_all";
            var builder = new StringBuilder();
            builder.Append(
                $"CREATE OR ALTER PROCEDURE {procName}{_nl}"
            );
            builder.Append($"AS{_nl}");
            builder.Append($"BEGIN{_nl}");
            builder.Append($"\tSELECT{_nl}");
            builder.Append(
                $"\t\t[{idProperty.CSharpName}] = [{idProperty.SqlName}],{_nl}"
            );
            builder.Append(
                $"\t\t[{_simpleInfo.Properties[1].CSharpName}] = [{_simpleInfo.Properties[1].SqlName}],{_nl}"
            );
            builder.Append(
                $"\t\t[{_simpleInfo.Properties[2].CSharpName}] = [{_simpleInfo.Properties[2].SqlName}]{_nl}"
            );
            builder.Append($"\tFROM {_schema}.{_simpleInfo.SqlTableName}{_nl}");
            builder.Append("END");
            var expected = new SqlFile
            {
                Content = builder.ToString(),
                Name = $"{procName}.sql"
            };

            //Act
            var actual = _generator.GetSql();

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Be(expected.Content);
        }

        [TestMethod]
        public void GetSql_ShouldGenerateTableDefiniton()
        {
            var idProperty = _simpleInfo.Properties[0];
            var procName = $"{_schema}.{_simpleInfo.SqlTableName}";
            var builder = new StringBuilder();
            builder.Append($"CREATE TABLE {_simpleInfo.SqlTableName} ({_nl}");
            builder.Append(
                $"\t{idProperty.SqlName} {idProperty.SqlType} NOT NULL,{_nl}"
            );
            builder.Append(
                $"\t{_simpleInfo.Properties[1].SqlName} {_simpleInfo.Properties[1].SqlType} NOT NULL,{_nl}"
            );
            builder.Append(
                $"\t{_simpleInfo.Properties[2].SqlName} {_simpleInfo.Properties[2].SqlType} NOT NULL,{_nl}"
            );
            builder.Append(
                $"\tCONSTRAINT pk_{_simpleInfo.SqlTableName} PRIMARY KEY ({idProperty.SqlName}){_nl}"
            );
            builder.Append(")");
            var expected = new SqlFile
            {
                Content = builder.ToString(),
                Name = $"{procName}.sql"
            };

            //Act
            var actual = _generator.GetSql();

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Be(expected.Content);
        }

        private static ClassInfo _multipleIdsInfo = new ClassInfo
        {
            CSharpClassName = "TestClass",
            Properties = new List<PropertyInfo>
            {
                new PropertyInfo
                {
                    CSharpName = "AnotherTestProperty",
                    ValidType = ValidType.Int,
                    IsIdProperty = true,
                    SqlName = "another_test_id_property",
                    SqlType = "INT"
                },
                new PropertyInfo
                {
                    CSharpName = "Id",
                    ValidType = ValidType.Int,
                    IsIdProperty = true,
                    SqlName = "test_id_property",
                    SqlType = "INT"
                },
                new PropertyInfo
                {
                    CSharpName = "TestProperty",
                    ValidType = ValidType.String,
                    SqlName = "test_property",
                    SqlType = "VARCHAR(128)"
                },
            },
            SqlTableName = "test_class"
        };


        [TestMethod]
        public void GetSql_MultipleIdParameters_ShouldOrderGetByIdSelectsProperly()
        {
            //Assemble
            var idProperties =
                _multipleIdsInfo.Properties.Where(x => x.IsIdProperty).ToList();
            var procName =
                $"{_schema}.{_multipleIdsInfo.SqlTableName}_get_by_id";
            var builder = new StringBuilder();
            builder.Append($"\tSELECT{_nl}");
            builder.Append(
                $"\t\t[{idProperties[0].CSharpName}] = [{idProperties[0].SqlName}],{_nl}"
            );
            builder.Append(
                $"\t\t[{idProperties[1].CSharpName}] = [{idProperties[1].SqlName}],{_nl}"
            );
            builder.Append(
                $"\t\t[{_multipleIdsInfo.Properties[2].CSharpName}] = [{_multipleIdsInfo.Properties[2].SqlName}]{_nl}"
            );
            var expected = new SqlFile
            {
                Content = builder.ToString(),
                Name = $"{procName}.sql"
            };
            var generator = new SqlGenerator(_multipleIdsInfo, _schema);

            //Act
            var actual = generator.GetSql();

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Contain(expected.Content);
        }

        [TestMethod]
        public void GetSql_MultipleIdParameters_ShouldOrderGetAllSelectsProperly()
        {
            //Assemble
            var idProperties =
                _multipleIdsInfo.Properties.Where(x => x.IsIdProperty).ToList();
            var procName =
                $"{_schema}.{_multipleIdsInfo.SqlTableName}_get_by_id";
            var builder = new StringBuilder();
            builder.Append($"\tSELECT{_nl}");
            builder.Append(
                $"\t\t[{idProperties[0].CSharpName}] = [{idProperties[0].SqlName}],{_nl}"
            );
            builder.Append(
                $"\t\t[{idProperties[1].CSharpName}] = [{idProperties[1].SqlName}],{_nl}"
            );
            builder.Append(
                $"\t\t[{_multipleIdsInfo.Properties[2].CSharpName}] = [{_multipleIdsInfo.Properties[2].SqlName}]{_nl}"
            );
            var expected = new SqlFile
            {
                Content = builder.ToString(),
                Name = $"{procName}.sql"
            };
            var generator = new SqlGenerator(_multipleIdsInfo, _schema);

            //Act
            var actual = generator.GetSql();

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Contain(expected.Content);
        }

        [TestMethod]
        public void GetSql_MultipleIdParameters_ShouldIncludeInGetByIdWhereClauses()
        {
            //Assemble
            var idProperties =
                _multipleIdsInfo.Properties.Where(x => x.IsIdProperty).ToList();
            var procName =
                $"{_schema}.{_multipleIdsInfo.SqlTableName}_get_by_id";
            var builder = new StringBuilder();
            builder.Append($"\tWHERE{_nl}");
            builder.Append(
                $"\t\t[{idProperties[0].SqlName}] = @{idProperties[0].SqlName},{_nl}"
            );
            builder.Append(
                $"\t\t[{idProperties[1].SqlName}] = @{idProperties[1].SqlName}{_nl}"
            );
            var expected = new SqlFile
            {
                Content = builder.ToString(),
                Name = $"{procName}.sql"
            };
            var generator = new SqlGenerator(_multipleIdsInfo, _schema);

            //Act
            var actual = generator.GetSql();

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Contain(expected.Content);
        }

        [TestMethod]
        public void GetSql_MultipleIdParameters_ShouldIncludeInUpdateWhereClauses()
        {
            //Assemble
            var idProperties =
                _multipleIdsInfo.Properties.Where(x => x.IsIdProperty).ToList();
            var procName =
                $"{_schema}.{_multipleIdsInfo.SqlTableName}_get_by_id";
            var builder = new StringBuilder();
            builder.Append($"\tWHERE{_nl}");
            builder.Append(
                $"\t\t[{idProperties[0].SqlName}] = @{idProperties[0].SqlName},{_nl}"
            );
            builder.Append(
                $"\t\t[{idProperties[1].SqlName}] = @{idProperties[1].SqlName}{_nl}"
            );
            var expected = new SqlFile
            {
                Content = builder.ToString(),
                Name = $"{procName}.sql"
            };
            var generator = new SqlGenerator(_multipleIdsInfo, _schema);

            //Act
            var actual = generator.GetSql();

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Contain(expected.Content);
        }

        [TestMethod]
        public void GetSql_MultipleIdParameters_ShouldIncludeInDeleteWhereClauses()
        {
            //Assemble
            var idProperties =
                _multipleIdsInfo.Properties.Where(x => x.IsIdProperty).ToList();
            var procName =
                $"{_schema}.{_multipleIdsInfo.SqlTableName}_get_by_id";
            var builder = new StringBuilder();
            builder.Append($"\tWHERE{_nl}");
            builder.Append(
                $"\t\t[{idProperties[0].SqlName}] = @{idProperties[0].SqlName},{_nl}"
            );
            builder.Append(
                $"\t\t[{idProperties[1].SqlName}] = @{idProperties[1].SqlName}{_nl}"
            );
            var expected = new SqlFile
            {
                Content = builder.ToString(),
                Name = $"{procName}.sql"
            };
            var generator = new SqlGenerator(_multipleIdsInfo, _schema);

            //Act
            var actual = generator.GetSql();

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Contain(expected.Content);
        }

        [TestMethod]
        public void GetSql_MultipleIdParameters_ShouldGenerateTableDefiniton()
        {
            var idProperties =
                _multipleIdsInfo.Properties.Where(x => x.IsIdProperty).ToList();
            var builder = new StringBuilder();
            builder.Append(
                $"\t{idProperties[0].SqlName} {idProperties[0].SqlType} NOT NULL,{_nl}"
            );
            builder.Append(
                $"\t{idProperties[1].SqlName} {idProperties[1].SqlType} NOT NULL,{_nl}"
            );
            builder.Append(
                $"\t{_multipleIdsInfo.Properties[2].SqlName} {_multipleIdsInfo.Properties[2].SqlType} NOT NULL,{_nl}"
            );
            builder.Append(
                $"\tCONSTRAINT pk_{_multipleIdsInfo.SqlTableName} PRIMARY KEY ({idProperties[0].SqlName}, {idProperties[1].SqlName})"
            );
            var expected = new SqlFile
            {
                Content = builder.ToString(),
                Name = $"{_schema}.{_multipleIdsInfo.SqlTableName}.sql"
            };
            var generator = new SqlGenerator(_multipleIdsInfo, _schema);

            //Act
            var actual = generator.GetSql();

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Contain(expected.Content);
        }

        [TestMethod]
        public void GetSql_PropertyIsNullable_TableDefinitionShouldHaveNullableColumn()
        {
            //Assemble
            var type = new ClassInfo
            {
                CSharpClassName = "TestClass",
                Properties = new List<PropertyInfo>
                {
                    new PropertyInfo
                    {
                        CSharpName = "Id",
                        ValidType = ValidType.Int,
                        IsNullable = true,
                        SqlName = "test_property",
                        SqlType = "INT"
                    },
                },
                SqlTableName = "test_class"
            };
            var expected = new SqlFile
            {
                Content = "\ttest_property INT NULL",
                Name = $"{_schema}.{_multipleIdsInfo.SqlTableName}.sql"
            };
            var generator = new SqlGenerator(type, _schema);

            //Act
            var actual = generator.GetSql();

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Contain(expected.Content);
        }

        [TestMethod]
        public void GetSql_PropertyIsNonNullable_TableDefinitionShouldHaveNullableColumn()
        {
            //Assemble
            var type = new ClassInfo
            {
                CSharpClassName = "TestClass",
                Properties = new List<PropertyInfo>
                {
                    new PropertyInfo
                    {
                        CSharpName = "Id",
                        ValidType = ValidType.Int,
                        IsNullable = false,
                        SqlName = "test_property",
                        SqlType = "INT"
                    },
                },
                SqlTableName = "test_class"
            };
            var expected = new SqlFile
            {
                Content = "\ttest_property INT NOT NULL",
                Name = $"{_schema}.{_multipleIdsInfo.SqlTableName}.sql"
            };
            var generator = new SqlGenerator(type, _schema);

            //Act
            var actual = generator.GetSql();

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Contain(expected.Content);
        }
    }
}