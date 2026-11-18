using System.Net;
using System.Net.Http.Json;
using Simple_CRUD.WebApi.Models;

namespace Simple_CRUD.Tests.Integration.endpoints;

public class MovieEndpointTests : IClassFixture<MovieEndpointTests.Factory>
{
    private readonly HttpClient _client;

    public MovieEndpointTests(Factory factory)
    {
        _client = factory.CreateClient();
    }

    // Create a movie in the store with a valid name
    [Fact]
    public async Task Create_Movie_WithValidName_ReturnsCreated()
    {
        var movie = new { Name = "The Matrix Reloaded" };

        var response = await _client.PostAsJsonAsync("/movies", movie);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // Create a movie in the store with an existing name returns an error
    [Fact]
    public async Task Create_Movie_WithExistingName_ReturnsConflict()
    {
        // A movie with the name "The Matrix" already exists in the store.
        var movie = new { Name = "The Matrix" };

        var response = await _client.PostAsJsonAsync("/movies", movie);

        Assert.Throws<HttpRequestException>(response.EnsureSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
        
    //TODO: Create a movie with name validation rules

    // Factory class to inject seed data
    public class Factory : InMemoryStoreWebApplicationFactory<IList<MovieDto>, Program>
    {
        private static readonly List<MovieDto> _seedData = new List<MovieDto>
        {
            new MovieDto("The Matrix")
        };

        public Factory() : base(_seedData)
        {
        }
    }
}