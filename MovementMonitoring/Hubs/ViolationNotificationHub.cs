using Microsoft.AspNetCore.SignalR;

namespace MovementMonitoring.Hubs
{
	public class ViolationNotificationHub: Hub<IViolationNotification>
	{
		public async Task SendMessage(string violationId, string dateTime, string roomName)
		{
			await Clients.All.ViolationNotify(violationId, dateTime, roomName);
		}
	}
}
