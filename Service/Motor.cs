using Service.Interfaces;

namespace Service
{
    public class Motor : BackgroundService
    {
        private System.Timers.Timer? _timer;
        private readonly IHandler _handler;

        public Motor(IHandler handler) => _handler = handler;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _timer = new System.Timers.Timer();
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
