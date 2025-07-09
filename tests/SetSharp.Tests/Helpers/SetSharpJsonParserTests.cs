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
        public void Parse_JsonWithStringValueWithEscapedChar_ReturnsDictionaryWithString()
        {
            var json = "{\"Key1\":\"\\\"\"}";
            var result = SetSharpJsonParser.Parse(json);

            Assert.NotNull(result);
            Assert.True(result.ContainsKey("Key1"));
            Assert.Equal("\"", result["Key1"]);
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

        [Fact]
        public void Parse_JsonObjectWithLeadingAndTrailingWhitespace_ReturnsDictionary()
        {
            var json = "  \n\t {\"key\":\"value\"} \r\n";
            var result = SetSharpJsonParser.Parse(json);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("value", result["key"]);
        }

        [Fact]
        public void Parse_JsonObjectWithInternalWhitespace_ReturnsDictionary()
        {
            var json = "{\n  \"key1\" \t : \r\n \"value1\",\n\"key2\"\n:\ntrue\n}";
            var result = SetSharpJsonParser.Parse(json);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("value1", result["key1"]);
            Assert.Equal(true, result["key2"]);
        }

        [Fact]
        public void Parse_JsonObjectWithEmptyKeyAndValue_ReturnsDictionary()
        {
            var json = "{\"key\":\"\", \"\":\"value\"}";
            var result = SetSharpJsonParser.Parse(json);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("", result["key"]);
            Assert.Equal("value", result[""]);
        }

        [Fact]
        public void Parse_JsonWithScientificNotation_ReturnsDictionaryWithDouble()
        {
            var json = "{\"key_pos\": 1.23e+5, \"key_neg\": 4.56E-2}";
            var result = SetSharpJsonParser.Parse(json);

            Assert.NotNull(result);
            Assert.Equal(123000d, result["key_pos"]);
            Assert.Equal(0.0456d, result["key_neg"]);
        }

        [Fact]
        public void Parse_DeeplyNestedJsonObject_ReturnsCorrectlyNestedDictionary()
        {
            var json = "{\"a\":{\"b\":{\"c\":{\"d\":[1, {\"e\":\"f\"}]}}}}";
            var result = SetSharpJsonParser.Parse(json);

            Assert.NotNull(result);
            var val_a = Assert.IsType<Dictionary<string, object>>(result["a"]);
            var val_b = Assert.IsType<Dictionary<string, object>>(val_a["b"]);
            var val_c = Assert.IsType<Dictionary<string, object>>(val_b["c"]);
            var val_d = Assert.IsType<List<object>>(val_c["d"]);
            Assert.Equal(1, val_d[0]);
            var val_d1 = Assert.IsType<Dictionary<string, object>>(val_d[1]);
            Assert.Equal("f", val_d1["e"]);
        }

        [Fact]
        public void ParseArray_ArrayWithAllDataTypes_ReturnsListOfCorrectTypes()
        {
            var json = "[\"string\", 123, 45.67, true, false, null, {\"k\":\"v\"}, [1, 2]]";
            var result = SetSharpJsonParser.ParseArray(json);

            Assert.NotNull(result);
            Assert.Equal(8, result.Count);
            Assert.Equal("string", result[0]);
            Assert.Equal(123, result[1]);
            Assert.Equal(45.67, result[2]);
            Assert.Equal(true, result[3]);
            Assert.Equal(false, result[4]);
            Assert.Null(result[5]);
            Assert.IsType<Dictionary<string, object>>(result[6]);
            Assert.IsType<List<object>>(result[7]);
        }

        [Fact]
        public void Parse_JsonStringWithUnicodeEscape_ReturnsUnescapedString()
        {
            // \u00E9 is the character 'é'
            var json = "{\"key\":\"caf\\u00E9\"}";
            var result = SetSharpJsonParser.Parse(json);

            Assert.Equal("café", result["key"]);
        }

        [Theory]
        [InlineData("{\"key\": [1, 2}}")]         // Mismatched Braces
        [InlineData("{\"key1\": \"v1\" \"key2\": \"v2\"}")] // Missing Comma
        [InlineData("{\"key\": \"v1\",}")]        // Trailing Comma in Object
        [InlineData("[1, 2,]")]                  // Trailing Comma in Array
        [InlineData("{\"key\": \"value")]         // Unterminated String
        [InlineData("{\"key\": tru}")]            // Invalid Literal
        [InlineData("{123: \"value\"}")]         // Key is not a string
        [InlineData("{\"key\"}")]                 // Missing Colon
        public void Parse_MalformedJson_ThrowsFormatException(string malformedJson)
        {
            Assert.Throws<FormatException>(() =>
            {

                // The entry point could be Parse or ParseArray
                if (malformedJson.Trim().StartsWith("["))
                {
                    SetSharpJsonParser.ParseArray(malformedJson);
                }
                else
                {
                    SetSharpJsonParser.Parse(malformedJson);
                }

            });
        }
    }
}