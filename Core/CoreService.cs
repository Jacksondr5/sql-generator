using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Core
{
    public class CoreService
    {
        private readonly IClassObtainer _classObtainer;
        private readonly IFileWriter _fileWriter;
        private readonly IUserInputRepository _userInputRepo;
        public CoreService(
            IClassObtainer classObtainer,
            IFileWriter fileWriter,
            IUserInputRepository userInputRepo
        ) => (_classObtainer, _fileWriter, _userInputRepo) =
            (classObtainer, fileWriter, userInputRepo);

        public async Task GenerateSqlForType(
            string assemblyPath,
            string typeName,
            string outputPath,
            bool includePrivateProperties
        )
        {
            var type = _classObtainer.GetTypeFromAssembly(
                assemblyPath,
                typeName
            );
            if (type == null)
                throw new ArgumentException(ArgumentExceptionTypeNotFound);
            var typeInfo = ClassInspector.GetFieldInfoFromType(
                type,
                includePrivateProperties
            );
            typeInfo = UserInputService.GetUserInfo(typeInfo, _userInputRepo);
            typeInfo.Properties = typeInfo.Properties
                .Select(x => SqlMapper.MapPropertyToSql(x))
                .ToList();
            var schemaName = _userInputRepo.GetUserInput(GetSchemaMessage);
            var sqlFiles =
                SqlGenerator.GetCrudStoredProcedures(typeInfo, schemaName);
            foreach (var sqlFile in sqlFiles)
                await _fileWriter.WriteFile(
                    Path.Join(outputPath, sqlFile.Name),
                    sqlFile.Content
                );
        }

        public const string ArgumentExceptionTypeNotFound =
            "The given type was not found in the given assembly";
        public const string GetSchemaMessage = "What is the schema name?";
    }
}