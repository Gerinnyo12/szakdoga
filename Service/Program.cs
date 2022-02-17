using Service.Components;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Scheduler>();
    })
    .Build();

await host.RunAsync();
