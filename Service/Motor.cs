using Service.Interfaces;

namespace Service
{
    public class Motor : BackgroundService
    {
        private readonly IHandler _handler;

        public Motor(IHandler handler)
        {
            _handler = handler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _handler.RunDlls();
                await Task.Delay(1000, stoppingToken);
            }
        }

    }
}
