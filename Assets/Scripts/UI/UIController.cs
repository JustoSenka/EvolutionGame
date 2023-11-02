using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Text FrameText;
    public Text GenerationText;
    public Text ScoreText;

    void Update()
    {
        if (SimulationBehaviour.Instance.Simulation == null)
            return;

        FrameText.text = $"Frame: {SimulationBehaviour.Instance.Simulation.Frame}";
        GenerationText.text = $"Generation: {SimulationBehaviour.Instance.Simulation.Generation}";

        var scores = string.Join(Environment.NewLine, SimulationBehaviour.Instance.Simulation.TopSpecimen.Select(s => $"{s.Id}_{s.Neural?.Id[..6]}: {s.Score}"));
        ScoreText.text = $"Top Scores: {Environment.NewLine}{scores}";
    }
}
