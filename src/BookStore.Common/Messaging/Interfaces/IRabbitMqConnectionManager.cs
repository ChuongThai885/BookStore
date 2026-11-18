using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookStore.Common.Messaging.Interfaces
{
    public interface IRabbitMqConnectionManager: IAsyncDisposable
    {
        RabbitMqOptions Options { get; }
        Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
        Task<IChannel> RentChannelAsync(CancellationToken cancellationToken = default);
        ValueTask ReturnChannelAsync(IChannel channel);
        Task<IChannel> CreateDedicatedChannelAsync(CancellationToken ct = default);
    }
}
