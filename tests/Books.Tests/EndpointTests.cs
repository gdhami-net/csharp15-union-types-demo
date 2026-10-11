using System.Net;
using Books;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Books.Tests;

public class EndpointTests : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _http = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddSingleton<BookService>();
        builder.Logging.ClearProviders();
        _app = builder.Build();
        BookEndpoints.Map(_app);
        await _app.StartAsync();
        var address = _app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
        _http = new HttpClient { BaseAddress = new Uri(address) };
    }

    public async Task DisposeAsync()
    {
        _http.Dispose();
        await _app.DisposeAsync();
    }

    [Theory]
    [InlineData("9781234567897", HttpStatusCode.OK)]
    [InlineData("9789999999999", HttpStatusCode.NotFound)]
    [InlineData("12-34", HttpStatusCode.BadRequest)]
    public async Task A_switch_in_the_endpoint_picks_the_status_code(string isbn, HttpStatusCode expected)
    {
        var response = await _http.GetAsync($"/books/{isbn}");
        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData("9781234567897", """{"isbn":"9781234567897","title":"A Book That Exists"}""")]
    [InlineData("9789999999999", """{"isbn":"9789999999999"}""")]
    [InlineData("12-34", """{"reason":"An ISBN has 13 digits."}""")]
    public async Task Returning_the_union_itself_sends_200_for_every_case(string isbn, string expectedBody)
    {
        var response = await _http.GetAsync($"/raw/books/{isbn}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedBody, await response.Content.ReadAsStringAsync());
    }
}
