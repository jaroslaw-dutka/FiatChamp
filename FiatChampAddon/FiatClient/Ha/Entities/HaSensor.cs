using FiatChamp.Ha.Model;
using System.Text.Json;

namespace FiatChamp.Ha.Entities;

public class HaSensor : HaEntity, IHaStateEntity
{
    private readonly Func<HaSensor, string, Task>? _setAction;

    public string State { get; set; }

    public HaSensor(HaDevice device, string name, Func<HaSensor, string, Task>? setAction = null) : base(device, "sensor", name)
    {
        _setAction = setAction;
        Icon = "mdi:eye";
    }

    public virtual async Task OnSetAsync(string state)
    {
        if (_setAction is not null)
            await _setAction.Invoke(this, state);
    }
}

public class HaSensor<T> : HaSensor, IHaAttributesEntity
{
    public string SerializedAttributes => JsonSerializer.Serialize(Attributes);
    public T Attributes { get; set; }

    public HaSensor(HaDevice device, string name) : base(device, name)
    {
    }
}