using System;
using System.Collections.Generic;
using System.Text;

namespace BookStore.Common.Messaging.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default) where T: class;
    }
}
