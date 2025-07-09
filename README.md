# SetSharp: Your AppSettings.json's Best Friend (and Code Generator Extraordinaire!)

Ever wished your `appsettings.json` could just... generate C# classes for you? Well, wish no more! SetSharp is here to sprinkle a little magic (and a lot of code) into your development workflow. Say goodbye to manual mapping and hello to strongly-typed configuration. It's like having a tiny, super-efficient assistant that turns your JSON into beautiful, usable C# classes, all while you sip your coffee.

## What is SetSharp?

SetSharp is a C# source generator that takes your `appsettings.json` and automatically generates C# classes that mirror its structure. This means you get compile-time safety, IntelliSense goodness, and a much cleaner way to access your application's settings. No more stringly-typed nightmares – just pure, unadulterated, strongly-typed bliss.

## How to Use It

Using SetSharp is as easy as pie (and much less messy!). Here's how to get started:

1.  **Install the NuGet Package:** Add the SetSharp NuGet package to your project.

    ```bash
    dotnet add package SetSharp
    ```

2.  **Add your `appsettings.json` as an Additional File:** This is the secret sauce! To make sure SetSharp can find and process your configuration, you need to include your `appsettings.json` as an `AdditionalFile` in your `.csproj` file. This tells the source generator where to look.

    ```xml
    <ItemGroup>
        <AdditionalFiles Include="appsettings.json" />
    </ItemGroup>
    ```

3.  **Build Your Project:** That's it! When you build your project, SetSharp will automatically generate the C# classes based on your JSON structure. You'll find them ready to use in your code, under the SetSharp namespace.

    Now you can access your settings with full type safety:
    ```csharp
    // Example: Assuming your appsettings.json has a structure like:
    // {"MySetting": {"ApiKey": "your-key", "FeatureToggle": true}}
    // The generated class might look something like:
    namespace SetSharp.Configuration
    {
        public class RootOptions
        {
            public MySettingOptions MySettings { get; set; }
        }

        public class MySettingOptions
        {
            public string ApiKey { get; set; }
            public bool FeatureToggle { get; set; }
        }
    }
    ```

## Future Roadmap

This section is currently under construction, much like a secret lair for future features. Stay tuned for exciting updates!

*   [ ] Feature Idea 1
*   [ ] Feature Idea 2

## Future Fixes

Even the best code needs a little polish now and then. This section will be updated with any planned bug fixes or improvements.

*   [ ] Bug Fix 1
*   [ ] Improvement 1


