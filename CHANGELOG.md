# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.0.0] - 2025-08-19
### Breaking Change
- **Generator configuration has been moved from `appsettings.json` to MSBuild properties in the `.csproj` file.** The `SetSharp` section within `appsettings.json` is no longer read. Users upgrading to this version **must** migrate their settings to their project file to continue configuring the generator.

### Added
- A new MSBuild property, `SetSharp_SourceFile`, allows you to specify a custom JSON file for generation, replacing the default `appsettings.json`.
- The `SetSharp_OptionPatternGenerationEnabled` MSBuild property now controls the generation of `IOptions` extension methods.
- A new diagnostic (`SSG004`) will now report an error if the specified source file cannot be found in the project's `AdditionalFiles`.

### Changed
- Refactored settings logic into a dedicated `GeneratorSettingsProvider` class to separate concerns.

## [2.1.0] - 2025-08-10
### Changed
- Enhanced the type inference logic for JSON arrays. The source generator now analyzes **all** objects within an array to create a single, comprehensive class for the list items. This replaces the previous behavior of only inspecting the first item, ensuring that properties from all objects are correctly captured.

## [2.0.1] - 2025-08-04
### Added
- Added `Shipped.md` and `Unshipped.md` files to track analyzer diagnostics and releases. These files follow the official Roslyn analyzer release tracking format.

### Notes
- This is a documentation-only update with no functional code changes.


## [2.0.0] - 2025-07-16
### Changed
- **Breaking Change**: All generated classes are now `record` types instead of regular `class` types.
- All properties use `init` accessors instead of `set` to support immutability.
- Collections are now generated as `ImmutableList<T>` instead of mutable `List<T>`.

### Notes
- These changes improve immutability and align with modern C# practices.
- This is a major version bump due to potential breaking changes in downstream usage, especially if consumers relied on mutable types or reflection-based assumptions.

## [1.3.0] - 2025-07-13
### Added
- Descriptive error messages if required dependencies are missing.
- New diagnostic IDs: `SSG001`, `SSG002`, `SSG003`.


