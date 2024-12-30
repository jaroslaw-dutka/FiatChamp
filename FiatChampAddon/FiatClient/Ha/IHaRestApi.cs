using FiatChamp.Ha.Model;

namespace FiatChamp.Ha;

public interface IHaRestApi
{
    Task<HaConfig> GetConfig();
    Task<IReadOnlyList<HaRestApiEntityState>> GetStates();
}