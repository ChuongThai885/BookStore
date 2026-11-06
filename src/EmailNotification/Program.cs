using BookStore.Common.Configuration;
using EmailNotification;

var builder = Host.CreateApplicationBuilder(args);

builder.AddSharedConfiguration();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
