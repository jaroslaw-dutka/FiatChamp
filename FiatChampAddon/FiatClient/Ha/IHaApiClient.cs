using FiatChamp.Ha.Model;

namespace FiatChamp.Ha;

public interface IHaApiClient
{
    Task<HaConfig> GetConfig();
    Task<IReadOnlyList<HaRestApiEntityState>> GetStates();
}