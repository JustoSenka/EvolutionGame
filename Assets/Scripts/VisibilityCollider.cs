using UnityEngine;

public class VisibilityCollider : MonoBehaviour
{
    public Specimen specimen;

    void OnTriggerEnter(Collider other)
    {
        //specimen.OnVisibilityEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        //specimen.OnVisibilityExit(other);
    }
}
