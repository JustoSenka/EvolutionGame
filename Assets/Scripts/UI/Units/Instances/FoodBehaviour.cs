using System;

public class FoodBehaviour : GenericPooledBehaviour<Food>
{
    public override Type UnderlyingUnitType => typeof(Food);

    public override void LinkWithBackendObject(Guid id, PoolItem poolItem = null)
    {
        base.LinkWithBackendObject(id, poolItem);

        lock (Database.Instance.Food.Objects)
        {
            Object = Database.Instance.Food.Objects[Id];
        }
    }
}
