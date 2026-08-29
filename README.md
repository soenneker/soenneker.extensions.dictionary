[![](https://img.shields.io/nuget/v/Soenneker.Extensions.Dictionary.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.Dictionary/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.dictionary/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.dictionary/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Extensions.Dictionary.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.Dictionary/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.dictionary/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.dictionary/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.Dictionary
A collection of helpful Dictionary extension methods.

## Installation

```bash
dotnet add package Soenneker.Extensions.Dictionary
```

## Quick start

```csharp
using Soenneker.Extensions.Dictionary;
```

Import the namespace, then call the extension methods directly on the matching value.

## Common operations

- `ToFlattenedValuesList()` - Flattens the values of a dictionary, where each key maps to a list of values, into a single list.
- `AddRange()` - Adds (or updates!) an enumerable to a dictionary without a loop in managed code. Compiles the expression and loops over the enumerable, adding to the dictionary via the expression selector.
- `AddRangeCompiled()` - Adds or updates values using a precompiled key selector. Prefer this overload in repeated or hot paths.
- `AddDictionary()` - Loops over the target and adds each of the items into the source. Useful for readonly scenarios.
- `ToObject()` - Iterates through each one of the keys in the dictionary to build a new T by looking up property names, and setting that to value of the key value pair.
- `TryGetKeyFromValue()` - Tries to retrieve a key from a particular value in the dictionary. If there are multiple of the same value, it returns the first key.
