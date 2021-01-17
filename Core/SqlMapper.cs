using System;
using CaseExtensions;

namespace Core
{
    public class SqlMapper
    {
        public static PropertyInfo MapPropertyToSql(PropertyInfo info)
        {
            info.SqlName = info.CSharpName.ToSnakeCase();
            info.SqlType = info.CSharpType switch
            {
                ValidType.Int => "INT",
                ValidType.Bool => "BIT",
                ValidType.Decimal => $"DECIMAL({info.Precision}, {info.Scale})",
                ValidType.Double => $"DECIMAL({info.Precision}, {info.Scale})",
                ValidType.String => $"VARCHAR({info.Length})",
                _ => throw new InvalidOperationException("unknown C# type")
            };
            return info;
        }
    }
}