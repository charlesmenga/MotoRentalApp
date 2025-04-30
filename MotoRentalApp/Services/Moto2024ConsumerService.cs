using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MotoRentalApp.Data;
using MotoRentalApp.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MotoRentalApp.Services
{
    public class Moto2024ConsumerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;

        public Moto2024ConsumerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

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

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var jsonDoc = JsonDocument.Parse(message);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("Event", out var eventProp) && eventProp.GetString() == "MotoCadastrada")
                    {
                        var ano = root.GetProperty("Ano").GetInt32();
                        if (ano == 2024)
                        {
                            var moto2024 = new Moto2024
                            {
                                Identificador = root.GetProperty("MotoId").GetString() ?? string.Empty,
                                Ano = ano,
                                Modelo = root.GetProperty("Modelo").GetString() ?? string.Empty,
                                Placa = root.GetProperty("Placa").GetString() ?? string.Empty
                            };

                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                                dbContext.Motos2024.Add(moto2024);
                                await dbContext.SaveChangesAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            };
            
            string consumerTag = _channel.BasicConsume(queue: "fila_mensagens", autoAck: false, consumer: consumer);

            return Task.Delay(Timeout.Infinite, stoppingToken);
        }


        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
