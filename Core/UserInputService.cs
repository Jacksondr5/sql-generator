namespace Core
{
    public static class UserInputService
    {
        public static ClassInfo GetUserInfo(
            ClassInfo info,
            IUserInputRepository repo
        )
        {
            foreach (var property in info.Properties)
            {
                var type = property.CSharpType;
                if (type == ValidType.String)
                {
                    property.Length = repo.GetUserInput(
                        GetLengthMessage(property.CSharpName)
                    );
                }
                else if (type == ValidType.Decimal || type == ValidType.Double)
                {
                    property.Precision = ParseUserInput(repo.GetUserInput(
                        GetPrecisionMessage(property.CSharpName)
                    ));
                    property.Scale = ParseUserInput(repo.GetUserInput(
                        GetScaleMessage(property.CSharpName)
                    ));
                }
            }
            return info;
        }

        private static int ParseUserInput(string userInput)
        {
            var success = int.TryParse(userInput, out int retVal);
            if (success)
            {
                return retVal;
            }
            else
            {
                throw new InvalidInputException(
                    InvalidInputExceptionBadInput(userInput)
                );
            }
        }

        public static string GetLengthMessage(string propName) =>
            $"Please enter the length for {propName}";
        public static string GetPrecisionMessage(string propName) =>
            $"Please enter the precision for {propName}";
        public static string GetScaleMessage(string propName) =>
            $"Please enter the scale for {propName}";
        public static string InvalidInputExceptionBadInput(string input) =>
            $"The value \"{input}\" cannot be parsed into an integer";
    }
}