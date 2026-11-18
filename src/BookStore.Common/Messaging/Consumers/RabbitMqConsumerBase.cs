using BookStore.Common.Messaging.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BookStore.Common.Messaging.Consumers
{
    public abstract class RabbitMqConsumerBase<TMessage>: BackgroundService where TMessage : class
    {
        private readonly IRabbitMqConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private IChannel? _consumerChannel;

        protected abstract string QueueName { get; }
        protected abstract string RoutingKey { get; }
        protected virtual ushort PrefetchCount => 10;
        protected RabbitMqConsumerBase(IRabbitMqConnectionManager connectionManager, ILogger logger)
        {
            _connectionManager = connectionManager;
            _logger = logger;
        }
        protected abstract Task HandleMessageAsync(TMessage message, CancellationToken cancellationToken);
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _consumerChannel = await _connectionManager.CreateDedicatedChannelAsync(cancellationToken);
            var options = _connectionManager.Options;

            // 1. Declare the main exchange and the dead-letter exchange
            await _consumerChannel.ExchangeDeclareAsync(options.ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
            await _consumerChannel.ExchangeDeclareAsync(options.DeadLetterExchangeName, ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);

            // 2. Declare the main queue with dead-lettering properties
            var dlqName = $"{QueueName}.dlq";
            await _consumerChannel.QueueDeclareAsync(dlqName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
            await _consumerChannel.QueueBindAsync(dlqName, options.DeadLetterExchangeName, routingKey: QueueName, cancellationToken: cancellationToken);

            // 3. Declare the main queue with dead-lettering properties
            var queueArgs = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", options.DeadLetterExchangeName },
                { "x-dead-letter-routing-key", QueueName }
            };
            await _consumerChannel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs, cancellationToken: cancellationToken);
            await _consumerChannel.QueueBindAsync(QueueName, options.ExchangeName, RoutingKey, cancellationToken: cancellationToken);

            // 4. Set the prefetch count for the consumer
            await _consumerChannel.BasicQosAsync(0, PrefetchCount, false, cancellationToken);

            // 5. Start consuming messages
            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<TMessage>(eventArgs.Body.Span);
                    if(message is null)
                    {
                        await _consumerChannel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
                        return;
                    }

                    await HandleMessageAsync(message, cancellationToken);
                    await _consumerChannel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process message on {Queue}. Sending to DLQ.", QueueName);
                    await _consumerChannel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
                }
            };

            await _consumerChannel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if(_consumerChannel is not null)
            {
                await _consumerChannel.CloseAsync(cancellationToken);
                await _consumerChannel.DisposeAsync();
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
