using Newtonsoft.Json.Linq;

namespace System.Data.OrientDbClient
{
    internal class OrientDbStrings
    {
        public static string BinaryLiteralsNotSupported => "Binary literals are not supported at this time";

        public static string DbTypeLiteralsNotSupported => "DbType literals are not supported at this time";

        public static string GetBytesNotSupported => "GetBytes is not supported";
        public static string GetCharsNotSupported => "GetChars is not supported";

        public static string OnlyInputParametersSupported => "Only input parameters are supported.";

        public static string ParameterNotFoundByName => "Parameter of given name does not exist in collection";

        public static string ParametersMustBeOrientDbParameter => "Parameters must be of type OrientDbParameter";

        internal static string ErrorFromOrientDb(JToken jToken) => $"Error from OrientDb: {jToken}";
        internal static string NoContentFromOrientDb => $"No content returned from OrientDb";

        public static string TransactionsNotSupported => "Transactions across requests are not supported";

        public static string BatchCommandUsesCommandsText => $"{nameof(OrientDbBatchCommand)} uses property {nameof(OrientDbBatchCommand.CommandsText)}; it does not support a single command.";

        public static string BatchCommandUsesIsTransaction => $"{nameof(OrientDbBatchCommand)} uses property {nameof(OrientDbBatchCommand.IsTransaction)}; it does not support a transaction object.";

        public static string DatabaseCouldNotBeCreated => "Database could not be created";

        internal static string InvalidParameterName(string value) => $"Parameter names must begin with '$'; '{value}' does not.";
    }
}