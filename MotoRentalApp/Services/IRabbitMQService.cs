using RabbitMQ.Client;
using System;

using MotoRentalApp.Services;

public interface IRabbitMQService
{
    void SendMessage(string message);
    void Dispose();
}

public class RabbitMQService : IRabbitMQService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQService()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            Port = 5672
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "fila_mensagens",
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
    }

    public void SendMessage(string message)
    {
        var body = System.Text.Encoding.UTF8.GetBytes(message);
        var properties = _channel.CreateBasicProperties();
        properties.DeliveryMode = 2;

        _channel.BasicPublish(exchange: "",
                             routingKey: "fila_mensagens",
                             basicProperties: properties,
                             body: body);
        Console.WriteLine($"[x] Sent {message}");
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
