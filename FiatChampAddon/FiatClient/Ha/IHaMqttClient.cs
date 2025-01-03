using FiatChamp.Ha.Entities;

namespace FiatChamp.Ha;

public interface IHaMqttClient
{
    Task ConnectAsync(CancellationToken cancellationToken);
    Task AnnounceAsync(IHaEntity entity);
    Task PublishAsync(IHaEntity entity);
    void Subscribe(IHaSetEntity entity);
}