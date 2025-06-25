using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StswExpress.Commons;

/// <summary>
/// Model for interacting with a Web API, providing methods for GET and POST requests.
/// </summary>
/// <param name="httpClient">Instance of <see cref="HttpClient"/> to use for making requests.</param>
public class StswHttpClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Performs a GET request to the specified path with optional query parameters.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="path">The API endpoint path.</param>
    /// <param name="query">The query parameters to include in the request, if any.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The response data of type T returned by the API.</returns>
    /// <exception cref="InvalidDataException">Thrown when the response is empty.</exception>
    public async Task<T> GetAsync<T>(
        string path,
        object? query = null,
        CancellationToken ct = default)
    {
        var url = BuildUrl(path, query);
        var result = await _httpClient.GetFromJsonAsync<T>(url, _jsonOptions, ct);
        return result ?? throw new InvalidDataException($"Empty response for GET {url}");
    }

    /// <summary>
    /// Performs a GET request to the specified path with optional query parameters.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="path">The API endpoint path.</param>
    /// <param name="query">The query parameters to include in the request, if any.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The list of items of type T returned by the API.</returns>
    /// <exception cref="InvalidDataException">Thrown when the response is empty.</exception>
    public async Task<IReadOnlyList<T>> GetAsyncList<T>(
        string path,
        object? query = null,
        CancellationToken ct = default)
    {
        var url = BuildUrl(path, query);
        var result = await _httpClient.GetFromJsonAsync<IReadOnlyList<T>>(url, _jsonOptions, ct);
        return result ?? throw new InvalidDataException($"Empty response for GET {url}");
    }

    /// <summary>
    /// Performs a POST request to the specified path with the provided body.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request body.</typeparam>
    /// <typeparam name="TResponse">The type of the response data.</typeparam>
    /// <param name="path">The API endpoint path.</param>
    /// <param name="body">The request body to send in the POST request.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The response data of type TResponse returned by the API.</returns>
    /// <exception cref="InvalidDataException">Thrown when the response is empty.</exception>
    /// <exception cref="HttpRequestException">Thrown when the response indicates an error.</exception>
    public async Task<TResponse> PostAsync<TRequest, TResponse>(
        string path,
        TRequest body,
        CancellationToken ct = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(path, body, _jsonOptions, ct);

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, ct);
            return data ?? throw new InvalidDataException($"Empty response for POST {path}");
        }

        var error = await TryReadErrorAsync(response, ct);
        throw new HttpRequestException($"{(int)response.StatusCode} {response.ReasonPhrase}: {error}");
    }

    /// <summary>
    /// Performs a POST request to the specified path with the provided body.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request body.</typeparam>
    /// <typeparam name="TResponse">The type of the response data.</typeparam>
    /// <param name="path">The API endpoint path.</param>
    /// <param name="body">The request body to send in the POST request.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The list of items of type TResponse returned by the API.</returns>
    /// <exception cref="InvalidDataException">Thrown when the response is empty.</exception>
    /// <exception cref="HttpRequestException">Thrown when the response indicates an error.</exception>
    public async Task<IReadOnlyList<TResponse>> PostAsyncList<TRequest, TResponse>(
        string path,
        TRequest body,
        CancellationToken ct = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(path, body, _jsonOptions, ct);

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<IReadOnlyList<TResponse>>(_jsonOptions, ct);
            return data ?? throw new InvalidDataException($"Empty response for POST {path}");
        }

        var error = await TryReadErrorAsync(response, ct);
        throw new HttpRequestException($"{(int)response.StatusCode} {response.ReasonPhrase}: {error}");
    }

    /// <summary>
    /// Builds a URL by appending query parameters to the specified path.
    /// </summary>
    /// <param name="path">The base path of the URL.</param>
    /// <param name="query">The query parameters to append, if any.</param>
    /// <returns>The complete URL with query parameters.</returns>
    private static string BuildUrl(string path, object? query)
    {
        if (query is null) return path;

        var props = from p in query.GetType().GetProperties()
                    let v = p.GetValue(query, null)
                    where v is not null
                    select $"{WebUtility.UrlEncode(p.Name)}={WebUtility.UrlEncode(v.ToString())}";

        var qs = string.Join('&', props);
        return string.IsNullOrWhiteSpace(qs) ? path : $"{path}?{qs}";
    }

    /// <summary>
    /// Attempts to read the error message from the response content.
    /// </summary>
    /// <param name="resp">The HTTP response message.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns></returns>
    private static async Task<string> TryReadErrorAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        try
        {
            var problem = await resp.Content.ReadFromJsonAsync<StswHttpErrorResponse>(cancellationToken: ct)
                ?? new StswHttpErrorResponse { Detail = await resp.Content.ReadAsStringAsync(ct) };
            return problem.Detail ?? problem.Title ?? "Unknown error";
        }
        catch
        {
            return await resp.Content.ReadAsStringAsync(ct);
        }
    }
}

/* usage:

var stswClient = new StswHttpClient(new HttpClient { BaseAddress = new Uri("https://api.example.com/") });

var user = await stswClient.GetAsync<UserDto>("users/1");
var users = await stswClient.GetAsyncList<UserDto>("users", new { role = "admin" });
var newUser = await stswClient.PostAsync<CreateUserRequest, UserDto>("users", new CreateUserRequest { Name = "Alice" });
var foundUsers = await stswClient.PostAsyncList<object, UserDto>("users/search", new { query = "Alice" });

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var user = await stswClient.GetAsync<UserDto>("users/1", ct: cts.Token);

*/
