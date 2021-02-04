using System;
using System.Collections.Generic;
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

        public Task GenerateSqlForType(
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
            var inspector = new ClassInspector(_userInputRepo);
            var typeInfo = inspector.GetFieldInfoFromType(
                type,
                includePrivateProperties
            );
            typeInfo = UserInputService.GetUserInfo(typeInfo, _userInputRepo);
            typeInfo.Properties = typeInfo.Properties
                .Select(x =>
                    SqlMapper.MapPropertyToSql(x, typeInfo.SqlTableName)
                )
                .ToList();
            var schemaName = _userInputRepo.GetUserInput(GetSchemaMessage);
            var sqlGenerator = new SqlGenerator(typeInfo, schemaName);
            var sqlFiles = sqlGenerator.GetSql();
            return GenerateSqlForTypeInternal(outputPath, sqlFiles);
        }

        private async Task GenerateSqlForTypeInternal(
            string outputPath,
            IEnumerable<SqlFile> sqlFiles
        )
        {
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