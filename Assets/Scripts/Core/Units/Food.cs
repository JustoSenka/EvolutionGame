using System;

[Serializable]
public class Food : Unit
{
    public float quantity = 10;

    public Food(bool addToDatabase) : base(addToDatabase) { }
}