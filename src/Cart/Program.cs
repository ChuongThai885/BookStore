using BookStore.Common.Configuration;
using Cart.API.Extension;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);


builder.AddSharedConfiguration();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
