using System.Net.Http.Headers;
using System.Net.Http.Json;
using FiatChamp.Ha.Model;
using Microsoft.Extensions.Options;

namespace FiatChamp.Ha;

public class HaApiClient : IHaApiClient
{
    private readonly HaApiSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;

    public HaApiClient(IOptions<HaApiSettings> config, IHttpClientFactory httpClientFactory)
    {
        _settings = config.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HaConfig> GetConfig()
    {
        using var client = GetHttpClient();
        return await client.GetFromJsonAsync<HaConfig>("config");
    }

    public async Task<IReadOnlyList<HaRestApiEntityState>> GetStates()
    {
        using var client = GetHttpClient();
        return await client.GetFromJsonAsync<HaRestApiEntityState[]>("states");
    }

    private HttpClient GetHttpClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_settings.Url + "/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.Token);
        return client;
    }
}