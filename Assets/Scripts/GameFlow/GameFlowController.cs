using UnityEngine;
using UnityEngine.SceneManagement;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    public sealed class GameFlowController : MonoBehaviour
    {
        public enum GameFlowState { Combat, BossDefeated, GameOver, RunComplete }

        [System.Serializable]
        private sealed class BossEncounter
        {
            public GameObject root;
            public Health health;
        }

        [SerializeField] private Health playerHealth;
        [SerializeField] private BossEncounter[] bosses;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerCombat playerCombat;
        [SerializeField] private PlayerDodge playerDodge;
        [SerializeField] private GameObject bossDefeatedPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject runCompletePanel;
        [SerializeField] private UpgradeSelectionController upgradeSelection;
        [SerializeField] private GameAudio gameAudio;
        [SerializeField] private GameFlowState state;
        [SerializeField] private int currentBossIndex;
        [SerializeField, Min(0f)] private float bossDeathResultDelay = 0.7f;
        [SerializeField, Min(0f)] private float playerDeathResultDelay = 0.5f;
        private bool retryRequested;
        private Health observedBoss;
        private System.Action bossDied;
        private PendingReveal pendingReveal;
        private float revealAt;
        public event System.Action StateChanged;

        private enum PendingReveal { None, Upgrade, RunComplete, GameOver }

        public GameFlowState State => state;
        public int CurrentBossIndex => currentBossIndex;
        public Health CurrentBossHealth => bosses != null && currentBossIndex < bosses.Length ? bosses[currentBossIndex].health : null;

        private void Awake()
        {
            if (playerHealth == null || bosses == null || bosses.Length != 3 || playerMovement == null ||
                playerCombat == null || playerDodge == null || bossDefeatedPanel == null || gameOverPanel == null ||
                runCompletePanel == null || upgradeSelection == null)
            {
                Debug.LogError("GameFlowController: Three bosses, player references, result panels and Upgrade Selection are required.", this);
                enabled = false;
                return;
            }
            for (int i = 0; i < bosses.Length; i++)
            {
                if (bosses[i] == null || bosses[i].root == null || bosses[i].health == null ||
                    bosses[i].health.gameObject != bosses[i].root)
                {
                    Debug.LogError("GameFlowController: Each encounter needs its root and root Health.", this);
                    enabled = false;
                    return;
                }
                for (int j = 0; j < i; j++)
                {
                    if (bosses[i].root != bosses[j].root) continue;
                    Debug.LogError("GameFlowController: Boss encounters must be distinct.", this);
                    enabled = false;
                    return;
                }
            }
            currentBossIndex = 0;
            state = GameFlowState.Combat;
            bossDefeatedPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            runCompletePanel.SetActive(false);
            for (int i = 0; i < bosses.Length; i++) bosses[i].root.SetActive(i == currentBossIndex);
        }

        private void OnEnable()
        {
            if (playerHealth != null) playerHealth.Died += OnPlayerDied;
            if (upgradeSelection != null) upgradeSelection.SelectionCompleted += OnSelectionCompleted;
            SubscribeCurrentBoss();
        }

        private void OnDisable()
        {
            if (playerHealth != null) playerHealth.Died -= OnPlayerDied;
            if (upgradeSelection != null) upgradeSelection.SelectionCompleted -= OnSelectionCompleted;
            UnsubscribeBoss();
            pendingReveal = PendingReveal.None;
        }

        private void Update()
        {
            if (pendingReveal == PendingReveal.None || Time.unscaledTime < revealAt) return;
            RevealPending();
        }

        private void SubscribeCurrentBoss()
        {
            observedBoss = CurrentBossHealth;
            if (observedBoss == null) return;
            Health source = observedBoss;
            bossDied = () => OnBossDied(source);
            observedBoss.Died += bossDied;
        }

        private void UnsubscribeBoss()
        {
            if (observedBoss != null) observedBoss.Died -= bossDied;
            observedBoss = null;
            bossDied = null;
        }

        private void OnBossDied(Health source)
        {
            if (state != GameFlowState.Combat || pendingReveal != PendingReveal.None || source != CurrentBossHealth) return;
            gameAudio?.PlayBossDeath();
            SetPlayerControls(false);
            pendingReveal = currentBossIndex == bosses.Length - 1 ? PendingReveal.RunComplete : PendingReveal.Upgrade;
            revealAt = Time.unscaledTime + Mathf.Max(0f, bossDeathResultDelay);
        }

        private void OnSelectionCompleted()
        {
            if (state != GameFlowState.BossDefeated || currentBossIndex >= bosses.Length - 1) return;
            UnsubscribeBoss();
            bosses[currentBossIndex].root.SetActive(false);
            currentBossIndex++;
            bosses[currentBossIndex].root.SetActive(true);
            // Subscribe after activation so the boss's own Death cleanup runs first.
            SubscribeCurrentBoss();
            bossDefeatedPanel.SetActive(false);
            SetPlayerControls(true);
            state = GameFlowState.Combat;
            StateChanged?.Invoke();
        }

        private void SetPlayerControls(bool active)
        {
            playerCombat.enabled = active;
            playerDodge.enabled = active;
            playerMovement.enabled = active;
        }

        private void OnPlayerDied()
        {
            if (state != GameFlowState.Combat || pendingReveal != PendingReveal.None) return;
            SetPlayerControls(false);
            pendingReveal = PendingReveal.GameOver;
            revealAt = Time.unscaledTime + Mathf.Max(0f, playerDeathResultDelay);
        }

        private void RevealPending()
        {
            PendingReveal reveal = pendingReveal;
            pendingReveal = PendingReveal.None;
            if (reveal == PendingReveal.RunComplete)
            {
                state = GameFlowState.RunComplete;
                bossDefeatedPanel.SetActive(false);
                gameOverPanel.SetActive(false);
                runCompletePanel.SetActive(true);
                gameAudio?.PlayRunComplete();
            }
            else if (reveal == PendingReveal.Upgrade)
            {
                state = GameFlowState.BossDefeated;
                bossDefeatedPanel.SetActive(true);
                upgradeSelection.Show();
            }
            else if (reveal == PendingReveal.GameOver)
            {
                state = GameFlowState.GameOver;
                bosses[currentBossIndex].root.SetActive(false);
                gameOverPanel.SetActive(true);
                gameAudio?.PlayGameOver();
            }
            StateChanged?.Invoke();
        }

        public void Retry()
        {
            if (!isActiveAndEnabled || (state != GameFlowState.GameOver && state != GameFlowState.RunComplete) || retryRequested) return;
            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex < 0)
            {
                Debug.LogError("GameFlowController: Add the active scene to Build Settings to enable Retry.", this);
                return;
            }
            retryRequested = true;
            gameAudio?.PlayUiClick();
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
