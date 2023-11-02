using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Single instance of settings to be shared by all Specimen
/// Edited via Unity UI
/// </summary>
[Serializable]
public class SpecimenSettings
{
    public float moveSpeed = 0.2f;
    public float sightRadius = 20;
    public float consumeRadius = 2;

    public float mutationFactor = 0.05f;

    public int[] neuralNetworkLayerSizes = new int[] { 2, 18, 9, 1 };
}

public struct Specimen : IUnit
{
    public bool Valid { get; private set; }
    public int Id { get; private set; }
    public Vector3 Position { get; set; }
    public float Score { get; set; }
    public NeuralNetwork Neural { get; private set; }

    private readonly SpecimenSettings _settings;
    private readonly Map _map;

    public Specimen(int id, SpecimenSettings settings, Map map)
    {
        Valid = true;
        Id = id;

        _settings = settings;
        _map = map;

        Score = default;
        Neural = default;
        Position = Vector3.zero;
    }

    public void InitializeRandom(int randomSeed)
    {
        Neural = new NeuralNetwork(_settings.neuralNetworkLayerSizes, null, randomSeed);
    }

    public void Copy(NeuralNetwork parentNeural)
    {
        InitializeRandom(0);
        Neural.Copy(parentNeural);
    }

    public void Mutate()
    {
        Neural.Mutate(_settings.mutationFactor);
    }

    public void Breed(NeuralNetwork parentY, NeuralNetwork parentX)
    {
        InitializeRandom(0);
        Neural.Breed(parentY, parentX);
    }

    public void CustomUpdate()
    {
        var specimenPosition = Position;

        var closestFood = _map.Food
            .Select((food, i) => (food, i))
            .Where(t => t.food.Valid)
            .OrderBy(t => Vector3.Distance(t.food.Position, specimenPosition))
            .First();

        var targetPosition = closestFood.food.Position - specimenPosition;

        var input = Neural.GetInputArray();

        // input[0] = specimenPosition.x - closestFood.food.Position.x;
        // input[1] = specimenPosition.z - closestFood.food.Position.z;

        var angle = Vector3.SignedAngle(targetPosition, Vector3.right, Vector3.up) / 360f;

        input[0] = angle;

        var outputs = Neural.FeedForward();

        var directionOutput = Id == 1 ? angle : outputs[0];

        var rotation = Quaternion.AngleAxis((float)(360 * directionOutput), Vector3.down);
        var direction = rotation * Vector3.right;
        /*
        if (Id == 1)
            Debug.Log($"ClosestFood: {targetPosition}, Angle: {directionOutput * 360f}, Direction: {direction}");
        */
        Position += direction * _settings.moveSpeed;

        if (Vector3.Distance(closestFood.food.Position, Position) <= _settings.consumeRadius)
        {
            _map.Food[closestFood.i].Destroy();
            Score += 50;
        }
    }

    public void Destroy()
    {
        Valid = false;
    }

    public void Reactivate()
    {
        Valid = true;
    }
}
