namespace NeuralSharp.Serialization;

public sealed record NetworkConfig(List<LayerConfig> Layers)
{
    public bool Equals(NetworkConfig other)
    {
        return other != null && Layers.SequenceEqual(other.Layers);
    }

    public static NetworkConfig From(NeuralNetwork network)
    {
        var layers = new List<LayerConfig>();
        foreach (var networkLayer in network.Layers)
        {
            var neuronData = new List<NeuronData>();

            foreach (var neuron in networkLayer.Neurons)
            {
                var weights = neuron.Out.Select(c => c.Weight).ToList();
                neuronData.Add(new NeuronData(neuron.Bias, weights));
            }

            layers.Add(new LayerConfig(neuronData));
        }

        return new NetworkConfig(layers);
    }

    public NetworkConfig DeepCopy()
    {
        var layers = new List<LayerConfig>();
        foreach (var networkLayer in Layers)
        {
            var neuronData = new List<NeuronData>();

            foreach (var neuron in networkLayer.Neurons)
            {
                neuronData.Add(new NeuronData(neuron.Bias, neuron.Weights.ToList()));
            }

            layers.Add(new LayerConfig(neuronData));
        }

        return new NetworkConfig(layers);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Layers);
    }
}