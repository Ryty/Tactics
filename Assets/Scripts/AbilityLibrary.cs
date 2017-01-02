using UnityEngine;
using System.Collections;

public class Ability
{
    public enum Targeting
    {
        Enemy,
        Self
    };

    public int StaminaCost;
    public string Description;
    public Targeting TargetingType;
    public UnitManager AbilityUser;
    public UnitManager Target = null;
    
    public virtual void OnActivation(UnitManager target)
    {
        if (Target && Target != target) //Jeśli mieliśmy jakiś inny cel
            Target.HandleBeingDetargeted(); //Niech ogarnie sobie że już nikt w niego nie celuje

        Target = target;
        target.HandleBeingTargeted(); //Podświetlmy nowy cel

        AbilityUser.GetCommander().GetComponent<CameraMovement>().StartSwiping(target.gameObject);
        AbilityUser.SetCalculatedStamina(AbilityUser.GetStamina() - StaminaCost);
        AbilityUser.AssumeCoveredPosition(target);
    }

    public virtual void OnCancel()
    {
        Target.HandleBeingDetargeted(); //Odświetlmy cel
        Target = null;

        AbilityUser.SetCalculatedStamina(AbilityUser.GetStamina());
        AbilityUser.GetCommander().StateManager.SetState(PlayerState.Free);
    }

    public virtual void OnAcceptance()
    {
        AbilityUser.GetCommander().StateManager.SetState(PlayerState.Free);
        AbilityUser.SetStamina(AbilityUser.GetCalculatedStamina());
    }
}

public class Shoot : Ability
{
    public Shoot(int cost)
    {
        StaminaCost = cost;
    }

    public override void OnActivation(UnitManager target)
    {
        base.OnActivation(target);
    }

    public override void OnAcceptance()
    {
        Target.SetHealth(Target.GetHealth() - 100);
        Debug.Log("Shot! Enemy hp: " + Target.GetHealth());

        base.OnAcceptance();
    }
}
