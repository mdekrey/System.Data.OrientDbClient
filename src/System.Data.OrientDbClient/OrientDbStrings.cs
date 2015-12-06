using Newtonsoft.Json.Linq;

namespace System.Data.OrientDbClient
{
    internal class OrientDbStrings
    {
        public static string GetBytesNotSupported => "GetBytes is not supported";
        public static string GetCharsNotSupported => "GetChars is not supported";

        public static string OnlyInputParametersSupported => "Only input parameters are supported.";

        public static string ParameterNotFoundByName => "Parameter of given name does not exist in collection";

        public static string ParametersMustBeOrientDbParameter => "Parameters must be of type OrientDbParameter";

        internal static string ErrorFromOrientDb(JToken jToken) => "Error from OrientDb: {jToken}";

        internal static string InvalidParameterName(string value) => $"Parameter names must begin with '$'; '{value}' does not.";
    }
}