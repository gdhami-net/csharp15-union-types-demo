using System.Text.Json;
using System.Text.Json.Serialization;
using Books;

namespace Books.Tests;

[JsonUnion(TypeClassifier = typeof(JsonUnionTypeStructuralClassifier))]
public union ClassifiedLookup(Book, NotFound, Invalid);

[JsonUnion(TypeClassifier = typeof(JsonUnionTypeStructuralClassifier))]
public union BookOrInvalid(Book, Invalid);

public class JsonTests
{
    private static readonly JsonSerializerOptions Web = JsonSerializerOptions.Web;

    [Fact]
    public void Each_case_is_written_as_itself_with_no_type_marker()
    {
        Assert.Equal("""{"isbn":"9781234567897","title":"A Book That Exists"}""",
            JsonSerializer.Serialize<LookupResult>(new Book("9781234567897", "A Book That Exists"), Web));
        Assert.Equal("""{"isbn":"9789999999999"}""",
            JsonSerializer.Serialize<LookupResult>(new NotFound("9789999999999"), Web));
        Assert.Equal("null", JsonSerializer.Serialize(default(LookupResult), Web));
    }

    [Fact]
    public void Reading_it_back_fails_when_two_cases_are_objects()
    {
        var ex = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<LookupResult>("""{"isbn":"9789999999999"}""", Web));
        Assert.StartsWith(
            "JSON value type 'Object' is ambiguous for union type 'Books.LookupResult' because multiple case types can use this value type. Specify a custom type classifier to support deserialization.",
            ex.Message);
    }

    [Fact]
    public void The_structural_classifier_refuses_a_case_whose_properties_another_case_also_has()
    {
        var ex = Assert.Throws<NotSupportedException>(() =>
            JsonSerializer.Deserialize<ClassifiedLookup>("""{"isbn":"9789999999999"}""", Web));
        Assert.Equal(
            "The JsonUnionTypeStructuralClassifier cannot classify union type 'Books.Tests.ClassifiedLookup' because the case type 'Books.NotFound' can never be selected uniquely. The case type 'Books.Book' recognizes every property name recognized by 'Books.NotFound' and has no stricter required-property or unmapped-member constraints.",
            ex.Message);
    }

    [Fact]
    public void The_structural_classifier_works_when_every_case_has_a_property_of_its_own()
    {
        var book = JsonSerializer.Deserialize<BookOrInvalid>("""{"isbn":"9781234567897","title":"A Book That Exists"}""", Web);
        var bad = JsonSerializer.Deserialize<BookOrInvalid>("""{"reason":"An ISBN has 13 digits."}""", Web);

        Assert.Equal(new Book("9781234567897", "A Book That Exists"), book.Value);
        Assert.Equal(new Invalid("An ISBN has 13 digits."), bad.Value);
    }

    [Fact]
    public void A_number_or_string_union_round_trips_with_no_setup()
    {
        Assert.Equal(42, JsonSerializer.Deserialize<Count>("42").Value);
        Assert.Equal("hello", JsonSerializer.Deserialize<Count>("\"hello\"").Value);
    }
}
