using BookStore.Common.Caching;
using BookStore.Common.Messaging.Consumers;
using BookStore.Common.Messaging.Interfaces;
using Cart.Application.Contract;
using Cart.Domain.Entity;
using Cart.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Cart.Infrastructure.BackgroundJobs
{
    public class CartSyncBackgroundService: RabbitMqBatchConsumerBase<CartTouchedEvent>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICacheService _cacheService;
        protected override string QueueName => "cart.sync.db";
        protected override string RoutingKey => "cart.*";
        protected override ushort BatchSize => 100;
        protected override TimeSpan BatchTimeout => TimeSpan.FromSeconds(5);
        public CartSyncBackgroundService(IRabbitMqConnectionManager connectionManager, ILogger<CartSyncBackgroundService> logger, IServiceScopeFactory scopeFactory, ICacheService cacheService): base(connectionManager, logger)
        {
            _scopeFactory = scopeFactory;
            _cacheService = cacheService;
        }
        protected override async Task ProcessBatchAsync(IReadOnlyList<BatchEnvelope<CartTouchedEvent>> batch, CancellationToken cancellationToken)
        {
            var uniqueUserToSync = batch
                .GroupBy(x => x.Payload.UserId)
                .Select(g => g.OrderByDescending(x => x.Payload.Timestamp).First().Payload.UserId)
                .ToList();

            if (!uniqueUserToSync.Any())
                return;

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            foreach (var userId in uniqueUserToSync)
            {
                var redisKey = $"cart:{userId}";

                Dictionary<string, long> itemDict = await _cacheService.HashGetAllAsync(redisKey);

                if (itemDict == null || itemDict.Count == 0)
                {
                    var existingEmptyCart = await dbContext.Carts.FirstOrDefaultAsync(c => c.Id == userId, cancellationToken);
                    if(existingEmptyCart != null)
                    {
                        dbContext.Carts.Remove(existingEmptyCart);
                    }
                    continue;
                }
                var itemJson = JsonSerializer.Serialize(itemDict);

                var existingCart = await dbContext.Carts.FirstOrDefaultAsync(c => c.Id == userId, cancellationToken);
                if(existingCart != null)
                {
                    existingCart.CartInfo = itemJson;
                    dbContext.Carts.Update(existingCart);
                }
                else
                {
                    var newCart = new CartEntity
                    {
                        Id = userId,
                        CartInfo = itemJson
                    };
                    dbContext.Carts.Add(newCart);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
