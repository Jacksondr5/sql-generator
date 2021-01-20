using CaseExtensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Core
{
    public class ClassInspector
    {
        private const string _idPropertyName = "Id";
        public static ClassInfo GetFieldInfoFromType(
            Type type,
            bool includePrivateProperties = false
        )
        {
            var info = new ClassInfo
            {
                CSharpClassName = type.Name,
                Properties = type
                    .GetProperties(
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance
                    )
                    .Where(x => FilterProperty(
                        x,
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
                            _ => ValidType.InvalidType
                        },
                        IsIdProperty =
                            x.Name.Equals(_idPropertyName)
                    })
                    .Where(x => x.CSharpType != ValidType.InvalidType)
                    .ToList(),
                SqlClassName = type.Name.ToSnakeCase()
            };
            if (!info.Properties.Any(x => x.IsIdProperty))
                throw new InvalidInputException(
                    InvalidInputExceptionNoIdProperty(info.CSharpClassName)
                );
            //Result of Find is not null due to the Any check above
            if (info.Properties.Find(x => x.IsIdProperty)!.CSharpType != ValidType.Int)
                throw new InvalidInputException(
                    InvalidInputExceptionIdPropertyNotInt(info.CSharpClassName)
                );
            return info;
        }

        private static bool FilterProperty(
            System.Reflection.PropertyInfo property,
            bool includePrivateProperties
        )
        {
            var getMethod = property.GetGetMethod(true);
            if (getMethod == null)
                throw new InvalidOperationException("this shouldnt happen");
            return getMethod.IsPublic ||
                (includePrivateProperties && getMethod.IsPrivate);
        }

        public static string InvalidInputExceptionIdPropertyNotInt(
            string className
        ) => $"The class {className} has an ID property that is not an int";
        public static string InvalidInputExceptionNoIdProperty(
            string className
        ) => $"The class {className} has no Id property";
    }

    public class ClassInfo
    {
        public string CSharpClassName { get; set; } = "";
        public List<PropertyInfo> Properties { get; set; } =
            new List<PropertyInfo>();
        public string SqlClassName { get; set; } = "";
    }

    public class PropertyInfo
    {
        public string CSharpName { get; set; } = "";
        public ValidType CSharpType { get; set; }
        public bool IsIdProperty { get; set; }
        public int Length { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public string SqlName { get; set; } = "";
        public string SqlType { get; set; } = "";
    }

    internal class BadType { }
}
