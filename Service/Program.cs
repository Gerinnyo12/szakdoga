using NLog;
using NLog.Web;
using Service;
using Service.Helpers;
using Service.Implementations;
using Service.Interfaces;
using Shared;
using Shared.Models.Parameters;


var logger = LogManager.Setup().RegisterNLogWeb().GetCurrentClassLogger();
try
{
    if (!Constants.IS_WINDOWS)
    {
        throw new InvalidOperationException("Nem támogatott operációs rendszer");
    }

    IHost host = Host.CreateDefaultBuilder(args)
        .UseWindowsService(options => options.ServiceName = Constants.SERVICE_NAME)
        .ConfigureServices((context, services) =>
        {
            services.AddHostedService<Motor>();
            services.AddSingleton<ReferenceHelper>();
            services.AddScoped<IObserver, Observer>();
            services.AddScoped<IContextContainer, ContextContainer>();
            services.AddScoped<IListener, Listener>();
            services.AddScoped<IAssemblyContext, AssemblyContext>();
            services.AddScoped<IZipExtracter, ZipExtracter>();
            services.AddScoped<IRunable, Runable>();
            services.AddScoped<IDllLifter, DllLifter>();
            services.Configure<ParametersModel>(context.Configuration.GetSection(Constants.PARAMETERS_NAME));
        })
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
        })
        .UseNLog()
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    logger.Error(ex, "A szervíz megállt hiba miatt!");
}
finally
{
    LogManager.Shutdown();
}
