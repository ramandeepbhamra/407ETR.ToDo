using System.Net.Http.Headers;

namespace ETR.ToDo.Tests.Api.BDD.Infrastructure;

/// <summary>
/// Stores context data shared across BDD step definitions within a scenario.
/// </summary>
public class ScenarioContextHelper
{
    private readonly Dictionary<string, object> _data = new();

    public HttpClient HttpClient { get; set; } = null!;
    public HttpResponseMessage? LastResponse { get; set; }
    public string? LastResponseContent { get; set; }

    public void Set<T>(string key, T value) where T : notnull
    {
        _data[key] = value;
    }

    public T Get<T>(string key)
    {
        if (_data.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        throw new KeyNotFoundException($"Key '{key}' not found in scenario context.");
    }

    public bool TryGet<T>(string key, out T? value)
    {
        if (_data.TryGetValue(key, out var obj))
        {
            value = (T)obj;
            return true;
        }
        value = default;
        return false;
    }

    public void SetAuthToken(string token)
    {
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Set("AuthToken", token);
    }

    public void ClearAuthToken()
    {
        if (HttpClient != null)
        {
            HttpClient.DefaultRequestHeaders.Authorization = null;
        }
        _data.Remove("AuthToken");
    }
}
