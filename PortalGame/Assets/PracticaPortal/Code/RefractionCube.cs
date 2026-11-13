using UnityEngine;

public class RefractionCube : MonoBehaviour
{
    public LineRenderer m_Laser;
    bool m_DisableLaserOnNextFrame = false;
    public float maxDistance = 50.0f;
    public LayerMask m_LaserMask;
    private void Update()
    {
        if (m_DisableLaserOnNextFrame)
        {
            m_Laser.gameObject.SetActive(false);
        }
        m_DisableLaserOnNextFrame=true;
    }
    public void Refract()
    {
        if (!m_DisableLaserOnNextFrame)
            return;

        m_DisableLaserOnNextFrame = false;
        Vector3 lposition = Vector3.forward * maxDistance;
        Ray ray = new Ray(m_Laser.transform.position, m_Laser.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit lrayhit, maxDistance, m_LaserMask.value, QueryTriggerInteraction.Ignore))
        {
            lposition = Vector3.forward * lrayhit.distance;
            if (lrayhit.collider.CompareTag("RefractionCube"))
                lrayhit.collider.GetComponent<RefractionCube>().Refract();
        }
        m_Laser.SetPosition(1, lposition);
        m_Laser.gameObject.SetActive(true);
    }
}
