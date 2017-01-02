using UnityEngine;
using System.Collections;

public class UnitInit : MonoBehaviour
{
    public Team UnitTeam;
    public Class UnitClass;

    private UnitManager manager;
    private CommandUnit CommanderScript;

	void Start ()
    {
        manager = GetComponent<UnitManager>();

        GameObject[] managers = GameObject.FindGameObjectsWithTag("PlayerManager");
        foreach (GameObject go in managers)
            if (go.GetComponent<CommandUnit>().TeamCommanded == UnitTeam)
                CommanderScript = go.GetComponent<CommandUnit>();

        InitByClass();
	}

    void InitByClass()
    {
        manager.UnitConfig.UnitClass = UnitClass;

        switch (UnitClass)
        {
            case Class.Rifleman:
                RiflemanVariables classPrefab = new RiflemanVariables();
                manager.UnitConfig.healthMax = classPrefab.healthMax;
                manager.UnitConfig.staminaMax = classPrefab.staminaMax;
                manager.SetClassIcon(classPrefab.classSprite);
                manager.UnitAbilities = classPrefab.ClassAbilities;
                break;
            default:
                break;
        }

        manager.SetHealth(manager.UnitConfig.healthMax);
        manager.SetStamina(manager.UnitConfig.staminaMax);
        manager.SetCalcStamina(manager.GetStamina());

        for (int i = 0; i < manager.UnitAbilities.Count; i++)
            manager.UnitAbilities[i].AbilityUser = manager;

        InitByTeam();
    }

    void InitByTeam()
    {
        manager.UnitConfig.UnitTeam = UnitTeam;

        switch (UnitTeam)
        {
            case Team.Green:
                manager.ActiveDisplay.GetComponent<SpriteRenderer>().color = VariableLibrary.GreenTeamColor;
                break;
            case Team.Red:
                manager.ActiveDisplay.GetComponent<SpriteRenderer>().color = VariableLibrary.RedTeamColor;
                break;
            default:
                break;
        }

        //Wpisujemy postać do listy jednostek swojego dowódcy
        if (CommanderScript != null)
        {
            CommanderScript.CommandedUnits.Add(manager.gameObject);
            manager.SetCommander(CommanderScript);
        }

        Destroy(this);
    }
}
