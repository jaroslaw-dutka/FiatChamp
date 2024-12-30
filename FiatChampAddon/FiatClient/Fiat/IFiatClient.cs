using FiatChamp.Fiat.Entities;

namespace FiatChamp.Fiat;

public interface IFiatClient
{
    Task LoginAndKeepSessionAlive();
    Task SendCommand(string vin, string command, string pin, string action);
    Task<List<VehicleInfo>> Fetch();
}