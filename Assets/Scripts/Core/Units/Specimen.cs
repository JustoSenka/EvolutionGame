using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class Specimen : Unit
{
    public NeuralNetwork Neural { get; private set; }

    private int[] _neuralNetworkLayerSizes = new int[] { 1, 2 };

    public float score = 0;
    public float mutationFactor = 0.05f;
    public float consumeRadius = 0.2f;

    public float baseMoveSpeed = 0.2f;

    public float health = 100;
    public float energy = 100;
    public float hunger = 100;
    public float maturity = 0;
    public float reproduction = 0;

    public float maxHealth = 100;
    public float maxEnergy = 100;
    public float maxHunger = 100;
    public float maxMaturity = 100;
    public float maxReproduction = 100;

    public float damage = 10;
    public float defense = 1;
    public float maturityRate = 0.2f;
    public float reproductionRate = 0.3f;
    public float energyRegen = 5;
    public float reproductionCount = 5;

    public float hungerCostToMove = 0.3f;
    public float hungerCostToStay = 0.1f;

    public Specimen(bool addToDatabase) : base(addToDatabase)
    {
        health = maxHealth;
        energy = maxEnergy;
        hunger = maxHunger;
    }

    public void InitializeRandomNaural(int randomSeed)
    {
        Neural = new NeuralNetwork(_neuralNetworkLayerSizes, null, randomSeed);
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

        var closestFood = FindClosesFood();

        input[0] = closestFood == null ? 0 : Vector3.SignedAngle(closestFood.Position - Position, Vector3.right, Vector3.up) / 360f;

        var outputs = Neural.FeedForward();

        var direction = Quaternion.AngleAxis((float)outputs[0] * 360f, Vector3.down) * Vector3.right;
        var movementSpeed = Mathf.Clamp01((float)outputs[1]);

        UpdatePosition(direction, movementSpeed);
        ConsumeFood(closestFood);
        Breed();

        Upkeep(movementSpeed);
    }

    private void UpdatePosition(Vector3 direction, float speed)
    {
        Position += baseMoveSpeed * speed * direction;
    }
    private void ConsumeFood(Food food)
    {
        if (food != null && Vector3.Distance(food.Position, Position) <= consumeRadius)
        {
            Database.Instance.Remove(food);

            score += food.quantity;
            hunger = Mathf.Clamp(hunger + food.quantity, 0, maxHunger);
            reproduction = reproduction + reproductionRate * food.quantity * 100;
        }
    }

    private void Breed()
    {
        if (reproduction >= maxReproduction)
        {
            Simulation.Instance.ActionsAfterUpdates.Enqueue(() =>
            {
                for (int i = 0; i < reproductionCount; i++)
                {
                    var child = new Specimen(true);
                    child.Copy(Neural);
                    child.Mutate();
                    child.Position = Position;
                }
            });
            
            reproduction -= maxReproduction;
        }
    }

    private void Upkeep(float speed)
    {
        maturity = Mathf.Clamp(maturity + maturityRate, 0, maxMaturity);

        if (hunger > maxHunger * 8 / 10)
            reproduction = Mathf.Clamp(reproduction + reproductionRate * hunger / maxHunger, 0, maxReproduction);

        hunger = Mathf.Clamp(hunger - hungerCostToStay - hungerCostToMove * speed, 0, maxHunger);
        if (hunger <= 0)
        {
            Simulation.Instance.ActionsAfterUpdates.Enqueue(() =>
            {
                Destroy();
            });
        }
    }

    private Food FindClosesFood()
    {
        Food closestFood;
        lock (Database.Instance.Food.Objects)
        {
            closestFood = Database.Instance.Food
                .Where(food => food.Valid)
                .OrderBy(food => Vector3.SqrMagnitude(food.Position - Position))
                .FirstOrDefault();
        }

        return closestFood;
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

        Position += direction * baseMoveSpeed;

        ConsumeFood(closestFood);
    }
}
