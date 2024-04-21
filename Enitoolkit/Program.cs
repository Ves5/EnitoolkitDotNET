using Enitoolkit.Dictionaries;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add CORS for dev environment
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin();
        });
    });
}

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// add output cache using redis
builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions()
    {
        EndPoints =
        {
            builder.Configuration.GetValue<string>("RedisServer"),
            builder.Configuration.GetValue<string>("RedisPort")
        }
    };
    options.InstanceName = "Enitoolkit";
});

builder.Services.AddOutputCache(options =>
{
    if (builder.Environment.IsDevelopment())
        options.AddBasePolicy(builder =>
            builder.Expire(TimeSpan.FromSeconds(30)));
    else
        options.AddBasePolicy(builder =>
            builder.Expire(TimeSpan.FromSeconds(3600)));
});

builder.Services.AddSingleton<IDictionaryService, DictionaryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors();
}

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/healthz");

app.UseOutputCache();

using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;
    var ds = services.GetRequiredService<IDictionaryService>();
    ds.LoadDictionary();
}

app.Run();
