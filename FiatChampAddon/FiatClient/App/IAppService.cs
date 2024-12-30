namespace FiatChamp.App;

public interface IAppService
{
    Task RunAsync(CancellationToken cancellationToken);
}