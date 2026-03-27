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

    [Header("UI References")]
    public Slider hpSlider;
    public TMP_Text hpText;
    public TMP_Text unitNameText;

    [Header("VFX")]
    public GameObject deathVFX;

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

        if (isDefending)
            def = Mathf.RoundToInt(def * 2f);

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
        return physicalAttack;
    }

    public int CalculateMagicDamage()
    {
        return magicAttack;
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

        if (deathVFX != null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                GameObject vfx = Instantiate(deathVFX, canvas.transform);

                Animator anim = vfx.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.SetTrigger("OnDeath");
                }
                else
                {
                    Debug.LogWarning("Death VFX prefab has no Animator!");
                }
            }
        }
        else
        {
            Debug.LogWarning("Death VFX prefab not assigned!");
        }

        hpSlider.gameObject.SetActive(false);
        gameObject.SetActive(false);
    } 
}