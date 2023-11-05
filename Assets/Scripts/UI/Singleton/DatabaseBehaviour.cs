using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class DatabaseBehaviour : MonoBehaviour
{
    public int randomSeed = 5;

    public GameObject specimenPrefab;
    public GameObject foodPrefab;
    public GameObject treePrefab;
    public GameObject silicaPrefab;
    public GameObject shelterPrefab;

    private ObjectPool _specimenPool;
    private ObjectPool _foodPool;
    private ObjectPool _treePool;
    private ObjectPool _silicaPool;
    private ObjectPool _shelterPool;

    private IDictionary<Type, (Type behaviourType, ObjectPool objectPool)> _typeMap;

    private ConcurrentQueue<Unit> _unitsToCreate = new();

    private Random _random;

    private void Awake()
    {
        _random = new Random(randomSeed);

        _specimenPool = new ObjectPool(gameObject, specimenPrefab, 0);
        _foodPool = new ObjectPool(gameObject, foodPrefab, 0);
        _treePool = new ObjectPool(gameObject, treePrefab, 0);
        _silicaPool = new ObjectPool(gameObject, silicaPrefab, 0);
        _shelterPool = new ObjectPool(gameObject, shelterPrefab, 0);

        _typeMap = new Dictionary<Type, (Type behaviouurType, ObjectPool objectPool)>()
        {
            [typeof(Food)] = (typeof(FoodBehaviour), _foodPool),
            [typeof(Tree)] = (typeof(TreeBehaviour), _treePool),
            [typeof(Silica)] = (typeof(SilicaBehaviour), _silicaPool),
            [typeof(Shelter)] = (typeof(ShelterBehaviour), _shelterPool),
            [typeof(Specimen)] = (typeof(SpecimenBehaviour), _specimenPool),
        };
    }
    
    void Start()
    {
        PutUnitsToDatabaseFromScene();
        Database.Instance.UnitAdded += _unitsToCreate.Enqueue;
    }

    private void Update()
    {
        // TODO: should check if the object is not consumed yet
        while (_unitsToCreate.TryDequeue(out Unit unit))
        {
            var unitType = unit.GetType();
            var objectPool = _typeMap[unitType].objectPool;
            var poolItem = objectPool.ReserveItem();

            var behaviour = (PooledBehaviour)poolItem.GameObject.GetComponent(_typeMap[unitType].behaviourType);

            behaviour.LinkWithBackendObject(unit.Id, poolItem);
            behaviour.transform.position = unit.Position;
        }
    }

    private void PutUnitsToDatabaseFromScene()
    {
        var behaviours = GameObject.FindObjectsByType<PooledBehaviour>(FindObjectsSortMode.None);
        
        Debug.Log($"Putitng objects from scene into the database: {behaviours.Length}");

        for (int i = 0; i < behaviours.Length; i++)
        {
            var unit = (Unit) Activator.CreateInstance(behaviours[i].UnderlyingUnitType, args: (object)true);

            if (unit is Specimen specimen)
            {
                Debug.Log($"Initializing NeuralNetwork for specimen: {specimen.Id}");
                specimen.InitializeRandomNaural(_random.Next());
            }

            if (unit is Tree tree)
            {
                Debug.Log($"Initializing Random for tree: {tree.Id}");
                tree.InitializeRandom(_random.Next());
            }

            unit.Position = behaviours[i].transform.position;
            behaviours[i].LinkWithBackendObject(unit.Id, null);
        }
    }
}
