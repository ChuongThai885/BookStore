using BookStore.Common.Messaging.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace BookStore.Common.Messaging.Consumers
{
    public record BatchEnvelope<T>(T Payload, ulong DeliveryTag);
    public abstract class RabbitMqBatchConsumerBase<TMessage> : BackgroundService where TMessage : class
    {
        private readonly IRabbitMqConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly Channel<BatchEnvelope<TMessage>> _buffer;
        private IChannel? _consumerChannel;

        protected abstract string QueueName { get; }
        protected abstract string RoutingKey { get; }
        protected virtual ushort BatchSize => 100;
        protected virtual TimeSpan BatchTimeout => TimeSpan.FromSeconds(3);
        protected RabbitMqBatchConsumerBase(IRabbitMqConnectionManager connectionManager, ILogger logger)
        {
            _connectionManager = connectionManager;
            _logger = logger;
            
            _buffer = Channel.CreateBounded<BatchEnvelope<TMessage>>(new BoundedChannelOptions(BatchSize * 2)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            });
        }
        protected abstract Task ProcessBatchAsync(IReadOnlyList<BatchEnvelope<TMessage>> batch, CancellationToken cancellationToken);
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

            // 4. Set the prefetch count to control the number of unacknowledged messages
            await _consumerChannel.BasicQosAsync(prefetchSize: 0, prefetchCount: (ushort)(BatchSize * 2), global: false, cancellationToken: cancellationToken);

            // 5. Start consuming messages
            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<TMessage>(eventArgs.Body.Span);

                    if (message is not null)
                    {
                        await _buffer.Writer.WriteAsync(new BatchEnvelope<TMessage>(message, eventArgs.DeliveryTag), cancellationToken);
                    }
                    else
                    {
                        // If the message is null, we consider it a poison message and send it to the DLQ, no requeueing.
                        await _consumerChannel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Poison message (invalid JSON) received on queue {QueueName}. Sending to DLQ.", QueueName);
                    await _consumerChannel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
                }
            };

            await _consumerChannel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
            _logger.LogInformation("Started consuming messages from queue [{QueueName}] with routing key [{RoutingKey}].", QueueName, RoutingKey);

            // 6. Start a background task to process messages in batches
            var currentBatch = new List<BatchEnvelope<TMessage>>(BatchSize);

            while (!cancellationToken.IsCancellationRequested)
            {
                currentBatch.Clear();

                // Wait for the first message in the batch
                if (!await _buffer.Reader.WaitToReadAsync(cancellationToken))
                    continue;

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(BatchTimeout);

                try
                {
                    while (currentBatch.Count < BatchSize && await _buffer.Reader.WaitToReadAsync(timeoutCts.Token))
                    {
                        while (currentBatch.Count < BatchSize && _buffer.Reader.TryRead(out var item))
                        {
                            currentBatch.Add(item);
                        }
                    }
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // Timeout reached, process the current batch
                }

                if(currentBatch.Count > 0)
                {
                    ulong maxDeliveryTag = currentBatch[^1].DeliveryTag;
                    try
                    {
                        await ProcessBatchAsync(currentBatch, cancellationToken);

                        // Acknowledge all messages in the batch
                        await _consumerChannel.BasicAckAsync(maxDeliveryTag, multiple: true, cancellationToken: cancellationToken);
                    }
                    catch(Exception exception)
                    {
                        _logger.LogError(exception, "Error processing batch of {Count} messages on {Queue}", currentBatch.Count, QueueName);
                        // Nack all messages in the batch without requeueing, sending them to the DLQ
                        await _consumerChannel.BasicNackAsync(maxDeliveryTag, multiple: true, requeue: false, cancellationToken: cancellationToken);
                    }
                }
            }
            
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _buffer.Writer.TryComplete();

            if (_consumerChannel is not null)
            {
                await _consumerChannel.CloseAsync(cancellationToken);
                await _consumerChannel.DisposeAsync();
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
