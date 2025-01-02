using System.Text.Json;
using CoordinateSharp;
using FiatChamp.Ha.Model;

namespace FiatChamp.Extensions
{
    public static class HaZoneExtensions
    {
        public static IEnumerable<HaRestApiZone> GetZones(this IReadOnlyList<HaRestApiEntityState> states) => states
            .Where(state => state.EntityId.StartsWith("zone."))
            .Select(state => state.Attributes.Deserialize<HaRestApiZone>()!)
            .ToArray();

        public static IEnumerable<HaRestApiZone> OrderByDistance(this IEnumerable<HaRestApiZone> zones, Coordinate location) => zones
            .Where(zone => zone.Coordinate.Get_Distance_From_Coordinate(location).Meters <= zone.Radius)
            .OrderBy(zone => zone.Coordinate.Get_Distance_From_Coordinate(location).Meters)
            .ToArray();
    }
}
