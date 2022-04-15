using Service.Interfaces;

namespace Service
{
    public class Motor : BackgroundService
    {
        private readonly IHandler _handler;
        private System.Timers.Timer? _timer;

        public Motor(IServiceScopeFactory serviceScopeFactory)
        {
            using var serviceScope = serviceScopeFactory.CreateScope();
            _handler = serviceScope.ServiceProvider.GetRequiredService<IHandler>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //felesleges a Task.Run
            _timer = new();
            _timer.Interval = 1000;
            _timer.AutoReset = true;
            _timer.Elapsed += (sender, e) =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    _timer.Stop();
                    _timer.Dispose();
                    return;
                }
                _handler.RunDlls();
            };
            _timer.Enabled = true;
        }

    }
}
