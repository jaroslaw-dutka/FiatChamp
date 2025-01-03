using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public abstract class HaActionEntity<TEntity> : HaEntity where TEntity: HaActionEntity<TEntity>
{
    protected readonly Func<TEntity, Task> Action;

    protected HaActionEntity(string type, IHaMqttClient mqttClient, HaDevice device, string name, Func<TEntity, Task> action) : base(type, mqttClient, device, name)
    {
        Action = action;
    }
}