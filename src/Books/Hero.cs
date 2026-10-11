namespace Books.Hero;

// The C# half of the post's hero image, kept here so it compiles.
public record Ok(Book Book);
public record NotFound;

public union Result(Ok, NotFound);

public static class HeroSwitch
{
    public static string Text(Result result) => result switch
    {
        Ok ok => ok.Book.Title,
        NotFound => "No such book",
    };
}
