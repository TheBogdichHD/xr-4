using UnityEngine;

namespace XRPhysics
{
    /// <summary>
    /// Marks a grabbable object as a key and identifies which lock it opens.
    /// </summary>
    [DisallowMultipleComponent]
    public class KeyItem : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Identifier compared against DoorLock.Required Key Id.")]
        string m_KeyId = "red";

        public string KeyId => m_KeyId;
    }
}
