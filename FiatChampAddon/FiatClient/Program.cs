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
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables("FiatChamp_")
    .AddUserSecrets<Program>()
    .Build();

Log.Logger = new LoggerConfiguration()
    // .MinimumLevel.Is(appConfig.Debug ? LogEventLevel.Debug : LogEventLevel.Information)
    .WriteTo.Console()
    .CreateLogger();

var services = new ServiceCollection();

services.Configure<AppConfig>(configuration.GetSection("app"));
services.Configure<FiatConfig>(configuration.GetSection("fiat"));
services.Configure<MqttConfig>(configuration.GetSection("mqtt"));
services.Configure<HaConfig>(configuration.GetSection("ha"));

services.AddSingleton<IApp, App>();
services.AddSingleton<IFiatClient, FiatClient>();
services.AddSingleton<IMqttClient, MqttClient>();
services.AddSingleton<IHaRestApi, HaRestApi>();

var provider = services.BuildServiceProvider();

var app = provider.GetRequiredService<IApp>();
if (!cts.IsCancellationRequested)
    await app.RunAsync(cts.Token);