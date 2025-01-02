using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public abstract class HaEntity
{
    protected readonly IHaMqttClient _mqttClient;
    protected readonly string _name;
    protected readonly HaDevice _haDevice;
    protected readonly string _id;

    public string Name => _name;

    protected HaEntity(IHaMqttClient mqttClient, string name, HaDevice haDevice)
    {
        _mqttClient = mqttClient;
        _name = name;
        _haDevice = haDevice;
        _id = $"{haDevice.Identifiers.First()}_{name}";
    }

    public abstract Task PublishStateAsync();
    public abstract Task AnnounceAsync();
}