using System.Text.Json.Nodes;
using FiatChamp.Fiat.Entities;
using FiatChamp.Fiat.Model;

namespace FiatChamp.Fiat;

public interface IFiatApiClient
{
    Task<FiatBootstrapResponse> Bootstrap();
    Task<FiatLoginResponse> Login();
    Task<FiatTokenResponse> GetToken(string loginToken);
    Task<FcaIdentityResponse> GetIdentity(string idToken);
    Task<FcaPinAuthResponse> AuthenticatePin(FiatSession session, string pin);
    Task<FcaCommandResponse> SendCommand(FiatSession session, string pinAuth, string vin, string action, string command);
    Task<VehicleResponse> GetVehicles(FiatSession session);
    Task<JsonObject> GetVehicleDetails(FiatSession session, string vin);
    Task<VehicleLocation> GetVehicleLocation(FiatSession session, string vin);
    Task<NotificationsResponse> GetNotifications(FiatSession session);
}