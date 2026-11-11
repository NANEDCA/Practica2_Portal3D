using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class FPSController : MonoBehaviour
{
    bool m_AngleLocked = false;
    public Transform m_PitchController;

    float m_Yaw;
    float m_Pitch;
    public float m_YawSpeed;
    public float m_PitchSpeed;
    public bool m_InvertedYaw;
    public bool m_InvertedPitch;
    public float m_MinPitch;
    public float m_MaxPitch;
    public float m_Speed;
<<<<<<< Updated upstream
    CharacterController m_CharacterController;
=======
    public float m_ShootAttachedObjectForce = 20.0f;

    public bool  attachingObject=false;
   public bool objecAttached=false;
    float GrabTIme;


    CharacterController m_CharacterController;
    public float m_teleportportalOffset = 0.01f;
>>>>>>> Stashed changes

    float m_VerticalSpeed = 0.0f;
    public float m_JumpSpeed = 0.0f;
    public float m_SprintSpeed = 0.0f;

    Vector3 m_StartPosition;
    Quaternion m_StartRotation;

    [Header("Health")]
    public float m_MaxHealth = 100.0f;
    public float m_CurrentHealth = 100.0f;

    [Header("Shield")]
    public float m_MaxShield = 100.0f;
    public float m_CurrentShield = 100.0f;

    [Header("Shoot")]
    public Camera m_Camera;
    public float m_MaxShootDistance = 200.0f;
    public LayerMask m_ShootLayerMask;
    public float m_RecoilTime = 0.5f;
    private float m_NextFireTime = 0f;
    public Portal m_Blue;
    public Portal m_Orange;

    [Header("Ammo")]
    public int m_MagazineSize = 30;
    public int m_CurrentAmmo = 30;
    public int m_ReserveAmmo = 2;
    public int m_MaxReserveAmmo = 180;
    public float m_ReloadDuration = 1.2f;
    bool m_IsReloading = false;

    [Header("Input")]
    public KeyCode m_LeftKeyCode = KeyCode.A;
    public KeyCode m_RightKeyCode = KeyCode.D;
    public KeyCode m_UpKeyCode = KeyCode.W;
    public KeyCode m_DownKeyCode = KeyCode.S;
    public KeyCode m_JumpKeyCode = KeyCode.Space;
    public KeyCode m_SprintKeyCode = KeyCode.LeftShift;
    public KeyCode m_DebugLockAngleKeyCode = KeyCode.I;
    public KeyCode m_DebugLockCursorKeyCode = KeyCode.O;
<<<<<<< Updated upstream
    public int m_ShootButton = 0;
=======
    public KeyCode m_GrabObject = KeyCode.E;
    public int mouse_ShootButton0 = 0;
    public int mouse_ShootButton1 = 1;

    [Header("Grab")]
    
    public Transform m_GrabTransform;
    public float TimeToGrab=0.8f;
    public float msjootAttacjedObjectforce = 20.0f;
>>>>>>> Stashed changes

    [Header("Animation")]
    public Animation m_Animation;
    public AnimationClip m_IdleAnimationClip;
    public AnimationClip m_ShootAnimationClip;
    public AnimationClip m_ReloadAnimationClip;

    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_CurrentHealth = Mathf.Clamp(m_CurrentHealth, 0f, m_MaxHealth);
        m_CurrentShield = Mathf.Clamp(m_CurrentShield, 0f, m_MaxShield);
        m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo, 0, m_MagazineSize);
        m_ReserveAmmo = Mathf.Clamp(m_ReserveAmmo, 0, m_MaxReserveAmmo);
    }
    private void Start()
    {
        m_StartPosition = transform.position;
        m_StartRotation = transform.rotation;
        m_Yaw = transform.rotation.eulerAngles.y;
        m_Pitch = m_PitchController.eulerAngles.x;
        Cursor.lockState = CursorLockMode.Locked;
        //SetIdleWeaponAnimation();
        GameController.GetGameController().SetPlayer(this);
    }
    private void Update()
    {
        if (Input.GetKeyDown(m_DebugLockAngleKeyCode))
            m_AngleLocked = !m_AngleLocked;

        if (Input.GetKeyDown(m_DebugLockCursorKeyCode))
        {
            Cursor.lockState = Cursor.lockState != CursorLockMode.Locked ? CursorLockMode.Locked : CursorLockMode.None;
        }

        float l_MouseX = Input.GetAxis("Mouse X");
        float l_MouseY = Input.GetAxis("Mouse Y");

        if (m_InvertedYaw) l_MouseX = -l_MouseX;
        if (m_InvertedPitch) l_MouseY = -l_MouseY;

        m_Yaw = m_Yaw + l_MouseX * m_YawSpeed * Time.deltaTime;
        m_Pitch = m_Pitch + l_MouseY * m_PitchSpeed * Time.deltaTime;

        m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);
        transform.rotation = Quaternion.Euler(0.0f, m_Yaw, 0.0f);
        m_PitchController.localRotation = Quaternion.Euler(m_Pitch, 0.0f, 0.0f);

        Vector3 l_Direction = Vector3.zero;
        Vector3 l_Right = transform.right;
        Vector3 l_Forward = transform.forward;
        l_Right.y = 0.0f;
        l_Forward.y = 0.0f;
        l_Right.Normalize();
        l_Forward.Normalize();

        if (Input.GetKey(m_LeftKeyCode)) l_Direction = -transform.right;
        else if (Input.GetKey(m_RightKeyCode)) l_Direction = transform.right;
        if (Input.GetKey(m_UpKeyCode)) l_Direction += transform.forward;
        if (Input.GetKey(m_DownKeyCode)) l_Direction -= transform.forward;

        float l_Speed = m_Speed;
        if (Input.GetKey(m_SprintKeyCode)) l_Speed = m_SprintSpeed;


        if (m_CharacterController.isGrounded)
        {
            if (m_VerticalSpeed < 0.0f) m_VerticalSpeed = -2.0f;
            if (Input.GetKeyDown(m_JumpKeyCode)) m_VerticalSpeed = m_JumpSpeed;
        }
        else m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;

        l_Direction.Normalize();
        l_Direction = l_Direction * l_Speed * Time.deltaTime;
        l_Direction.y = m_VerticalSpeed * Time.deltaTime;

        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Direction);


        if (CanShoot() && MustShoot())
        {  
           if (Input.GetMouseButtonDown(mouse_ShootButton0)) Shoot(m_Orange);
           if(Input.GetMouseButtonDown(mouse_ShootButton1)) Shoot(m_Blue);

<<<<<<< Updated upstream
        }
    }
=======
        }
       

        if (CanGrabObject()) 
        {
           GrabObject();
        } 
       
        UpdateAttachedObject(); 
        UpdateAttachingObject();


    }  



    bool CanGrabObject()
    {
      if (Input.GetKeyDown(m_GrabObject) && !attachingObject && !objecAttached)
        {
           Shoot(m_Orange);
            return true;
        } 
      return false;

    }  


    void GrabObject()
    {
        Ray lray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f)); 
        if (Physics.Raycast(lray,out RaycastHit lraycastHit, m_MaxShootDistance, m_ShootLayerMask.value))
        {
            if (lraycastHit.collider.CompareTag("Cube"))
            {
                GrabObject(lraycastHit.collider);
            }
        }
        if (CanReload() && MustReload())
    }
    void UpdateAttachingObject()
    {

        if (attachingObject)
        {
            float ldifftime = Time.time - GrabTIme;
            float lpct = Mathf.Clamp01(ldifftime / TimeToGrab);
            m_GrabObjectt.transform.position = Vector3.Lerp(m_GrabObjectt.transform.position, m_GrabTransform.position, lpct);
            m_GrabObjectt.transform.rotation = Quaternion.Lerp(m_GrabObjectt.transform.rotation, m_GrabTransform.rotation, lpct);

            if (lpct == 1.0f)
            {

                attachingObject = false;
                objecAttached = true;

            }
        }

    }
    void UpdateAttachedObject()
    {

        if (objecAttached)
        {
            if (Input.GetKeyUp(m_GrabObject))
            {
                ThrowAttachedObkect(0.0f);
            }
            else if (Input.GetMouseButtonDown(mouse_ShootButton0))
            {
                ThrowAttachedObkect(m_ShootAttachedObjectForce);

            }

        }

    }

    void GrabObject(Collider _Collider)
    {

        m_GrabObjectt = _Collider.GetComponent<Rigidbody>();
        m_GrabObjectt.isKinematic = true;
        attachingObject = true;
        m_GrabObjectt.transform.SetParent(m_GrabTransform);
        GrabTIme = Time.time;

    }  

    void ThrowAttachedObkect(float force)
    {
        m_GrabObjectt.isKinematic = false;
        m_GrabObjectt.linearVelocity =m_PitchController.forward*force;
        m_GrabObjectt.transform.SetParent(null);
        objecAttached = false;

    }

>>>>>>> Stashed changes
    bool CanReload()
    {
        return !m_IsReloading && m_CurrentAmmo < m_MagazineSize && m_ReserveAmmo > 0;
    }
    bool MustReload()
    {
        return true;
    }
    bool CanShoot()
    {
        return !m_IsReloading && m_CurrentAmmo > 0 && Time.time >= m_NextFireTime;
    }
    bool MustShoot()
    {
        return Input.GetMouseButtonDown(m_ShootButton);
        return Input.GetMouseButtonDown(mouse_ShootButton0) || Input.GetMouseButtonDown(mouse_ShootButton1);
    }


    /*
    void Shoot()
    {
        m_CurrentAmmo--;
        SetShootWeaponAnimation();

        m_NextFireTime = Time.time + m_RecoilTime;

    void Shoot(Portal _Portal) {
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));

        if (Physics.Raycast(l_Ray, out RaycastHit l_RayCastHit, m_MaxShootDistance, m_ShootLayerMask.value))
        {
            
        }
    } 
    */
        if (Physics.Raycast(l_Ray, out RaycastHit l_RayCastHit, m_MaxShootDistance, m_ShootLayerMask.value)) {
          
            if (_Portal.IsValidPosition(l_RayCastHit.point, l_RayCastHit.normal))
            {
                _Portal.transform.position = l_RayCastHit.point + l_RayCastHit.normal*0.001f;


    void Shoot(Portal _Portal) {
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));

        if (Physics.Raycast(l_Ray, out RaycastHit l_RayCastHit, m_MaxShootDistance, m_ShootLayerMask.value)) { }
      /*
        if (_Portal.IsValidPosition(l_RayCastHit.point, l_RayCastHit.normal)) 
        {
            _Portal.gameObject.SetActive(true);
                _Portal.transform.rotation = Quaternion.LookRotation(l_RayCastHit.normal);
                _Portal.gameObject.SetActive(true);
            }
            
        }
         else
        {
            _Portal.gameObject.SetActive(false);
        } 
      */
        
    }

void SetIdleWeaponAnimation()
    {
        m_Animation.CrossFade(m_IdleAnimationClip.name, 0.1f);
    }
    void SetShootWeaponAnimation()
    {
        m_Animation.CrossFade(m_ShootAnimationClip.name, 0.1f);
        m_Animation.CrossFadeQueued(m_IdleAnimationClip.name, 0.3f);
    }
    void SetReloadWeaponAnimation()
    {
        m_Animation.CrossFade(m_ReloadAnimationClip.name, 0.1f);
        m_Animation.CrossFadeQueued(m_IdleAnimationClip.name, 0.3f);
    }
    private void OnTriggerEnter(Collider other)
    {
<<<<<<< Updated upstream
        if (other.CompareTag("Item"))
        {
            //Item l_Item = other.GetComponent<Item>();
            //if (l_Item.CanPick()) l_Item.Pick();
        }
        else if (other.CompareTag("DeadZone"))
        {
            Kill();
        }
=======
       
        if (other.CompareTag("Portal"))
        {
            Portal portal= other.GetComponent<Portal>();
              
            if (CanTeleport(portal))
            {
                Teleport(portal);
            }
        } 
       
>>>>>>> Stashed changes
    }
    void Kill()
    {
        /*StartCoroutine(GameController.GetGameController().GetFade().FadeIn(() =>
        {
            GameController.GetGameController().GetFade().gameObject.SetActive(false);
            GameController.GetGameController().Restart();
        }));*/
<<<<<<< Updated upstream
=======
    } 
    public void Teleport(Portal portal)
    {
        Vector3 Worldlposition = transform.position + m_direction * m_teleportportalOffset;  
        Vector3 lWorldForward=transform.forward;
        Vector3 localPosition=portal.m_OtherPortalTransform.InverseTransformPoint(Worldlposition); 
        Vector3 localForward= portal.m_OtherPortalTransform.InverseTransformDirection(lWorldForward);
       
        localPosition = Quaternion.Euler(0f, 180f, 0f) * localPosition;
        localForward = Quaternion.Euler(0f, 180f, 0f) * localForward;

        m_CharacterController.enabled = false; 
        transform.position=portal.m_MirrorPortal.transform.TransformPoint(localPosition);
        transform.forward = portal.m_MirrorPortal.transform.TransformDirection(localForward); 
        m_Yaw= transform.eulerAngles.y;
        m_direction = Vector3.zero;
        m_CharacterController.enabled = true;  


    }  
    private bool CanTeleport(Portal portal)
    {
        float lmaxDotangle = Mathf.Cos(m_naxvalidangle*Mathf.Deg2Rad);
        float ldotAngle = Vector3.Dot(-m_direction, portal.transform.forward); 
        return ldotAngle >= lmaxDotangle;

>>>>>>> Stashed changes
    }
    public void Restart()
    {
        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
    }
}