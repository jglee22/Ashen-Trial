using UnityEngine;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    public sealed class DamageSource : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float damageAmount = 25f;

        private void OnTriggerEnter(Collider other)
        {
            // Only the player's root controller counts, not its attack trigger.
            if (!(other is CharacterController)) return;
            PlayerVitals player = other.GetComponent<PlayerVitals>();
            if (player != null) other.GetComponent<Health>().TakeDamage(damageAmount);
        }
    }
}
