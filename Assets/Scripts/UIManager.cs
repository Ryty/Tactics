using UnityEngine;
using System.Collections;
using UnityEngine.UI;

 public class UIManager : MonoBehaviour
{
    public Text StateText;
    public Button[] UnitButtons;
    public Image[] PanelImages;
    public Text[] PanelTexts;
    public Text CoverText;
    public float BarFlashSpeed = 1;

    private Canvas CurrentCanvas;
    private UnitManager ActiveUnit;
    
	// Use this for initialization
	void Start ()
    {
        CurrentCanvas = GetComponentInChildren<Canvas>();
        if (CurrentCanvas == null)
            Debug.LogError("No Canvas found by UIManager!");

        SelectNameplateColor();
	}

    void Awake()
    {
        StartCoroutine(AssignButtons());
    }

    IEnumerator AssignButtons()
    {
        for(int i = 0; i < 3; i++)
            yield return new WaitForEndOfFrame();

        
        switch (GetComponent<CommandUnit>().CommandedUnits.Count)
        {
            case 1:
                SetButtonCount(1);
                break;
            case 2:
                SetButtonCount(2);
                break;
            case 3:
                SetButtonCount(3);
                break;
            case 4:
                SetButtonCount(4);
                break;
            case 5:
                SetButtonCount(5);
                break;
            default:
                break;
        }

        PrepareButtons();
    }

    void Update()
    {
        ActiveUnit = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>().ActiveUnit;

        if (ActiveUnit != null)
        {
            int[] vals = new int[4];
            ActiveUnit.CoverValues.TryGetValue(Orientation.North, out vals[0]);
            ActiveUnit.CoverValues.TryGetValue(Orientation.South, out vals[1]);
            ActiveUnit.CoverValues.TryGetValue(Orientation.West, out vals[2]);
            ActiveUnit.CoverValues.TryGetValue(Orientation.East, out vals[3]);
            CoverText.text = "N: " + vals[0] + " S: " + vals[1] + " W: " + vals[2] + " E: " + vals[3];

            FillPanelVariables();

            //Miganie pasków
            PanelImages[3].color = new Color(255, 255, 255, Mathf.Abs(Mathf.Sin(Time.time * BarFlashSpeed)));        
        }
    }

    void SetButtonCount(int num)
    {
        for(int i = UnitButtons.Length-1; i >= num; i--)
            if(UnitButtons[i] != null)
            {
                Destroy(UnitButtons[i].gameObject);
                UnitButtons[i] = null;
            }
    }

    void PrepareButtons()
    {
        for (int i = 0; i < GetComponent<CommandUnit>().CommandedUnits.Count; i++)
        {
            UnitButtons[i].GetComponent<UnitButton>().CorrespondingUnit = GetComponent<CommandUnit>().CommandedUnits[i].GetComponent<UnitManager>(); //Przypisujemy jednostkę do przycisku
            UnitButtons[i].onClick.AddListener(UnitButtons[i].GetComponent<UnitButton>().CorrespondingUnit.HandleButtoned); //Przypisujemy funkcję do przycisku
            UnitButtons[i].GetComponentInChildren<Text>().text = UnitButtons[i].GetComponent<UnitButton>().CorrespondingUnit.name; //Nazywamy przycisk odpowiednim imieniem postaci
        }
    }

    void SelectNameplateColor()
    {
        Team playerTeam = GetComponent<CommandUnit>().TeamCommanded;

        switch(playerTeam)
        {
            case Team.Green:
                PanelImages[0].color = VariableLibrary.GreenTeamColor;
                break;
            case Team.Red:
                PanelImages[0].color = VariableLibrary.RedTeamColor;
                break;
            default:
                PanelImages[0].color = Color.white;
                break;
        }
    }

    void FillPanelVariables()
    {
        //Wartość pasków życia i energii
        PanelImages [1].fillAmount = (float)ActiveUnit.GetHealth() / (float)ActiveUnit.UnitConfig.healthMax;
        PanelImages[2].fillAmount = (float)ActiveUnit.GetStamina() / (float)ActiveUnit.UnitConfig.staminaMax;
        PanelImages[3].fillAmount = (float)(1 - (float)ActiveUnit.GetCalculatedStamina() / (float)ActiveUnit.UnitConfig.staminaMax);
           
        //Nazwa postaci i liczba hp i staminy
        PanelTexts[0].text = ActiveUnit.UnitConfig.name + " " + ActiveUnit.UnitConfig.surname;
        PanelTexts[1].text = ActiveUnit.GetHealth().ToString() + " / " + ActiveUnit.UnitConfig.healthMax.ToString();
        if(ActiveUnit.GetCalculatedStamina() != ActiveUnit.GetStamina()) //Jeśli mamy do wyświetlenia przewidywaną staminę
            PanelTexts[2].text = "[" + ActiveUnit.GetCalculatedStamina() + "] " + ActiveUnit.GetStamina().ToString() + " / " + ActiveUnit.UnitConfig.staminaMax.ToString();
        else
            PanelTexts[2].text = ActiveUnit.GetStamina().ToString() + " / " + ActiveUnit.UnitConfig.staminaMax.ToString();
    }
}
