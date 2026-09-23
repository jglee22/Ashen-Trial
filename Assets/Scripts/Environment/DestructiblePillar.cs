using UnityEngine;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    public sealed class DestructiblePillar : MonoBehaviour
    {
        private enum ChunkGroup { Base, Lower, Middle, Upper, Capital, Debris }

        [SerializeField] private GameObject intactRoot;
        [SerializeField] private Collider intactCollider;
        [SerializeField] private GameObject fracturedRoot;
        [SerializeField] private Transform impactPoint;
        [SerializeField] private ParticleSystem dust;
        [SerializeField] private GameAudio gameAudio;
        [SerializeField] private PlayerFollowCamera followCamera;
        [Header("Shake")]
        [SerializeField, Min(0f)] private float shakeStrength = 0.2f;
        [SerializeField, Min(0f)] private float shakeDuration = 0.22f;
        [Header("Break Speeds")]
        [SerializeField, Min(0f)] private float lowerSpeed = 2.2f;
        [SerializeField, Min(0f)] private float middleSpeed = 3.4f;
        [SerializeField, Min(0f)] private float upperSpeed = 4.4f;
        [SerializeField, Min(0f)] private float capitalSpeed = 5.0f;
        [SerializeField, Min(0f)] private float debrisSpeed = 6.2f;
        [SerializeField, Min(0f)] private float lowerLift = 0.25f;
        [SerializeField, Min(0f)] private float middleLift = 0.7f;
        [SerializeField, Min(0f)] private float upperLift = 1.15f;
        [SerializeField, Min(0f)] private float capitalLift = 1.35f;
        [SerializeField, Min(0f)] private float debrisLift = 1.1f;
        [SerializeField, Range(0f, 0.3f)] private float directionJitter = 0.08f;
        [Header("Torque")]
        [SerializeField, Min(0f)] private float lowerTorque = 1.1f;
        [SerializeField, Min(0f)] private float middleTorque = 1.6f;
        [SerializeField, Min(0f)] private float upperTorque = 2.0f;
        [SerializeField, Min(0f)] private float capitalTorque = 2.2f;
        [SerializeField, Min(0f)] private float debrisTorque = 3.0f;
        [Header("Limits")]
        [SerializeField, Min(0.01f)] private float maxHorizontalSpeed = 7.5f;
        [SerializeField, Min(0.1f)] private float settleDelay = 3.2f;
        [SerializeField, Min(0.1f)] private float debrisHideDelay = 5.5f;
        [Header("Chunk Names")]
        [SerializeField] private string baseName = "Chunk_Base";
        [SerializeField] private string lowerPrefix = "Chunk_Lower";
        [SerializeField] private string middlePrefix = "Chunk_Middle";
        [SerializeField] private string upperPrefix = "Chunk_Upper";
        [SerializeField] private string capitalName = "Chunk_Capital";
        [SerializeField] private string debrisPrefix = "Debris_";
        private struct ChunkRestState
        {
            public Vector3 localPosition;
            public Quaternion localRotation;
            public Vector3 localScale;
            public bool isKinematic;
            public bool useGravity;
            public bool colliderEnabled;
            public bool hasCollider;
            public bool gameObjectActive;
        }

        private Rigidbody[] bodies;
        private ChunkRestState[] restStates;
        private Coroutine settleRoutine;
        private bool isBroken;

        public bool IsBroken => isBroken;

        private void Awake()
        {
            if (intactRoot == null || intactCollider == null || fracturedRoot == null)
            {
                Debug.LogError("DestructiblePillar: Intact root, Intact collider and Fractured root are required.", this);
                enabled = false;
                return;
            }

            bodies = fracturedRoot.GetComponentsInChildren<Rigidbody>(true);
            PrepareRestingBodies();
            CaptureRestStates();
            intactRoot.SetActive(true);
            intactCollider.enabled = true;
            fracturedRoot.SetActive(false);
        }

        public bool TryBreak(Vector3 impactOrigin, Vector3 chargeDirection)
        {
            if (!isActiveAndEnabled || isBroken) return false;
            isBroken = true;

            Vector3 direction = Flatten(chargeDirection);
            if (direction.sqrMagnitude < 0.0001f)
                direction = Flatten(transform.position - impactOrigin);
            if (direction.sqrMagnitude < 0.0001f)
                direction = transform.forward;
            direction.Normalize();

            intactRoot.SetActive(false);
            intactCollider.enabled = false;
            fracturedRoot.SetActive(true);
            IgnoreCharacterControllers();
            ApplyBreakForces(direction);
            PlayFeedback();
            settleRoutine = StartCoroutine(SettlePhysics());
            return true;
        }

        public void ResetPillar()
        {
            if (bodies == null || restStates == null) return;
            if (settleRoutine != null)
            {
                StopCoroutine(settleRoutine);
                settleRoutine = null;
            }
            if (dust != null)
                dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            for (int i = 0; i < bodies.Length; i++)
            {
                Rigidbody body = bodies[i];
                if (body == null) continue;
                if (!body.isKinematic)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
                ChunkRestState rest = restStates[i];
                body.isKinematic = rest.isKinematic;
                body.useGravity = rest.useGravity;
            }

            fracturedRoot.SetActive(false);
            for (int i = 0; i < bodies.Length; i++)
            {
                Rigidbody body = bodies[i];
                if (body == null) continue;
                ChunkRestState rest = restStates[i];
                body.gameObject.SetActive(rest.gameObjectActive);
                Transform chunk = body.transform;
                chunk.localPosition = rest.localPosition;
                chunk.localRotation = rest.localRotation;
                chunk.localScale = rest.localScale;
                if (rest.hasCollider && body.TryGetComponent(out Collider collider))
                    collider.enabled = rest.colliderEnabled;
            }

            intactRoot.SetActive(true);
            intactCollider.enabled = true;
            isBroken = false;
        }

        private void PrepareRestingBodies()
        {
            if (bodies == null) return;
            for (int i = 0; i < bodies.Length; i++)
            {
                Rigidbody body = bodies[i];
                if (body == null) continue;
                body.useGravity = Classify(body) != ChunkGroup.Base;
                body.isKinematic = true;
            }
        }

        private void CaptureRestStates()
        {
            restStates = new ChunkRestState[bodies.Length];
            for (int i = 0; i < bodies.Length; i++)
            {
                Rigidbody body = bodies[i];
                if (body == null) continue;
                Transform chunk = body.transform;
                restStates[i].localPosition = chunk.localPosition;
                restStates[i].localRotation = chunk.localRotation;
                restStates[i].localScale = chunk.localScale;
                restStates[i].isKinematic = body.isKinematic;
                restStates[i].useGravity = body.useGravity;
                restStates[i].gameObjectActive = body.gameObject.activeSelf;
                if (body.TryGetComponent(out Collider collider))
                {
                    restStates[i].hasCollider = true;
                    restStates[i].colliderEnabled = collider.enabled;
                }
            }
        }

        private void IgnoreCharacterControllers()
        {
            CharacterController[] characters = FindObjectsByType<CharacterController>(FindObjectsSortMode.None);
            for (int i = 0; i < bodies.Length; i++)
            {
                if (bodies[i] == null || !bodies[i].TryGetComponent(out Collider chunkCollider)) continue;
                for (int j = 0; j < characters.Length; j++)
                {
                    if (characters[j] != null)
                        Physics.IgnoreCollision(chunkCollider, characters[j], true);
                }
            }
        }

        private void ApplyBreakForces(Vector3 impactDirection)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                Rigidbody body = bodies[i];
                if (body == null) continue;
                ChunkGroup group = Classify(body);
                if (group == ChunkGroup.Base)
                {
                    body.useGravity = false;
                    body.isKinematic = true;
                    continue;
                }

                body.isKinematic = false;
                body.useGravity = true;
                Vector3 jittered = (impactDirection + new Vector3(
                    Random.Range(-directionJitter, directionJitter),
                    0f,
                    Random.Range(-directionJitter, directionJitter))).normalized;
                Vector3 velocity = jittered * SpeedFor(group) + Vector3.up * LiftFor(group);
                velocity.x = Mathf.Clamp(velocity.x, -maxHorizontalSpeed, maxHorizontalSpeed);
                velocity.z = Mathf.Clamp(velocity.z, -maxHorizontalSpeed, maxHorizontalSpeed);
                body.linearVelocity = velocity;
                body.angularVelocity = Random.insideUnitSphere * TorqueFor(group);
            }
        }

        private void PlayFeedback()
        {
            if (dust != null)
            {
                dust.transform.position = impactPoint != null ? impactPoint.position :
                    transform.position + Vector3.up * 1.15f;
                dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                dust.Play(true);
            }

            gameAudio?.PlayStoneBreak();
            if (followCamera != null)
                followCamera.Shake(shakeStrength, shakeDuration);
        }

        private System.Collections.IEnumerator SettlePhysics()
        {
            yield return new WaitForSeconds(settleDelay);
            for (int i = 0; i < bodies.Length; i++)
            {
                Rigidbody body = bodies[i];
                if (body == null) continue;
                if (!body.isKinematic)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
                body.isKinematic = true;
                ChunkGroup group = Classify(body);
                if (group == ChunkGroup.Debris || group == ChunkGroup.Base) continue;
                if (body.TryGetComponent(out Collider collider))
                    collider.enabled = false;
            }

            yield return new WaitForSeconds(Mathf.Max(0f, debrisHideDelay - settleDelay));
            for (int i = 0; i < bodies.Length; i++)
            {
                Rigidbody body = bodies[i];
                if (body == null || Classify(body) != ChunkGroup.Debris) continue;
                body.gameObject.SetActive(false);
            }
        }

        private ChunkGroup Classify(Rigidbody body)
        {
            string name = body.name;
            if (name == baseName) return ChunkGroup.Base;
            if (name == capitalName) return ChunkGroup.Capital;
            if (name.StartsWith(debrisPrefix)) return ChunkGroup.Debris;
            if (name.StartsWith(lowerPrefix)) return ChunkGroup.Lower;
            if (name.StartsWith(middlePrefix)) return ChunkGroup.Middle;
            if (name.StartsWith(upperPrefix)) return ChunkGroup.Upper;
            return ChunkGroup.Middle;
        }

        private float SpeedFor(ChunkGroup group)
        {
            switch (group)
            {
                case ChunkGroup.Lower: return lowerSpeed;
                case ChunkGroup.Middle: return middleSpeed;
                case ChunkGroup.Upper: return upperSpeed;
                case ChunkGroup.Capital: return capitalSpeed;
                case ChunkGroup.Debris: return debrisSpeed;
                default: return middleSpeed;
            }
        }

        private float LiftFor(ChunkGroup group)
        {
            switch (group)
            {
                case ChunkGroup.Lower: return lowerLift;
                case ChunkGroup.Middle: return middleLift;
                case ChunkGroup.Upper: return upperLift;
                case ChunkGroup.Capital: return capitalLift;
                case ChunkGroup.Debris: return debrisLift;
                default: return middleLift;
            }
        }

        private float TorqueFor(ChunkGroup group)
        {
            switch (group)
            {
                case ChunkGroup.Lower: return lowerTorque;
                case ChunkGroup.Middle: return middleTorque;
                case ChunkGroup.Upper: return upperTorque;
                case ChunkGroup.Capital: return capitalTorque;
                case ChunkGroup.Debris: return debrisTorque;
                default: return middleTorque;
            }
        }

        private static Vector3 Flatten(Vector3 value)
        {
            value.y = 0f;
            return value;
        }
    }
}
