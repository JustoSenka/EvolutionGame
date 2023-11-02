using UnityEngine;

public struct Food : IUnit
{
    public bool Valid { get; private set; }
    public int Id { get; private set; }
    public Vector3 Position { get; set; }

    public Food(int id, Vector3 position)
    {
        Valid = true;
        Id = id;
        Position = position;
    }

    public void Destroy()
    {
        Valid = false;
    }

    public void Reactivate()
    {
        Valid = true;
    }
}
