using Newtonsoft.Json.Linq;

namespace FiatChamp.Extensions;

public static class JTokenExtensions
{
    public static Dictionary<string, string> Compact(this JToken container, string key = "root", Dictionary<string, string>? result = null)
    {
        if (result == null)
        {
            result = new Dictionary<string, string>();
        }

        if (container is JValue value)
        {
            result.Add(key, value.Value?.ToString() ?? "null");
        }
        else if (container is JArray array)
        {
            for (int i = 0; i < array.Count(); i++)
            {
                var token = array[i];
                token.Compact($"{key}_array_{i}", result);
            }
        }
        else if (container is JObject obj)
        {
            foreach (var kv in obj)
            {
                kv.Value.Compact($"{key}_{kv.Key}", result);
            }
        }

        return result;
    }
}