/// <summary>
/// Represents a single queued action chosen by the player
/// during the Command Selection phase.
/// </summary>
[System.Serializable]
public class PlayerAction
{
    public enum ActionType
    {
        Attack,
        Magic,
        Defend,
        UsePotion,
        Skip
    }

    public BattleUnit actor;        
    public ActionType actionType;
    public BattleUnit target;        

    public PlayerAction(BattleUnit actor, ActionType type, BattleUnit target = null)
    {
        this.actor      = actor;
        this.actionType = type;
        this.target     = target;
    }
}