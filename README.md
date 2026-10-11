# csharp15-union-types-demo

A book lookup that returns a C# 15 union, next to the same lookup written against a hand-written
`Result<T, TError>` class. It backs the blog post
[Return a C# 15 union type instead of a Result class](https://gdhami.net/blog/return-a-csharp-15-union-type-instead-of-a-result-class).

## What it proves

- `public union LookupResult(Book, NotFound, Invalid);` compiles in a plain `net11.0` project with no
  `<LangVersion>` set. C# 15 is the default for .NET 11 from RC1 on.
- Each case converts to the union implicitly, including through `cond ? book : new NotFound(isbn)`.
- A switch expression with all three arms needs no discard arm and builds with zero warnings.
- Deleting one arm gives exactly one warning, captured from a real build:
  `warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern 'Books.Invalid' is not covered.`
  With `WarningsAsErrors=CS8509` the same code fails to build.
- The same Result-class code needs a `_ =>` arm, because the error side is an open record hierarchy.
- The generated type is a struct with `[Union]`, implements `IUnion`, has one constructor per case and a single
  `object` field behind `Value`. An `int` case is boxed on conversion; a record (class) case allocates nothing.
- `default(LookupResult)` passed to a switch with all three arms compiles without a warning and throws
  `SwitchExpressionException` at run time. A `null =>` arm catches it.
- System.Text.Json writes each case as itself, with no case marker:
  `{"isbn":"9789999999999"}` for `NotFound`, `null` for a default union.
- Reading it back into the union throws `JsonException` ("JSON value type 'Object' is ambiguous ..."), because two
  cases are JSON objects. An `int`/`string` union round-trips with no setup.
- `JsonUnionTypeStructuralClassifier` refuses this union with `NotSupportedException`, because every property of
  `NotFound` is also on `Book`. It works for `union BookOrInvalid(Book, Invalid)`, where each case has a property
  of its own.
- A minimal API endpoint that returns the union as it is answers 200 for all three cases. A switch in the
  endpoint gives 200, 404 and 400.
- A C# 15 `closed record` hierarchy with `[JsonPolymorphic]` writes `{"$type":"missing",...}`, reads it back, and
  its switch needs no discard arm either.

## How to run it

Install the .NET 11 SDK, RC1 or later, from https://dotnet.microsoft.com/download/dotnet/11.0.
`global.json` asks for 11.0.100-rc.1 or a later 11.0 SDK.

```bash
./check.sh                  # builds every project and probe, runs the console app and the tests
dotnet run --project src/Books
dotnet test tests/Books.Tests
```

If the .NET 11 SDK is not your default `dotnet`, point the script at it:
`DOTNET=/path/to/dotnet11/dotnet ./check.sh`. On Windows, run `check.sh` from Git Bash.

## Layout

- `src/Books`: the union, the service, the Result class, the call sites and the minimal API endpoints.
- `probes/MissingCase`: a switch with one arm missing. Built on its own to capture CS8509.
- `probes/SwitchStatement`: the same missing arm in a switch statement. Builds with no warning.
- `probes/SameTypeTwice`: a generic `union Result<T, TError>(T, TError)`. Fails with CS0457 for `Result<string, string>`.
- `tests/Books.Tests`: shape, boxing, default, JSON, closed hierarchy and endpoint tests (xUnit).

## Tool versions

- .NET SDK 11.0.100-rc.1.26425.128, runtime 11.0.0-rc.1.26425.128, C# 15 (default language version)
- xUnit 2.9.3, xunit.runner.visualstudio 3.1.5, Microsoft.NET.Test.Sdk 18.9.0
- Run on Windows 11, x64

## Edge cases the post leaves out

- **Switch statements aren't checked.** Only switch expressions get CS8509 for a missing case.
- **A generic Result union breaks when both sides are the same type.** `Result<string, string>` has two
  constructors taking `string`, and converting a string is `error CS0457: Ambiguous user defined conversions`.
  Named case types avoid it.
- **Nullable case values do warn.** Converting a `Book?` that may be null into the union gives CS8604
  ("Possible null reference argument for parameter 'value'"). Only `default` gets through silently.
- **Boxing size.** 1000 `int` conversions allocated 24,000 bytes on x64 (one 24-byte box each). The test only
  asserts at least 16 bytes per conversion, and zero for a record case.
- **Classifier on the options.** Adding `JsonUnionTypeStructuralClassifier` to
  `JsonSerializerOptions.TypeClassifiers` instead of the attribute gives the same `NotSupportedException`.
- **Equality and ToString.** Two unions holding equal records compare equal with `Equals` (struct equality over the
  one field). `ToString()` returns the type name only, not the case.
- **Closed hierarchies** (`closed` modifier) also compile with no `LangVersion` on RC1.

## License

MIT. See [LICENSE](LICENSE).
