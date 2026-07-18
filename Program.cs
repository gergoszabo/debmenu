// namespace debmenu;

using System.Reflection;
using debmenu;
using debmenu.Providers.Inference;
using debmenu.Providers.Infrastructure;
using debmenu.Restaurants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = Host.CreateApplicationBuilder();

builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    builder.Services.AddSerilog();

    builder.Logging.AddSerilog();

    builder.Services.AddSingleton(Log.Logger);

    builder.Services.AddHttpClient();

    builder.Services.AddOptions<GeminiOptions>()
        .Bind(builder.Configuration.GetSection("Gemini"))
        .ValidateDataAnnotations()
        .ValidateOnStart();
    builder.Services.AddOptions<AWSOptions>()
        .Bind(builder.Configuration.GetSection("AWS"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    builder.Services.AddTransient<IInfrastructureProvider, AWS>();
    builder.Services.AddKeyedTransient<IInferenceProvider, Gemini>("gemini");
    builder.Services.AddTransient<IInferenceProvider>(sp =>
        new CachedInferenceProvider(sp.GetRequiredKeyedService<IInferenceProvider>("gemini"), sp.GetRequiredService<ILogger>())
    );

    builder.Services.AddTransient<IRestaurant, Forest>();
    builder.Services.AddTransient<IRestaurant, Viktoria>();
    builder.Services.AddTransient<IRestaurant, Huse>();
    builder.Services.AddTransient<IRestaurant, Govinda>();
    builder.Services.AddSingleton<DataCollector>();
    builder.Services.AddSingleton<Orchestrator>();

    using IHost host = builder.Build();

    await host.Services.GetRequiredService<Orchestrator>().RunAsync();
}
finally
{
    Log.CloseAndFlush();
}

