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
            var builder = new StringBuilder();
            builder.Append(
                $"CREATE OR ALTER PROCEDURE {_schema}.{_info.SqlClassName}_get_by_id{_nl}"
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
                $"CREATE OR ALTER PROCEDURE {_schema}.{_info.SqlClassName}_create{_nl}"
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
                $"CREATE OR ALTER PROCEDURE {_schema}.{_info.SqlClassName}_update{_nl}"
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
                $"CREATE OR ALTER PROCEDURE {_schema}.{_info.SqlClassName}_delete{_nl}"
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
            var expected = builder.ToString();

            //Act
            var actual = SqlGenerator.GetCrudStoredProcedures(_info, _schema);

            //Assert
            actual.Delete.Should().Be(expected);
        }
    }
}