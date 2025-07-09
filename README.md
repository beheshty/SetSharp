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

*   [ ] **Option Pattern Integration:** We're planning to add helper methods to easily register your generated configuration classes with the `IOptions` pattern, making dependency injection a breeze.
*   [ ] **`SectionName` Property:** Imagine a world where you can instantly know which part of your JSON a class maps to! We're looking into adding a `SectionName` property to each generated class for easier identification and potentially other benefits.
*   [ ] **Partial Classes for Customization:** To give you even more control, we'll be making the generated classes `partial`, allowing you to extend them with your own logic or properties without touching the generated code.

## Contributing

We love contributions! Whether it's a new feature, a bug fix, or an improvement to the documentation, your help is always welcome. SetSharp is an open-source project, and we believe in the power of community. If you have ideas, suggestions, or want to dive into the code, don't hesitate to open an issue or submit a pull request. Let's make SetSharp even sharper together!

