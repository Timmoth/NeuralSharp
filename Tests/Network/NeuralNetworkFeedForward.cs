using Moq;
using NeuralSharp;
using NeuralSharp.Activation;
using NeuralSharp.Serialization;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Tests.Network;

public class NeuralNetworkFeedForward
{
    [Fact]
    public void FeedForwardOn1nNetwork()
    {
        //Arrange
        var networkData = JObject.FromObject(new
        {
            Layers = new[]
            {
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 0.3,
                            Weights = new float[] { }
                        }
                    }
                }
            }
        }).ToObject<NetworkConfig>();

        var mockActivator = new Mock<IActivationFunction>();
        mockActivator.Setup(i => i.Activate(It.IsAny<float>())).Returns<float>(x => x / 0.5f);

        var network = new NeuralNetwork(networkData);

        var input = new[] { 0.1f };

        //Act
        var actualOutput = network.FeedForward(mockActivator.Object, input);

        //Assert
        Assert.Equal(new[] { 0.1f }, actualOutput);
    }

    [Fact]
    public void FeedForwardOn2n1nNetwork()
    {
        //Arrange
        var networkData = JObject.FromObject(new
        {
            Layers = new[]
            {
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 0.2f,
                            Weights = new[] { 0.4f }
                        },
                        new
                        {
                            Bias = 0.3f,
                            Weights = new[] { 0.2f }
                        }
                    }
                },
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 0.3f,
                            Weights = new float[] { }
                        }
                    }
                }
            }
        }).ToObject<NetworkConfig>();

        var mockActivator = new Mock<IActivationFunction>();
        mockActivator.Setup(i => i.Activate(It.IsAny<float>())).Returns<float>(x => x / 2);

        var network = new NeuralNetwork(networkData);
        var input = new[] { 0.1f, 0.8f };
        //Act
        var actualOutput = network.FeedForward(mockActivator.Object, input);

        //Assert
        Assert.Equal(new[] { 0.25f }, actualOutput);
    }

    [Fact]
    public void FeedForwardOn2n3n2nNetwork()
    {
        //Arrange
        var networkData = JObject.FromObject(new
        {
            Layers = new[]
            {
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 1.0f,
                            Weights = new[] { 0.4f, 0.3f, 0.1f }
                        },
                        new
                        {
                            Bias = 1.0f,
                            Weights = new[] { 0.2f, 0.9f, 0.3f }
                        }
                    }
                },
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 0.7f,
                            Weights = new[] { 0.4f, 0.1f }
                        },
                        new
                        {
                            Bias = 0.6f,
                            Weights = new[] { 0.5f, 0.3f }
                        },
                        new
                        {
                            Bias = 0.3f,
                            Weights = new[] { 0.2f, 0.8f }
                        }
                    }
                },
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 0.2f,
                            Weights = new float[] { }
                        },
                        new
                        {
                            Bias = 0.8f,
                            Weights = new float[] { }
                        }
                    }
                }
            }
        }).ToObject<NetworkConfig>();

        var mockActivator = new Mock<IActivationFunction>();
        mockActivator.Setup(i => i.Activate(It.IsAny<float>())).Returns<float>(x => x / 2);

        var network = new NeuralNetwork(networkData);
        var input = new[] { 0.1f, 0.8f };
        //Act
        var actualOutput = network.FeedForward(mockActivator.Object, input);

        //Assert
        Assert.Equal(new[] { 0.38625f, 0.63375f }, actualOutput);
    }
}