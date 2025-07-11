using SetSharp.CodeGeneration;
using SetSharp.Models;

namespace SetSharp.Tests.CodeGeneration
{
    public class OptionsPatternGeneratorTests
    {
        [Fact]
        public void Generate_WithSingleClass_GeneratesCorrectMethods()
        {
            // Arrange
            var classes = new List<SettingClassInfo>
            {
                new SettingClassInfo { ClassName = "LoggingOptions", SectionPath = "Logging" }
            };

            // Act
            var result = OptionsPatternGenerator.Generate(classes);

            // Assert
            // Check for the specific Add[ClassName] method
            Assert.Contains("public static IServiceCollection AddLoggingOptions(this IServiceCollection services, IConfiguration configuration)", result);
            Assert.Contains("services.Configure<LoggingOptions>(configuration.GetSection(LoggingOptions.SectionName));", result);

            // Check for the AddAllGeneratedOptions method
            Assert.Contains("public static IServiceCollection AddAllGeneratedOptions(this IServiceCollection services, IConfiguration configuration)", result);
            Assert.Contains("services.AddLoggingOptions(configuration);", result);
        }

        [Fact]
        public void Generate_WithMultipleClasses_GeneratesAllRequiredMethods()
        {
            // Arrange
            var classes = new List<SettingClassInfo>
            {
                new SettingClassInfo { ClassName = "DatabaseOptions", SectionPath = "Database" },
                new SettingClassInfo { ClassName = "ApiOptions", SectionPath = "Api" }
            };

            // Act
            var result = OptionsPatternGenerator.Generate(classes);

            // Assert
            // Check for the first class's methods
            Assert.Contains("public static IServiceCollection AddDatabaseOptions(this IServiceCollection services, IConfiguration configuration)", result);
            Assert.Contains("services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));", result);

            // Check for the second class's methods
            Assert.Contains("public static IServiceCollection AddApiOptions(this IServiceCollection services, IConfiguration configuration)", result);
            Assert.Contains("services.Configure<ApiOptions>(configuration.GetSection(ApiOptions.SectionName));", result);

            // Check that AddAllGeneratedOptions contains calls to both methods
            Assert.Contains("public static IServiceCollection AddAllGeneratedOptions(this IServiceCollection services, IConfiguration configuration)", result);
            Assert.Contains("services.AddDatabaseOptions(configuration);", result);
            Assert.Contains("services.AddApiOptions(configuration);", result);
        }

        [Fact]
        public void Generate_WithMixedClasses_IgnoresClassesWithoutSectionPath()
        {
            // Arrange
            var classes = new List<SettingClassInfo>
            {
                new SettingClassInfo { ClassName = "RootOptions", SectionPath = "" }, // Should be ignored
                new SettingClassInfo { ClassName = "FeaturesOptions", SectionPath = "Features" } // Should be included
            };

            // Act
            var result = OptionsPatternGenerator.Generate(classes);

            // Assert
            // Verify the included class is generated
            Assert.Contains("public static IServiceCollection AddFeaturesOptions(this IServiceCollection services, IConfiguration configuration)", result);
            Assert.Contains("services.AddFeaturesOptions(configuration);", result);

            // Crucially, verify the ignored class is NOT generated
            Assert.DoesNotContain("AddRootOptions", result);
        }

        [Fact]
        public void Generate_WithEmptyList_GeneratesBoilerplateWithoutMethods()
        {
            // Arrange
            var classes = new List<SettingClassInfo>();

            // Act
            var result = OptionsPatternGenerator.Generate(classes);

            // Assert
            // Check that the basic structure is there
            Assert.Contains("namespace Microsoft.Extensions.DependencyInjection", result);
            Assert.Contains("public static class GeneratedOptionsExtensions", result);

            // Check that no actual registration methods were created
            Assert.DoesNotContain("public static IServiceCollection Add", result);
            Assert.DoesNotContain("AddAllGeneratedOptions", result);
        }

        [Fact]
        public void Generate_WithOnlyRootClass_GeneratesBoilerplateWithoutMethods()
        {
            // Arrange
            var classes = new List<SettingClassInfo>
            {
                new SettingClassInfo { ClassName = "RootOptions", SectionPath = null } // Should be ignored
            };

            // Act
            var result = OptionsPatternGenerator.Generate(classes);

            // Assert
            // Check that the basic structure is there
            Assert.Contains("public static class GeneratedOptionsExtensions", result);

            // Check that no methods were created for the root options class
            Assert.DoesNotContain("AddRootOptions", result);
            Assert.DoesNotContain("AddAllGeneratedOptions", result);
        }

        [Fact]
        public void Generate_ForStandardObject_UsesClassNameInConfigure()
        {
            // Arrange
            var classes = new List<SettingClassInfo>
            {
                new SettingClassInfo
                {
                    ClassName = "DatabaseOptions",
                    SectionPath = "Database",
                    IsFromCollection = false // Explicitly a standard object
                }
            };

            // Act
            var result = OptionsPatternGenerator.Generate(classes);

            // Assert
            // Verify the method signature is correct
            Assert.Contains("public static IServiceCollection AddDatabaseOptions(this IServiceCollection services, IConfiguration configuration)", result);

            // Verify it generates the standard Configure<T> call
            Assert.Contains("services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));", result);

            // Verify it does NOT generate the List<T> version
            Assert.DoesNotContain("services.Configure<System.Collections.Generic.List<DatabaseOptions>>", result);
        }

        [Fact]
        public void Generate_ForListObject_UsesListOfClassNameInConfigure()
        {
            // Arrange
            var classes = new List<SettingClassInfo>
            {
                new SettingClassInfo
                {
                    ClassName = "EndpointOptions",
                    SectionPath = "Endpoints",
                    IsFromCollection = true // This class represents an item in a collection
                }
            };

            // Act
            var result = OptionsPatternGenerator.Generate(classes);

            // Assert
            // Verify the method signature is correct (it doesn't change)
            Assert.Contains("public static IServiceCollection AddEndpointOptions(this IServiceCollection services, IConfiguration configuration)", result);

            // Verify it generates the Configure<List<T>> call with the fully qualified name
            Assert.Contains("services.Configure<System.Collections.Generic.List<EndpointOptions>>(configuration.GetSection(EndpointOptions.SectionName));", result);

            // Verify it does NOT generate the standard T version
            Assert.DoesNotContain("services.Configure<EndpointOptions>(configuration.GetSection(EndpointOptions.SectionName));", result);
        }

        [Fact]
        public void Generate_ForMixedObjectTypes_UsesCorrectConfigureTypeForEach()
        {
            // Arrange
            var classes = new List<SettingClassInfo>
            {
                new SettingClassInfo
                {
                    ClassName = "ApiOptions",
                    SectionPath = "Api",
                    IsFromCollection = false // Standard object
                },
                new SettingClassInfo
                {
                    ClassName = "FirewallRuleOptions",
                    SectionPath = "FirewallRules",
                    IsFromCollection = true // Collection item object
                }
            };

            // Act
            var result = OptionsPatternGenerator.Generate(classes);

            // Assert
            // Check standard object generation
            Assert.Contains("public static IServiceCollection AddApiOptions(this IServiceCollection services, IConfiguration configuration)", result);
            Assert.Contains("services.Configure<ApiOptions>(configuration.GetSection(ApiOptions.SectionName));", result);

            // Check collection object generation
            Assert.Contains("public static IServiceCollection AddFirewallRuleOptions(this IServiceCollection services, IConfiguration configuration)", result);
            Assert.Contains("services.Configure<System.Collections.Generic.List<FirewallRuleOptions>>(configuration.GetSection(FirewallRuleOptions.SectionName));", result);

            // Check that the AddAll method includes both
            Assert.Contains("public static IServiceCollection AddAllGeneratedOptions(this IServiceCollection services, IConfiguration configuration)", result);
            Assert.Contains("services.AddApiOptions(configuration);", result);
            Assert.Contains("services.AddFirewallRuleOptions(configuration);", result);
        }
    }
}
