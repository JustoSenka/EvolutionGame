using System.Collections.Generic;

public class NeuralIdEqualityComparer : IEqualityComparer<(Specimen, NeuralNetwork)>
{
    public bool Equals((Specimen, NeuralNetwork) x, (Specimen, NeuralNetwork) y)
    {
        return x.Item2.Id == y.Item2.Id;
    }

    public int GetHashCode((Specimen, NeuralNetwork) obj)
    {
        return obj.Item2.Id.GetHashCode();
    }
}
