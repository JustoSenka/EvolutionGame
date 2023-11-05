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

    private Simulation _simulation;
    void Start()
    {
        _simulation = new Simulation();

        if (runAsynchronous)
        {
            _cts = new CancellationTokenSource();
            _simulation.StartAsync(settings, _cts.Token);
        }
        else
        {
            _simulation.StartSync(settings);
        }
    }
    private void OnDisable()
    {
        _cts?.Cancel();
    }

    void FixedUpdate()
    {
        Time.fixedDeltaTime = fixedUpdateTimestep / 1000;

        if (_simulation == null)
            return;

        if (!runAsynchronous)
            _simulation.CustomUpdate();
    }
}
