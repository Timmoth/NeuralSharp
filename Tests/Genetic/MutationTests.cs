using Moq;
using NeuralSharp;
using NeuralSharp.Genetic;
using Xunit;

namespace Tests.Genetic;

public class MutationTests
{
    [Fact]
    public void NetworkMutator2n1nTest()
    {
        //Arrange
        var network = NeuralNetwork.From(new
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
                            Weights = new[] { 0.7f }
                        },
                        new
                        {
                            Bias = 0.6f,
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
                            Bias = 0.9f,
                            Weights = new float[] { }
                        }
                    }
                }
            }
        });

        var fitnessFunction = (NeuralNetwork n) => 0.1f;
        var mockMutationDecider = new Mock<IMutationDecider>();
        mockMutationDecider
            .Setup(m => m.ShouldMutate(It.IsAny<float>()))
            .Returns<float>(f => f > 0.5);

        var mockMutator = new Mock<IFloatMutator>();
        mockMutator
            .Setup(m => m.Mutate(It.IsAny<float>()))
            .Returns<float>(f => f + 0.1f);

        var mutation = new NetworkMutator(mockMutationDecider.Object, mockMutator.Object);

        //Act
        var mutatedNetwork = mutation.Mutate(network);

        //Assert
        var expectedNetwork = NeuralNetwork.From(new
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
                            Weights = new[] { 0.8f }
                        },
                        new
                        {
                            Bias = 0.7f,
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
                            Bias = 1.0f,
                            Weights = new float[] { }
                        }
                    }
                }
            }
        });

        Assert.True(expectedNetwork == mutatedNetwork);
    }
}