using System;
using Moq;
using NeuralSharp;
using NeuralSharp.Generators;
using NeuralSharp.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Tests.Network;

public class NeuralNetworkSerializationTests
{
    [Fact]
    public void SingleLayerSingleNeuronNetworkSerialization()
    {
        //Arrange
        var mockWeightGenerator = new Mock<IWeightGenerator>();
        mockWeightGenerator.Setup(i => i.Generate()).Returns(1);
        var mockBiasGenerator = new Mock<IBiasGenerator>();
        mockBiasGenerator.Setup(i => i.Generate()).Returns(1);
        var network =
            new NeuralNetwork(mockWeightGenerator.Object, mockBiasGenerator.Object, 1);

        //Act
        var networkData = NetworkConfig.From(network);
        var serializedNetwork = JsonSerializer.Serialize(networkData);

        //Assert
        var expectedJson = JObject.FromObject(new
        {
            Layers = new[]
            {
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 1,
                            Weights = new float[] { }
                        }
                    }
                }
            }
        }).ToString(Formatting.None);

        Assert.Equal(expectedJson, serializedNetwork);
    }

    [Fact]
    public void DoubleLayerDoubleNeuronNetworkSerialization()
    {
        //Arrange
        var mockWeightGenerator = new Mock<IWeightGenerator>();
        mockWeightGenerator.Setup(i => i.Generate()).Returns(1);
        var mockBiasGenerator = new Mock<IBiasGenerator>();
        mockBiasGenerator.Setup(i => i.Generate()).Returns(1);
        var network =
            new NeuralNetwork(mockWeightGenerator.Object, mockBiasGenerator.Object, 2, 2);

        //Act
        var networkData = NetworkConfig.From(network);
        var serializedNetwork = JsonSerializer.Serialize(networkData);

        //Assert
        var expectedJson = JObject.FromObject(new
        {
            Layers = new[]
            {
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 1,
                            Weights = new[] { 1, 1 }
                        },
                        new
                        {
                            Bias = 1,
                            Weights = new[] { 1, 1 }
                        }
                    }
                },
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 1,
                            Weights = Array.Empty<int>()
                        },
                        new
                        {
                            Bias = 1,
                            Weights = Array.Empty<int>()
                        }
                    }
                }
            }
        }).ToString(Formatting.None);

        Assert.Equal(expectedJson, serializedNetwork);
    }

    [Fact]
    public void DoubleLayerDoubleNeuronNetworkDeserialization()
    {
        //Arrange
        var mockWeightGenerator = new Mock<IWeightGenerator>();
        mockWeightGenerator.Setup(i => i.Generate()).Returns(1);
        var mockBiasGenerator = new Mock<IBiasGenerator>();
        mockBiasGenerator.Setup(i => i.Generate()).Returns(1);

        var network =
            new NeuralNetwork(mockWeightGenerator.Object, mockBiasGenerator.Object, 2, 2);

        //Act
        var networkData = NetworkConfig.From(network);
        var serializedNetwork = JsonSerializer.Serialize(networkData);

        //Assert
        Assert.Equal(
            "{\"Layers\":[{\"Neurons\":[{\"Bias\":1,\"Weights\":[1,1]},{\"Bias\":1,\"Weights\":[1,1]}]},{\"Neurons\":[{\"Bias\":1,\"Weights\":[]},{\"Bias\":1,\"Weights\":[]}]}]}",
            serializedNetwork);
    }
}