using System.Text.Json;
using System.Text.Json.Serialization;

namespace Books.Tests;

// The other C# 15 way to close a set: a closed base record. It can carry a type marker in JSON.
[JsonPolymorphic]
[JsonDerivedType(typeof(Found), "found")]
[JsonDerivedType(typeof(Missing), "missing")]
public closed record Outcome;
public record Found(string Isbn, string Title) : Outcome;
public record Missing(string Isbn) : Outcome;

public class ClosedHierarchyTests
{
    private static string Describe(Outcome outcome) => outcome switch
    {
        Found f => $"Found {f.Title}",
        Missing m => $"No book {m.Isbn}",
    };

    [Fact]
    public void A_closed_hierarchy_round_trips_with_a_type_marker()
    {
        string json = JsonSerializer.Serialize<Outcome>(new Missing("9789999999999"), JsonSerializerOptions.Web);
        Assert.Equal("""{"$type":"missing","isbn":"9789999999999"}""", json);

        var back = JsonSerializer.Deserialize<Outcome>(json, JsonSerializerOptions.Web);
        Assert.Equal("No book 9789999999999", Describe(back!));
    }
}
