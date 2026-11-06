using Unity.VisualScripting;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public LineRenderer m_laser;
    public float maxDistance;
    public LayerMask m_LaserMask;
    private void Update()
    {
        Vector3 lposition=Vector3.forward*maxDistance; 
        Ray ray=new Ray(m_laser.transform.position,m_laser.transform.forward); 
        if (Physics.Raycast(ray,out RaycastHit lrayhit,maxDistance,m_LaserMask.value,QueryTriggerInteraction.Ignore))
        {
            lposition=Vector3.forward*lrayhit.distance;
        }
        m_laser.SetPosition(1,lposition);
    }
}
