using System.Collections.Generic;
using Moq;
using NeuralNetwork.Activation;
using NeuralNetwork.Serialization;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Tests.Network;

public class NeuralNetworkBackPropagateTests
{
    [Fact]
    public void BackPropagateOn1nNetwork()
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

        var network = new NeuralNetwork.NeuralNetwork(networkData);

        var input = new[] { 0.1f };
        var expected = new[] { 0.1f };

        //Act
        var actualOutput = network.BackPropagate(mockActivator.Object, input, expected);

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
        mockActivator.Setup(i => i.Derivative(It.IsAny<float>())).Returns<float>(x => x * 2);

        var network = new NeuralNetwork.NeuralNetwork(networkData);
        var input = new[] { 0.1f, 0.8f };
        var expected = new[] { 0.5f };

        //Act
        var actualOutput = network.BackPropagate(mockActivator.Object, input, expected);

        //Assert
        Assert.Equal(new[] { 0.25f }, actualOutput);
        Assert.Equal(new NeuronData(0.2f, new List<float> { 0.40125f }), network.Layers[0].Neurons[0].Data);
        Assert.Equal(new NeuronData(0.3f, new List<float> { 0.210000008f }), network.Layers[0].Neurons[1].Data);
        Assert.Equal(new NeuronData(0.3125f, new List<float>()), network.Layers[1].Neurons[0].Data);
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
        mockActivator.Setup(i => i.Derivative(It.IsAny<float>())).Returns<float>(x => x * 2);

        var network = new NeuralNetwork.NeuralNetwork(networkData);
        var input = new[] { 0.1f, 0.8f };
        var expected = new[] { 0.45f, 0.82f };

        //Act
        var firstIterationOutput = network.BackPropagate(mockActivator.Object, input, expected);
        var firstIterationNetwork = NetworkConfig.From(network);

        var secondIterationOutput = network.BackPropagate(mockActivator.Object, input, expected);
        var secondIterationNetwork = NetworkConfig.From(network);

        //Assert
        Assert.Equal(new[] { 0.38625f, 0.63375f }, firstIterationOutput);
        Assert.Equal(new[] { 0.39490512f, 0.659816444f }, secondIterationOutput);

        var expectedFirstIterationNetwork = JObject.FromObject(new
        {
            Layers = new[]
            {
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 1f,
                            Weights = new[]
                            {
                                0.4004133f,
                                0.3013415f,
                                0.10110168f
                            }
                        },
                        new
                        {
                            Bias = 1f,
                            Weights = new[]
                            {
                                0.20330645f,
                                0.910732f,
                                0.30881348f
                            }
                        }
                    }
                },
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 0.70413303f,
                            Weights = new[]
                            {
                                0.4022161f,
                                0.11062323f
                            }
                        },
                        new
                        {
                            Bias = 0.61341506f,
                            Weights = new[]
                            {
                                0.50332415f,
                                0.31593487f
                            }
                        },
                        new
                        {
                            Bias = 0.31101686f,
                            Weights = new[]
                            {
                                0.2013543f,
                                0.806492f
                            }
                        }
                    }
                },
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 0.20492469f,
                            Weights = new float[] { }
                        },
                        new
                        {
                            Bias = 0.8236072f,
                            Weights = new float[] { }
                        }
                    }
                }
            }
        }).ToObject<NetworkConfig>();

        var expectedSecondIterationNetwork = JObject.FromObject(new
        {
            Layers = new[]
            {
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 1f,
                            Weights = new[]
                            {
                                0.4008032f,
                                0.30260223f,
                                0.102127604f
                            }
                        },
                        new
                        {
                            Bias = 1f,
                            Weights = new[]
                            {
                                0.20642576f,
                                0.9208178f,
                                0.3170209f
                            }
                        }
                    }
                },
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 0.7080322f,
                            Weights = new[]
                            {
                                0.4041891f,
                                0.12020756f
                            }
                        },
                        new
                        {
                            Bias = 0.62602234f,
                            Weights = new[]
                            {
                                0.5063095f,
                                0.33043718f
                            }
                        },
                        new
                        {
                            Bias = 0.3212761f,
                            Weights = new[]
                            {
                                0.2025905f,
                                0.81249714f
                            }
                        }
                    }
                },
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 0.20927614f,
                            Weights = new float[] { }
                        },
                        new
                        {
                            Bias = 0.8447456f,
                            Weights = new float[] { }
                        }
                    }
                }
            }
        }).ToObject<NetworkConfig>();

        Assert.Equal(expectedFirstIterationNetwork, firstIterationNetwork);
        Assert.Equal(expectedSecondIterationNetwork, secondIterationNetwork);
    }
}