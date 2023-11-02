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
    public float consumeRadius = 0.2f;

    public float mutationFactor = 0.05f;

    public int[] neuralNetworkLayerSizes = new int[] { 2, 18, 9, 1 };
}

[Serializable]
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
        var input = Neural.GetInputArray();

        var specimenPosition = Position;

        var closestFood = _map.Food
            .Select((food, id) => (food, id))
            .Where(t => t.food.Valid)
            .OrderBy(t => Vector3.SqrMagnitude(t.food.Position - specimenPosition))
            .Take(input.Length);

        (Food food, int i) firstFood = default;
        int index = 0;
        foreach(var (food, id) in closestFood)
        {
            if (index == 0)
                firstFood = (food, id);

            input[index++] = Vector3.SignedAngle(food.Position - specimenPosition, Vector3.right, Vector3.up) / 360f;
        }

        var outputs = Neural.FeedForward();

        var targetAngle = (float) outputs[0] * 360f;

        var rotation = Quaternion.AngleAxis(targetAngle, Vector3.down);
        var direction = rotation * Vector3.right;

        Position += direction * _settings.moveSpeed;

        ConsumeFood(firstFood);
    }

    public void CustomUpdateSimple()
    {
        var specimenPosition = Position;

        var closestFood = _map.Food
            .Select((food, i) => (food, i))
            .Where(t => t.food.Valid)
            .OrderBy(t => Vector3.Distance(t.food.Position, specimenPosition))
            .First();

        var targetPosition = closestFood.food.Position - specimenPosition;

        var angle = Vector3.SignedAngle(targetPosition, Vector3.right, Vector3.up);
        var rotation = Quaternion.AngleAxis(angle, Vector3.down);
        var direction = rotation * Vector3.right;

        Position += direction * _settings.moveSpeed;

        ConsumeFood(closestFood);
    }

    private void ConsumeFood((Food food, int i) closestFood)
    {
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
