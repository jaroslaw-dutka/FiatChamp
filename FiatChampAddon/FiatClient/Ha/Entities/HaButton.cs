using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public class HaButton : HaEntity, IHaSetEntity
{
    private readonly Func<HaButton, string, Task> _setAction;

    public HaButton(HaDevice device, string name, Func<HaButton, string, Task> setAction) : base(device, "button", name)
    {
        _setAction = setAction;
    }

    public virtual async Task OnSetAsync(string state) =>
        await _setAction(this, state);
}