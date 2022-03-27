using FluentAssertions;
using Xunit;

namespace Tests.Network.Layer.Neuron;

public class NeuronTests
{
    [Theory]
    [InlineData(10, 5, 5, -15)]
    public void NeuronAdjustUpdatesBias(float bias, float error, float learningRate, float expectedBias)
    {
        //Arrange
        var n = new NeuralSharp.Neuron { Activation = 1, Bias = bias };

        //Act
        n.Adjust(error, learningRate);

        //Assert
        n.Bias.Should().Be(expectedBias);
    }
}