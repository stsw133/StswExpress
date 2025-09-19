using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Moq;
using Moq.Protected;

namespace StswExpress.Commons.Tests.Utils.Http;
public class StswHttpClientTests
{
    private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, object? responseContent, string? mediaType = "application/json")
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken ct) =>
            {
                var resp = new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = responseContent != null
                        ? new StringContent(JsonSerializer.Serialize(responseContent), null, mediaType)
                        : new StringContent(string.Empty)
                };
                return resp;
            });

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
    }

    public class DummyDto
    {
        public string? Name { get; set; }
    }

    [Fact]
    public async Task GetAsync_ReturnsDeserializedObject_WhenResponseIsValid()
    {
        var expected = new DummyDto { Name = "Alice" };
        var client = new StswExpress.Commons.StswHttpClient(CreateMockHttpClient(HttpStatusCode.OK, expected));

        var result = await client.GetAsync<DummyDto>("users/1");

        Assert.NotNull(result);
        Assert.Equal("Alice", result.Name);
    }

    [Fact]
    public async Task GetAsync_ThrowsInvalidDataException_WhenResponseIsEmpty()
    {
        var client = new StswExpress.Commons.StswHttpClient(CreateMockHttpClient(HttpStatusCode.OK, null));

        await Assert.ThrowsAsync<InvalidDataException>(() => client.GetAsync<DummyDto>("users/1"));
    }

    [Fact]
    public async Task GetAsyncList_ReturnsDeserializedList_WhenResponseIsValid()
    {
        var expected = new[] { new DummyDto { Name = "Alice" }, new DummyDto { Name = "Bob" } };
        var client = new StswExpress.Commons.StswHttpClient(CreateMockHttpClient(HttpStatusCode.OK, expected));

        var result = await client.GetAsyncList<DummyDto>("users");

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal("Bob", result[1].Name);
    }

    [Fact]
    public async Task GetAsyncList_ThrowsInvalidDataException_WhenResponseIsEmpty()
    {
        var client = new StswExpress.Commons.StswHttpClient(CreateMockHttpClient(HttpStatusCode.OK, null));

        await Assert.ThrowsAsync<InvalidDataException>(() => client.GetAsyncList<DummyDto>("users"));
    }

    [Fact]
    public async Task PostAsync_ReturnsDeserializedObject_WhenResponseIsValid()
    {
        var expected = new DummyDto { Name = "Alice" };
        var client = new StswExpress.Commons.StswHttpClient(CreateMockHttpClient(HttpStatusCode.OK, expected));

        var result = await client.PostAsync<DummyDto, DummyDto>("users", new DummyDto { Name = "Alice" });

        Assert.NotNull(result);
        Assert.Equal("Alice", result.Name);
    }

    [Fact]
    public async Task PostAsync_ThrowsInvalidDataException_WhenResponseIsEmpty()
    {
        var client = new StswExpress.Commons.StswHttpClient(CreateMockHttpClient(HttpStatusCode.OK, null));

        await Assert.ThrowsAsync<InvalidDataException>(() => client.PostAsync<DummyDto, DummyDto>("users", new DummyDto { Name = "Alice" }));
    }

    [Fact]
    public async Task PostAsync_ThrowsHttpRequestException_WhenResponseIsError()
    {
        var error = new StswExpress.Commons.StswHttpErrorResponse { Title = "Bad Request", Detail = "Invalid data" };
        var client = new StswExpress.Commons.StswHttpClient(CreateMockHttpClient(HttpStatusCode.BadRequest, error));

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => client.PostAsync<DummyDto, DummyDto>("users", new DummyDto { Name = "Alice" }));
        Assert.Contains("400", ex.Message);
        Assert.Contains("Invalid data", ex.Message);
    }

    [Fact]
    public async Task PostAsyncList_ReturnsDeserializedList_WhenResponseIsValid()
    {
        var expected = new[] { new DummyDto { Name = "Alice" }, new DummyDto { Name = "Bob" } };
        var client = new StswExpress.Commons.StswHttpClient(CreateMockHttpClient(HttpStatusCode.OK, expected));

        var result = await client.PostAsyncList<DummyDto, DummyDto>("users", new DummyDto { Name = "Alice" });

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal("Bob", result[1].Name);
    }

    [Fact]
    public async Task PostAsyncList_ThrowsInvalidDataException_WhenResponseIsEmpty()
    {
        var client = new StswExpress.Commons.StswHttpClient(CreateMockHttpClient(HttpStatusCode.OK, null));

        await Assert.ThrowsAsync<InvalidDataException>(() => client.PostAsyncList<DummyDto, DummyDto>("users", new DummyDto { Name = "Alice" }));
    }

    [Fact]
    public async Task PostAsyncList_ThrowsHttpRequestException_WhenResponseIsError()
    {
        var error = new StswExpress.Commons.StswHttpErrorResponse { Title = "Bad Request", Detail = "Invalid data" };
        var client = new StswExpress.Commons.StswHttpClient(CreateMockHttpClient(HttpStatusCode.BadRequest, error));

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => client.PostAsyncList<DummyDto, DummyDto>("users", new DummyDto { Name = "Alice" }));
        Assert.Contains("400", ex.Message);
        Assert.Contains("Invalid data", ex.Message);
    }

    [Fact]
    public void CreateBasicAuthHeader_ReturnsCorrectHeader()
    {
        var header = StswExpress.Commons.StswHttpClient.CreateBasicAuthHeader("user", "pass");
        Assert.Equal("Basic", header.Scheme);
        var expected = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("user:pass"));
        Assert.Equal(expected, header.Parameter);
    }

    [Fact]
    public void BuildUrl_AppendsQueryParametersCorrectly()
    {
        var type = typeof(StswExpress.Commons.StswHttpClient);
        var method = type.GetMethod("BuildUrl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var query = new { a = "1", b = "2" };
        var url = (string)method.Invoke(null, new object[] { "endpoint", query });
        Assert.Contains("endpoint?", url);
        Assert.Contains("a=1", url);
        Assert.Contains("b=2", url);
    }
}
