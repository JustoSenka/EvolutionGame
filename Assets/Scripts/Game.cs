using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class Game : MonoBehaviour
{
    public static Game instance;

    public GameObject specimenPrefab;
    public GameObject foodPrefab;

    public GameObject specimenParent;
    public GameObject foodParent;

    public int specimenCount = 10;
    public int foodCount = 50;
    public float fixedUpdateTimestep = 0.002f;
    
    public int generationLengthFrames = 500;

    public Rect spawnArea = new(-100, -100, 200, 200);

    public int randomSeed = 5;

    public int frame = 0;
    public int generation = 0;

    private Random _random;

    [NonSerialized]
    public Specimen[] specimen;

    [NonSerialized]
    public List<NeuralNetwork> topNeurals;

    public List<int> topScores;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Time.fixedDeltaTime = fixedUpdateTimestep;

        _random = new Random(randomSeed);

        specimen = new Specimen[0];
        topNeurals = new();

        ResetSimulation();
    }

    void FixedUpdate()
    {
        if (specimen == null)
            return;

        Time.fixedDeltaTime = fixedUpdateTimestep;

        frame++;

        if (frame % generationLengthFrames == 0)
        {
            ResetSimulation();
            return;
        }

        for (int i = 0; i < specimen.Length; i++)
        {
            specimen[i].CustomFixedUpdate(frame);
        }
    }

    private void ResetSimulation()
    {
        _random = new Random(randomSeed);

        generation++;
        topNeurals = specimen.OrderByDescending(s => s.score).Take(10).Select(s => s.neural).ToList();
        topScores = specimen.Select(s => s.score).OrderByDescending(s => s).Take(10).ToList();
        DestroyAll();
        SpawnAll();
    }

    private void SpawnAll()
    {
        Spawn("Food", foodParent, foodCount, foodPrefab, 0.25f).ToList();

        specimen = Spawn("Specimen", specimenParent, specimenCount, specimenPrefab, 0.5f)
            .Select((go, i) =>
            {
                var specimen = go.GetComponent<Specimen>();
                specimen.Init(i);

                var neural = topNeurals.Count == 0 ? null : topNeurals[i % topNeurals.Count];
                if (neural != null)
                {
                    if (i < topNeurals.Count)
                        specimen.Copy(neural);
                    else
                        specimen.Mutate(neural);
                }

                return specimen;
            })
            .ToArray();
    }

    private void DestroyAll()
    {
        for (int i = 0; i < specimen.Length; i++)
        {
            Destroy(specimen[i].gameObject);
        }
        specimen = new Specimen[0];

        var food = foodParent.GetComponentsInChildren<Food>();
        for (int i = 0; i < food.Length; i++)
        {
            Destroy(food[i].gameObject);
        }
    }


    private IEnumerable<GameObject> Spawn(string name, GameObject parent, int count, GameObject prefab, float size)
    {
        for (int i = 0; i < count; i++)
        {
            var x = (float)_random.Next((int)spawnArea.width) + spawnArea.x;
            var z = (float)_random.Next((int)spawnArea.height) + spawnArea.y;
            var go = Instantiate(prefab, new Vector3(x, size, z), Quaternion.identity, parent.transform);
            go.name = $"{name}_{i}";
            yield return go;
        }
    }
}
