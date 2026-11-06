using UnityEngine;
using UnityEngine.Events;

public class PortalButton : MonoBehaviour
{
    public UnityEvent m_event; 
     private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
           m_event.Invoke();
        }
    }
        
    
}
