using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

public static class Utils
{
    public static void SaveNeuralNetworkToFileAsync(INeural neural)
    {
        var json = JsonConvert.SerializeObject(neural, Formatting.Indented);

        Task.Run(() =>
        {
            var currentTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_ff");

            var filePath = $"Neurals/Neural_{currentTime}.json";
            if (!Directory.Exists("Neurals"))
                Directory.CreateDirectory("Neurals");

            File.WriteAllText(filePath, json);
        }).ContinueWith(task =>
        {
            if (task.IsFaulted)
                Debug.LogError(task.Exception);
        });
    }

    public static Vector3 CreateRandomPositionWithinRect(Rect rect, Random random)
    {
        return new Vector3((float)random.NextDouble() * rect.width + rect.x, 0, (float)random.NextDouble() * rect.height + rect.y);
    }

    public static Vector3 CreateRandomPositionAround(Vector3 obj, float radius, Random random)
    {
        var angle = random.NextDouble() * 2 * Math.PI;
        var distance = Math.Sqrt(random.NextDouble()) * radius;

        var x = obj.x + distance * Math.Cos(angle);
        var z = obj.z + distance * Math.Sin(angle);

        return new Vector3((float) x, 0, (float) z);
    }
}
