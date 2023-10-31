using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class Specimen : MonoBehaviour
{
    public int score = 0;
    public float moveSpeed = 0.2f;
    public float visibilityRadius = 10;
    public float consumeRadius = 2;

    public float mutationFactor = 0.05f;

    public int[] neuralNetworkLayerSizes = new int[] { 2, 18, 9, 1 };

    public double[] inputs = new double[12];
    public double[] outputs = new double[1];

    private Random _random;

    [NonSerialized]
    public NeuralNetwork neural;

    // private SortedSet<GameObject> _closestFood;
    [SerializeField]
    private Collider[] _closestFoodArr;

    public VisibilityCollider visibilityCollider;
    public ConsumeCollider consumeCollider;

    public void Awake()
    {
        visibilityCollider.GetComponent<SphereCollider>().radius = visibilityRadius;
        consumeCollider.GetComponent<SphereCollider>().radius = consumeRadius;

        // _closestFood = new SortedSet<GameObject>(new DistanceComparer(gameObject));
    }

    public void Init(int randomSeed)
    {
        _random = new Random(randomSeed);

        inputs = new double[neuralNetworkLayerSizes[0]];
        outputs = new double[neuralNetworkLayerSizes[^1]];

        neural = new NeuralNetwork(neuralNetworkLayerSizes, null, randomSeed);

        // _closestFood = new SortedSet<GameObject>(new DistanceComparer(gameObject));
    }

    public void Copy(NeuralNetwork parentNeural)
    {
        neural.Copy(parentNeural);
    }

    public void Mutate(NeuralNetwork parentNeural)
    {
        neural.Mutate(parentNeural, mutationFactor);
    }

    public void CustomFixedUpdate(int frame)
    {
        var position = transform.position;
        /*
        inputs[0] = position.x;
        inputs[1] = position.z;
        */
        var closestFood = Physics.OverlapSphere(transform.position, visibilityRadius, LayerMask.GetMask("Food"))
            .OrderBy(a => Vector3.Distance(a.transform.position, position))
            .Take(1)
            .Select(c => c.gameObject)
            .ToList();

        for (int i = 0; i < 1; i++)
        {
            Vector3 target = new Vector3(9999, 0, 9999);
            if (closestFood.Count > i)
            {
                target = closestFood[i].transform.position;
            }

            inputs[i * 2 + 0] = position.x - target.x;
            inputs[i * 2 + 1] = position.z - target.z;
        }

        var outputs = neural.FeedForward(inputs);

        var rot = Quaternion.AngleAxis((float)(360 * outputs[0]), Vector3.up);
        var direction = rot * Vector3.right;

        transform.Translate(direction * moveSpeed, Space.World);
    }

    public void RemoveFood(GameObject go)
    {
        // _closestFood.Remove(go);
    }

    public void OnVisibilityEnter(Collider other)
    {
        var food = other.GetComponent<Food>();
        if (!food)
            return;

        Debug.Log($"Food entered visibility range: {name}", gameObject);

        // _closestFood.Add(other.gameObject);
    }

    public void OnVisibilityExit(Collider other)
    {
        var food = other.GetComponent<Food>();
        if (!food)
            return;
        /*
        if (_closestFood.Contains(other.gameObject))
        {
            _closestFood.Remove(other.gameObject);
            Debug.Log($"Food left visibility range: {name}", gameObject);
        }
        */
    }

    public void OnConsumeEnter(Collider other)
    {
        var food = other.GetComponent<Food>();
        if (!food)
            return;

        score += 50;

        // Debug.Log($"Food Consumed: {other.gameObject.name}", gameObject);
        /*
        for (int i = 0; i < Game.instance._specimen.Length; i++)
        {
            Game.instance._specimen[i].RemoveFood(other.gameObject);
        }
        */
        Destroy(other.gameObject);
    }

    public void OnConsumeExit(Collider other)
    {

    }

    public class DistanceComparer : IComparer<GameObject>
    {
        private readonly GameObject _specimen;

        public DistanceComparer(GameObject specimen)
        {
            _specimen = specimen;
        }

        public int Compare(GameObject a, GameObject b)
        {
            var specimenPos = _specimen.transform.position;
            var posA = a.transform.position;
            var posB = b.transform.position;

            var distA = Mathf.Pow(posA.x - specimenPos.x, 2) + Mathf.Pow(posA.z - specimenPos.z, 2);
            var distB = Mathf.Pow(posB.x - specimenPos.x, 2) + Mathf.Pow(posB.z - specimenPos.z, 2);
            return (int)(distA - distB);
        }
    }
}
