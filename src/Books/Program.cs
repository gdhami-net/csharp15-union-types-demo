using System.Text.Json;
using Books;

if (args is ["--serve"])
{
    var builder = WebApplication.CreateBuilder();
    builder.Services.AddSingleton<BookService>();
    var app = builder.Build();
    BookEndpoints.Map(app);
    app.Run();
    return;
}

var books = new BookService();
string[] isbns = ["9781234567897", "9789999999999", "12-34"];

foreach (var isbn in isbns)
{
    LookupResult result = books.Find(isbn);
    Console.WriteLine($"{isbn,-14} {result.Value?.GetType().Name,-9} {CallSites.Describe(books, isbn),-22} {JsonSerializer.Serialize(result, JsonSerializerOptions.Web)}");
}
