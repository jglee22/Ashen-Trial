using UnityEngine;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    public sealed class Boss02Telegraph : MonoBehaviour
    {
        [SerializeField] private Transform[] lines;
        [SerializeField] private Transform circle;
        [SerializeField] private float surfaceOffset = 0.03f;

        public bool IsVisible => circle.gameObject.activeSelf || lines[0].gameObject.activeSelf;

        private void Awake()
        {
            if (circle == null || lines == null || lines.Length != 5 || System.Array.Exists(lines, line => line == null))
            {
                Debug.LogError("Boss02Telegraph: Circle and five line references are required.", this);
                enabled = false;
                return;
            }
            Hide();
        }

        public void ShowShots(Vector3 origin, Vector3 direction, int count, float angleStep,
            float length, float width, float groundHeight)
        {
            circle.gameObject.SetActive(false);
            origin.y = groundHeight + surfaceOffset;
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].gameObject.SetActive(i < count);
                if (i >= count) continue;
                Vector3 shotDirection = Quaternion.Euler(0f, (i - (count - 1) * 0.5f) * angleStep, 0f) * direction;
                lines[i].SetPositionAndRotation(origin + shotDirection * (length * 0.5f), Quaternion.LookRotation(shotDirection));
                lines[i].localScale = new Vector3(width, 0.02f, length);
            }
        }

        public void ShowCircle(Vector3 position, float radius)
        {
            foreach (Transform line in lines) line.gameObject.SetActive(false);
            // The boss can turn during windup; keep this child at the locked world position.
            circle.position = position + Vector3.up * surfaceOffset;
            circle.rotation = Quaternion.identity;
            circle.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
            circle.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (lines != null)
                foreach (Transform line in lines)
                    if (line != null) line.gameObject.SetActive(false);
            if (circle != null) circle.gameObject.SetActive(false);
        }

        private void OnDisable() => Hide();
    }
}
