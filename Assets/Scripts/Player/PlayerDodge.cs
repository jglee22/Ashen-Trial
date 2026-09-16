using UnityEngine;
using UnityEngine.InputSystem;

namespace AshenTrial
{
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class PlayerDodge : MonoBehaviour
    {
        [SerializeField] private PlayerConfig config;
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private Health health;
        [SerializeField] private PlayerUpgradeState upgrades;
        [SerializeField] private InputActionReference dodgeActionReference;
        [SerializeField] private GameAudio gameAudio;
        [SerializeField] private bool isDodging;
        private InputAction dodgeAction;
        private Vector3 direction;
        private float remaining;
        private float speed;
        private float readyAt;

        public bool IsDodging => isDodging;
        public Vector3 CurrentDodgeDirection => direction;
        public float DodgeDuration => config.DodgeDuration;
        public float CooldownRemaining => Mathf.Max(0f, readyAt - Time.time);
        public float CooldownDuration => config.DodgeCooldown * (upgrades != null ? upgrades.DodgeCooldownMultiplier : 1f);
        public bool CanDodge => isActiveAndEnabled && !health.IsDead && movement.isActiveAndEnabled &&
            !isDodging && CooldownRemaining <= 0f && combat.Phase == PlayerCombat.AttackPhase.Idle;

        private void Awake()
        {
            if (config == null || movement == null || combat == null || health == null ||
                dodgeActionReference == null || dodgeActionReference.action == null)
            {
                Debug.LogError("PlayerDodge: Missing references.", this);
                enabled = false;
                return;
            }
            dodgeAction = dodgeActionReference.action.Clone();
        }

        private void OnEnable() => dodgeAction?.Enable();
        private void OnDestroy() => dodgeAction?.Dispose();
        private void OnDisable()
        {
            dodgeAction?.Disable();
            isDodging = false;
            remaining = 0f;
            direction = Vector3.zero;
        }

        private void Update()
        {
            if (dodgeAction == null || !dodgeAction.WasPressedThisFrame() || !CanDodge) return;
            Vector2 input = movement.MoveInput;
            direction = input.sqrMagnitude > 0.0001f ?
                new Vector3(input.x, 0f, input.y).normalized :
                Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            remaining = config.DodgeDuration;
            speed = config.DodgeDistance / remaining;
            isDodging = true;
            gameAudio?.PlayDodge();
        }

        // Movement is the only CharacterController.Move caller.
        public Vector3 ConsumeDisplacement(float deltaTime)
        {
            if (!isDodging) return Vector3.zero;
            float step = Mathf.Min(Mathf.Max(0f, deltaTime), remaining);
            Vector3 displacement = direction * (speed * step);
            remaining -= step;
            if (remaining <= 0f)
            {
                isDodging = false;
                readyAt = Time.time + CooldownDuration;
            }
            return displacement;
        }
    }
}
