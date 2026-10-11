namespace Books;

public record Book(string Isbn, string Title);
public record NotFound(string Isbn);
public record Invalid(string Reason);

public union LookupResult(Book, NotFound, Invalid);
