[![](https://img.shields.io/nuget/v/Soenneker.Extensions.Dictionary.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.Dictionary/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.dictionary/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.dictionary/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Extensions.Dictionary.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.Dictionary/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.dictionary/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.dictionary/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.Dictionary

Extensions for flattening dictionary values, merging keyed objects, mapping string/object data to objects, and reverse value lookup.

## Installation

```bash
dotnet add package Soenneker.Extensions.Dictionary
```

## Flatten lists of values

```csharp
using Soenneker.Extensions.Dictionary;

var groups = new Dictionary<string, List<int>>
{
    ["odd"] = [1, 3],
    ["even"] = [2, 4]
};

List<int> values = groups.ToFlattenedValuesList(); // 1, 3, 2, 4
```

The result follows dictionary enumeration order and then each list's order. Null lists are skipped, and the source lists are not changed.

## Add or update ranges

```csharp
var people = new[]
{
    new Person("a", "Alice"),
    new Person("b", "Bob")
};

var byId = new Dictionary<string, Person>();
byId.AddRange(people, person => person.Id);

public sealed record Person(string Id, string Name);
```

`AddRange()` compiles the expression selector on each call. Use `AddRangeCompiled()` with a reusable `Func<TValue, TKey>` in repeated or hot paths. Both methods mutate the destination, add missing keys, and replace existing values. If multiple input items produce the same key, the last item wins.

`AddDictionary(source, dictionary)` applies the same add-or-replace behavior to every entry from another dictionary.

## Map a dictionary to an object

```csharp
var data = new Dictionary<string, object>
{
    ["name"] = "Jane",
    ["Age"] = "42"
};

PersonDto person = data.ToObject<PersonDto>();
// person.Name == "Jane"
// person.Age == 42

public sealed class PersonDto
{
    public string? Name { get; set; }
    public int Age { get; set; }
}
```

Property names are matched case-insensitively. Directly assignable values are used as-is; otherwise, `Convert.ChangeType` is attempted. Unknown properties, read-only properties, nulls that cannot be assigned, and values that fail ordinary type conversion are skipped. This is a lightweight mapper, not a serializer: nested objects, collections, enums, and nullable conversions may require explicit preparation.

## Find a key by value

```csharp
bool found = byId.TryGetKeyFromValue(people[0], out string? key);
```

`TryGetKeyFromValue()` uses reference equality first and then `Equals`. When several keys have equal values, it returns the first match in dictionary enumeration order. When no value matches, it returns `false` and sets the output key to its default value.
