using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public class HaButton : HaSet<HaButton>, IHaSetEntity
{
    public HaButton(HaDevice device, string name, Func<HaButton, string, Task> setAction) : base(device, "button", name, setAction)
    {
    }
}