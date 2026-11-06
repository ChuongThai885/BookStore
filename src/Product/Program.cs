using BookStore.Common.Configuration;
using Product.API.Extentions;

var builder = WebApplication.CreateBuilder(args);

builder.AddSharedConfiguration();

// Add services to the container.

builder.Services.AddServices(builder.Configuration);

var app = builder.Build();

app.UseCors("GlobalCorsPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
