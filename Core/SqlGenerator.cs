using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class SqlGenerator
    {
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
            getBuilder.Append("\tSELECT\n");
            getBuilder.Append($"{FormatSelect(idProperty)},\n");
            getBuilder.AppendJoin(
                ",\n",
                nonIdProperties.Select(x => FormatSelect(x))
            );
            getBuilder.Append($"\n\tFROM {schemaName}.{info.SqlClassName}\n");
            getBuilder.Append(
                $"\tWHERE [{idProperty.SqlName}] = @{idProperty.SqlName}\n"
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
                $"\tINSERT {schemaName}.{info.SqlClassName} (\n"
            );
            createBuilder.AppendJoin(
                ",\n",
                nonIdProperties.Select(x => $"\t\t{x.SqlName}")
            );
            createBuilder.Append("\n\t)\n");
            createBuilder.Append("\tVALUES (\n");
            createBuilder.AppendJoin(
                ",\n",
                nonIdProperties.Select(x => $"\t\t@{x.SqlName}")
            );
            createBuilder.Append("\n\t);\n\n");
            createBuilder.Append("\tSELECT SCOPE_IDENTITY()\n");
            createBuilder.Append("END");
            retVal.Create = createBuilder.ToString();

            var updateBuilder = new StringBuilder();
            updateBuilder.Append(GetProcedureStart(
                schemaName,
                info.SqlClassName,
                "update",
                nonIdProperties.Prepend(idProperty)
            ));
            updateBuilder.Append($"\tUPDATE {schemaName}.{info.SqlClassName}\n");
            updateBuilder.Append("\tSET\n");
            updateBuilder.AppendJoin(
                ",\n",
                nonIdProperties.Select(x => $"\t\t[{x.SqlName}] = @{x.SqlName}")
            );
            updateBuilder.Append(
                $"\n\tWHERE [{idProperty.SqlName}] = @{idProperty.SqlName}\n"
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
            deleteBuilder.Append("\tDELETE\n");
            deleteBuilder.Append($"\tFROM {schemaName}.{info.SqlClassName}\n");
            deleteBuilder.Append(
                $"\tWHERE [{idProperty.SqlName}] = @{idProperty.SqlName}\n"
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
                $"{schemaName}.{tableName}_{procedureNameSuffix}\n"
            );
            builder.AppendJoin(",\n", parameterDeclarations);
            builder.Append("\nAS\nBEGIN\n");
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