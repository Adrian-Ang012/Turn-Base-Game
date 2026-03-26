using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHandler : MonoBehaviour
{
    public List<BattleUnit> playerUnits;
    public List<BattleUnit> enemyUnits;

    public EnemyAI enemyAI;
    public BattleQueue battleQueue;
    public BattleRNG battleRNG;
    public BattleUI battleUI;

    public enum BattlePhase { CommandSelection, Resolution, Victory, Defeat }
    public BattlePhase CurrentPhase { get; private set; }

    private int currentCommandIndex = 0;

    void Start()
    {
        StartCommandPhase();
    }

    void StartCommandPhase()
    {
        if (CheckBattleEnd()) return;

        CurrentPhase = BattlePhase.CommandSelection;
        currentCommandIndex = 0;

        battleQueue.ClearQueues();

        foreach (var u in playerUnits)
            if (u != null && !u.isDead)
                u.ResetTurnFlags();

        foreach (var u in enemyUnits)
            if (u != null && !u.isDead)
                u.ResetTurnFlags();

        PromptNextPlayerCommand();
    }

    void PromptNextPlayerCommand()
    {
        while (currentCommandIndex < playerUnits.Count &&
               playerUnits[currentCommandIndex].isDead)
        {
            currentCommandIndex++;
        }

        if (currentCommandIndex >= playerUnits.Count)
        {
            battleUI.HideAllPanelsForResolution();
            BuildEnemyQueue();
            StartCoroutine(ResolvePhase());
            return;
        }

        BattleUnit current = playerUnits[currentCommandIndex];
        battleUI.ShowActionPanel(current);
    }

    public void ReceivePlayerAction(PlayerAction action)
    {
        if (action != null)
            battleQueue.AddPlayerAction(action);

        currentCommandIndex++;
        PromptNextPlayerCommand();
    }

    void BuildEnemyQueue()
    {
        foreach (var enemy in enemyUnits)
        {
            if (enemy == null || enemy.isDead) continue;

            PlayerAction action = enemyAI.DecideAction(enemy, GetAlivePlayers());
            battleQueue.AddEnemyAction(action);
        }
    }

    IEnumerator ResolvePhase()
    {
        yield return ExecuteQueue(battleQueue.GetPlayerQueue(), true);
        if (CheckBattleEnd()) yield break;

        yield return ExecuteQueue(battleQueue.GetEnemyQueue(), false);
        if (CheckBattleEnd()) yield break;

        yield return new WaitForSeconds(1f);
        StartCommandPhase();
    }

    IEnumerator ExecuteQueue(List<PlayerAction> queue, bool actorIsPlayer)
    {
        foreach (var action in queue)
        {
            if (action == null || action.actor == null || action.actor.isDead)
                continue;

            BattleUnit target = action.target;

            if ((action.actionType == PlayerAction.ActionType.Attack ||
                 action.actionType == PlayerAction.ActionType.Magic) &&
                (target == null || target.isDead))
            {
                target = actorIsPlayer ? GetRandomAliveEnemy() : GetRandomAlivePlayer();
            }

            ExecuteAction(action.actor, action.actionType, target);

            if (CheckBattleEnd())
                yield break;

            yield return new WaitForSeconds(1f);
        }
    }

    void ExecuteAction(BattleUnit actor, PlayerAction.ActionType actionType, BattleUnit target)
    {
        string msg = "";

        switch (actionType)
        {
            case PlayerAction.ActionType.Attack:
                int dmg = battleRNG.ApplyVariance(actor.CalculatePhysicalDamage());
                if (battleRNG.RollCrit()) dmg = battleRNG.ApplyCrit(dmg);
                target.TakeDamage(dmg, false);
                msg = $"{actor.unitName} attacked {target.unitName} for {dmg} damage";
                break;

            case PlayerAction.ActionType.Magic:
                int mdmg = battleRNG.ApplyVariance(actor.CalculateMagicDamage());
                if (battleRNG.RollCrit()) mdmg = battleRNG.ApplyCrit(mdmg);
                target.TakeDamage(mdmg, true);
                msg = $"{actor.unitName} used Magic on {target.unitName} for {mdmg} damage";
                break;

            case PlayerAction.ActionType.Defend:
                actor.StartDefend();
                msg = $"{actor.unitName} is defending";
                break;

            case PlayerAction.ActionType.UsePotion:
                actor.UsePotion();
                msg = $"{actor.unitName} used a potion";
                break;

            case PlayerAction.ActionType.Skip:
                msg = $"{actor.unitName} skipped the turn";
                break;
        }

        battleUI.ShowBattleMessage(msg);

        actor.RefreshUI();
        if (target != null) target.RefreshUI();
    }

    BattleUnit GetRandomAliveEnemy()
    {
        var alive = GetAliveEnemies();
        return alive.Count == 0 ? null : alive[Random.Range(0, alive.Count)];
    }

    BattleUnit GetRandomAlivePlayer()
    {
        var alive = GetAlivePlayers();
        return alive.Count == 0 ? null : alive[Random.Range(0, alive.Count)];
    }

    public bool CheckBattleEnd()
    {
        if (playerUnits.TrueForAll(p => p.isDead))
        {
            battleUI.ShowBattleMessage("Defeat!");
            return true;
        }

        if (enemyUnits.TrueForAll(e => e.isDead))
        {
            battleUI.ShowBattleMessage("Victory!");
            return true;
        }

        return false;
    }

    public List<BattleUnit> GetAliveEnemies() => enemyUnits.FindAll(e => !e.isDead);
    public List<BattleUnit> GetAlivePlayers() => playerUnits.FindAll(p => !p.isDead);
}