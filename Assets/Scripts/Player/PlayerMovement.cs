using UnityEngine;
using UnityEngine.InputSystem;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerConfig config;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private InputActionReference moveActionReference;
        [SerializeField] private PlayerDodge dodge;
        [SerializeField] private PlayerUpgradeState upgrades;

        private CharacterController characterController;
        private InputAction moveAction;
        private float verticalSpeed;
        private PlayerFollowCamera cameraFollow;

        public float MoveSpeed => config != null ? config.MoveSpeed * (upgrades != null ? upgrades.MoveSpeedMultiplier : 1f) : 0f;
        public Vector2 MoveInput => moveAction != null ?
            Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f) : Vector2.zero;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (config == null || aimCamera == null || moveActionReference == null ||
                moveActionReference.action == null)
            {
                Debug.LogError("PlayerMovement: PlayerConfig, Aim Camera, Player/Move 참조를 연결하세요.", this);
                enabled = false;
                return;
            }

            if (moveActionReference.action.expectedControlType != "Vector2")
            {
                Debug.LogError("PlayerMovement: Move Action은 Vector2 액션이어야 합니다.", this);
                enabled = false;
                return;
            }

            // 기존 바인딩을 재사용하되 공유 액션의 활성 상태를 변경하지 않는다.
            moveAction = moveActionReference.action.Clone();
            cameraFollow = aimCamera.GetComponent<PlayerFollowCamera>();
        }

        private void OnEnable()
        {
            if (moveAction != null)
                moveAction.Enable();
        }

        private void OnDisable()
        {
            if (moveAction != null)
                moveAction.Disable();
            verticalSpeed = 0f;
        }

        private void OnDestroy()
        {
            if (moveAction != null)
                moveAction.Dispose();
        }

        private void Update()
        {
            if (moveAction == null || !characterController.enabled)
                return;

            Vector2 input = MoveInput;
            Vector3 velocity = new Vector3(input.x, 0f, input.y) * MoveSpeed;
            Vector3 horizontalDisplacement = dodge != null && dodge.IsDodging ?
                dodge.ConsumeDisplacement(Time.deltaTime) : velocity * Time.deltaTime;

            // CharacterController는 중력을 자동 적용하지 않는다.
            if (characterController.isGrounded && verticalSpeed < 0f)
                verticalSpeed = -2f;
            verticalSpeed += Physics.gravity.y * Time.deltaTime;
            velocity.y = verticalSpeed;
            CollisionFlags collisionFlags = characterController.Move(
                horizontalDisplacement + Vector3.up * (verticalSpeed * Time.deltaTime));
            if ((collisionFlags & CollisionFlags.Above) != 0 && verticalSpeed > 0f)
                verticalSpeed = 0f;

            RotateTowardsPointer();
        }

        private void RotateTowardsPointer()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null)
                return;

            Ray ray = aimCamera.ScreenPointToRay(pointer.position.ReadValue());
            if (cameraFollow != null) ray.origin -= cameraFollow.ShakeOffset;
            Plane aimPlane = new Plane(Vector3.up, transform.position);
            if (!aimPlane.Raycast(ray, out float distance))
                return;

            Vector3 direction = ray.GetPoint(distance) - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRotation, config.RotationSpeed * Time.deltaTime);
        }
    }
}
