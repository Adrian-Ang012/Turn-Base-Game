using System.Collections.Generic;
using UnityEngine;

public class BattleQueue : MonoBehaviour
{
    private List<PlayerAction> playerQueue = new List<PlayerAction>();
    private List<PlayerAction> enemyQueue = new List<PlayerAction>();

    public void ClearQueues()
    {
        playerQueue.Clear();
        enemyQueue.Clear();
    }

    public void AddPlayerAction(PlayerAction action)
    {
        if (action != null)
            playerQueue.Add(action);
    }

    public void AddEnemyAction(PlayerAction action)
    {
        if (action != null)
            enemyQueue.Add(action);
    }

    public List<PlayerAction> GetPlayerQueue()
    {
        return playerQueue;
    }

    public List<PlayerAction> GetEnemyQueue()
    {
        return enemyQueue;
    }
}