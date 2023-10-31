using System;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Text FrameText;
    public Text GenerationText;
    public Text ScoreText;

    void Update()
    {
        FrameText.text = $"Frame: {Game.instance.frame}";
        GenerationText.text = $"Generation: {Game.instance.generation}";

        var scores = string.Join(Environment.NewLine, Game.instance.topScores);
        ScoreText.text = $"Top Scores: {Environment.NewLine}{scores}";
    }
}
