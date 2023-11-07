using System;
using System.Linq;
using UnityEngine;
using Random = System.Random;

[Serializable]
public class Specimen : Unit
{
    public INeural Neural { get; private set; }

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

    public float hungerCostToMove = 0.1f;
    public float hungerCostToStay = 0.03f;

    private Random _random;

    public Specimen(bool addToDatabase) : base(addToDatabase)
    {
        health = maxHealth;
        energy = maxEnergy;
        hunger = maxHunger;
    }

    public void InitializeRandomTrainedNaural(int randomSeed)
    {
        _random = new Random(randomSeed);

        Neural = new Genome(1, 2, randomSeed);
        Neural.InitializeRandom();

        if (Neural is NeuralNetwork neuralNetwork)
        {
            neuralNetwork.weights[0][0][0] = 1;
            neuralNetwork.biases[0][0] = 0;

            neuralNetwork.weights[0][1][0] = 0;
            neuralNetwork.biases[0][1] = 1;
        }
        else if (Neural is Genome genome)
        {
            genome.Connections[0].SetWeight(1);
            genome.Connections[1].SetWeight(0);
            genome.Nodes[2].SetBias(1);
        }

        // Mutate();
    }

    public override void CustomUpdate(long frame)
    {
        var input = Neural.GetInputArray();

        var closestFood = FindClosesFood();

        var angleToFood = closestFood == null ? 0 : Vector3.SignedAngle(closestFood.Position - Position, Vector3.right, Vector3.up) / 360f;

        input[0] = angleToFood;

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
            reproduction += reproductionRate * food.quantity;
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
                    child.InitializeRandomTrainedNaural(_random.Next());
                    child.Neural.Copy(Neural);
                    child.Neural.Mutate(child.mutationFactor);
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
        {
            reproduction += reproductionRate * (hunger - maxHunger / 2) / maxHunger;
            if (reproduction < 0)
                reproduction = 0;
        }

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
        var input = Neural.GetInputArray();

        var closestFood = FindClosesFood();

        input[0] = closestFood == null ? 0 : Vector3.SignedAngle(closestFood.Position - Position, Vector3.right, Vector3.up) / 360f;
        var outputs = Neural.FeedForward();

        var targetPosition = closestFood.Position - Position;

        var angle = Vector3.SignedAngle(targetPosition, Vector3.right, Vector3.up);
        var direction = Quaternion.AngleAxis(angle, Vector3.down) * Vector3.right;

        var angle2 = (float) outputs[0] * 360f;
        var direction2 = Quaternion.AngleAxis(angle2, Vector3.down) * Vector3.right;
        var movementSpeed = Mathf.Clamp01((float)outputs[1]);

        Debug.Log($"FoodAngle: {input[0] * 360}, ManualAngle: {angle} NeuralAngle: {angle2}");

        Position += direction2 * baseMoveSpeed;

        ConsumeFood(closestFood);
    }
}
