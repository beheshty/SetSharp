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

3.  **Build Your Project:** That's it! When you build your project, SetSharp will automatically generate the C# classes based on your JSON structure. You'll find them ready to use in your code, under the SetSharp.Configuration namespace.

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

This section is currently in development. The following is a list of features I am planning for future updates.

* [ ] **Partial Classes for Customization:** To give you more control, I will be making the generated classes `partial`. This will allow you to extend them with your own logic or properties without altering the generated code.
* [ ] **Option Pattern Integration:** I am planning to add helper methods to easily register the generated configuration classes with the `IOptions` pattern, which will make dependency injection more straightforward.
* [ ] **`SectionName` Property:** I am looking into adding a `SectionName` property to each generated class. This will make it easier to identify which part of the JSON a class maps to and may offer other benefits.

## Contributing

I welcome contributions to this project. Whether it's a new feature, a bug fix, or an improvement to the documentation, your help is appreciated. SetSharp is an open-source project, and I believe in the power of community collaboration. If you have ideas, suggestions, or want to contribute to the code, please feel free to open an issue or submit a pull request. Together, we can make SetSharp even sharper.

