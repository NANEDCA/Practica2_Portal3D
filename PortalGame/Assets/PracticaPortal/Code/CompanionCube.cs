using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CompanionCube : MonoBehaviour
{
    public float m_maxValidAngle = 40f;
    public float m_teleportportalOffset = 1.0f;
    Rigidbody rb;

    private void Awake()
    {  
  

        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Portal"))
            return;

        Portal portal = other.GetComponent<Portal>();

        if (portal != null && CanTeleport(portal))
            Teleport(portal);
    }

    private bool CanTeleport(Portal portal)
    {
        if (rb == null || rb.linearVelocity.sqrMagnitude < 0.0001f)
            return false;

        float l_MaxDotAngle =
            Mathf.Cos(m_maxValidAngle * Mathf.Deg2Rad);

        Vector3 l_Direction = rb.linearVelocity.normalized;

        float l_DotAngle =
            Vector3.Dot(-l_Direction, portal.transform.forward);

        return l_DotAngle >= l_MaxDotAngle;
    }

    public void Teleport(Portal portal)
    {
        
         float l_ScaleRatio = portal.m_MirrorPortal.m_SizeMultiplier / portal.m_SizeMultiplier;
        transform.localScale *= l_ScaleRatio;
      
        Vector3 l_Direction = rb.linearVelocity.normalized;

        Vector3 l_WorldPosition =
            transform.position + l_Direction * m_teleportportalOffset;

        Vector3 l_WorldForward = transform.forward;
        Vector3 l_WorldLinearVelocity = rb.linearVelocity;

        Vector3 l_LocalPosition =
            portal.m_OtherPortalTransform.InverseTransformPoint(l_WorldPosition);

        Vector3 l_LocalForward =
            portal.m_OtherPortalTransform.InverseTransformDirection(l_WorldForward);

        Vector3 l_LocalLinearVelocity =
            portal.m_OtherPortalTransform.InverseTransformDirection(l_WorldLinearVelocity);
      
        rb.isKinematic = true;

        transform.position =
            portal.m_MirrorPortal.transform.TransformPoint(l_LocalPosition);

        transform.forward =
            portal.m_MirrorPortal.transform.TransformDirection(l_LocalForward);
        rb.isKinematic = false;

        rb.linearVelocity =
            portal.m_MirrorPortal.transform.TransformDirection(l_LocalLinearVelocity);
    }
}