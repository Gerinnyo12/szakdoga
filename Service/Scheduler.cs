using Service.Interfaces;

namespace Service
{
    public class Scheduler : BackgroundService
    {
        private readonly IApp _app;

        public Scheduler(IApp app)
        {
            _app = app;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _app.RunDlls();
                await Task.Delay(1000, stoppingToken);
            }
        }

    }
}
