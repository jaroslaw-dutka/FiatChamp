namespace FiatChamp;

public interface IApp
{
    Task RunAsync(CancellationToken cancellationToken);
}