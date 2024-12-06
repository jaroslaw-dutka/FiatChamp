using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FiatChamp.Extensions;

public static class ObjectExtensions
{
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
                    var json = JObject.Parse(str);
                    return json.ToString(Formatting.Indented);
                }
                catch (Exception e)
                {
                    return str;
                }
            }

            return JsonConvert.SerializeObject(result, Formatting.Indented);

        }
        catch (Exception)
        {
            return o?.GetType().ToString() ?? "null";
        }
    }
}