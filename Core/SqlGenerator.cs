using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class SqlGenerator
    {
        private static readonly string _nl = Environment.NewLine;
        private readonly ClassInfo _classInfo;
        private readonly string _schemaName;
        private readonly IEnumerable<PropertyInfo> _idProperties;
        private readonly IEnumerable<PropertyInfo> _nonIdProperties;
        private readonly IEnumerable<PropertyInfo> _orderedProperties;
        public SqlGenerator(ClassInfo info, string schemaName)
        {
            _classInfo = info;
            _schemaName = schemaName;
            _idProperties = info.Properties
                .Where(x => x.IsIdProperty)
                .OrderBy(x => x.SqlName);
            _nonIdProperties = info.Properties
                .Where(x => !x.IsIdProperty)
                .OrderBy(x => x.SqlName);
            _orderedProperties = _idProperties.Concat(_nonIdProperties);
        }

        public IEnumerable<SqlFile> GetSql()
        {
            var builder = new StringBuilder();
            builder.Append($"CREATE TABLE {_classInfo.SqlTableName} ({_nl}");
            builder.AppendJoin(
                _nl,
                _orderedProperties.Select(x => GetColumnDefinition(x))
            );
            builder.Append(_nl);
            builder.Append(
                $"\tCONSTRAINT pk_{_classInfo.SqlTableName} PRIMARY KEY ("
            );
            builder.AppendJoin(", ", _idProperties.Select(x => x.SqlName));
            builder.Append($"){_nl}");
            builder.Append(')');
            yield return new SqlFile
            {
                Name = $"{_schemaName}.{_classInfo.SqlTableName}.sql",
                Content = builder.ToString()
            };
            builder.Clear();

            var getByIdProcName = GetProcedureName("get_by_id");
            builder.Append(
                GetProcedureStart(getByIdProcName, _idProperties)
            );
            builder.Append(GetSelectSql());
            builder.Append(GetWhereByIdClause());
            builder.Append("END");
            yield return new SqlFile
            {
                Name = $"{getByIdProcName}.sql",
                Content = builder.ToString()
            };
            builder.Clear();

            var getAllProcName = GetProcedureName("get_all");
            builder.Append(
                GetProcedureStart(getAllProcName, new List<PropertyInfo>())
            );
            builder.Append(GetSelectSql());
            builder.Append("END");
            yield return new SqlFile
            {
                Name = $"{getAllProcName}.sql",
                Content = builder.ToString()
            };
            builder.Clear();

            var createProcName = GetProcedureName("insert");
            builder.Append(
                GetProcedureStart(createProcName, _nonIdProperties)
            );
            builder.Append(
                $"\tINSERT {_schemaName}.{_classInfo.SqlTableName} ({_nl}"
            );
            builder.AppendJoin(
                $",{_nl}",
                _nonIdProperties.Select(x => $"\t\t[{x.SqlName}]")
            );
            builder.Append($"{_nl}\t){_nl}");
            builder.Append($"\tVALUES ({_nl}");
            builder.AppendJoin(
                $",{_nl}",
                _nonIdProperties.Select(x => $"\t\t@{x.SqlName}")
            );
            builder.Append($"{_nl}\t);{_nl}{_nl}");
            builder.Append($"\tSELECT SCOPE_IDENTITY(){_nl}");
            builder.Append("END");
            yield return new SqlFile
            {
                Name = $"{createProcName}.sql",
                Content = builder.ToString()
            };
            builder.Clear();

            var updateProcName = GetProcedureName("update");
            builder.Append(GetProcedureStart(
                updateProcName,
                _orderedProperties
            ));
            builder.Append(
                $"\tUPDATE {_schemaName}.{_classInfo.SqlTableName}{_nl}"
            );
            builder.Append($"\tSET{_nl}");
            builder.AppendJoin(
                $",{_nl}",
                _nonIdProperties.Select(x => $"\t\t[{x.SqlName}] = @{x.SqlName}")
            );
            builder.Append($"{_nl}");
            builder.Append(GetWhereByIdClause());
            builder.Append("END");
            yield return new SqlFile
            {
                Name = $"{updateProcName}.sql",
                Content = builder.ToString()
            };
            builder.Clear();

            var deleteProcName = GetProcedureName("delete");
            builder.Append(GetProcedureStart(
                deleteProcName,
                _idProperties
            ));
            builder.Append($"\tDELETE{_nl}");
            builder.Append(
                $"\tFROM {_schemaName}.{_classInfo.SqlTableName}{_nl}"
            );
            builder.Append(GetWhereByIdClause());
            builder.Append("END");
            yield return new SqlFile
            {
                Name = $"{deleteProcName}.sql",
                Content = builder.ToString()
            };
        }

        private static string GetColumnDefinition(PropertyInfo property)
        {
            var nullDefiniton = property.IsNullable ? "NULL" : "NOT NULL";
            return $"\t{property.SqlName} {property.SqlType} {nullDefiniton},";
        }

        private static string GetProcedureStart(
            string procedureName,
            IEnumerable<PropertyInfo> inputProperties
        )
        {
            var parameterDeclarations = inputProperties.Select(x =>
                $"\t@{x.SqlName} {x.SqlType}"
            );
            var builder = new StringBuilder();
            builder.Append("CREATE OR ALTER PROCEDURE ");
            builder.Append($"{procedureName}{_nl}");
            if (parameterDeclarations.Any())
            {
                builder.AppendJoin($",{_nl}", parameterDeclarations);
                builder.Append($"{_nl}");
            }
            builder.Append($"AS{_nl}BEGIN{_nl}");
            return builder.ToString();
        }

        private string GetProcedureName(string procedureNameSuffix) =>
            $"{_schemaName}.{_classInfo.SqlTableName}_{procedureNameSuffix}";

        private string GetSelectSql()
        {
            var builder = new StringBuilder();
            builder.Append($"\tSELECT{_nl}");
            builder.AppendJoin(
                $",{_nl}",
                _orderedProperties.Select(
                    x => $"\t\t[{x.CSharpName}] = [{x.SqlName}]"
                )
            );
            builder.Append(
                $"{_nl}\tFROM {_schemaName}.{_classInfo.SqlTableName}{_nl}"
            );
            return builder.ToString();
        }

        private string GetWhereByIdClause()
        {
            var builder = new StringBuilder();
            builder.Append($"\tWHERE{_nl}");
            builder.AppendJoin(
                $",{_nl}",
                _idProperties.Select(x => $"\t\t[{x.SqlName}] = @{x.SqlName}")
            );
            builder.Append(_nl);
            return builder.ToString();
        }
    }

    public class SqlFile
    {
        public string Content { get; set; } = "";
        public string Name { get; set; } = "";
    }
}