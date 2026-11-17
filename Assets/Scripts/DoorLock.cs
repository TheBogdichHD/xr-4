using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace XRPhysics
{
    /// <summary>
    /// Keeps a physical hinge door locked until a key with the matching id is
    /// inserted into the socket. Locked means the door body is kinematic and
    /// the hinge limits are clamped to zero, so the panel cannot move at all.
    /// </summary>
    [DisallowMultipleComponent]
    public class DoorLock : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        [Tooltip("Hinge joint of the door panel.")]
        HingeJoint m_Hinge;

        [SerializeField]
        [Tooltip("Socket placed at the keyhole.")]
        XRSocketInteractor m_Socket;

        [SerializeField]
        [Tooltip("Rigidbody of the door panel. Found on the same object when empty.")]
        Rigidbody m_Rigidbody;

        [Header("Lock")]
        [SerializeField]
        [Tooltip("Key id accepted by this lock.")]
        string m_RequiredKeyId = "red";

        [SerializeField]
        [Tooltip("Lock the door again when the key is removed. Leave off so the door stays open.")]
        bool m_RelockWhenKeyRemoved;

        [Header("Unlocked hinge limits")]
        [SerializeField]
        float m_OpenMin = -90f;

        [SerializeField]
        float m_OpenMax = 90f;

        [SerializeField]
        [Tooltip("Caps the angular speed of the door so it feels heavy instead of spinning freely.")]
        float m_MaxAngularVelocity = 3f;

        bool m_IsUnlocked;

        public bool IsUnlocked => m_IsUnlocked;

        void Reset()
        {
            m_Hinge = GetComponent<HingeJoint>();
        }

        void Awake()
        {
            if (m_Hinge == null)
                m_Hinge = GetComponent<HingeJoint>();

            if (m_Hinge == null)
            {
                Debug.LogError("DoorLock: no HingeJoint assigned.", this);
                enabled = false;
                return;
            }

            if (m_Rigidbody == null)
                m_Rigidbody = GetComponent<Rigidbody>();

            if (m_Rigidbody == null)
            {
                Debug.LogError("DoorLock: no Rigidbody assigned.", this);
                enabled = false;
                return;
            }

            // The hinge owns the rotation axis, so the body must not freeze any axis.
            m_Rigidbody.constraints = RigidbodyConstraints.None;
            m_Rigidbody.maxAngularVelocity = m_MaxAngularVelocity;
            m_Hinge.useLimits = true;
            m_Hinge.useSpring = false;

            Lock();
        }

        void OnEnable()
        {
            if (m_Socket == null)
                return;

            m_Socket.selectEntered.AddListener(OnSocketSelectEntered);
            m_Socket.selectExited.AddListener(OnSocketSelectExited);
        }

        void OnDisable()
        {
            if (m_Socket == null)
                return;

            m_Socket.selectEntered.RemoveListener(OnSocketSelectEntered);
            m_Socket.selectExited.RemoveListener(OnSocketSelectExited);
        }

        void OnSocketSelectEntered(SelectEnterEventArgs args)
        {
            var key = args.interactableObject.transform.GetComponentInParent<KeyItem>();

            if (key != null && key.KeyId == m_RequiredKeyId)
            {
                Unlock();
                return;
            }

            StartCoroutine(RejectNextFrame(args.manager, args.interactorObject, args.interactableObject));
        }

        void OnSocketSelectExited(SelectExitEventArgs args)
        {
            if (!m_RelockWhenKeyRemoved || m_Socket.hasSelection)
                return;

            Lock();
        }

        void Lock()
        {
            m_IsUnlocked = false;
            ApplyLimits(0f, 0f);

            // A kinematic body cannot be pushed and does not jitter against the
            // frame or the player, unlike a dynamic body clamped to zero limits.
            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
            m_Rigidbody.isKinematic = true;
        }

        void Unlock()
        {
            m_IsUnlocked = true;
            ApplyLimits(m_OpenMin, m_OpenMax);

            // Clear any stored velocity before the body becomes dynamic again,
            // otherwise the door jerks on the frame when the key is inserted.
            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
            m_Rigidbody.isKinematic = false;
            m_Rigidbody.WakeUp();
        }

        void ApplyLimits(float min, float max)
        {
            var limits = m_Hinge.limits;
            limits.min = min;
            limits.max = max;
            m_Hinge.limits = limits;
        }

        IEnumerator RejectNextFrame(
            XRInteractionManager manager,
            IXRSelectInteractor interactor,
            IXRSelectInteractable interactable)
        {
            // Exiting a select from inside the selectEntered callback is unsafe, wait one frame.
            yield return null;

            if (manager != null)
                manager.SelectExit(interactor, interactable);
        }
    }
}
