using FiatChamp.Ha.Model;

namespace FiatChamp.Ha;

public interface IHaApiClient
{
    Task<HaConfig> GetConfigAsync();
    Task<IReadOnlyList<HaRestApiEntityState>> GetStatesAsync();
}