using System;

public class TreeBehaviour : GenericPooledBehaviour<Tree>
{
    public override Type UnderlyingUnitType => typeof(Tree);

    public override void LinkWithBackendObject(Guid id, PoolItem poolItem = null)
    {
        base.LinkWithBackendObject(id, poolItem);

        lock (Database.Instance.Trees.Objects)
        {
            Object = Database.Instance.Trees.Objects[Id];
        }
    }
}
