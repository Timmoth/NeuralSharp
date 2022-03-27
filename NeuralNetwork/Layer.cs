using System.Text;
using NeuralNetwork.Activation;
using NeuralNetwork.Generators;
using NeuralNetwork.Helpers;
using NeuralNetwork.Serialization;

namespace NeuralNetwork;

public sealed class Layer : IEquatable<Layer>
{
    #region Properties

    public Neuron[] Neurons { get; }

    #endregion

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        for (var i = 0; i < Neurons.Length; i++)
        {
            stringBuilder.Append($"n{i}[{Neurons[i]}], ");
        }

        return stringBuilder.ToString();
    }

    #region Construction

    public Layer(Neuron[] neurons)
    {
        Neurons = neurons;
    }

    public static Layer Create(IBiasGenerator biasGenerator, int neuronCount)
    {
        var neurons = new Neuron[neuronCount];
        for (var j = 0; j < neuronCount; j++)
        {
            neurons[j] = new Neuron
            {
                Bias = biasGenerator.Generate()
            };
        }

        return new Layer(neurons);
    }

    public static Layer Create(LayerConfig layerConfig)
    {
        var neurons = new Neuron[layerConfig.Neurons.Count];
        for (var j = 0; j < layerConfig.Neurons.Count; j++)
        {
            neurons[j] = new Neuron
            {
                Bias = layerConfig.Neurons[j].Bias
            };
        }

        return new Layer(neurons);
    }

    #endregion

    #region Connection

    public void Connect(IWeightGenerator weightGenerator, Layer layer)
    {
        foreach (var to in layer.Neurons)
        {
            foreach (var from in Neurons)
            {
                from.Connect(to, weightGenerator.Generate());
            }
        }
    }

    public void Connect(Layer layer, LayerConfig layerConfig)
    {
        for (var i = 0; i < Neurons.Count(); i++)
        {
            var fromNeuron = Neurons[i];
            var fromNeuronData = layerConfig.Neurons[i];
            var weights = fromNeuronData.Weights;
            for (var j = 0; j < layer.Neurons.Count(); j++)
            {
                var toNeuron = layer.Neurons[j];
                fromNeuron.Connect(toNeuron, weights[j]);
            }
        }
    }

    #endregion

    #region Activation

    public void Activate(float[] inputs)
    {
        for (var i = 0; i < inputs.Length; i++)
        {
            Neurons[i].Activation = inputs[i];
        }
    }

    public void Activate(IActivationFunction activation)
    {
        foreach (var neuron in Neurons)
        {
            var weightedActivations = neuron.In.Sum(connection => connection.Weight * connection.From.Activation);
            neuron.Activation = activation.Activate(weightedActivations + neuron.Bias);
        }
    }

    #endregion

    #region Equality

    public bool Equals(Layer other)
    {
        return (object)other != null && Neurons.SequenceEqual(other.Neurons);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Neurons);
    }

    public static bool operator ==(Layer b1, Layer b2)
    {
        if ((object)b1 == null)
        {
            return (object)b2 == null;
        }

        return b1.Equals(b2);
    }

    public static bool operator !=(Layer b1, Layer b2)
    {
        return !(b1 == b2);
    }

    #endregion
}