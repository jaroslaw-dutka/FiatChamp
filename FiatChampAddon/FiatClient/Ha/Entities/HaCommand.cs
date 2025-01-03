using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public abstract class HaCommand<TEntity> : HaEntity where TEntity: HaCommand<TEntity>
{
    protected readonly Func<TEntity, Task> Action;

    protected HaCommand(string type, IHaMqttClient mqttClient, HaDevice device, string name, Func<TEntity, Task> action) : base(type, mqttClient, device, name)
    {
        Action = action;
    }

    protected override void BuildAnnouncement(HaAnnouncement announcement) => 
        announcement.CommandTopic = CommandTopic;
}