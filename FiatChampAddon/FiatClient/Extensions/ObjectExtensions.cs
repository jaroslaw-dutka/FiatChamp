using System.Text.Json;

namespace FiatChamp.Extensions;

public static class ObjectExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public static string Dump(this object? result)
    {
        try
        {
            if (result is string str)
                return str;

            return JsonSerializer.Serialize(result, SerializerOptions);

        }
        catch (Exception)
        {
            return result?.GetType().ToString() ?? "null";
        }
    }
}