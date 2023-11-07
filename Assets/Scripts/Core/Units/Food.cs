using System;

[Serializable]
public class Food : Unit
{
    public float quantity = 20;

    public Food(bool addToDatabase) : base(addToDatabase) { }
}