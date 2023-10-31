using System;
using Random = System.Random;

public class NeuralNetwork
{
    private int[] layerSizes;
    private double[][] activations;
    private double[][][] weights;
    private double[][] biases;
    private Func<double, double>[] activationFunctions;

    private Random _random;

    public NeuralNetwork(int[] layerSizes, Func<double, double>[] activationFunctions, int randomSeed)
    {
        this.layerSizes = layerSizes;
        this.activationFunctions = activationFunctions;

        _random = new Random(randomSeed);

        InitializeParameters();
    }

    private void InitializeParameters()
    {
        activations = new double[layerSizes.Length][];
        weights = new double[layerSizes.Length - 1][][];
        biases = new double[layerSizes.Length - 1][];

        for (int i = 0; i < layerSizes.Length; i++)
        {
            activations[i] = new double[layerSizes[i]];

            if (i > 0)
            {
                weights[i - 1] = new double[layerSizes[i]][];
                biases[i - 1] = new double[layerSizes[i]];

                for (int j = 0; j < layerSizes[i]; j++)
                {
                    weights[i - 1][j] = new double[layerSizes[i - 1]];
                    biases[i - 1][j] = 0.0; // Initialize biases to zero

                    for (int k = 0; k < layerSizes[i - 1]; k++)
                    {
                        weights[i - 1][j][k] = _random.NextDouble() - 0.5; // Initialize weights with small random values
                    }
                }
            }
        }
    }

    public void Copy(NeuralNetwork parentNeural)
    {
        Mutate(parentNeural, 0f);
    }

    public void Mutate(NeuralNetwork parentNeural, float mutationFactor)
    {
        for (int i = 0; i < layerSizes.Length; i++)
        {
            if (i > 0)
            {
                for (int j = 0; j < layerSizes[i]; j++)
                {
                    biases[i - 1][j] = parentNeural.biases[i - 1][j] + (_random.NextDouble() - 0.5f) * mutationFactor;

                    for (int k = 0; k < layerSizes[i - 1]; k++)
                    {
                        weights[i - 1][j][k] = parentNeural.weights[i - 1][j][k] + (_random.NextDouble() - 0.5f) * mutationFactor;
                    }
                }
            }
        }
    }

    public double Sigmoid(double x)
    {
        return 1.0 / (1.0 + Math.Exp(-x));
    }

    private double ApplyActivationFunction(double x, int layer)
    {
        return x;
        // return Sigmoid(x);
        // return activationFunctions[layer](x);
    }

    public double[] FeedForward(double[] input)
    {
        // Implement the forward pass of the neural network.
        // Apply activation functions for each layer.
        // Store intermediate activations in the 'activations' array.

        if (input.Length != layerSizes[0])
        {
            throw new ArgumentException("Input size does not match the input layer size.");
        }

        // Set input as the first layer's activations
        activations[0] = input;

        // Perform feedforward for each layer
        for (int layer = 1; layer < layerSizes.Length; layer++)
        {
            for (int neuron = 0; neuron < layerSizes[layer]; neuron++)
            {
                double weightedSum = 0.0;
                for (int prevNeuron = 0; prevNeuron < layerSizes[layer - 1]; prevNeuron++)
                {
                    weightedSum += weights[layer - 1][neuron][prevNeuron] * activations[layer - 1][prevNeuron];
                }
                weightedSum += biases[layer - 1][neuron];

                activations[layer][neuron] = ApplyActivationFunction(weightedSum, layer);
            }
        }

        return activations[layerSizes.Length - 1]; // Return the output layer's activations
    }

    public void Train(double[] input, double[] target, double learningRate)
    {
        // Implement backpropagation to update weights and biases.
        // This is a simplified example and should be replaced with a proper training algorithm.
    }
}