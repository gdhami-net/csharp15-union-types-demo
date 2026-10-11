using Books;

namespace MissingCase;

// Built on its own by check.sh, which expects exactly one CS8509 from this file.
public static class Describer
{
    public static string Describe(LookupResult result) => result switch
    {
        Book book => $"Found {book.Title}",
        NotFound nf => $"No book {nf.Isbn}",
    };
}
