using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    public sealed class UpgradeSelectionController : MonoBehaviour
    {
        private const int ChoiceCount = 3;
        [SerializeField] private PlayerUpgradeState playerUpgrades;
        [SerializeField] private GameObject selectionPanel;
        [SerializeField, FormerlySerializedAs("choices")] private UpgradeDefinition[] pool;
        [SerializeField] private Button[] buttons;
        [SerializeField] private TMP_Text[] names;
        [SerializeField] private TMP_Text[] descriptions;
        [SerializeField] private GameAudio gameAudio;
        private readonly HashSet<UpgradeDefinition> selectedUpgrades = new HashSet<UpgradeDefinition>();
        private readonly UpgradeDefinition[] choices = new UpgradeDefinition[ChoiceCount];
        private int[] rollIndices;
        public bool IsOpen { get; private set; }
        public event Action SelectionCompleted;

        private void Awake()
        {
            if (playerUpgrades == null || selectionPanel == null || pool == null || buttons == null ||
                names == null || descriptions == null || pool.Length < ChoiceCount || buttons.Length != ChoiceCount ||
                names.Length != ChoiceCount || descriptions.Length != ChoiceCount)
            {
                Debug.LogError("UpgradeSelectionController: Player, panel, at least three pool entries and exactly three complete cards are required.", this);
                enabled = false;
                return;
            }
            for (int i = 0; i < ChoiceCount; i++)
            {
                if (buttons[i] != null && names[i] != null && descriptions[i] != null) continue;
                Debug.LogError("UpgradeSelectionController: A choice reference is missing.", this);
                enabled = false;
                return;
            }
            for (int i = 0; i < pool.Length; i++)
            {
                if (pool[i] == null)
                {
                    Debug.LogError("UpgradeSelectionController: Pool entry is missing.", this);
                    enabled = false;
                    return;
                }
                for (int j = 0; j < i; j++)
                {
                    if (pool[i] != pool[j]) continue;
                    Debug.LogError("UpgradeSelectionController: Pool must contain distinct definitions.", this);
                    enabled = false;
                    return;
                }
            }
            rollIndices = new int[pool.Length];
            selectionPanel.SetActive(false);
        }

        public void Show()
        {
            if (!isActiveAndEnabled || IsOpen) return;
            int availableCount = 0;
            for (int i = 0; i < pool.Length; i++)
                if (!selectedUpgrades.Contains(pool[i])) rollIndices[availableCount++] = i;
            if (availableCount < ChoiceCount)
            {
                Debug.LogError("UpgradeSelectionController: Not enough unselected upgrades for three choices.", this);
                return;
            }
            IsOpen = true;
            for (int i = 0; i < ChoiceCount; i++)
            {
                int selected = UnityEngine.Random.Range(i, availableCount);
                int swap = rollIndices[i];
                rollIndices[i] = rollIndices[selected];
                rollIndices[selected] = swap;
                choices[i] = pool[rollIndices[i]];
                names[i].text = choices[i].DisplayName;
                descriptions[i].text = choices[i].Description;
                buttons[i].interactable = true;
            }
            selectionPanel.SetActive(true);
        }

        public void Select(int index)
        {
            if (!isActiveAndEnabled || !IsOpen || !selectionPanel.activeInHierarchy || index < 0 || index >= ChoiceCount) return;
            IsOpen = false;
            if (!playerUpgrades.TryApply(choices[index]))
            {
                IsOpen = true;
                return;
            }
            selectedUpgrades.Add(choices[index]);
            gameAudio?.PlayUiConfirm();
            foreach (Button button in buttons) button.interactable = false;
            selectionPanel.SetActive(false);
            SelectionCompleted?.Invoke();
        }

        private void OnDisable()
        {
            IsOpen = false;
            if (selectionPanel != null) selectionPanel.SetActive(false);
        }
    }
}
