using SetSharp.Helpers;

namespace SetSharp.Tests.Helpers
{
    public class SetSharpJsonReaderTests
    {
        private readonly Dictionary<string, object> _testJson = new()
        {
            { "TopLevelString", "Hello World" },
            { "TopLevelInt", 123 },
            { "SetSharp", new Dictionary<string, object>
                {
                    { "Enabled", true },
                    { "Generation", new Dictionary<string, object>
                        {
                            { "Poco", true },
                            { "OptionsPattern", false }
                        }
                    }
                }
            }
        };

        [Fact]
        public void Read_WithValidTopLevelPath_ReturnsCorrectValue()
        {
            // Arrange
            var keyPath = "TopLevelString";

            // Act
            var result = SetSharpJsonReader.Read(_testJson, keyPath);

            // Assert
            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void Read_WithValidNestedPath_ReturnsCorrectValue()
        {
            // Arrange
            var keyPath = "SetSharp:Enabled";

            // Act
            var result = SetSharpJsonReader.Read(_testJson, keyPath);

            // Assert
            Assert.IsType<bool>(result);
            Assert.Equal(true, result);
        }

        [Fact]
        public void Read_WithDeeplyNestedPath_ReturnsCorrectValue()
        {
            // Arrange
            var keyPath = "SetSharp:Generation:Poco";

            // Act
            var result = SetSharpJsonReader.Read(_testJson, keyPath);

            // Assert
            Assert.IsType<bool>(result);
            Assert.Equal(true, result);
        }

        [Fact]
        public void Read_PathToADictionary_ReturnsDictionaryObject()
        {
            // Arrange
            var keyPath = "SetSharp:Generation";

            // Act
            var result = SetSharpJsonReader.Read(_testJson, keyPath);

            // Assert
            var dictResult = Assert.IsType<Dictionary<string, object>>(result);
            Assert.True((bool)dictResult["Poco"]);
            Assert.False((bool)dictResult["OptionsPattern"]);
        }

        [Fact]
        public void Read_WithNonExistentTopLevelKey_ReturnsNull()
        {
            // Arrange
            var keyPath = "NonExistent";

            // Act
            var result = SetSharpJsonReader.Read(_testJson, keyPath);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Read_WithNonExistentNestedKey_ReturnsNull()
        {
            // Arrange
            var keyPath = "SetSharp:Generation:NonExistent";

            // Act
            var result = SetSharpJsonReader.Read(_testJson, keyPath);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Read_PathGoesThroughLeafValue_ReturnsNull()
        {
            // Arrange
            // Trying to navigate deeper into "TopLevelString", which is not a dictionary
            var keyPath = "TopLevelString:Deeper";

            // Act
            var result = SetSharpJsonReader.Read(_testJson, keyPath);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Read_WithNullJson_ReturnsNull()
        {
            // Arrange
            Dictionary<string, object> nullJson = null;
            var keyPath = "Any:Path";

            // Act
            var result = SetSharpJsonReader.Read(nullJson, keyPath);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Read_WithInvalidKeyPath_ReturnsNull(string invalidKeyPath)
        {
            // Act
            var result = SetSharpJsonReader.Read(_testJson, invalidKeyPath);

            // Assert
            Assert.Null(result);
        }
    }
}