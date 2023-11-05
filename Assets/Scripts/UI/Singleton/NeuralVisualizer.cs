using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class NeuralVisualizer : MonoBehaviour
{
    public TextAsset textAsset;

    public SpriteRenderer neuronPrefab;
    public LineRenderer weightPrefab;
    public TextMeshPro textPrefab;

    void Start()
    {
        var neural = JsonConvert.DeserializeObject<NeuralNetwork>(textAsset.text);
        DrawNeural(neural);
    }

    public void DrawNeural(NeuralNetwork neural)
    {
        var layers = neural.layerSizes.Length;

        for (int layerId = 0; layerId < layers; layerId++)
        {
            var neuronsInLayer = neural.layerSizes[layerId];

            for (int neuronInLayer = 0; neuronInLayer < neuronsInLayer; neuronInLayer++)
            {
                if (layerId == 0)
                {
                    SpawnNeuron(layerId, layers, neuronInLayer, neuronsInLayer);
                    continue;
                }
                else
                {
                    var bias = neural.biases[layerId - 1][neuronInLayer];
                    SpawnNeuron(layerId, layers, neuronInLayer, neuronsInLayer, bias);
                }

                var neuronsInPreviousLayer = neural.layerSizes[layerId - 1];
                for (int neuronInPreviousLayer = 0; neuronInPreviousLayer < neuronsInPreviousLayer; neuronInPreviousLayer++)
                {
                    var weight = neural.weights[layerId - 1][neuronInLayer][neuronInPreviousLayer];
                    SpawnWeight(layerId, layers, neuronInLayer, neuronInPreviousLayer, neuronsInLayer, neuronsInPreviousLayer, weight);
                }
            }
        }
    }

    private void SpawnNeuron(int layerId, int layers, int neuronInLayer, int neuronsInLayer, double? bias = null)
    {
        var position = new Vector3((layerId - (layers / 2f)) * 6, (neuronInLayer - (neuronsInLayer / 2f)) * 2, -0.1f);
        var go = Instantiate(neuronPrefab, position, Quaternion.identity, transform);
        if (bias != null)
        {
            go.color = bias < 0
                ? Color.Lerp(Color.blue, Color.white, (float)bias.Value + 1)
                : Color.Lerp(Color.white, Color.red, (float)bias.Value);
        }
    }
    private void SpawnWeight(int layerId, int layers, int neuronInLayer, int neuronInPreviousLayer, int neuronsInLayer, int neuronsInPreviousLayer, double weight)
    {
        if (weight == 0)
            return;

        var positionTo = new Vector3((layerId - (layers / 2f)) * 6, (neuronInLayer - (neuronsInLayer / 2f)) * 2);
        var positionFrom = new Vector3((layerId - 1 - (layers / 2f)) * 6, (neuronInPreviousLayer - (neuronsInPreviousLayer / 2f)) * 2);

        var go = Instantiate(weightPrefab, Vector3.zero, Quaternion.identity, transform);

        go.positionCount = 2;
        go.SetPosition(0, positionFrom);
        go.SetPosition(1, positionTo);

        go.startColor = weight < 0
            ? Color.Lerp(Color.blue, Color.white, (float)weight + 1)
            : Color.Lerp(Color.white, Color.red, (float)weight);

        go.endColor = go.startColor;
    }
}
