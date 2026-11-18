using BookStore.Common.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace BookStore.Common.Messaging.Services
{
    public class RabbitMqConnectionManager: IRabbitMqConnectionManager
    {
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqConnectionManager> _logger;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1,1); 
        private readonly SemaphoreSlim _poolSemaphore;
        private readonly ConcurrentQueue<IChannel> _channelPool = new ();
        private IConnection? _connection;
        private bool _exchangeDeclared;
        public RabbitMqOptions Options => _options;
        public RabbitMqConnectionManager(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnectionManager> logger)
        {
            _options = options.Value;
            _logger = logger;
            _poolSemaphore = new SemaphoreSlim(_options.MaxProducerChannels, _options.MaxProducerChannels);
        }
        public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (_connection is { IsOpen: true })
                return _connection;

            await _connectionLock.WaitAsync(cancellationToken);

            try
            {
                if (_connection is { IsOpen: true })
                    return _connection;

                var factory = new ConnectionFactory
                {
                    HostName = _options.HostName,
                    Port = _options.Port,
                    UserName = _options.UserName,
                    Password = _options.Password,
                    VirtualHost = _options.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
                };

                _connection = await factory.CreateConnectionAsync("BookStore-Connection", cancellationToken);
                _logger.LogInformation("Connected to RabbitMQ Broker at {Host}:{Port}", _options.HostName, _options.Port);

                return _connection;
            }
            finally
            {
                _connectionLock.Release();
            }
        }
        public async Task<IChannel> RentChannelAsync(CancellationToken cancellationToken = default)
        {
            await _poolSemaphore.WaitAsync(cancellationToken);

            try
            {
                while(_channelPool.TryDequeue(out var pooledChannel))
                {
                    if (pooledChannel.IsOpen)
                        return pooledChannel;

                    await SafeDisposeChannelAsync(pooledChannel);
                }

                var connection = await GetConnectionAsync(cancellationToken);
                var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                if (!_exchangeDeclared)
                {
                    await channel.ExchangeDeclareAsync(
                        exchange: _options.ExchangeName,
                        type: ExchangeType.Topic,
                        durable: true,
                        autoDelete: false,
                        cancellationToken: cancellationToken);
                    _exchangeDeclared = true;
                }

                return channel;
            }
            catch
            {
                _poolSemaphore.Release();
                throw;
            }
        }
        public async ValueTask ReturnChannelAsync(IChannel channel)
        {
            try
            {
                if(channel.IsOpen)
                {
                    _channelPool.Enqueue(channel);
                }
                else
                {
                    await SafeDisposeChannelAsync(channel);
                }
            }
            finally
            {
                _poolSemaphore.Release();
            }
        }
        public async Task<IChannel> CreateDedicatedChannelAsync(CancellationToken cancellationToken = default)
        {
            var connection = await GetConnectionAsync(cancellationToken);
            return await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        }
        public async ValueTask DisposeAsync()
        {
            while (_channelPool.TryDequeue(out var channel))
            {
                await SafeDisposeChannelAsync(channel);
            }

            if (_connection is not null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }

            _connectionLock.Dispose();
            _poolSemaphore.Dispose();
        }
        private static async ValueTask SafeDisposeChannelAsync(IChannel channel)
        {
            try
            {
                await channel.CloseAsync();
                await channel.DisposeAsync();
            }
            catch { }
        }
    }
}
