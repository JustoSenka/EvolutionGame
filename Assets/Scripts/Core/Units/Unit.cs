using UnityEngine;

public interface IUnit
{
    public bool Valid { get; }
    public int Id { get; }
    public Vector3 Position { get; set; }

    public void Destroy();
    public void Reactivate();
}
