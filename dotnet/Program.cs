// namespace debmenu;

using System.Reflection;
using System.Text.Json;
using debmenu.Providers.Inference;
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
    .WriteTo.Console( outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Services.AddSerilog();

builder.Logging.AddSerilog();

builder.Services.AddSingleton(Log.Logger);

builder.Services.AddHttpClient();

builder.Services.AddOptions<GeminiOptions>()
    .Bind(builder.Configuration.GetSection("Gemini"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddTransient<IInferenceProvider, Gemini>();
builder.Services.AddSingleton<Forest>();
builder.Services.AddSingleton<Viktoria>();
builder.Services.AddSingleton<Huse>();
builder.Services.AddSingleton<Govinda>();

using IHost host = builder.Build();

var restaurant = host.Services.GetRequiredService<Govinda>();

var response = await restaurant.GetOffers();

Console.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions(){ WriteIndented = true }));




// public static class Program
// {
//     public static async Task Main(string[] args)
//     {
//         Env.Load();

//         var gemini = new Gemini(Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? throw new Exception("GEMINI_API_KEY not set"));
//         var viktoria = await Viktoria.GetOffers(gemini);
//         var govinda = await Govinda.GetOffers(gemini);
//         var forest = await Forest.GetOffers(gemini);
//         var huse = await Huse.GetOffers(gemini);

//         var allOffers = new Dictionary<string, Dictionary<string, List<string>>>
//         {
//             { "Viktoria", viktoria },
//             { "Govinda", govinda },
//             { "Forest", forest },
//             { "Huse", huse }
//         };

//         var offersJson =JsonSerializer.Serialize(allOffers, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

//         var indexHtml = Html.Template.Replace("JSON_HERE", offersJson);

//         var aws = new AWS(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? throw new Exception("AWS_ACCESS_KEY_ID not set"), Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? throw new Exception("AWS_SECRET_ACCESS_KEY not set"));
//         await aws.UploadToS3Bucket(indexHtml);
//     }

//     private static IHost SetupDI()
//     {
//         var builder = Host.CreateApplicationBuilder();

//         builder.Services.AddTransient<IMyService, MyService>();
//         builder.Services.AddSingleton<Worker>();

//         using IHost host = builder.Build();
//     }
// }
