using FiatChamp.Fiat.Entities;

namespace FiatChamp.Fiat;

public interface IFiatClient
{
    Task LoginAndKeepSessionAliveAsync(CancellationToken cancellationToken);
    Task SendCommandAsync(string vin, string command, string pin, string action);
    Task<List<VehicleInfo>> FetchAsync();
    Task ConnectToMqtt();
}