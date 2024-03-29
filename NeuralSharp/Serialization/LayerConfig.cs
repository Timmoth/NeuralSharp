﻿namespace NeuralSharp.Serialization;

public sealed record LayerConfig(NeuronData[] Neurons)
{
    public bool Equals(LayerConfig other)
    {
        return other != null && Neurons.SequenceEqual(other.Neurons);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Neurons);
    }
}