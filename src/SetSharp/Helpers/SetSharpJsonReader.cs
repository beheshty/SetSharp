

namespace SetSharp.Helpers
{
    internal static class SetSharpJsonReader
    {
        /// <summary>
        /// Reads a value from a nested dictionary using a colon-delimited key path.
        /// </summary>
        /// <param name="json">The top-level dictionary representing the parsed JSON.</param>
        /// <param name="keyPath">The path to the desired value (e.g., "SetSharp:OptionPatternGenerationEnabled").</param>
        /// <returns>The found value as an object, or null if the path is invalid or the key is not found.</returns>
        internal static object? Read(Dictionary<string, object> json, string keyPath)
        {
            if (json == null || string.IsNullOrWhiteSpace(keyPath))
            {
                return null;
            }

            string[] keys = keyPath.Split(':');
            object currentNode = json;

            foreach (var key in keys)
            {
                if (currentNode is not Dictionary<string, object> currentDict)
                {
                    return null;
                }

                if (!currentDict.TryGetValue(key, out currentNode))
                {
                    return null;
                }
            }

            return currentNode;
        }
    }
}
