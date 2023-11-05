using System;
using UnityEngine;

public abstract class GenericPooledBehaviour<T> : PooledBehaviour where T : Unit
{
    public T Object;

    protected override void Update()
    {
        base.Update();

        if(Object != null)
            transform.position = Object.Position;
    }
}
