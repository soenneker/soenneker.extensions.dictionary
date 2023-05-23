using System.Collections.Generic;
using FluentAssertions;
using Soenneker.Tests.Unit;
using Xunit;

namespace Soenneker.Extensions.Dictionary.Tests;

public class DictionaryExtensionTests : UnitTest
{
    [Fact]
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
}