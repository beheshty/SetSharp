# SetSharp: Your AppSettings.json's Best Friend (and Code Generator Extraordinaire!)

Ever wished your `appsettings.json` could just... generate C# classes for you? Well, wish no more! SetSharp is here to sprinkle a little magic (and a lot of code) into your development workflow. Say goodbye to manual mapping and hello to strongly-typed configuration. It's like having a tiny, super-efficient assistant that turns your JSON into beautiful, usable C# classes, all while you sip your coffee.

## What is SetSharp?

SetSharp is a C# source generator that takes your `appsettings.json` and automatically generates C# classes that mirror its structure. This means you get compile-time safety, IntelliSense goodness, and a much cleaner way to access your application's settings. No more stringly-typed nightmares – just pure, unadulterated, strongly-typed bliss.

## Getting Started

Integrating SetSharp into your project is a simple, three-step process.

### 1. Install the NuGet Package

Add the SetSharp NuGet package to your project.

```bash
dotnet add package SetSharp
```

### 2. Configure your .csproj file
For the source generator to find your configuration, you must include `appsettings.json` as an AdditionalFile in your project file. This tells the compiler to pass the file to SetSharp for processing.
```xml
<ItemGroup>
    <AdditionalFiles Include="appsettings.json" />
</ItemGroup>
```

### 3. Build Your Project
That's it! When you build your project (`dotnet build`), SetSharp automatically generates the C# classes based on your JSON structure. You'll find them ready to use in your code under the SetSharp.Configuration namespace.

## Usage Example

Let's see how SetSharp handles a more realistic configuration file and how you can use the generated classes with dependency injection.

### 1. The Configuration (`appsettings.json`)

Here is a sample `appsettings.json` with nested objects, simple values, and an array of objects.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=test;Trusted_Connection=True;"
  },
  "FeatureFlags": [
    {
      "Name": "UseNewDashboard",
      "IsEnabled": true
    },
    {
      "Name": "EnableExperimentalApi",
      "IsEnabled": false
    }
  ]
}
```

### 2. The Generated Code (Done for You!)

After a build, SetSharp generates the following classes for you automatically.

```csharp
using System.Collections.Generic;

namespace SetSharp.Configuration
{
    /// <summary>Represents the root of the configuration settings.</summary>
    [System.CodeDom.Compiler.GeneratedCode("SetSharp", "1.0.6.0")]
    public partial class RootOptions
    {
        public LoggingOptions Logging { get; set; }
        public ConnectionStringsOptions ConnectionStrings { get; set; }
        public List<FeatureFlagsItemOptions> FeatureFlags { get; set; }
    }

    /// <summary>Represents the 'Logging' section from the configuration.</summary>
    [System.CodeDom.Compiler.GeneratedCode("SetSharp", "1.0.6.0")]
    public partial class LoggingOptions
    {
        /// <summary>The configuration section name for this class: "Logging"</summary>
        public const string SectionName = "Logging";

        public LogLevelOptions LogLevel { get; set; }
    }

    /// <summary>Represents the 'ConnectionStrings' section from the configuration.</summary>
    [System.CodeDom.Compiler.GeneratedCode("SetSharp", "1.0.6.0")]
    public partial class ConnectionStringsOptions
    {
        /// <summary>The configuration section name for this class: "ConnectionStrings"</summary>
        public const string SectionName = "ConnectionStrings";

        public string DefaultConnection { get; set; }
    }

    /// <summary>Represents the 'FeatureFlags' section from the configuration.</summary>
    [System.CodeDom.Compiler.GeneratedCode("SetSharp", "1.0.6.0")]
    public partial class FeatureFlagsItemOptions
    {
        /// <summary>The configuration section name for this class: "FeatureFlags"</summary>
        public const string SectionName = "FeatureFlags";

        public string Name { get; set; }
        public bool IsEnabled { get; set; }
    }

    /// <summary>Represents the 'Logging:LogLevel' section from the configuration.</summary>
    [System.CodeDom.Compiler.GeneratedCode("SetSharp", "1.0.6.0")]
    public partial class LogLevelOptions
    {
        /// <summary>The configuration section name for this class: "Logging:LogLevel"</summary>
        public const string SectionName = "Logging:LogLevel";

        public string Default { get; set; }
        [Microsoft.Extensions.Configuration.ConfigurationKeyName("Microsoft.AspNetCore")]
        public string MicrosoftAspNetCore { get; set; }
    }

}
```

### 3. Using the Settings in Your Application

Here's how you can use the generated classes in a `Program.cs` file:

```csharp
using Microsoft.Extensions.Configuration;
using SetSharp.Configuration; // Namespace for generated classes
using System;
using System.IO;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// --- Logging Options ---
var loggingOptions = builder.Configuration.GetSection(LogLevelOptions.SectionName).Get<LogLevelOptions>();
if (loggingOptions is not null)
{
    Console.WriteLine("Logging Options:");
    Console.WriteLine($"  • {nameof(loggingOptions.Default)}: {loggingOptions.Default}");
    Console.WriteLine($"  • {nameof(loggingOptions.MicrosoftAspNetCore)}: {loggingOptions.MicrosoftAspNetCore}");
    Console.WriteLine();
}
```

## Future Roadmap

This section is currently in development. The following is a list of features I am planning for future updates.

* [x] **Partial Classes for Customization:** To give you more control, I will be making the generated classes `partial`. This will allow you to extend them with your own logic or properties without altering the generated code.
* [ ] **Option Pattern Integration:** I am planning to add helper methods to easily register the generated configuration classes with the `IOptions` pattern, which will make dependency injection more straightforward.
* [ ] **`SectionName` Property:** I am looking into adding a `SectionName` property to each generated class. This will make it easier to identify which part of the JSON a class maps to and may offer other benefits.

## Contributing

I welcome contributions to this project. Whether it's a new feature, a bug fix, or an improvement to the documentation, your help is appreciated. SetSharp is an open-source project, and I believe in the power of community collaboration. If you have ideas, suggestions, or want to contribute to the code, please feel free to open an issue or submit a pull request. Together, we can make SetSharp even sharper.

