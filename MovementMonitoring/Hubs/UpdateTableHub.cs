using Microsoft.AspNetCore.SignalR;

namespace MovementMonitoring.Hubs
{
    public class UpdateTableHub : Hub<IUpdateTableClient>
    {
        public async Task SendMessage(string table)
        {
            await Clients.All.ReceiveUpdateTableNotify(table);
        }
    }
}
