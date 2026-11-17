using UnityEngine;

namespace XRPhysics
{
    /// <summary>
    /// Kinematic sphere that follows a hand pose so the hand can push dynamic
    /// props (doors, boxes) with physical contact.
    /// </summary>
    [RequireComponent(typeof(KinematicPusher))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class HandPusher : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Hand pose to follow, for example Pinch Grab Pose or Aim Pose.")]
        Transform m_Target;

        [SerializeField]
        [Tooltip("Offset from the target transform, in the target local space.")]
        Vector3 m_LocalOffset = Vector3.zero;

        [SerializeField]
        [Tooltip("Follow the target rotation. Disable to keep the pusher upright.")]
        bool m_FollowRotation = true;

        KinematicPusher m_Pusher;

        void Awake()
        {
            m_Pusher = GetComponent<KinematicPusher>();
        }

        void FixedUpdate()
        {
            if (m_Target == null)
                return;

            Vector3 position = m_Target.TransformPoint(m_LocalOffset);
            Quaternion rotation = m_FollowRotation ? m_Target.rotation : transform.rotation;

            m_Pusher.Push(position, rotation);
        }
    }
}
