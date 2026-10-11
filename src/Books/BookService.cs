namespace Books;

public class BookService
{
    private readonly Dictionary<string, Book> _books = new()
    {
        ["9781234567897"] = new("9781234567897", "A Book That Exists"),
    };

    public LookupResult Find(string isbn)
    {
        if (isbn.Length != 13 || !isbn.All(char.IsAsciiDigit))
            return new Invalid("An ISBN has 13 digits.");

        return _books.TryGetValue(isbn, out var book) ? book : new NotFound(isbn);
    }

    // The same method, written against the hand-written Result class.
    public Result<Book, LookupError> FindOld(string isbn)
    {
        if (isbn.Length != 13 || !isbn.All(char.IsAsciiDigit))
            return Result<Book, LookupError>.Fail(new InvalidIsbn("An ISBN has 13 digits."));

        return _books.TryGetValue(isbn, out var book)
            ? Result<Book, LookupError>.Ok(book)
            : Result<Book, LookupError>.Fail(new BookNotFound(isbn));
    }
}
