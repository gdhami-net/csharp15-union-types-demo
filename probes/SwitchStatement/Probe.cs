using Books;

namespace SwitchStatement;

// The same missing case as probes/MissingCase, in a switch statement.
// check.sh expects this project to build with zero warnings.
public static class Probe
{
    public static string Describe(LookupResult result)
    {
        switch (result)
        {
            case Book book:
                return $"Found {book.Title}";
            case NotFound nf:
                return $"No book {nf.Isbn}";
        }

        return "fell through";
    }
}
