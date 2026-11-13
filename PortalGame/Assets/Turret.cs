using Unity.VisualScripting;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public LineRenderer m_laser;
    public float maxDistance;
    public LayerMask m_LaserMask;
    public float m_AngleToFall = 30.0f;
    private void Update()
    {
        bool l_Fallen = Vector3.Dot(transform.up, Vector3.up) < Mathf.Cos(m_AngleToFall * Mathf.Deg2Rad);
        m_laser.gameObject.SetActive(!l_Fallen);

        if (!l_Fallen)
        {
            Vector3 lposition = Vector3.forward * maxDistance;
            Ray ray = new Ray(m_laser.transform.position, m_laser.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit lrayhit, maxDistance, m_LaserMask.value, QueryTriggerInteraction.Ignore))
            {
                lposition = Vector3.forward * lrayhit.distance;
            }
            m_laser.SetPosition(1, lposition);
        }
    }
}
