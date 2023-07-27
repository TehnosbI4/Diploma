namespace MovementMonitoring.Hubs
{
    public interface IUpdateTableClient
    {
        Task ReceiveUpdateTableNotify(string table);
    }
}
