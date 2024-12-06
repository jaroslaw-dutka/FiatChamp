using CoordinateSharp;
using FiatChamp.Ha.Model;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace FiatChamp.Ha;

public class HaRestApi : IHaRestApi
{
    private readonly HaConfig _config;

    public HaRestApi(IOptions<HaConfig> config)
    {
        _config = config.Value;
    }

    private async Task<JObject> GetConfig()
    {
        return await _config.Url
            .WithOAuthBearerToken(_config.Token)
            .AppendPathSegment("config")
            .GetJsonAsync<JObject>();
    }

    public async Task<string> GetTimeZone()
    {
        var config = await GetConfig();

        return config["time_zone"].ToString();
    }

    public async Task<HaRestApiUnitSystem> GetUnitSystem()
    {
        var config = await GetConfig();

        return config["unit_system"].ToObject<HaRestApiUnitSystem>();
    }

    public async Task<IReadOnlyList<HaRestApiZone>> GetZones()
    {
        var states = await GetStates();
        var zones = states
            .Where(state => state.EntityId.StartsWith("zone."))
            .Select(state => state.AttrTo<HaRestApiZone>())
            .ToArray();

        return zones;
    }

    public async Task<IReadOnlyList<HaRestApiZone>> GetZonesAscending(Coordinate inside)
    {
        var zones = await GetZones();
        return zones
            .Where(zone => zone.Coordinate.Get_Distance_From_Coordinate(inside).Meters <= zone.Radius)
            .OrderBy(zone => zone.Coordinate.Get_Distance_From_Coordinate(inside).Meters)
            .ToArray();
    }

    public async Task<IReadOnlyList<HaRestApiEntityState>> GetStates()
    {
        var result = await _config.Url
            .WithOAuthBearerToken(_config.Token)
            .AppendPathSegment("states")
            .GetJsonAsync<HaRestApiEntityState[]>();

        return result.ToArray();
    }
}