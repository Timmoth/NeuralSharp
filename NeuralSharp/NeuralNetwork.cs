using NeuralSharp.Activation;
using NeuralSharp.Generators;
using NeuralSharp.Serialization;

namespace NeuralSharp;

public sealed class NeuralNetwork
{
    #region Properties

    private readonly float _learningRate = 0.1f;
    public readonly Layer[] Layers;

    #endregion

    #region Construction

    /// <summary>
    ///     Create and connect each layer & neuron
    ///     Given random weights & bias'
    /// </summary>
    /// <param name="weightGenerator"></param>
    /// <param name="biasGenerator"></param>
    /// <param name="activationFunction"></param>
    /// <param name="layers"></param>
    public NeuralNetwork(
        IWeightGenerator weightGenerator,
        IBiasGenerator biasGenerator,
        params int[] layers)
    {
        Layers = new Layer[layers.Length];
        for (var i = 0; i < layers.Length; i++)
        {
            //Create the layer
            Layers[i] = Layer.Create(biasGenerator, layers[i]);
            if (i > 0)
            {
                //Connect the last layer to the current layer
                Layers[i - 1].Connect(weightGenerator, Layers[i]);
            }
        }
    }

    /// <summary>
    ///     Create a neural network from the given network configuration
    /// </summary>
    /// <param name="activationFunction"></param>
    /// <param name="networkConfig"></param>
    public NeuralNetwork(NetworkConfig networkConfig)
    {
        Layers = new Layer[networkConfig.Layers.Length];
        for (var i = 0; i < Layers.Length; i++)
        {
            var layerData = networkConfig.Layers[i];

            //Create the layer from the layer data
            Layers[i] = Layer.Create(layerData);
            if (i > 0)
            {
                //Connect the last layer to the current layer
                Layers[i - 1].Connect(Layers[i], networkConfig.Layers[i - 1]);
            }
        }
    }

    public NeuralNetwork(Layer[] layers)
    {
        Layers = layers;
    }

    #endregion

    #region Learning

    /// <summary>
    ///     Feed the inputs into the network
    /// </summary>
    /// <param name="activationFunction"></param>
    /// <param name="inputs"></param>
    /// <returns>Output neuron activations</returns>
    public float[] FeedForward(IActivationFunction activationFunction, float[] inputs)
    {
        //Activate input layer
        Layers[0].Activate(inputs);

        //Activate each layer of the network
        for (var i = 1; i < Layers.Length; i++)
        {
            Layers[i].Activate(activationFunction);
        }

        //return output activations
        var lastLayerNeurons = Layers[^1].Neurons;
        var activations = new float[lastLayerNeurons.Length];
        for(var i = 0; i <activations.Length; i++)
        {
            activations[i] = lastLayerNeurons[i].Activation;
        }

        return activations;
    }

    /// <summary>
    ///     Train the network
    /// </summary>
    /// <param name="activationFunction"></param>
    /// <param name="inputs"></param>
    /// <param name="expected"></param>
    /// <returns></returns>
    public float[] BackPropagate(IActivationFunction activationFunction, float[] inputs, float[] expected)
    {
        //Feed forward to activate neurons
        var output = FeedForward(activationFunction, inputs);

        //Initialize the error matrix
        var layerErrors = new float[Layers.Length][];
        for (var i = 0; i < Layers.Length; i++)
        {
            layerErrors[i] = new float[Layers[i].Neurons.Length];
        }

        //Calculate the error for the output neurons
        var outputLayer = Layers[^1];
        var outputLayerError = layerErrors[^1];
        for (var i = 0; i < output.Length; i++)
        {
            var outputNeuron = outputLayer.Neurons[i];
            var actualOutput = output[i];
            var error = outputLayerError[i] =
                (actualOutput - expected[i]) * activationFunction.Derivative(actualOutput);

            //Update the bias for the output neuron
            outputNeuron.Adjust(error, _learningRate);
        }

        //runs on all hidden layers
        for (var i = Layers.Length - 2; i > 0; i--)
        {
            var layer = Layers[i];
            for (var j = 0; j < layer.Neurons.Length; j++) //outputs
            {
                var layerError = layerErrors[i];
                var nextLayerErrors = layerErrors[i + 1];
                var neuron = layer.Neurons[j];
                //Calculate the neurons error as the sum of errors x weights for each output
                var error = 0.0f;
                for (var k = 0; k < neuron.Out.Count; k++)
                {
                    error += nextLayerErrors[k] * neuron.Out[k].Weight;
                }

                layerError[j] = error * activationFunction.Derivative(neuron.Activation); //calculate gamma

                //Adjust the bias and weight for the neuron
                layer.Neurons[j].Adjust(layerError[j], _learningRate);
            }
        }

        return output;
    }

    #endregion
}