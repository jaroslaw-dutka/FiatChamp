using FiatChamp.Ha.Model;
using System.Text.Json;

namespace FiatChamp.Ha.Entities;

public class HaSensor : HaEntity, IHaStateEntity
{
    public string State { get; set; }

    public HaSensor(HaDevice device, string name) : base(device, "sensor", name)
    {
        Icon = "mdi:eye";
    }

    public virtual Task OnSetAsync(string state) => 
        Task.CompletedTask;
}

public class HaSensor<T> : HaSensor, IHaAttributesEntity
{
    public string SerializedAttributes => JsonSerializer.Serialize(Attributes);
    public T Attributes { get; set; }

    public HaSensor(HaDevice device, string name) : base(device, name)
    {
    }
}