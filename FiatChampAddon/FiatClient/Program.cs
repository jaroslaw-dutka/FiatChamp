using FiatChamp;
using FiatChamp.Fiat;
using FiatChamp.Ha;
using FiatChamp.Mqtt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

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

var appConfig = new AppSettings();
configuration.GetSection("app").Bind(appConfig);

var logger = new LoggerConfiguration()
    .MinimumLevel.Is(appConfig.Debug ? LogEventLevel.Debug : LogEventLevel.Information)
    .WriteTo.Console()
    .CreateLogger();

var services = new ServiceCollection();

services.AddHttpClient();
services.AddLogging(i => i.AddSerilog(logger));

services.Configure<AppSettings>(configuration.GetSection("app"));
services.Configure<FiatSettings>(configuration.GetSection("fiat"));
services.Configure<MqttSettings>(configuration.GetSection("mqtt"));
services.Configure<HaSettings>(configuration.GetSection("ha"));

services.AddSingleton<IApp, App>();
services.AddSingleton<FiatClient>();
services.AddSingleton<FiatClientFake>();
services.AddSingleton<IFiatClient>(s => s.GetService<IOptions<AppSettings>>().Value.FakeApi
    ? s.GetService<FiatClientFake>()
    : s.GetService<FiatClient>());
services.AddSingleton<IMqttClient, MqttClient>();
services.AddSingleton<IHaRestApi, HaRestApi>();

var provider = services.BuildServiceProvider();

var app = provider.GetRequiredService<IApp>();
if (!cts.IsCancellationRequested)
    await app.RunAsync(cts.Token);