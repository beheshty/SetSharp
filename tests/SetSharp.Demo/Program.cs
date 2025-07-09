using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SetSharp.Configuration;


Console.WriteLine("Configuration Explorer is live! Let's see what secrets your appsettings hold...\n");

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// --- Logging Options ---
var loggingOptions = builder.Configuration.GetSection("Logging:LogLevel").Get<LogLevelOptions>();
if (loggingOptions is not null)
{
    Console.WriteLine("Logging Options:");
    Console.WriteLine($"  • {nameof(loggingOptions.Default)}: {loggingOptions.Default}");
    Console.WriteLine($"  • {nameof(loggingOptions.MicrosoftAspNetCore)}: {loggingOptions.MicrosoftAspNetCore}");
    Console.WriteLine();
}

// --- Connection Strings ---
var connectionOptions = builder.Configuration.GetSection("ConnectionStrings").Get<ConnectionStringsOptions>();
if (connectionOptions is not null)
{
    Console.WriteLine("Connection Strings:");
    Console.WriteLine($"  • {nameof(connectionOptions.DefaultConnection)}: {connectionOptions.DefaultConnection}");
    Console.WriteLine();
}

// --- Feature Flags ---
var featureFlagOptions = new List<FeatureFlagsItemOptions>();
builder.Configuration.GetSection("FeatureFlags").Bind(featureFlagOptions);

if (featureFlagOptions.Count != 0)
{
    Console.WriteLine("Feature Flag Options:");
    for (int i = 0; i < featureFlagOptions.Count; i++)
    {
        var op = featureFlagOptions[i];
        Console.WriteLine($"  {i + 1}.");
        Console.WriteLine($"    • {nameof(op.Name)}: {op.Name}");
        Console.WriteLine($"    • {nameof(op.IsEnabled)}: {op.IsEnabled}");
        Console.WriteLine();
    }
}

Console.WriteLine("All configuration values displayed. Press any key to exit...");
Console.ReadKey();


