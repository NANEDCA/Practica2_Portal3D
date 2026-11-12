using UnityEngine;

public class CompanionCube : MonoBehaviour
{
    public float m_maxValidAngle = 40f; 
    Rigidbody rb;
    public float m_teleportportalOffset = 1.0f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Portal"))
        {
            Portal portal = other.GetComponent<Portal>();

            if (CanTeleport(portal))
            {
                Teleport(portal);
            }
        }
    }

    private bool CanTeleport(Portal portal)
    {
        float lmaxDotangle = Mathf.Cos(m_maxValidAngle * Mathf.Deg2Rad);
        Vector3 lDirection = rb.linearVelocity.normalized;
        float ldotAngle = Vector3.Dot(-lDirection, portal.transform.forward);
        return ldotAngle >= lmaxDotangle;

    }
    public void Teleport(Portal portal) 

    {   
        Vector3 lDirection =rb.linearVelocity.normalized; 

        Vector3 Worldlposition = transform.position + lDirection * m_teleportportalOffset;
        Vector3 lWorldForward = transform.forward;
        Vector3 worldLinearVelocity = rb.linearVelocity; 

        Vector3 localPosition = portal.m_OtherPortalTransform.InverseTransformPoint(Worldlposition);
        Vector3 localForward = portal.m_OtherPortalTransform.InverseTransformDirection(lWorldForward);
        Vector3 localLinearVelocity = portal.m_OtherPortalTransform.InverseTransformDirection(worldLinearVelocity);


        rb.isKinematic = true;
        transform.position = portal.m_MirrorPortal.transform.TransformPoint(localPosition);
        transform.forward = portal.m_MirrorPortal.transform.TransformDirection(localForward);
        rb.isKinematic = false;
        rb.linearVelocity = portal.m_MirrorPortal.transform.TransformDirection(localLinearVelocity);


    }
}
