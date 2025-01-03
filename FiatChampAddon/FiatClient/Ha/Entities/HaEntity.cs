using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public abstract class HaEntity: IHaEntity
{
    public HaDevice Device { get; }
    public string Type { get; }
    public string Name { get; }
    public string Id { get; }
    public string? Icon { get; set; }
    public string? UnitOfMeasurement { get; set; }
    public string? DeviceClass { get; set; }

    protected HaEntity(HaDevice device, string type, string name)
    {
        Device = device;
        Type = type;
        Name = name;
        Id = $"{device.Identifiers.First()}_{name}";
    }
}