using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public interface IHaEntity
{
    public HaDevice Device { get; }

    public string Id { get; }
    public string Name { get; }
    public string Type { get; }
    public string? UnitOfMeasurement { get; set; }
    public string? DeviceClass { get; set; }
    public string? Icon { get; set; }
}