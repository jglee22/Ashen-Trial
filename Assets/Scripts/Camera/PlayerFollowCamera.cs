using UnityEngine;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class PlayerFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 8.8f, -7.2f);
        [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1f, 0f);
        [SerializeField, Min(0.01f)] private float followSpeed = 10f;
        private Vector3 followPosition;
        private float shakeUntil;
        private float shakeStrength;
        private float shakeDuration;
        private bool initialized;

        public Vector3 ShakeOffset { get; private set; }

        public void Shake(float strength, float duration)
        {
            if (!isActiveAndEnabled || strength <= 0f || duration <= 0f) return;
            shakeStrength = Mathf.Max(shakeStrength, strength);
            shakeDuration = Mathf.Max(shakeDuration, duration);
            shakeUntil = Mathf.Max(shakeUntil, Time.unscaledTime + duration);
        }

        private void OnEnable()
        {
            if (target == null || (lookOffset - offset).sqrMagnitude < 0.0001f ||
                Vector3.Cross(lookOffset - offset, Vector3.up).sqrMagnitude < 0.0001f)
            {
                Debug.LogError("PlayerFollowCamera: Target과 유효한 3/4 시점 Offset을 설정하세요.", this);
                enabled = false;
                return;
            }

            initialized = true;
            followPosition = target.position + offset;
            transform.SetPositionAndRotation(followPosition,
                Quaternion.LookRotation(lookOffset - offset, Vector3.up));
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                Debug.LogError("PlayerFollowCamera: Target이 없어 추적을 중단합니다.", this);
                enabled = false;
                return;
            }

            float blend = 1f - Mathf.Exp(-Mathf.Max(0.01f, followSpeed) * Time.deltaTime);
            followPosition = Vector3.Lerp(followPosition, target.position + offset, blend);
            transform.rotation = Quaternion.LookRotation(lookOffset - offset, Vector3.up);
            if (Time.unscaledTime < shakeUntil)
            {
                float strength = shakeStrength * Mathf.Clamp01((shakeUntil - Time.unscaledTime) / shakeDuration);
                // Deterministic visual noise leaves gameplay's Random stream untouched.
                ShakeOffset = (transform.right * Mathf.Sin(Time.unscaledTime * 83f) +
                    transform.up * Mathf.Sin(Time.unscaledTime * 107f)) * strength;
            }
            else
            {
                ShakeOffset = Vector3.zero;
                shakeStrength = 0f;
                shakeDuration = 0f;
            }
            transform.position = followPosition + ShakeOffset;
        }

        private void OnDisable()
        {
            ShakeOffset = Vector3.zero;
            shakeUntil = 0f;
            shakeStrength = 0f;
            shakeDuration = 0f;
            if (initialized) transform.position = followPosition;
        }
    }
}
