using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BookStore.Common.Messaging.Interfaces
{
    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMQ";
        [Required(AllowEmptyStrings = false)]
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        [Required(AllowEmptyStrings = false)]
        public string UserName { get; set; } = "guest";

        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; } = "guest";

        [Required(AllowEmptyStrings = false)]
        public string VirtualHost { get; set; } = "/";

        [Required(AllowEmptyStrings = false)]
        public string ExchangeName { get; set; } = "bookstore.topic";

        [Required(AllowEmptyStrings = false)]
        public string DeadLetterExchangeName { get; set; } = "bookstore.dlx";
        [Range(1, 200)]
        public int MaxProducerChannels { get; set; } = 20;
    }
}
