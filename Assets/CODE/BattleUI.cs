using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class BattleUI : MonoBehaviour
{
    [Header("Main Action Buttons")]
    public Button attackButton;
    public Button defendButton;
    public Button healButton;
    public Button skipButton;

    [Header("Attack Panel Buttons")]
    public Button basicAttackButton;
    public Button specialAttackButton;
    public Button returnButton;

    [Header("UI References")]
    public TextMeshProUGUI actionText;
    public GameObject actionPanel;
    public GameObject attackPanel;
    public GameObject indicator;

    [Header("Battle References")]
    public BattleHandler battleHandler;
    private BattleUnit currentUnit;

    [Header("Enemy Targets")]
    public Transform[] enemyTargets;
    private int currentTargetIndex = -1;

    void Start()
    {
        // Main buttons
        attackButton.onClick.AddListener(OnAttackClicked);
        defendButton.onClick.AddListener(OnDefendClicked);
        healButton.onClick.AddListener(OnHealClicked);

        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipClicked);

        // Attack panel
        basicAttackButton.onClick.AddListener(OnBasicAttack);
        specialAttackButton.onClick.AddListener(OnSpecialAttack);
        returnButton.onClick.AddListener(OnReturn);

        actionPanel.SetActive(true);
        attackPanel.SetActive(false);

        if (indicator != null)
        {
            indicator.SetActive(true);

            SpriteRenderer sr = indicator.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sortingOrder = 999;
        }
    }

    // =========================
    // COMMAND PHASE
    // =========================

    public void ShowActionPanel(BattleUnit unit)
    {
        currentUnit = unit;
        currentTargetIndex = -1;

        actionPanel.SetActive(true);
        attackPanel.SetActive(false);

        SetMainButtonsVisible(true);

        actionText.text = "Choose an action for " + unit.unitName;
        MoveIndicatorTo(unit.transform);
    }

    public void HideAllPanelsForResolution()
    {
        actionPanel.SetActive(true); // keep text visible

        SetMainButtonsVisible(false);
        attackPanel.SetActive(false);

        if (indicator != null)
            indicator.SetActive(false);
    }

    public void ShowBattleMessage(string message)
    {
        if (actionText != null)
            actionText.text = message;
    }

    // =========================
    // BUTTON ACTIONS
    // =========================

    void OnAttackClicked()
    {
        if (currentUnit == null) return;

        SetMainButtonsVisible(false);
        attackPanel.SetActive(true);

        currentTargetIndex = GetFirstAliveEnemyIndex();

        if (currentTargetIndex == -1)
        {
            actionText.text = "No valid enemy targets.";
            return;
        }

        SelectEnemyTarget(currentTargetIndex);
    }

    void OnDefendClicked()
    {
        if (currentUnit == null) return;

        PlayerAction action = new PlayerAction(currentUnit, PlayerAction.ActionType.Defend);
        battleHandler.ReceivePlayerAction(action);
    }

    void OnHealClicked()
    {
        if (currentUnit == null) return;

        PlayerAction action = new PlayerAction(currentUnit, PlayerAction.ActionType.UsePotion, currentUnit);
        battleHandler.ReceivePlayerAction(action);
    }

    void OnSkipClicked()
    {
        if (currentUnit == null) return;

        PlayerAction action = new PlayerAction(currentUnit, PlayerAction.ActionType.Skip);
        battleHandler.ReceivePlayerAction(action);
    }

    void OnBasicAttack()
    {
        if (currentUnit == null) return;
        if (!IsValidTargetIndex(currentTargetIndex)) return;

        BattleUnit targetUnit = enemyTargets[currentTargetIndex].GetComponent<BattleUnit>();
        if (targetUnit == null || targetUnit.isDead) return;

        PlayerAction action = new PlayerAction(currentUnit, PlayerAction.ActionType.Attack, targetUnit);
        battleHandler.ReceivePlayerAction(action);
    }

    void OnSpecialAttack()
    {
        if (currentUnit == null) return;
        if (!IsValidTargetIndex(currentTargetIndex)) return;

        BattleUnit targetUnit = enemyTargets[currentTargetIndex].GetComponent<BattleUnit>();
        if (targetUnit == null || targetUnit.isDead) return;

        PlayerAction action = new PlayerAction(currentUnit, PlayerAction.ActionType.Magic, targetUnit);
        battleHandler.ReceivePlayerAction(action);
    }

    void OnReturn()
    {
        attackPanel.SetActive(false);
        SetMainButtonsVisible(true);

        if (currentUnit != null)
        {
            currentTargetIndex = -1;
            actionText.text = "Choose an action for " + currentUnit.unitName;
            MoveIndicatorTo(currentUnit.transform);
        }
    }

    // =========================
    // TARGET SELECTION
    // =========================

    public void SelectEnemyTarget(int index)
    {
        if (!IsValidTargetIndex(index)) return;

        BattleUnit targetUnit = enemyTargets[index].GetComponent<BattleUnit>();
        if (targetUnit == null || targetUnit.isDead) return;

        currentTargetIndex = index;
        MoveIndicatorTo(enemyTargets[index]);

        actionText.text = "Selected target: " + targetUnit.unitName;
    }

    void Update()
    {
        if (!attackPanel.activeSelf) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
            CycleTarget(-1);

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
            CycleTarget(1);
    }

    void CycleTarget(int direction)
    {
        if (enemyTargets == null || enemyTargets.Length == 0) return;

        int index = currentTargetIndex < 0 ? GetFirstAliveEnemyIndex() : currentTargetIndex;

        for (int i = 0; i < enemyTargets.Length; i++)
        {
            index = (index + direction + enemyTargets.Length) % enemyTargets.Length;

            BattleUnit unit = enemyTargets[index].GetComponent<BattleUnit>();
            if (unit != null && !unit.isDead)
            {
                SelectEnemyTarget(index);
                return;
            }
        }
    }

    // =========================
    // INDICATOR
    // =========================

            void MoveIndicatorTo(Transform target)
    {
        if (indicator == null || target == null) return;

        // Look for a child transform with a specific indicator anchor name
        Transform anchor = target.Find("mageindicator");
        if (anchor == null) anchor = target.Find("knightindicator");
        if (anchor == null) anchor = target.Find("robotindicator");
        if (anchor == null) anchor = target.Find("dragonindicator");

        Vector3 newPos;

        if (anchor != null)
        {
            // Place indicator exactly at the anchor point
            newPos = anchor.position;
        }
        else
        {
            // Fallback: place above the unit’s transform
            newPos = target.position + Vector3.up * 1.5f;
        }

        indicator.transform.position = newPos;
        indicator.SetActive(true);
    }

    // =========================
    // HELPERS
    // =========================

    void SetMainButtonsVisible(bool visible)
    {
        attackButton.gameObject.SetActive(visible);
        defendButton.gameObject.SetActive(visible);
        healButton.gameObject.SetActive(visible);
        if (skipButton != null)
            skipButton.gameObject.SetActive(visible);
    }

    bool IsValidTargetIndex(int index)
    {
        return enemyTargets != null &&
               index >= 0 &&
               index < enemyTargets.Length &&
               enemyTargets[index] != null;
    }

    int GetFirstAliveEnemyIndex()
    {
        for (int i = 0; i < enemyTargets.Length; i++)
        {
            if (enemyTargets[i] == null) continue;

            BattleUnit unit = enemyTargets[i].GetComponent<BattleUnit>();
            if (unit != null && !unit.isDead)
                return i;
        }

        return -1;
    }
}