using CaseExtensions;
using System;

namespace Core
{
    public static class SqlMapper
    {
        public static PropertyInfo MapPropertyToSql(
            PropertyInfo info,
            string tableName
        )
        {
            info.SqlName = info.CSharpName.ToSnakeCase();
            if (
                info.IsIdProperty &&
                info.CSharpName.Equals(ClassInspector.IdPropertyName)
            )
                info.SqlName = info.SqlName.Insert(0, $"{tableName}_");
            info.SqlType = info.ValidType switch
            {
                ValidType.Int => "INT",
                ValidType.Bool => "BIT",
                ValidType.DateTime => "DATETIME",
                ValidType.Decimal => $"DECIMAL({info.Precision}, {info.Scale})",
                ValidType.Double => $"DECIMAL({info.Precision}, {info.Scale})",
                ValidType.Enum => "INT",
                ValidType.List => $"VARCHAR({info.Length})",
                ValidType.String => $"VARCHAR({info.Length})",
                _ => throw new InvalidOperationException("unknown C# type")
            };
            if (info.IsNullable)
                info.SqlType += " NULL";
            else
                info.SqlType += " NOT NULL";
            return info;
        }
    }
}