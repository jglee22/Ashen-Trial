using UnityEngine;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    public sealed class BossTelegraph : MonoBehaviour
    {
        [SerializeField] private BoxCollider attackBounds;
        [SerializeField] private Transform meleeArea;
        [SerializeField] private Transform chargeLine;
        [SerializeField] private Transform circleArea;
        [SerializeField, Min(0.001f)] private float surfaceHeight = 0.03f;

        public bool IsVisible => meleeArea.gameObject.activeSelf ||
            chargeLine.gameObject.activeSelf || circleArea.gameObject.activeSelf;

        private void Awake()
        {
            if (attackBounds == null || meleeArea == null || chargeLine == null || circleArea == null)
            {
                Debug.LogError("BossTelegraph: Attack Bounds and all three visual references are required.", this);
                enabled = false;
                return;
            }
            Hide();
        }

        public void Show(BossController.Pattern pattern, float chargeDistance, float aoeRadius)
        {
            if (!isActiveAndEnabled) return;
            Vector3 center = attackBounds.transform.TransformPoint(attackBounds.center);
            center.y = transform.position.y + surfaceHeight;
            Vector3 scale = attackBounds.transform.lossyScale;
            Vector3 size = Vector3.Scale(attackBounds.size,
                new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            meleeArea.SetPositionAndRotation(center, attackBounds.transform.rotation);
            meleeArea.localScale = new Vector3(size.x, surfaceHeight, size.z);
            // Charge moves the boss root ChargeDistance. Keep the line start at the current
            // hitbox front, but place the far edge on that root stop — not another ChargeDistance
            // beyond the hitbox.
            Vector3 chargeEnd = new Vector3(transform.position.x, center.y, transform.position.z) +
                transform.forward * chargeDistance;
            Vector3 chargeStart = center + transform.forward * (size.z * 0.5f);
            float lineLength = Mathf.Max(0.001f, Vector3.Dot(chargeEnd - chargeStart, transform.forward));
            chargeLine.SetPositionAndRotation(chargeStart + transform.forward * (lineLength * 0.5f),
                transform.rotation);
            chargeLine.localScale = new Vector3(size.x, surfaceHeight, lineLength);
            circleArea.position = new Vector3(transform.position.x, center.y, transform.position.z);
            circleArea.localScale = new Vector3(aoeRadius * 2f, surfaceHeight * 0.5f, aoeRadius * 2f);
            meleeArea.gameObject.SetActive(pattern == BossController.Pattern.MeleeCombo || pattern == BossController.Pattern.Charge);
            chargeLine.gameObject.SetActive(pattern == BossController.Pattern.Charge);
            circleArea.gameObject.SetActive(pattern == BossController.Pattern.CircleAoE);
        }

        public void Hide()
        {
            if (meleeArea != null) meleeArea.gameObject.SetActive(false);
            if (chargeLine != null) chargeLine.gameObject.SetActive(false);
            if (circleArea != null) circleArea.gameObject.SetActive(false);
        }

        private void OnDisable() => Hide();
    }
}
