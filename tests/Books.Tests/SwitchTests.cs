using System.Runtime.CompilerServices;
using Books;

namespace Books.Tests;

public class SwitchTests
{
    private readonly BookService _books = new();

    [Theory]
    [InlineData("9781234567897", "Found A Book That Exists")]
    [InlineData("9789999999999", "No book 9789999999999")]
    [InlineData("12-34", "An ISBN has 13 digits.")]
    public void The_union_and_the_Result_class_give_the_same_answers(string isbn, string expected)
    {
        Assert.Equal(expected, CallSites.Describe(_books, isbn));
        Assert.Equal(expected, CallSites.DescribeOld(_books, isbn));
    }

    [Fact]
    public void A_default_union_gets_past_a_complete_switch_and_throws()
    {
        // No compiler warning on this call: check.sh builds the solution and expects zero warnings.
        var ex = Assert.Throws<SwitchExpressionException>(() => CallSites.DescribeResult(default));
        Assert.Equal("Non-exhaustive switch expression failed to match its input.\nUnmatched value was Books.LookupResult.",
            ex.Message.Replace("\r\n", "\n"));
    }

    [Fact]
    public void A_null_arm_catches_the_default_union()
    {
        Assert.Equal("Nothing was looked up", CallSites.DescribeWithNull(default));
    }
}
