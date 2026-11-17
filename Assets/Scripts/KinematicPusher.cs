using UnityEngine;

namespace XRPhysics
{
    /// <summary>
    /// Moves a kinematic Rigidbody to a target pose every physics step and keeps
    /// its linear and angular velocity in sync, so contacts with dynamic
    /// Rigidbodies push them instead of snapping through them.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public class KinematicPusher : MonoBehaviour
    {
        [Header("Speed limits")]
        [SerializeField]
        [Tooltip("Maximum linear speed in m/s used for contact impulses.")]
        float m_MaxSpeed = 3f;

        [SerializeField]
        [Tooltip("Maximum angular speed in degrees per second used for contact impulses.")]
        float m_MaxAngularSpeed = 720f;

        [Header("Teleport")]
        [SerializeField]
        [Tooltip("Target jumps larger than this distance are treated as a teleport: the body is moved without velocity.")]
        float m_SnapDistance = 0.5f;

        Rigidbody m_Body;

        protected void Awake()
        {
            m_Body = GetComponent<Rigidbody>();
            m_Body.isKinematic = true;
            m_Body.useGravity = false;
            m_Body.interpolation = RigidbodyInterpolation.Interpolate;
            m_Body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        /// <summary>
        /// Moves the kinematic body towards the target world pose and updates
        /// the velocity used for contact resolution.
        /// </summary>
        public void Push(Vector3 position, Quaternion rotation)
        {
            if (m_Body == null)
                m_Body = GetComponent<Rigidbody>();

            var step = Mathf.Max(Time.fixedDeltaTime, 0.0001f);

            Vector3 delta = position - m_Body.position;

            if (delta.magnitude > m_SnapDistance)
            {
                m_Body.position = position;
                m_Body.rotation = rotation;
                m_Body.linearVelocity = Vector3.zero;
                m_Body.angularVelocity = Vector3.zero;
                return;
            }

            Quaternion deltaRotation = rotation * Quaternion.Inverse(m_Body.rotation);
            deltaRotation.ToAngleAxis(out var angle, out var axis);
            if (angle > 180f)
                angle -= 360f;

            Vector3 angularVelocity = axis * (angle * Mathf.Deg2Rad / step);
            angularVelocity = Vector3.ClampMagnitude(angularVelocity, m_MaxAngularSpeed * Mathf.Deg2Rad);

            Vector3 velocity = Vector3.ClampMagnitude(delta / step, m_MaxSpeed);

            m_Body.linearVelocity = velocity;
            m_Body.angularVelocity = angularVelocity;
            m_Body.MovePosition(position);
            m_Body.MoveRotation(rotation);
        }
    }
}
