using UnityEngine;

public class CompanionCube : MonoBehaviour
{
    public float m_MaxValidAngle = 40.0f;
    Rigidbody m_RigidBody;
    public float m_TeleportPortalOffset = 1.0f;

    private void Awake()
    {
        m_RigidBody = GetComponent<Rigidbody>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Portal"))
        {
            Portal l_Portal = GetComponent<Portal>();
            if (CanTeleport(l_Portal))
            {
                Teleport(l_Portal);
            }
        }
    }
    private bool CanTeleport(Portal _portal)
    {
        float l_MaxDotAngle = Mathf.Cos(m_MaxValidAngle * Mathf.Deg2Rad);
        Vector3 l_Direction = m_RigidBody.linearVelocity.normalized;
        float l_DotAngle = Vector3.Dot(-l_Direction, _portal.transform.forward);
        return l_DotAngle >= l_MaxDotAngle;
    }
    void Teleport(Portal _portal)
    {
        Vector3 l_Direction = m_RigidBody.linearVelocity.normalized;
        Vector3 l_WorldPosition = transform.position + l_Direction * m_TeleportPortalOffset;
        Vector3 l_WorldForward = transform.forward;
        Vector3 l_WorldLinearVelocity = m_RigidBody.linearVelocity;
        Vector3 l_LocalPosition = _portal.m_OtherPortalTransform.InverseTransformPoint(l_WorldPosition);
        Vector3 l_LocalForward = _portal.m_OtherPortalTransform.InverseTransformDirection(l_WorldForward);
        Vector3 l_LocalLinearVelocity = _portal.m_OtherPortalTransform.InverseTransformDirection(l_WorldLinearVelocity);

        m_RigidBody.isKinematic = true;
        transform.position = _portal.m_MirrorPortal.transform.TransformPoint(l_LocalPosition);
        transform.forward = _portal.m_MirrorPortal.transform.TransformDirection(l_LocalForward);
        m_RigidBody.isKinematic = false;
        m_RigidBody.linearVelocity = _portal.m_MirrorPortal.transform.TransformDirection(l_LocalLinearVelocity);
    }
}
