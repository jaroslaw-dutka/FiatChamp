using System.Text.Json.Nodes;

namespace FiatChamp.Extensions;

public static class JsonNodeExtensions
{
    public static Dictionary<string, string> Flatten(this JsonNode container, string key = "root", Dictionary<string, string>? result = null)
    {
        result ??= new Dictionary<string, string>();

        switch (container)
        {
            case JsonValue value:
                result.Add(key, value.ToString());
                break;
            case JsonArray array:
                for (var i = 0; i < array.Count; i++)
                    array[i]!.Flatten($"{key}_array_{i}", result);
                break;
            case JsonObject obj:
                foreach (var (objKey, objValue) in obj)
                    objValue!.Flatten($"{key}_{objKey}", result);
                break;
        }

        return result;
    }
}