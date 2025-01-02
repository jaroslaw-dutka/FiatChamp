using FiatChamp.Fiat.Entities;

namespace FiatChamp.Fiat;

public interface IFiatClient
{
    Task ConnectAsync(CancellationToken cancellationToken);
    Task<List<VehicleInfo>> FetchAsync();
    Task SendCommandAsync(string vin, string command, string pin, string action);
}