using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

[Serializable]
public class SimulationSettings
{
    public int initialSpecimenCount = 100;
    public int initialFoodCount = 50;
    public int initialSilicaCount = 50;

    public Rect spawnArea = new(-100, -100, 200, 200);
    public int randomSeed = 5;

    public int delayBetweekFramesMs = 0;
}

public class Simulation
{
    public static Simulation Instance { get; } = new Simulation();
    public long Frame { get; private set; } = 0;
    public float MaxScore { get; private set; } = 0;

    private SimulationSettings _settings;
    private Random _random;

    public ConcurrentQueue<Action> ActionsAfterUpdates = new();

    private Simulation() { }

    public void StartAsync(SimulationSettings simulationSettings, CancellationToken token)
    {
        Debug.Log("Starting Asynchronously");

        _settings = simulationSettings;
        _random = new Random(_settings.randomSeed);

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                CustomUpdate();

                if (_settings.delayBetweekFramesMs != 0)
                    await Task.Delay(_settings.delayBetweekFramesMs);
            }

        }, token).ContinueWith(task =>
        {
            if (task.IsFaulted)
                Debug.LogError(task.Exception);
        });
    }

    public void StartSync(SimulationSettings simulationSettings)
    {
        Debug.Log("Starting Synchronously");

        _settings = simulationSettings;
        _random = new Random(_settings.randomSeed);
    }

    public void CustomUpdate()
    {
        Frame++;

        if (Frame == 1)
            SpawnInitialUnits();

        if (Database.Instance.Specimen.Objects.Count == 0)
        {
            Debug.Log("No specimen in the simulation");
        }

        Parallel.ForEach(Database.Instance.Specimen, specimen =>
        {
            specimen.CustomUpdate(Frame);
        });

        Parallel.ForEach(Database.Instance.Trees, tree =>
        {
            tree.CustomUpdate(Frame);
        });

        foreach (var ac in ActionsAfterUpdates)
            ac.Invoke();

        ActionsAfterUpdates.Clear();

        SaveTopSpecimen();
    }

    private void SpawnInitialUnits()
    {
        for (int i = 0; i < _settings.initialSpecimenCount; i++)
        {
            var s = new Specimen(true)
            {
                Position = Utils.CreateRandomPositionWithinRect(_settings.spawnArea, _random)
            };

            s.InitializeRandomNaural(_random.Next());
        }

        for (int i = 0; i < _settings.initialFoodCount; i++)
        {
            new Food(true)
            {
                Position = Utils.CreateRandomPositionWithinRect(_settings.spawnArea, _random)
            };
        }
    }

    private void SaveTopSpecimen()
    {
        var topSingleSpecimen = Database.Instance.Specimen.OrderByDescending(s => s.score).FirstOrDefault();
        if (topSingleSpecimen == null || topSingleSpecimen.score <= MaxScore)
            return;

        Debug.Log("Saving top Specimen");

        MaxScore = topSingleSpecimen.score;
        Utils.SaveNeuralNetworkToFileAsync(topSingleSpecimen.Neural);
    }
}
