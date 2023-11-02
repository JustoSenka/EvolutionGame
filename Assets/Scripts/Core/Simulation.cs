using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

[Serializable]
/// <summary>
/// Global settings for simulation
/// Edited via Unity UI
/// </summary>
public class SimulationSettings
{
    public int specimenCount = 100;
    public int foodCount = 50;
    public int topSpecimenToKeep = 20;
    public int bottomSpecimenToKeep = 10;
    public int newSpecimenToAdd = 20;

    public int generationLengthFrames = 500;
    public Rect spawnArea = new(-100, -100, 200, 200);
    public int randomSeed = 5;

    public int delayBetweekFramesMs = 0;

    public SpecimenSettings specimenSettings;
}

public partial class Simulation
{
    public Specimen[] Specimen { get; private set; }
    public Map[] Maps { get; private set; }
    public Specimen[] TopSpecimen { get; private set; }
    public Specimen[] BottomSpecimen { get; private set; }
    public int Frame { get; private set; } = 0;
    public int Generation { get; private set; } = 0;
    public float MaxScore { get; private set; } = 0;

    private readonly SimulationSettings _settings;
    private readonly Random _random;

    public Simulation(SimulationSettings simulationSettings)
    {
        _settings = simulationSettings;

        _random = new Random(_settings.randomSeed);

        Specimen = new Specimen[_settings.specimenCount];
        TopSpecimen = new Specimen[_settings.topSpecimenToKeep];
        BottomSpecimen = new Specimen[_settings.bottomSpecimenToKeep];

        Maps = GenerateMaps();

        Generation = 1;
        _random = new Random(_settings.randomSeed);
        GenerateNewSpecimen();
    }

    public void Start(CancellationToken token)
    {
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

    public void CustomUpdate()
    {
        Frame++;

        if (Frame % _settings.generationLengthFrames == 0)
        {
            ResetSimulation();
            return;
        }

        Parallel.For(0, Specimen.Length, (i) =>
        {
            if (i == 10)
                Specimen[i].CustomUpdateSimple(); // Hand written AI to go for closest food
            else
                Specimen[i].CustomUpdate();
        });
    }

    private void ResetSimulation()
    {
        Debug.Log($"Resetting Simulation, generation: {Generation}->{Generation + 1}");

        SelectTopSpecimen();
        SelectBottomSpecimen();

        SaveTopSpecimen();
        ResetSpecimen();
        ResetMapState();

        Frame = 0;
        Generation++;
    }

    private void SelectTopSpecimen()
    {
        Debug.Log("Determining best Specimen");

        var specimenPool = Generation == 1 ? Specimen : Specimen.Concat(TopSpecimen);

        foreach (var (s, n, i) in specimenPool
            .Select(s => (s, s.Neural))
            .ToHashSet(new NeuralIdEqualityComparer())
            .OrderByDescending(t => t.Item1.Score)
            .Take(TopSpecimen.Length)
            .Select((t, i) => (t.Item1, t.Item2, i)))
        {
            TopSpecimen[i] = s;
        }
    }

    private void SelectBottomSpecimen()
    {
        Debug.Log("Determining worst Specimen");

        foreach (var (s, i) in Specimen
            .OrderBy(t => t.Score)
            .Take(BottomSpecimen.Length)
            .Select((s, i) => (s, i)))
        {
            BottomSpecimen[i] = s;
        }
    }

    private void SaveTopSpecimen()
    {
        Debug.Log("Saving top Specimen");

        var topSingleSpecimen = TopSpecimen.Where(s => s.Id != 10).First();
        if (topSingleSpecimen.Score <= MaxScore)
            return;

        MaxScore = topSingleSpecimen.Score;
        var json = JsonConvert.SerializeObject(topSingleSpecimen.Neural, Formatting.Indented);

        Task.Run(() =>
        {
            var currentTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_ff");

            var filePath = $"Neurals/Neural_{currentTime}_{Generation}.json";
            if (!Directory.Exists("Neurals"))
                Directory.CreateDirectory("Neurals");

            File.WriteAllText(filePath, json);
        }).ContinueWith(task =>
        {
            if (task.IsFaulted)
                Debug.LogError(task.Exception);
        });
    }

    private void ResetSpecimen()
    {
        Debug.Log("Resetting Specimen");

        for (int i = 0; i < Specimen.Length; i++)
        {
            var newSpecimen = new Specimen(i, _settings.specimenSettings, Maps[i]);
            if (i < _settings.topSpecimenToKeep)
            {
                newSpecimen.Copy(TopSpecimen[i].Neural);
            }
            else if (i < _settings.bottomSpecimenToKeep + _settings.topSpecimenToKeep)
            {
                newSpecimen.Copy(BottomSpecimen[i % BottomSpecimen.Length].Neural);
            }
            else if (i < _settings.bottomSpecimenToKeep + _settings.topSpecimenToKeep + _settings.newSpecimenToAdd)
            {
                newSpecimen.InitializeRandom(_random.Next());
            }
            else 
            {
                newSpecimen.Breed(TopSpecimen[i % _settings.topSpecimenToKeep].Neural, TopSpecimen[(2 * i + i % 2 + i % 3) % _settings.topSpecimenToKeep].Neural);
                newSpecimen.Mutate();
            }

            Specimen[i] = newSpecimen;
        }
    }

    private void ResetMapState()
    {
        Debug.Log("Resetting Map");

        for (int i = 0; i < Maps.Length; i++)
        {
            var map = Maps[i];
            for (int j = 0; j < _settings.foodCount; j++)
                map.Food[j].Reactivate();
        }
    }

    private void GenerateNewSpecimen()
    {
        Debug.Log("Generating new Specimen");

        for (int i = 0; i < Specimen.Length; i++)
        {
            var specimen = new Specimen(i, _settings.specimenSettings, Maps[i]);
            specimen.InitializeRandom(_random.Next());
            Specimen[i] = specimen;
        }
    }

    private Map[] GenerateMaps()
    {
        Debug.Log("Generating Map");

        var maps = new Map[_settings.specimenCount];

        var cachedFood = new Food[_settings.foodCount];
        for (int i = 0; i < cachedFood.Length; i++)
            cachedFood[i] = new Food(i, CreateRandomPositionWithinRect(_settings.spawnArea));

        for (int i = 0; i < maps.Length; i++)
        {
            var foodForMap = new Food[_settings.foodCount];
            for (int j = 0; j < foodForMap.Length; j++)
                foodForMap[j] = cachedFood[j];

            maps[i] = new Map(foodForMap);
        }

        return maps;
    }

    private Vector3 CreateRandomPositionWithinRect(Rect rect)
        => new((float)_random.NextDouble() * rect.width + rect.x, 0, (float)_random.NextDouble() * rect.height + rect.y);

}
