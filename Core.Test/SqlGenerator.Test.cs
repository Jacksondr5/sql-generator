using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.Test
{
    [TestClass]
    public class SqlGeneratorTest
    {
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
            var builder = new StringBuilder();
            builder.Append(
                $"CREATE OR ALTER PROCEDURE {_schema}.{_info.SqlClassName}_get_by_id\n"
            );
            builder.Append($"\t@{idProperty.SqlName} {idProperty.SqlType}\n");
            builder.Append("AS\n");
            builder.Append("BEGIN\n");
            builder.Append("\tSELECT\n");
            builder.Append(
                $"\t\t[{idProperty.CSharpName}] = [{idProperty.SqlName}],\n"
            );
            builder.Append(
                $"\t\t[{_info.Properties[1].CSharpName}] = [{_info.Properties[1].SqlName}],\n"
            );
            builder.Append(
                $"\t\t[{_info.Properties[2].CSharpName}] = [{_info.Properties[2].SqlName}]\n"
            );
            builder.Append($"\tFROM {_schema}.{_info.SqlClassName}\n");
            builder.Append(
                $"\tWHERE [{idProperty.SqlName}] = @{idProperty.SqlName}\n"
            );
            builder.Append("END");
            var expected = builder.ToString();

            //Act
            var actual = SqlGenerator.GetCrudStoredProcedures(_info, _schema);

            //Assert
            actual.GetById.Should().Be(expected);
        }

        [TestMethod]
        public void GetCrudStoredProcedures_ShouldGenerateCreateProc()
        {
            //Assemble
            var builder = new StringBuilder();
            builder.Append(
                $"CREATE OR ALTER PROCEDURE {_schema}.{_info.SqlClassName}_create\n"
            );
            builder.Append(
                $"\t@{_info.Properties[1].SqlName} {_info.Properties[1].SqlType},\n"
            );
            builder.Append(
                $"\t@{_info.Properties[2].SqlName} {_info.Properties[2].SqlType}\n"
            );
            builder.Append("AS\n");
            builder.Append("BEGIN\n");
            builder.Append($"\tINSERT {_schema}.{_info.SqlClassName} (\n");
            builder.Append($"\t\t{_info.Properties[1].SqlName},\n");
            builder.Append($"\t\t{_info.Properties[2].SqlName}\n");
            builder.Append("\t)\n");
            builder.Append("\tVALUES (\n");
            builder.Append($"\t\t@{_info.Properties[1].SqlName},\n");
            builder.Append($"\t\t@{_info.Properties[2].SqlName}\n");
            builder.Append("\t);\n");
            builder.Append("\n");
            builder.Append("\tSELECT SCOPE_IDENTITY()\n");
            builder.Append("END");
            var expected = builder.ToString();

            //Act
            var actual = SqlGenerator.GetCrudStoredProcedures(_info, _schema);

            //Assert
            actual.Create.Should().Be(expected);
        }

        [TestMethod]
        public void GetCrudStoredProcedures_ShouldGenerateUpdateProc()
        {
            //Assemble
            var idProperty = _info.Properties[0];
            var builder = new StringBuilder();
            builder.Append(
                $"CREATE OR ALTER PROCEDURE {_schema}.{_info.SqlClassName}_update\n"
            );
            builder.Append($"\t@{idProperty.SqlName} {idProperty.SqlType},\n");
            builder.Append(
                $"\t@{_info.Properties[1].SqlName} {_info.Properties[1].SqlType},\n"
            );
            builder.Append(
                $"\t@{_info.Properties[2].SqlName} {_info.Properties[2].SqlType}\n"
            );
            builder.Append($"AS\n");
            builder.Append($"BEGIN\n");
            builder.Append($"\tUPDATE {_schema}.{_info.SqlClassName}\n");
            builder.Append($"\tSET\n");
            builder.Append(
                $"\t\t[{_info.Properties[1].SqlName}] = @{_info.Properties[1].SqlName},\n"
            );
            builder.Append(
                $"\t\t[{_info.Properties[2].SqlName}] = @{_info.Properties[2].SqlName}\n"
            );
            builder.Append(
                $"\tWHERE [{idProperty.SqlName}] = @{idProperty.SqlName}\n"
            );
            builder.Append("END");
            var expected = builder.ToString();

            //Act
            var actual = SqlGenerator.GetCrudStoredProcedures(_info, _schema);

            //Assert
            actual.Update.Should().Be(expected);
        }

        [TestMethod]
        public void GetCrudStoredProcedures_ShouldGenerateDeleteProc()
        {
            //Assemble
            var idProperty = _info.Properties[0];
            var builder = new StringBuilder();
            builder.Append(
                $"CREATE OR ALTER PROCEDURE {_schema}.{_info.SqlClassName}_delete\n"
            );
            builder.Append($"\t@{idProperty.SqlName} {idProperty.SqlType}\n");
            builder.Append("AS\n");
            builder.Append("BEGIN\n");
            builder.Append($"\tDELETE\n");
            builder.Append($"\tFROM {_schema}.{_info.SqlClassName}\n");
            builder.Append(
                $"\tWHERE [{idProperty.SqlName}] = @{idProperty.SqlName}\n"
            );
            builder.Append("END");
            var expected = builder.ToString();

            //Act
            var actual = SqlGenerator.GetCrudStoredProcedures(_info, _schema);

            //Assert
            actual.Delete.Should().Be(expected);
        }
    }
}