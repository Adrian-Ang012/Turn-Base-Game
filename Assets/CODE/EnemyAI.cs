using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("AI Weights (0–100)")]
    [Range(0, 100)] public int attackWeight = 60;
    [Range(0, 100)] public int defendWeight = 40;

    public PlayerAction DecideAction(BattleUnit enemy, List<BattleUnit> playerUnits)
    {
        if (playerUnits == null || playerUnits.Count == 0) return null;

        PlayerAction.ActionType chosenType = PickActionType(enemy);

        // Defend has no target
        if (chosenType == PlayerAction.ActionType.Defend)
        {
            return new PlayerAction(enemy, chosenType);
        }

        // Attack / Magic → pick target
        BattleUnit target = PickRandomTarget(playerUnits);
        return new PlayerAction(enemy, chosenType, target);
    }

    PlayerAction.ActionType PickActionType(BattleUnit enemy)
    {
        int total = attackWeight + defendWeight;
        if (total <= 0) return PlayerAction.ActionType.Attack;

        int roll = Random.Range(0, total);

        if (roll < attackWeight)
            return PlayerAction.ActionType.Attack;

        return PlayerAction.ActionType.Defend;
    }

    BattleUnit PickRandomTarget(List<BattleUnit> players)
    {
        List<BattleUnit> alive = players.FindAll(p => !p.isDead);
        if (alive.Count == 0) return null;

        return alive[Random.Range(0, alive.Count)];
    }
}