using Microsoft.EntityFrameworkCore;
using PriceBridge;
using PriceBridge.Service;

Console.WriteLine("Hello, World!");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Extension",
        policy =>
            policy
                .WithOrigins("chrome-extension://edkamdmpklejphpabdmkojdnokmeenpf")
                .AllowAnyHeader()
                .AllowAnyMethod()
    );
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder
    .Services.AddScoped<MarketplaceItemService>()
    .AddScoped<LinkProcessor>()
    .AddScoped<LocalizationService>()
    .AddSingleton<BrowserHolder>();

var app = builder.Build();

app.UseCors("Extension");

app.MapApiRoutes();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
