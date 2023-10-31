using UnityEngine;

public class ConsumeCollider : MonoBehaviour
{
    public Specimen specimen;

    void OnTriggerEnter(Collider other)
    {
        specimen.OnConsumeEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        specimen.OnConsumeExit(other);
    }
}
