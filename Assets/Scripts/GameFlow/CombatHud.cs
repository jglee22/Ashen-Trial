using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    public sealed class CombatHud : MonoBehaviour
    {
        [SerializeField] private Health playerHealth;
        [SerializeField] private GameFlowController gameFlow;
        [SerializeField] private GameObject playerPanel;
        [SerializeField] private GameObject bossPanel;
        [SerializeField] private Image playerFill;
        [SerializeField] private Image bossFill;
        [SerializeField] private TMP_Text playerHpText;
        [SerializeField] private TMP_Text bossNameText;
        [SerializeField] private TMP_Text bossHpText;
        private Health observedBoss;

        private void Awake()
        {
            if (playerHealth != null && gameFlow != null && playerPanel != null && bossPanel != null &&
                playerFill != null && bossFill != null && playerHpText != null && bossNameText != null && bossHpText != null) return;
            Debug.LogError("CombatHud: Health, GameFlow, panels, bars and TMP references are required.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (playerHealth == null || gameFlow == null) return;
            playerHealth.Changed += RefreshPlayer;
            gameFlow.StateChanged += RefreshFlow;
            RefreshFlow();
        }

        private void RefreshFlow()
        {
            if (observedBoss != null) observedBoss.Changed -= RefreshBoss;
            observedBoss = gameFlow.CurrentBossHealth;
            if (observedBoss != null) observedBoss.Changed += RefreshBoss;
            bool combat = gameFlow.State == GameFlowController.GameFlowState.Combat;
            playerPanel.SetActive(combat || gameFlow.State == GameFlowController.GameFlowState.BossDefeated);
            bossPanel.SetActive(combat);
            bossNameText.text = gameFlow.CurrentBossIndex == 0 ? "BOSS I" :
                gameFlow.CurrentBossIndex == 1 ? "BOSS II" : "FINAL BOSS";
            RefreshPlayer();
            RefreshBoss();
        }

        private void RefreshPlayer()
        {
            playerFill.fillAmount = playerHealth.CurrentHp / playerHealth.MaxHp;
            playerHpText.SetText("{0:0} / {1:0}", playerHealth.CurrentHp, playerHealth.MaxHp);
        }

        private void RefreshBoss()
        {
            bossFill.fillAmount = observedBoss != null ? observedBoss.CurrentHp / observedBoss.MaxHp : 0f;
            bossHpText.SetText("{0:0} / {1:0}", observedBoss != null ? observedBoss.CurrentHp : 0f,
                observedBoss != null ? observedBoss.MaxHp : 0f);
        }

        private void OnDisable()
        {
            if (playerHealth != null) playerHealth.Changed -= RefreshPlayer;
            if (gameFlow != null) gameFlow.StateChanged -= RefreshFlow;
            if (observedBoss != null) observedBoss.Changed -= RefreshBoss;
            observedBoss = null;
        }
    }
}
