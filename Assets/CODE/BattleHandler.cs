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

    [Header("RNG Settings")]
    [Range(0f, 1f)] public float critChance = 0.10f;

    public enum BattlePhase { CommandSelection, Resolution, Victory, Defeat }
    public BattlePhase CurrentPhase { get; private set; }

    private List<PlayerAction> playerQueue = new List<PlayerAction>();
    private List<PlayerAction> enemyQueue  = new List<PlayerAction>();
    private int currentCommandIndex = 0;

    void Start()
    {
        StartCommandPhase();
    }

    void StartCommandPhase()
    {
        CurrentPhase = BattlePhase.CommandSelection;
        playerQueue.Clear();
        enemyQueue.Clear();
        currentCommandIndex = 0;

        foreach (var u in playerUnits) u.ResetTurnFlags();
        foreach (var u in enemyUnits)  u.ResetTurnFlags();

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
            GenerateEnemyCommands();
            StartCoroutine(ResolvePhase());
            return;
        }

        BattleUnit current = playerUnits[currentCommandIndex];
        Debug.Log($"Choose an action for {current.unitName}.");
    }

    public void ReceivePlayerAction(PlayerAction action)
    {
        playerQueue.Add(action);
        currentCommandIndex++;
        PromptNextPlayerCommand();
    }

    void GenerateEnemyCommands()
    {
        List<BattleUnit> aliveEnemies = enemyUnits.FindAll(e => !e.isDead);
        List<BattleUnit> alivePlayers = playerUnits.FindAll(p => !p.isDead);

        foreach (var enemy in aliveEnemies)
        {
            PlayerAction action = enemyAI.DecideAction(enemy, alivePlayers);
            if (action != null) enemyQueue.Add(action);
        }
    }

    IEnumerator ResolvePhase()
    {
        CurrentPhase = BattlePhase.Resolution;
        Debug.Log("Actions resolve!");

        yield return new WaitForSeconds(0.5f);

        foreach (var action in playerQueue)
        {
            if (action.actor.isDead) continue;

            yield return StartCoroutine(ExecuteAction(action));
            yield return new WaitForSeconds(0.8f);

            if (CheckBattleEnd()) yield break;
        }

        yield return new WaitForSeconds(0.3f);

        foreach (var action in enemyQueue)
        {
            if (action.actor.isDead) continue;

            if (action.target != null && action.target.isDead)
                action.target = GetRandomAlivePlayer();

            if (action.target == null &&
                action.actionType != PlayerAction.ActionType.Defend &&
                action.actionType != PlayerAction.ActionType.Charge)
            {
                continue;
            }

            yield return StartCoroutine(ExecuteAction(action));
            yield return new WaitForSeconds(0.8f);

            if (CheckBattleEnd()) yield break;
        }

        StartCommandPhase();
    }

    IEnumerator ExecuteAction(PlayerAction action)
    {
        BattleUnit actor  = action.actor;
        BattleUnit target = action.target;
        bool isCrit = Random.value < critChance;

        switch (action.actionType)
        {
            case PlayerAction.ActionType.Attack:
            {
                int dmg = actor.CalculatePhysicalDamage(isCrit);
                target.TakeDamage(dmg, isMagic: false);
                string critStr = isCrit ? " CRITICAL HIT!" : "";
                Debug.Log($"{actor.unitName} attacks {target.unitName} for {dmg} damage!{critStr}");
                if (target.isDead)
                    Debug.Log($"{target.unitName} has been defeated!");
                break;
            }

            case PlayerAction.ActionType.Magic:
            {
                int dmg = actor.CalculateMagicDamage(isCrit);
                target.TakeDamage(dmg, isMagic: true);
                string critStr = isCrit ? " CRITICAL HIT!" : "";
                Debug.Log($"{actor.unitName} casts magic on {target.unitName} for {dmg} damage!{critStr}");
                if (target.isDead)
                    Debug.Log($"{target.unitName} has been defeated!");
                break;
            }

            case PlayerAction.ActionType.Defend:
                actor.StartDefend();
                Debug.Log($"{actor.unitName} takes a defensive stance!");
                break;

            case PlayerAction.ActionType.UsePotion:
            {
                int hpBefore = actor.currentHP;
                bool success = actor.UsePotion();
                if (success)
                {
                    int healed = actor.currentHP - hpBefore;
                    Debug.Log($"{actor.unitName} used a Potion and recovered {healed} HP! ({actor.potionCount} left)");
                }
                else
                {
                    Debug.Log($"{actor.unitName} has no Potions left!");
                }
                break;
            }

            case PlayerAction.ActionType.Charge:
                actor.StartCharge();
                Debug.Log($"{actor.unitName} is charging up power for next attack!");
                break;
        }

        yield return null;
    }

    bool CheckBattleEnd()
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

    BattleUnit GetRandomAlivePlayer()
    {
        List<BattleUnit> alive = playerUnits.FindAll(p => !p.isDead);
        if (alive.Count == 0) return null;
        return alive[Random.Range(0, alive.Count)];
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