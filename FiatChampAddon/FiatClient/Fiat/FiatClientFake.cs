using System.Text.Json;
using System.Text.Json.Nodes;
using FiatChamp.Fiat.Entities;
using FiatChamp.Fiat.Model;
using Flurl.Http.Configuration;

namespace FiatChamp.Fiat;

public class FiatClientFake : IFiatClient
{
    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
    }

    public async Task SendCommandAsync(string vin, string command, string pin, string action)
    {
        await Task.Delay(5000);
    }

    public Task<List<VehicleInfo>> GetVehiclesAsync()
    {
        var serializer = new DefaultJsonSerializer(JsonSerializerOptions.Default);
        return Task.FromResult(new List<VehicleInfo>
        {
            new()
            {
                Vehicle = serializer.Deserialize<Vehicle>(File.OpenRead("./Mocks/vehicles.json")),
                Location = serializer.Deserialize<VehicleLocation>(File.OpenRead("./Mocks/location.json")),
                Details = serializer.Deserialize<JsonObject>(File.OpenRead("./Mocks/details.json"))
            }
        });
    }
}