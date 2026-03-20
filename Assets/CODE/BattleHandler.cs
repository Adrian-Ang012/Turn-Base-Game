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

    public enum BattlePhase { CommandSelection, Resolution, Victory, Defeat }
    public BattlePhase CurrentPhase { get; private set; }

    private int currentCommandIndex = 0;

    void Start()
    {
        StartCommandPhase();
    }

    void StartCommandPhase()
    {
        CurrentPhase = BattlePhase.CommandSelection;
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
            StartCoroutine(ResolvePhase());
            return;
        }

        BattleUnit current = playerUnits[currentCommandIndex];
        Debug.Log($"Choose an action for {current.unitName}.");
    }

    public void ReceivePlayerAction(PlayerAction action)
    {
        currentCommandIndex++;
        PromptNextPlayerCommand();
    }

    IEnumerator ResolvePhase()
    {
        CurrentPhase = BattlePhase.Resolution;
        Debug.Log("Actions resolve!");

        yield return new WaitForSeconds(0.5f);

        StartCommandPhase();
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