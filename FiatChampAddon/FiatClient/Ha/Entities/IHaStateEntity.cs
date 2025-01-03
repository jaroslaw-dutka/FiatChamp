namespace FiatChamp.Ha.Entities;

public interface IHaStateEntity : IHaSetEntity
{
    string State { get; }
}