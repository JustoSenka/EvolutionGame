using System;

[Serializable]
public class Silica : Unit
{
    public float quantity = 10;

    public Silica(bool addToDatabase) : base(addToDatabase) { }
}
