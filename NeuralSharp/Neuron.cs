using NeuralSharp.Helpers;
using NeuralSharp.Serialization;

namespace NeuralSharp;

public sealed class Neuron : IEquatable<Neuron>
{
    public Neuron()
    {
        In = new List<Connection>();
        Out = new List<Connection>();
        Bias = 0;
        Activation = 0.0f;
    }

    public NeuronData Data => new(Bias, Out.Select(c => c.Weight).ToArray());

    public override string ToString()
    {
        return $"a{Activation:0.00} b{Bias:0.00}";
    }

    public void Adjust(float error, float learningRate)
    {
        //Update the bias
        Bias -= error * learningRate;

        for (int i = 0; i < In.Count; i++)
        {
            Connection? connection = In[i];
            //Update the weight for each connection going into the output neuron
            connection.Weight -= error * connection.From.Activation * learningRate;
        }
    }

    #region Properties

    public float Bias { get; set; }
    public float Activation { get; set; }
    public List<Connection> In { get; set; }
    public List<Connection> Out { get; set; }

    #endregion

    #region Equality

    public override bool Equals(object obj)
    {
        return Equals(obj as Neuron);
    }

    public bool Equals(Neuron other)
    {
        return (object)other != null &&
               Math.Abs(Bias - other.Bias) < Constants.Tolerance &&
               Math.Abs(Activation - other.Activation) < Constants.Tolerance &&
               Out.SequenceEqual(other.Out, new ConnectionEquality());
    }

    private class ConnectionEquality : IEqualityComparer<Connection>
    {
        public bool Equals(Connection x, Connection y)
        {
            if (x == y)
            {
                return true;
            }


            return x.To.Equals(y.To) && Math.Abs(x.Weight - y.Weight) < Constants.Tolerance;
        }

        public int GetHashCode(Connection obj)
        {
            return HashCode.Combine(obj.To, obj.Weight);
        }
    }

    public static bool operator ==(Neuron b1, Neuron b2)
    {
        if ((object)b1 == null)
        {
            return (object)b2 == null;
        }

        return b1.Equals(b2);
    }

    public static bool operator !=(Neuron b1, Neuron b2)
    {
        return !(b1 == b2);
    }

    #endregion
}