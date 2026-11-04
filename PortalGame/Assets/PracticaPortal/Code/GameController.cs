using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Transform m_DestroyObjects;
    static GameController m_GameController = null;
    public KeyCode m_RestartKeyCode = KeyCode.F5;
    FPSController m_Player;
   
//    Fade m_Fade;

    private void Awake()
    {
        if(m_GameController == null)
        {
            m_GameController = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }
    static public GameController GetGameController()
    {
        return m_GameController;
    }
    public void SetPlayer(FPSController Player)
    {
        m_Player = Player;
    }
    public FPSController GetPlayer()
    {
        return m_Player;
    }
    public void Restart()
    {
        Debug.Log("RestartGame");
        for (int i = 0; i < m_DestroyObjects.childCount; i++)
            GameObject.Destroy(m_DestroyObjects.GetChild(i).gameObject);
        m_Player.Restart();
       // StartCoroutine(m_Fade.FadeOut());
    }
    private void Update()
    {
        if (Input.GetKeyDown(m_RestartKeyCode))
            Restart();
    }
 
  /*
    public void SetFade(Fade _Fade)
    {
        m_Fade = _Fade;
    }
    public Fade GetFade()
    {
        return m_Fade;
    }*/
}
