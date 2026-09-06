using Product.API.Extentions;

var builder = WebApplication.CreateBuilder(args);

var rootPath = Directory.GetParent(builder.Environment.ContentRootPath)!.Parent!.FullName;

var configPath = Path.Combine(rootPath, "Settings", "appsettings.json");

builder.Configuration.AddJsonFile(configPath, optional: true, reloadOnChange: true);

// Add services to the container.

builder.Services.AddServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
}
    app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
