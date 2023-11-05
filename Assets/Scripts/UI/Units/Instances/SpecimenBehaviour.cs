using System;
using UnityEngine;

public class SpecimenBehaviour : GenericPooledBehaviour<Specimen>
{
    public override Type UnderlyingUnitType => typeof(Specimen);

    public override void LinkWithBackendObject(Guid id, PoolItem poolItem = null)
    {
        base.LinkWithBackendObject(id, poolItem);

        lock (Database.Instance.Specimen.Objects)
        {
            Object = Database.Instance.Specimen.Objects[Id];
        }
    }

    protected override void Update()
    {
        if (Object != null && transform.position != Object.Position)
        {
            var angle = Vector3.SignedAngle(Object.Position - transform.position, Vector3.right, Vector3.up);
            var rotation = Quaternion.AngleAxis(angle, Vector3.down);
            var direction = rotation * Vector3.right;

            transform.rotation = rotation;
        }

        base.Update();
    }
}
