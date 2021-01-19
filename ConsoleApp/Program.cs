using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Core;

namespace ConsoleApp
{
    class Program
    {
        static void Main(
            string assemblyPath,
            string className,
            string outputPath,
            bool includePrivateProperties = false
        )
        {
            if (!File.Exists(assemblyPath))
                throw new ArgumentException(
                    "The given assembly does not exist",
                    nameof(assemblyPath)
                );
            Console.WriteLine(className);
            var assembly = Assembly.LoadFrom(assemblyPath);
            var targetClass = assembly.GetType(className);
            var repo = new UserInputRepository();
            var service = new UserInputService(repo);
            var classInfo = service.GetUserInfo(
                ClassInspector.GetFieldInfoFromType(targetClass)
            );
            classInfo.Properties = classInfo.Properties
                .Select(x => SqlMapper.MapPropertyToSql(x))
                .ToList();
            var schemaName = repo.GetUserInput("What is the schame name?");
            var sqlFiles =
                SqlGenerator.GetCrudStoredProcedures(classInfo, schemaName);
            File.WriteAllText($"{outputPath}/Create.sql", sqlFiles.Create);
            File.WriteAllText($"{outputPath}/Delete.sql", sqlFiles.Delete);
            File.WriteAllText($"{outputPath}/GetById.sql", sqlFiles.GetById);
            File.WriteAllText($"{outputPath}/Update.sql", sqlFiles.Update);
        }
    }
}
