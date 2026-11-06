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
    bool m_AttachingObject = false;
    bool m_ObjectAttached = false;
    float m_GrabTime;

    Rigidbody m_GrabObject;
    float m_Yaw;
    float m_Pitch;
    Vector3 m_Direction;
    public float m_YawSpeed;
    public float m_PitchSpeed;
    public bool m_InvertedYaw;
    public bool m_InvertedPitch;
    public float m_MinPitch;
    public float m_MaxPitch;
    public float m_Speed;
    CharacterController m_CharacterController;

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

    [Header("Portals")]
    public Portal m_Blue;
    public Portal m_Orange;
    public float m_TeleportPortalOffset = 1.0f;
    public float m_MaxValidAngle = 40.0f;

    [Header("Grab")]
    public Transform m_GrabTransfrom;
    public float m_TimeToGrab = 0.0f;
    public float m_TeleportPortalOffset1 = 1.0f;
    public float m_ShootAttachedObjectForce = 20.0f;

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
    public KeyCode m_GrabObjectKeyCode = KeyCode.E;
    public KeyCode m_SprintKeyCode = KeyCode.LeftShift;
    public KeyCode m_DebugLockAngleKeyCode = KeyCode.I;
    public KeyCode m_DebugLockCursorKeyCode = KeyCode.O;
    public int m_ShootButton = 0;

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

        m_Direction = Vector3.zero;
        Vector3 l_Right = transform.right;
        Vector3 l_Forward = transform.forward;
        l_Right.y = 0.0f;
        l_Forward.y = 0.0f;
        l_Right.Normalize();
        l_Forward.Normalize();

        if (Input.GetKey(m_LeftKeyCode)) m_Direction = -transform.right;
        else if (Input.GetKey(m_RightKeyCode)) m_Direction = transform.right;
        if (Input.GetKey(m_UpKeyCode)) m_Direction += transform.forward;
        if (Input.GetKey(m_DownKeyCode)) m_Direction -= transform.forward;

        float l_Speed = m_Speed;
        if (Input.GetKey(m_SprintKeyCode)) l_Speed = m_SprintSpeed;


        if (m_CharacterController.isGrounded)
        {
            if (m_VerticalSpeed < 0.0f) m_VerticalSpeed = -2.0f;
            if (Input.GetKeyDown(m_JumpKeyCode)) m_VerticalSpeed = m_JumpSpeed;
        }
        else m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;

        m_Direction.Normalize();
        Vector3 l_Direction = m_Direction * l_Speed * Time.deltaTime;
        l_Direction.y = m_VerticalSpeed * Time.deltaTime;
        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Direction);

        if (CanGrabObject())
        {
            GrabObject();
        }

        if (CanShoot() && MustShoot())
        {
           //Shoot(m_BluePortal);
        }

        UpdateAttachingObject();
        UpdateAttachedObject();
    }

    bool CanGrabObject()
    {
        if (Input.GetKeyDown(m_GrabObjectKeyCode) && !m_AttachingObject && !m_ObjectAttached)
        {
            return true;
        }
        return false;
    }
    void GrabObject()
    {
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_MaxShootDistance, m_ShootLayerMask.value))
        {
            if (l_RaycastHit.collider.CompareTag("Cube"))
            {
                GrabObject(l_RaycastHit.collider);
            }
        }
    }
    void UpdateAttachingObject()
    {
        if (m_AttachingObject)
        {
            float l_DiffTime = Time.time - m_GrabTime;
            float l_Pct = Mathf.Min(1.0f, l_DiffTime / m_TimeToGrab);
            m_GrabObject.transform.position = Vector3.Lerp(m_GrabObject.transform.position, m_GrabTransfrom.position, l_Pct);
            m_GrabObject.transform.rotation = Quaternion.Lerp(m_GrabObject.transform.rotation, m_GrabTransfrom.rotation, l_Pct);
            if (l_Pct == 1.0f)
            {
                m_AttachingObject = false;
                m_ObjectAttached = true;
            }
        }
    }
    void UpdateAttachedObject()
    {
        if (m_ObjectAttached)
        {
            if (Input.GetKeyUp(m_GrabObjectKeyCode))
            {
                ThrowAttachedObject(0.0f);
            }
            else if (Input.GetMouseButtonDown(m_ShootButton))
            {
                ThrowAttachedObject(m_ShootAttachedObjectForce);
            }
        }
    }
    void GrabObject(Collider _Collider)
    {
        m_GrabObject = _Collider.GetComponent<Rigidbody>();
        m_GrabObject.isKinematic = true;
        m_AttachingObject= true;
        m_GrabObject.transform.SetParent(m_GrabTransfrom);
        m_GrabTime = Time.time;
    }
    void ThrowAttachedObject(float Force)
    {
        m_GrabObject.isKinematic = false;
        m_GrabObject.linearVelocity = m_PitchController.forward * Force;
        m_GrabObject.transform.SetParent(null);
        m_ObjectAttached = false;
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
        return Input.GetMouseButtonDown(m_ShootButton);
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


    void Shoot(Portal _Portal) {
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));

        if (Physics.Raycast(l_Ray, out RaycastHit l_RayCastHit, m_MaxShootDistance, m_ShootLayerMask.value)) { }
      /*
        if (_Portal.IsValidPosition(l_RayCastHit.point, l_RayCastHit.normal)) 
        {
            _Portal.gameObject.SetActive(true);
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
        if (other.CompareTag("DeadZone"))
        {
           Kill();
        }
        else if (other.CompareTag("portal"))
        {
            Portal l_Portal = other.GetComponent<Portal>();
            if (CanTeleport(l_Portal))
            {
                Teleport(other.GetComponent<Portal>());
            }
        }
    }
    private bool CanTeleport(Portal _portal)
    {
        float l_MaxDotAngle = Mathf.Cos(m_MaxValidAngle * Mathf.Deg2Rad);
        float l_DotAngle = Vector3.Dot(-m_Direction, _portal.transform.forward);
        return l_DotAngle >= l_MaxDotAngle;
    }
    void Teleport(Portal _portal)
    {
        Vector3 l_WorldPosition = transform.position + m_Direction * m_TeleportPortalOffset1;
        Vector3 l_WorldForward = transform.forward;
        Vector3 l_LocalPosition = _portal.m_OtherPortalTransform.InverseTransformPoint(l_WorldPosition);
        Vector3 l_LocalForward = _portal.m_OtherPortalTransform.InverseTransformDirection(l_WorldForward);

        m_CharacterController.enabled = false;
        transform.position = _portal.m_MirrorPortal.transform.TransformPoint(l_LocalPosition);
        transform.forward = _portal.m_MirrorPortal.transform.TransformDirection(l_LocalForward);
        m_Yaw = transform.rotation.eulerAngles.y;
        m_Direction = Vector3.zero;
        m_CharacterController.enabled = true;
        Debug.Log("Teleport" + _portal.name);
    }
    void Kill()
    {
        /*StartCoroutine(GameController.GetGameController().GetFade().FadeIn(() =>
        {
            GameController.GetGameController().GetFade().gameObject.SetActive(false);
            GameController.GetGameController().Restart();
        }));*/
    }
    public void Restart()
    {
        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
    }
}