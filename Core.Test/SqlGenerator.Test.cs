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
        private static readonly ClassInfo _info = new ClassInfo
        {
            CSharpClassName = "TestClass",
            Properties = new List<PropertyInfo>
            {
                new PropertyInfo
                {
                    CSharpName = "Id",
                    CSharpType = ValidType.Int,
                    IsIdProperty = true,
                    SqlName = "test_class_id",
                    SqlType = "INT"
                },
                new PropertyInfo
                {
                    CSharpName = "AnotherTestProperty",
                    CSharpType = ValidType.Int,
                    SqlName = "another_test_property",
                    SqlType = "INT"
                },
                new PropertyInfo
                {
                    CSharpName = "TestProperty",
                    CSharpType = ValidType.String,
                    SqlName = "test_property",
                    SqlType = "VARCHAR(128)"
                },
            },
            SqlClassName = "test_class"
        };
        private const string _schema = "dbo";

        [TestMethod]
        public void GetCrudStoredProcedures_ShouldGenerateGetByIdProc()
        {
            //Assemble
            var idProperty = _info.Properties[0];
            var procName = $"{_schema}.{_info.SqlClassName}_get_by_id";
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
                $"\t\t[{_info.Properties[1].CSharpName}] = [{_info.Properties[1].SqlName}],{_nl}"
            );
            builder.Append(
                $"\t\t[{_info.Properties[2].CSharpName}] = [{_info.Properties[2].SqlName}]{_nl}"
            );
            builder.Append($"\tFROM {_schema}.{_info.SqlClassName}{_nl}");
            builder.Append(
                $"\tWHERE [{idProperty.SqlName}] = @{idProperty.SqlName}{_nl}"
            );
            builder.Append("END");
            var expected = new SqlFile
            {
                Content = builder.ToString(),
                Name = $"{procName}.sql"
            };

            //Act
            var actual = SqlGenerator.GetCrudStoredProcedures(_info, _schema);

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Be(expected.Content);
        }

        [TestMethod]
        public void GetCrudStoredProcedures_ShouldGenerateCreateProc()
        {
            //Assemble
            var procName = $"{_schema}.{_info.SqlClassName}_insert";
            var builder = new StringBuilder();
            builder.Append(
                $"CREATE OR ALTER PROCEDURE {procName}{_nl}"
            );
            builder.Append(
                $"\t@{_info.Properties[1].SqlName} {_info.Properties[1].SqlType},{_nl}"
            );
            builder.Append(
                $"\t@{_info.Properties[2].SqlName} {_info.Properties[2].SqlType}{_nl}"
            );
            builder.Append($"AS{_nl}");
            builder.Append($"BEGIN{_nl}");
            builder.Append($"\tINSERT {_schema}.{_info.SqlClassName} ({_nl}");
            builder.Append($"\t\t{_info.Properties[1].SqlName},{_nl}");
            builder.Append($"\t\t{_info.Properties[2].SqlName}{_nl}");
            builder.Append($"\t){_nl}");
            builder.Append($"\tVALUES ({_nl}");
            builder.Append($"\t\t@{_info.Properties[1].SqlName},{_nl}");
            builder.Append($"\t\t@{_info.Properties[2].SqlName}{_nl}");
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
            var actual = SqlGenerator.GetCrudStoredProcedures(_info, _schema);

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Be(expected.Content);
        }

        [TestMethod]
        public void GetCrudStoredProcedures_ShouldGenerateUpdateProc()
        {
            //Assemble
            var idProperty = _info.Properties[0];
            var procName = $"{_schema}.{_info.SqlClassName}_update";
            var builder = new StringBuilder();
            builder.Append(
                $"CREATE OR ALTER PROCEDURE {procName}{_nl}"
            );
            builder.Append($"\t@{idProperty.SqlName} {idProperty.SqlType},{_nl}");
            builder.Append(
                $"\t@{_info.Properties[1].SqlName} {_info.Properties[1].SqlType},{_nl}"
            );
            builder.Append(
                $"\t@{_info.Properties[2].SqlName} {_info.Properties[2].SqlType}{_nl}"
            );
            builder.Append($"AS{_nl}");
            builder.Append($"BEGIN{_nl}");
            builder.Append($"\tUPDATE {_schema}.{_info.SqlClassName}{_nl}");
            builder.Append($"\tSET{_nl}");
            builder.Append(
                $"\t\t[{_info.Properties[1].SqlName}] = @{_info.Properties[1].SqlName},{_nl}"
            );
            builder.Append(
                $"\t\t[{_info.Properties[2].SqlName}] = @{_info.Properties[2].SqlName}{_nl}"
            );
            builder.Append(
                $"\tWHERE [{idProperty.SqlName}] = @{idProperty.SqlName}{_nl}"
            );
            builder.Append("END");
            var expected = new SqlFile
            {
                Content = builder.ToString(),
                Name = $"{procName}.sql"
            };

            //Act
            var actual = SqlGenerator.GetCrudStoredProcedures(_info, _schema);

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Be(expected.Content);
        }

        [TestMethod]
        public void GetCrudStoredProcedures_ShouldGenerateDeleteProc()
        {
            //Assemble
            var idProperty = _info.Properties[0];
            var procName = $"{_schema}.{_info.SqlClassName}_delete";
            var builder = new StringBuilder();
            builder.Append(
                $"CREATE OR ALTER PROCEDURE {procName}{_nl}"
            );
            builder.Append($"\t@{idProperty.SqlName} {idProperty.SqlType}{_nl}");
            builder.Append($"AS{_nl}");
            builder.Append($"BEGIN{_nl}");
            builder.Append($"\tDELETE{_nl}");
            builder.Append($"\tFROM {_schema}.{_info.SqlClassName}{_nl}");
            builder.Append(
                $"\tWHERE [{idProperty.SqlName}] = @{idProperty.SqlName}{_nl}"
            );
            builder.Append("END");
            var expected = new SqlFile
            {
                Content = builder.ToString(),
                Name = $"{procName}.sql"
            };

            //Act
            var actual = SqlGenerator.GetCrudStoredProcedures(_info, _schema);

            //Assert
            actual.Should().Contain(x => x.Name.Equals(expected.Name));
            actual
                .Where(x => x.Name.Equals(expected.Name))
                .First().Content
                .Should()
                .Be(expected.Content);
        }
    }
}