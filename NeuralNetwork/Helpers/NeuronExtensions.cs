namespace NeuralNetwork.Helpers;

public static class NeuronExtensions
{
    public static Connection Connect(this Neuron from, Neuron to, float weight)
    {
        var connection = new Connection(from, to, weight);

        from.Out.Add(connection);
        to.In.Add(connection);

        return connection;
    }
}