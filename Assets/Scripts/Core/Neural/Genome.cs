using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Genome : INeural
{
    public Guid Id { get; private set; }

    public List<NodeGene> Nodes { get; }
    public List<ConnectionGene> Connections { get; }

    private readonly double[] _input;
    private readonly double[] _output;
    private readonly Random _random;

    public double[] GetInputArray() => _input;

    public Genome(int inputNodeCount, int outputNodeCount, int randomSeed)
    {
        Id = Guid.NewGuid();

        _random = new Random(randomSeed);
        _input = new double[inputNodeCount];
        _output = new double[outputNodeCount];

        Nodes = new List<NodeGene>();
        Connections = new List<ConnectionGene>();
    }

    public void InitializeRandom()
    {
        for (int i = 0; i < _input.Length; i++)
            Nodes.Add(new NodeGene(Nodes.Count, NodeType.Input, ActivationFunctionType.None, 1.0, 0.0));

        for (int i = 0; i < _output.Length; i++)
            Nodes.Add(new NodeGene(Nodes.Count, NodeType.Output, ActivationFunctionType.None, 1.0, 0.0));

        for (int fromNodeId = 0; fromNodeId < _input.Length; fromNodeId++)
        {
            for (int toNodeId = _input.Length; toNodeId < _input.Length + _output.Length; toNodeId++)
            {
                if (_random.NextDouble() <= 1) // Connects to every output node
                {
                    Connections.Add(new ConnectionGene(Connections.Count, fromNodeId, toNodeId, (_random.NextDouble() * 2.0) - 1.0));
                }
            }
        }
    }

    public double[] FeedForward()
    {
        for (int i = 0; i < Nodes.Count; i++)
            Nodes[i].SetValue(0);

        for (int i = 0; i < _input.Length; i++)
            Nodes[i].SetValue(_input[i]);

        for (int nodeIndex = 0; nodeIndex < Nodes.Count; nodeIndex++)
        {
            if (Nodes[nodeIndex].Type == NodeType.Input)
                continue;

            var weightedSum = 0.0d;
            for (int connectionIndex = 0; connectionIndex < Connections.Count; connectionIndex++)
            {
                if (!Connections[connectionIndex].IsEnabled || Connections[connectionIndex].ToNodeId != Nodes[nodeIndex].Id)
                    continue;

                var sourceNode = Nodes.Find(n => n.Id == Connections[connectionIndex].FromNodeId);
                weightedSum += sourceNode.Value * Nodes[nodeIndex].ActivationResponse * Connections[connectionIndex].Weight;
            }

            weightedSum += Nodes[nodeIndex].Bias;

            Nodes[nodeIndex].Activate(weightedSum);
        }

        int outputIndex = 0;
        for (int nodeId = 0; nodeId < Nodes.Count; nodeId++)
        {
            if (Nodes[nodeId].Type != NodeType.Output)
                continue;

            _output[outputIndex++] = Nodes[nodeId].Value;
        }

        return _output;
    }

    public void Copy(INeural neural)
    {
        var genome = (Genome)neural;

        Id = genome.Id;
        Nodes.Clear();
        Connections.Clear();

        for (int i = 0; i < genome.Nodes.Count; i++)
            Nodes.Add(new NodeGene(
                genome.Nodes[i].Id, 
                genome.Nodes[i].Type, 
                genome.Nodes[i].ActivationFunction, 
                genome.Nodes[i].ActivationResponse, 
                genome.Nodes[i].Bias));

        for (int i = 0; i < genome.Connections.Count; i++)
            Connections.Add(new ConnectionGene(
                genome.Connections[i].InnovationNumber,
                genome.Connections[i].FromNodeId,
                genome.Connections[i].ToNodeId,
                genome.Connections[i].Weight,
                genome.Connections[i].IsEnabled));
    }

    public void Breed(INeural neuralX, INeural neuralY)
    {
        throw new NotImplementedException();
    }

    public void Mutate(float mutationFactor)
    {
        Id = Guid.NewGuid();

        var mutationType = _random.NextDouble();

        if (mutationType < 0.1)
            AddNewNodeMutation();

        else if (mutationType < 0.2)
            AddNewConnectionMutation(mutationFactor);

        else if (mutationType < 0.75)
            ModifyWeightsMutation(mutationFactor);

        else
            ModifyBiasMutation(mutationFactor);
    }

    private void AddNewNodeMutation()
    {
        int connectionIndex;
        do
        {
            connectionIndex = _random.Next(Connections.Count - 1);
        } while (!Connections[connectionIndex].IsEnabled);

        Connections[connectionIndex].SetIsEnabled(false);

        int newNodeId = GetNextNodeId();
        int newConnection1Id = GetNextConnectionId();
        int newConnection2Id = GetNextConnectionId();

        var newNode = new NodeGene(newNodeId, NodeType.Hidden, ActivationFunctionType.None, 1.0, 0.0);
        var newConnection1 = new ConnectionGene(newConnection1Id, Connections[connectionIndex].FromNodeId, newNodeId, 1.0);
        var newConnection2 = new ConnectionGene(newConnection2Id, newNodeId, Connections[connectionIndex].ToNodeId, Connections[connectionIndex].Weight);

        Nodes.Insert(Nodes.Count - _output.Length, newNode); // Insert as a last hidden node
        Connections.Add(newConnection1);
        Connections.Add(newConnection2);
    }

    private void AddNewConnectionMutation(double mutationFactor)
    {
        int node1Index = _random.Next(Nodes.Count - _output.Length - 1);
        int node2Index = _random.Next(node1Index + 1, Nodes.Count - 1);

        int fromNodeId = Nodes[node1Index].Id;
        int toNodeId = Nodes[node2Index].Id;

        if (Connections.Exists(conn => conn.FromNodeId == fromNodeId && conn.ToNodeId == toNodeId))
            return;

        int newConnectionId = GetNextConnectionId();
        var newConnection = new ConnectionGene(newConnectionId, fromNodeId, toNodeId, (_random.NextDouble() * 2 - 1) * mutationFactor);

        Connections.Add(newConnection);
    }

    private void ModifyWeightsMutation(double mutationFactor)
    {
        var connectionId = _random.Next(Connections.Count - 1);
        var currentWeight = Connections[connectionId].Weight;
        var newWeight = currentWeight + (_random.NextDouble() * 2 - 1) * mutationFactor;
        Connections[connectionId].SetWeight(newWeight);
    }

    private void ModifyBiasMutation(double mutationFactor)
    {
        var nodeId = _random.Next(_input.Length, Nodes.Count - 1); // Skipping input nodes as they do not have bias
        var currentBias = Nodes[nodeId].Bias;
        var newBias = currentBias + (_random.NextDouble() * 2 - 1) * mutationFactor;
        Nodes[nodeId].SetBias(newBias);
    }

    private int GetNextNodeId()
    {
        int nextNodeId = 0;
        for (int nodeId = 0; nodeId < Nodes.Count; nodeId++)
        {
            if (Nodes[nodeId].Id >= nextNodeId)
                nextNodeId = Nodes[nodeId].Id + 1;
        }
        return nextNodeId;
    }

    private int GetNextConnectionId()
    {
        int nextConnectionId = 0;
        for (int connectionId = 0; connectionId < Connections.Count; connectionId++)
        {
            if (Connections[connectionId].InnovationNumber >= nextConnectionId)
                nextConnectionId = Connections[connectionId].InnovationNumber + 1;
        }
        return nextConnectionId;
    }
}

[Serializable]
public class NodeGene
{
    public int Id { get; }
    public NodeType Type { get; }
    public ActivationFunctionType ActivationFunction { get; }
    public double ActivationResponse { get; private set; }
    public double Bias { get; private set; }
    public double Value { get; private set; }

    public NodeGene(int id, NodeType type, ActivationFunctionType activationFunction, double activationResponse, double bias)
    {
        Id = id;
        Type = type;
        ActivationResponse = activationResponse;
        ActivationFunction = activationFunction;
        Bias = bias;
        Value = 0.0;
    }

    private double Sigmoid(double x) => 1.0 / (1.0 + Math.Exp(-x));
    private double Tanh(double x) => Math.Tanh(x);
    private double ReLU(double x) => Math.Max(0, x);

    public void Activate(double value)
    {
        Value = ActivationFunction switch
        {
            ActivationFunctionType.Sigmoid => Sigmoid(value),
            ActivationFunctionType.Tanh => Tanh(value),
            ActivationFunctionType.ReLU => ReLU(value),
            ActivationFunctionType.None => value,
            _ => value
        };
    }

    public void SetValue(double value)
    {
        Value = value;
    }

    public void SetBias(double bias)
    {
        Bias = bias;
    }

    public void SetActivationResponse(double activationResponse)
    {
        ActivationResponse = activationResponse;
    }
}

[Serializable]
public class ConnectionGene
{
    public int InnovationNumber { get; }
    public int FromNodeId { get; }
    public int ToNodeId { get; }
    public double Weight { get; private set; }
    public bool IsEnabled { get; private set; }

    public ConnectionGene(int innovationNumber, int fromNodeId, int toNodeId, double weight, bool isEnabled = true)
    {
        InnovationNumber = innovationNumber;
        FromNodeId = fromNodeId;
        ToNodeId = toNodeId;
        Weight = weight;
        IsEnabled = isEnabled;
    }

    public void SetWeight(double weight)
    {
        Weight = weight;
    }

    public void SetIsEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}

[Serializable]
public enum NodeType
{
    Input,
    Output,
    Hidden
}

[Serializable]
public enum ActivationFunctionType
{
    Sigmoid,
    Tanh,
    ReLU,
    None
}
