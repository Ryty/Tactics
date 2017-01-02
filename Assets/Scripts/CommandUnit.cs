using UnityEngine;
using System.Collections.Generic;


public enum Team
{
    Red,
    Green
};

public class CommandUnit : MonoBehaviour
{
    public Team TeamCommanded;
    public LayerMask CommandRayLayerMask;
    public List<GameObject> CommandedUnits = new List<GameObject>();
    public PlayerStateManager StateManager;

    private UnitManager ActiveUnit;
   
	// Use this for initialization
	void Start ()
    {
        ActiveUnit = null;
        StateManager = GetComponent<PlayerStateManager>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (StateManager.CurrentState == PlayerState.Free)
        {
            if (Input.GetMouseButtonDown(1)) //Jeśli klikniemy prawy przycisk myszy
                DeselectUnit();

            if (ActiveUnit == null) // Jeśli nie mamy nikogo zaznaczonego - musimy kogoś zaznaczyć----------------------------------------------------------------------------------
            {
                HandleNoUnitCommanding();
            }
            else //inaczej (jeśli mamy już jakąś jednostkę zaznaczoną)--------------------------------------------------------------------------------------------------------------
            {
                HandleUnitCommanding();
            }
        }
        else if(StateManager.CurrentState == PlayerState.Targeting)
        {
            HandleTargeting();
        }
	}

    void HandleNoUnitCommanding()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, float.PositiveInfinity, CommandRayLayerMask))
        {
            if (hit.collider.gameObject.layer == 10) //Jeśli trafiliśmy w jakąś postać
            {
                if (hit.collider.gameObject.GetComponent<UnitManager>() && hit.collider.gameObject.GetComponent<UnitManager>().UnitConfig.UnitTeam == TeamCommanded) //Jeśli jest ona w naszej drużynie
                {
                    if (Input.GetMouseButtonDown(0)) //Jeśli kliknęliśmy lewy przycisk myszy
                        HandleChangingUnit(hit.collider.gameObject.GetComponent<UnitManager>()); //Zaznaczamy tę postać jako aktywną
                }
            }
            else if (hit.collider.gameObject.layer == 8) //Inaczej, jeśli trafiliśmy w grida
            {
                if (hit.collider.gameObject.GetComponent<Tile>() && hit.collider.gameObject.GetComponent<Tile>().GetOccupier() != null) //Jeśli to jest tile i jeśli ma okupanta
                    if (hit.collider.gameObject.GetComponent<Tile>().GetOccupier().UnitConfig.UnitTeam == TeamCommanded) //Jeśli ten okupant jest w naszej drużynie
                    {
                        if (Input.GetMouseButtonDown(0)) //Jeśli kliknęliśmy lewy przycisk myszy
                            HandleChangingUnit(hit.collider.gameObject.GetComponent<Tile>().GetOccupier()); //Zaznaczamy tę postać jako aktywną
                    }
            }
        }
    }

    void HandleUnitCommanding()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, float.PositiveInfinity, CommandRayLayerMask))
        {
            if (hit.collider.gameObject.layer == 10) //Jeśli trafiliśmy w jakąś postać----------------------------
            {
                ActiveUnit.SetCalculatedStamina(ActiveUnit.GetStamina());
                CutLine();
                if (hit.collider.gameObject.GetComponent<UnitManager>() && hit.collider.gameObject.GetComponent<UnitManager>().UnitConfig.UnitTeam == TeamCommanded && hit.collider.gameObject.GetComponent<UnitManager>() != ActiveUnit) //Jeśli jest ona w naszej drużynie i nie jest to już aktywna jednostka
                {
                    if (Input.GetMouseButtonDown(0)) //Jeśli kliknęliśmy lewy przycisk myszy
                        HandleChangingUnit(hit.collider.gameObject.GetComponent<UnitManager>());
                }
                else if (hit.collider.gameObject.GetComponent<UnitManager>() && hit.collider.gameObject.GetComponent<UnitManager>().UnitConfig.UnitTeam == TeamCommanded && hit.collider.gameObject.GetComponent<UnitManager>() == ActiveUnit) //Inaczej, jeśli jest on w naszej drużynie i jest to aktywna jednostka
                {
                    //........................
                }
                else //Inaczej (jeśli nie jest on w naszej drużynie)
                {
                    if (Input.GetMouseButtonDown(0)) //Jeśli kliknęliśmy lewy przycisk myszy
                    {
                        Debug.Log("Effective cover: " + ActiveUnit.FindEffectiveCover(hit.collider.gameObject.GetComponent<UnitManager>(), ActiveUnit).ToString());
                        Debug.Log("Visibility: " + ActiveUnit.CheckSight(ActiveUnit, hit.collider.gameObject.GetComponent<UnitManager>()).ToString());
                        ActiveUnit.UseAbility(0, hit.collider.gameObject.GetComponent<UnitManager>());
                    }
                }
            }
            else if (hit.collider.gameObject.layer == 8) //Inaczej, jeśli trafiliśmy w grida-------------------------
            {
                if (hit.collider.gameObject.GetComponent<Tile>() && hit.collider.gameObject.GetComponent<Tile>().GetOccupier() != null) //Jeśli to jest tile i jeśli ma okupanta
                {
                    ActiveUnit.SetCalculatedStamina(ActiveUnit.GetStamina());
                    CutLine();
                    if (hit.collider.gameObject.GetComponent<Tile>().GetOccupier().UnitConfig.UnitTeam == TeamCommanded && hit.collider.gameObject.GetComponent<Tile>().GetOccupier() != ActiveUnit) //Jeśli ten okupant jest w naszej drużynie i nie jest to już aktywna jednostka
                    {
                        if (Input.GetMouseButtonDown(0)) //Jeśli kliknęliśmy lewy przycisk myszy
                            HandleChangingUnit(hit.collider.gameObject.GetComponent<Tile>().GetOccupier()); //Zaznaczamy tę postać jako aktywną
                    }
                    else if (hit.collider.gameObject.GetComponent<Tile>().GetOccupier().UnitConfig.UnitTeam == TeamCommanded && hit.collider.gameObject.GetComponent<Tile>().GetOccupier() == ActiveUnit) //Inaczej, jeśli jest on w naszej drużynie i jest aktywny
                    {
                        //.................................
                    }
                    else //Inaczej (jeśli nie jest on w naszej drużynie)
                    {
                        if (Input.GetMouseButtonDown(0)) //Jeśli kliknęliśmy lewy przycisk myszy
                        {
                            Debug.Log("Effective cover: " + ActiveUnit.FindEffectiveCover(hit.collider.gameObject.GetComponent<Tile>().GetOccupier(), ActiveUnit));
                            Debug.Log("Visibility: " + ActiveUnit.CheckSight(ActiveUnit, hit.collider.gameObject.GetComponent<Tile>().GetOccupier()).ToString());
                            ActiveUnit.UseAbility(0, hit.collider.gameObject.GetComponent<UnitManager>());
                        }
                    }
                }
                else //Inaczej (jeśli jest to tile, ale nie ma okupanta)
                {
                    ActiveUnit.PlotPath(hit.collider.gameObject.transform.position); //Wyświetlamy drogę
                    if (Input.GetMouseButtonDown(0)) //Jeśli klikniemy lewy przycisk myszy
                        ActiveUnit.NavigateTo(hit.collider.gameObject.GetComponent<Tile>()); //Idziemy do pozycji
                }
            }
            else //Inaczej, jeśli nie trafiliśmy ani w postać ani w grida
            {
                ActiveUnit.SetCalculatedStamina(ActiveUnit.GetStamina());
                CutLine();
            }
        }
        else //Inaczej, jeśli w nic nie trafiliśmy
        {
            ActiveUnit.SetCalculatedStamina(ActiveUnit.GetStamina());
            CutLine();
        }
    }

    void HandleTargeting()
    {
        if (Input.GetMouseButton(1)) //Prawy przycisk anuluje celowanie
            StateManager.CurrentAbility.OnCancel();
        if (Input.GetButton("Submit")) //Enter wykonuje umiejętność
            StateManager.CurrentAbility.OnAcceptance();

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, float.PositiveInfinity, CommandRayLayerMask))
        {
            if (hit.collider.gameObject.layer == 10) //Jeśli trafiliśmy w jakąś postać----------------------------
            {
                if (hit.collider.gameObject.GetComponent<UnitManager>()) //Jeśli na pewno ma unitmanagera
                {
                    if (hit.collider.gameObject.GetComponent<UnitManager>().UnitConfig.UnitTeam != ActiveUnit.UnitConfig.UnitTeam) //Jeśli jest w innej drużynie niż nasza
                    {
                        if (hit.collider.gameObject.GetComponent<UnitManager>() == StateManager.CurrentAbility.Target) //Jeśli najedziemy na aktualny cel
                        {
                            if (Input.GetMouseButtonDown(0)) //Jeśli klikniemy w cel
                            {
                                StateManager.CurrentAbility.OnAcceptance();  //To używamy na niego skilla
                            }
                        }
                        else //Jeśli klikniemy w innego przeciwnika
                        {
                            if (Input.GetMouseButtonDown(0))
                            {
                                for (int i = 0; i < ActiveUnit.UnitAbilities.Count; i++) //Znajdujemy indeks aktualnej umiejętności w liście postaci
                                    if (ActiveUnit.UnitAbilities[i] == StateManager.CurrentAbility)
                                    {
                                        ActiveUnit.UseAbility(i, hit.collider.gameObject.GetComponent<UnitManager>()); //ponownie wzywamy tę umiejętność
                                    }
                            }
                        }
                    }
                }
            }
            else if (hit.collider.gameObject.layer == 8) //Inaczej, jeśli trafiliśmy w grida-------------------------
            {
                if(hit.collider.gameObject.GetComponent<Tile>() && hit.collider.gameObject.GetComponent<Tile>().GetOccupier()) //Jeśli na pewno jest tilem i ma kogoś na sobie
                {
                    if(hit.collider.gameObject.GetComponent<Tile>().GetOccupier().UnitConfig.UnitTeam != ActiveUnit.UnitConfig.UnitTeam) //Jeśli jest w innej drużynie niż nasza
                    {
                        if(hit.collider.gameObject.GetComponent<Tile>().GetOccupier() == StateManager.CurrentAbility.Target) //Jeśli jest celem naszej umiejętności
                        {
                            if (Input.GetMouseButtonDown(0)) //Jeśli kliknęliśmy lewy przycisk myszy)
                                StateManager.CurrentAbility.OnAcceptance(); //Uzywamy tę umiejętność
                        }
                        else //Inaczej (jesli to inny przeciwnik)
                        {
                            if (Input.GetMouseButtonDown(0)) //Jeśli kliknęliśmy lewy przycisk myszy)
                            {
                                for (int i = 0; i < ActiveUnit.UnitAbilities.Count; i++) //Znajdujemy indeks aktualnej umiejętności w liście postaci
                                    if (ActiveUnit.UnitAbilities[i] == StateManager.CurrentAbility)
                                    {
                                        ActiveUnit.UseAbility(i, hit.collider.gameObject.GetComponent<Tile>().GetOccupier()); //ponownie wzywamy tę umiejętność
                                    }
                            }
                        }
                    }
                }
            }
        }


    }

    public void HandleChangingUnit(UnitManager newUnit)
    {
        DeselectUnit();

        ActiveUnit = newUnit;

        if (ActiveUnit != null)
        {
            ActiveUnit.HandleChangingState(true);
            Debug.Log(ActiveUnit.name + " is now active!");
        }
    }

    void DeselectUnit()
    {
        if(ActiveUnit != null)
        {
            ActiveUnit.HandleChangingState(false);
            ActiveUnit = null;
        }
    }
    //Znika pathmakera gdy pokazujemy gdzieś, gdzie nie możemy się dostać
    void CutLine()
    {
        if (ActiveUnit != null)
            ActiveUnit.CharacterMovement.GetLine().SetVertexCount(1);
    }
}
