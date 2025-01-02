using System.Text.Json;
using CoordinateSharp;
using FiatChamp.Ha.Model;

namespace FiatChamp.Ha;

public static class HaRestApiExtensions
{
    public static async Task<HaUnitSystem> GetUnitSystem(this IHaApiClient client)
    {
        var config = await client.GetConfig();
        return config.UnitSystem;
    }

    public static async Task<IReadOnlyList<HaRestApiZone>> GetZones(this IHaApiClient client)
    {
        var states = await client.GetStates();
        return states
            .Where(state => state.EntityId.StartsWith("zone."))
            .Select(state => state.Attributes.Deserialize<HaRestApiZone>()!)
            .ToArray();
    }

    public static async Task<IReadOnlyList<HaRestApiZone>> GetZonesAscending(this IHaApiClient client, Coordinate inside)
    {
        var zones = await client.GetZones();
        return zones
            .Where(zone => zone.Coordinate.Get_Distance_From_Coordinate(inside).Meters <= zone.Radius)
            .OrderBy(zone => zone.Coordinate.Get_Distance_From_Coordinate(inside).Meters)
            .ToArray();
    }
}