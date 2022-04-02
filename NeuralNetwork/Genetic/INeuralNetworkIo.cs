using NeuralSharp.Serialization;

namespace NeuralSharp.Genetic;

public interface INeuralNetworkIo
{
    Task<NetworkConfig?> Load();
    Task Save(NetworkConfig network);
}