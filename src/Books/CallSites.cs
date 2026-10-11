using System.Diagnostics;

namespace Books;

public static class CallSites
{
    public static string Describe(BookService books, string isbn) => books.Find(isbn) switch
    {
        Book book => $"Found {book.Title}",
        NotFound nf => $"No book {nf.Isbn}",
        Invalid bad => bad.Reason,
    };

    public static string DescribeOld(BookService books, string isbn) => books.FindOld(isbn).Match(
        book => $"Found {book.Title}",
        error => error switch
        {
            BookNotFound nf => $"No book {nf.Isbn}",
            InvalidIsbn bad => bad.Reason,
            _ => throw new UnreachableException(),
        });

    // A union value nobody assigned: Value is null, and the switch above has no arm for it.
    public static string DescribeWithNull(LookupResult result) => result switch
    {
        Book book => $"Found {book.Title}",
        NotFound nf => $"No book {nf.Isbn}",
        Invalid bad => bad.Reason,
        null => "Nothing was looked up",
    };

    public static string DescribeResult(LookupResult result) => result switch
    {
        Book book => $"Found {book.Title}",
        NotFound nf => $"No book {nf.Isbn}",
        Invalid bad => bad.Reason,
    };
}
