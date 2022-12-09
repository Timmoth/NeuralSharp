namespace NeuralSharp.Serialization;

public sealed record NetworkConfig(LayerConfig[] Layers)
{
    public bool Equals(NetworkConfig other)
    {
        return other != null && Layers.SequenceEqual(other.Layers);
    }

    public static NetworkConfig From(NeuralNetwork network)
    {
        var layers = new LayerConfig[network.Layers.Length];
        for (int i = 0; i < network.Layers.Length; i++)
        {
            Layer? networkLayer = network.Layers[i];
            var neuronData = new NeuronData[networkLayer.Neurons.Length];

            for (int i1 = 0; i1 < networkLayer.Neurons.Length; i1++)
            {
                Neuron? neuron = networkLayer.Neurons[i1];
                var weights = neuron.Out.Select(c => c.Weight).ToArray();
                neuronData[i1] = new NeuronData(neuron.Bias, weights);
            }

            layers[i] = new LayerConfig(neuronData);
        }

        return new NetworkConfig(layers);
    }

    public NetworkConfig DeepCopy()
    {
        var layers = new LayerConfig[Layers.Length];
        for (int i = 0; i < Layers.Length; i++)
        {
            LayerConfig? networkLayer = Layers[i];
            var neuronData = new NeuronData[networkLayer.Neurons.Length];

            for (int i1 = 0; i1 < networkLayer.Neurons.Length; i1++)
            {
                NeuronData? neuron = networkLayer.Neurons[i1];
                neuronData[i1] = new NeuronData(neuron.Bias, neuron.Weights);
            }

            layers[i] = new LayerConfig(neuronData);
        }

        return new NetworkConfig(layers);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Layers);
    }
}