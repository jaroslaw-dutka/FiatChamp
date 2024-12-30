using CoordinateSharp;
using FiatChamp.Ha.Model;

namespace FiatChamp.Ha;

public static class HaRestApiExtensions
{
    public static async Task<HaUnitSystem> GetUnitSystem(this IHaRestApi api)
    {
        var config = await api.GetConfig();
        return config.UnitSystem;
    }

    public static async Task<IReadOnlyList<HaRestApiZone>> GetZones(this IHaRestApi api)
    {
        var states = await api.GetStates();
        return states
            .Where(state => state.EntityId.StartsWith("zone."))
            .Select(state => state.AttrTo<HaRestApiZone>())
            .ToArray();
    }

    public static async Task<IReadOnlyList<HaRestApiZone>> GetZonesAscending(this IHaRestApi api, Coordinate inside)
    {
        var zones = await api.GetZones();
        return zones
            .Where(zone => zone.Coordinate.Get_Distance_From_Coordinate(inside).Meters <= zone.Radius)
            .OrderBy(zone => zone.Coordinate.Get_Distance_From_Coordinate(inside).Meters)
            .ToArray();
    }
}