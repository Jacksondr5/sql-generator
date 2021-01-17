using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CaseExtensions;

namespace Core
{
    public class ClassInspector
    {
        public static ClassInfo GetFieldInfoFromType(
            Type type,
            bool includeInternalProperties = false,
            bool includePrivateProperties = false
        )
        {
            return new ClassInfo
            {
                ClassName = type.Name,
                Properties = type
                    .GetProperties(
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance
                    )
                    .Where(x => FilterProperty(
                        x,
                        includeInternalProperties,
                        includePrivateProperties
                    ))
                    .Select(x => new PropertyInfo
                    {
                        CSharpName = x.Name,
                        CSharpType = x.PropertyType.Name switch
                        {
                            "Int32" => ValidType.Int,
                            "Boolean" => ValidType.Bool,
                            "Decimal" => ValidType.Decimal,
                            "Double" => ValidType.Double,
                            "String" => ValidType.String,
                            _ => throw new InvalidOperationException(
                                "unknown C# type"
                            )
                        },
                    })
                    .ToList()
            };
        }

        private static bool FilterProperty(
            System.Reflection.PropertyInfo property,
            bool includeInternalProperties,
            bool includePrivateProperties
        )
        {
            var getMethod = property.GetGetMethod(true);
            if (getMethod == null)
                throw new InvalidOperationException("this shouldnt happen");
            return getMethod.IsPublic ||
                (includePrivateProperties && getMethod.IsPrivate);
        }
    }

    public class ClassInfo
    {
        public string ClassName { get; set; } = "";
        public List<PropertyInfo> Properties { get; set; } =
            new List<PropertyInfo>();
    }

    public class PropertyInfo
    {
        public string CSharpName { get; set; } = "";
        public ValidType CSharpType { get; set; }
        public int Length { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public string SqlName { get; set; } = "";
        public string SqlType { get; set; } = "";
    }

    internal class BadType { }
}
