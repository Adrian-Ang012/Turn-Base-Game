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

    public Transform[] enemyTargets; // Drag Enemy_Robot and Enemy_Dragon here
    private int currentTargetIndex = 0;

    void Start()
    {
        // Hook up buttons
        attackButton.onClick.AddListener(OnAttackClicked);
        defendButton.onClick.AddListener(OnDefendClicked);
        healButton.onClick.AddListener(OnHealClicked);

        basicAttackButton.onClick.AddListener(OnBasicAttack);
        specialAttackButton.onClick.AddListener(OnSpecialAttack);
        returnButton.onClick.AddListener(OnReturn);

        // Hide panels at start
        actionPanel.SetActive(true);
        attackPanel.SetActive(false);
        indicator.SetActive(false);
    }

    // Called by BattleHandler when it’s time for a player unit to act
    public void ShowActionPanel(BattleUnit unit)
    {
        currentUnit = unit;
        actionPanel.SetActive(true);
        attackPanel.SetActive(false);
        actionText.text = $"Choose an action for {unit.unitName}";
        indicator.SetActive(true);
        MoveIndicatorTo(unit.transform);
    }

    void OnAttackClicked()
    {
        actionPanel.SetActive(false);
        attackPanel.SetActive(true);
        actionText.text = "Choose your attack!";
        currentTargetIndex = 0;
        MoveIndicatorTo(enemyTargets[currentTargetIndex]);
    }

    void OnDefendClicked()
    {
        PlayerAction action = new PlayerAction(currentUnit, PlayerAction.ActionType.Defend, null);
        battleHandler.ReceivePlayerAction(action);
        HidePanels();
        actionText.text = $"{currentUnit.unitName} defends!";
    }

    void OnHealClicked()
    {
        PlayerAction action = new PlayerAction(currentUnit, PlayerAction.ActionType.UsePotion, currentUnit);
        battleHandler.ReceivePlayerAction(action);
        HidePanels();
        actionText.text = $"{currentUnit.unitName} heals!";
    }

    void OnBasicAttack()
    {
        BattleUnit targetUnit = enemyTargets[currentTargetIndex].GetComponent<BattleUnit>();
        PlayerAction action = new PlayerAction(currentUnit, PlayerAction.ActionType.Attack, targetUnit);
        battleHandler.ReceivePlayerAction(action);
        HidePanels();
        actionText.text = $"{currentUnit.unitName} attacks {targetUnit.unitName}!";
    }

    void OnSpecialAttack()
    {
        BattleUnit targetUnit = enemyTargets[currentTargetIndex].GetComponent<BattleUnit>();
        PlayerAction action = new PlayerAction(currentUnit, PlayerAction.ActionType.Magic, targetUnit);
        battleHandler.ReceivePlayerAction(action);
        HidePanels();
        actionText.text = $"{currentUnit.unitName} casts magic on {targetUnit.unitName}!";
    }

    void OnReturn()
    {
        attackPanel.SetActive(false);
        actionPanel.SetActive(true);
        MoveIndicatorTo(currentUnit.transform);
    }

    void Update()
    {
        if (attackPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentTargetIndex = 0;
                MoveIndicatorTo(enemyTargets[0]);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentTargetIndex = 1;
                MoveIndicatorTo(enemyTargets[1]);
            }
        }
    }

    void MoveIndicatorTo(Transform target)
    {
        indicator.transform.position = target.position + Vector3.up * 2f;
    }

    void HidePanels()
    {
        actionPanel.SetActive(false);
        attackPanel.SetActive(false);
        indicator.SetActive(false);
    }
}