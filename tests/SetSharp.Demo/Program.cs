using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SetSharp.Configuration;


Console.WriteLine("Configuration Explorer is live! Let's see what secrets your appsettings hold...\n");

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// --- Register All Your Settings with a Single Call ---
builder.Services.AddAllGeneratedOptions(builder.Configuration);

using var host = builder.Build();
// --- Logging Options ---
var loggingOptions = host.Services.GetRequiredService<IOptions<LogLevelOptions>>().Value;
if (loggingOptions is not null)
{
    Console.WriteLine("Logging Options:");
    Console.WriteLine($"  • {nameof(loggingOptions.Default)}: {loggingOptions.Default}");
    Console.WriteLine($"  • {nameof(loggingOptions.MicrosoftAspNetCore)}: {loggingOptions.MicrosoftAspNetCore}");
    Console.WriteLine();
}

// --- Connection Strings ---
var connectionOptions = host.Services.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value;
if (connectionOptions is not null)
{
    Console.WriteLine("Connection Strings:");
    Console.WriteLine($"  • {nameof(connectionOptions.DefaultConnection)}: {connectionOptions.DefaultConnection}");
    Console.WriteLine();
}

// --- Feature Flags ---
var featureFlagOptions = host.Services.GetRequiredService<IOptions<List<FeatureFlagsItemOptions>>>().Value;
if (featureFlagOptions is not null && featureFlagOptions.Count != 0)
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


