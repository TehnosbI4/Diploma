using Microsoft.AspNetCore.SignalR;
using MovementMonitoring.Data;
using MovementMonitoring.Hubs;
using MovementMonitoring.Utilities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MovementMonitoring.Services;
public class BackgroundBusService : BackgroundService
{
    private readonly ILogger<BackgroundBusService> _logger;
    private readonly EntityDBContext _context;
    private readonly IHubContext<ViolationNotificationHub, IViolationNotification> _notContext;
    private IConnection _connection;
    private IModel _channel;


    public BackgroundBusService(
        ILogger<BackgroundBusService> logger,
        IServiceProvider serviceProvider,
        IHubContext<ViolationNotificationHub, IViolationNotification> notContext)
    {
        _context = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<EntityDBContext>();
        _logger = logger;
        ConnectionFactory factory = new() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "validator", durable: false, exclusive: false, autoDelete: false, arguments: null);
        _notContext = notContext;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();
        EventingBasicConsumer consumer = new(_channel);
        consumer.Received += (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            MessageHandler messageHandler = new(message, _context, _notContext);
        };
        _channel.BasicConsume(queue: "validator", autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}
