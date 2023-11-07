using System;
using UnityEngine;
using Random = System.Random;

public interface INeural
{
    public Guid Id { get; }

    public void InitializeRandom();
    public void Copy(INeural neural);
    public void Mutate(float mutationFactor);
    public void Breed(INeural neuralX, INeural neuralY);

    public double[] GetInputArray();
    public double[] FeedForward();
}
