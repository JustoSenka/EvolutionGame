using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class Specimen : Unit
{
    public NeuralNetwork Neural { get; private set; }

    public float score = 0;
    public int[] neuralNetworkLayerSizes = new int[] { 1, 1 };
    public float mutationFactor = 0.05f;
    public float consumeRadius = 0.2f;

    public float moveSpeed = 0.2f;

    public float health = 100;
    public float energy = 100;
    public float hunger = 100;

    public float maxHealth = 100;
    public float maxEnergy = 100;
    public float maxHunger = 100;
    public float damage = 10;
    public float defense = 1;

    public float hungerCostToMove = 0.1f;
    public float hungerCostToStay = 0.02f;
    public float energyRegen = 5;

    public Specimen(bool addToDatabase) : base(addToDatabase)
    {
        health = maxHealth;
        energy = maxEnergy;
        hunger = maxHunger;
    }

    public void InitializeRandomNaural(int randomSeed)
    {
        Neural = new NeuralNetwork(neuralNetworkLayerSizes, null, randomSeed);
    }

    public void Copy(NeuralNetwork parentNeural)
    {
        InitializeRandomNaural(0);
        Neural.Copy(parentNeural);
    }

    public void Mutate()
    {
        Neural.Mutate(mutationFactor);
    }

    public void Breed(NeuralNetwork parentY, NeuralNetwork parentX)
    {
        InitializeRandomNaural(0);
        Neural.Breed(parentY, parentX);
    }

    public override void CustomUpdate(long frame)
    {
        var input = Neural.GetInputArray();

        var specimenPosition = Position;

        Food closestFood;
        lock (Database.Instance.Food.Objects)
        {
            closestFood = Database.Instance.Food
                .Where(food => food.Valid)
                .OrderBy(food => Vector3.SqrMagnitude(food.Position - specimenPosition))
                .FirstOrDefault();
        }

        input[0] = closestFood == null ? 0 : Vector3.SignedAngle(closestFood.Position - specimenPosition, Vector3.right, Vector3.up) / 360f;

        var outputs = Neural.FeedForward();

        var targetAngle = (float)outputs[0] * 360f;

        var rotation = Quaternion.AngleAxis(targetAngle, Vector3.down);
        var direction = rotation * Vector3.right;

        Position += direction * moveSpeed;

        ConsumeFood(closestFood);
    }

    public void CustomUpdateSimple()
    {
        var specimenPosition = Position;

        var closestFood = Database.Instance.Food
            .Where(food => food.Valid)
            .OrderBy(food => Vector3.Distance(food.Position, specimenPosition))
            .First();

        var targetPosition = closestFood.Position - specimenPosition;

        var angle = Vector3.SignedAngle(targetPosition, Vector3.right, Vector3.up);
        var rotation = Quaternion.AngleAxis(angle, Vector3.down);
        var direction = rotation * Vector3.right;

        Position += direction * moveSpeed;

        ConsumeFood(closestFood);
    }

    private void ConsumeFood(Food food)
    {
        if (food != null && Vector3.Distance(food.Position, Position) <= consumeRadius)
        {
            Database.Instance.Remove(food);
            score += 50;
        }
    }
}
