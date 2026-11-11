using UnityEngine;

public class CompanionSpawner : MonoBehaviour
{
    public Transform m_spawnPosition; 
    public GameObject m_CompanionCube; 
    public void EXecute()
    {

       GameObject  gameobject=GameObject.Instantiate(m_CompanionCube);
        gameObject.transform.position=m_spawnPosition.position;

    }
}
