using BookStore.Common.Messaging.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookStore.Common.Messaging.Services
{
    public static class RabbitMqServicesExtensions
    {
        public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services) 
        {
            services.AddOptions<RabbitMqOptions>()
                .BindConfiguration(RabbitMqOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IRabbitMqConnectionManager, RabbitMqConnectionManager>();
            services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

            return services;
        }
    }
}
