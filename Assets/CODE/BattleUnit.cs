using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUnit : MonoBehaviour
{
    [Header("Unit Info")]
    public string unitName = "Unit";
    public bool isPlayerUnit = true;

    [Header("Base Stats")]
    public int maxHP = 100;
    public int currentHP;
    public int physicalAttack = 20;
    public int magicAttack = 30;
    public int defense = 10;
    public int magicDefense = 10;

    [Header("Items")]
    public int potionCount = 2;
    public int potionHealAmount = 40;

    [Header("Charge / Unique Mechanic")]
    public bool isCharging = false;
    public float chargeMultiplier = 2f;

    [Header("UI References")]
    public Slider hpSlider;          // Drag the Slider for this unit’s HP bar
    public TMP_Text hpText;          // Drag the TMP text showing HP numbers
    public TMP_Text unitNameText;    // Drag the TMP text showing the unit’s name

    [HideInInspector] public bool isDefending = false;
    [HideInInspector] public bool isDead = false;

    void Awake()
    {
        currentHP = maxHP;
    }

    void Start()
    {
        RefreshUI();
    }

    public void TakeDamage(int rawDamage, bool isMagic = false)
    {
        int def = isMagic ? magicDefense : defense;

        if (isDefending) def = Mathf.RoundToInt(def * 2f);

        int finalDamage = Mathf.Max(1, rawDamage - def);
        currentHP = Mathf.Max(0, currentHP - finalDamage);

        if (currentHP <= 0)
        {
            isDead = true;
            OnDeath();
        }

        RefreshUI();
    }

    public int CalculatePhysicalDamage()
    {
        int baseDmg = physicalAttack;

        if (isCharging)
        {
            baseDmg = Mathf.RoundToInt(baseDmg * chargeMultiplier);
            isCharging = false;
        }

        return baseDmg;
    }

    public int CalculateMagicDamage()
    {
        int baseDmg = magicAttack;

        if (isCharging)
        {
            baseDmg = Mathf.RoundToInt(baseDmg * chargeMultiplier);
            isCharging = false;
        }

        return baseDmg;
    }

    public bool UsePotion()
    {
        if (potionCount <= 0) return false;

        potionCount--;
        int healed = Mathf.Min(potionHealAmount, maxHP - currentHP);
        currentHP += healed;
        RefreshUI();
        return true;
    }

    public void StartDefend()
    {
        isDefending = true;
    }

    public void StartCharge()
    {
        isCharging = true;
    }

    public void ResetTurnFlags()
    {
        isDefending = false;
    }

    public void RefreshUI()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        if (hpText != null)
            hpText.text = $"{currentHP} / {maxHP}";

        if (unitNameText != null)
            unitNameText.text = unitName;
    }

    void OnDeath()
    {
        Debug.Log($"{unitName} has been defeated!");
        // Optional: disable GameObject, play death animation, etc.
    }
}