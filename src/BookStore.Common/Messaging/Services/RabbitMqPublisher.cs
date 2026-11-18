using BookStore.Common.Messaging.Interfaces;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BookStore.Common.Messaging.Services
{
    public class RabbitMqPublisher: IMessagePublisher
    {
        private readonly IRabbitMqConnectionManager _connectionManager;
        public RabbitMqPublisher(IRabbitMqConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }
        public async Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default) where T : class
        {
            var channel = await _connectionManager.RentChannelAsync(cancellationToken);
            try
            {
                var body = JsonSerializer.SerializeToUtf8Bytes(message);

                var props = new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent,
                    MessageId = Guid.NewGuid().ToString(),
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                };

                await channel.BasicPublishAsync(
                    exchange: _connectionManager.Options.ExchangeName,
                    routingKey: routingKey,
                    mandatory: false,
                    basicProperties: props,
                    body: body,
                    cancellationToken: cancellationToken);
            }
            finally
            {
                await _connectionManager.ReturnChannelAsync(channel);
            }
        }
    }
}
