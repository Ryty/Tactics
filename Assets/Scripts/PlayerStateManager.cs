using UnityEngine;
using System.Collections;


public enum PlayerState
{
    Free,
    Targeting,
    Action
};

[RequireComponent(typeof(CommandUnit))]
public class PlayerStateManager : MonoBehaviour
{
    public PlayerState CurrentState;
    public Ability CurrentAbility = null;

    void Start()
    {
        CurrentState = PlayerState.Free;
    }

    public void SetState(PlayerState newState, Ability usedAbility = default(Ability)) //Nowy state, jeśli to targeting to dodatkowo wrzucić ability
    {
        switch (newState)
        {
            case PlayerState.Free:
                HandleFreeState();
                break;
            case PlayerState.Targeting:
                if (usedAbility != null)
                {
                    CurrentAbility = usedAbility;
                    HandleTargetingState();
                    break;
                }
                else
                    break;
            case PlayerState.Action:
                HandleActionState();
                break;
            default:
                break;
        }
    }

    void HandleFreeState()
    {
        CurrentState = PlayerState.Free;
        //Obracamy jednostki w odpowiednie strony
        UnitManager[] allUnits = FindObjectsOfType<UnitManager>();
        foreach (UnitManager unit in allUnits)
            unit.AssumeCoveredPosition();

        GetComponent<UIManager>().StateText.text = "Free";
        CurrentAbility = null;        
    }

    void HandleTargetingState()
    {
        GetComponent<UIManager>().StateText.text = "Targeting";
        CurrentState = PlayerState.Targeting;
    }

    void HandleActionState()
    {

    }
}
