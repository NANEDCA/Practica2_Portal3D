using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Turret : MonoBehaviour
{
    public LineRenderer m_laser;
    public float maxDistance;
    public LayerMask m_LaserMask;
    public float m_AngleToFall = 30.0f;

    [Header("Portal Teleport")]
    public float m_MaxValidAngle = 40.0f;
    public float m_TeleportPortalOffset = 1.0f;

    Rigidbody m_Rigidbody;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        bool l_Fallen =
            Vector3.Dot(transform.up, Vector3.up) <
            Mathf.Cos(m_AngleToFall * Mathf.Deg2Rad);

        m_laser.gameObject.SetActive(!l_Fallen);

        if (!l_Fallen)
        {
            Vector3 l_Position = Vector3.forward * maxDistance;

            Ray l_Ray = new Ray(
                m_laser.transform.position,
                m_laser.transform.forward);

            if (Physics.Raycast(
                l_Ray,
                out RaycastHit l_RayHit,
                maxDistance,
                m_LaserMask.value,
                QueryTriggerInteraction.Ignore))
            {
                l_Position = Vector3.forward * l_RayHit.distance;

                if (l_RayHit.collider.CompareTag("RefractionCube"))
                {
                    RefractionCube l_Cube =
                        l_RayHit.collider.GetComponent<RefractionCube>();

                    if (l_Cube != null)
                        l_Cube.Refract();
                }
            }

            m_laser.SetPosition(1, l_Position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Portal"))
            return;

        Portal l_Portal = other.GetComponent<Portal>();

        if (l_Portal != null && CanTeleport(l_Portal))
            Teleport(l_Portal);
    }

    private bool CanTeleport(Portal portal)
    {
        if (m_Rigidbody == null ||
            m_Rigidbody.linearVelocity.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        float l_MaxDotAngle =
            Mathf.Cos(m_MaxValidAngle * Mathf.Deg2Rad);

        Vector3 l_Direction = m_Rigidbody.linearVelocity.normalized;

        float l_DotAngle =
            Vector3.Dot(-l_Direction, portal.transform.forward);

        return l_DotAngle >= l_MaxDotAngle;
    }

    private void Teleport(Portal portal)
    {
        Vector3 l_Direction = m_Rigidbody.linearVelocity.normalized;

        Vector3 l_WorldPosition =
            transform.position + l_Direction * m_TeleportPortalOffset;

        Vector3 l_WorldForward = transform.forward;
        Vector3 l_WorldVelocity = m_Rigidbody.linearVelocity;

        Vector3 l_LocalPosition =
            portal.m_OtherPortalTransform.InverseTransformPoint(l_WorldPosition);

        Vector3 l_LocalForward =
            portal.m_OtherPortalTransform.InverseTransformDirection(l_WorldForward);

        Vector3 l_LocalVelocity =
            portal.m_OtherPortalTransform.InverseTransformDirection(l_WorldVelocity);

        float l_ScaleRatio =
            portal.m_MirrorPortal.m_SizeMultiplier / portal.m_SizeMultiplier;

        m_Rigidbody.isKinematic = true;

        transform.position =
            portal.m_MirrorPortal.transform.TransformPoint(l_LocalPosition);

        transform.forward =
            portal.m_MirrorPortal.transform.TransformDirection(l_LocalForward);

        transform.localScale *= l_ScaleRatio;

        m_Rigidbody.isKinematic = false;

        m_Rigidbody.linearVelocity =
            portal.m_MirrorPortal.transform.TransformDirection(l_LocalVelocity);
    }
}