using SetSharp.Helpers;

namespace SetSharp.Tests.Helpers
{
    public class SetSharpJsonParserTests
    {
        [Fact]
        public void Parse_ValidJsonObject_ReturnsDictionary()
        {
            var json = "{\"Key1\":\"Value1\",\"Key2\":123,\"Key3\":true}";
            var result = SetSharpJsonParser.Parse(json);

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("Value1", result["Key1"]);
            Assert.Equal(123, result["Key2"]);
            Assert.Equal(true, result["Key3"]);
        }

        [Fact]
        public void Parse_NestedJsonObject_ReturnsDictionaryWithNestedDictionary()
        {
            var json = "{\"Key1\":{\"NestedKey\":\"NestedValue\"}}";
            var result = SetSharpJsonParser.Parse(json);

            Assert.NotNull(result);
            Assert.True(result.ContainsKey("Key1"));
            var nestedDict = Assert.IsType<Dictionary<string, object>>(result["Key1"]);
            Assert.Equal("NestedValue", nestedDict["NestedKey"]);
        }

        [Fact]
        public void Parse_JsonArray_ReturnsDictionaryWithList()
        {
            var json = "{\"Key1\":[\"Item1\",\"Item2\"]}";
            var result = SetSharpJsonParser.Parse(json);

            Assert.NotNull(result);
            Assert.True(result.ContainsKey("Key1"));
            var list = Assert.IsType<List<object>>(result["Key1"]);
            Assert.Equal(2, list.Count);
            Assert.Equal("Item1", list[0]);
            Assert.Equal("Item2", list[1]);
        }

        [Fact]
        public void Parse_EmptyJsonObject_ReturnsEmptyDictionary()
        {
            var json = "{}";
            var result = SetSharpJsonParser.Parse(json);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Parse_InvalidJson_ThrowsFormatException()
        {
            var json = "Invalid Json";
            Assert.Throws<FormatException>(() => SetSharpJsonParser.Parse(json));
        }

        [Fact]
        public void Parse_JsonWithNullValue_ReturnsDictionaryWithNull()
        {
            var json = "{\"Key1\":null}";
            var result = SetSharpJsonParser.Parse(json);

            Assert.NotNull(result);
            Assert.True(result.ContainsKey("Key1"));
            Assert.Null(result["Key1"]);
        }

        [Fact]
        public void Parse_JsonWithDoubleValue_ReturnsDictionaryWithDouble()
        {
            var json = "{\"Key1\":123.45}";
            var result = SetSharpJsonParser.Parse(json);

            Assert.NotNull(result);
            Assert.True(result.ContainsKey("Key1"));
            Assert.Equal(123.45, result["Key1"]);
        }

        [Fact]
        public void Parse_JsonWithIntegerValue_ReturnsDictionaryWithInt()
        {
            var json = "{\"Key1\":123}";
            var result = SetSharpJsonParser.Parse(json);

            Assert.NotNull(result);
            Assert.True(result.ContainsKey("Key1"));
            Assert.Equal(123, result["Key1"]);
        }

        [Fact]
        public void Parse_JsonWithMixedTypes_ReturnsCorrectTypes()
        {
            var json = "{\"stringVal\":\"hello\",\"intVal\":10,\"boolVal\":true,\"doubleVal\":1.23,\"nullVal\":null,\"arrayVal\":[1,2],\"objectVal\":{\"nested\":\"value\"}}";
            var result = SetSharpJsonParser.Parse(json);

            Assert.NotNull(result);
            Assert.Equal("hello", result["stringVal"]);
            Assert.Equal(10, result["intVal"]);
            Assert.Equal(true, result["boolVal"]);
            Assert.Equal(1.23, result["doubleVal"]);
            Assert.Null(result["nullVal"]);
            var array = Assert.IsType<List<object>>(result["arrayVal"]);
            Assert.Equal(2, array.Count);
            var nestedObject = Assert.IsType<Dictionary<string, object>>(result["objectVal"]);
            Assert.Equal("value", nestedObject["nested"]);
        }

        [Fact]
        public void ParseArray_EmptyArray_ReturnsEmptyList()
        {
            var jsonArray = "[]";
            var result = SetSharpJsonParser.ParseArray(jsonArray);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ParseArray_ArrayWithSpaces_ReturnsCorrectList()
        {
            var jsonArray = "[ \"item1\" , 123 ]";
            var result = SetSharpJsonParser.ParseArray(jsonArray);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("item1", result[0]);
            Assert.Equal(123, result[1]);
        }

        [Fact]
        public void ParseArray_ArrayOfObjects_ReturnsListOfDictionaries()
        {
            var jsonArray = "[{\"a\":1}, {\"b\":2}]";
            var result = SetSharpJsonParser.ParseArray(jsonArray);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            var obj1 = Assert.IsType<Dictionary<string, object>>(result[0]);
            Assert.Equal(1, obj1["a"]);
            var obj2 = Assert.IsType<Dictionary<string, object>>(result[1]);
            Assert.Equal(2, obj2["b"]);
        }
    }
}