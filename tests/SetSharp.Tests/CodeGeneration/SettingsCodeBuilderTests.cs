using SetSharp.CodeGeneration;

namespace SetSharp.Tests.CodeGeneration
{
    public class SettingsCodeBuilderTests
    {
        [Fact]
        public void GenerateClasses_SimpleObject_GeneratesCorrectClass()
        {
            var json = "{\"Key1\":\"Value1\",\"Key2\":123}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("using System.Collections.Generic;", generatedCode);
            Assert.Contains("namespace SetSharp.Configuration", generatedCode);
            Assert.Contains("public partial class RootOptions", generatedCode);
            Assert.Contains("public string Key1 { get; set; }", generatedCode);
            Assert.Contains("public int Key2 { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_SimpleObject_DoesNotGenerateSectionName()
        {
            var json = "{\"Key1\":\"Value1\",\"Key2\":123}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.DoesNotContain("public const string SectionName =", generatedCode);
        }

        [Fact]
        public void GenerateClasses_NestedObject_GeneratesCorrectClasses()
        {
            var json = "{\"Key1\":{\"NestedKey\":\"NestedValue\"}}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public partial class RootOptions", generatedCode);
            Assert.Contains("public Key1Options Key1 { get; set; }", generatedCode);
            Assert.Contains("public partial class Key1Options", generatedCode);
            Assert.Contains("public string NestedKey { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_NestedObject_GeneratesSectionName()
        {
            var json = "{\"Key1\":{\"NestedKey\":\"NestedValue\"}}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public const string SectionName = \"Key1\"", generatedCode);

        }

        [Fact]
        public void GenerateClasses_ArrayOfStrings_GeneratesCorrectClass()
        {
            var json = "{\"Key1\":[\"Item1\",\"Item2\"]}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public partial class RootOptions", generatedCode);
            Assert.Contains("public List<string> Key1 { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_ArrayOfObjects_GeneratesCorrectClasses()
        {
            var json = "{\"Key1\":[{\"PropA\":1},{\"PropA\":2}]}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public partial class RootOptions", generatedCode);
            Assert.Contains("public List<Key1ItemOptions> Key1 { get; set; }", generatedCode);
            Assert.Contains("public partial class Key1ItemOptions", generatedCode);
            Assert.Contains("public int PropA { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_EmptyObject_GeneratesEmptyClass()
        {
            var json = "{}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public partial class RootOptions", generatedCode);
            Assert.Contains(@"    public partial class RootOptions
    {
    }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_JsonWithNullValue_GeneratesObjectProperty()
        {
            var json = "{\"Key1\":null}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public partial class RootOptions", generatedCode);
            Assert.Contains("public object Key1 { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_JsonWithReservedKeyword_NormalizesName()
        {
            var json = "{\"class\":\"Value\"}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public partial class RootOptions", generatedCode);
            Assert.Contains("public string Class { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_JsonWithDotInKey_RemovesDot()
        {
            var json = "{\"my.key\":\"Value\"}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public partial class RootOptions", generatedCode);
            Assert.Contains("public string Mykey { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_CustomNamespace_GeneratesCorrectNamespace()
        {
            var json = "{\"Key1\":\"Value1\"}";
            var customNamespace = "MyCustomNamespace";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json, customNamespace);

            Assert.Contains("namespace MyCustomNamespace", generatedCode);
            Assert.Contains("public partial class RootOptions", generatedCode);
            Assert.Contains("public string Key1 { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_HandlesBooleanAndDoubleTypes()
        {
            var json = @"{""IsEnabled"": true, ""Value"": 99.9, ""Factors"": [1.1, 2.2], ""States"": [false, true]}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public bool IsEnabled { get; set; }", generatedCode);
            Assert.Contains("public double Value { get; set; }", generatedCode);
            Assert.Contains("public List<double> Factors { get; set; }", generatedCode);
            Assert.Contains("public List<bool> States { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_KeyStartsWithNumber_NormalizesAndAddsAttribute()
        {
            var json = @"{""1stKey"": ""value""}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            // Asserts that the original key name is preserved for configuration binding.
            Assert.Contains("[Microsoft.Extensions.Configuration.ConfigurationKeyName(\"1stKey\")]", generatedCode);
            // Asserts that the property name is a valid C# identifier.
            Assert.Contains("public string _1stKey { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_KeyWithHyphen_NormalizesAndAddsAttribute()
        {
            var json = @"{""my-key"": ""value""}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("[Microsoft.Extensions.Configuration.ConfigurationKeyName(\"my-key\")]", generatedCode);
            Assert.Contains("public string Mykey { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_KeyWithOnlyInvalidChars_NormalizesAndAddsAttribute()
        {
            var json = @"{""@#$!"": ""value""}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("[Microsoft.Extensions.Configuration.ConfigurationKeyName(\"@#$!\")]", generatedCode);
            Assert.Contains("public string InvalidNameProperty { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_EmptyArray_GeneratesListOfObject()
        {
            var json = @"{""EmptyList"": []}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public List<object> EmptyList { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_ArrayWithNullFirst_GeneratesListOfObject()
        {
            var json = @"{""NullableList"": [null, ""item2""]}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public List<object> NullableList { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_DeeplyNestedObject_GeneratesAllClasses()
        {
            var json = @"{""Level1"":{""Level2"":{""Name"":""DeepValue""}}}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            // Check level 1 class and property
            Assert.Contains("public partial class RootOptions", generatedCode);
            Assert.Contains("public Level1Options Level1 { get; set; }", generatedCode);

            // Check level 2 class and property
            Assert.Contains("public partial class Level1Options", generatedCode);
            Assert.Contains("public Level2Options Level2 { get; set; }", generatedCode);

            // Check level 3 property
            Assert.Contains("public partial class Level2Options", generatedCode);
            Assert.Contains("public string Name { get; set; }", generatedCode);
        }

        [Fact]
        public void GenerateClasses_GeneratedCodeAttribute_IsPresent()
        {
            var json = "{}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);
            var assemblyVersion = typeof(SettingsCodeBuilder).Assembly.GetName().Version.ToString();

            var expectedAttribute = $"[System.CodeDom.Compiler.GeneratedCode(\"SetSharp\", \"{assemblyVersion}\")]";
            Assert.Contains(expectedAttribute, generatedCode);
        }

        [Fact]
        public void GenerateClasses_ArrayWithMixedTypes_InfersFromFirstElement()
        {
            // This test documents the current behavior where the type of a list
            // is inferred solely from its first element.
            var json = @"{""MixedList"": [{""Name"": ""Obj1""}, ""text"", 123]}";
            var generatedCode = SettingsCodeBuilder.GenerateClasses(json);

            Assert.Contains("public List<MixedListItemOptions> MixedList { get; set; }", generatedCode);
            Assert.Contains("public partial class MixedListItemOptions", generatedCode);
            Assert.Contains("public string Name { get; set; }", generatedCode);
        }
    }
}