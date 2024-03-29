﻿using System.Text.Json;
using NeuralSharp.Serialization;

namespace NeuralSharp.Genetic;

public class FileNetworkIo : INeuralNetworkIo
{
    private readonly string _filename;

    public FileNetworkIo(string filename)
    {
        _filename = filename;
    }

    public async Task<NetworkConfig?> Load()
    {
        try
        {
            return JsonSerializer.Deserialize<NetworkConfig>(await File.ReadAllTextAsync(_filename));
        }
        catch
        {
            return null;
        }
    }

    public async Task Save(NetworkConfig network)
    {
        await File.WriteAllTextAsync(_filename, JsonSerializer.Serialize(network));
    }
}