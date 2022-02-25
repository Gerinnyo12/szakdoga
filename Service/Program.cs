using NLog;
using NLog.Web;
using Service;
using Service.Helpers;
using Service.Implementations;
using Service.Interfaces;

var logger = LogManager.Setup().RegisterNLogWeb().GetCurrentClassLogger();
try
{
    IHost host = Host.CreateDefaultBuilder(args)
        .UseWindowsService(options =>
        {
            options.ServiceName = "Scheduler";
        })
        .ConfigureServices(services =>
        {
            services.AddHostedService<Scheduler>();
            services.AddSingleton(args);
            services.AddSingleton(AppDomain.CurrentDomain.GetAssemblies());
            services.AddTransient<IApp, App>();
            services.AddTransient<IContextContainer, ContextContainer>();
            services.AddTransient<IAssemblyContext, AssemblyContext>();
            services.AddTransient<IZipHandler, ZipHandler>();
            services.AddTransient<IRunable, Runable>();
            services.AddTransient<IFileHandler, FileHandler>();
        })
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
        })
        .UseNLog()
        .Build();

    // ha nem tud hozzaferni a mappakhoz akkor el se induljon
    //TODO blazorben a service inditasakor elkapni
    FileHelper.PrepareDirs();
    await host.RunAsync();
}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}
