using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public class HaSwitch : HaSet<HaSwitch>, IHaStateEntity
{
    public const string OnState = "ON";
    public const string OffState = "OFF";

    public bool IsOn { get; private set; }
    public string State => IsOn ? OnState : OffState;

    public HaSwitch(HaDevice device, string name, Func<HaSwitch, string, Task> setAction) : base(device, "switch", name, setAction)
    {
    }

    public override async Task OnSetAsync(string state)
    {
        IsOn = state == OnState;
        await base.OnSetAsync(state);
    }
}