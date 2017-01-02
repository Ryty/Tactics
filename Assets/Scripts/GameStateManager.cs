using UnityEngine;
using System.Collections;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        RedTurn,
        GreenTurn,
        Activity
    };

    public GameState CurrentGameState;
    public UnitManager ActiveUnit;

    private GameObject[] PlayerManagers;
    private GameObject GreenManager;
   // private GameObject RedManager;


	// Use this for initialization
	void Start ()
    {
        CurrentGameState = GameState.GreenTurn;

        PlayerManagers = GameObject.FindGameObjectsWithTag("PlayerManager");
        GreenManager = FindManager(Team.Green);
   //     RedManager = FindManager(Team.Red);
	}
	
	// Update is called once per frame
	void Update ()
    {
	    switch(CurrentGameState)
        {
            case GameState.GreenTurn:
                break;
            case GameState.RedTurn:
                break;
            case GameState.Activity:
                HandleActivityState();
                break;
            default:
                Debug.LogError("CRITICAL ERROR: GameStateManager entered unknown state!");
                break;                
        }
	}
    //Zezwala zewnętrznym skryptom ustawiać state
    public void ChangeStateByTeam(Team team)
    {
        switch (team)
        {
            case Team.Green:
                CurrentGameState = GameState.GreenTurn;
                HandleGreenTurn();
                break;
            case Team.Red:
                CurrentGameState = GameState.RedTurn;
                break;
            default:
                break;
        }              
    }
    //Znajduje managera konkretnej drużyny
    GameObject FindManager(Team team)
    {
        for (int i = 0; i < PlayerManagers.Length; i++)
            if (PlayerManagers[i].GetComponent<CommandUnit>().TeamCommanded == team)
                return PlayerManagers[i];

        return null;
    }

    void HandleGreenTurn()
    {
        //Włączamy mu możliwość rozkazywania
        GreenManager.GetComponent<CommandUnit>().enabled = true;
        //Dajemy mu wolną kamerę
      //  BlueManager.GetComponent<CameraMovement>().StartFreeMovement();

        //Wyłączamy możliwość rozkazywania wszystkim innym
        foreach (GameObject go in PlayerManagers)
            if (go != GreenManager)
                go.GetComponent<CommandUnit>().enabled = false;
    }

    void HandleActivityState()
    {
        foreach (GameObject go in PlayerManagers)
        {
            //TO DO.....Sprawdź, czy widać tę postać
            if (true)//Jeśli tak, to followuj te postać
            {
                Debug.Log(">");
                go.GetComponent<CameraMovement>().StartFollowing(ActiveUnit.gameObject);
                go.GetComponent<CommandUnit>().enabled = false;
            }
        }
    }
}
