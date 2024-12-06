namespace FiatChamp.Ha;

public abstract class HaEntity
{
    protected readonly SimpleMqttClient _mqttClient;
    protected readonly string _name;
    protected readonly HaDevice _haDevice;
    protected readonly string _id;

    public string Name => _name;

    protected HaEntity(SimpleMqttClient mqttClient, string name, HaDevice haDevice)
    {
        _mqttClient = mqttClient;
        _name = name;
        _haDevice = haDevice;
        _id = $"{haDevice.Identifier}_{name}";
    }

    public abstract Task PublishState();
    public abstract Task Announce();
}