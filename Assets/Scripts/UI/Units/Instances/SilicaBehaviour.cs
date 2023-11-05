using System;

public class SilicaBehaviour : GenericPooledBehaviour<Silica>
{
    public override Type UnderlyingUnitType => typeof(Silica);

    public override void LinkWithBackendObject(Guid id, PoolItem poolItem = null)
    {
        base.LinkWithBackendObject(id, poolItem);

        lock (Database.Instance.Silica.Objects)
        {
            Object = Database.Instance.Silica.Objects[Id];
        }
    }
}
