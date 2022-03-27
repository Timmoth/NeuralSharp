using System.Collections.Generic;
using NeuralNetwork;
using NeuralNetwork.Helpers;
using Xunit;

namespace Tests.Network.Layer.Neuron;

public class NeuronEqualityTests
{
    [Fact]
    public void NeuronEquality()
    {
        //Arrange
        var n1 = new NeuralNetwork.Neuron { Activation = 1, Bias = 0.1f, Out = new List<Connection>() };
        var n2 = new NeuralNetwork.Neuron { Activation = 1, Bias = 0.1f, Out = new List<Connection>() };
        var n3 = new NeuralNetwork.Neuron { Activation = 1, Bias = 0.1f, Out = new List<Connection>() };
        var n4 = new NeuralNetwork.Neuron { Activation = 1, Bias = 0.2f, Out = new List<Connection>() };
        var n5 = new NeuralNetwork.Neuron { Activation = 1, Bias = 0.1f, Out = new List<Connection>() };

        var n6 = new NeuralNetwork.Neuron { Activation = 1, Bias = 0.3f, Out = new List<Connection>() };
        n1.Connect(n6, 1);
        n2.Connect(n6, 1);
        n3.Connect(n6, 2);
        n4.Connect(n6, 1);


        //Act
        //Assert
        Assert.True(n1 == n1); //Same Neuron
        Assert.True(n1 == n2); //Same Bias Same Connection Weight
        Assert.False(n1 == n3); //Same Bias Different Connection Weight
        Assert.False(n1 == n4); //Same Connection Different Bias
        Assert.False(n1 == n5); //Same Bias No Connection
    }
}