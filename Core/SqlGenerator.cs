using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class SqlGenerator
    {
        private static readonly string _nl = Environment.NewLine;
        public static CrudStoredProcedures GetCrudStoredProcedures(
            ClassInfo info,
            string schemaName
        )
        {
            var retVal = new CrudStoredProcedures();
            var idProperty = info.Properties.Find(x => x.IsIdProperty);
            if (idProperty == null)
                throw new InvalidOperationException("shouldn't happen");
            var nonIdProperties = info.Properties
                .FindAll(x => !x.IsIdProperty)
                .OrderBy(x => x.CSharpName);

            var getBuilder = new StringBuilder();
            getBuilder.Append(GetProcedureStart(
                schemaName,
                info.SqlClassName,
                "get_by_id",
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
            retVal.GetById = getBuilder.ToString();

            var createBuilder = new StringBuilder();
            createBuilder.Append(GetProcedureStart(
                schemaName,
                info.SqlClassName,
                "create",
                nonIdProperties
            ));
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
            retVal.Create = createBuilder.ToString();

            var updateBuilder = new StringBuilder();
            updateBuilder.Append(GetProcedureStart(
                schemaName,
                info.SqlClassName,
                "update",
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
            retVal.Update = updateBuilder.ToString();

            var deleteBuilder = new StringBuilder();
            deleteBuilder.Append(GetProcedureStart(
                schemaName,
                info.SqlClassName,
                "delete",
                new List<PropertyInfo> { idProperty }
            ));
            deleteBuilder.Append($"\tDELETE{_nl}");
            deleteBuilder.Append($"\tFROM {schemaName}.{info.SqlClassName}{_nl}");
            deleteBuilder.Append(
                $"\tWHERE [{idProperty.SqlName}] = @{idProperty.SqlName}{_nl}"
            );
            deleteBuilder.Append("END");
            retVal.Delete = deleteBuilder.ToString();
            return retVal;
        }

        private static string GetProcedureStart(
            string schemaName,
            string tableName,
            string procedureNameSuffix,
            IEnumerable<PropertyInfo> inputProperties
        )
        {
            var parameterDeclarations = inputProperties.Select(x =>
                $"\t@{x.SqlName} {x.SqlType}"
            );
            var builder = new StringBuilder();
            builder.Append("CREATE OR ALTER PROCEDURE ");
            builder.Append(
                $"{schemaName}.{tableName}_{procedureNameSuffix}{_nl}"
            );
            builder.AppendJoin($",{_nl}", parameterDeclarations);
            builder.Append($"{_nl}AS{_nl}BEGIN{_nl}");
            return builder.ToString();
        }

        private static string FormatSelect(PropertyInfo info) =>
            $"\t\t[{info.CSharpName}] = [{info.SqlName}]";

        // private static string FormatInsert()
    }

    public class CrudStoredProcedures
    {
        public string Create { get; set; } = "";
        public string Delete { get; set; } = "";
        public string GetById { get; set; } = "";
        public string Update { get; set; } = "";
    }
}