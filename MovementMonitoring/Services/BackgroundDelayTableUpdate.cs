using Microsoft.AspNetCore.SignalR;
using MovementMonitoring.Hubs;
using MovementMonitoring.Utilities;

namespace MovementMonitoring.Services
{
    public class BackgroundDelayTableUpdate : BackgroundService
    {
        private readonly ILogger<BackgroundDelayTableUpdate> _logger;
        private readonly IHubContext<UpdateTableHub, IUpdateTableClient> _hubContext;
        private readonly TimeSpan _period = TimeSpan.FromSeconds(5);

        public BackgroundDelayTableUpdate(ILogger<BackgroundDelayTableUpdate> logger, IHubContext<UpdateTableHub, IUpdateTableClient> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(_period);
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    foreach (KeyValuePair<string, bool> table in TablesUpdateList.Tables)
                    {
                        if (table.Value)
                        {
                            TablesUpdateList.Tables[table.Key] = false;
                            await _hubContext.Clients.All.ReceiveUpdateTableNotify(table.Key);
                        }

                    }
                    //for (int i = 0; i < 3; i++)
                    //{
                    //    TablesUpdateList.SetTableUpdateRequest("AccessLevel");
                    //}
                }
                catch
                {

                }
            }
        }
    }
}
