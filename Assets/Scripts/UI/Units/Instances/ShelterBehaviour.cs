using System;

public class ShelterBehaviour : GenericPooledBehaviour<Shelter>
{
    public override Type UnderlyingUnitType => typeof(Shelter);

    public override void LinkWithBackendObject(Guid id, PoolItem poolItem = null)
    {
        base.LinkWithBackendObject(id, poolItem);

        lock (Database.Instance.Shelters.Objects)
        {
            Object = Database.Instance.Shelters.Objects[Id];
        }
    }
}
