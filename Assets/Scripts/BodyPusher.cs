using UnityEngine;

namespace XRPhysics
{
    /// <summary>
    /// Kinematic body that follows the headset position and yaw so the player
    /// can push dynamic props (doors, boxes) with their body.
    /// </summary>
    [RequireComponent(typeof(KinematicPusher))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class BodyPusher : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Camera to follow. Falls back to Camera.main when empty.")]
        Transform m_Camera;

        [SerializeField]
        [Tooltip("Distance in meters the pusher is placed in front of the camera.")]
        float m_ForwardOffset = 0.1f;

        [SerializeField]
        [Tooltip("Capsule center height below the camera position, in meters.")]
        float m_CenterOffset = 0.45f;

        KinematicPusher m_Pusher;

        void Awake()
        {
            if (m_Camera == null && Camera.main != null)
                m_Camera = Camera.main.transform;

            m_Pusher = GetComponent<KinematicPusher>();
        }

        void FixedUpdate()
        {
            if (m_Camera == null)
                return;

            Vector3 forward = Vector3.ProjectOnPlane(m_Camera.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.ProjectOnPlane(m_Camera.up, Vector3.up);
            forward.Normalize();

            Vector3 position = new Vector3(
                m_Camera.position.x,
                m_Camera.position.y - m_CenterOffset,
                m_Camera.position.z) + forward * m_ForwardOffset;

            Quaternion rotation = Quaternion.Euler(0f, m_Camera.eulerAngles.y, 0f);

            m_Pusher.Push(position, rotation);
        }
    }
}
