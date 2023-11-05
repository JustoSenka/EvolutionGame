using System;
using UnityEngine;

public abstract class Unit : IEquatable<Unit>
{
    public bool Valid { get; private set; }
    public Guid Id { get; private set; }
    public Vector3 Position { get; set; }

    public virtual void CustomUpdate(long frame) { }

    // Cannot be default constructor as it is used by unity serializer.
    public Unit(bool addToDatabase = false)
    {
        Id = Guid.NewGuid();

        if (addToDatabase)
            Activate();
    }

    public virtual void Destroy()
    {
        Database.Instance.Remove(this);
        Valid = false;
    }

    public virtual void Activate()
    {
        Database.Instance.Add(this);
        Valid = true;
    }

    public bool Equals(Unit other) => Id == other.Id;
    public override bool Equals(object obj) => obj is Unit unit && Equals(unit);
    public override int GetHashCode() => Id.GetHashCode();
}
