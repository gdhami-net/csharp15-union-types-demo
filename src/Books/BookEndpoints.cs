namespace Books;

public static class BookEndpoints
{
    public static void Map(WebApplication app)
    {
        // Each case picks its own status code.
        app.MapGet("/books/{isbn}", (string isbn, BookService books) => books.Find(isbn) switch
        {
            Book book => Results.Ok(book),
            NotFound => Results.NotFound(),
            Invalid bad => Results.BadRequest(bad.Reason),
        });

        // The union returned as it is.
        app.MapGet("/raw/books/{isbn}", (string isbn, BookService books) => books.Find(isbn));
    }
}
