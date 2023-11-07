using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Text FrameText;
    public Text SpecimenText;
    public Text ScoreText;

    void Update()
    {
        FrameText.text = $"Frame: {Simulation.Instance.Frame}";
        SpecimenText.text = $"Specimen: {Database.Instance.Specimen.Objects.Count}";
        
        // Doing this less rarely to not slow down performance
        if (Simulation.Instance.Frame % 20 == 0)
        {
            var scores = string.Join(Environment.NewLine, Database.Instance.Specimen
                .OrderByDescending(s => s.score)
                .Take(10)
                .Select(s => $"{s.Id.ToString()[..6]}: {s.score}"));

            ScoreText.text = $"Top Scores: {Environment.NewLine}{scores}";
        }
    }
}
