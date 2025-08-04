# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.1] - 2025-08-04
### Added
- Added `Shipped.md` and `Unshipped.md` files to track analyzer diagnostics and releases.
  These files follow the official Roslyn analyzer release tracking format.

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


