using CoordinateSharp;
using FiatChamp.Ha.Model;

namespace FiatChamp.Ha;

public interface IHaRestApi
{
    Task<string> GetTimeZone();
    Task<HaRestApiUnitSystem> GetUnitSystem();
    Task<IReadOnlyList<HaRestApiZone>> GetZones();
    Task<IReadOnlyList<HaRestApiZone>> GetZonesAscending(Coordinate inside);
    Task<IReadOnlyList<HaRestApiEntityState>> GetStates();
}