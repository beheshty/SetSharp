using System;
using System.Collections.Generic;
using System.Text;

namespace SetSharp.Definitions
{
    internal static class MSBuildPropertyKeys
    {
        private const string _prefix = "build_property.";

        public const string SetSharpPrefix = "SetSharp_";

        public const string OptionPatternGenerationEnabled = _prefix + SetSharpPrefix + "OptionPatternGenerationEnabled";

        public const string SourceFile = _prefix + SetSharpPrefix + "SourceFile";
    }
}
