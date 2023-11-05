using System;

[Serializable]
public class Tree : Unit
{
    public float spawnRadius = 30f;

    public int spawnFrequency = 30;

    private long nextSpawn;
    private Random _random;

    public Tree(bool addToDatabase) : base(addToDatabase) { }

    public override void CustomUpdate(long frame)
    {
        // spawn additional food
        if (frame == nextSpawn)
        {
            var food = new Food(true)
            {
                Position = Utils.CreateRandomPositionAround(Position, spawnRadius, _random)
            };
        }

        // update next spawn time
        if (frame >= nextSpawn)
            nextSpawn += _random.Next(spawnFrequency, spawnFrequency * 2);
    }

    public void InitializeRandom(int randomSeed)
    {
        _random = new Random(randomSeed);
    }
}
