namespace SameTypeTwice;

// A generic Result union compiles, until both type arguments are the same type.
// check.sh expects this project to fail with CS0457.
public union Result<T, TError>(T, TError);

public static class Probe
{
    public static Result<int, string> Fine(bool ok) => ok ? 1 : "bad";

    public static Result<string, string> Ambiguous() => "which case is this?";
}
