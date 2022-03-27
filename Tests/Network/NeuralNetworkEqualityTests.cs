using Moq;
using NeuralNetwork.Activation;
using NeuralNetwork.Generators;
using Xunit;

namespace Tests.Network;

public class NeuralNetworkEqualityTests
{
    [Fact]
    public void NeuralNetworkSerialization()
    {
        //Arrange
        var mockWeightGenerator = new Mock<IWeightGenerator>();
        mockWeightGenerator.Setup(i => i.Generate()).Returns(1);
        var mockBiasGenerator = new Mock<IBiasGenerator>();
        mockBiasGenerator.Setup(i => i.Generate()).Returns(1);
        var nA = new NeuralNetwork.NeuralNetwork(mockWeightGenerator.Object, mockBiasGenerator.Object,
            0);
        var nB = new NeuralNetwork.NeuralNetwork(mockWeightGenerator.Object, mockBiasGenerator.Object, 
            0);
        var nC = new NeuralNetwork.NeuralNetwork(mockWeightGenerator.Object, mockBiasGenerator.Object,
            1);
        var nD = new NeuralNetwork.NeuralNetwork(mockWeightGenerator.Object, mockBiasGenerator.Object, 
            1);
        var nE = new NeuralNetwork.NeuralNetwork(mockWeightGenerator.Object, mockBiasGenerator.Object,  1,
            1);
        var nF = new NeuralNetwork.NeuralNetwork(mockWeightGenerator.Object, mockBiasGenerator.Object, 1,
            1);

        //Act
        //Assert
        Assert.True(nA == nA); //0 Layers 0 Neurons
        Assert.True(nA == nB); //0 Layers 0 Neurons
        Assert.True(nC == nD); //1 Layers 1 Neurons
        Assert.True(nE == nF); //2 Layers 1 Neurons

        Assert.False(nA == nC); //Different Layers Different Neurons 
        Assert.False(nC == nE); //
    }
}