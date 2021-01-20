using Core;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static Task Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Option<FileInfo>(
                    new string[] { "--assembly-path", "-a" },
                    "The path to the assembly that contains the type to generate SQL for"
                ),
                new Option<string>(
                    new string[] { "--type-name", "-t" },
                    "The fully qualified name of the type to generate SQL for"
                ),
                new Option<FileInfo>(
                    new string[] { "--output-folder", "-o" },
                    "The output folder"
                ),
                new Option<bool>(
                    new string[] { "--include-private-properties", "--private"},
                    "A flag that when true will include non-public properties in the generated SQL"
                )
            };
            rootCommand.Description = "SQL Generator";
            rootCommand.Handler = CommandHandler.Create<FileInfo, string, FileInfo, bool>(
                async (assemblyPath, typeName, outputFolder, includePrivateProperties) =>
                {
                    if (!File.Exists(assemblyPath.FullName))
                        throw new ArgumentException(
                            "The given assembly does not exist",
                            nameof(assemblyPath)
                        );
                    var userInputRepo = new UserInputRepository();
                    var fileWriter = new FileWriter();
                    var classObtainer = new ClassObtainer();
                    var service = new CoreService
                    (
                        classObtainer,
                        fileWriter,
                        userInputRepo
                    );
                    await service.GenerateSqlForType(
                        assemblyPath.FullName,
                        typeName,
                        outputFolder.FullName,
                        includePrivateProperties
                    );
                }
            );
            return rootCommand.InvokeAsync(args);
        }
    }
}
