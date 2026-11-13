using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using System.Linq;

public class FPSController : MonoBehaviour
{
    bool m_AngleLocked = false;
    public Transform m_PitchController;

    Rigidbody m_GrabObjectt;
    float m_Yaw;
    float m_Pitch;
    public float m_YawSpeed;
    public float m_PitchSpeed;
    public bool m_InvertedYaw;
    public bool m_InvertedPitch;
    public float m_MinPitch;
    public float m_MaxPitch;
    public float m_Speed;
    public float m_ShootButtonobjectforce = 20.0f;

   public bool  attachingObject=false;
   public bool objecAttached=false;
    float GrabTIme;


    CharacterController m_CharacterController;
    public float m_teleportportalOffset = 0.2f;

    float m_VerticalSpeed = 0.0f;
    public float m_JumpSpeed = 0.0f;
    public float m_SprintSpeed = 0.0f;

    Vector3 m_StartPosition;
    Quaternion m_StartRotation;

    [Header("Health")]
    
    public float m_MaxHealth = 100.0f;
    public float m_CurrentHealth = 100.0f;
   
    [Header("Portals")]
    public Portal m_PreviewOrange;
    public Portal m_PreviewBlue;
   
    public Color IsValidColor= new Color(0, 1, 0, 0.75f);
    public Color NoIsValidColor = new Color(1, 0, 0, 0.75f);
    Vector3 m_direction;
    public float m_naxvalidangle = 40f;  
    public float teleportPortalOffset=1.0f;  

    

    [Header("Shield")]
    public float m_MaxShield = 100.0f;
    public float m_CurrentShield = 100.0f;

    [Header("Shoot")]
    public Camera m_Camera;
    public float m_MaxShootDistance = 200.0f;
    
    public LayerMask m_grabLayermask;
    public LayerMask PortalLayerMask;
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
    public KeyCode m_GrabObject = KeyCode.E;
    public int mouse_ShootButton0 = 0;
    public int mouse_ShootButton1 = 1;

    [Header("Grab")]
    public Transform m_GrabTransform;
    public float TimeToGrab=0.01f;
    public float msjootAttacjedObjectforce = 20.0f;

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
        m_PreviewOrange.m_Player = this;
        m_PreviewBlue.m_Player = this;
        m_Orange.m_Player = this;
        m_Blue.m_Player = this;
        m_PreviewOrange = Instantiate(m_PreviewOrange);
        m_PreviewBlue = Instantiate(m_PreviewBlue);
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

        m_direction  = Vector3.zero;
        Vector3 l_Right = transform.right;
        Vector3 l_Forward = transform.forward;
        l_Right.y = 0.0f;
        l_Forward.y = 0.0f;
        l_Right.Normalize();
        l_Forward.Normalize();

        if (Input.GetKey(m_LeftKeyCode)) m_direction = -transform.right;
        else if (Input.GetKey(m_RightKeyCode)) m_direction = transform.right;
        if (Input.GetKey(m_UpKeyCode)) m_direction+= transform.forward;
        if (Input.GetKey(m_DownKeyCode))m_direction-= transform.forward;

        float l_Speed = m_Speed;
        if (Input.GetKey(m_SprintKeyCode)) l_Speed = m_SprintSpeed;


        if (m_CharacterController.isGrounded)
        {
            if (m_VerticalSpeed < 0.0f) m_VerticalSpeed = -2.0f;
            if (Input.GetKeyDown(m_JumpKeyCode)) m_VerticalSpeed = m_JumpSpeed;
        }
        else m_VerticalSpeed += Physics.gravity.y * Time.deltaTime; 
        m_direction.Normalize(); 

        Vector3 ldirection = m_direction * l_Speed * Time.deltaTime;
        ldirection.y = m_VerticalSpeed * Time.deltaTime; 
        CollisionFlags l_CollisionFlags = m_CharacterController.Move(ldirection);



        if (Input.GetMouseButton(0))
        {
            Debug.Log("BOT� PREMUT"); 
            m_PreviewOrange.gameObject.SetActive(true);
            PreviewPortal(m_PreviewOrange);

        } 
        if (Input.GetMouseButtonUp(0))
        {
            Shoot(m_Orange); 
            m_PreviewOrange.gameObject.SetActive(false);    
        }

        if (Input.GetMouseButton(mouse_ShootButton1))
        {
            PreviewPortal(m_PreviewBlue);
        }
        if (Input.GetMouseButtonUp(mouse_ShootButton1))
        {
            Shoot(m_Blue);
            m_PreviewBlue.gameObject.SetActive(false);
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
        return Input.GetKeyDown(m_GrabObject) && !attachingObject && !objecAttached;
    }

    void GrabObject()
    {
        Ray lray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f)); 
        if (Physics.Raycast(lray,out RaycastHit lraycastHit, m_MaxShootDistance, m_grabLayermask))
        {
            if (lraycastHit.collider.CompareTag("Cube"))
            {
                GrabObject(lraycastHit.collider);
            }
        }
    }

    void GrabObject(Collider _Collider)
    {
        m_GrabObjectt = _Collider.GetComponent<Rigidbody>();

        if (m_GrabObjectt == null)
            return;

        m_GrabObjectt.isKinematic = true;
        attachingObject = true;
        m_GrabObjectt.transform.SetParent(m_GrabTransform);

        GrabTIme = Time.time;
    }

    void UpdateAttachingObject()
    {
        if (attachingObject)
        {
            float ldifftime = Time.time - GrabTIme;
            float lpct = Mathf.Clamp01(ldifftime/GrabTIme);
            m_GrabObjectt.transform.position =Vector3.Lerp(m_GrabObjectt.transform.position, m_GrabTransform.position, lpct);
            m_GrabObjectt.transform.rotation = Quaternion.Lerp(m_GrabObjectt.transform.rotation, m_GrabTransform.rotation, lpct); 

            m_GrabObjectt.transform.position = Vector3.Lerp(m_GrabObjectt.transform.position, m_GrabTransform.position, lpct);

                attachingObject = false; 
                objecAttached = true;
                

            if (lpct >= 1.0f)
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
                ThrowAttachedObkect(m_ShootButtonobjectforce); 
            }      
        }
    }

    void ThrowAttachedObkect(float force)
    {   
        m_GrabObjectt.isKinematic = false; 
        m_GrabObjectt.linearVelocity=m_PitchController.forward*force; 
        m_GrabObjectt.transform.SetParent(null); 
        objecAttached=false;
    }
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
        return Input.GetMouseButtonDown(mouse_ShootButton0)|| Input.GetMouseButtonDown(mouse_ShootButton1);
    }


    /*
    void Shoot()
    {
        m_CurrentAmmo--;
        SetShootWeaponAnimation();

        m_NextFireTime = Time.time + m_RecoilTime;

        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));

        if (Physics.Raycast(l_Ray, out RaycastHit l_RayCastHit, m_MaxShootDistance, m_ShootLayerMask.value))
        {
            
        }
    } 
    */

    void ChangePreviewCOlor(Portal portalPreview,Color portalcolor)
    {
        if (portalPreview!=null) {
            List<Renderer> portalrenders = portalPreview.GetComponentsInChildren<Renderer>(true).ToList();
            for (int i = 0; i < portalrenders.Count; i++)
            {
                if (portalrenders[i] != null)
                {
                    Material l_Material = portalrenders[i].material;
                    l_Material.color = portalcolor;
                }
            }
        } 

    }
    void PreviewPortal(Portal previewportal)
    {
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));

        if (Physics.Raycast(l_Ray, out RaycastHit l_RayCastHit, m_MaxShootDistance,PortalLayerMask))
        {

            Debug.Log("Raycast ha detectat: " + l_RayCastHit.collider.name);
           // bool isValid = previewportal.IsValidPosition(l_RayCastHit.point, l_RayCastHit.normal);
           
            previewportal.transform.position =
              l_RayCastHit.point + l_RayCastHit.normal * 0.01f;

            previewportal.transform.rotation =
                Quaternion.LookRotation(l_RayCastHit.normal);

            previewportal.gameObject.SetActive(true); 

            ChangePreviewCOlor(previewportal,IsValidColor);
            
            
        }
        else
        {
            previewportal.gameObject.SetActive(false);
        }


    }



    void Shoot(Portal _Portal) {
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));

        if (Physics.Raycast(l_Ray, out RaycastHit l_RayCastHit, m_MaxShootDistance, PortalLayerMask))
        {
            if (_Portal.IsValidPosition(l_RayCastHit.point, l_RayCastHit.normal))
            {
                _Portal.transform.position = l_RayCastHit.point + l_RayCastHit.normal * 0.01f;

                _Portal.transform.rotation = Quaternion.LookRotation(l_RayCastHit.normal);

                _Portal.gameObject.SetActive(true);
            }
        }
      
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
       
        if (other.CompareTag("Portal"))
        {
            Portal portal= other.GetComponent<Portal>();
              
            if (CanTeleport(portal))
            {
                Teleport(portal);
            }
        } 
       
    }
    void Kill()
    {
        /*StartCoroutine(GameController.GetGameController().GetFade().FadeIn(() =>
        {
            GameController.GetGameController().GetFade().gameObject.SetActive(false);
            GameController.GetGameController().Restart();
        }));*/
    } 
    public void Teleport(Portal portal)
    {
        Vector3 Worldlposition = transform.position + m_direction * m_teleportportalOffset;  
        Vector3 lWorldForward=transform.forward;
        Vector3 localPosition=portal.m_OtherPortalTransform.InverseTransformPoint(Worldlposition); 
        Vector3 localForward= portal.m_OtherPortalTransform.InverseTransformDirection(lWorldForward);

        /*localPosition = Quaternion.Euler(0f, 180f, 0f) * localPosition;
        localForward = Quaternion.Euler(0f, 180f, 0f) * localForward;*/

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

    }
    public void Restart()
    {
        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
    }
}