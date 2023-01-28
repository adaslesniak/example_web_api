using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using WorkerServicePlayground;

using IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options => {
        options.ServiceName = "Product Watch";
    })
    .ConfigureServices(services => {
        LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(services);
        services.AddSingleton<WebClient>();
        services.AddHostedService<WatchYourProducts>();
    })
    .ConfigureLogging((context, logging) => {
        logging.AddConfiguration(context.Configuration.GetSection("Logging"));
    })
    .Build();
await host.RunAsync();
