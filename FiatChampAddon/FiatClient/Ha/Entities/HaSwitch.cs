using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public class HaSwitch : HaEntity, IHaStateEntity
{
    private readonly Func<HaSwitch, string, Task> _setAction;

    public const string OnState = "ON";
    public const string OffState = "OFF";

    public bool IsOn { get; private set; }
    public string State => IsOn ? OnState : OffState;

    public HaSwitch(HaDevice device, string name, Func<HaSwitch, string, Task> setAction) : base(device, "switch", name)
    {
        _setAction = setAction;
    }

    public async Task OnSetAsync(string state)
    {
        IsOn = state == OnState;
        await _setAction(this, state);
    }
}