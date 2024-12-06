using CoordinateSharp;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json.Linq;

namespace FiatChamp.Ha;

public class HaRestApi
{
    private readonly string _url;
    private readonly string _token;

    public HaRestApi(string url, string token)
    {
        _url = url.AppendPathSegment("api");
        _token = token;
    }

    public HaRestApi(string token)
    {
        _token = token;
    }

    private async Task<JObject> GetConfig()
    {
        return await _url
            .WithOAuthBearerToken(_token)
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
        var result = await _url
            .WithOAuthBearerToken(_token)
            .AppendPathSegment("states")
            .GetJsonAsync<HaRestApiEntityState[]>();

        return result.ToArray();
    }
}