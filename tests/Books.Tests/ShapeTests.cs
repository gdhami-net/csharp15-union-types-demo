using System.Reflection;
using System.Runtime.CompilerServices;
using Books;

namespace Books.Tests;

public union Count(int, string);

public class ShapeTests
{
    [Fact]
    public void A_union_is_a_struct_that_holds_one_object()
    {
        var type = typeof(LookupResult);
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        Assert.True(type.IsValueType);
        Assert.NotNull(type.GetCustomAttribute<UnionAttribute>());
        Assert.Contains(typeof(IUnion), type.GetInterfaces());
        Assert.Equal(typeof(object), Assert.Single(fields).FieldType);
        Assert.Equal(3, type.GetConstructors().Length);
    }

    [Fact]
    public void Each_case_converts_implicitly_and_Value_holds_it()
    {
        LookupResult found = new Book("9781234567897", "A Book That Exists");
        LookupResult missing = new NotFound("9789999999999");

        Assert.IsType<Book>(found.Value);
        Assert.IsType<NotFound>(missing.Value);
    }

    [Fact]
    public void A_value_type_case_is_boxed_and_a_reference_case_is_not()
    {
        var book = new Book("9781234567897", "A Book That Exists");
        ConvertInt(1);
        ConvertBook(book);

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) ConvertInt(i);
        long ints = GC.GetAllocatedBytesForCurrentThread() - before;

        before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) ConvertBook(book);
        long books = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.True(ints >= 1000 * 16, $"1000 int conversions allocated {ints} bytes");
        Assert.Equal(0, books);
    }

    [Fact]
    public void Equals_compares_the_case_and_ToString_prints_only_the_type_name()
    {
        LookupResult a = new NotFound("9789999999999");
        LookupResult b = new NotFound("9789999999999");

        Assert.True(a.Equals(b));
        Assert.Equal("Books.LookupResult", a.ToString());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static object? ConvertInt(int value) { Count c = value; return c.Value; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static object? ConvertBook(Book value) { LookupResult r = value; return r.Value; }
}
