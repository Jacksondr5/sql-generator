using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class SqlGenerator
    {
        private static readonly string _nl = Environment.NewLine;
        public static IEnumerable<SqlFile> GetCrudStoredProcedures(
            ClassInfo info,
            string schemaName
        )
        {
            var idProperty = info.Properties.Find(x => x.IsIdProperty);
            if (idProperty == null)
                throw new InvalidOperationException("shouldn't happen");
            var nonIdProperties = info.Properties
                .FindAll(x => !x.IsIdProperty)
                .OrderBy(x => x.CSharpName);

            var getBuilder = new StringBuilder();
            var getProcName = GetProcedureName(
                schemaName,
                info.SqlClassName,
                "get_by_id"
            );
            getBuilder.Append(GetProcedureStart(
                getProcName,
                new List<PropertyInfo> { idProperty }
            ));
            getBuilder.Append($"\tSELECT{_nl}");
            getBuilder.Append($"{FormatSelect(idProperty)},{_nl}");
            getBuilder.AppendJoin(
                $",{_nl}",
                nonIdProperties.Select(x => FormatSelect(x))
            );
            getBuilder.Append($"{_nl}\tFROM {schemaName}.{info.SqlClassName}{_nl}");
            getBuilder.Append(
                $"\tWHERE [{idProperty.SqlName}] = @{idProperty.SqlName}{_nl}"
            );
            getBuilder.Append("END");
            yield return new SqlFile
            {
                Name = $"{getProcName}.sql",
                Content = getBuilder.ToString()
            };

            var createBuilder = new StringBuilder();
            var createProcName = GetProcedureName(
                schemaName,
                info.SqlClassName,
                "insert"
            );
            createBuilder.Append(
                GetProcedureStart(createProcName, nonIdProperties)
            );
            createBuilder.Append(
                $"\tINSERT {schemaName}.{info.SqlClassName} ({_nl}"
            );
            createBuilder.AppendJoin(
                $",{_nl}",
                nonIdProperties.Select(x => $"\t\t{x.SqlName}")
            );
            createBuilder.Append($"{_nl}\t){_nl}");
            createBuilder.Append($"\tVALUES ({_nl}");
            createBuilder.AppendJoin(
                $",{_nl}",
                nonIdProperties.Select(x => $"\t\t@{x.SqlName}")
            );
            createBuilder.Append($"{_nl}\t);{_nl}{_nl}");
            createBuilder.Append($"\tSELECT SCOPE_IDENTITY(){_nl}");
            createBuilder.Append("END");
            yield return new SqlFile
            {
                Name = $"{createProcName}.sql",
                Content = createBuilder.ToString()
            };

            var updateBuilder = new StringBuilder();
            var updateProcName = GetProcedureName(
                schemaName,
                info.SqlClassName,
                "update"
            );
            updateBuilder.Append(GetProcedureStart(
                updateProcName,
                nonIdProperties.Prepend(idProperty)
            ));
            updateBuilder.Append($"\tUPDATE {schemaName}.{info.SqlClassName}{_nl}");
            updateBuilder.Append($"\tSET{_nl}");
            updateBuilder.AppendJoin(
                $",{_nl}",
                nonIdProperties.Select(x => $"\t\t[{x.SqlName}] = @{x.SqlName}")
            );
            updateBuilder.Append(
                $"{_nl}\tWHERE [{idProperty.SqlName}] = @{idProperty.SqlName}{_nl}"
            );
            updateBuilder.Append("END");
            yield return new SqlFile
            {
                Name = $"{updateProcName}.sql",
                Content = updateBuilder.ToString()
            };

            var deleteBuilder = new StringBuilder();
            var deleteProcName = GetProcedureName(
                schemaName,
                info.SqlClassName,
                "delete"
            );
            deleteBuilder.Append(GetProcedureStart(
                deleteProcName,
                new List<PropertyInfo> { idProperty }
            ));
            deleteBuilder.Append($"\tDELETE{_nl}");
            deleteBuilder.Append($"\tFROM {schemaName}.{info.SqlClassName}{_nl}");
            deleteBuilder.Append(
                $"\tWHERE [{idProperty.SqlName}] = @{idProperty.SqlName}{_nl}"
            );
            deleteBuilder.Append("END");
            yield return new SqlFile
            {
                Name = $"{deleteProcName}.sql",
                Content = deleteBuilder.ToString()
            };
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
            builder.AppendJoin($",{_nl}", parameterDeclarations);
            builder.Append($"{_nl}AS{_nl}BEGIN{_nl}");
            return builder.ToString();
        }

        private static string GetProcedureName(
            string schemaName,
            string tableName,
            string procedureNameSuffix
        ) => $"{schemaName}.{tableName}_{procedureNameSuffix}";

        private static string FormatSelect(PropertyInfo info) =>
            $"\t\t[{info.CSharpName}] = [{info.SqlName}]";
    }

    public class SqlFile
    {
        public string Content { get; set; } = "";
        public string Name { get; set; } = "";
    }
}