using BookStore.Common.Configuration;
using Microsoft.AspNetCore.HttpOverrides;
using Product.API.Extentions;

var builder = WebApplication.CreateBuilder(args);

builder.AddSharedConfiguration();

// Add services to the container.

builder.Services.AddServices(builder.Configuration);

var app = builder.Build();

// 1. CONFIGURE FORWARDED HEADERS MIDDLEWARE
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
// Delete the default networks and proxies to allow all forwarded headers from any source
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

// 2. TURN ON FORWARDED HEADERS MIDDLEWARE
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseCors("GlobalCorsPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
