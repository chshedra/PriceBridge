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
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder
    .Services.AddScoped<MarketplaceItemService>()
    .AddScoped<LocalizationService>()
    .AddScoped<TaobaoSearchService>()
    .AddScoped<AlibabaTranslationService>()
    .AddScoped<SearchQueryBuilder>();

builder.Services.AddSingleton(_ => new AlibabaCloud.SDK.Alimt20181012.Client(
    new AlibabaCloud.OpenApiClient.Models.Config
    {
        AccessKeyId = builder.Configuration["AlibabaCloud:Id"],
        AccessKeySecret = builder.Configuration["AlibabaCloud:Secret"],
        Endpoint =
            builder.Configuration["AlibabaCloud:TranslationEndpoint"]
            ?? "mt.cn-hangzhou.aliyuncs.com",
    }
));

builder.Services.AddHttpClient<TaobaoClient>(client =>
{
    var host = builder.Configuration["RapidApi:TaobaoHost"] ?? "taobao-datahub.p.rapidapi.com";
    client.BaseAddress = new Uri($"https://{host}/");
    client.DefaultRequestHeaders.Add("x-rapidapi-host", host);
    client.DefaultRequestHeaders.Add("x-rapidapi-key", builder.Configuration["RapidApi:Key"]);
});

var app = builder.Build();

app.UseCors("Extension");

app.MapApiRoutes();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
