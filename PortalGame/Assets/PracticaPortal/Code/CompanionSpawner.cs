using Unity.VisualScripting;
using UnityEngine;

public class CompanionSpawner : MonoBehaviour
{
    public Transform m_spawnPosition; 
    public GameObject m_CompanionCube;
      void OnTriggerEnter(Collider other){
        if (other.CompareTag("Player")) {

            GameObject gameobject = GameObject.Instantiate(m_CompanionCube);
            gameObject.transform.position = m_spawnPosition.position;
        }

    }
}
