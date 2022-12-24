using Demo.Commands;
using Demo.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NeuralSharp.Activation;
using NeuralSharp.Generators;
using NeuralSharp.Genetic;
using Spectre.Console.Cli;
using Trainer.Commands;

// Create a type registrar and register any dependencies.
// A type registrar is an adapter for a DI framework.
var services = new ServiceCollection();

// Services
services.AddTransient<IWeightGenerator, WeightGenerator>();
services.AddTransient<IBiasGenerator, BiasGenerator>();
services.AddTransient<IActivationFunction, Tanh>();
services.AddTransient<INeuralNetworkIo>(_ => new FileNetworkIo("./network.json"));
services.AddTransient<INetworkMutator, NetworkMutator>();
services.AddTransient<IFloatMutator, FloatMutator>();
services.AddTransient<IMutationDecider>(_ => new MutationDecider(0.1f));
services.AddTransient<NetworkTrainer>();
services.AddLogging(b => b
    .AddFilter("Microsoft", LogLevel.Warning)
    .AddFilter("System", LogLevel.Warning)
    .AddConsole());
var registrar = new TypeRegistrar(services);

// Create a new command app with the registrar
// and run it with the provided arguments.
var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.AddCommand<Test1Command>("test1")
        .WithDescription("Run Test 1");

    config.AddCommand<Test2Command>("test2")
        .WithDescription("Run Test 2");

    config.AddCommand<Test3Command>("snake")
    .WithDescription("Run Test Snake");

#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
});

return app.Run(args);