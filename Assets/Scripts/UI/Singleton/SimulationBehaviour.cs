using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class SimulationBehaviour : MonoBehaviour
{
    public SimulationSettings settings;

    public bool runAsynchronous = false;
    public float fixedUpdateTimestep = 2f;

    private CancellationTokenSource _cts;

    void Start()
    {
        if (runAsynchronous)
        {
            _cts = new CancellationTokenSource();
            Simulation.Instance.StartAsync(settings, _cts.Token);
        }
        else
        {
            Simulation.Instance.StartSync(settings);
        }
    }
    private void OnDisable()
    {
        _cts?.Cancel();
    }

    void FixedUpdate()
    {
        Time.fixedDeltaTime = fixedUpdateTimestep / 1000;

        if (!runAsynchronous)
            Simulation.Instance.CustomUpdate();
    }
}
