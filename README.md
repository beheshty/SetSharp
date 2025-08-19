# SetSharp

[![NuGet Version](https://img.shields.io/nuget/v/SetSharp.svg?style=flat-square)](https://www.nuget.org/packages/SetSharp/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SetSharp.svg?style=flat-square)](https://www.nuget.org/packages/SetSharp/)
[![Build Status](https://img.shields.io/github/actions/workflow/status/beheshty/SetSharp/.github/workflows/publish.yml?branch=master&style=flat-square)](https://github.com/beheshty/SetSharp/actions) 

**Tired of manually mapping `appsettings.json` to C# classes? SetSharp is a powerful .NET source generator that automatically creates strongly-typed C# configuration classes directly from your JSON settings, seamlessly integrating with the `IOptions` pattern.**

Say goodbye to magic strings and runtime errors. With SetSharp, your configuration becomes a first-class citizen in your codebase, complete with compile-time safety and IntelliSense support.

## Key Features

-   **Automatic POCO Generation:** Mirrors your `appsettings.json` structure into clean, ready-to-use C# records.
-   **Strongly-Typed Access:** No more `_configuration["Section:Key"]`. Access settings with `options.Value.Section.Key`.
-   **Seamless DI Integration:** Automatically generates extension methods to register your configuration with Dependency Injection using the `IOptions` pattern.
-   **Zero Runtime Overhead:** All code generation happens at compile time, adding no performance cost to your application.
-   **Configurable Generation:** Easily enable or disable `IOptions` pattern integration to fit your project's needs.

## Prerequisites

SetSharp has the following dependencies that you need to be aware of:

-   **`Microsoft.Extensions.Configuration.Abstractions`**
    This is a fundamental dependency and is **always required** for the generator to function. If this package is missing, you will receive a compile-time error (`SSG003`).

-   **`Microsoft.Extensions.Options.ConfigurationExtensions`**
    This package is required **only if** you are using the automatic `IOptions` pattern generation (which is enabled by default). If this package is missing while the feature is active, you will receive a compile-time error (`SSG002`).

*You can disable the `IOptions` pattern feature and remove this second dependency by setting the `SetSharp_OptionPatternGenerationEnabled` MSBuild property to `false` in your `.csproj` file. See the Configuration section for details.*

## Getting Started

Follow these steps to integrate SetSharp into your .NET project.

### 1. Install the NuGet Package

Add the SetSharp NuGet package to your project using the .NET CLI or the NuGet Package Manager.

```bash
dotnet add package SetSharp
```

### 2. Add `appsettings.json` to your Project File

For the source generator to work its magic, you must explicitly tell the compiler to include your `appsettings.json` file during the build process. Edit your `.csproj` file and add the following `ItemGroup`:

```xml
<ItemGroup>
  <AdditionalFiles Include="appsettings.json" />
</ItemGroup>
```

### 3. Build Your Project

That's it! Simply build your project. SetSharp will run automatically, generating your configuration records in the background.

```bash
dotnet build
```

## How to Use

SetSharp generates two key things for you: strongly-typed records and Dependency Injection extension methods.

### 1. Generated Configuration Records

For an `appsettings.json` like this:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MyDb;Trusted_Connection=True;"
  },
  "FeatureManagement": {
    "EnableNewDashboard": true
  }
}
```

SetSharp will generate corresponding C# records, within the `SetSharp.Configuration` namespace:

```csharp
namespace SetSharp.Configuration
{
    public partial record RootOptions
    {
        public ConnectionStringsOptions ConnectionStrings { get; init; }
        public FeatureManagementOptions FeatureManagement { get; init; }
    }

    public partial record ConnectionStringsOptions
    {
        public const string SectionName = "ConnectionStrings";
        public string DefaultConnection { get; init; }
    }

    public partial record FeatureManagementOptions
    {
        public const string SectionName = "FeatureManagement";
        public bool EnableNewDashboard { get; init; }
    }
}
```

### 2. Dependency Injection with the Options Pattern

SetSharp makes registering these records with your DI container incredibly simple by generating extension methods for `IServiceCollection`.

You have two ways to register your settings:

**A) Register a specific option:**

Use the generated `Add[OptionName]` method to register a single configuration section.

```csharp
// In your Program.cs or Startup.cs
builder.Services.AddConnectionStringsOptions(builder.Configuration);
```

**B) Register all options at once:**

Use the convenient `AddAllGeneratedOptions` method to register all settings from your `appsettings.json` in a single call.

```csharp
// In your Program.cs or Startup.cs
builder.Services.AddAllGeneratedOptions(builder.Configuration);
```

Once registered, you can inject your settings anywhere in your application using the standard `IOptions<T>` interface.

```csharp
public class MyService
{
    private readonly ConnectionStringsOptions _connectionStrings;

    public MyService(IOptions<ConnectionStringsOptions> connectionStringsOptions)
    {
        _connectionStrings = connectionStringsOptions.Value;
    }

    public void DoWork()
    {
        var connectionString = _connectionStrings.DefaultConnection;
        // ... use the connection string
    }
}
```

## Configuration (via MSBuild Properties)

You can control the behavior of SetSharp by setting MSBuild properties in your project's `.csproj` file.

```xml
<PropertyGroup>
  <SetSharp_OptionPatternGenerationEnabled>false</SetSharp_OptionPatternGenerationEnabled>
  <SetSharp_SourceFile>config/production.json</SetSharp_SourceFile>
</PropertyGroup>
```

### `SetSharp_OptionPatternGenerationEnabled`

Controls whether the Dependency Injection extension methods for the `IOptions` pattern are generated.

-   **`true`** (Default): Generates `Add[OptionName]` and `AddAllGeneratedOptions` extension methods. Requires a reference to `Microsoft.Extensions.Options.ConfigurationExtensions`.
-   **`false`**: Skips generation of DI extension methods. This removes the dependency on `Microsoft.Extensions.Options.ConfigurationExtensions`.

### `SetSharp_SourceFile`

Specifies the name of the JSON configuration file to use as the source for generation. If you use this, remember to update the `<AdditionalFiles>` item in your `.csproj` to match the new name.

-   **Default:** `appsettings.json`

If this property is set but the file cannot be found in the project's `AdditionalFiles`, you will receive a compile-time error (`SSG004`).

## Future Plans

SetSharp is actively being developed. Have an idea or a feature request? Feel free to open an issue on GitHub to discuss it!

## Changelog
See [CHANGELOG.md](https://github.com/beheshty/SetSharp/blob/master/CHANGELOG.md) for version history.

## Contributing

Contributions are welcome! Whether it's a new feature idea, a bug report, or a pull request, your input is valued. Please feel free to open an issue to discuss your ideas or submit a pull request with your improvements.

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/beheshty/SetSharp/blob/master/LICENSE.txt) file for details.
