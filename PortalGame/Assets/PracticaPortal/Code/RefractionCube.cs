using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RefractionCube : MonoBehaviour
{
    public LineRenderer m_Laser;
    bool m_DisableLaserOnNextFrame = false;

    public float maxDistance = 50.0f;
    public LayerMask m_LaserMask;

    public float m_maxValidAngle = 40f;
    public float m_teleportportalOffset = 1.0f;
    bool playerkilled = false;

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {

        if (m_DisableLaserOnNextFrame)
        {
            m_Laser.gameObject.SetActive(false);
            return;
        }

       
        Vector3 lposition = Vector3.forward * maxDistance;
        Ray ray = new Ray(m_Laser.transform.position, m_Laser.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit lrayhit, maxDistance, m_LaserMask.value, QueryTriggerInteraction.Ignore))
        {
            lposition = Vector3.forward * lrayhit.distance;

            if (lrayhit.collider.CompareTag("Player") && !playerkilled)
            {
                LaserKill();
                playerkilled = true;
            }
        }
        m_Laser.SetPosition(1, lposition);

        m_DisableLaserOnNextFrame = true;
    }
    private void LaserKill()
    {
        GameController.GetGameController().Restart();
        GameController.GetGameController().GetPlayer().Restart();

    }
    public void Refract()
    {
        if (!m_DisableLaserOnNextFrame)
            return;

        m_DisableLaserOnNextFrame = false;

        Vector3 l_Position = Vector3.forward * maxDistance;

        Ray l_Ray = new Ray(
            m_Laser.transform.position,
            m_Laser.transform.forward);

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

        m_Laser.SetPosition(1, l_Position);
        m_Laser.gameObject.SetActive(true);
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
        if (rb == null || rb.linearVelocity.sqrMagnitude < 0.0001f)
            return false;

        float l_MaxDotAngle =
            Mathf.Cos(m_maxValidAngle * Mathf.Deg2Rad);

        Vector3 l_Direction = rb.linearVelocity.normalized;

        float l_DotAngle =
            Vector3.Dot(-l_Direction, portal.transform.forward);

        return l_DotAngle >= l_MaxDotAngle;
    }

    private void Teleport(Portal portal)
    {
        float l_ScaleRatio =
          portal.m_MirrorPortal.m_SizeMultiplier / portal.m_SizeMultiplier;
        transform.localScale *= l_ScaleRatio;
        Vector3 l_Direction = rb.linearVelocity.normalized;

        Vector3 l_WorldPosition =
            transform.position + l_Direction * m_teleportportalOffset;

        Vector3 l_WorldForward = transform.forward;
        Vector3 l_WorldVelocity = rb.linearVelocity;

        Vector3 l_LocalPosition =
            portal.m_OtherPortalTransform.InverseTransformPoint(l_WorldPosition);

        Vector3 l_LocalForward =
            portal.m_OtherPortalTransform.InverseTransformDirection(l_WorldForward);

        Vector3 l_LocalVelocity =
            portal.m_OtherPortalTransform.InverseTransformDirection(l_WorldVelocity);


        rb.isKinematic = true;

        transform.position =
            portal.m_MirrorPortal.transform.TransformPoint(l_LocalPosition);

        transform.forward =
            portal.m_MirrorPortal.transform.TransformDirection(l_LocalForward);
        
        

        rb.isKinematic = false;

        rb.linearVelocity =
            portal.m_MirrorPortal.transform.TransformDirection(l_LocalVelocity);
    }
}