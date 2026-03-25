using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHandler : MonoBehaviour
{
    [Header("Party Members")]
    public List<BattleUnit> playerUnits;
    public List<BattleUnit> enemyUnits;

    [Header("Dependencies")]
    public EnemyAI enemyAI;
    public BattleQueue battleQueue;
    public BattleRNG battleRNG;

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
        {
            if (u != null && !u.isDead)
                u.ResetTurnFlags();
        }

        foreach (var u in enemyUnits)
        {
            if (u != null && !u.isDead)
                u.ResetTurnFlags();
        }

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
            BuildEnemyQueue();
            StartCoroutine(ResolvePhase());
            return;
        }

        BattleUnit current = playerUnits[currentCommandIndex];
        Debug.Log("Choose an action for " + current.unitName + ".");
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
        CurrentPhase = BattlePhase.Resolution;
        Debug.Log("Actions resolve!");

        yield return ExecuteQueue(battleQueue.GetPlayerQueue(), true);

        if (CheckBattleEnd())
            yield break;

        yield return ExecuteQueue(battleQueue.GetEnemyQueue(), false);

        if (CheckBattleEnd())
            yield break;

        yield return new WaitForSeconds(0.5f);
        StartCommandPhase();
    }

    IEnumerator ExecuteQueue(List<PlayerAction> queue, bool actorIsPlayer)
    {
        foreach (var action in queue)
        {
            if (action == null || action.actor == null || action.actor.isDead)
                continue;

            BattleUnit finalTarget = action.target;

            if ((action.actionType == PlayerAction.ActionType.Attack ||
                 action.actionType == PlayerAction.ActionType.Magic) &&
                (finalTarget == null || finalTarget.isDead))
            {
                finalTarget = actorIsPlayer ? GetRandomAliveEnemy() : GetRandomAlivePlayer();
            }

            ExecuteAction(action.actor, action.actionType, finalTarget);

            if (CheckBattleEnd())
                yield break;

            yield return new WaitForSeconds(0.6f);
        }
    }

    void ExecuteAction(BattleUnit actor, PlayerAction.ActionType actionType, BattleUnit target)
    {
        switch (actionType)
        {
            case PlayerAction.ActionType.Attack:
                {
                    if (target == null) return;

                    int damage = actor.CalculatePhysicalDamage();
                    damage = battleRNG.ApplyVariance(damage);

                    if (battleRNG.RollCrit())
                        damage = battleRNG.ApplyCrit(damage);

                    target.TakeDamage(damage, false);
                    break;
                }

            case PlayerAction.ActionType.Magic:
                {
                    if (target == null) return;

                    int damage = actor.CalculateMagicDamage();
                    damage = battleRNG.ApplyVariance(damage);

                    if (battleRNG.RollCrit())
                        damage = battleRNG.ApplyCrit(damage);

                    target.TakeDamage(damage, true);
                    break;
                }

            case PlayerAction.ActionType.Defend:
                actor.StartDefend();
                break;

            case PlayerAction.ActionType.UsePotion:
                actor.UsePotion();
                break;

            case PlayerAction.ActionType.Charge:
                actor.StartCharge();
                break;
        }

        actor.RefreshUI();
        if (target != null)
            target.RefreshUI();
    }

    BattleUnit GetRandomAliveEnemy()
    {
        List<BattleUnit> alive = GetAliveEnemies();
        if (alive.Count == 0) return null;
        return alive[Random.Range(0, alive.Count)];
    }

    BattleUnit GetRandomAlivePlayer()
    {
        List<BattleUnit> alive = GetAlivePlayers();
        if (alive.Count == 0) return null;
        return alive[Random.Range(0, alive.Count)];
    }

    public bool CheckBattleEnd()
    {
        bool allPlayersDead = playerUnits.TrueForAll(p => p.isDead);
        bool allEnemiesDead = enemyUnits.TrueForAll(e => e.isDead);

        if (allPlayersDead)
        {
            CurrentPhase = BattlePhase.Defeat;
            Debug.Log("DEFEAT! Your party has been wiped out...");
            return true;
        }

        if (allEnemiesDead)
        {
            CurrentPhase = BattlePhase.Victory;
            Debug.Log("VICTORY! All enemies defeated!");
            return true;
        }

        return false;
    }

    public BattleUnit GetCurrentCommandUnit()
    {
        if (currentCommandIndex < playerUnits.Count)
            return playerUnits[currentCommandIndex];
        return null;
    }

    public List<BattleUnit> GetAliveEnemies() => enemyUnits.FindAll(e => !e.isDead);
    public List<BattleUnit> GetAlivePlayers() => playerUnits.FindAll(p => !p.isDead);
}