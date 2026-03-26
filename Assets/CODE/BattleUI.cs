using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUI : MonoBehaviour
{
    [Header("Main Action Buttons")]
    public Button attackButton;
    public Button defendButton;
    public Button healButton;

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
        attackButton.onClick.AddListener(OnAttackClicked);
        defendButton.onClick.AddListener(OnDefendClicked);
        healButton.onClick.AddListener(OnHealClicked);

        basicAttackButton.onClick.AddListener(OnBasicAttack);
        specialAttackButton.onClick.AddListener(OnSpecialAttack);
        returnButton.onClick.AddListener(OnReturn);

        actionPanel.SetActive(true);
        attackPanel.SetActive(false);

        if (indicator != null)
        {
            indicator.SetActive(false);

            SpriteRenderer sr = indicator.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sortingOrder = 999;
        }
    }

    public void ShowActionPanel(BattleUnit unit)
    {
        currentUnit = unit;
        currentTargetIndex = -1;

        actionPanel.SetActive(true);
        attackPanel.SetActive(false);

        actionText.text = "Choose an action for " + unit.unitName;
        MoveIndicatorTo(unit.transform);
    }

    public void HideAllPanelsForResolution()
    {
        actionPanel.SetActive(false);
        attackPanel.SetActive(false);

        if (indicator != null)
            indicator.SetActive(false);
    }

    void OnAttackClicked()
    {
        if (currentUnit == null) return;

        actionPanel.SetActive(false);
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

        PlayerAction action = new PlayerAction(currentUnit, PlayerAction.ActionType.Defend, null);
        battleHandler.ReceivePlayerAction(action);
        actionText.text = currentUnit.unitName + " defends!";
    }

    void OnHealClicked()
    {
        if (currentUnit == null) return;

        PlayerAction action = new PlayerAction(currentUnit, PlayerAction.ActionType.UsePotion, currentUnit);
        battleHandler.ReceivePlayerAction(action);
        actionText.text = currentUnit.unitName + " heals!";
    }

    void OnBasicAttack()
    {
        if (currentUnit == null) return;
        if (!IsValidTargetIndex(currentTargetIndex)) return;

        BattleUnit targetUnit = enemyTargets[currentTargetIndex].GetComponent<BattleUnit>();
        if (targetUnit == null || targetUnit.isDead) return;

        PlayerAction action = new PlayerAction(currentUnit, PlayerAction.ActionType.Attack, targetUnit);
        battleHandler.ReceivePlayerAction(action);
        actionText.text = currentUnit.unitName + " attacks " + targetUnit.unitName + "!";
    }

    void OnSpecialAttack()
    {
        if (currentUnit == null) return;
        if (!IsValidTargetIndex(currentTargetIndex)) return;

        BattleUnit targetUnit = enemyTargets[currentTargetIndex].GetComponent<BattleUnit>();
        if (targetUnit == null || targetUnit.isDead) return;

        PlayerAction action = new PlayerAction(currentUnit, PlayerAction.ActionType.Magic, targetUnit);
        battleHandler.ReceivePlayerAction(action);
        actionText.text = currentUnit.unitName + " casts magic on " + targetUnit.unitName + "!";
    }

    void OnReturn()
    {
        attackPanel.SetActive(false);
        actionPanel.SetActive(true);

        if (currentUnit != null)
        {
            currentTargetIndex = -1;
            actionText.text = "Choose an action for " + currentUnit.unitName;
            MoveIndicatorTo(currentUnit.transform);
        }
    }

    public void SelectEnemyTarget(int index)
    {
        if (!IsValidTargetIndex(index)) return;

        BattleUnit targetUnit = enemyTargets[index].GetComponent<BattleUnit>();
        if (targetUnit == null || targetUnit.isDead) return;

        currentTargetIndex = index;
        MoveIndicatorTo(enemyTargets[currentTargetIndex]);
        actionText.text = "Selected target: " + targetUnit.unitName + "  (Left/Right to switch)";
        Debug.Log("Target selected: " + targetUnit.unitName);
    }

    void Update()
    {
        if (!attackPanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            CycleTarget(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            CycleTarget(1);
        }
    }

    void CycleTarget(int direction)
    {
        if (enemyTargets == null || enemyTargets.Length == 0) return;

        int startIndex = currentTargetIndex;
        if (startIndex < 0)
            startIndex = GetFirstAliveEnemyIndex();

        if (startIndex == -1) return;

        int index = startIndex;

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

    void MoveIndicatorTo(Transform target)
    {
        if (indicator == null || target == null) return;

        SpriteRenderer targetSR = target.GetComponent<SpriteRenderer>();
        SpriteRenderer indicatorSR = indicator.GetComponent<SpriteRenderer>();

        if (indicatorSR != null)
            indicatorSR.sortingOrder = 999;

        if (targetSR != null)
        {
            float topY = targetSR.bounds.max.y;
            indicator.transform.position = new Vector3(
                targetSR.bounds.center.x,
                topY + 0.25f,
                target.position.z
            );
        }
        else
        {
            indicator.transform.position = target.position + Vector3.up * 1.2f;
        }

        indicator.SetActive(true);
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
        if (enemyTargets == null) return -1;

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