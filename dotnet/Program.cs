// namespace debmenu;

using System.Text.Encodings.Web;
using System.Text.Json;
using debmenu.Providers.Inference;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var builder = Host.CreateApplicationBuilder();

builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));

builder.Services.AddTransient<IInferenceProvider, Gemini>();

// builder.Services.AddTransient<IMyService, MyService>();
// builder.Services.AddSingleton<Worker>();

using IHost host = builder.Build();

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
