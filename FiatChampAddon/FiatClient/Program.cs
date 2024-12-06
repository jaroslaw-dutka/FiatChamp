using FiatChamp;
using FiatChamp.Fiat;
using FiatChamp.Ha;
using FiatChamp.Mqtt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += delegate
{
    cts.Cancel();
};

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables("FiatChamp_")
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .Build();

Log.Logger = new LoggerConfiguration()
    // .MinimumLevel.Is(appConfig.Debug ? LogEventLevel.Debug : LogEventLevel.Information)
    .WriteTo.Console()
    .CreateLogger();

var services = new ServiceCollection();

services.Configure<AppConfig>(configuration);
services.AddSingleton<IApp, App>();
services.AddSingleton<IMqttClient, MqttClient>();
services.AddSingleton<IFiatClient, FiatClient>();
services.AddSingleton<HaRestApi>();

var provider = services.BuildServiceProvider();

var app = provider.GetRequiredService<IApp>();
if (!cts.IsCancellationRequested)
    await app.RunAsync(cts.Token);