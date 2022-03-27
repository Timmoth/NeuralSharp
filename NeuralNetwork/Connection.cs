namespace NeuralNetwork;

public sealed class Connection
{
    public Connection(Neuron from, Neuron to, float weight)
    {
        From = from;
        To = to;
        Weight = weight;
    }

    public override string ToString()
    {
        return $"{Weight}";
    }

    #region Properties

    public Neuron From { get; }
    public Neuron To { get; }
    public float Weight { get; set; }

    #endregion
}