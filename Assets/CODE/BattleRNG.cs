using UnityEngine;

public class BattleRNG : MonoBehaviour
{
    [Header("Crit Settings")]
    [Range(0, 100)] public int critChance = 15;
    public float critMultiplier = 1.5f;

    [Header("Damage Variance")]
    public float minVariance = 0.85f;
    public float maxVariance = 1.15f;

    public bool RollCrit()
    {
        int roll = Random.Range(1, 101);
        return roll <= critChance;
    }

    public int ApplyCrit(int damage)
    {
        return Mathf.RoundToInt(damage * critMultiplier);
    }

    public int ApplyVariance(int damage)
    {
        float multiplier = Random.Range(minVariance, maxVariance);
        return Mathf.RoundToInt(damage * multiplier);
    }
}