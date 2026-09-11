using System.Globalization;
using TMPro;
using UnityEngine;

namespace AshenTrial
{
    [RequireComponent(typeof(TextMeshPro))]
    public sealed class DamageNumber : MonoBehaviour
    {
        private TMP_Text label;
        private Camera viewCamera;
        private Vector3 origin;
        private float startedAt;
        private float duration;
        private float rise;

        private void Awake() => label = GetComponent<TMP_Text>();

        public void Initialize(float amount, Camera camera, float lifetime, float riseDistance)
        {
            label.text = amount.ToString("0.##", CultureInfo.InvariantCulture);
            label.alpha = 1f;
            viewCamera = camera;
            origin = transform.position;
            startedAt = Time.unscaledTime;
            duration = Mathf.Max(0.01f, lifetime);
            rise = riseDistance;
        }

        private void LateUpdate()
        {
            float progress = Mathf.Clamp01((Time.unscaledTime - startedAt) / duration);
            transform.position = origin + Vector3.up * (rise * progress);
            if (viewCamera != null) transform.rotation = viewCamera.transform.rotation;
            label.alpha = 1f - progress;
            if (progress >= 1f) Destroy(gameObject);
        }
    }
}
