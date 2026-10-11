#!/usr/bin/env bash
# Asserts every claim the blog post makes about C# 15 unions on .NET 11.
# DOTNET can point at a .NET 11 SDK that is not on PATH; it defaults to `dotnet`.
set -u
cd "$(dirname "$0")"
DOTNET="${DOTNET:-dotnet}"
fails=0

pass() { echo "PASS  $1"; [ -n "${2:-}" ] && echo "      $2"; return 0; }
fail() { echo "FAIL  $1"; [ -n "${2:-}" ] && echo "      $2"; fails=$((fails + 1)); }

# Build output, one line per diagnostic, with the absolute paths cut off.
diagnostics() {
  "$DOTNET" build "$1" --no-incremental -nologo "${@:2}" 2>&1 \
    | grep -E ": (warning|error) [A-Z]+[0-9]+" \
    | sed -E 's/ \[[^]]*\]$//; s#^.*[\\/](probes|src)[\\/]#\1/#; s#\\#/#g' \
    | sort -u
}

sdk=$("$DOTNET" --version)
case "$sdk" in
  11.*) pass "SDK is .NET 11" "$sdk" ;;
  *) fail "SDK is .NET 11" "got $sdk; set DOTNET to a .NET 11 SDK"; echo "STOPPED"; exit 1 ;;
esac

if grep -rq --include="*.csproj" --include="*.props" "<LangVersion" src probes tests Directory.Build.props; then
  fail "no project sets LangVersion"
else
  pass "no project sets LangVersion: union compiles with the net11.0 default"
fi

out=$(diagnostics src/Books)
if [ -z "$out" ]; then pass "src/Books builds with zero warnings: every switch over the union is complete"; else fail "src/Books builds with zero warnings" "$out"; fi

out=$(diagnostics tests/Books.Tests)
if [ -z "$out" ]; then pass "tests build with zero warnings, including the call that passes default"; else fail "tests build with zero warnings" "$out"; fi

out=$(diagnostics probes/MissingCase)
expected="probes/MissingCase/Describe.cs(8,66): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern 'Books.Invalid' is not covered."
if [ "$out" = "$expected" ]; then pass "a switch missing the Invalid case gives exactly one CS8509" "$out"; else fail "a switch missing the Invalid case gives exactly one CS8509" "$out"; fi

out=$(diagnostics probes/MissingCase -p:WarningsAsErrors=CS8509)
if echo "$out" | grep -q "error CS8509"; then pass "with WarningsAsErrors=CS8509 the same switch breaks the build" "$out"; else fail "with WarningsAsErrors=CS8509 the same switch breaks the build" "$out"; fi

out=$(diagnostics probes/SwitchStatement)
if [ -z "$out" ]; then pass "a switch statement missing the same case gets no warning"; else fail "a switch statement missing the same case gets no warning" "$out"; fi

out=$(diagnostics probes/SameTypeTwice)
if echo "$out" | grep -q "error CS0457" && [ "$(echo "$out" | wc -l)" -eq 1 ]; then pass "Result<string, string> as a union is an ambiguous conversion" "$out"; else fail "Result<string, string> as a union is an ambiguous conversion" "$out"; fi

out=$(diagnostics probes/NullableCase)
if echo "$out" | grep -q "warning CS8604" && [ "$(echo "$out" | wc -l)" -eq 1 ]; then pass "a maybe-null Book converted into the union warns CS8604" "$out"; else fail "a maybe-null Book converted into the union warns CS8604" "$out"; fi

echo "----- dotnet run --project src/Books"
"$DOTNET" run --project src/Books --no-build
echo "-----"

if "$DOTNET" test tests/Books.Tests --no-build -nologo 2>&1 | tail -n 3 | grep -q "Passed!"; then
  pass "dotnet test: shape, boxing, default, JSON, closed hierarchy and endpoint tests"
else
  fail "dotnet test" "run '$DOTNET test tests/Books.Tests' to see which one"
fi

if [ "$fails" -eq 0 ]; then echo "ALL CHECKS PASSED"; else echo "$fails CHECK(S) FAILED"; exit 1; fi
