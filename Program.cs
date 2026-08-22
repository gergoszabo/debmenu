// namespace debmenu;

using System.Reflection;
using System.Text.Json;
using debmenu;
using debmenu.Caching;
using debmenu.Providers.Inference;
using debmenu.Providers.Infrastructure;
using debmenu.Restaurants;
using debmenu.Utils;
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

    // builder.Services.AddHttpClient();
    builder.Services.AddHttpClient("LlamaCppClient", client =>
    {
        client.Timeout = TimeSpan.FromMinutes(10);
    });
    builder.Services.AddOptions<GeminiOptions>()
        .Bind(builder.Configuration.GetSection("Gemini"))
        .ValidateDataAnnotations()
        .ValidateOnStart();
    builder.Services.AddOptions<AWSOptions>()
        .Bind(builder.Configuration.GetSection("AWS"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    builder.Services.AddTransient<IInfrastructureProvider, AWS>();
    builder.Services.AddKeyedTransient<IInferenceProvider, LlamaCpp>("llama");
    //builder.Services.AddKeyedTransient<IInferenceProvider, Gemini>("gemini");
    builder.Services.AddTransient<IInferenceProvider>(sp =>
        new CachedInferenceProvider(sp.GetRequiredKeyedService<IInferenceProvider>("llama"), sp.GetRequiredService<ILogger>())
    );

    builder.Services.AddSingleton<IHttpResourceStateStore, FileHttpResourceStateStore>();
    builder.Services.AddSingleton<IRestaurantResultCache, FileRestaurantResultCache>();

    builder.Services.AddTransient<IRestaurant, Viktoria>();
    // builder.Services.AddTransient<IRestaurant, Huse>();
    // builder.Services.AddTransient<IRestaurant, Govinda>();
    // builder.Services.AddTransient<IRestaurant, Manna>();
    // builder.Services.AddTransient<IRestaurant, Forest>();
    builder.Services.AddSingleton<IDataCollector, DataCollector>();
    builder.Services.AddSingleton<IOrchestrator, Orchestrator>();
    builder.Services.AddSingleton<LlamaCpp>();

    using IHost host = builder.Build();

    await host.Services.GetRequiredService<IOrchestrator>().RunAsync();
    // await host.Services.GetRequiredService<LlamaCpp>().Inference("Hello");

    //var imageBytes = File.ReadAllBytes("huse.jpg");
    //var inferenceResult = await host.Services.GetRequiredService<LlamaCpp>().Inference(string.Empty, imageBytes, "image/jpg");

    //Console.WriteLine(JsonSerializer.Serialize(inferenceResult));
}
finally
{
    Log.CloseAndFlush();
}

