namespace MovementMonitoring.Hubs
{
	public interface IViolationNotification
	{
		Task ViolationNotify(string violationId, string dateTime, string roomName);
	}
}
