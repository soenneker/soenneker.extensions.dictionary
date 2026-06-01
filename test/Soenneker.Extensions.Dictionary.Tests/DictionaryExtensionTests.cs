using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using Soenneker.Tests.Unit;

namespace Soenneker.Extensions.Dictionary.Tests;

public class DictionaryExtensionTests : UnitTest
{
    [Test]
    public void TryGetValueFromKey_should_get_value()
    {
        string value = Faker.Random.AlphaNumeric(5);
        string key = Faker.Random.AlphaNumeric(5);

        var dictionary = new Dictionary<string, string>
        {
            {Faker.Random.AlphaNumeric(5), Faker.Random.AlphaNumeric(5)},
            {key, value}
        };

        bool succeeded = dictionary.TryGetKeyFromValue(value, out string? result);

        succeeded.Should().BeTrue();

        result.Should().Be(key);
    }

    [Test]
    public void AddRange_should_add_items()
    {
        var dictionary = new Dictionary<string, ComplexObject>();

        var list = AutoFaker.Generate<List<ComplexObject>>();

        dictionary.AddRange(list, c => c.Name);

        ComplexObject firstItem = list.First();

        dictionary[firstItem.Name].Value.Should().Be(firstItem.Value);
        dictionary.Count.Should().Be(list.Count);
    }

    [Test]
    public void ToObject_should_set_properties()
    {
        var dictionary = new Dictionary<string, object>
        {
            {nameof(ToObjectTarget.Name), "Jane"},
            {nameof(ToObjectTarget.Age), "42"}
        };

        ToObjectTarget result = dictionary.ToObject<ToObjectTarget>();

        result.Name.Should().Be("Jane");
        result.Age.Should().Be(42);
    }

    private sealed class ToObjectTarget
    {
        public string? Name { get; set; }

        public int Age { get; set; }
    }
}
