using Moq;
using NeuralSharp.Generators;
using Xunit;

namespace Tests.Network.Layer;

public class LayerEqualityTests
{
    [Fact]
    public void LayerEquality()
    {
        //Arrange
        var mockWeightGenerator = new Mock<IWeightGenerator>();
        mockWeightGenerator.Setup(i => i.Generate()).Returns(1);
        var mockBiasGenerator = new Mock<IBiasGenerator>();
        mockBiasGenerator.Setup(i => i.Generate()).Returns(1);

        var outLayer = NeuralSharp.Layer.Create(mockBiasGenerator.Object, 1);

        var l1 = NeuralSharp.Layer.Create(mockBiasGenerator.Object, 1);
        l1.Connect(mockWeightGenerator.Object, outLayer);

        var l2 = NeuralSharp.Layer.Create(mockBiasGenerator.Object, 1);
        l2.Connect(mockWeightGenerator.Object, outLayer);

        var l3 = NeuralSharp.Layer.Create(mockBiasGenerator.Object, 1);

        var l4 = NeuralSharp.Layer.Create(mockBiasGenerator.Object, 1);
        mockWeightGenerator.Setup(i => i.Generate()).Returns(2);
        l4.Connect(mockWeightGenerator.Object, outLayer);

        //Act
        //Assert
        Assert.True(l1 == l1); //Same Layer
        Assert.True(l1 == l2); //Same Neurons Same Connections
        Assert.False(l1 == l3); //Same Neurons No Connections
        Assert.False(l1 == l4); //Same Neurons Same Connections Different Weights
    }
}