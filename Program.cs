// namespace debmenu;

using System.Reflection;
using debmenu;
using debmenu.Caching;
using debmenu.Providers.Inference;
using debmenu.Providers.Infrastructure;
using debmenu.Restaurants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

    var provider = (builder.Configuration.GetValue("Inference:Provider", "llamacpp") ?? "llamacpp").ToLowerInvariant();

    // Both option sets are always bound so CachedInferenceProvider can read them, but only the
    // selected provider's options carry validation attributes — that's what ValidateOnStart checks.
    builder.Services.AddOptions<GeminiOptions>().Bind(builder.Configuration.GetSection("Gemini"));
    builder.Services.AddOptions<LlamaCppOptions>().Bind(builder.Configuration.GetSection("LlamaCpp"));

    switch (provider)
    {
        case "llamacpp":
            builder.Services.AddOptions<LlamaCppOptions>()
                .ValidateDataAnnotations().ValidateOnStart();
            builder.Services.AddKeyedTransient<IInferenceProvider, LlamaCpp>("llamacpp");
            break;
        default:
            builder.Services.AddOptions<GeminiOptions>()
                .ValidateDataAnnotations().ValidateOnStart();
            builder.Services.AddKeyedTransient<IInferenceProvider, Gemini>("gemini");
            break;
    }

    builder.Services.AddOptions<AWSOptions>()
        .Bind(builder.Configuration.GetSection("AWS"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    builder.Services.AddTransient<IInfrastructureProvider, AWS>();
    builder.Services.AddTransient<IInferenceProvider>(sp =>
        new CachedInferenceProvider(
            sp.GetRequiredKeyedService<IInferenceProvider>(provider),
            sp.GetRequiredService<ILogger>())
    );

    builder.Services.AddSingleton<IHttpResourceStateStore, FileHttpResourceStateStore>();
    builder.Services.AddSingleton<IRestaurantResultCache, FileRestaurantResultCache>();

    builder.Services.AddTransient<IRestaurant, Forest>();
    builder.Services.AddTransient<IRestaurant, Viktoria>();
    builder.Services.AddTransient<IRestaurant, Huse>();
    builder.Services.AddTransient<IRestaurant, Govinda>();
    builder.Services.AddTransient<IRestaurant, Manna>();
    builder.Services.AddSingleton<IDataCollector, DataCollector>();
    builder.Services.AddSingleton<IOrchestrator, Orchestrator>();

    using IHost host = builder.Build();

    // Starts the options validation hosted services (ValidateOnStart) so missing/invalid
    // configuration fails fast with a descriptive error before RunAsync.
    await host.StartAsync();

    await host.Services.GetRequiredService<IOrchestrator>().RunAsync();
}
finally
{
    Log.CloseAndFlush();
}

