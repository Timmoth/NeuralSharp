using System;
using System.Net.Http;
using System.Threading.Tasks;
using Aptacode.AppFramework;
using Aptacode.BlazorCanvas;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NeuralSharp.Activation;
using NeuralSharp.Generators;

namespace BlazorDemo;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddScoped<SceneRenderController>();
        builder.Services.AddScoped<SceneInteractionController>();

        builder.Services.AddTransient<IWeightGenerator, WeightGenerator>();
        builder.Services.AddTransient<IBiasGenerator, BiasGenerator>();
        builder.Services.AddTransient<IActivationFunction, Tanh>();
        await builder.Build().RunAsync();
    }
}