using UnityEngine;
using UnityEngine.Events;

public class PortalButton : MonoBehaviour
{
    public UnityEvent m_event;
    public Animation DoorAnimationM;
     private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cube"))
        {
          
            m_event.Invoke();
        }
    } 

    public void OpenDoor()
    {
       
        DoorAnimationM.Play();      
    }       
   
}
