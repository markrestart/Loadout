using UnityEngine;

public class Ragdoll_Controller : MonoBehaviour
{
    [SerializeField]
    Collider[] colliders;
    [SerializeField]
    Rigidbody[] rigidbodies;
    public void EnableRagdoll()
    {
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
        }
    }

    public void DisableRagdoll()
    {
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }
    }
}
