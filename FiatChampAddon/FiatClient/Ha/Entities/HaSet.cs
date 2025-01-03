using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public abstract class HaSet<TEntity> : HaEntity where TEntity: HaSet<TEntity>, IHaSetEntity
{
    private readonly Func<TEntity, string, Task> _setAction;

    protected HaSet(HaDevice device, string type, string name, Func<TEntity, string, Task> setAction) : base(device, type, name)
    {
        _setAction = setAction;
    }

    public virtual async Task OnSetAsync(string state) => 
        await _setAction(this as TEntity, state);
}