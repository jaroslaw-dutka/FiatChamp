using System.Text.Json;
using System.Text.Json.Nodes;

namespace FiatChamp.Extensions;

public static class ObjectExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public static string Dump(this object? o)
    {

        try
        {
            var result = o;
            if (o is Task task)
            {
                task.Wait();
                result = ((dynamic)task).Result;
            }

            if (result is string str)
            {
                try
                {
                    var json = JsonNode.Parse(str);
                    return json.ToString();
                }
                catch (Exception e)
                {
                    return str;
                }
            }

            return JsonSerializer.Serialize(result, SerializerOptions);

        }
        catch (Exception)
        {
            return o?.GetType().ToString() ?? "null";
        }
    }
}