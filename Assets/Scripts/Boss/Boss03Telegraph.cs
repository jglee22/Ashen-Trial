using UnityEngine;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    public sealed class Boss03Telegraph : MonoBehaviour
    {
        [SerializeField] private BoxCollider dashBounds;
        [SerializeField] private Transform dashLine;
        [SerializeField] private Transform[] radialLines;
        [SerializeField] private Transform circle;
        [SerializeField] private float surfaceOffset = 0.03f;

        public bool IsVisible => dashLine.gameObject.activeSelf || circle.gameObject.activeSelf ||
            radialLines[0].gameObject.activeSelf;

        private void Awake()
        {
            if (dashBounds == null || dashLine == null || circle == null || radialLines == null ||
                radialLines.Length != 12 || System.Array.Exists(radialLines, line => line == null))
            {
                Debug.LogError("Boss03Telegraph: Dash bounds, dash line, circle and twelve radial lines are required.", this);
                enabled = false;
                return;
            }
            Hide();
        }

        public void ShowDash(float distance, float groundHeight)
        {
            Vector3 scale = dashBounds.transform.lossyScale;
            Vector3 size = Vector3.Scale(dashBounds.size,
                new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            Vector3 center = dashBounds.transform.TransformPoint(dashBounds.center) +
                dashBounds.transform.forward * (distance * 0.5f);
            center.y = groundHeight + surfaceOffset;
            dashLine.SetPositionAndRotation(center, dashBounds.transform.rotation);
            // Include the initial hitbox footprint as well as its full travel distance.
            dashLine.localScale = new Vector3(size.x, 0.02f, size.z + distance);
            dashLine.gameObject.SetActive(true);
        }

        public void ShowRadial(Vector3 origin, Vector3 forward, int count, float length,
            float width, float groundHeight)
        {
            origin.y = groundHeight + surfaceOffset;
            for (int i = 0; i < radialLines.Length; i++)
            {
                radialLines[i].gameObject.SetActive(i < count);
                if (i >= count) continue;
                Vector3 direction = Quaternion.Euler(0f, i * (360f / count), 0f) * forward;
                radialLines[i].SetPositionAndRotation(origin + direction * (length * 0.5f), Quaternion.LookRotation(direction));
                radialLines[i].localScale = new Vector3(width, 0.02f, length);
            }
        }

        public void ShowBurst(Vector3 position, float radius)
        {
            circle.SetPositionAndRotation(position + Vector3.up * surfaceOffset, Quaternion.identity);
            circle.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
            circle.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (dashLine != null) dashLine.gameObject.SetActive(false);
            if (circle != null) circle.gameObject.SetActive(false);
            if (radialLines != null)
                foreach (Transform line in radialLines)
                    if (line != null) line.gameObject.SetActive(false);
        }

        private void OnDisable() => Hide();
    }
}
