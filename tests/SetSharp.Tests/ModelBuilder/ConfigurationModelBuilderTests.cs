using SetSharp.ModelBuilder;


namespace SetSharp.Tests.ModelBuilder
{
    public class ConfigurationModelBuilderTests
    {
        [Fact]
        public void BuildFrom_WithFlatPrimitives_ShouldCreateModelCorrectly()
        {
            // Arrange
            var builder = new ConfigurationModelBuilder();
            var root = new Dictionary<string, object>
            {
                { "ConnectionString", "Server=.;Database=Test;"},
                { "Timeout", 30 },
                { "MaxRetries", 9223372036854775807L }, 
                { "EnableLogging", true },
                { "DefaultThreshold", 0.95 }
            };

            // Act
            var result = builder.BuildFrom(root);

            // Assert
            var rootModel = Assert.Single(result);
            Assert.Equal("RootOptions", rootModel.ClassName);
            Assert.Equal("", rootModel.SectionPath);
            Assert.Equal(5, rootModel.Properties.Count);

            Assert.Equal("string", rootModel.Properties.First(p => p.PropertyName == "ConnectionString").PropertyType);
            Assert.Equal("long", rootModel.Properties.First(p => p.PropertyName == "MaxRetries").PropertyType);
            Assert.Equal("bool", rootModel.Properties.First(p => p.PropertyName == "EnableLogging").PropertyType);
            Assert.Equal("double", rootModel.Properties.First(p => p.PropertyName == "DefaultThreshold").PropertyType);
        }

        [Fact]
        public void BuildFrom_WithNestedObject_ShouldCreateSeparateClassModel()
        {
            // Arrange
            var builder = new ConfigurationModelBuilder();
            var root = new Dictionary<string, object>
            {
                { "Logging", new Dictionary<string, object> {
                    { "Level", "Info" },
                    { "RetainDays", 7 }
                }}
            };

            // Act
            var result = builder.BuildFrom(root);

            // Assert
            Assert.Equal(2, result.Count);

            // Test Root Class
            var rootModel = result.First(c => c.ClassName == "RootOptions");
            var loggingProperty = Assert.Single(rootModel.Properties);
            Assert.Equal("Logging", loggingProperty.PropertyName);
            Assert.Equal("LoggingOptions", loggingProperty.PropertyType); // Should point to the new class

            // Test Nested Class
            var loggingModel = result.First(c => c.ClassName == "LoggingOptions");
            Assert.Equal("Logging", loggingModel.SectionPath);
            Assert.Equal(2, loggingModel.Properties.Count);
            Assert.Contains(loggingModel.Properties, p => p.PropertyName == "Level" && p.PropertyType == "string");
            Assert.Contains(loggingModel.Properties, p => p.PropertyName == "RetainDays" && p.PropertyType == "int");
        }

        [Fact]
        public void BuildFrom_WithListOfPrimitives_ShouldInferListType()
        {
            // Arrange
            var builder = new ConfigurationModelBuilder();
            var root = new Dictionary<string, object>
            {
                { "AllowedHosts", new List<object> { "localhost", "127.0.0.1" } }
            };

            // Act
            var result = builder.BuildFrom(root);

            // Assert
            var rootModel = Assert.Single(result);
            var listProperty = Assert.Single(rootModel.Properties);
            Assert.Equal("AllowedHosts", listProperty.PropertyName);
            Assert.Equal("List<string>", listProperty.PropertyType);
        }

        [Fact]
        public void BuildFrom_WithEmptyList_ShouldDefaultToListOfObject()
        {
            // Arrange
            var builder = new ConfigurationModelBuilder();
            var root = new Dictionary<string, object>
            {
                { "EmptyList", new List<object>() }
            };

            // Act
            var result = builder.BuildFrom(root);

            // Assert
            var rootModel = Assert.Single(result);
            var listProperty = Assert.Single(rootModel.Properties);
            Assert.Equal("EmptyList", listProperty.PropertyName);
            Assert.Equal("List<object>", listProperty.PropertyType);
        }

        [Fact]
        public void BuildFrom_WithListOfObjects_ShouldCreateSeparateClassForListItems()
        {
            // Arrange
            var builder = new ConfigurationModelBuilder();
            var root = new Dictionary<string, object>
            {
                { "Endpoints", new List<object> {
                    new Dictionary<string, object> {
                        { "Name", "Primary" },
                        { "Url", "https://api.example.com" }
                    }
                }}
            };

            // Act
            var result = builder.BuildFrom(root);

            // Assert
            Assert.Equal(2, result.Count);

            // Test Root Class
            var rootModel = result.First(c => c.ClassName == "RootOptions");
            var listProperty = Assert.Single(rootModel.Properties);
            Assert.Equal("Endpoints", listProperty.PropertyName);
            Assert.Equal("List<EndpointsItemOptions>", listProperty.PropertyType);

            // Test Nested List Item Class
            var endpointItemModel = result.First(c => c.ClassName == "EndpointsItemOptions");
            Assert.Equal("Endpoints", endpointItemModel.SectionPath);
            Assert.Equal(2, endpointItemModel.Properties.Count);
            Assert.Contains(endpointItemModel.Properties, p => p.PropertyName == "Name" && p.PropertyType == "string");
            Assert.Contains(endpointItemModel.Properties, p => p.PropertyName == "Url" && p.PropertyType == "string");
        }

        [Fact]
        public void NormalizeName_WithInvalidChars_ShouldSanitizeName()
        {
            // This test calls a private method. To test it directly, you could make it internal
            // or use a PrivateObject accessor. Here, we test it indirectly via its effect in BuildFrom.
            // Arrange
            var builder = new ConfigurationModelBuilder();
            var root = new Dictionary<string, object>
            {
                { "logging:level-default", "Warning" }, // Contains : and -
                { "1st-try", 1 }, // Starts with digit
                { "!@#$", "junk" } // Only invalid chars
            };

            // Act
            var result = builder.BuildFrom(root);

            // Assert
            var rootModel = Assert.Single(result);
            Assert.Equal(3, rootModel.Properties.Count);
            Assert.Contains(rootModel.Properties, p => p.PropertyName == "Loggingleveldefault");
            Assert.Contains(rootModel.Properties, p => p.PropertyName == "_1sttry");
            Assert.Contains(rootModel.Properties, p => p.PropertyName == "InvalidNameProperty");
        }

        [Fact]
        public void BuildFrom_WithDeeplyNestedObject_ShouldHandleRecursionCorrectly()
        {
            // Arrange
            var builder = new ConfigurationModelBuilder();
            var root = new Dictionary<string, object>
            {
                { "L1", new Dictionary<string, object> {
                    { "L2", new Dictionary<string, object> {
                        { "L3", new Dictionary<string, object> {
                            { "Name", "Deep" }
                        }}
                    }}
                }}
            };

            // Act
            var result = builder.BuildFrom(root);

            // Assert
            Assert.Equal(4, result.Count); // Root, L1, L2, L3

            // L1
            var l1Model = result.First(c => c.ClassName == "L1Options");
            Assert.Equal("L1", l1Model.SectionPath);
            Assert.Equal("L2Options", Assert.Single(l1Model.Properties).PropertyType);

            // L2
            var l2Model = result.First(c => c.ClassName == "L2Options");
            Assert.Equal("L1:L2", l2Model.SectionPath);
            Assert.Equal("L3Options", Assert.Single(l2Model.Properties).PropertyType);

            // L3
            var l3Model = result.First(c => c.ClassName == "L3Options");
            Assert.Equal("L1:L2:L3", l3Model.SectionPath);
            Assert.Equal("string", Assert.Single(l3Model.Properties).PropertyType);
        }

        [Fact]
        public void BuildFrom_WithEmptyDictionary_ShouldReturnRootWithNoProperties()
        {
            // Arrange
            var builder = new ConfigurationModelBuilder();
            var root = new Dictionary<string, object>();

            // Act
            var result = builder.BuildFrom(root);

            // Assert
            var rootModel = Assert.Single(result);
            Assert.Equal("RootOptions", rootModel.ClassName);
            Assert.Empty(rootModel.Properties);
        }
    }
}
