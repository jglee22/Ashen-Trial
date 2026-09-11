using UnityEngine;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    public sealed class HitFlashFeedback : MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        [SerializeField] private Renderer[] visualRenderers;
        [SerializeField] private bool replaceBaseMapDuringFlash;
        private MaterialPropertyBlock[] originals;
        private MaterialPropertyBlock[] flashes;
        private bool flashing;
        private float flashUntil;

        private void Awake()
        {
            if (visualRenderers == null || visualRenderers.Length == 0 ||
                System.Array.Exists(visualRenderers, visual => visual == null))
            {
                Debug.LogError("HitFlashFeedback: Visual Renderer references are required.", this);
                enabled = false;
                return;
            }
            originals = new MaterialPropertyBlock[visualRenderers.Length];
            flashes = new MaterialPropertyBlock[visualRenderers.Length];
            for (int i = 0; i < visualRenderers.Length; i++)
            {
                originals[i] = new MaterialPropertyBlock();
                flashes[i] = new MaterialPropertyBlock();
            }
        }

        public void Flash(Color color, float duration)
        {
            if (!isActiveAndEnabled || duration <= 0f) return;
            for (int i = 0; i < visualRenderers.Length; i++)
            {
                if (!flashing) visualRenderers[i].GetPropertyBlock(originals[i]);
                visualRenderers[i].GetPropertyBlock(flashes[i]);
                flashes[i].SetColor(BaseColor, color);
                // A palette texture otherwise keeps its dark colors even with a white tint.
                if (replaceBaseMapDuringFlash) flashes[i].SetTexture(BaseMap, Texture2D.whiteTexture);
                visualRenderers[i].SetPropertyBlock(flashes[i]);
            }
            flashing = true;
            flashUntil = Time.unscaledTime + duration;
        }

        private void Update()
        {
            if (flashing && Time.unscaledTime >= flashUntil) Restore();
        }

        private void Restore()
        {
            if (!flashing) return;
            for (int i = 0; i < visualRenderers.Length; i++)
                if (visualRenderers[i] != null) visualRenderers[i].SetPropertyBlock(originals[i]);
            flashing = false;
        }

        private void OnDisable() => Restore();
    }
}
