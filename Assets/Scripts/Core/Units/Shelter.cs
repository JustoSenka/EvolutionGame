using System;

[Serializable]
public class Shelter : Unit
{
    public float radius;

    public Shelter(bool addToDatabase) : base(addToDatabase) { }
}
