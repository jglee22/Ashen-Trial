using TMPro;
using UnityEngine;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    public sealed class GameplayDebugView : MonoBehaviour
    {
        [SerializeField] private PlayerMovement player;
        [SerializeField] private TMP_Text debugText;
        [SerializeField] private Health health;
        [SerializeField] private PlayerDodge dodge;
        [SerializeField] private BossController boss;
        [SerializeField] private Boss02Controller boss02;
        [SerializeField] private Boss03Controller boss03;
        [SerializeField] private GameFlowController gameFlow;
        [SerializeField] private PlayerCombat playerCombat;
        [SerializeField] private PlayerUpgradeState upgrades;
        [SerializeField, Min(0.05f)] private float refreshInterval = 0.2f;

        private float elapsedTime;
        private int frameCount;

        private void OnEnable()
        {
            if (player == null || debugText == null)
            {
                Debug.LogError("GameplayDebugView: Player와 TMP Debug Text를 연결하세요.", this);
                enabled = false;
                return;
            }

            elapsedTime = 0f;
            frameCount = 0;
            RefreshText(0f);
        }

        private void Update()
        {
            if (player == null || debugText == null)
            {
                Debug.LogError("GameplayDebugView: 참조가 없어 표시 갱신을 중단합니다.", this);
                enabled = false;
                return;
            }

            elapsedTime += Time.unscaledDeltaTime;
            frameCount++;
            if (elapsedTime < Mathf.Max(0.05f, refreshInterval))
                return;

            RefreshText(frameCount / elapsedTime);
            elapsedTime = 0f;
            frameCount = 0;
        }

        private void RefreshText(float fps)
        {
            Vector3 position = player.transform.position;
            debugText.SetText(
                "FPS: {0:0}\nPlayer Position: ({1:2}, {2:2}, {3:2})\nMove Speed: {4:2}",
                fps, position.x, position.y, position.z, player.MoveSpeed);
            if (health != null && dodge != null)
                debugText.text += $"\nHP: {health.CurrentHp:0} / {health.MaxHp:0}" +
                    $"\nDodge: {(dodge.IsDodging ? "Dodging" : dodge.CooldownRemaining > 0f ? $"Cooldown {dodge.CooldownRemaining:0.0}" : "Ready")}" +
                    $"\nDead: {health.IsDead}";
            if (gameFlow != null)
            {
                Health currentBoss = gameFlow.CurrentBossHealth;
                int index = gameFlow.CurrentBossIndex;
                debugText.text += $"\nRun Boss: {index + 1} / 3";
                if (currentBoss != null)
                    debugText.text += $"\nBoss HP: {currentBoss.CurrentHp:0} / {currentBoss.MaxHp:0}";
                int phase = index == 0 && boss != null ? (int)boss.CurrentFightPhase :
                    index == 1 && boss02 != null ? (boss02.PhaseTwo ? 2 : 1) :
                    index == 2 && boss03 != null ? (boss03.PhaseTwo ? 2 : 1) : 1;
                debugText.text += $"\nBoss Phase: {phase}";
                debugText.text += $"\nFlow: {gameFlow.State}";
            }
            if (playerCombat != null)
                debugText.text += $"\nDamage: {playerCombat.AttackDamage:0.##} / Final: {playerCombat.FinisherDamage:0.##}" +
                    $"\nAttack Speed: {playerCombat.AttackSpeedMultiplier:0.##}x";
            if (dodge != null)
                debugText.text += $"\nDodge Cooldown: {dodge.CooldownDuration:0.##}s";
            if (upgrades != null && upgrades.SelectedUpgrade != null)
                debugText.text += $"\nLast Upgrade: {upgrades.SelectedUpgrade.DisplayName}";
        }
    }
}
