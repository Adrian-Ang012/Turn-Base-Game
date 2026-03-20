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
    public Slider hpSlider;
    public TMP_Text hpText;
    public TMP_Text unitNameText;

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

    public int CalculatePhysicalDamage(bool isCritical = false)
    {
        float roll = Random.Range(0.85f, 1.15f);
        int baseDmg = Mathf.RoundToInt(physicalAttack * roll);

        if (isCharging)
        {
            baseDmg = Mathf.RoundToInt(baseDmg * chargeMultiplier);
            isCharging = false;
        }

        if (isCritical) baseDmg = Mathf.RoundToInt(baseDmg * 1.5f);
        return baseDmg;
    }

    public int CalculateMagicDamage(bool isCritical = false)
    {
        float roll = Random.Range(0.85f, 1.15f);
        int baseDmg = Mathf.RoundToInt(magicAttack * roll);

        if (isCharging)
        {
            baseDmg = Mathf.RoundToInt(baseDmg * chargeMultiplier);
            isCharging = false;
        }

        if (isCritical) baseDmg = Mathf.RoundToInt(baseDmg * 1.5f);
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
    }
}