using System;
using UnityEngine;

/// <summary>
/// Automatically registers behaviours into the database
/// </summary>
public abstract class PooledBehaviour : MonoBehaviour
{
    // Used to set values from inspector, structs fail to serialize well
    // public TObject underlyingObject;

    public Guid Id { get; private set; }
    public PoolItem PoolItem { get; private set; }
    public abstract Type UnderlyingUnitType { get; }

    protected bool ShouldRemoveSelf = false;

    public virtual void LinkWithBackendObject(Guid id, PoolItem poolItem = null)
    {
        Database.Instance.UnitRemoved += RemoveSelf;

        PoolItem = poolItem;
        Id = id;
    }

    private void RemoveSelf(Unit unit)
    {
        if (unit.Id == Id)
            ShouldRemoveSelf = true;
    }

    protected virtual void Update()
    {
        if (!ShouldRemoveSelf)
            return;

        ShouldRemoveSelf = false;

        if (PoolItem != null)
        {
            PoolItem.Release();
            Database.Instance.UnitRemoved -= RemoveSelf;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
