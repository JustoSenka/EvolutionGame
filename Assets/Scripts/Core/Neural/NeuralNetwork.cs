using System;
using UnityEngine;
using Random = System.Random;

public class NeuralNetwork
{
    public string Id { get; private set; }

    public int[] layerSizes;
    public double[][][] weights;
    public double[][] biases;

    [NonSerialized]
    private double[][] _activations;

    [NonSerialized]
    private Func<double, double>[] _activationFunctions;

    [NonSerialized]
    private readonly Random _random;

    public NeuralNetwork(int[] layerSizes, Func<double, double>[] activationFunctions, int randomSeed)
    {
        Id = Guid.NewGuid().ToString();

        this.layerSizes = layerSizes;
        this._activationFunctions = activationFunctions;

        _random = new Random(randomSeed);

        InitializeParameters();
    }

    private void InitializeParameters()
    {
        _activations = new double[layerSizes.Length][];
        weights = new double[layerSizes.Length - 1][][];
        biases = new double[layerSizes.Length - 1][];

        for (int i = 0; i < layerSizes.Length; i++)
        {
            _activations[i] = new double[layerSizes[i]];

            if (i > 0)
            {
                weights[i - 1] = new double[layerSizes[i]][];
                biases[i - 1] = new double[layerSizes[i]];

                for (int j = 0; j < layerSizes[i]; j++)
                {
                    weights[i - 1][j] = new double[layerSizes[i - 1]];
                    biases[i - 1][j] = _random.NextDouble() * 2 - 1f; // Initialize biases to zero

                    for (int k = 0; k < layerSizes[i - 1]; k++)
                    {
                        weights[i - 1][j][k] = _random.NextDouble() * 2 - 1f; // Initialize weights with small random values
                    }
                }
            }
        }
    }

    public void Copy(NeuralNetwork parentNeural)
    {
        Id = parentNeural.Id;

        for (int i = 0; i < layerSizes.Length; i++)
        {
            if (i > 0)
            {
                for (int j = 0; j < layerSizes[i]; j++)
                {
                    biases[i - 1][j] = parentNeural.biases[i - 1][j];

                    for (int k = 0; k < layerSizes[i - 1]; k++)
                    {
                        weights[i - 1][j][k] = parentNeural.weights[i - 1][j][k];
                    }
                }
            }
        }
    }

    public void Mutate(float mutationFactor)
    {
        Id = Guid.NewGuid().ToString();

        for (int i = 0; i < layerSizes.Length; i++)
        {
            if (i > 0)
            {
                for (int j = 0; j < layerSizes[i]; j++)
                {
                    biases[i - 1][j] = biases[i - 1][j] + (_random.NextDouble() - 0.5f) * mutationFactor;

                    for (int k = 0; k < layerSizes[i - 1]; k++)
                    {
                        weights[i - 1][j][k] = Mathf.Clamp((float)(weights[i - 1][j][k] + (_random.NextDouble() - 0.5f) * mutationFactor), -1, 1);
                    }
                }
            }
        }
    }

    public void Breed(NeuralNetwork parentX, NeuralNetwork parentY)
    {
        Id = Guid.NewGuid().ToString();

        for (int i = 0; i < layerSizes.Length; i++)
        {
            if (i > 0)
            {
                for (int j = 0; j < layerSizes[i]; j++)
                {
                    biases[i - 1][j] = (parentX.biases[i - 1][j] + parentY.biases[i - 1][j]) / 2;

                    for (int k = 0; k < layerSizes[i - 1]; k++)
                    {
                        weights[i - 1][j][k] = (parentX.weights[i - 1][j][k] + parentY.weights[i - 1][j][k]) / 2;
                    }
                }
            }
        }
    }

    public double Sigmoid(double x) => 1.0 / (1.0 + Math.Exp(-x));
    public double Tanh(double x) => Math.Tanh(x);
    public double ReLU(double x) => Math.Max(0, x);

    private double ApplyActivationFunction(double x, int layer)
    {
        if (layer == layerSizes.Length - 1)
            return x;
        
        return ReLU(x);
        // return activationFunctions[layer](x);
    }

    public double[] GetInputArray()
    {
        return _activations[0];
    }

    public double[] FeedForward()
    {
        // Perform feedforward for each layer
        for (int layer = 1; layer < layerSizes.Length; layer++)
        {
            for (int neuron = 0; neuron < layerSizes[layer]; neuron++)
            {
                double weightedSum = 0.0;
                for (int prevNeuron = 0; prevNeuron < layerSizes[layer - 1]; prevNeuron++)
                {
                    weightedSum += weights[layer - 1][neuron][prevNeuron] * _activations[layer - 1][prevNeuron];
                }
                weightedSum += biases[layer - 1][neuron];

                _activations[layer][neuron] = ApplyActivationFunction(weightedSum, layer);
            }
        }

        return _activations[layerSizes.Length - 1]; // Return the output layer's activations
    }

    public void Train(double[] input, double[] target, double learningRate)
    {
        // Implement backpropagation to update weights and biases.
        // This is a simplified example and should be replaced with a proper training algorithm.
    }
}
