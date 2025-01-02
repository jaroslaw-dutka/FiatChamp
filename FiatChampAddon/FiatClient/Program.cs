using FiatChamp.App;
using FiatChamp.Fiat;
using FiatChamp.Ha;
using FiatChamp.Http;
using FiatChamp.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += delegate
{
    cts.Cancel();
};

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables("FiatChamp_")
    .AddUserSecrets<Program>()
    .Build();

var services = new ServiceCollection();

services.AddLogger(configuration);
services.AddHttp(configuration);

services.AddFiat(configuration);
services.AddHa(configuration);
services.AddApp(configuration);

var provider = services.BuildServiceProvider();

var app = provider.GetRequiredService<IAppService>();
if (!cts.IsCancellationRequested)
    await app.RunAsync(cts.Token);