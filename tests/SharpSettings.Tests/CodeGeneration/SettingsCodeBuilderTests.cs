using SharpSettings.CodeGeneration;

namespace SharpSettings.Tests.CodeGeneration
{
    public class SettingsCodeBuilderTests
    {
        [Fact]
        public void GenerateClasses_SimpleObject_GeneratesCorrectClass()
        {
            var json = "{\"Key1\":\"Value1\",\"Key2\":123}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("using System.Collections.Generic;", generatedCode);
            Assert.Contains("namespace SharpSettings.Configuration", generatedCode);
            Assert.Contains("public class RootOptions", generatedCode);
            Assert.Contains("public string Key1 { get; set; }", generatedCode);
            Assert.Contains("public int Key2 { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_NestedObject_GeneratesCorrectClasses()
        {
            var json = "{\"Key1\":{\"NestedKey\":\"NestedValue\"}}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public class RootOptions", generatedCode);
            Assert.Contains("public Key1Options Key1 { get; set; }", generatedCode);
            Assert.Contains("public class Key1Options", generatedCode);
            Assert.Contains("public string NestedKey { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_ArrayOfStrings_GeneratesCorrectClass()
        {
            var json = "{\"Key1\":[\"Item1\",\"Item2\"]}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public class RootOptions", generatedCode);
            Assert.Contains("public List<string> Key1 { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_ArrayOfObjects_GeneratesCorrectClasses()
        {
            var json = "{\"Key1\":[{\"PropA\":1},{\"PropA\":2}]}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public class RootOptions", generatedCode);
            Assert.Contains("public List<Key1ItemOptions> Key1 { get; set; }", generatedCode);
            Assert.Contains("public class Key1ItemOptions", generatedCode);
            Assert.Contains("public int PropA { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_EmptyObject_GeneratesEmptyClass()
        {
            var json = "{}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public class RootOptions", generatedCode);
            Assert.Contains(@"    public class RootOptions
    {
    }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_JsonWithNullValue_GeneratesObjectProperty()
        {
            var json = "{\"Key1\":null}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public class RootOptions", generatedCode);
            Assert.Contains("public object Key1 { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_JsonWithReservedKeyword_NormalizesName()
        {
            var json = "{\"class\":\"Value\"}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public class RootOptions", generatedCode);
            Assert.Contains("public string ClassProperty { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_JsonWithDotInKey_RemovesDot()
        {
            var json = "{\"my.key\":\"Value\"}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public class RootOptions", generatedCode);
            Assert.Contains("public string Mykey { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_CustomNamespace_GeneratesCorrectNamespace()
        {
            var json = "{\"Key1\":\"Value1\"}";
            var customNamespace = "MyCustomNamespace";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json, customNamespace);

            Assert.Contains("namespace MyCustomNamespace", generatedCode);
            Assert.Contains("public class RootOptions", generatedCode);
            Assert.Contains("public string Key1 { get; set; }", generatedCode);
        }
    }
}