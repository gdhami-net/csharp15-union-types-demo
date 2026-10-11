using Books;

namespace NullableCase;

// A case value that may be null does get a warning on the way into the union.
// check.sh expects exactly one CS8604 from this file.
public static class Probe
{
    public static LookupResult Wrap(Book? maybe) => maybe;
}
