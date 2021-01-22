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
        private readonly IUserInputRepository _userInputRepo;
        public ClassInspector(IUserInputRepository userInputRepo) =>
            (_userInputRepo) = (userInputRepo);
        private const string _idPropertyName = "Id";
        public ClassInfo GetFieldInfoFromType(
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
                        CSharpType = x.PropertyType,
                        ValidType = GetValidType(x.PropertyType),
                        IsIdProperty = x.Name.Equals(_idPropertyName)
                    })
                    .Where(x => x.ValidType != ValidType.InvalidType)
                    .ToList(),
                SqlClassName = type.Name.ToSnakeCase()
            };
            if (!info.Properties.Any(x => x.IsIdProperty))
            {
                var idPropertyNames = _userInputRepo
                    .GetUserInput(NoIdPropertyMessage)
                    .Split(',');
                foreach (var idPropertyName in idPropertyNames)
                {
                    if (!info.Properties.Any(x =>
                        x.CSharpName.Equals(idPropertyName)
                    ))
                        throw new InvalidInputException(
                            InvalidInputExceptionIdProperty(idPropertyName)
                        );
                    info.Properties
                        .First(x => x.CSharpName.Equals(idPropertyName))
                        .IsIdProperty = true;
                }
            }
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

        private static ValidType GetValidType(Type type)
        {
            if (type.IsEnum)
                return ValidType.Enum;
            else if (
                type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(List<>)
            )
                return ValidType.List;
            return type.Name switch
            {
                "Int32" => ValidType.Int,
                "Boolean" => ValidType.Bool,
                "DateTime" => ValidType.DateTime,
                "Decimal" => ValidType.Decimal,
                "Double" => ValidType.Double,
                "String" => ValidType.String,
                _ => ValidType.InvalidType
            };
        }

        public static string InvalidInputExceptionIdProperty(string property) =>
            $"The given ID property {property} does not exist";
        public const string NoIdPropertyMessage =
            "What property/properties should be considered the ID property/properties?  Enter a comma deliniated list of ID properties";
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
        public Type CSharpType { get; set; } = typeof(BadType);
        public bool IsIdProperty { get; set; }
        public string Length { get; set; } = "";
        public int Precision { get; set; }
        public int Scale { get; set; }
        public string SqlName { get; set; } = "";
        public string SqlType { get; set; } = "";
        public ValidType ValidType { get; set; }
    }

    internal class BadType { }
}
