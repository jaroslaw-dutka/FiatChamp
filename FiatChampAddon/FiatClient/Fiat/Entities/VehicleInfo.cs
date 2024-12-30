using FiatChamp.Fiat.Model;
using System.Text.Json.Nodes;

namespace FiatChamp.Fiat.Entities
{
    public class VehicleInfo
    {
        public Vehicle Vehicle { get; set; }
        public JsonNode Details { get; set; }
        public VehicleLocation Location { get; set; }
    }
}