using Service;
using Service.Components;
using Service.Helpers;
using Service.Interfaces;

using IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "Scheduler";
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<Scheduler>();
        services.AddSingleton<ILogWriter, LogWriter>();
        services.AddSingleton<string[]>(args);
        services.AddSingleton<IApp, App>();
    })
    .Build();

await host.RunAsync();
