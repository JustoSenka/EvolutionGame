using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class SimulationBehaviour : MonoBehaviour
{
    public static SimulationBehaviour Instance { get; private set; }

    public SimulationSettings settings;

    public Simulation Simulation { get; private set; }

    public bool runAsynchronous = false;
    public float fixedUpdateTimestep = 2f;

    public GameObject specimenPrefab;
    public GameObject foodPrefab;

    public GameObject specimenParent;
    public GameObject foodParent;

    [NonSerialized]
    public List<GameObject> specimen;

    [NonSerialized]
    public List<GameObject> food;

    private CancellationTokenSource _cts;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Simulation = new Simulation(settings);

        SpawnAll();

        if (runAsynchronous)
            StartAsynchronousSimulation();
    }

    private void OnDisable()
    {
        _cts.Cancel();
    }

    private void StartAsynchronousSimulation()
    {
        _cts = new CancellationTokenSource();
        Simulation.Start(_cts.Token);
    }

    void FixedUpdate()
    {
        Time.fixedDeltaTime = fixedUpdateTimestep / 1000;

        if (Simulation == null)
            return;

        specimen[1].GetComponent<MeshRenderer>().material.color = Color.blue;
        specimen[10].GetComponent<MeshRenderer>().material.color = Color.gray;

        if (!runAsynchronous)
            Simulation.CustomUpdate();
    }

    private void Update()
    {
        for (int i = 0; i < Simulation.Specimen.Length; i++)
            specimen[i].transform.position = Simulation.Specimen[i].Position;
    }

    private void SpawnAll()
    {
        food = Spawn("Food", Simulation.Maps[0].Food.Select(f => f.Position), foodParent, foodPrefab, 0.25f).ToList();
        specimen = Spawn("Specimen", Simulation.Specimen.Select(s => s.Position), specimenParent, specimenPrefab, 0.5f).ToList();
    }

    private void DestroyAll()
    {
        var specimen = specimenParent.GetComponentsInChildren<FoodBehaviour>();
        for (int i = 0; i < specimen.Length; i++)
            Destroy(specimen[i].gameObject);

        var food = foodParent.GetComponentsInChildren<FoodBehaviour>();
        for (int i = 0; i < food.Length; i++)
            Destroy(food[i].gameObject);
    }

    private IEnumerable<GameObject> Spawn(string name, IEnumerable<Vector3> positions, GameObject parent, GameObject prefab, float size)
    {
        return positions.Select((pos, i) =>
        {
            var go = Instantiate(prefab, new Vector3(pos.x, size, pos.z), Quaternion.identity, parent.transform);
            go.name = $"{name}_{i}";
            return go;
        });
    }
}
