using FiatChamp.Mqtt;

namespace FiatChamp.Ha;

public abstract class HaEntity
{
    protected readonly IMqttClient _mqttClient;
    protected readonly string _name;
    protected readonly HaDevice _haDevice;
    protected readonly string _id;

    public string Name => _name;

    protected HaEntity(IMqttClient mqttClient, string name, HaDevice haDevice)
    {
        _mqttClient = mqttClient;
        _name = name;
        _haDevice = haDevice;
        _id = $"{haDevice.Identifiers}_{name}";
    }

    public abstract Task PublishState();
    public abstract Task Announce();
}